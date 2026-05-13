using UnityEngine;
using UnityEngine.UI;
using TBS.Core;
using TBS.Presentation.UI;

namespace TBS.Presentation.UI.Panels.BattleHUD
{
    public class BattleHUDView : BaseView<BattleHUDViewModel>
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _dayText;
        [SerializeField] private Text _timeText;
        [SerializeField] private Button _menuButton;
        private BattleHUDPresenter _presenter;
        private GameTimeSystem _gameTimeSystem;

        protected override IPresenter CreatePresenter()
        {
            _presenter = new BattleHUDPresenter();
            return _presenter;
        }

        protected override void OnBind()
        {
            Debug.Log("[BattleHUDView] OnBind - 开始绑定UI");

            // 获取时间系统
            _gameTimeSystem = GameTimeSystem.Instance;

            // 自动查找子物体
            if (_titleText == null)
            {
                _titleText = transform.Find("TopBar/TitleText")?.GetComponent<Text>();
                Debug.Log($"[BattleHUDView] 自动查找 TitleText: {(_titleText != null ? "成功" : "失败")}");
            }

            if (_dayText == null)
            {
                _dayText = transform.Find("TopBar/DayText")?.GetComponent<Text>();
                Debug.Log($"[BattleHUDView] 自动查找 DayText: {(_dayText != null ? "成功" : "失败")}");
            }

            if (_timeText == null)
            {
                _timeText = transform.Find("TopBar/TimeText")?.GetComponent<Text>();
                Debug.Log($"[BattleHUDView] 自动查找 TimeText: {(_timeText != null ? "成功" : "失败")}");
            }

            if (_menuButton == null)
            {
                _menuButton = transform.Find("TopBar/MenuButton")?.GetComponent<Button>();
                Debug.Log($"[BattleHUDView] 自动查找 MenuButton: {(_menuButton != null ? "成功" : "失败")}");
            }

            // 绑定标题
            if (_titleText != null)
            {
                _titleText.text = ViewModel.GameTitle;
                Bind(nameof(ViewModel.GameTitle), () =>
                {
                    if (_titleText != null)
                        _titleText.text = ViewModel.GameTitle;
                });
                Debug.Log("[BattleHUDView] GameTitle 绑定成功");
            }

            // 绑定日期
            if (_dayText != null)
            {
                _dayText.text = ViewModel.DayText;
                Bind(nameof(ViewModel.DayText), () =>
                {
                    if (_dayText != null)
                        _dayText.text = ViewModel.DayText;
                });
                Debug.Log("[BattleHUDView] DayText 绑定成功");
            }

            // 绑定时间
            if (_timeText != null)
            {
                _timeText.text = ViewModel.TimeText;
                Bind(nameof(ViewModel.TimeText), () =>
                {
                    if (_timeText != null)
                        _timeText.text = ViewModel.TimeText;
                });
                Debug.Log("[BattleHUDView] TimeText 绑定成功");
            }

            // 绑定菜单按钮
            if (_menuButton != null)
            {
                _menuButton.onClick.AddListener(() =>
                {
                    Debug.Log("[BattleHUDView] 菜单按钮被点击");
                    _presenter?.OnMenuClicked();
                });
                Debug.Log("[BattleHUDView] MenuButton 监听器已添加");
            }
            else
                Debug.LogError("[BattleHUDView] 找不到 MenuButton");

            // 设置单位信息卡管理器
            var cardsContainerTransform = transform.Find("UnitInfoCardsContainer");
            if (cardsContainerTransform != null)
            {
                var canvasGO = GetComponent<Canvas>();
                var mgr = gameObject.AddComponent<TBS.Presentation.UI.Panels.UnitInfoCard.UnitInfoCardManager>();
                mgr.Setup(canvasGO, cardsContainerTransform.GetComponent<RectTransform>());
                Debug.Log("[BattleHUDView] UnitInfoCardManager 已配置");
            }
            else
            {
                Debug.LogWarning("[BattleHUDView] 找不到 UnitInfoCardsContainer");
            }

            // 设置事件点卡片管理器（复用同一容器）
            var eventContainer = cardsContainerTransform ?? transform;
            var eventMgr = gameObject.AddComponent<TBS.Presentation.UI.Panels.EventPointCard.EventPointCardManager>();
            eventMgr.Setup(GetComponent<Canvas>(), eventContainer.GetComponent<RectTransform>());
            Debug.Log("[BattleHUDView] EventPointCardManager 已配置");

            Debug.Log("[BattleHUDView] OnBind - 绑定完成");
        }

        void Update()
        {
            if (_gameTimeSystem != null && IsVisible)
            {
                ViewModel.UpdateTime(_gameTimeSystem.GameHours);
            }
        }

        public override void OnBeforeDestroy()
        {
            if (_menuButton != null)
                _menuButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 供编辑器生成脚本调用，关联所有UI元素
        /// </summary>
        public void SetUIElements(Text titleText, Text dayText, Text timeText, Button menuButton)
        {
            _titleText = titleText;
            _dayText = dayText;
            _timeText = timeText;
            _menuButton = menuButton;
        }
    }
}
