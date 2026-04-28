namespace TBS.Unit
{
    public enum Faction
    {
        KMT,   // 国军
        Japan, // 日军
        PLA    // 八路军
    }

    public enum UnitTier
    {
        HQ,      // 师部 / 师团司令部
        Regiment // 团 / 联队
    }

    public enum UnitGrade
    {
        KMT_Elite,   // 国军精锐（德械师）
        KMT_Regular, // 国军正规
        KMT_Militia, // 国军杂牌
        Japan,       // 日军
        PLA_Elite,   // 八路军精锐
        PLA_Regular  // 八路军普通
    }

    public enum UnitState
    {
        Inspired,     // 高昂：士气≥80 且 兵力≥4
        Normal,       // 正常
        Suppressed,   // 压制：炮击/密集射击触发，约2-4小时
        Shaken,       // 动摇：攻防-1，速-20%，概率自动后退
        Routed,       // 溃散：强制撤退，丧失指令响应
        Recuperating  // 后撤整补：自动恢复兵力与士气
    }

    public enum WeatherType
    {
        Clear, // 晴天
        Rain   // 下雨
    }

    public enum TerrainType
    {
        Plain,
        Forest,
        Mountain,
        River,
        Road,
        Swamp,
        City,
        Village
    }
}
