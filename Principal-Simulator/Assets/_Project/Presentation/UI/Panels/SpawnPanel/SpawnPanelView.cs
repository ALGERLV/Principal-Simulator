using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TBS.Presentation.UI;

namespace TBS.Presentation.UI.Panels.SpawnPanel
{
    public class SpawnPanelView : BaseView<SpawnPanelViewModel>
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private Text _statusText;

        private SpawnPanelPresenter _presenter;
        private List<Button> _unitButtons = new List<Button>();
        private int _selectedButtonIndex = -1;

        protected override IPresenter CreatePresenter()
        {
            _presenter = new SpawnPanelPresenter();
            return _presenter;
        }

        protected override void OnBind()
        {
            Debug.Log("[SpawnPanelView] OnBind - 开始绑定UI");

            // 自动查找子物体
            if (_listContainer == null)
            {
                _listContainer = transform.Find("Panel/ListContainer");
                Debug.Log($"[SpawnPanelView] 自动查找 ListContainer: {(_listContainer != null ? "成功" : "失败")}");
            }

            if (_statusText == null)
            {
                _statusText = transform.Find("Panel/StatusText")?.GetComponent<Text>();
                Debug.Log($"[SpawnPanelView] 自动查找 StatusText: {(_statusText != null ? "成功" : "失败")}");
            }

            // 绑定状态文本
            if (_statusText != null)
            {
                _statusText.text = ViewModel.StatusText;
                Bind(nameof(ViewModel.StatusText), () =>
                {
                    if (_statusText != null)
                        _statusText.text = ViewModel.StatusText;
                });
                Debug.Log("[SpawnPanelView] StatusText 绑定成功");
            }

            // 绑定选中状态
            Bind(nameof(ViewModel.SelectedEntry), () =>
            {
                UpdateSelectedButtonHighlight();
            });

            // 绑定列表变化（单位被移除时重新创建按钮）
            Bind(nameof(ViewModel.UnitList), () =>
            {
                CreateUnitListButtons();
                UpdateSelectedButtonHighlight();
            });

            // 创建单位列表按钮
            CreateUnitListButtons();

            Debug.Log("[SpawnPanelView] OnBind - 绑定完成");
        }

        private void CreateUnitListButtons()
        {
            if (_listContainer == null)
            {
                Debug.LogError("[SpawnPanelView] ListContainer为空，无法创建按钮");
                return;
            }

            // 清空现有按钮
            foreach (var btn in _unitButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }
            _unitButtons.Clear();

            // 为每个单位创建按钮
            var units = ViewModel.UnitList;
            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var btnGO = new GameObject($"UnitItem_{i}");
                btnGO.transform.SetParent(_listContainer, false);

                var btn = btnGO.AddComponent<Button>();
                var image = btnGO.AddComponent<Image>();
                image.color = new Color(0.3f, 0.3f, 0.3f, 1f); // 默认深灰
                btn.targetGraphic = image;

                // 按钮状态颜色
                var colors = btn.colors;
                colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                colors.pressedColor = new Color(0.2f, 0.4f, 0.7f, 1f);
                btn.colors = colors;

                var btnRect = btnGO.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(180, 40);
                btnRect.anchoredPosition = new Vector2(0, -i * 50);

                // 按钮文本
                var textGO = new GameObject("Text");
                textGO.transform.SetParent(btnGO.transform, false);
                var text = textGO.AddComponent<Text>();
                text.text = unit.DisplayName;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 14;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;
                var textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                // 点击事件
                int index = i;
                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"[SpawnPanelView] 单位 {unit.DisplayName} 被点击");
                    ViewModel.SelectUnit(unit);
                    _selectedButtonIndex = index;
                });

                _unitButtons.Add(btn);
            }

            Debug.Log($"[SpawnPanelView] 已创建 {_unitButtons.Count} 个单位按钮");
        }

        private void UpdateSelectedButtonHighlight()
        {
            // 清空所有按钮高亮
            for (int i = 0; i < _unitButtons.Count; i++)
            {
                if (_unitButtons[i] == null) continue;

                var image = _unitButtons[i].GetComponent<Image>();
                if (i == _selectedButtonIndex && ViewModel.SelectedEntry != null)
                {
                    image.color = new Color(0.2f, 0.5f, 0.9f, 1f); // 蓝色高亮
                }
                else
                {
                    image.color = new Color(0.3f, 0.3f, 0.3f, 1f); // 恢复默认
                }
            }
        }

        public override void OnBeforeDestroy()
        {
            foreach (var btn in _unitButtons)
            {
                if (btn != null)
                    btn.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 供编辑器生成脚本调用
        /// </summary>
        public void SetUIElements(Transform listContainer, Text statusText)
        {
            _listContainer = listContainer;
            _statusText = statusText;
        }
    }
}
