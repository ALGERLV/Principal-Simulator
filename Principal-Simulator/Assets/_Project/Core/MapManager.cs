using System;
using System.Collections.Generic;
using TBS.Map.Data;
using TBS.Map.Rendering;
using TBS.Map.Tools;
using TBS.Map.Runtime;
using UnityEngine;
using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Managers
{
    /// <summary>
    /// 地图系统总控 - 直接管理所有地块、加载配置、协调渲染层
    /// 用户使用流程：
    /// 1. 场景中创建 MapManager 和 TerrainParent
    /// 2. 在 Inspector 中配置所有参数
    /// 3. 运行时自动调用 InitializeMap()
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        #region Serialized Fields - 基本设置
        [Header("基本设置")]
        [SerializeField] private GameObject tilePrefab;           // 地块预制体（必须包含MapTileCell）
        [SerializeField] private TBS.Map.Runtime.HexOrientation orientation = TBS.Map.Runtime.HexOrientation.PointyTop;
        [SerializeField] private float hexSize = 1f;
        [SerializeField] private Transform terrainParent;         // 地块父节点
        #endregion

        #region Serialized Fields - 地形设置（配置模式）
        [Header("地形设置 - 配置模式")]
        [Tooltip("使用配置文件（true）或随机生成（false）")]
        [SerializeField] private bool useGridSetting = true;
        [SerializeField] private MapGridSetting gridSetting;      // 网格配置文件
        #endregion

        #region Serialized Fields - 地形设置（随机生成模式）
        [Header("地形设置 - 随机生成模式")]
        [Tooltip("随机地形数组，从其中选择")]
        [SerializeField] private TerrainData[] randomTerrains;
        [Tooltip("使用随机地形的概率(0-1)，剩余使用默认")]
        [SerializeField, Range(0f, 1f)] private float randomChance = 0.3f;
        [Tooltip("默认地形（当不随机时使用）")]
        [SerializeField] private TerrainData defaultTerrain;
        #endregion

        #region Serialized Fields - 河流设置
        [Header("河流设置")]
        [SerializeField] private Material riverMaterial;
        [SerializeField] private Mesh riverMesh;
        [SerializeField] private MapRouteSetting riverSetting;
        #endregion

        #region Serialized Fields - 道路设置
        [Header("道路设置")]
        [SerializeField] private Material roadMaterial;
        [SerializeField] private Mesh roadMesh;
        [SerializeField] private MapRouteSetting roadSetting;
        #endregion

        #region Serialized Fields - 植被设置
        [Header("植被设置")]
        [SerializeField] private Mesh vegetationMesh;
        [SerializeField] private Material vegetationMaterial;
        [SerializeField] private int vegetationPerTile = 5;
        #endregion

        #region Runtime Data
        private Dictionary<MapHexCoord, MapTileCell> tiles = new Dictionary<MapHexCoord, MapTileCell>();
        private MapRouteRenderer routeRenderer;
        private MapVegetationRenderer vegetationRenderer;
        #endregion

        #region Public Properties
        public IReadOnlyDictionary<MapHexCoord, MapTileCell> Tiles => tiles;
        public TBS.Map.Runtime.HexOrientation Orientation => orientation;
        public float HexSize => hexSize;
        public Transform TerrainParent => terrainParent;
        public const float ElevationWorldStep = 0.06f;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 自动查找渲染器
            routeRenderer = GetComponentInChildren<MapRouteRenderer>(true);
            vegetationRenderer = GetComponentInChildren<MapVegetationRenderer>(true);
        }

        void Start()
        {
            // 确保有父节点
            if (terrainParent == null)
            {
                var parent = transform.Find("TerrainParent");
                if (parent == null)
                {
                    var go = new GameObject("TerrainParent");
                    go.transform.SetParent(transform);
                    terrainParent = go.transform;
                }
                else
                {
                    terrainParent = parent;
                }
            }

            // 自动加载缺失的配置
            if (useGridSetting && gridSetting == null)
            {
                gridSetting = Resources.Load<MapGridSetting>("MapConfigs/TestGridSetting");
            }
            if (defaultTerrain == null)
            {
                defaultTerrain = Resources.Load<TerrainData>("Terrain/Plain");
            }

            InitializeMap();
        }
        #endregion

        #region Map Initialization
        /// <summary>
        /// 初始化地图 - 核心入口
        /// </summary>
        public void InitializeMap()
        {
            if (tilePrefab == null)
            {
                Debug.LogError("[MapManager] TilePrefab 未设置！请在 Inspector 中配置。");
                return;
            }

            // 清除旧地图
            ClearMap();

            // 获取地图参数
            int width, height;
            TBS.Map.Runtime.GridShape shape;
            TerrainData baseTerrain;

            if (useGridSetting && gridSetting != null)
            {
                // 配置模式
                hexSize = gridSetting.HexSize;
                orientation = gridSetting.Orientation;
                width = gridSetting.MapWidth;
                height = gridSetting.MapHeight;
                shape = gridSetting.GridShape;
                baseTerrain = gridSetting.DefaultTerrain;
            }
            else
            {
                // 随机模式 - 使用默认值或Inspector设置
                width = 15;
                height = 10;
                shape = TBS.Map.Runtime.GridShape.Rectangle;
                baseTerrain = defaultTerrain;
            }

            // 生成网格
            GenerateGrid(width, height, shape, baseTerrain);

            // 初始化渲染器
            InitializeRenderers();

            Debug.Log($"[MapManager] 地图初始化完成: {tiles.Count} 个地块");
        }

        /// <summary>
        /// 清除所有地块
        /// </summary>
        void ClearMap()
        {
            foreach (var tile in tiles.Values)
            {
                if (tile != null && tile.gameObject != null)
                {
                    Destroy(tile.gameObject);
                }
            }
            tiles.Clear();
        }

        /// <summary>
        /// 生成网格
        /// </summary>
        void GenerateGrid(int width, int height, TBS.Map.Runtime.GridShape shape, TerrainData baseTerrain)
        {
            switch (shape)
            {
                case TBS.Map.Runtime.GridShape.Rectangle:
                    GenerateRectangle(width, height, baseTerrain);
                    break;
                case TBS.Map.Runtime.GridShape.Circle:
                    GenerateCircle(Mathf.Min(width, height) / 2, baseTerrain);
                    break;
                case TBS.Map.Runtime.GridShape.Hexagon:
                    GenerateHexagon(Mathf.Min(width, height) / 2, baseTerrain);
                    break;
                default:
                    GenerateRectangle(width, height, baseTerrain);
                    break;
            }
        }

        void GenerateRectangle(int width, int height, TerrainData baseTerrain)
        {
            for (int q = 0; q < width; q++)
            {
                for (int r = 0; r < height; r++)
                {
                    var coord = new MapHexCoord(q, r);
                    CreateTile(coord, baseTerrain);
                }
            }
        }

        void GenerateCircle(int radius, TerrainData baseTerrain)
        {
            var center = new MapHexCoord(0, 0);
            for (int q = -radius; q <= radius; q++)
            {
                for (int r = -radius; r <= radius; r++)
                {
                    var coord = new MapHexCoord(q, r);
                    if (center.DistanceTo(coord) <= radius)
                    {
                        CreateTile(coord, baseTerrain);
                    }
                }
            }
        }

        void GenerateHexagon(int radius, TerrainData baseTerrain)
        {
            var center = new MapHexCoord(0, 0);
            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    var coord = new MapHexCoord(q, r);
                    CreateTile(coord, baseTerrain);
                }
            }
        }
        #endregion

        #region Tile Creation
        /// <summary>
        /// 创建单个地块
        /// </summary>
        MapTileCell CreateTile(MapHexCoord coord, TerrainData baseTerrain)
        {
            // 实例化预制体
            GameObject tileObject = Instantiate(tilePrefab, terrainParent);
            tileObject.name = $"Tile_{coord.Q}_{coord.R}";

            // 定位
            Vector3 worldPos = CoordToWorldPosition(coord);
            tileObject.transform.position = worldPos;

            // 获取或添加 MapTileCell
            var tile = tileObject.GetComponent<MapTileCell>();
            if (tile == null)
                tile = tileObject.AddComponent<MapTileCell>();

            // 确定地形
            TerrainData terrain = SelectTerrain(baseTerrain);

            // 初始化
            tile.Initialize(coord, terrain);

            tiles[coord] = tile;
            return tile;
        }

        /// <summary>
        /// 根据配置选择地形
        /// </summary>
        TerrainData SelectTerrain(TerrainData baseTerrain)
        {
            // 只要配置了随机地形数组，就尝试随机选择
            if (randomTerrains != null && randomTerrains.Length > 0)
            {
                if (UnityEngine.Random.value < randomChance)
                {
                    return randomTerrains[UnityEngine.Random.Range(0, randomTerrains.Length)];
                }
            }
            return baseTerrain ?? defaultTerrain;
        }
        #endregion

        #region Renderers Initialization
        /// <summary>
        /// 初始化所有渲染器
        /// </summary>
        void InitializeRenderers()
        {
            // 确保有路由渲染器
            if (routeRenderer == null)
            {
                routeRenderer = gameObject.AddComponent<MapRouteRenderer>();
            }

            // 初始化河流/道路渲染器
            if (routeRenderer != null)
            {
                if (riverMaterial != null)
                    routeRenderer.SetRiverMaterial(riverMaterial);
                if (roadMaterial != null)
                    routeRenderer.SetRoadMaterial(roadMaterial);

                // 合并河流和道路配置
                MapRouteSetting combinedSetting = CreateCombinedRouteSetting();
                routeRenderer.OnMapLoaded(combinedSetting, this);
            }

            // 确保有植被渲染器
            if (vegetationRenderer == null)
            {
                vegetationRenderer = gameObject.AddComponent<MapVegetationRenderer>();
            }

            // 初始化植被渲染器
            if (vegetationRenderer != null)
            {
                if (vegetationMaterial != null)
                    vegetationRenderer.SetVegetationMaterial(vegetationMaterial);
                if (vegetationMesh != null)
                    vegetationRenderer.SetVegetationMesh(vegetationMesh);

                vegetationRenderer.OnMapLoaded(this);
            }
        }

        /// <summary>
        /// 合并河流和道路配置
        /// </summary>
        MapRouteSetting CreateCombinedRouteSetting()
        {
            if (riverSetting == null && roadSetting == null)
                return null;

            // 创建临时合并配置
            var combined = ScriptableObject.CreateInstance<MapRouteSetting>();

            // 添加河流连接
            if (riverSetting != null && riverSetting.Links != null)
            {
                combined.AddLinks(riverSetting.Links);
            }

            // 添加道路连接
            if (roadSetting != null && roadSetting.Links != null)
            {
                combined.AddLinks(roadSetting.Links);
            }

            return combined;
        }
        #endregion

        #region Coordinate Conversion
        /// <summary>
        /// 轴向坐标转世界坐标
        /// </summary>
        public Vector3 CoordToWorldPosition(MapHexCoord coord)
        {
            float x, z;
            float size = hexSize;

            if (orientation == HexOrientation.PointyTop)
            {
                x = size * (Mathf.Sqrt(3) * coord.Q + Mathf.Sqrt(3) / 2f * coord.R);
                z = size * 1.5f * coord.R;
            }
            else
            {
                x = size * 1.5f * coord.Q;
                z = size * (Mathf.Sqrt(3) * coord.R + Mathf.Sqrt(3) / 2f * coord.Q);
            }

            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// 世界坐标转轴向坐标
        /// </summary>
        public MapHexCoord WorldPositionToCoord(Vector3 worldPos)
        {
            float size = hexSize;
            float q, r;

            if (orientation == HexOrientation.PointyTop)
            {
                q = (Mathf.Sqrt(3) / 3f * worldPos.x - 1f / 3f * worldPos.z) / size;
                r = (2f / 3f * worldPos.z) / size;
            }
            else
            {
                q = (2f / 3f * worldPos.x) / size;
                r = (Mathf.Sqrt(3) / 3f * worldPos.z - 1f / 3f * worldPos.x) / size;
            }

            return MapHexCoord.FromAxial(Mathf.RoundToInt(q), Mathf.RoundToInt(r));
        }
        #endregion

        #region Public API
        /// <summary>
        /// 获取指定坐标的地块
        /// </summary>
        public MapTileCell GetTile(MapHexCoord coord)
        {
            tiles.TryGetValue(coord, out var tile);
            return tile;
        }

        /// <summary>
        /// 修改指定坐标的地形
        /// </summary>
        public void SetTileTerrain(MapHexCoord coord, TerrainData terrain)
        {
            if (tiles.TryGetValue(coord, out var tile))
            {
                tile.ApplyTerrain(terrain);
            }
        }

        /// <summary>
        /// 设置地块海拔
        /// </summary>
        public void SetTileElevation(MapHexCoord coord, int elevationLevel)
        {
            if (tiles.TryGetValue(coord, out var tile))
            {
                tile.ElevationLevel = elevationLevel;
                // 更新Y轴位置
                Vector3 pos = tile.transform.position;
                pos.y = elevationLevel * ElevationWorldStep;
                tile.transform.position = pos;
            }
        }
        #endregion

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
