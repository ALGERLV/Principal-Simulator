using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TBS.Map.Data;

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
        private Action<LevelConfig> _onLevelSelected;

        private List<GameObject> _levelButtons = new List<GameObject>();
        private Button _selectedLevelButton;

        protected override IPresenter CreatePresenter()
        {
            var presenter = new MainMenuPresenter();
            return presenter;
        }

    protected override void OnBind()
    {
        Debug.Log("[MainMenuView] OnBind - 开始绑定UI");

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

        if (_titleText != null)
        {
            _titleText.text = ViewModel.Title;
            Bind(nameof(ViewModel.Title), () =>
            {
                if (_titleText != null)
                    _titleText.text = ViewModel.Title;
            });
        }

        if (_startButton != null)
        {
            _startButton.onClick.AddListener(() =>
            {
                Debug.Log("[MainMenuView] 点击 StartButton");
                _onStartClick?.Invoke();
            });
        }
        else
            Debug.LogError("[MainMenuView] 找不到 StartButton");

        if (_exitButton != null)
        {
            _exitButton.onClick.AddListener(() =>
            {
                Debug.Log("[MainMenuView] 点击 ExitButton");
                _onExitClick?.Invoke();
            });
        }
        else
            Debug.LogError("[MainMenuView] 找不到 ExitButton");

        if (_TestButton1 != null)
        {
            _TestButton1.onClick.AddListener(() =>
            {
                Debug.Log("[MainMenuView] 点击 TestButton1 - 禁用屏幕边缘滚动");
                var cameraController = FindObjectOfType<TBS.Presentation.Camera.BoardCameraController>();
                if (cameraController != null)
                    cameraController.SetEdgeScrollingEnabled(false);
            });
        }

        // 生成关卡选择按钮
        CreateLevelButtons();

        Debug.Log("[MainMenuView] OnBind - 绑定完成");
    }

        void CreateLevelButtons()
        {
            // 清理旧按钮
            foreach (var go in _levelButtons)
                if (go != null) Destroy(go);
            _levelButtons.Clear();

            if (ViewModel.AvailableLevels.Count == 0) return;

            // 找到按钮容器（StartButton 的父级）
            Transform container = _startButton != null ? _startButton.transform.parent : transform;

            for (int i = 0; i < ViewModel.AvailableLevels.Count; i++)
            {
                var level = ViewModel.AvailableLevels[i];
                var btnGo = new GameObject($"LevelButton_{level.LevelId}");
                btnGo.transform.SetParent(container, false);

                var rt = btnGo.AddComponent<RectTransform>();
                if (_startButton != null)
                {
                    // 定位在 StartButton 上方
                    var startRt = _startButton.GetComponent<RectTransform>();
                    rt.anchorMin = startRt.anchorMin;
                    rt.anchorMax = startRt.anchorMax;
                    rt.sizeDelta = startRt.sizeDelta;
                    rt.anchoredPosition = startRt.anchoredPosition + new Vector2(0, 50 + i * 45);
                }

                var image = btnGo.AddComponent<Image>();
                image.color = new Color(0.2f, 0.3f, 0.5f, 0.9f);

                var btn = btnGo.AddComponent<Button>();
                var colors = btn.colors;
                colors.highlightedColor = new Color(0.3f, 0.4f, 0.7f);
                colors.selectedColor = new Color(0.4f, 0.5f, 0.8f);
                btn.colors = colors;

                var textGo = new GameObject("Text");
                textGo.transform.SetParent(btnGo.transform, false);
                var textRt = textGo.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;

                var text = textGo.AddComponent<Text>();
                text.text = level.LevelName;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 16;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;

                var capturedLevel = level;
                var capturedBtn = btn;
                btn.onClick.AddListener(() =>
                {
                    ViewModel.SelectedLevel = capturedLevel;
                    _onLevelSelected?.Invoke(capturedLevel);
                    UpdateLevelButtonHighlight(capturedBtn);
                    Debug.Log($"[MainMenuView] 选择关卡: {capturedLevel.LevelName}");
                });

                _levelButtons.Add(btnGo);
            }

            // 默认选中第一个关卡
            if (ViewModel.AvailableLevels.Count > 0)
            {
                ViewModel.SelectedLevel = ViewModel.AvailableLevels[0];
                if (_levelButtons.Count > 0)
                    UpdateLevelButtonHighlight(_levelButtons[0].GetComponent<Button>());
            }
        }

        void UpdateLevelButtonHighlight(Button selected)
        {
            foreach (var go in _levelButtons)
            {
                var img = go.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(0.2f, 0.3f, 0.5f, 0.9f);
            }

            if (selected != null)
            {
                var img = selected.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(0.4f, 0.5f, 0.8f, 1f);
            }
            _selectedLevelButton = selected;
        }

        public void BindStartButton(Action callback) => _onStartClick = callback;
        public void BindExitButton(Action callback) => _onExitClick = callback;
        public void BindLevelSelected(Action<LevelConfig> callback) => _onLevelSelected = callback;

        public void SetUIElements(Text titleText, Button startButton, Button exitButton)
        {
            _titleText = titleText;
            _startButton = startButton;
            _exitButton = exitButton;
        }
    }
}
