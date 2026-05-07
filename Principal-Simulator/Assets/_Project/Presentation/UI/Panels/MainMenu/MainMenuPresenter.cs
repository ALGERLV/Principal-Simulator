using UnityEngine;
using TBS.Core.Events;
using TBS.Contracts.Events;

namespace TBS.Presentation.UI.Panels.MainMenu
{
    public class MainMenuPresenter : BasePresenter<MainMenuView, MainMenuViewModel>
    {
        protected override void OnInitialize()
        {
            // 绑定命令
            View.BindStartButton(OnStartClicked);
            View.BindExitButton(OnExitClicked);
        }

        private void OnStartClicked()
        {
            ViewModel.StartGameCommand.Execute();
            Debug.Log("[MainMenuPresenter] 开始游戏");
            // 发送事件通知GameManager开始游戏
            EventBus.Emit(new GameStartRequestedEvent());
        }

        private void OnExitClicked()
        {
            ViewModel.ExitGameCommand.Execute();
            Debug.Log("[MainMenuPresenter] 退出游戏");
            // 发送事件通知GameManager退出
            EventBus.Emit(new GameExitRequestedEvent());
        }

        public override void OnShow()
        {
            base.OnShow();
            Debug.Log("[MainMenuPresenter] 主菜单显示");
        }

        public override void OnHide()
        {
            base.OnHide();
            Debug.Log("[MainMenuPresenter] 主菜单隐藏");
        }
    }
}
