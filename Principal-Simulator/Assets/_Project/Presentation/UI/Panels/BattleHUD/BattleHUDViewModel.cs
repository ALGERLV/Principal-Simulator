using System.Runtime.CompilerServices;
using UnityEngine;
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

        public string TimeText
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public BattleHUDViewModel()
        {
            GameTitle = "电子战棋";
            DayText = "第 1 天";
            TimeText = "0:00";
        }

        public void UpdateTime(float gameHours)
        {
            int day = Mathf.FloorToInt(gameHours / 24f);
            int hour = Mathf.FloorToInt(gameHours);

            DayText = $"第 {day + 1} 天";
            TimeText = $"{hour}小时";
        }
    }
}
