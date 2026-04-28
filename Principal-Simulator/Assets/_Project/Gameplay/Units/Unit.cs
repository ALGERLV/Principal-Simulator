using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Unit
{
    /// <summary>
    /// 兵牌运行时实体 — 对应文档第3节属性体系和第5节状态系统
    /// </summary>
    public class Unit : MonoBehaviour, IUnitToken
    {
        #region Serialized

        private UnitData data;

        #endregion

        #region Runtime State

        private HexCoord position;
        private int strength;
        private int morale;
        private int supply;
        private int fortificationLevel;
        private float fortificationProgress;

        // 压制计时（小时）
        private float suppressionTimer;
        private const float SuppressDurationMin = 2f;
        private const float SuppressDurationMax = 4f;

        // 修正量（由状态机叠加/恢复）
        public int AttackModifier { get; internal set; }
        public int DefenseModifier { get; internal set; }
        public float SpeedModifier { get; internal set; } = 1f;
        public bool CanAttack { get; internal set; } = true;
        public bool CanReceiveOrders { get; internal set; } = true;

        private readonly UnitStateMachine stateMachine = new();

        #endregion

        #region IUnitToken

        public string UnitId => data ? data.UnitId : "";
        public string DisplayName => data ? data.DisplayName : "";
        public Faction Faction => data ? data.Faction : Faction.KMT;
        public UnitTier Tier => data ? data.Tier : UnitTier.Regiment;
        public UnitGrade Grade => data ? data.Grade : UnitGrade.KMT_Regular;

        public int AttackPower => data ? data.AttackPower : 0;
        public int DefensePower => data ? data.DefensePower : 0;
        public float MoveSpeedKmPerDay => data ? data.MoveSpeedKmPerDay : 0;
        public int Firepower => data ? data.Firepower : 0;

        public int Strength
        {
            get => strength;
            set => strength = Mathf.Clamp(value, 0, 5);
        }

        public int Morale
        {
            get => morale;
            set => morale = Mathf.Clamp(value, 0, 100);
        }

        public int Supply
        {
            get => supply;
            set => supply = Mathf.Clamp(value, 0, 5);
        }

        public UnitState State => stateMachine.CurrentState;
        public int FortificationLevel => fortificationLevel;
        public float FortificationProgress => fortificationProgress;
        public HexCoord Position => position;

        #endregion

        #region Public

        public UnitData Data => data;

        /// <summary>
        /// 计算有效攻击力（基础 + 修正）
        /// </summary>
        public int EffectiveAttack => Mathf.Max(0, AttackPower + AttackModifier);

        /// <summary>
        /// 计算有效防御力（基础 + 修正 + 工事加成）
        /// </summary>
        public int EffectiveDefense => Mathf.Max(0, DefensePower + DefenseModifier + fortificationLevel);

        /// <summary>
        /// 计算有效行军速度（km/天，含修正）
        /// </summary>
        public float EffectiveMoveSpeed => MoveSpeedKmPerDay * SpeedModifier;

        #endregion

        #region Lifecycle

        /// <summary>
        /// 由时间系统每游戏小时调用一次
        /// </summary>
        public void Tick(float deltaHours, WeatherType weather, bool isNight)
        {
            TickSuppressionTimer(deltaHours);
            TickFortification(deltaHours, weather);
            TickMoraleRecovery(deltaHours);
            TickRecuperation(deltaHours);
            stateMachine.Evaluate(this);

            if (strength <= 0)
                OnEliminated();
        }

        #endregion

        #region Initialize

        public void Initialize(UnitData unitData, HexCoord coord)
        {
            data = unitData;
            position = coord;
            strength = unitData.InitialStrength;
            morale = unitData.InitialMorale;
            supply = unitData.InitialSupply;
            fortificationLevel = 0;
            fortificationProgress = 0;
            AttackModifier = 0;
            DefenseModifier = 0;
            SpeedModifier = 1f;
            CanAttack = true;
            CanReceiveOrders = true;
        }

        #endregion

        #region Events

        public event System.Action<Unit> OnEliminatedEvent;

        private void OnEliminated()
        {
            OnEliminatedEvent?.Invoke(this);
            Destroy(gameObject);
        }

        #endregion

        #region Combat

        /// <summary>
        /// 受到炮击/密集射击，进入压制状态（优先级1）
        /// </summary>
        public void ApplySuppression()
        {
            // 工事越高，需要的次数越多（已在战斗系统判断，此处直接压制）
            suppressionTimer = Random.Range(SuppressDurationMin, SuppressDurationMax);
            stateMachine.ForceSuppress(this);
        }

        /// <summary>
        /// 受到兵力损失
        /// </summary>
        public void TakeStrengthDamage(int amount)
        {
            Strength -= amount;
            Morale -= amount * 6; // 文档§5.5：兵力每降1点士气-6
        }

        #endregion

        #region Morale Events (文档§5.5)

        public void OnBattleWon() => Morale += 5;
        public void OnBattleLost() => Morale -= 8;
        public void OnFriendlyUnitEliminatedNearby() => Morale -= 10;
        public void OnHQEliminated() => Morale -= 20;
        public void OnAmbushSuccess() => Morale += 10; // 八路军专属
        public void OnPositionBreached() => Morale -= 12;

        // 由时间系统每天调用
        public void OnDailySupplyTick()
        {
            if (supply >= 3) Morale += 3;
            else if (supply == 0) Morale -= 8;
        }

        #endregion

        #region Recuperation

        /// <summary>
        /// 尝试进入整补状态（由外部补给/位置系统判断三个条件后调用）
        /// </summary>
        public void TryEnterRecuperation()
        {
            if (State == UnitState.Routed || State == UnitState.Suppressed) return;
            // 状态机强制切换至整补
            if (State != UnitState.Recuperating)
                stateMachine.ClearSuppression(this); // 确保刷新一次评估
        }

        public void ExitRecuperation()
        {
            stateMachine.Evaluate(this);
        }

        #endregion

        #region Private Ticks

        private void TickSuppressionTimer(float deltaHours)
        {
            if (State != UnitState.Suppressed) return;
            suppressionTimer -= deltaHours;
            if (suppressionTimer <= 0)
                stateMachine.ClearSuppression(this);
        }

        private void TickFortification(float deltaHours, WeatherType weather)
        {
            if (fortificationLevel >= 4) return;

            // 移动或主动攻击时暂停（由外部设置 isFortifying = false）
            float speedMult = weather == WeatherType.Rain ? 0.5f : 1.0f;
            fortificationProgress += deltaHours / 24f * speedMult; // 1天=1级

            if (fortificationProgress >= 1f)
            {
                fortificationProgress -= 1f;
                fortificationLevel = Mathf.Min(4, fortificationLevel + 1);
            }
        }

        private void TickMoraleRecovery(float deltaHours)
        {
            if (data == null) return;
            if (State == UnitState.Routed)
            {
                // 溃散时恢复×0.3
                Morale += Mathf.RoundToInt(data.MoraleRecoveryPerHour * 0.3f * deltaHours);
                return;
            }

            // 非战斗且持续>1小时才恢复（简化：非Suppressed即可）
            if (State == UnitState.Suppressed) return;

            float mult = 1f;
            if (supply >= 3) mult *= 1.5f;
            else if (supply == 0) return; // 断补：无法自然恢复

            Morale += Mathf.RoundToInt(data.MoraleRecoveryPerHour * mult * deltaHours);
        }

        private void TickRecuperation(float deltaHours)
        {
            if (State != UnitState.Recuperating) return;
            if (supply <= 0) return;

            recuperationAccum += deltaHours / 24f * data.RecuperationSpeedModifier;
            if (recuperationAccum >= 1f)
            {
                recuperationAccum -= 1f;
                Strength++;
                supply = Mathf.Max(0, supply - 1);
            }
        }

        private float recuperationAccum;

        #endregion

        #region Fortification

        /// <summary>
        /// 炮击破坏工事（由 FortificationSystem 调用）
        /// </summary>
        public void DamageFortification()
        {
            fortificationLevel = Mathf.Max(0, fortificationLevel - 1);
            fortificationProgress = 0;
        }

        /// <summary>
        /// 停止工事建设（移动/攻击时调用）
        /// </summary>
        public void PauseFortification() { /* 进度保留，仅停止推进 */ }

        #endregion
    }
}
