using System.Collections.Generic;
using TBS.Map.Data;
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

        private LevelConfig _selectedLevel;
        public LevelConfig SelectedLevel
        {
            get => _selectedLevel;
            set => SetProperty(ref _selectedLevel, value);
        }

        public List<LevelConfig> AvailableLevels { get; } = new List<LevelConfig>();

        public RelayCommand StartGameCommand { get; }
        public RelayCommand ExitGameCommand { get; }

        public MainMenuViewModel()
        {
            StartGameCommand = new RelayCommand(OnStartGame);
            ExitGameCommand = new RelayCommand(OnExitGame);

            LoadAvailableLevels();
        }

        void LoadAvailableLevels()
        {
            AvailableLevels.Add(TestLevelBuilder.Build());
        }

        private void OnStartGame() { }
        private void OnExitGame() { }
    }
}
