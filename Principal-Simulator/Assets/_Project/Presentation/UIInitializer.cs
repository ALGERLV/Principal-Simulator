using UnityEngine;
using TBS.Presentation.UI;
using TBS.Presentation.UI.Panels.MainMenu;

namespace TBS.Presentation
{
    /// <summary>
    /// UI系统初始化脚本
    /// 在游戏启动时注册所有UI配置和加载Prefab
    /// </summary>
    public class UIInitializer : MonoBehaviour
    {
        private void Awake()
        {
            // Awake 中保留自动初始化，以支持独立运行场景
            InitializeUIConfigs();
        }

        private void Start()
        {
            // Start 中不再自动显示 MainMenu，由 GameManager 控制
            // 如果需要独立测试此脚本，可在 Inspector 中配置
        }

        public void InitializeUIConfigs()
        {
            var uiManager = UIManager.Instance;

            // 加载Prefabs（从Resources文件夹）
            var mainMenuPrefab = Resources.Load<GameObject>("Prefabs/UI/MainMenu/MainMenu");

            if (mainMenuPrefab == null)
            {
                Debug.LogError("[UIInitializer] 无法加载MainMenu预制体，请检查路径: Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab");
                return;
            }

            // 注册MainMenu配置
            var mainMenuConfig = new UIConfig
            {
                UIId = typeof(MainMenuView).Name,
                Prefab = mainMenuPrefab,
                Parent = uiManager.UIRoot,
                CacheOnHide = true,
                IsModal = true,
                CustomSortingOrder = -1
            };

            uiManager.RegisterConfig(mainMenuConfig);

            Debug.Log("[UIInitializer] UI系统初始化完成");
        }

        private void ShowMainMenu()
        {
            UIManager.Instance.Show<MainMenuView>();
        }
    }
}
