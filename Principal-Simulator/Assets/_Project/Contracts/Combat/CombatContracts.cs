using System.Collections.Generic;
using TBS.Contracts.Events;
using TBS.Map.Tools;
using TBS.Unit;

namespace TBS.Contracts.Combat
{
    public struct CombatParameters
    {
        public IUnitToken Attacker;
        public IUnitToken Defender;
        public MapHexCoord AttackerCoord;
        public MapHexCoord DefenderCoord;

        public float EffectiveAttack;
        public float EffectiveDefense;
        public float CombinedAttackBonus;
        public float TerrainAttackModifier;
        public float TerrainDefenseBonus;
        public float WeatherModifier;
        public float SupplyAttackModifier;
        public float SupplyDefenseModifier;
        public int FortificationLevel;

        public bool IgnoreFortification;
        public bool IgnoreTerrainDefense;
        public int AttackBonusFlat;
    }

    public struct CombatDamage
    {
        public int AttackerStrengthLoss;
        public int DefenderStrengthLoss;
        public int AttackerMoraleLoss;
        public int DefenderMoraleLoss;
    }

    public interface ICombatRule
    {
        string RuleId { get; }
        bool Applies(IUnitToken attacker, IUnitToken defender, CombatType type);
        void ModifyParameters(ref CombatParameters parameters);
    }

    public interface IGameContext
    {
        WeatherType CurrentWeather { get; }
        float CurrentTimeHours { get; }
        IUnitToken GetUnitAt(MapHexCoord coord);
        bool IsOccupied(MapHexCoord coord);
    }

    public interface IDamageCalculator
    {
        CombatDamage CalculateAssault(CombatParameters parameters);
        int CalculateArtillery(IUnitToken source, IUnitToken target, IGameContext context);
        int CalculateReactionFire(IUnitToken source, IUnitToken target);
    }

    public interface ITacticalEffect
    {
        string EffectId { get; }
        void Apply(IUnitToken target, IGameContext context);
        bool IsExpired(IUnitToken target, float gameTimeHours);
    }

    public interface ICombatExecutor
    {
        bool CanAttack(IUnitToken attacker, IUnitToken defender, CombatType type, out string reason);
        CombatResult ExecuteAssault(IUnitToken attacker, IUnitToken defender, IGameContext context);
        void ExecuteArtillery(IUnitToken source, MapHexCoord targetCoord, IGameContext context);
        CombatResult ExecuteCombinedAssault(IReadOnlyList<IUnitToken> attackers, IUnitToken defender, IGameContext context);
    }
}
