namespace TBS.Contracts.Events
{
    public struct GameStartRequestedEvent
    {
        public TBS.Map.Data.LevelConfig LevelConfig;
    }

    public struct GameExitRequestedEvent { }

    public struct LevelLoadedEvent
    {
        public TBS.Map.Data.LevelConfig LevelConfig;
    }
}
