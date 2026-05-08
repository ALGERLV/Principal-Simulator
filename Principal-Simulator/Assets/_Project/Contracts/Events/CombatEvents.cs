using TBS.Map.Tools;
using TBS.Unit;

namespace TBS.Contracts.Events
{
    public enum CombatType { Assault, Artillery, Reaction, CombinedAssault, Ambush }
    public enum CombatResult { AttackerWon, DefenderWon, Draw }
    public enum DamageSource { Attack, Counter, Artillery, Reaction }

    public struct CombatStartedEvent
    {
        public IUnitToken Attacker;
        public IUnitToken Defender;
        public CombatType Type;
        public MapHexCoord AttackerCoord;
        public MapHexCoord DefenderCoord;
    }

    public struct DamageDealtEvent
    {
        public IUnitToken Source;
        public IUnitToken Target;
        public int StrengthLoss;
        public int MoraleLoss;
        public DamageSource DamageSource;
    }

    public struct CombatEndedEvent
    {
        public IUnitToken Attacker;
        public IUnitToken Defender;
        public CombatResult Result;
        public bool DefenderRetreated;
        public bool OverrunTriggered;
    }

    public struct UnitShakenEvent
    {
        public IUnitToken Unit;
        public int MoraleAtTrigger;
    }

    public struct UnitRoutedEvent
    {
        public IUnitToken Unit;
        public MapHexCoord RetreatTarget;
    }

    public struct UnitEliminatedEvent
    {
        public IUnitToken Unit;
        public IUnitToken KilledBy;
        public MapHexCoord LastPosition;
    }

    public struct OverrunTriggeredEvent
    {
        public IUnitToken Attacker;
        public MapHexCoord FromCoord;
    }

    public struct SuppressionAppliedEvent
    {
        public IUnitToken Target;
        public float DurationHours;
        public IUnitToken Source;
    }
}
