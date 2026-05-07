using TBS.Presentation.UI;

namespace TBS.Presentation.UI.Panels.MainMenu
{
    public class MainMenuViewModel : ViewModelBase
    {
        private string _title = "电子战棋";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public RelayCommand StartGameCommand { get; }
        public RelayCommand ExitGameCommand { get; }

        public MainMenuViewModel()
        {
            StartGameCommand = new RelayCommand(OnStartGame);
            ExitGameCommand = new RelayCommand(OnExitGame);
        }

        private void OnStartGame()
        {
            // TODO: 发送事件或调用GamePlay层开始游戏
        }

        private void OnExitGame()
        {
            // TODO: 发送事件或调用GamePlay层退出游戏
        }
    }
}
