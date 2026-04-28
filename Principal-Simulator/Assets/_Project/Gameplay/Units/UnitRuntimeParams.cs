namespace TBS.Unit
{
    /// <summary>
    /// 运行时单位参数 — 替代 ScriptableObject，用于程序化/测试场景
    /// </summary>
    public class UnitRuntimeParams
    {
        public string    UnitId                  = "test_unit";
        public string    DisplayName             = "测试单位";
        public Faction   Faction                 = Faction.KMT;
        public UnitTier  Tier                    = UnitTier.Regiment;
        public UnitGrade Grade                   = UnitGrade.KMT_Regular;

        public int   AttackPower                 = 7;
        public int   DefensePower                = 6;
        public float MoveSpeedKmPerDay           = 25f;
        public int   Firepower                   = 4;

        public int InitialStrength               = 5;
        public int InitialMorale                 = 80;
        public int InitialSupply                 = 4;

        public int   ShakenMoraleThreshold       = 40;
        public int   RoutedMoraleThreshold       = 20;
        public int   RoutedStrengthThreshold     = 1;
        public float MoraleRecoveryPerHour       = 2.5f;
        public float RecuperationSpeedModifier   = 1.0f;
    }
}
