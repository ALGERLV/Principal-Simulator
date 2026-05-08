using TBS.Map.Tools;
using TBS.Unit;
using TBS.UnitSystem;

namespace TBS.Contracts.Events
{
    /// <summary>
    /// 单位生成相关事件
    /// </summary>

    public struct UnitSpawnRequestedEvent
    {
        public HexCoord TargetCoord;
        public UnitRuntimeParams Params;
    }

    public struct SpawnModeChangedEvent
    {
        public bool IsActive;
    }

    public struct UnitSpawnedEvent
    {
        public UnitToken Token;
    }

    public struct UnitDespawnedEvent
    {
        public UnitToken Token;
    }
}
