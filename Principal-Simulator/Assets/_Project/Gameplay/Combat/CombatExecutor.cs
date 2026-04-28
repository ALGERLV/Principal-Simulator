using System.Collections.Generic;
using TBS.Contracts.Combat;
using TBS.Contracts.Events;
using TBS.Core.Events;
using TBS.Unit;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Gameplay.Combat
{
    public sealed class CombatExecutor : ICombatExecutor
    {
        readonly IDamageCalculator _calculator;
        readonly ICombatRule[]     _rules;

        public CombatExecutor(IDamageCalculator calculator, ICombatRule[] rules)
        {
            _calculator = calculator;
            _rules      = rules;
        }

        // ─── 验证 ──────────────────────────────────────────────────────────────

        public bool CanAttack(IUnitToken attacker, IUnitToken defender, CombatType type, out string reason)
        {
            if (attacker.State == UnitState.Suppressed || attacker.State == UnitState.Routed)
            {
                reason = $"{attacker.DisplayName} 状态不允许攻击（{attacker.State}）";
                return false;
            }
            if (attacker.Supply < 1)
            {
                reason = $"{attacker.DisplayName} 断补，无法发起攻击";
                return false;
            }
            if (type == CombatType.Assault && attacker.Position.DistanceTo(defender.Position) > 1)
            {
                reason = "目标超出近战射程（>1格）";
                return false;
            }
            reason = null;
            return true;
        }

        // ─── 近战进攻 ─────────────────────────────────────────────────────────

        public CombatResult ExecuteAssault(IUnitToken attacker, IUnitToken defender, IGameContext context)
        {
            EventBus.Emit(new CombatStartedEvent
            {
                Attacker      = attacker,
                Defender      = defender,
                Type          = CombatType.Assault,
                AttackerCoord = attacker.Position,
                DefenderCoord = defender.Position,
            });

            var p = BuildParameters(attacker, defender, context, CombatType.Assault);
            ApplyRules(ref p, CombatType.Assault);

            var damage = _calculator.CalculateAssault(p);
            ApplyDamage(attacker, defender, damage, DamageSource.Attack, DamageSource.Counter);
            ApplyMoraleBreakBonus(attacker, defender, damage);

            var result = DetermineResult(damage);
            bool retreated = HandleOutcome(attacker, defender, damage, context);
            bool overrun   = CheckOverrun(attacker, defender, damage);

            EventBus.Emit(new CombatEndedEvent
            {
                Attacker         = attacker,
                Defender         = defender,
                Result           = result,
                DefenderRetreated = retreated,
                OverrunTriggered  = overrun,
            });

            return result;
        }

        // ─── 炮击 ─────────────────────────────────────────────────────────────

        public void ExecuteArtillery(IUnitToken source, HexCoord targetCoord, IGameContext context)
        {
            var target = context.GetUnitAt(targetCoord);
            if (target == null) return;

            EventBus.Emit(new CombatStartedEvent
            {
                Attacker      = source,
                Defender      = target,
                Type          = CombatType.Artillery,
                AttackerCoord = source.Position,
                DefenderCoord = targetCoord,
            });

            int rawDamage = _calculator.CalculateArtillery(source, target, context);

            target.Morale   = Mathf.Max(0, target.Morale - 15);
            int strLoss      = rawDamage >= 1 ? Mathf.FloorToInt(rawDamage / 3f) : 0;
            target.Strength = Mathf.Max(0, target.Strength - strLoss);

            if (rawDamage >= 1 && target.State != UnitState.Suppressed)
                EventBus.Emit(new SuppressionAppliedEvent { Target = target, DurationHours = 3f, Source = source });

            EventBus.Emit(new DamageDealtEvent
            {
                Source       = source,
                Target       = target,
                StrengthLoss = strLoss,
                MoraleLoss   = 15,
                DamageSource = DamageSource.Artillery,
            });

            CheckElimination(target, source);

            EventBus.Emit(new CombatEndedEvent
            {
                Attacker = source,
                Defender = target,
                Result   = CombatResult.AttackerWon,
            });
        }

        // ─── 协同攻击 ─────────────────────────────────────────────────────────

        public CombatResult ExecuteCombinedAssault(IReadOnlyList<IUnitToken> attackers, IUnitToken defender, IGameContext context)
        {
            // 以主攻单位（第一个）为代表走流程，规则链负责写入协同加成
            var primary = attackers[0];

            EventBus.Emit(new CombatStartedEvent
            {
                Attacker      = primary,
                Defender      = defender,
                Type          = CombatType.CombinedAssault,
                AttackerCoord = primary.Position,
                DefenderCoord = defender.Position,
            });

            var p = BuildParameters(primary, defender, context, CombatType.CombinedAssault);
            p.CombinedAttackBonus = CombinedBonus(attackers.Count);
            ApplyRules(ref p, CombatType.CombinedAssault);

            var damage = _calculator.CalculateAssault(p);
            ApplyDamage(primary, defender, damage, DamageSource.Attack, DamageSource.Counter);
            ApplyMoraleBreakBonus(primary, defender, damage);

            var result    = DetermineResult(damage);
            bool retreated = HandleOutcome(primary, defender, damage, context);
            bool overrun   = CheckOverrun(primary, defender, damage);

            EventBus.Emit(new CombatEndedEvent
            {
                Attacker         = primary,
                Defender         = defender,
                Result           = result,
                DefenderRetreated = retreated,
                OverrunTriggered  = overrun,
            });

            return result;
        }

        // ─── 私有辅助 ─────────────────────────────────────────────────────────

        CombatParameters BuildParameters(IUnitToken attacker, IUnitToken defender, IGameContext context, CombatType type)
        {
            return new CombatParameters
            {
                Attacker              = attacker,
                Defender              = defender,
                AttackerCoord         = attacker.Position,
                DefenderCoord         = defender.Position,
                EffectiveAttack       = attacker.AttackPower,
                EffectiveDefense      = defender.DefensePower,
                CombinedAttackBonus   = 1.0f,
                TerrainAttackModifier = 1.0f,
                TerrainDefenseBonus   = 0,
                WeatherModifier       = context.CurrentWeather == WeatherType.Rain
                                        && attacker.Faction != Faction.PLA ? 0.9f : 1.0f,
                SupplyAttackModifier  = 1.0f,
                SupplyDefenseModifier = 1.0f,
                FortificationLevel    = defender.FortificationLevel,
            };
        }

        void ApplyRules(ref CombatParameters p, CombatType type)
        {
            foreach (var rule in _rules)
                if (rule.Applies(p.Attacker, p.Defender, type))
                    rule.ModifyParameters(ref p);
        }

        void ApplyDamage(IUnitToken attacker, IUnitToken defender, CombatDamage damage,
                         DamageSource attackerSource, DamageSource defenderSource)
        {
            defender.Strength = Mathf.Max(0, defender.Strength - damage.DefenderStrengthLoss);
            defender.Morale   = Mathf.Max(0, defender.Morale   - damage.DefenderMoraleLoss);
            attacker.Strength = Mathf.Max(0, attacker.Strength - damage.AttackerStrengthLoss);
            attacker.Morale   = Mathf.Max(0, attacker.Morale   - damage.AttackerMoraleLoss);

            if (damage.DefenderStrengthLoss > 0)
                EventBus.Emit(new DamageDealtEvent { Source = attacker, Target = defender,
                    StrengthLoss = damage.DefenderStrengthLoss, MoraleLoss = damage.DefenderMoraleLoss,
                    DamageSource = attackerSource });

            if (damage.AttackerStrengthLoss > 0)
                EventBus.Emit(new DamageDealtEvent { Source = defender, Target = attacker,
                    StrengthLoss = damage.AttackerStrengthLoss, MoraleLoss = damage.AttackerMoraleLoss,
                    DamageSource = defenderSource });
        }

        static void ApplyMoraleBreakBonus(IUnitToken attacker, IUnitToken defender, CombatDamage damage)
        {
            // 阵地被突破额外士气惩罚（防方后撤时由 HandleOutcome 负责 -12）
        }

        bool HandleOutcome(IUnitToken attacker, IUnitToken defender, CombatDamage damage, IGameContext context)
        {
            bool retreated = false;

            // ① 防方歼灭
            if (defender.Strength <= 0)
            {
                EventBus.Emit(new UnitEliminatedEvent { Unit = defender, KilledBy = attacker, LastPosition = defender.Position });
                return false;
            }

            // ② 溃散检查（溃散条件参见 UnitStates）
            if (IsRouted(defender))
            {
                defender.Morale = Mathf.Max(0, defender.Morale - 12); // 阵地被突破
                HexCoord retreatTarget = FindRetreatTarget(defender, attacker, context);
                EventBus.Emit(new UnitRoutedEvent { Unit = defender, RetreatTarget = retreatTarget });
                retreated = true;
            }
            // ③ 动摇检查
            else if (IsShaken(defender))
            {
                EventBus.Emit(new UnitShakenEvent { Unit = defender, MoraleAtTrigger = defender.Morale });
            }

            // ④ 攻方歼灭
            if (attacker.Strength <= 0)
                EventBus.Emit(new UnitEliminatedEvent { Unit = attacker, KilledBy = defender, LastPosition = attacker.Position });

            return retreated;
        }

        bool CheckOverrun(IUnitToken attacker, IUnitToken defender, CombatDamage damage)
        {
            bool mechanized = attacker.Grade == UnitGrade.Japan; // 预留占位，待兵种扩展
            bool defenderDefeated = damage.DefenderStrengthLoss >= 2 || IsRouted(defender);

            if (mechanized && defenderDefeated && attacker.Strength >= 3 && attacker.Supply >= 2)
            {
                attacker.Supply--;
                EventBus.Emit(new OverrunTriggeredEvent { Attacker = attacker, FromCoord = attacker.Position });
                return true;
            }
            return false;
        }

        void CheckElimination(IUnitToken target, IUnitToken source)
        {
            if (target.Strength <= 0)
                EventBus.Emit(new UnitEliminatedEvent { Unit = target, KilledBy = source, LastPosition = target.Position });
        }

        static CombatResult DetermineResult(CombatDamage damage)
        {
            if (damage.DefenderStrengthLoss > damage.AttackerStrengthLoss) return CombatResult.AttackerWon;
            if (damage.AttackerStrengthLoss > damage.DefenderStrengthLoss) return CombatResult.DefenderWon;
            return CombatResult.Draw;
        }

        static bool IsRouted(IUnitToken unit) =>
            unit.Morale <= 20 || unit.Strength <= 1;

        static bool IsShaken(IUnitToken unit) =>
            unit.Morale <= 40 && unit.Morale > 20;

        static HexCoord FindRetreatTarget(IUnitToken retreating, IUnitToken attacker, IGameContext context)
        {
            // 优先选择远离攻方的相邻格
            var neighbors = retreating.Position.GetNeighbors();
            foreach (var n in neighbors)
            {
                if (n.DistanceTo(attacker.Position) > retreating.Position.DistanceTo(attacker.Position)
                    && !context.IsOccupied(n))
                    return n;
            }
            // 侧方
            foreach (var n in neighbors)
            {
                if (!context.IsOccupied(n))
                    return n;
            }
            // 无路可退
            retreating.Morale = Mathf.Max(0, retreating.Morale - 10);
            return retreating.Position;
        }

        static float CombinedBonus(int attackerCount)
        {
            float[] bonuses = { 1.0f, 1.15f, 1.30f, 1.50f, 1.65f, 1.80f };
            int idx = Mathf.Clamp(attackerCount - 1, 0, bonuses.Length - 1);
            return bonuses[idx];
        }
    }
}
