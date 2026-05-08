using UnityEngine;
using UnityEngine.UI;
using System;

namespace TBS.Presentation.UI.Panels.MainMenu
{
    public class MainMenuView : BaseView<MainMenuViewModel>
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _TestButton1;

        private Action _onStartClick;
        private Action _onExitClick;

        protected override IPresenter CreatePresenter()
        {
            var presenter = new MainMenuPresenter();
            return presenter;
        }

    protected override void OnBind()
    {
        Debug.Log("[MainMenuView] OnBind - 开始绑定UI");

        // 如果通过Inspector没有关联，则自动查找子物体
        if (_titleText == null)
        {
            _titleText = GetComponentInChildren<Text>();
            Debug.Log($"[MainMenuView] 自动查找 Title: {(_titleText != null ? "成功" : "失败")}");
        }
        if (_startButton == null)
        {
            _startButton = transform.Find("StartButton")?.GetComponent<Button>();
            Debug.Log($"[MainMenuView] 自动查找 StartButton: {(_startButton != null ? "成功" : "失败")}");
        }
        if (_exitButton == null)
        {
            _exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
            Debug.Log($"[MainMenuView] 自动查找 ExitButton: {(_exitButton != null ? "成功" : "失败")}");
        }

        // 更新标题文本
        if (_titleText != null)
        {
            _titleText.text = ViewModel.Title;
            Bind(nameof(ViewModel.Title), () =>
            {
                if (_titleText != null)
                    _titleText.text = ViewModel.Title;
            });
            Debug.Log("[MainMenuView] Title 绑定成功");
        }

        // 绑定按钮事件
        if (_startButton != null)
        {
            Debug.Log($"[MainMenuView] StartButton 状态 - Interactable: {_startButton.interactable}, Active: {_startButton.gameObject.activeInHierarchy}");
            Debug.Log($"[MainMenuView] Canvas 配置 - RenderMode: {GetComponentInParent<Canvas>()?.renderMode}");

            _startButton.onClick.AddListener(() =>
            {
                Debug.Log("[MainMenuView] 点击 StartButton");
                _onStartClick?.Invoke();
            });
            Debug.Log("[MainMenuView] StartButton 监听器已添加");
        }
        else
            Debug.LogError("[MainMenuView] 找不到 StartButton");

        if (_exitButton != null)
        {
            Debug.Log($"[MainMenuView] ExitButton 状态 - Interactable: {_exitButton.interactable}, Active: {_exitButton.gameObject.activeInHierarchy}");

            _exitButton.onClick.AddListener(() =>
            {
                Debug.Log("[MainMenuView] 点击 ExitButton");
                _onExitClick?.Invoke();
            });
            Debug.Log("[MainMenuView] ExitButton 监听器已添加");
        }
        else
            Debug.LogError("[MainMenuView] 找不到 ExitButton");

        // 绑定 TestButton1：禁用屏幕边缘滚动
        if (_TestButton1 != null)
        {
            _TestButton1.onClick.AddListener(() =>
            {
                Debug.Log("[MainMenuView] 点击 TestButton1 - 禁用屏幕边缘滚动");
                var cameraController = FindObjectOfType<TBS.Presentation.Camera.BoardCameraController>();
                if (cameraController != null)
                {
                    cameraController.SetEdgeScrollingEnabled(false);
                    Debug.Log("[MainMenuView] 已禁用屏幕边缘滚动功能");
                }
                else
                {
                    Debug.LogWarning("[MainMenuView] 找不到 BoardCameraController");
                }
            });
            Debug.Log("[MainMenuView] TestButton1 监听器已添加");
        }

        // 检查EventSystem
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        Debug.Log($"[MainMenuView] EventSystem: {(eventSystem != null ? "存在" : "缺失")}");

        Debug.Log("[MainMenuView] OnBind - 绑定完成");
    }

        public void BindStartButton(Action callback)
        {
            _onStartClick = callback;
        }

        public void BindExitButton(Action callback)
        {
            _onExitClick = callback;
        }

        /// <summary>
        /// 设置UI元素（通常用于编辑器生成Prefab时调用）
        /// </summary>
        public void SetUIElements(Text titleText, Button startButton, Button exitButton)
        {
            _titleText = titleText;
            _startButton = startButton;
            _exitButton = exitButton;
        }
    }
}
