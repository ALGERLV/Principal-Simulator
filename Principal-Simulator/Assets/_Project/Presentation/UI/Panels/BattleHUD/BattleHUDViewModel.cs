using System.Runtime.CompilerServices;
using TBS.Presentation.UI;

namespace TBS.Presentation.UI.Panels.BattleHUD
{
    public class BattleHUDViewModel : ViewModelBase
    {
        public string GameTitle
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string DayText
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public BattleHUDViewModel()
        {
            GameTitle = "电子战棋";
            DayText = "第 1 天";
        }

        public void UpdateTime(int gameDay)
        {
            DayText = $"第 {gameDay} 天";
        }
    }
}
