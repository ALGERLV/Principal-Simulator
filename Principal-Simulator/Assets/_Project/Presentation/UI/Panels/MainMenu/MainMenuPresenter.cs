using UnityEngine;
using TBS.Core.Events;
using TBS.Contracts.Events;

namespace TBS.Presentation.UI.Panels.MainMenu
{
    public class MainMenuPresenter : BasePresenter<MainMenuView, MainMenuViewModel>
    {
        protected override void OnInitialize()
        {
            View.BindStartButton(OnStartClicked);
            View.BindExitButton(OnExitClicked);
            View.BindLevelSelected(OnLevelSelected);
        }

        private void OnStartClicked()
        {
            ViewModel.StartGameCommand.Execute();
            var evt = new GameStartRequestedEvent
            {
                LevelConfig = ViewModel.SelectedLevel
            };
            EventBus.Emit(evt);
            Debug.Log($"[MainMenuPresenter] 开始游戏 - 关卡: {ViewModel.SelectedLevel?.LevelName ?? "随机"}");
        }

        private void OnExitClicked()
        {
            ViewModel.ExitGameCommand.Execute();
            EventBus.Emit(new GameExitRequestedEvent());
        }

        private void OnLevelSelected(TBS.Map.Data.LevelConfig config)
        {
            Debug.Log($"[MainMenuPresenter] 选择关卡: {config.LevelName}");
        }

        public override void OnShow()
        {
            base.OnShow();
        }

        public override void OnHide()
        {
            base.OnHide();
        }
    }
}
