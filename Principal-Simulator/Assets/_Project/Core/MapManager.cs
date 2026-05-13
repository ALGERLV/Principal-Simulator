using System;
using System.Collections.Generic;
using TBS.Map.Data;
using TBS.Map.Rendering;
using TBS.Map.Repositories;
using TBS.Map.Tools;
using TBS.Map.Runtime;
using TBS.Core.Events;
using TBS.Contracts.Events;
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
        private MapEventPointManager eventPointManager;
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

            // 输出配置信息
            Debug.Log($"[MapManager] 地图配置: 宽{width}高{height}, " +
                      $"随机地形数量: {(randomTerrains?.Length ?? 0)}, " +
                      $"随机概率: {randomChance:P0}, " +
                      $"基础地形: {baseTerrain?.TerrainName ?? "null"}, " +
                      $"默认地形: {defaultTerrain?.TerrainName ?? "null"}");

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

        #region Level-Based Initialization

        public void InitializeFromLevel(LevelConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[MapManager] LevelConfig 为空");
                return;
            }

            if (tilePrefab == null)
            {
                Debug.LogError("[MapManager] TilePrefab 未设置");
                return;
            }

            ClearMap();

            hexSize = config.HexSize;
            orientation = config.Orientation;

            Debug.Log($"[MapManager] 加载关卡: {config.LevelName} ({config.MapWidth}x{config.MapHeight})");

            // 确保 TerrainLibrary 初始化
            TerrainLibrary.Initialize();

            // 构建 terrainId → coord 的查找表
            var terrainLookup = new Dictionary<MapHexCoord, string>();
            foreach (var entry in config.TileTerrains)
                terrainLookup[new MapHexCoord(entry.Q, entry.R)] = entry.TerrainId;

            // 生成地块
            for (int q = 0; q < config.MapWidth; q++)
            {
                for (int r = 0; r < config.MapHeight; r++)
                {
                    var coord = new MapHexCoord(q, r);
                    TerrainData terrain;

                    if (terrainLookup.TryGetValue(coord, out var terrainId))
                        terrain = TerrainLibrary.GetTerrainById(terrainId) ?? config.DefaultTerrain ?? defaultTerrain;
                    else
                        terrain = config.DefaultTerrain ?? defaultTerrain;

                    CreateTileWithTerrain(coord, terrain);
                }
            }

            // 初始化渲染器（含河流）
            InitializeRenderersForLevel(config);

            // 生成事件点
            SpawnEventPoints(config);

            Debug.Log($"[MapManager] 关卡加载完成: {tiles.Count} 个地块");
        }

        void CreateTileWithTerrain(MapHexCoord coord, TerrainData terrain)
        {
            GameObject tileObject = Instantiate(tilePrefab, terrainParent);
            tileObject.name = $"Tile_{coord.Q}_{coord.R}";

            Vector3 worldPos = CoordToWorldPosition(coord);
            tileObject.transform.position = worldPos;

            var tile = tileObject.GetComponent<MapTileCell>();
            if (tile == null)
                tile = tileObject.AddComponent<MapTileCell>();

            tile.Initialize(coord, terrain);

            Transform hexTransform = tileObject.transform.Find("Hex");
            if (hexTransform != null)
            {
                hexTransform.localScale = Vector3.one;

                var hexFilter = hexTransform.GetComponent<MeshFilter>();
                if (hexFilter != null)
                    hexFilter.mesh = CreateHexMeshForTile();

                var hexRenderer = hexTransform.GetComponent<MeshRenderer>();
                if (hexRenderer != null && terrain != null)
                {
                    hexRenderer.material = new Material(hexRenderer.sharedMaterial);
                    hexRenderer.material.color = terrain.TerrainColor;
                }

                var hexCollider = hexTransform.GetComponent<MeshCollider>();
                if (hexCollider != null && hexFilter != null)
                    hexCollider.sharedMesh = hexFilter.mesh;
            }

            tiles[coord] = tile;
        }

        void InitializeRenderersForLevel(LevelConfig config)
        {
            if (routeRenderer == null)
                routeRenderer = gameObject.AddComponent<MapRouteRenderer>();

            if (routeRenderer != null)
            {
                if (riverMaterial != null)
                    routeRenderer.SetRiverMaterial(riverMaterial);
                if (roadMaterial != null)
                    routeRenderer.SetRoadMaterial(roadMaterial);

                // 使用关卡配置的河流数据
                var routeSetting = ScriptableObject.CreateInstance<MapRouteSetting>();
                if (config.Rivers != null && config.Rivers.Count > 0)
                    routeSetting.AddLinks(config.Rivers);
                // 也加入 Inspector 中配置的河流/道路
                if (riverSetting != null && riverSetting.Links != null)
                    routeSetting.AddLinks(riverSetting.Links);
                if (roadSetting != null && roadSetting.Links != null)
                    routeSetting.AddLinks(roadSetting.Links);

                routeRenderer.OnMapLoaded(routeSetting, this);
            }

            if (vegetationRenderer == null)
                vegetationRenderer = gameObject.AddComponent<MapVegetationRenderer>();

            if (vegetationRenderer != null)
            {
                if (vegetationMaterial != null)
                    vegetationRenderer.SetVegetationMaterial(vegetationMaterial);
                if (vegetationMesh != null)
                    vegetationRenderer.SetVegetationMesh(vegetationMesh);
                vegetationRenderer.OnMapLoaded(this);
            }
        }

        void SpawnEventPoints(LevelConfig config)
        {
            if (config.EventPoints == null || config.EventPoints.Count == 0)
                return;

            // 生成 3D 标记物
            if (eventPointManager == null)
                eventPointManager = gameObject.AddComponent<MapEventPointManager>();
            eventPointManager.SpawnEventPoints(config, this);

            // 发出事件，让 EventPointCardManager 在自身就绪后创建 UI 卡片
            EventBus.Emit(new LevelLoadedEvent { LevelConfig = config });
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

            // 获取或添加 MapTileCell（在父对象上）
            var tile = tileObject.GetComponent<MapTileCell>();
            if (tile == null)
                tile = tileObject.AddComponent<MapTileCell>();

            // 确定地形
            TerrainData terrain = SelectTerrain(baseTerrain);
            Debug.Log($"[MapManager] CreateTile: 为 {coord} 选择地形: {terrain?.TerrainName ?? "null"}");

            // 初始化（传递Hex子对象给MapTileCell用于设置视觉）
            tile.Initialize(coord, terrain);

            // 找到Hex子对象并设置mesh和材质颜色
            Transform hexTransform = tileObject.transform.Find("Hex");
            if (hexTransform != null)
            {
                // 确保 scale 是统一缩放，防止拉伸
                hexTransform.localScale = Vector3.one;

                // 设置正确大小的mesh（避免重叠闪烁）
                var hexFilter = hexTransform.GetComponent<MeshFilter>();
                if (hexFilter != null)
                {
                    hexFilter.mesh = CreateHexMeshForTile();
                }

                // 设置材质颜色
                var hexRenderer = hexTransform.GetComponent<MeshRenderer>();
                if (hexRenderer != null && terrain != null)
                {
                    hexRenderer.material = new Material(hexRenderer.sharedMaterial);
                    hexRenderer.material.color = terrain.TerrainColor;
                    Debug.Log($"[MapManager] 在Hex子对象上设置mesh和颜色: {coord} -> {terrain.TerrainColor}");
                }
                else
                {
                    Debug.LogWarning($"[MapManager] Hex子对象缺少MeshRenderer或terrain为null");
                }

                // 同步更新collider的mesh
                var hexCollider = hexTransform.GetComponent<MeshCollider>();
                if (hexCollider != null && hexFilter != null)
                {
                    hexCollider.sharedMesh = hexFilter.mesh;
                }
            }
            else
            {
                Debug.LogWarning($"[MapManager] 找不到Hex子对象，无法设置颜色和mesh");
            }

            tiles[coord] = tile;
            return tile;
        }

        /// <summary>创建适合当前hexSize的六边形mesh（带缝隙避免重叠）。</summary>
        Mesh CreateHexMeshForTile()
        {
            Mesh mesh = new Mesh();
            mesh.name = "HexTile_Runtime";

            // 使用外接圆半径作为六边形大小基准
            float r = hexSize; // 外接圆半径
            float a = r * Mathf.Sqrt(3) / 2f; // 内切圆半径
            float w = a; // 半边宽
            float thickness = 0.2f;
            
            // 缩放因子：生成比理论尺寸略小的 mesh，留出缝隙避免重叠闪烁
            float scaleFactor = 0.95f;
            r *= scaleFactor;
            w *= scaleFactor;

            // 14个顶点
            Vector3[] vertices = new Vector3[14];
            
            // 上面
            vertices[0] = new Vector3(0, thickness/2, 0);
            vertices[1] = new Vector3(0, thickness/2, r);
            vertices[2] = new Vector3(w, thickness/2, r * 0.5f);
            vertices[3] = new Vector3(w, thickness/2, -r * 0.5f);
            vertices[4] = new Vector3(0, thickness/2, -r);
            vertices[5] = new Vector3(-w, thickness/2, -r * 0.5f);
            vertices[6] = new Vector3(-w, thickness/2, r * 0.5f);
            
            // 下面
            vertices[7] = new Vector3(0, -thickness/2, 0);
            vertices[8] = new Vector3(0, -thickness/2, r);
            vertices[9] = new Vector3(w, -thickness/2, r * 0.5f);
            vertices[10] = new Vector3(w, -thickness/2, -r * 0.5f);
            vertices[11] = new Vector3(0, -thickness/2, -r);
            vertices[12] = new Vector3(-w, -thickness/2, -r * 0.5f);
            vertices[13] = new Vector3(-w, -thickness/2, r * 0.5f);

            // 三角形
            int[] triangles = new int[144];
            int triIdx = 0;
            
            // 上面
            triangles[triIdx++] = 0; triangles[triIdx++] = 1; triangles[triIdx++] = 2;
            triangles[triIdx++] = 0; triangles[triIdx++] = 2; triangles[triIdx++] = 3;
            triangles[triIdx++] = 0; triangles[triIdx++] = 3; triangles[triIdx++] = 4;
            triangles[triIdx++] = 0; triangles[triIdx++] = 4; triangles[triIdx++] = 5;
            triangles[triIdx++] = 0; triangles[triIdx++] = 5; triangles[triIdx++] = 6;
            triangles[triIdx++] = 0; triangles[triIdx++] = 6; triangles[triIdx++] = 1;
            
            // 下面
            triangles[triIdx++] = 7; triangles[triIdx++] = 9; triangles[triIdx++] = 8;
            triangles[triIdx++] = 7; triangles[triIdx++] = 10; triangles[triIdx++] = 9;
            triangles[triIdx++] = 7; triangles[triIdx++] = 11; triangles[triIdx++] = 10;
            triangles[triIdx++] = 7; triangles[triIdx++] = 12; triangles[triIdx++] = 11;
            triangles[triIdx++] = 7; triangles[triIdx++] = 13; triangles[triIdx++] = 12;
            triangles[triIdx++] = 7; triangles[triIdx++] = 8; triangles[triIdx++] = 13;
            
            // 侧面
            triangles[triIdx++] = 1; triangles[triIdx++] = 8; triangles[triIdx++] = 9;
            triangles[triIdx++] = 1; triangles[triIdx++] = 9; triangles[triIdx++] = 2;
            triangles[triIdx++] = 2; triangles[triIdx++] = 9; triangles[triIdx++] = 10;
            triangles[triIdx++] = 2; triangles[triIdx++] = 10; triangles[triIdx++] = 3;
            triangles[triIdx++] = 3; triangles[triIdx++] = 10; triangles[triIdx++] = 11;
            triangles[triIdx++] = 3; triangles[triIdx++] = 11; triangles[triIdx++] = 4;
            triangles[triIdx++] = 4; triangles[triIdx++] = 11; triangles[triIdx++] = 12;
            triangles[triIdx++] = 4; triangles[triIdx++] = 12; triangles[triIdx++] = 5;
            triangles[triIdx++] = 5; triangles[triIdx++] = 12; triangles[triIdx++] = 13;
            triangles[triIdx++] = 5; triangles[triIdx++] = 13; triangles[triIdx++] = 6;
            triangles[triIdx++] = 6; triangles[triIdx++] = 13; triangles[triIdx++] = 8;
            triangles[triIdx++] = 6; triangles[triIdx++] = 8; triangles[triIdx++] = 1;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// 根据配置选择地形
        /// </summary>
        TerrainData SelectTerrain(TerrainData baseTerrain)
        {
            // 检查随机地形配置
            if (randomTerrains != null && randomTerrains.Length > 0 && randomChance > 0f)
            {
                // 以 randomChance 的概率使用随机地形
                float roll = UnityEngine.Random.value;
                if (roll < randomChance)
                {
                    int index = UnityEngine.Random.Range(0, randomTerrains.Length);
                    var selected = randomTerrains[index];
                    if (selected != null)
                    {
                        Debug.Log($"[MapManager] 随机地形选中: {selected.TerrainName} (概率{randomChance:P0}, 投掷{roll:F3})");
                        return selected;
                    }
                }
            }

            // 回退到基础地形或默认地形
            var result = baseTerrain ?? defaultTerrain;
            return result;
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
