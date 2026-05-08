using TBS.Unit;

namespace TBS.Presentation.UI.Panels.SpawnPanel
{
    /// <summary>
    /// 单位生成列表项数据
    /// </summary>
    public class SpawnUnitEntry
    {
        public string DisplayName { get; set; }
        public UnitRuntimeParams Params { get; set; }

        public SpawnUnitEntry(string displayName, UnitRuntimeParams @params)
        {
            DisplayName = displayName;
            Params = @params;
        }
    }
}
