using System.Collections.Generic;
using TBS.Unit;
using TBS.Presentation.UI;

namespace TBS.Presentation.UI.Panels.SpawnPanel
{
    public class SpawnPanelViewModel : ViewModelBase
    {
        private List<SpawnUnitEntry> _unitList;

        public List<SpawnUnitEntry> UnitList
        {
            get => _unitList;
        }

        public SpawnUnitEntry SelectedEntry
        {
            get => GetProperty<SpawnUnitEntry>();
            set => SetProperty(value);
        }

        public string StatusText
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public SpawnPanelViewModel()
        {
            _unitList = InitializeUnitList();
            SelectedEntry = null;
            StatusText = "请选择单位";
        }

        private List<SpawnUnitEntry> InitializeUnitList()
        {
            var list = new List<SpawnUnitEntry>
            {
                // 国军一个师的编制：1个师部 + 3个团
                new SpawnUnitEntry(
                    "88师 师部",
                    new UnitRuntimeParams
                    {
                        UnitId = "88D_HQ",
                        DisplayName = "88师 师部",
                        Faction = Faction.KMT,
                        Tier = UnitTier.HQ,
                        Grade = UnitGrade.KMT_Elite,
                        AttackPower = 8,
                        DefensePower = 7,
                        MoveSpeedKmPerDay = 20f,
                        Firepower = 5,
                        InitialStrength = 5,
                        InitialMorale = 85,
                        InitialSupply = 4,
                        ShakenMoraleThreshold = 40,
                        RoutedMoraleThreshold = 20,
                        RoutedStrengthThreshold = 1,
                        MoraleRecoveryPerHour = 2.5f,
                        RecuperationSpeedModifier = 1.0f
                    }
                ),
                new SpawnUnitEntry(
                    "88师 262团",
                    new UnitRuntimeParams
                    {
                        UnitId = "88D_262R",
                        DisplayName = "88师 262团",
                        Faction = Faction.KMT,
                        Tier = UnitTier.Regiment,
                        Grade = UnitGrade.KMT_Elite,
                        AttackPower = 7,
                        DefensePower = 6,
                        MoveSpeedKmPerDay = 25f,
                        Firepower = 4,
                        InitialStrength = 5,
                        InitialMorale = 80,
                        InitialSupply = 4,
                        ShakenMoraleThreshold = 40,
                        RoutedMoraleThreshold = 20,
                        RoutedStrengthThreshold = 1,
                        MoraleRecoveryPerHour = 2.5f,
                        RecuperationSpeedModifier = 1.0f
                    }
                ),
                new SpawnUnitEntry(
                    "88师 263团",
                    new UnitRuntimeParams
                    {
                        UnitId = "88D_263R",
                        DisplayName = "88师 263团",
                        Faction = Faction.KMT,
                        Tier = UnitTier.Regiment,
                        Grade = UnitGrade.KMT_Regular,
                        AttackPower = 6,
                        DefensePower = 5,
                        MoveSpeedKmPerDay = 25f,
                        Firepower = 3,
                        InitialStrength = 5,
                        InitialMorale = 75,
                        InitialSupply = 3,
                        ShakenMoraleThreshold = 35,
                        RoutedMoraleThreshold = 15,
                        RoutedStrengthThreshold = 1,
                        MoraleRecoveryPerHour = 2.0f,
                        RecuperationSpeedModifier = 1.0f
                    }
                ),
                new SpawnUnitEntry(
                    "88师 264团",
                    new UnitRuntimeParams
                    {
                        UnitId = "88D_264R",
                        DisplayName = "88师 264团",
                        Faction = Faction.KMT,
                        Tier = UnitTier.Regiment,
                        Grade = UnitGrade.KMT_Regular,
                        AttackPower = 6,
                        DefensePower = 5,
                        MoveSpeedKmPerDay = 25f,
                        Firepower = 3,
                        InitialStrength = 5,
                        InitialMorale = 75,
                        InitialSupply = 3,
                        ShakenMoraleThreshold = 35,
                        RoutedMoraleThreshold = 15,
                        RoutedStrengthThreshold = 1,
                        MoraleRecoveryPerHour = 2.0f,
                        RecuperationSpeedModifier = 1.0f
                    }
                )
            };
            return list;
        }

        public void SelectUnit(SpawnUnitEntry entry)
        {
            SelectedEntry = entry;
            StatusText = entry != null ? $"已选择：{entry.DisplayName}  (点击地格生成)" : "请选择单位";
        }

        public void RemoveSelectedUnit()
        {
            if (SelectedEntry != null)
            {
                _unitList.Remove(SelectedEntry);
                RaisePropertyChanged(nameof(UnitList));
                ClearSelection();
            }
        }

        public void ClearSelection()
        {
            SelectedEntry = null;
            StatusText = "请选择单位";
        }
    }
}
