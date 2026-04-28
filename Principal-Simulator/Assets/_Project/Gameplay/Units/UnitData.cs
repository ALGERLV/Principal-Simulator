using UnityEngine;

namespace TBS.Unit
{
    /// <summary>
    /// 兵牌静态数据 — ScriptableObject，对应文档第4节数值表
    /// </summary>
    [CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Unit Data", order = 2)]
    public class UnitData : ScriptableObject
    {
        [Header("基础识别")]
        [SerializeField] private string unitId;
        [SerializeField] private string displayName;
        [SerializeField] private Faction faction;
        [SerializeField] private UnitTier tier;
        [SerializeField] private UnitGrade grade;

        [Header("战斗属性（文档第4节）")]
        [SerializeField, Range(0, 10)] private int attackPower;
        [SerializeField, Range(0, 10)] private int defensePower;
        [SerializeField] private float moveSpeedKmPerDay;
        [SerializeField, Range(0, 10)] private int firepower;

        [Header("初始资源")]
        [SerializeField, Range(0, 5)] private int initialStrength = 5;
        [SerializeField, Range(0, 100)] private int initialMorale;
        [SerializeField, Range(0, 5)] private int initialSupply;

        [Header("作战意志（文档§5.1）")]
        [SerializeField, Range(0, 100)] private int shakenMoraleThreshold;
        [Tooltip("士气归零时直接崩溃")]
        [SerializeField, Range(0, 100)] private int routedMoraleThreshold;
        [Tooltip("兵力≤此值触发动摇/崩溃判定")]
        [SerializeField, Range(0, 5)] private int routedStrengthThreshold;
        [Tooltip("非战斗休整时士气恢复速度（/小时）")]
        [SerializeField] private float moraleRecoveryPerHour;
        [Tooltip("整补速度修正（1.0=标准速度）")]
        [SerializeField] private float recuperationSpeedModifier = 1f;

        [Header("指挥（仅HQ有效）")]
        [SerializeField] private int commandRadius;

        public string UnitId => unitId;
        public string DisplayName => displayName;
        public Faction Faction => faction;
        public UnitTier Tier => tier;
        public UnitGrade Grade => grade;

        public int AttackPower => attackPower;
        public int DefensePower => defensePower;
        public float MoveSpeedKmPerDay => moveSpeedKmPerDay;
        public int Firepower => firepower;

        public int InitialStrength => initialStrength;
        public int InitialMorale => initialMorale;
        public int InitialSupply => initialSupply;

        public int ShakenMoraleThreshold => shakenMoraleThreshold;
        public int RoutedMoraleThreshold => routedMoraleThreshold;
        public int RoutedStrengthThreshold => routedStrengthThreshold;
        public float MoraleRecoveryPerHour => moraleRecoveryPerHour;
        public float RecuperationSpeedModifier => recuperationSpeedModifier;
        public int CommandRadius => commandRadius;

#if UNITY_EDITOR
        // 快捷创建：国军精锐师部
        [UnityEditor.MenuItem("Assets/Create/Game/Unit Presets/KMT Elite HQ")]
        static void CreateKMTEliteHQ()
        {
            var d = CreateInstance<UnitData>();
            d.unitId = "kmt_elite_hq";
            d.displayName = "国军精锐师部";
            d.faction = Faction.KMT;
            d.tier = UnitTier.HQ;
            d.grade = UnitGrade.KMT_Elite;
            d.attackPower = 2; d.defensePower = 4;
            d.moveSpeedKmPerDay = 20; d.firepower = 6;
            d.initialStrength = 5; d.initialMorale = 80; d.initialSupply = 5;
            d.shakenMoraleThreshold = 40; d.routedMoraleThreshold = 30;
            d.routedStrengthThreshold = 1; d.moraleRecoveryPerHour = 2.5f;
            d.recuperationSpeedModifier = 1.0f; d.commandRadius = 3;
            SaveAsset(d, "KMT_Elite_HQ");
        }

        [UnityEditor.MenuItem("Assets/Create/Game/Unit Presets/KMT Elite Regiment")]
        static void CreateKMTEliteReg()
        {
            var d = CreateInstance<UnitData>();
            d.unitId = "kmt_elite_regiment";
            d.displayName = "国军精锐步兵团";
            d.faction = Faction.KMT;
            d.tier = UnitTier.Regiment;
            d.grade = UnitGrade.KMT_Elite;
            d.attackPower = 7; d.defensePower = 6;
            d.moveSpeedKmPerDay = 25; d.firepower = 4;
            d.initialStrength = 5; d.initialMorale = 80; d.initialSupply = 4;
            d.shakenMoraleThreshold = 40; d.routedMoraleThreshold = 30;
            d.routedStrengthThreshold = 1; d.moraleRecoveryPerHour = 2.5f;
            d.recuperationSpeedModifier = 1.0f;
            SaveAsset(d, "KMT_Elite_Regiment");
        }

        [UnityEditor.MenuItem("Assets/Create/Game/Unit Presets/Japan HQ")]
        static void CreateJapanHQ()
        {
            var d = CreateInstance<UnitData>();
            d.unitId = "japan_hq";
            d.displayName = "日军师团司令部";
            d.faction = Faction.Japan;
            d.tier = UnitTier.HQ;
            d.grade = UnitGrade.Japan;
            d.attackPower = 3; d.defensePower = 4;
            d.moveSpeedKmPerDay = 20; d.firepower = 8;
            d.initialStrength = 5; d.initialMorale = 90; d.initialSupply = 5;
            d.shakenMoraleThreshold = 35; d.routedMoraleThreshold = 20;
            d.routedStrengthThreshold = 1; d.moraleRecoveryPerHour = 3.0f;
            d.recuperationSpeedModifier = 1.2f; d.commandRadius = 4;
            SaveAsset(d, "Japan_HQ");
        }

        [UnityEditor.MenuItem("Assets/Create/Game/Unit Presets/Japan Elite Regiment")]
        static void CreateJapanEliteReg()
        {
            var d = CreateInstance<UnitData>();
            d.unitId = "japan_elite_regiment";
            d.displayName = "日军精锐联队（第3师团）";
            d.faction = Faction.Japan;
            d.tier = UnitTier.Regiment;
            d.grade = UnitGrade.Japan;
            d.attackPower = 8; d.defensePower = 7;
            d.moveSpeedKmPerDay = 30; d.firepower = 5;
            d.initialStrength = 5; d.initialMorale = 90; d.initialSupply = 5;
            d.shakenMoraleThreshold = 35; d.routedMoraleThreshold = 20;
            d.routedStrengthThreshold = 1; d.moraleRecoveryPerHour = 3.0f;
            d.recuperationSpeedModifier = 1.2f;
            SaveAsset(d, "Japan_Elite_Regiment");
        }

        [UnityEditor.MenuItem("Assets/Create/Game/Unit Presets/PLA Elite HQ")]
        static void CreatePLAEliteHQ()
        {
            var d = CreateInstance<UnitData>();
            d.unitId = "pla_elite_hq";
            d.displayName = "八路军师���";
            d.faction = Faction.PLA;
            d.tier = UnitTier.HQ;
            d.grade = UnitGrade.PLA_Elite;
            d.attackPower = 2; d.defensePower = 3;
            d.moveSpeedKmPerDay = 35; d.firepower = 2;
            d.initialStrength = 5; d.initialMorale = 75; d.initialSupply = 3;
            d.shakenMoraleThreshold = 40; d.routedMoraleThreshold = 10;
            d.routedStrengthThreshold = 1; d.moraleRecoveryPerHour = 2.0f;
            d.recuperationSpeedModifier = 1.0f; d.commandRadius = 3;
            SaveAsset(d, "PLA_Elite_HQ");
        }

        [UnityEditor.MenuItem("Assets/Create/Game/Unit Presets/PLA Elite Regiment")]
        static void CreatePLAEliteReg()
        {
            var d = CreateInstance<UnitData>();
            d.unitId = "pla_elite_regiment";
            d.displayName = "八路军精锐团（685团）";
            d.faction = Faction.PLA;
            d.tier = UnitTier.Regiment;
            d.grade = UnitGrade.PLA_Elite;
            d.attackPower = 5; d.defensePower = 4;
            d.moveSpeedKmPerDay = 40; d.firepower = 2;
            d.initialStrength = 5; d.initialMorale = 75; d.initialSupply = 3;
            d.shakenMoraleThreshold = 40; d.routedMoraleThreshold = 10;
            d.routedStrengthThreshold = 1; d.moraleRecoveryPerHour = 2.0f;
            d.recuperationSpeedModifier = 1.0f;
            SaveAsset(d, "PLA_Elite_Regiment");
        }

        static void SaveAsset(UnitData d, string name)
        {
            string path = $"Assets/Resources/ScriptableObjects/Units/{name}.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            UnityEditor.AssetDatabase.CreateAsset(d, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.Selection.activeObject = d;
        }
#endif
    }
}
