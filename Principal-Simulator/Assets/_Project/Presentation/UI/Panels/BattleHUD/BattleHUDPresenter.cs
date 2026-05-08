using UnityEngine;
using TBS.Core;
using TBS.Core.Events;
using TBS.Contracts.Events;
using TBS.Presentation.UI;

namespace TBS.Presentation.UI.Panels.BattleHUD
{
    public class BattleHUDPresenter : BasePresenter<BattleHUDView, BattleHUDViewModel>
    {
        private GameTimeSystem gameTimeSystem;

        protected override void OnInitialize()
        {
            Debug.Log("[BattleHUDPresenter] OnInitialize");

            // 订阅游戏启动事件，初始化时间
            EventBus.On<GameStartRequestedEvent>(OnGameStarted);

            // 获取时间系统引用，用于实时更新
            gameTimeSystem = GameTimeSystem.Instance;
            if (gameTimeSystem != null)
            {
                ViewModel.UpdateTime(gameTimeSystem.GameDay);
                Debug.Log($"[BattleHUDPresenter] 已获取 GameTimeSystem，当前游戏天数: {gameTimeSystem.GameDay}");
            }

            Debug.Log("[BattleHUDPresenter] 初始化完成");
        }

        private void OnGameStarted(GameStartRequestedEvent evt)
        {
            Debug.Log("[BattleHUDPresenter] 游戏已启动，刷新时间显示");
            if (gameTimeSystem != null)
            {
                ViewModel.UpdateTime(gameTimeSystem.GameDay);
            }
        }

        public override void OnShow()
        {
            base.OnShow();
            Debug.Log("[BattleHUDPresenter] BattleHUD 显示");
        }

        public override void OnHide()
        {
            base.OnHide();
            Debug.Log("[BattleHUDPresenter] BattleHUD 隐藏");
        }

        public void OnMenuClicked()
        {
            Debug.Log("[BattleHUDPresenter] 菜单按钮点击");
            EventBus.Emit(new GameExitRequestedEvent());
        }

        public override void OnDestroy()
        {
            EventBus.Off<GameStartRequestedEvent>(OnGameStarted);
            base.OnDestroy();
            Debug.Log("[BattleHUDPresenter] 已销毁");
        }
    }
}
