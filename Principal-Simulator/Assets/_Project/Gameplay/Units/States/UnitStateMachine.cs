using UnityEngine;

namespace TBS.Unit
{
    /// <summary>
    /// 状态机 — 每帧/每小时评估单位状态，按文档§5.2优先级执行
    /// </summary>
    public class UnitStateMachine
    {
        private IStateHandler currentHandler;
        private readonly IStateHandler[] handlers;

        public UnitState CurrentState => currentHandler?.State ?? UnitState.Normal;

        public UnitStateMachine()
        {
            handlers = new IStateHandler[]
            {
                new SuppressedState(),
                new InspiredState(),
                new RoutedState(),
                new ShakenState(),
                new RecuperatingState(),
                new NormalState()
            };
            currentHandler = handlers[5]; // 默认 Normal
        }

        /// <summary>
        /// 按优先级评估并切换状态（优先级1由外部事件触发 Suppress，此处处理优先级2+3）
        /// </summary>
        public void Evaluate(Unit unit)
        {
            // 优先级3：兵力=0直接移除（由外部处理，这里跳过）
            if (unit.Strength <= 0) return;

            // 已压制状态由计时器自行管理，不参与普通评估
            if (CurrentState == UnitState.Suppressed) return;

            foreach (var handler in handlers)
            {
                if (handler.State == UnitState.Suppressed) continue;
                var next = handler.Evaluate(unit);
                if (next == handler.State)
                {
                    TransitionTo(handler, unit);
                    return;
                }
            }
        }

        /// <summary>
        /// 强制进入压制状态（炮击/密集射击事件触发，优先级1）
        /// </summary>
        public void ForceSuppress(Unit unit)
        {
            var suppressed = handlers[0]; // SuppressedState
            TransitionTo(suppressed, unit);
        }

        public void ClearSuppression(Unit unit)
        {
            if (CurrentState == UnitState.Suppressed)
                Evaluate(unit);
        }

        private void TransitionTo(IStateHandler next, Unit unit)
        {
            if (currentHandler == next) return;
            currentHandler?.OnExit(unit);
            currentHandler = next;
            currentHandler.OnEnter(unit);
        }
    }
}
