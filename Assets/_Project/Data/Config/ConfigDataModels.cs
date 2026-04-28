using System;

namespace TBS.Data.Config
{
    // ── 兵牌数值 ──────────────────────────────────────────────────────

    [Serializable]
    public class UnitTokenData
    {
        public int    id;
        public string name;
        public string grade;          // 国军·精锐 / 国军·正规 / 国军·杂牌 / 日军 / 八路军·精锐 / 八路军·普通
        public int    attack;
        public int    defense;
        public float  moveSpeed;      // km/天
        public int    firepower;
        public int    initMorale;
        public int    initSupply;
        public int    initStrength;
    }

    [Serializable]
    public class UnitTokenCollection
    {
        public UnitTokenData[] units;
    }

    // ── 作战意志 ──────────────────────────────────────────────────────

    [Serializable]
    public class CombatWillData
    {
        public string grade;
        public int    shakenThresholdMorale;
        public string routedCondition;          // 原始条件字符串，运行时由 CombatWillEvaluator 解析
        public float  moraleRecoveryPerHour;
        public float  replenishSpeedMod;
    }

    [Serializable]
    public class CombatWillCollection
    {
        public CombatWillData[] combatWills;
    }

    // ── 天气 ──────────────────────────────────────────────────────────

    [Serializable]
    public class WeatherData
    {
        public string type;                     // Sunny / Rainy
        public string displayName;
        public float  moveSpeedMod;
        public bool   airSupportAvailable;
        public float  artilleryAccuracyMod;
        public float  fortBuildSpeedMod;
        public int    moralePerDay;
    }

    [Serializable]
    public class WeatherCollection
    {
        public WeatherData[] weatherTypes;
    }

    // ── 工事等级 ──────────────────────────────────────────────────────

    [Serializable]
    public class FortificationData
    {
        public int    level;
        public string name;
        public int    defenseBonus;
        public float  buildTimeDays;
        public int    suppressionHitsRequired;
    }

    [Serializable]
    public class FortificationCollection
    {
        public FortificationData[] fortificationLevels;
    }

    // ── 状态效果 ──────────────────────────────────────────────────────

    [Serializable]
    public class UnitStateData
    {
        public string state;                    // Inspired / Normal / Suppressed / Shaken / Routed / Recuperating
        public string displayName;
        public int    attackMod;
        public int    defenseMod;
        public float  moveSpeedMod;
        public bool   canAttack;
        public bool   canCommand;
        public bool   canReplenish;
    }

    [Serializable]
    public class UnitStateCollection
    {
        public UnitStateData[] unitStates;
    }
}
