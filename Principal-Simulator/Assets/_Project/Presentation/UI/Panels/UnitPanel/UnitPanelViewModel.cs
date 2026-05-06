// 注意：使用完整命名空间避免与TBS.Unit命名空间冲突
namespace TBS.Presentation.UI.Panels
{
    /// <summary>
    /// UnitPanel的ViewModel - 展示单位属性
    /// </summary>
    public class UnitPanelViewModel : ViewModelBase
    {
        #region 基础属性

        private string _unitName;
        private TBS.Unit.Faction _faction;
        private TBS.Unit.UnitTier _tier;
        private TBS.Unit.UnitGrade _grade;
        private TBS.Unit.UnitState _state;

        public string UnitName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public TBS.Unit.Faction Faction
        {
            get => GetProperty<TBS.Unit.Faction>();
            set => SetProperty(value);
        }

        public TBS.Unit.UnitTier Tier
        {
            get => GetProperty<TBS.Unit.UnitTier>();
            set => SetProperty(value);
        }

        public TBS.Unit.UnitGrade Grade
        {
            get => GetProperty<TBS.Unit.UnitGrade>();
            set => SetProperty(value);
        }

        public TBS.Unit.UnitState State
        {
            get => GetProperty<TBS.Unit.UnitState>();
            set => SetProperty(value);
        }

        #endregion

        #region 战斗属性

        private int _attackPower;
        private int _defensePower;
        private int _firepower;
        private int _effectiveAttack;
        private int _effectiveDefense;

        public int AttackPower
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public int DefensePower
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public int Firepower
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public int EffectiveAttack
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public int EffectiveDefense
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        #endregion

        #region 资源属性

        private int _strength;
        private int _maxStrength;
        private int _morale;
        private int _supply;
        private int _maxSupply;

        public int Strength
        {
            get => GetProperty<int>();
            set
            {
                if (SetProperty(value))
                {
                    RaisePropertyChanged(nameof(StrengthPercent));
                }
            }
        }

        public int MaxStrength
        {
            get => GetProperty<int>();
            set
            {
                if (SetProperty(value))
                {
                    RaisePropertyChanged(nameof(StrengthPercent));
                }
            }
        }

        public int Morale
        {
            get => GetProperty<int>();
            set
            {
                if (SetProperty(value))
                {
                    RaisePropertyChanged(nameof(MoralePercent));
                }
            }
        }

        public int Supply
        {
            get => GetProperty<int>();
            set
            {
                if (SetProperty(value))
                {
                    RaisePropertyChanged(nameof(SupplyPercent));
                }
            }
        }

        public int MaxSupply
        {
            get => GetProperty<int>();
            set
            {
                if (SetProperty(value))
                {
                    RaisePropertyChanged(nameof(SupplyPercent));
                }
            }
        }

        // 百分比属性（用于进度条）
        public float StrengthPercent => MaxStrength > 0 ? (float)Strength / MaxStrength : 0;
        public float MoralePercent => Morale / 100f;
        public float SupplyPercent => MaxSupply > 0 ? (float)Supply / MaxSupply : 0;

        #endregion

        #region 工事属性

        private int _fortificationLevel;
        private float _fortificationProgress;

        public int FortificationLevel
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public float FortificationProgress
        {
            get => GetProperty<float>();
            set => SetProperty(value);
        }

        public bool HasFortification => FortificationLevel > 0;

        #endregion

        #region 当前单位引用

        private TBS.Unit.Unit _currentUnit;

        public TBS.Unit.Unit CurrentUnit
        {
            get => _currentUnit;
            set
            {
                _currentUnit = value;
                if (value != null)
                {
                    UpdateFromUnit(value);
                }
            }
        }

        #endregion

        #region 数据更新

        /// <summary>
        /// 从Unit对象更新所有属性
        /// </summary>
        public void UpdateFromUnit(TBS.Unit.Unit unit)
        {
            if (unit == null) return;

            // 基础属性
            UnitName = unit.DisplayName;
            Faction = unit.Faction;
            Tier = unit.Tier;
            Grade = unit.Grade;
            State = unit.State;

            // 战斗属性
            AttackPower = unit.AttackPower;
            DefensePower = unit.DefensePower;
            Firepower = unit.Firepower;
            EffectiveAttack = unit.EffectiveAttack;
            EffectiveDefense = unit.EffectiveDefense;

            // 资源属性
            MaxStrength = 5; // 最大兵力固定为5
            Strength = unit.Strength;
            Morale = unit.Morale;
            MaxSupply = 5; // 最大补给固定为5
            Supply = unit.Supply;

            // 工事属性
            FortificationLevel = unit.FortificationLevel;
            FortificationProgress = unit.FortificationProgress;

            // 保存引用
            _currentUnit = unit;
        }

        /// <summary>
        /// 更新动态属性（用于游戏过程中刷新）
        /// </summary>
        public void UpdateDynamicProperties()
        {
            if (_currentUnit == null) return;

            State = _currentUnit.State;
            Strength = _currentUnit.Strength;
            Morale = _currentUnit.Morale;
            Supply = _currentUnit.Supply;
            FortificationLevel = _currentUnit.FortificationLevel;
            FortificationProgress = _currentUnit.FortificationProgress;
            EffectiveAttack = _currentUnit.EffectiveAttack;
            EffectiveDefense = _currentUnit.EffectiveDefense;
        }

        #endregion

        #region 命令

        /// <summary>
        /// 关闭面板命令
        /// </summary>
        public void ClosePanel()
        {
            OnCloseRequested?.Invoke();
        }

        public event System.Action OnCloseRequested;

        #endregion
    }
}
