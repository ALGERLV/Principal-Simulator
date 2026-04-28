using TBS.Contracts.Combat;
using TBS.Contracts.Events;
using TBS.Map.API;
using TBS.Map.Tools;
using TBS.Unit;

namespace TBS.Gameplay.Combat
{
    public sealed class TerrainCombatRule : ICombatRule
    {
        public string RuleId => "terrain";

        readonly ITerrainQuery _terrain;

        public TerrainCombatRule(ITerrainQuery terrain) => _terrain = terrain;

        public bool Applies(IUnitToken attacker, IUnitToken defender, CombatType type) =>
            type == CombatType.Assault || type == CombatType.CombinedAssault || type == CombatType.Ambush;

        public void ModifyParameters(ref CombatParameters p)
        {
            if (p.IgnoreTerrainDefense) return;
            p.TerrainAttackModifier = GetAttackMod(p.AttackerCoord);
            p.TerrainDefenseBonus   = _terrain.GetDefenseBonus(p.DefenderCoord);
        }

        float GetAttackMod(HexCoord coord)
        {
            // 从防御加成反推地形攻击系数（正式项目可扩展 ITerrainQuery 提供攻击修正）
            float def = _terrain.GetDefenseBonus(coord);
            return def switch
            {
                >= 0.4f => 0.6f, // 河流
                >= 0.28f => 0.7f, // 山地
                >= 0.19f => 0.8f, // 城市/森林/丘陵
                _ => 1.0f
            };
        }
    }

    public sealed class SupplyCombatRule : ICombatRule
    {
        public string RuleId => "supply";

        public bool Applies(IUnitToken attacker, IUnitToken defender, CombatType type) => true;

        public void ModifyParameters(ref CombatParameters p)
        {
            p.SupplyAttackModifier  = AttackMod(p.Attacker.Supply);
            p.SupplyDefenseModifier = DefenseMod(p.Defender.Supply);
        }

        static float AttackMod(int supply) => supply switch
        {
            >= 3 => 1.0f,
            >= 1 => 0.8f,
            _    => 0.5f
        };

        static float DefenseMod(int supply) => supply switch
        {
            >= 3 => 1.0f,
            >= 1 => 0.85f,
            _    => 0.6f
        };
    }

    public sealed class StateCombatRule : ICombatRule
    {
        public string RuleId => "state";

        public bool Applies(IUnitToken attacker, IUnitToken defender, CombatType type) => true;

        public void ModifyParameters(ref CombatParameters p)
        {
            if (p.Attacker.State == UnitState.Inspired) p.AttackBonusFlat += 2;
            if (p.Attacker.State == UnitState.Shaken)   p.AttackBonusFlat -= 1;

            if (p.Defender.State == UnitState.Shaken)   p.TerrainDefenseBonus -= 1;
            if (p.Defender.State == UnitState.Routed)   p.TerrainDefenseBonus -= 2;
        }
    }

    public sealed class CombinedAttackRule : ICombatRule
    {
        public string RuleId => "combined_attack";

        static readonly float[] _bonuses = { 1.0f, 1.15f, 1.30f, 1.50f, 1.65f, 1.80f };

        readonly IMapDataProvider _map;

        public CombinedAttackRule(IMapDataProvider map) => _map = map;

        public bool Applies(IUnitToken attacker, IUnitToken defender, CombatType type) =>
            type == CombatType.CombinedAssault;

        public void ModifyParameters(ref CombatParameters p)
        {
            int directions = CountAttackDirections(p.AttackerCoord, p.DefenderCoord);
            int idx = UnityEngine.Mathf.Clamp(directions - 1, 0, _bonuses.Length - 1);
            p.CombinedAttackBonus = _bonuses[idx];
        }

        int CountAttackDirections(HexCoord attacker, HexCoord defender)
        {
            // 单攻方时从 attacker→defender 方向出发，统计到有友军的相邻格方向数
            var neighbors = defender.GetNeighbors();
            int count = 0;
            foreach (var n in neighbors)
            {
                var unit = _map.GetTile(n);
                if (unit != null) count++;
            }
            return UnityEngine.Mathf.Max(1, count);
        }
    }

    public sealed class FactionSpecialRule : ICombatRule
    {
        public string RuleId => "faction_special";

        public bool Applies(IUnitToken attacker, IUnitToken defender, CombatType type) => true;

        public void ModifyParameters(ref CombatParameters p)
        {
            // 伏击：无视地形防御，攻击+4
            if (p.Attacker.Faction == Faction.PLA && p.IgnoreTerrainDefense)
            {
                p.AttackBonusFlat      += 4;
                p.TerrainDefenseBonus   = 0;
                p.FortificationLevel    = 0;
            }
        }
    }
}
