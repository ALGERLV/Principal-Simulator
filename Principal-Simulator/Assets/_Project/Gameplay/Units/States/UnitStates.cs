using UnityEngine;

namespace TBS.Unit
{
    // ---------- 高昂 ----------
    // 触发：士气≥80 且 兵力≥4；攻击+2，速度+20%
    public class InspiredState : IStateHandler
    {
        public UnitState State => UnitState.Inspired;

        public UnitState Evaluate(IUnitToken unit) =>
            unit.Morale >= 80 && unit.Strength >= 4 ? UnitState.Inspired : UnitState.Normal;

        public void OnEnter(IUnitToken unit)
        {
            if (unit is Unit u) { u.AttackModifier += 2; u.SpeedModifier *= 1.2f; }
        }

        public void OnExit(IUnitToken unit)
        {
            if (unit is Unit u) { u.AttackModifier -= 2; u.SpeedModifier /= 1.2f; }
        }
    }

    // ---------- 正常 ----------
    public class NormalState : IStateHandler
    {
        public UnitState State => UnitState.Normal;

        public UnitState Evaluate(IUnitToken unit) => UnitState.Normal;

        public void OnEnter(IUnitToken unit) { }
        public void OnExit(IUnitToken unit) { }
    }

    // ---------- 压制 ----------
    // 持续2-4小时（由Unit内部计时器管理），不可主动攻击，速度×0.5
    public class SuppressedState : IStateHandler
    {
        public UnitState State => UnitState.Suppressed;

        public UnitState Evaluate(IUnitToken unit) => UnitState.Normal; // 不主动触发

        public void OnEnter(IUnitToken unit)
        {
            if (unit is Unit u) { u.SpeedModifier *= 0.5f; u.CanAttack = false; }
        }

        public void OnExit(IUnitToken unit)
        {
            if (unit is Unit u) { u.SpeedModifier /= 0.5f; u.CanAttack = true; }
        }
    }

    // ---------- 动摇 ----------
    // 攻防各-1，速度-20%；满足各等级动摇条件
    public class ShakenState : IStateHandler
    {
        public UnitState State => UnitState.Shaken;

        public UnitState Evaluate(IUnitToken unit)
        {
            if (unit is not Unit u) return UnitState.Normal;

            // 已满足崩溃条件则交由 RoutedState 处理
            if (IsRouted(u)) return UnitState.Normal;

            return u.Morale < u.ShakenMoraleThreshold ? UnitState.Shaken : UnitState.Normal;
        }

        public void OnEnter(IUnitToken unit)
        {
            if (unit is Unit u) { u.AttackModifier -= 1; u.DefenseModifier -= 1; u.SpeedModifier *= 0.8f; }
        }

        public void OnExit(IUnitToken unit)
        {
            if (unit is Unit u) { u.AttackModifier += 1; u.DefenseModifier += 1; u.SpeedModifier /= 0.8f; }
        }

        static bool IsRouted(Unit u) =>
            u.Strength <= 0 ||
            u.Morale == 0 ||
            (u.Strength <= u.RoutedStrengthThreshold && u.Morale <= u.RoutedMoraleThreshold) ||
            (u.Grade == UnitGrade.KMT_Militia && u.Strength <= 2);
    }

    // ---------- 溃散 ----------
    // 强制撤退，速度×0.7，丧失指令；防御-2
    public class RoutedState : IStateHandler
    {
        public UnitState State => UnitState.Routed;

        public UnitState Evaluate(IUnitToken unit)
        {
            if (unit is not Unit u) return UnitState.Normal;
            if (IsRouted(u)) return UnitState.Routed;
            return UnitState.Normal;
        }

        public void OnEnter(IUnitToken unit)
        {
            if (unit is Unit u)
            {
                u.DefenseModifier -= 2;
                u.SpeedModifier *= 0.7f;
                u.CanAttack = false;
                u.CanReceiveOrders = false;
            }
        }

        public void OnExit(IUnitToken unit)
        {
            if (unit is Unit u)
            {
                u.DefenseModifier += 2;
                u.SpeedModifier /= 0.7f;
                u.CanAttack = true;
                u.CanReceiveOrders = true;
            }
        }

        internal static bool IsRouted(Unit u) =>
            u.Strength <= 0 ||
            u.Morale == 0 ||
            (u.Strength <= u.RoutedStrengthThreshold && u.Morale <= u.RoutedMoraleThreshold) ||
            (u.Grade == UnitGrade.KMT_Militia && u.Strength <= 2);
    }

    // ---------- 后撤整补 ----------
    // 脱离前线+有补给时自动恢复，速度/攻防恢复基准值
    public class RecuperatingState : IStateHandler
    {
        public UnitState State => UnitState.Recuperating;

        // 由 Unit.TryEnterRecuperation() 主动触发，Evaluate 不自动进入
        public UnitState Evaluate(IUnitToken unit) => UnitState.Normal;

        public void OnEnter(IUnitToken unit) { }
        public void OnExit(IUnitToken unit) { }
    }
}
