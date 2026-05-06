using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TBS.Presentation.UI.Panels
{
    /// <summary>
    /// UnitPanel的View - 单位属性面板UI
    /// 挂载位置: Prefabs/UI/UnitPanel/UnitPanel.prefab 的根节点
    /// </summary>
    public class UnitPanelView : BaseView<UnitPanelViewModel>
    {
        #region UI组件引用

        [Header("基础信息")]
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _factionText;
        [SerializeField] private TextMeshProUGUI _tierText;
        [SerializeField] private TextMeshProUGUI _gradeText;
        [SerializeField] private Image _factionIcon;
        [SerializeField] private Image _stateIcon;

        [Header("状态显示")]
        [SerializeField] private TextMeshProUGUI _stateText;
        [SerializeField] private GameObject _stateAlertObj; // 状态警告图标（如动摇、溃散）

        [Header("兵力资源")]
        [SerializeField] private Slider _strengthSlider;
        [SerializeField] private TextMeshProUGUI _strengthText;
        [SerializeField] private Slider _moraleSlider;
        [SerializeField] private TextMeshProUGUI _moraleText;
        [SerializeField] private Slider _supplySlider;
        [SerializeField] private TextMeshProUGUI _supplyText;

        [Header("战斗属性")]
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _defenseText;
        [SerializeField] private TextMeshProUGUI _firepowerText;
        [SerializeField] private TextMeshProUGUI _effectiveAttackText;
        [SerializeField] private TextMeshProUGUI _effectiveDefenseText;

        [Header("工事信息")]
        [SerializeField] private GameObject _fortificationObj;
        [SerializeField] private TextMeshProUGUI _fortificationText;
        [SerializeField] private Slider _fortificationProgressSlider;

        [Header("操作按钮")]
        [SerializeField] private Button _closeButton;

        #endregion

        private UnitPanelPresenter _presenter;

        #region MVP组装

        protected override IPresenter CreatePresenter()
        {
            _presenter = new UnitPanelPresenter();
            return _presenter;
        }

        protected override void OnBind()
        {
            // 基础信息绑定
            Bind(nameof(ViewModel.UnitName), () => _unitNameText.text = ViewModel.UnitName);
            Bind(nameof(ViewModel.Faction), UpdateFactionDisplay);
            Bind(nameof(ViewModel.Tier), () => _tierText.text = GetTierDisplayName(ViewModel.Tier));
            Bind(nameof(ViewModel.Grade), () => _gradeText.text = GetGradeDisplayName(ViewModel.Grade));
            Bind(nameof(ViewModel.State), UpdateStateDisplay);

            // 资源属性绑定
            Bind(nameof(ViewModel.Strength), UpdateStrengthDisplay);
            Bind(nameof(ViewModel.StrengthPercent), UpdateStrengthDisplay);
            Bind(nameof(ViewModel.Morale), UpdateMoraleDisplay);
            Bind(nameof(ViewModel.MoralePercent), UpdateMoraleDisplay);
            Bind(nameof(ViewModel.Supply), UpdateSupplyDisplay);
            Bind(nameof(ViewModel.SupplyPercent), UpdateSupplyDisplay);

            // 战斗属性绑定
            Bind(nameof(ViewModel.AttackPower), () => _attackText.text = ViewModel.AttackPower.ToString());
            Bind(nameof(ViewModel.DefensePower), () => _defenseText.text = ViewModel.DefensePower.ToString());
            Bind(nameof(ViewModel.Firepower), () => _firepowerText.text = ViewModel.Firepower.ToString());
            Bind(nameof(ViewModel.EffectiveAttack), UpdateEffectiveAttackDisplay);
            Bind(nameof(ViewModel.EffectiveDefense), UpdateEffectiveDefenseDisplay);

            // 工事属性绑定
            Bind(nameof(ViewModel.FortificationLevel), UpdateFortificationDisplay);
            Bind(nameof(ViewModel.FortificationProgress), UpdateFortificationDisplay);

            // 按钮事件
            _closeButton.onClick.AddListener(() => _presenter.OnCloseClicked());

            // 订阅ViewModel关闭事件
            ViewModel.OnCloseRequested += OnCloseRequested;
        }

        #endregion

        #region 更新显示方法

        private void UpdateFactionDisplay()
        {
            _factionText.text = GetFactionDisplayName(ViewModel.Faction);
            // 可以在这里根据派系设置不同的颜色或图标
            if (_factionIcon != null)
            {
                // _factionIcon.sprite = GetFactionIcon(ViewModel.Faction);
            }
        }

        private void UpdateStateDisplay()
        {
            _stateText.text = GetStateDisplayName(ViewModel.State);

            // 根据状态设置警告显示
            bool showAlert = ViewModel.State == TBS.Unit.UnitState.Shaken ||
                           ViewModel.State == TBS.Unit.UnitState.Routed ||
                           ViewModel.State == TBS.Unit.UnitState.Suppressed;
            _stateAlertObj?.SetActive(showAlert);

            // 根据状态设置颜色
            if (_stateText != null)
            {
                _stateText.color = GetStateColor(ViewModel.State);
            }
        }

        private void UpdateStrengthDisplay()
        {
            _strengthSlider.value = ViewModel.StrengthPercent;
            _strengthText.text = $"{ViewModel.Strength}/{ViewModel.MaxStrength}";

            // 低兵力警告颜色
            _strengthText.color = ViewModel.Strength <= 2 ? Color.red : Color.white;
        }

        private void UpdateMoraleDisplay()
        {
            _moraleSlider.value = ViewModel.MoralePercent;
            _moraleText.text = $"{ViewModel.Morale}/100";

            // 根据士气设置颜色
            if (ViewModel.Morale <= 20)
                _moraleText.color = Color.red;
            else if (ViewModel.Morale <= 40)
                _moraleText.color = Color.yellow;
            else
                _moraleText.color = Color.white;
        }

        private void UpdateSupplyDisplay()
        {
            _supplySlider.value = ViewModel.SupplyPercent;
            _supplyText.text = $"{ViewModel.Supply}/{ViewModel.MaxSupply}";

            // 补给不足警告
            _supplyText.color = ViewModel.Supply == 0 ? Color.red : Color.white;
        }

        private void UpdateEffectiveAttackDisplay()
        {
            // 如果有效攻击力与基础不同，显示加成
            if (ViewModel.EffectiveAttack != ViewModel.AttackPower)
            {
                _effectiveAttackText.text = $"{ViewModel.AttackPower} → {ViewModel.EffectiveAttack}";
                _effectiveAttackText.color = ViewModel.EffectiveAttack > ViewModel.AttackPower ? Color.green : Color.red;
            }
            else
            {
                _effectiveAttackText.text = ViewModel.EffectiveAttack.ToString();
                _effectiveAttackText.color = Color.white;
            }
        }

        private void UpdateEffectiveDefenseDisplay()
        {
            // 如果有效防御力与基础不同，显示加成（工事加成）
            if (ViewModel.EffectiveDefense != ViewModel.DefensePower)
            {
                _effectiveDefenseText.text = $"{ViewModel.DefensePower} → {ViewModel.EffectiveDefense}";
                _effectiveDefenseText.color = ViewModel.EffectiveDefense > ViewModel.DefensePower ? Color.green : Color.white;
            }
            else
            {
                _effectiveDefenseText.text = ViewModel.EffectiveDefense.ToString();
                _effectiveDefenseText.color = Color.white;
            }
        }

        private void UpdateFortificationDisplay()
        {
            bool hasFort = ViewModel.FortificationLevel > 0;
            _fortificationObj.SetActive(hasFort);

            if (hasFort)
            {
                _fortificationText.text = $"Lv.{ViewModel.FortificationLevel}/4";
                _fortificationProgressSlider.value = ViewModel.FortificationProgress;
            }
        }

        #endregion

        #region 显示控制

        /// <summary>
        /// 显示指定单位信息（对外接口）
        /// </summary>
        public void ShowUnitInfo(TBS.Unit.Unit unit)
        {
            _presenter.ShowUnitInfo(unit);
        }

        private void OnCloseRequested()
        {
            // 通过UIManager隐藏面板
            UIManager.Instance.Hide<UnitPanelView>();
        }

        public override void OnBeforeDestroy()
        {
            // 清理事件订阅
            _closeButton.onClick.RemoveAllListeners();
            if (ViewModel != null)
            {
                ViewModel.OnCloseRequested -= OnCloseRequested;
            }
        }

        #endregion

        #region 辅助方法 - 本地化显示

        private string GetFactionDisplayName(TBS.Unit.Faction faction)
        {
            return faction switch
            {
                TBS.Unit.Faction.KMT => "国军",
                TBS.Unit.Faction.Japan => "日军",
                TBS.Unit.Faction.PLA => "八路军",
                _ => "未知"
            };
        }

        private string GetTierDisplayName(TBS.Unit.UnitTier tier)
        {
            return tier switch
            {
                TBS.Unit.UnitTier.HQ => "指挥部",
                TBS.Unit.UnitTier.Regiment => "团级单位",
                _ => "未知"
            };
        }

        private string GetGradeDisplayName(TBS.Unit.UnitGrade grade)
        {
            return grade switch
            {
                TBS.Unit.UnitGrade.KMT_Elite => "国军精锐",
                TBS.Unit.UnitGrade.KMT_Regular => "国军普通",
                TBS.Unit.UnitGrade.PLA_Elite => "八路军精锐",
                TBS.Unit.UnitGrade.PLA_Regular => "八路军普通",
                TBS.Unit.UnitGrade.Japan => "日军",
                _ => "未知"
            };
        }

        private string GetStateDisplayName(TBS.Unit.UnitState state)
        {
            return state switch
            {
                TBS.Unit.UnitState.Inspired => "高昂",
                TBS.Unit.UnitState.Normal => "正常",
                TBS.Unit.UnitState.Suppressed => "被压制",
                TBS.Unit.UnitState.Shaken => "动摇",
                TBS.Unit.UnitState.Routed => "溃散",
                TBS.Unit.UnitState.Recuperating => "整补中",
                _ => "未知"
            };
        }

        private Color GetStateColor(TBS.Unit.UnitState state)
        {
            return state switch
            {
                TBS.Unit.UnitState.Inspired => new Color(0.2f, 1f, 0.2f), // 绿色
                TBS.Unit.UnitState.Normal => Color.white,
                TBS.Unit.UnitState.Suppressed => new Color(1f, 0.5f, 0f), // 橙色
                TBS.Unit.UnitState.Shaken => Color.yellow,
                TBS.Unit.UnitState.Routed => Color.red,
                TBS.Unit.UnitState.Recuperating => Color.cyan,
                _ => Color.gray
            };
        }

        #endregion
    }
}
