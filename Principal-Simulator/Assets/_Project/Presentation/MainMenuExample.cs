using UnityEngine;
using TBS.Presentation.UI;
using TBS.Presentation.UI.Panels.MainMenu;

namespace TBS.Presentation
{
    /// <summary>
    /// MainMenuUI使用示例
    /// 演示如何通过UIManager显示/隐藏/管理MainMenuUI
    /// </summary>
    public class MainMenuExample : MonoBehaviour
    {
        private void Start()
        {
            // 示例1: 通过类型显示MainMenu
            UIManager.Instance.Create<MainMenuView>(view =>
            {
                Debug.Log("[Example] MainMenu创建完成，准备显示");
                UIManager.Instance.Show<MainMenuView>();
            });

            // 注意: 实际使用中应该通过事件系统通知业务逻辑
            // 例如当用户点击开始游戏时，Presenter会触发相应的事件
        }

        // 示例2: 隐藏MainMenu
        public void HideMainMenu()
        {
            UIManager.Instance.Hide<MainMenuView>();
        }

        // 示例3: 销毁MainMenu
        public void DestroyMainMenu()
        {
            UIManager.Instance.Destroy<MainMenuView>();
        }

        // 示例4: 切换MainMenu显示/隐藏
        public void ToggleMainMenu()
        {
            UIManager.Instance.Toggle<MainMenuView>();
        }

        // 示例5: 获取MainMenu实例
        public void GetMainMenuInstance()
        {
            var mainMenu = UIManager.Instance.Get<MainMenuView>();
            if (mainMenu != null)
            {
                Debug.Log($"[Example] 获取MainMenu实例成功，当前可见: {mainMenu.IsVisible}");
            }
        }
    }
}
