using System;
using System.Collections.Generic;
using System.Linq;
using TBS.Map.API;
using TBS.Map.Data;
using TBS.Map.Tools;
using UnityEngine;

// 明确指定使用我们的TerrainData，避免与UnityEngine.TerrainData冲突
using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Components
{
    /// <summary>
    /// 六边形网格管理器 - 负责生成和管理所有地块
    /// </summary>
    public class HexGrid : MonoBehaviour, IMapDataProvider, ITerrainQuery
    {
        #region Events

        /// <summary>
        /// 地图生成完成时触发
        /// </summary>
        public event Action OnGridGenerated;

        /// <summary>
        /// 地块创建时触发
        /// </summary>
        public event Action<HexTile> OnTileCreated;

        /// <summary>
        /// 地块移除时触发
        /// </summary>
        public event Action<HexCoord> OnTileRemoved;

        #endregion

        #region Serialized Fields

        [Header("网格设置")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private int mapWidth = 20;
        [SerializeField] private int mapHeight = 15;
        [SerializeField] private float hexSize = 1f;
        [SerializeField] private HexOrientation orientation = HexOrientation.PointyTop;
        [SerializeField] private GridShape gridShape = GridShape.Rectangle;

        [Header("地形设置")]
        [SerializeField] private TerrainData defaultTerrain;
        [SerializeField] private TerrainData[] randomTerrains; // 随机地形数组
        [SerializeField] private float randomTerrainChance = 0.3f; // 随机地形概率
        [SerializeField] private Transform tileParent;

        #endregion

        #region Private Fields

        private Dictionary<HexCoord, HexTile> tiles = new Dictionary<HexCoord, HexTile>();
        private BoundsInt bounds;
        private bool isInitialized = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// 地图宽度
        /// </summary>
        public int Width => mapWidth;

        /// <summary>
        /// 地图高度
        /// </summary>
        public int Height => mapHeight;

        /// <summary>
        /// 地块总数
        /// </summary>
        public int TileCount => tiles.Count;

        /// <summary>
        /// 地图边界
        /// </summary>
        public BoundsInt Bounds => bounds;

        /// <summary>
        /// 六边形大小
        /// </summary>
        public float HexSize => hexSize;

        /// <summary>
        /// 六边形朝向
        /// </summary>
        public HexOrientation Orientation => orientation;

        /// <summary>
        /// 所有地块的枚举
        /// </summary>
        public IEnumerable<HexTile> AllTiles => tiles.Values;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => isInitialized;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            tiles = new Dictionary<HexCoord, HexTile>();
            CalculateBounds();
        }

        private void Start()
        {
            if (!isInitialized && tiles.Count == 0)
            {
                // 如未初始化则自动生成地图
                GenerateDefault();
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        #endregion

        #region Generation Methods

        /// <summary>
        /// 使用默认设置生成地图
        /// </summary>
        public void GenerateDefault()
        {
            switch (gridShape)
            {
                case GridShape.Rectangle:
                    GenerateRectangle(mapWidth, mapHeight);
                    break;
                case GridShape.Circle:
                    GenerateCircle(Mathf.Min(mapWidth, mapHeight) / 2);
                    break;
                default:
                    GenerateRectangle(mapWidth, mapHeight);
                    break;
            }
        }

        /// <summary>
        /// 根据配置生成地图
        /// </summary>
        public void Generate(MapConfig config)
        {
            Clear();

            mapWidth = config.Width;
            mapHeight = config.Height;
            gridShape = config.Shape;
            defaultTerrain = config.DefaultTerrain;

            if (config.TerrainOverrides != null)
            {
                foreach (var kvp in config.TerrainOverrides)
                {
                    CreateTile(kvp.Key, kvp.Value);
                }
            }
            else
            {
                GenerateDefault();
            }

            OnGridGenerated?.Invoke();
            isInitialized = true;
        }

        /// <summary>
        /// 生成矩形地图
        /// </summary>
        public void GenerateRectangle(int width, int height)
        {
            Clear();

            for (int q = 0; q < width; q++)
            {
                for (int r = 0; r < height; r++)
                {
                    // 轴向坐标（q列，r行）
                    var coord = new HexCoord(q, r);
                    CreateTile(coord, null);
                }
            }

            CalculateBounds();
            OnGridGenerated?.Invoke();
            isInitialized = true;
        }

        /// <summary>
        /// 生成圆形地图
        /// </summary>
        public void GenerateCircle(int radius)
        {
            Clear();

            var center = new HexCoord(0, 0);
            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    var coord = new HexCoord(q, r);
                    CreateTile(coord, null);
                }
            }

            CalculateBounds();
            OnGridGenerated?.Invoke();
            isInitialized = true;
        }

        /// <summary>
        /// 清除所有地块
        /// </summary>
        public void Clear()
        {
            foreach (var tile in tiles.Values)
            {
                if (tile != null && tile.gameObject != null)
                {
                    if (Application.isPlaying)
                        Destroy(tile.gameObject);
                    else
                        DestroyImmediate(tile.gameObject);
                }
            }

            tiles.Clear();
            isInitialized = false;
        }

        #endregion

        #region Tile Creation

        /// <summary>
        /// 创建单个地块
        /// </summary>
        public HexTile CreateTile(HexCoord coord, TerrainData terrain)
        {
            if (tiles.ContainsKey(coord))
            {
                Debug.LogWarning($"HexGrid: 坐标 {coord} 已存在地块，跳过创建");
                return tiles[coord];
            }

            if (tilePrefab == null)
            {
                Debug.LogError("HexGrid: 未设置tilePrefab");
                return null;
            }

            // 实例化地块
            var tileObject = Instantiate(tilePrefab, tileParent ?? transform);
            var tile = tileObject.GetComponent<HexTile>();

            if (tile == null)
            {
                tile = tileObject.AddComponent<HexTile>();
            }

            // 选择地形：传入的terrain > 随机地形 > 默认地形
            TerrainData selectedTerrain = terrain;
            Debug.Log($"CreateTile坐标{coord}: 传入terrain={(terrain != null ? terrain.TerrainName : "null")}");
            
            if (selectedTerrain == null && randomTerrains != null && randomTerrains.Length > 0)
            {
                float randomValue = UnityEngine.Random.value;
                Debug.Log($"  随机检查: {randomValue:F3} < {randomTerrainChance}? {(randomValue < randomTerrainChance ? "选中随机" : "使用默认")}");
                // 随机决定是否使用随机地形
                if (randomValue < randomTerrainChance)
                {
                    int randomIndex = UnityEngine.Random.Range(0, randomTerrains.Length);
                    selectedTerrain = randomTerrains[randomIndex];
                    Debug.Log($"  选中随机地形[{randomIndex}]: {selectedTerrain.TerrainName}");
                }
            }
            else if (selectedTerrain == null)
            {
                Debug.LogWarning($"  RandomTerrains为空，使用默认地形");
            }
            selectedTerrain ??= defaultTerrain;
            
            // 初始化地块
            tile.Initialize(coord, selectedTerrain);

            // 设置世界位置
            tileObject.transform.position = CoordToWorldPosition(coord);

            // 存储地块
            tiles[coord] = tile;

            OnTileCreated?.Invoke(tile);

            return tile;
        }

        /// <summary>
        /// 移除指定地块
        /// </summary>
        public void RemoveTile(HexCoord coord)
        {
            if (!tiles.TryGetValue(coord, out var tile))
                return;

            tiles.Remove(coord);

            if (tile != null && tile.gameObject != null)
            {
                if (Application.isPlaying)
                    Destroy(tile.gameObject);
                else
                    DestroyImmediate(tile.gameObject);
            }

            OnTileRemoved?.Invoke(coord);
        }

        /// <summary>
        /// 修改指定坐标的地形
        /// </summary>
        public void SetTerrain(HexCoord coord, TerrainData terrain)
        {
            if (tiles.TryGetValue(coord, out var tile))
            {
                // 通过反射或序列化修改地形数据
                var serializedObject = new UnityEditor.SerializedObject(tile);
                var terrainProp = serializedObject.FindProperty("terrainData");
                if (terrainProp != null)
                {
                    terrainProp.objectReferenceValue = terrain;
                    serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                CreateTile(coord, terrain);
            }
        }

        #endregion

        #region Query Methods - IMapDataProvider

        /// <summary>
        /// 获取指定坐标的地块
        /// </summary>
        public HexTile GetTile(HexCoord coord)
        {
            tiles.TryGetValue(coord, out var tile);
            return tile;
        }

        /// <summary>
        /// 安全获取地块
        /// </summary>
        public bool TryGetTile(HexCoord coord, out HexTile tile)
        {
            return tiles.TryGetValue(coord, out tile);
        }

        /// <summary>
        /// 检查坐标是否存在地块
        /// </summary>
        public bool HasTile(HexCoord coord)
        {
            return tiles.ContainsKey(coord);
        }

        /// <summary>
        /// 获取范围内的所有地块
        /// </summary>
        public HexTile[] GetTilesInRange(HexCoord center, int range)
        {
            var coords = center.GetSpiral(range);
            var result = new List<HexTile>();

            foreach (var coord in coords)
            {
                if (tiles.TryGetValue(coord, out var tile))
                {
                    result.Add(tile);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取环上的地块
        /// </summary>
        public HexTile[] GetTilesInRing(HexCoord center, int radius)
        {
            var coords = center.GetRing(radius);
            var result = new List<HexTile>();

            foreach (var coord in coords)
            {
                if (tiles.TryGetValue(coord, out var tile))
                {
                    result.Add(tile);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取所有地块数组
        /// </summary>
        public HexTile[] GetAllTiles()
        {
            return tiles.Values.ToArray();
        }

        /// <summary>
        /// 获取邻接地块（只返回存在的）
        /// </summary>
        public HexTile[] GetNeighbors(HexCoord coord)
        {
            var neighborCoords = coord.GetNeighbors();
            var result = new List<HexTile>();

            foreach (var neighborCoord in neighborCoords)
            {
                if (tiles.TryGetValue(neighborCoord, out var tile))
                {
                    result.Add(tile);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取地图边界（IMapDataProvider实现）
        /// </summary>
        BoundsInt IMapDataProvider.GetMapBounds()
        {
            return bounds;
        }

        #endregion

        #region Terrain Query - ITerrainQuery

        /// <summary>
        /// 获取指定坐标的移动消耗
        /// </summary>
        public float GetMovementCost(HexCoord coord)
        {
            if (tiles.TryGetValue(coord, out var tile))
            {
                return tile.MovementCost;
            }
            return float.MaxValue; // 不可通行
        }

        /// <summary>
        /// 获取指定坐标的防御加成
        /// </summary>
        public float GetDefenseBonus(HexCoord coord)
        {
            if (tiles.TryGetValue(coord, out var tile))
            {
                return tile.DefenseBonus;
            }
            return 0f;
        }

        /// <summary>
        /// 获取指定坐标的视野修正
        /// </summary>
        public float GetVisibilityModifier(HexCoord coord)
        {
            if (tiles.TryGetValue(coord, out var tile))
            {
                return tile.VisibilityModifier;
            }
            return 0f;
        }

        /// <summary>
        /// 检查是否可通过
        /// </summary>
        public bool IsPassable(HexCoord coord)
        {
            if (tiles.TryGetValue(coord, out var tile))
            {
                return tile.IsPassable;
            }
            return false;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 坐标转世界位置
        /// </summary>
        public Vector3 CoordToWorldPosition(HexCoord coord)
        {
            return coord.ToWorldPosition(hexSize, orientation);
        }

        /// <summary>
        /// 世界位置转坐标
        /// </summary>
        public HexCoord WorldPositionToCoord(Vector3 worldPos)
        {
            float q, r;

            if (orientation == HexOrientation.PointyTop)
            {
                q = (Mathf.Sqrt(3) / 3 * worldPos.x - 1f / 3 * worldPos.z) / hexSize;
                r = (2f / 3 * worldPos.z) / hexSize;
            }
            else
            {
                q = (2f / 3 * worldPos.x) / hexSize;
                r = (-1f / 3 * worldPos.x + Mathf.Sqrt(3) / 3 * worldPos.z) / hexSize;
            }

            return HexCoord.FromAxial(Mathf.RoundToInt(q), Mathf.RoundToInt(r));
        }

        /// <summary>
        /// 获取两点间直线路径
        /// </summary>
        public HexCoord[] GetLine(HexCoord from, HexCoord to)
        {
            int distance = from.DistanceTo(to);
            var result = new HexCoord[distance + 1];

            for (int i = 0; i <= distance; i++)
            {
                float t = (float)i / distance;
                float x = Mathf.Lerp(from.X, to.X, t);
                float y = Mathf.Lerp(from.Y, to.Y, t);
                float z = Mathf.Lerp(from.Z, to.Z, t);
                result[i] = HexCoord.FromCube(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
            }

            return result;
        }

        /// <summary>
        /// 从数据加载地图
        /// </summary>
        public void LoadFromData(GridData data)
        {
            Clear();

            foreach (var tileData in data.Tiles)
            {
                // 根据地形ID查找对应的TerrainData资源
                var terrain = FindTerrainById(tileData.TerrainId);
                CreateTile(tileData.Coord, terrain);
            }

            CalculateBounds();
            OnGridGenerated?.Invoke();
            isInitialized = true;
        }

        /// <summary>
        /// 导出地图数据
        /// </summary>
        public GridData SaveToData()
        {
            var data = new GridData
            {
                Width = mapWidth,
                Height = mapHeight,
                Shape = gridShape,
                Tiles = new List<TileData>()
            };

            foreach (var kvp in tiles)
            {
                data.Tiles.Add(new TileData
                {
                    Coord = kvp.Key,
                    TerrainId = kvp.Value.TerrainId
                });
            }

            return data;
        }

        #endregion

        #region Private Methods

        private void CalculateBounds()
        {
            if (tiles.Count == 0)
            {
                bounds = new BoundsInt(0, 0, 0, 0, 0, 0);
                return;
            }

            int minX = int.MaxValue, maxX = int.MinValue;
            int minZ = int.MaxValue, maxZ = int.MinValue;

            foreach (var coord in tiles.Keys)
            {
                var offset = coord.ToOffset(OffsetCoordType.OddR);
                minX = Mathf.Min(minX, offset.x);
                maxX = Mathf.Max(maxX, offset.x);
                minZ = Mathf.Min(minZ, offset.y);
                maxZ = Mathf.Max(maxZ, offset.y);
            }

            bounds = new BoundsInt(minX, 0, minZ, maxX - minX + 1, 1, maxZ - minZ + 1);
        }

        private TerrainData FindTerrainById(string terrainId)
        {
            // 这里可以通过资源管理器查找地形数据
            // 暂时返回默认地形
            return defaultTerrain;
        }

        #endregion
    }

    #region Helper Types

    /// <summary>
    /// 网格形状
    /// </summary>
    public enum GridShape
    {
        Rectangle,
        Circle,
        Hexagon,
        Custom
    }

    /// <summary>
    /// 地图配置
    /// </summary>
    [Serializable]
    public class MapConfig
    {
        public int Width;
        public int Height;
        public GridShape Shape;
        public TerrainData DefaultTerrain;
        public Dictionary<HexCoord, TerrainData> TerrainOverrides;
    }

    /// <summary>
    /// 网格数据（用于序列化）
    /// </summary>
    [Serializable]
    public class GridData
    {
        public int Width;
        public int Height;
        public GridShape Shape;
        public List<TileData> Tiles;
    }

    /// <summary>
    /// 地块数据
    /// </summary>
    [Serializable]
    public class TileData
    {
        public HexCoord Coord;
        public string TerrainId;
    }

    #endregion
}
