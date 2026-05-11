using TBS.Map.Runtime;
using UnityEngine;

using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Data
{
    /// <summary>
    /// 地图系统设计 3.1：地块网格配置（尺寸、形状、默认地形等）。
    /// 由 MapManager 在编辑器中拖入引用，不通过 CreateAssetMenu 创建。
    /// </summary>
    public class MapGridSetting : ScriptableObject
    {
        [Header("网格")]
        [SerializeField] private int mapWidth = 20;
        [SerializeField] private int mapHeight = 15;
        [SerializeField] private float hexSize = 1f;
        [SerializeField] private HexOrientation orientation = HexOrientation.PointyTop;
        [SerializeField] private TBS.Map.Runtime.GridShape gridShape = TBS.Map.Runtime.GridShape.Hexagon;

        [Header("地形")]
        [SerializeField] private TerrainData defaultTerrain;

        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public float HexSize => hexSize;
        public HexOrientation Orientation => orientation;
        public TBS.Map.Runtime.GridShape GridShape => gridShape;
        public TerrainData DefaultTerrain => defaultTerrain;
    }
}
