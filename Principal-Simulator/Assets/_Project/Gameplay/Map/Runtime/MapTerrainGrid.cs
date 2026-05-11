using System;
using System.Collections.Generic;
using System.Linq;
using TBS.Map.API;
using TBS.Map.Data;
using TBS.Map.Managers;
using TBS.Map.Tools;
using UnityEngine;

using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Runtime
{
    /// <summary>
    /// 六边形网格朝向枚举。
    /// </summary>
    public enum HexOrientation
    {
        PointyTop,  // 尖顶朝上（轴向 q 指向右上）
        FlatTop     // 平顶朝上（轴向 q 指向右）
    }

    /// <summary>
    /// 网格形状枚举。
    /// </summary>
    public enum GridShape
    {
        Rectangle,
        Circle,
        Hexagon
    }

    /// <summary>
    /// 六边形网格管理器 - 负责生成和管理所有地块。
    /// </summary>
    public class MapTerrainGrid : MonoBehaviour, IMapDataProvider, ITerrainQuery
    {
        #region Events

        /// <summary>地图生成完成时触发。</summary>
        public event Action OnGridGenerated;

        /// <summary>地块创建时触发。</summary>
        public event Action<MapTileCell> OnTileCreated;

        /// <summary>地块移除时触发。</summary>
        public event Action<MapHexCoord> OnTileRemoved;

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
        [SerializeField] private TBS.Map.Data.TerrainData defaultTerrain;
        [SerializeField] private TBS.Map.Data.TerrainData[] randomTerrains;
        [SerializeField, Range(0f, 1f)] private float randomTerrainChance = 0.3f;
        [SerializeField] private Transform tileParent;
        [SerializeField] private bool autoLoadTerrains = true;

        #endregion

        #region Private Fields

        private Dictionary<MapHexCoord, MapTileCell> tiles = new Dictionary<MapHexCoord, MapTileCell>();
        private BoundsInt bounds;
        private bool isInitialized = false;

        #endregion

        #region Public Properties

        /// <summary>当前地块字典（只读）。</summary>
        public IReadOnlyDictionary<MapHexCoord, MapTileCell> Tiles => tiles;

        /// <summary>地图宽度。</summary>
        public int Width => mapWidth;

        /// <summary>地图高度。</summary>
        public int Height => mapHeight;

        /// <summary>当前网格形状。</summary>
        public GridShape ActiveGridShape => gridShape;

        /// <summary>六边形大小。</summary>
        public float HexSize => hexSize;

        /// <summary>六边形朝向。</summary>
        public HexOrientation Orientation => orientation;

        /// <summary>地块总数。</summary>
        public int TileCount => tiles.Count;

        /// <summary>地图边界。</summary>
        public BoundsInt Bounds => bounds;

        /// <summary>所有地块枚举。</summary>
        public IEnumerable<MapTileCell> AllTiles => tiles.Values;

        /// <summary>是否已初始化。</summary>
        public bool IsInitialized => isInitialized;

        /// <summary>海拔等级对应的世界高度步长。</summary>
        public const float ElevationWorldStep = 0.06f;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            tiles = new Dictionary<MapHexCoord, MapTileCell>();
            // TerrainLibrary 初始化已移至 MapManager

            CalculateBounds();
        }

        private void Start()
        {
            // 如果没有设置默认地形，尝试从 Resources 加载
            if (defaultTerrain == null)
            {
                defaultTerrain = Resources.Load<TerrainData>("Terrain/Plain");
                if (defaultTerrain == null)
                    defaultTerrain = Resources.Load<TerrainData>("Terrain/Forest");
            }

            // 若 MapManager 存在，由 MapManager 初始化；否则自动生成默认地图
            if (FindObjectOfType<MapManager>() == null && !isInitialized && tiles.Count == 0)
            {
                GenerateDefault();
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        #endregion

        #region Configuration Application

        /// <summary>应用 ScriptableObject 配置。</summary>
        public void ApplyFromSettings(MapGridSetting settings)
        {
            if (settings == null) return;
            mapWidth = settings.MapWidth;
            mapHeight = settings.MapHeight;
            hexSize = settings.HexSize;
            orientation = settings.Orientation;
            gridShape = settings.GridShape;
            defaultTerrain = settings.DefaultTerrain;
            CalculateBounds();
        }

        /// <summary>运行时设置尺寸（无配置时使用）。</summary>
        public void ApplyRuntimeDimensions(int width, int height)
        {
            mapWidth = width;
            mapHeight = height;
            CalculateBounds();
        }

        /// <summary>设置地块预制体。</summary>
        public void SetTilePrefab(GameObject prefab)
        {
            tilePrefab = prefab;
        }

        #endregion

        #region Generation Methods

        /// <summary>生成默认地图。</summary>
        public void GenerateDefault()
        {
            Clear();

            switch (gridShape)
            {
                case GridShape.Rectangle:
                    GenerateRectangle(mapWidth, mapHeight);
                    break;
                case GridShape.Circle:
                    GenerateCircle(Mathf.Min(mapWidth, mapHeight) / 2);
                    break;
                case GridShape.Hexagon:
                    GenerateHexagon(Mathf.Min(mapWidth, mapHeight) / 2);
                    break;
                default:
                    GenerateRectangle(mapWidth, mapHeight);
                    break;
            }

            isInitialized = true;
            OnGridGenerated?.Invoke();
        }

        /// <summary>生成矩形网格。</summary>
        public void GenerateRectangle(int width, int height)
        {
            for (int q = 0; q < width; q++)
            {
                for (int r = 0; r < height; r++)
                {
                    var coord = new MapHexCoord(q, r);
                    CreateTile(coord, null);
                }
            }
        }

        /// <summary>生成圆形网格。</summary>
        public void GenerateCircle(int radius)
        {
            var center = new MapHexCoord(0, 0);
            for (int q = -radius; q <= radius; q++)
            {
                for (int r = -radius; r <= radius; r++)
                {
                    var coord = new MapHexCoord(q, r);
                    if (center.DistanceTo(coord) <= radius)
                    {
                        CreateTile(coord, null);
                    }
                }
            }
        }

        /// <summary>生成六边形蜂巢网格。</summary>
        public void GenerateHexagon(int radius)
        {
            var center = new MapHexCoord(0, 0);

            // 始终创建中心点
            CreateTile(center, null);

            // 逐圈生成六边形环
            for (int ring = 1; ring <= radius; ring++)
            {
                var ringCoords = center.GetRing(ring);
                foreach (var coord in ringCoords)
                {
                    CreateTile(coord, null);
                }
            }
        }

        /// <summary>清除所有地块。</summary>
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

        #region Tile Creation & Removal

        /// <summary>创建单个地块。</summary>
        public MapTileCell CreateTile(MapHexCoord coord, TerrainData terrain)
        {
            if (tiles.ContainsKey(coord))
            {
                Debug.LogWarning($"[MapTerrainGrid] 坐标 {coord} 已存在地块");
                return tiles[coord];
            }

            GameObject tileObject;

            if (tilePrefab != null)
            {
                // 使用预制的预制体
                Transform parent = tileParent ?? transform;
                tileObject = Instantiate(tilePrefab, parent);
            }
            else
            {
                // 创建默认的简单地块
                tileObject = CreateDefaultTileObject(coord);
            }

            var tile = tileObject.GetComponent<MapTileCell>();
            if (tile == null)
            {
                tile = tileObject.AddComponent<MapTileCell>();
            }

            // 选择地形（优先使用传入的地形，否则根据配置随机选择）
            TerrainData selectedTerrain = terrain;
            if (selectedTerrain == null && randomTerrains != null && randomTerrains.Length > 0)
            {
                if (UnityEngine.Random.value < randomTerrainChance)
                {
                    selectedTerrain = randomTerrains[UnityEngine.Random.Range(0, randomTerrains.Length)];
                }
            }
            selectedTerrain ??= defaultTerrain;

            // 如果仍然没有地形数据，尝试直接加载
            if (selectedTerrain == null)
            {
                selectedTerrain = Resources.Load<TerrainData>("Terrain/Plain");
                if (selectedTerrain == null)
                    selectedTerrain = Resources.Load<TerrainData>("Terrain/Forest");
            }

            if (selectedTerrain == null)
            {
                Debug.LogError($"[MapTerrainGrid] 无法加载任何地形数据！请确保 Resources/Terrain/ 文件夹中有地形资源。");
            }

            // 初始化并定位
            tile.Initialize(coord, selectedTerrain);
            ApplyTileWorldPosition(coord, tile, tileObject.transform);

            tiles[coord] = tile;
            OnTileCreated?.Invoke(tile);

            return tile;
        }

        /// <summary>创建默认的简单地块对象（用于测试）。</summary>
        private GameObject CreateDefaultTileObject(MapHexCoord coord)
        {
            Transform parent = tileParent ?? transform;
            var go = new GameObject($"Tile_{coord.Q}_{coord.R}");
            go.transform.SetParent(parent);

            // 创建六边形网格
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateHexMesh();

            // 添加渲染器
            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.7f, 0.8f, 0.4f, 1f) // 默认绿色
            };

            // 添加碰撞器用于射线检测
            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = meshFilter.mesh;

            // 设置标签
            go.tag = "Tile";
            go.layer = LayerMask.NameToLayer("Default");

            return go;
        }

        /// <summary>创建带厚度的六边形网格（pointy-top 正六边形）。</summary>
        private Mesh CreateHexMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "HexTile_Thick";

            // 使用内切圆半径（apothem）作为六边形的"大小"基准
            float a = hexSize; // 内切圆半径 = 中心到边的距离
            float r = a * 2f / Mathf.Sqrt(3); // 外接圆半径 = 中心到顶点的距离
            float w = a; // 半边宽 = 内切圆半径
            float thickness = 0.2f; // 地块厚度（世界单位）

            // 14个顶点：上面7个 + 下面7个（中心+6个角）
            Vector3[] vertices = new Vector3[14];
            
            // 上面（Y = thickness/2）
            vertices[0] = new Vector3(0, thickness/2, 0);           // 中心 [0]
            vertices[1] = new Vector3(0, thickness/2, r);          // 顶部 [1]
            vertices[2] = new Vector3(w, thickness/2, r * 0.5f);   // 右上 [2]
            vertices[3] = new Vector3(w, thickness/2, -r * 0.5f); // 右下 [3]
            vertices[4] = new Vector3(0, thickness/2, -r);        // 底部 [4]
            vertices[5] = new Vector3(-w, thickness/2, -r * 0.5f); // 左下 [5]
            vertices[6] = new Vector3(-w, thickness/2, r * 0.5f);  // 左上 [6]
            
            // 下面（Y = -thickness/2）
            vertices[7] = new Vector3(0, -thickness/2, 0);          // 中心 [7]
            vertices[8] = new Vector3(0, -thickness/2, r);          // 顶部 [8]
            vertices[9] = new Vector3(w, -thickness/2, r * 0.5f);   // 右上 [9]
            vertices[10] = new Vector3(w, -thickness/2, -r * 0.5f);// 右下 [10]
            vertices[11] = new Vector3(0, -thickness/2, -r);       // 底部 [11]
            vertices[12] = new Vector3(-w, -thickness/2, -r * 0.5f);// 左下 [12]
            vertices[13] = new Vector3(-w, -thickness/2, r * 0.5f); // 左上 [13]

            // 三角形索引：上面6个 + 下面6个 + 侧面6个边
            int[] triangles = new int[144];
            int triIdx = 0;
            
            // 上面（顺时针）
            triangles[triIdx++] = 0; triangles[triIdx++] = 1; triangles[triIdx++] = 2;
            triangles[triIdx++] = 0; triangles[triIdx++] = 2; triangles[triIdx++] = 3;
            triangles[triIdx++] = 0; triangles[triIdx++] = 3; triangles[triIdx++] = 4;
            triangles[triIdx++] = 0; triangles[triIdx++] = 4; triangles[triIdx++] = 5;
            triangles[triIdx++] = 0; triangles[triIdx++] = 5; triangles[triIdx++] = 6;
            triangles[triIdx++] = 0; triangles[triIdx++] = 6; triangles[triIdx++] = 1;
            
            // 下面（逆时针，从下面看是正面）
            triangles[triIdx++] = 7; triangles[triIdx++] = 9; triangles[triIdx++] = 8;
            triangles[triIdx++] = 7; triangles[triIdx++] = 10; triangles[triIdx++] = 9;
            triangles[triIdx++] = 7; triangles[triIdx++] = 11; triangles[triIdx++] = 10;
            triangles[triIdx++] = 7; triangles[triIdx++] = 12; triangles[triIdx++] = 11;
            triangles[triIdx++] = 7; triangles[triIdx++] = 13; triangles[triIdx++] = 12;
            triangles[triIdx++] = 7; triangles[triIdx++] = 8; triangles[triIdx++] = 13;
            
            // 侧面6个矩形（每个分成2个三角形）
            // 边1: 顶-右上 (1-2 -> 8-9)
            triangles[triIdx++] = 1; triangles[triIdx++] = 8; triangles[triIdx++] = 9;
            triangles[triIdx++] = 1; triangles[triIdx++] = 9; triangles[triIdx++] = 2;
            // 边2: 右上-右下 (2-3 -> 9-10)
            triangles[triIdx++] = 2; triangles[triIdx++] = 9; triangles[triIdx++] = 10;
            triangles[triIdx++] = 2; triangles[triIdx++] = 10; triangles[triIdx++] = 3;
            // 边3: 右下-底部 (3-4 -> 10-11)
            triangles[triIdx++] = 3; triangles[triIdx++] = 10; triangles[triIdx++] = 11;
            triangles[triIdx++] = 3; triangles[triIdx++] = 11; triangles[triIdx++] = 4;
            // 边4: 底部-左下 (4-5 -> 11-12)
            triangles[triIdx++] = 4; triangles[triIdx++] = 11; triangles[triIdx++] = 12;
            triangles[triIdx++] = 4; triangles[triIdx++] = 12; triangles[triIdx++] = 5;
            // 边5: 左下-左上 (5-6 -> 12-13)
            triangles[triIdx++] = 5; triangles[triIdx++] = 12; triangles[triIdx++] = 13;
            triangles[triIdx++] = 5; triangles[triIdx++] = 13; triangles[triIdx++] = 6;
            // 边6: 左上-顶 (6-1 -> 13-8)
            triangles[triIdx++] = 6; triangles[triIdx++] = 13; triangles[triIdx++] = 8;
            triangles[triIdx++] = 6; triangles[triIdx++] = 8; triangles[triIdx++] = 1;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>移除指定地块。</summary>
        public void RemoveTile(MapHexCoord coord)
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

        /// <summary>修改指定坐标的地形。</summary>
        public void SetTerrain(MapHexCoord coord, TerrainData terrain)
        {
            if (tiles.TryGetValue(coord, out var tile))
            {
                tile.ApplyTerrain(terrain);
            }
            else
            {
                CreateTile(coord, terrain);
            }
        }

        /// <summary>按当前坐标与地块海拔，刷新地块世界位置。</summary>
        public void RefreshTileWorldPosition(MapHexCoord coord)
        {
            if (!tiles.TryGetValue(coord, out var tile) || tile == null)
                return;
            ApplyTileWorldPosition(coord, tile, tile.transform);
        }

        void ApplyTileWorldPosition(MapHexCoord coord, MapTileCell tile, Transform tr)
        {
            if (tr == null) return;
            Vector3 basePos = CoordToWorldPosition(coord);
            float elevation = tile != null ? tile.ElevationLevel * ElevationWorldStep : 0f;
            tr.position = basePos + Vector3.up * elevation;
        }

        #endregion

        #region Query Methods

        /// <summary>获取指定坐标的地块。</summary>
        public MapTileCell GetTile(MapHexCoord coord)
        {
            tiles.TryGetValue(coord, out var tile);
            return tile;
        }

        /// <summary>安全获取地块。</summary>
        public bool TryGetTile(MapHexCoord coord, out MapTileCell tile)
        {
            return tiles.TryGetValue(coord, out tile);
        }

        /// <summary>检查坐标是否存在地块。</summary>
        public bool HasTile(MapHexCoord coord)
        {
            return tiles.ContainsKey(coord);
        }

        /// <summary>获取范围内的所有地块。</summary>
        public MapTileCell[] GetTilesInRange(MapHexCoord center, int range)
        {
            var coords = center.GetSpiral(range);
            var result = new List<MapTileCell>();

            foreach (var coord in coords)
            {
                if (tiles.TryGetValue(coord, out var tile))
                    result.Add(tile);
            }

            return result.ToArray();
        }

        /// <summary>获取环上的地块。</summary>
        public MapTileCell[] GetTilesInRing(MapHexCoord center, int radius)
        {
            var coords = center.GetRing(radius);
            var result = new List<MapTileCell>();

            foreach (var coord in coords)
            {
                if (tiles.TryGetValue(coord, out var tile))
                    result.Add(tile);
            }

            return result.ToArray();
        }

        /// <summary>获取所有地块数组。</summary>
        public MapTileCell[] GetAllTiles()
        {
            return tiles.Values.ToArray();
        }

        /// <summary>获取邻接地块（只返回存在的）。</summary>
        public MapTileCell[] GetNeighbors(MapHexCoord coord)
        {
            var neighborCoords = coord.GetNeighbors();
            var result = new List<MapTileCell>();

            foreach (var neighborCoord in neighborCoords)
            {
                if (tiles.TryGetValue(neighborCoord, out var tile))
                    result.Add(tile);
            }

            return result.ToArray();
        }

        #endregion

        #region IMapDataProvider Implementation

        MapTileCell IMapDataProvider.GetTile(MapHexCoord coord) => GetTile(coord);

        MapTileCell[] IMapDataProvider.GetTilesInRange(MapHexCoord center, int range) => GetTilesInRange(center, range);

        BoundsInt IMapDataProvider.GetMapBounds() => Bounds;

        #endregion

        #region ITerrainQuery Implementation

        float ITerrainQuery.GetMovementCost(MapHexCoord coord)
        {
            var tile = GetTile(coord);
            return tile?.MovementCost ?? 1f;
        }

        float ITerrainQuery.GetDefenseBonus(MapHexCoord coord)
        {
            var tile = GetTile(coord);
            return tile?.DefenseBonus ?? 0f;
        }

        float ITerrainQuery.GetVisibilityModifier(MapHexCoord coord)
        {
            var tile = GetTile(coord);
            return tile?.VisibilityModifier ?? 0f;
        }

        bool ITerrainQuery.IsPassable(MapHexCoord coord)
        {
            var tile = GetTile(coord);
            return tile?.IsPassable ?? false;
        }

        #endregion

        #region Coordinate Conversion

        /// <summary>轴向坐标转世界坐标（pointy-top 六边形紧密排列）。</summary>
        public Vector3 CoordToWorldPosition(MapHexCoord coord)
        {
            float x, z;
            float size = hexSize;

            if (orientation == HexOrientation.PointyTop)
            {
                // Pointy-top: 顶点朝上
                // 水平间距 = sqrt(3) * size，垂直间距 = 1.5 * size
                // 每行交替偏移 sqrt(3)/2 * size
                x = size * (Mathf.Sqrt(3) * coord.Q + Mathf.Sqrt(3) / 2f * coord.R);
                z = size * 1.5f * coord.R;
            }
            else
            {
                // Flat-top: 平顶朝上
                x = size * 1.5f * coord.Q;
                z = size * (Mathf.Sqrt(3) * coord.R + Mathf.Sqrt(3) / 2f * coord.Q);
            }

            return new Vector3(x, 0, z) + transform.position;
        }

        /// <summary>世界坐标转轴向坐标（六边形）。</summary>
        public MapHexCoord WorldPositionToCoord(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - transform.position;
            float size = hexSize;
            float q, r;

            if (orientation == HexOrientation.PointyTop)
            {
                q = (Mathf.Sqrt(3) / 3f * localPos.x - 1f / 3f * localPos.z) / size;
                r = (2f / 3f * localPos.z) / size;
            }
            else
            {
                q = (2f / 3f * localPos.x) / size;
                r = (Mathf.Sqrt(3) / 3f * localPos.z - 1f / 3f * localPos.x) / size;
            }

            return MapHexCoord.FromAxial(Mathf.RoundToInt(q), Mathf.RoundToInt(r));
        }

        #endregion

        #region Private Helpers

        void CalculateBounds()
        {
            bounds = new BoundsInt(0, 0, 0, mapWidth, mapHeight, 1);
        }

        Transform GetOrCreateTerrainRoot()
        {
            if (tileParent != null) return tileParent;

            var root = transform.Find("TerrainTiles");
            if (root == null)
            {
                var go = new GameObject("TerrainTiles");
                go.transform.SetParent(transform, false);
                root = go.transform;
            }
            return root;
        }

        #endregion
    }
}
