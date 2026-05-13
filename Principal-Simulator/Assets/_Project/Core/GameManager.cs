using UnityEngine;
using TBS.Map.Managers;
using TBS.Map.Data;
using TBS.Core.Events;
using TBS.Contracts.Events;
using TBS.Presentation;
using TBS.Presentation.UI;
using TBS.Presentation.UI.Panels.MainMenu;
using TBS.Presentation.UI.Panels.BattleHUD;
using TBS.Presentation.UI.Panels.SpawnPanel;

namespace TBS.Core
{
    /// <summary>
    /// 游戏管理器 - 游戏启动的唯一入口
    /// 负责初始化各大模块、管理游戏状态、接收和转发关键事件
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState { Boot, MainMenu, InGame, Paused }

        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; }

        [SerializeField] private UIInitializer uiInitializer;
        [SerializeField] private InputRouter inputRouter;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // 确保 SpawnController 存在
            if (GetComponent<SpawnController>() == null)
            {
                gameObject.AddComponent<SpawnController>();
                Debug.Log("[GameManager] 动态添加了 SpawnController");
            }

            // 订阅事件
            EventBus.On<GameStartRequestedEvent>(OnGameStartRequested);
            EventBus.On<GameExitRequestedEvent>(OnGameExitRequested);

            Debug.Log("[GameManager] Awake - 单例初始化完成");
        }

        private void Start()
        {
            InitModules();
        }

        /// <summary>
        /// 初始化所有游戏模块
        /// </summary>
        private void InitModules()
        {
            Debug.Log("[GameManager] 开始初始化模块");

            // 1. 初始化 UI 配置（调用 UIInitializer 的初始化逻辑）
            if (uiInitializer != null)
            {
                uiInitializer.InitializeUIConfigs();
            }
            else
            {
                Debug.LogWarning("[GameManager] UIInitializer 未指定");
            }

            // 2. InputRouter 默认禁用（只在 InGame 时启用）
            if (inputRouter != null)
            {
                inputRouter.Enabled = false;
            }

            // 3. 显示主菜单
            ShowMainMenu();

            Debug.Log("[GameManager] 模块初始化完成");
        }

        /// <summary>
        /// 显示主菜单
        /// </summary>
        private void ShowMainMenu()
        {
            SetState(GameState.MainMenu);
            UIManager.Instance.Show<MainMenuView>();
            UIManager.Instance.Hide<BattleHUDView>();
            UIManager.Instance.Hide<SpawnPanelView>();
            if (inputRouter != null)
                inputRouter.Enabled = false;
        }

        /// <summary>
        /// 游戏启动事件处理
        /// </summary>
        private void OnGameStartRequested(GameStartRequestedEvent evt)
        {
            StartGame(evt.LevelConfig);
        }

        /// <summary>
        /// 游戏退出事件处理
        /// </summary>
        private void OnGameExitRequested(GameExitRequestedEvent evt)
        {
            ExitGame();
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame(LevelConfig levelConfig = null)
        {
            UIManager.Instance.Hide<MainMenuView>();
            UIManager.Instance.Show<BattleHUDView>();
            UIManager.Instance.Show<SpawnPanelView>();
            SetState(GameState.InGame);
            if (inputRouter != null)
                inputRouter.Enabled = true;

            if (levelConfig != null)
            {
                MapManager.Instance?.InitializeFromLevel(levelConfig);
                Debug.Log($"[GameManager] 游戏开始 - 加载关卡: {levelConfig.LevelName}");
            }
            else
            {
                MapManager.Instance?.InitializeMap();
                Debug.Log("[GameManager] 游戏开始 - 随机地图");
            }
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void ExitGame()
        {
            Debug.Log("[GameManager] 退出游戏");

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// 设置游戏状态
        /// </summary>
        private void SetState(GameState newState)
        {
            State = newState;
            Debug.Log($"[GameManager] 状态变化: {newState}");
        }

        private void OnDestroy()
        {
            // 取消事件订阅
            EventBus.Off<GameStartRequestedEvent>(OnGameStartRequested);
            EventBus.Off<GameExitRequestedEvent>(OnGameExitRequested);
        }
    }
}
