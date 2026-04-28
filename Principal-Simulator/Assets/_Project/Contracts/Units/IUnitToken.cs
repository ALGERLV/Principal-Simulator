using TBS.Map.Tools;

namespace TBS.Unit
{
    /// <summary>
    /// 兵牌数据契约
    /// </summary>
    public interface IUnitToken
    {
        string UnitId { get; }
        string DisplayName { get; }
        Faction Faction { get; }
        UnitTier Tier { get; }
        UnitGrade Grade { get; }

        int AttackPower { get; }
        int DefensePower { get; }
        float MoveSpeedKmPerDay { get; }
        int Firepower { get; }

        int Strength { get; set; }    // 0-5
        int Morale { get; set; }      // 0-100
        int Supply { get; set; }      // 0-5

        UnitState State { get; }

        int FortificationLevel { get; }
        float FortificationProgress { get; }

        HexCoord Position { get; }
    }

    /// <summary>
    /// 状态处理器契约
    /// </summary>
    public interface IStateHandler
    {
        UnitState State { get; }
        void OnEnter(IUnitToken unit);
        void OnExit(IUnitToken unit);
        UnitState Evaluate(IUnitToken unit);
    }

    /// <summary>
    /// 属性计算器契约
    /// </summary>
    public interface IStatCalculator
    {
        int GetEffectiveAttack(IUnitToken unit);
        int GetEffectiveDefense(IUnitToken unit, TerrainType terrain);
        float GetEffectiveMoveSpeed(IUnitToken unit, TerrainType terrain, WeatherType weather, bool isNight);
    }

    /// <summary>
    /// 工事构筑器契约
    /// </summary>
    public interface IFortificationBuilder
    {
        void Tick(IUnitToken unit, float deltaHours, WeatherType weather);
        int GetCurrentLevel(IUnitToken unit);
        void Damage(IUnitToken unit, int artilleryGrade);
    }

    /// <summary>
    /// 单位特性（附件/阵营特殊能力）契约
    /// </summary>
    public interface IUnitTrait
    {
        void OnActivate(IUnitToken target);
        bool CanActivate(IUnitToken source);
    }
}
