using System;
using System.Collections.Generic;
using TBS.Map.Data;
using TBS.Map.Rendering;
using TBS.Map.Runtime;
using TBS.Map.Tools;
using UnityEngine;

using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Managers
{
    /// <summary>
    /// 地图系统总控（见《地图系统设计》）：加载配置、协调网格与渲染层、对外提供查询入口。
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [Header("数据配置（可选 ScriptableObject）")]
        [SerializeField] private MapGridSetting defaultGridSettings;
        [SerializeField] private MapRouteSetting routeLinks;

        [Header("场景引用")]
        [SerializeField] private MapTerrainGrid terrainGrid;
        [SerializeField] private MapRenderer groundRenderer;
        [SerializeField] private MapRouteRenderer routeOverlayRenderer;
        [SerializeField] private MapVegetationRenderer vegetationRenderer;

        [Header("无配置文件时的 fallback 尺寸")]
        [SerializeField] private int fallbackWidth = 20;
        [SerializeField] private int fallbackHeight = 15;

        /// <summary>网格访问器（外部只读）。</summary>
        public MapTerrainGrid Terrain => terrainGrid;

        /// <summary>当前地块字典视图（只读）。</summary>
        public IReadOnlyDictionary<MapHexCoord, MapTileCell> Tiles =>
            terrainGrid != null ? terrainGrid.Tiles : EmptyTiles;

        static readonly IReadOnlyDictionary<MapHexCoord, MapTileCell> EmptyTiles =
            new Dictionary<MapHexCoord, MapTileCell>();

        #region Lifecycle

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 自动查找或创建 MapTerrainGrid
            terrainGrid ??= FindObjectOfType<MapTerrainGrid>();
            if (terrainGrid == null)
            {
                var go = new GameObject("MapTerrainGrid");
                go.transform.SetParent(transform, false);
                terrainGrid = go.AddComponent<MapTerrainGrid>();
            }

            // 自动查找子节点中的渲染器（允许场景预制体挂载）
            groundRenderer ??= GetComponentInChildren<MapRenderer>(true);
            routeOverlayRenderer ??= GetComponentInChildren<MapRouteRenderer>(true);
            vegetationRenderer ??= GetComponentInChildren<MapVegetationRenderer>(true);
        }

        void Start()
        {
            // 如果没有配置，尝试从 Resources 加载
            if (defaultGridSettings == null)
            {
                defaultGridSettings = Resources.Load<MapGridSetting>("MapConfigs/TestGridSetting");
                if (defaultGridSettings != null)
                    Debug.Log("[MapManager] 自动加载网格配置: TestGridSetting");
            }

            if (routeLinks == null)
            {
                routeLinks = Resources.Load<MapRouteSetting>("MapConfigs/TestRouteSetting");
                if (routeLinks != null)
                    Debug.Log("[MapManager] 自动加载道路配置: TestRouteSetting");
            }

            // 延迟到 Start 确保所有组件都准备好了再初始化地图
            InitializeMap();
            Debug.Log($"[MapManager] 地图初始化完成: {Tiles.Count} 个地块");
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 对外统一入口：优先使用 <see cref="defaultGridSettings"/>，否则走 fallback 尺寸。
        /// </summary>
        public void InitializeMap()
        {
            if (defaultGridSettings != null)
                InitializeFromSettings(defaultGridSettings);
            else
                InitializeWithFallbackDimensions();
        }

        /// <summary>
        /// 使用 ScriptableObject 配置初始化网格并通知渲染层。
        /// </summary>
        public void InitializeFromSettings(MapGridSetting settings)
        {
            if (terrainGrid == null) return;
            if (settings != null)
            {
                terrainGrid.ApplyFromSettings(settings);
                terrainGrid.Clear();
                GenerateShapeForCurrentParams();
            }
            else
                InitializeWithFallbackDimensions();

            NotifyRenderersLoaded();
        }

        /// <summary>无配置文件时，使用 Inspector 上的 fallback 尺寸。</summary>
        public void InitializeWithFallbackDimensions()
        {
            if (terrainGrid == null) return;
            terrainGrid.ApplyRuntimeDimensions(fallbackWidth, fallbackHeight);
            terrainGrid.Clear();
            terrainGrid.GenerateDefault();
            NotifyRenderersLoaded();
        }

        void GenerateShapeForCurrentParams()
        {
            switch (terrainGrid.ActiveGridShape)
            {
                case GridShape.Rectangle:
                    terrainGrid.GenerateRectangle(terrainGrid.Width, terrainGrid.Height);
                    break;
                case GridShape.Circle:
                    terrainGrid.GenerateCircle(Mathf.Min(terrainGrid.Width, terrainGrid.Height) / 2);
                    break;
                case GridShape.Hexagon:
                    terrainGrid.GenerateHexagon(Mathf.Min(terrainGrid.Width, terrainGrid.Height) / 2);
                    break;
                default:
                    terrainGrid.GenerateRectangle(terrainGrid.Width, terrainGrid.Height);
                    break;
            }
        }

        void NotifyRenderersLoaded()
        {
            groundRenderer?.OnMapLoaded(terrainGrid);
            routeOverlayRenderer?.OnMapLoaded(routeLinks, terrainGrid);
            vegetationRenderer?.OnMapLoaded(terrainGrid);
        }

        #endregion

        #region Query API

        /// <summary>获取指定坐标的地块。</summary>
        public MapTileCell GetTile(MapHexCoord coord)
        {
            return terrainGrid?.GetTile(coord);
        }

        /// <summary>给定世界坐标，返回对应的地块（若存在）。</summary>
        public MapTileCell GetTileFromWorldPosition(Vector3 worldPos)
        {
            if (terrainGrid == null) return null;
            MapHexCoord coord = terrainGrid.WorldPositionToCoord(worldPos);
            return terrainGrid.GetTile(coord);
        }

        /// <summary>给定世界坐标，返回地形类型ID（无地块返回 "void"）。</summary>
        public string GetTerrainTypeAt(Vector3 worldPos)
        {
            var tile = GetTileFromWorldPosition(worldPos);
            return tile?.TerrainId ?? "void";
        }

        /// <summary>两点之间是否有河流/道路连接。</summary>
        public bool HasRoute(MapHexCoord from, MapHexCoord to, out MapRouteType kind)
        {
            kind = default;
            if (routeLinks == null) return false;

                foreach (var link in routeLinks.Links)
            {
                bool match = (link.From == from && link.To == to) ||
                             (link.From == to && link.To == from);
                if (match)
                {
                    kind = link.Type;
                    return true;
                }
            }
            return false;
        }

        /// <summary>检查指定连接是否可通行。</summary>
        public bool IsRoutePassable(MapHexCoord from, MapHexCoord to)
        {
            if (routeLinks == null) return false;

            foreach (var link in routeLinks.Links)
            {
                bool match = (link.From == from && link.To == to) ||
                             (link.From == to && link.To == from);
                if (match) return link.Passable;
            }
            return false;
        }

        /// <summary>获取所有与指定坐标相连的路网。</summary>
        public IReadOnlyList<MapLinkType> GetRoutesFrom(MapHexCoord coord)
        {
            var list = new List<MapLinkType>();
            if (routeLinks == null) return list;

            foreach (var link in routeLinks.Links)
            {
                if (link.From == coord || link.To == coord)
                    list.Add(link);
            }
            return list;
        }

        #endregion

        #region Mutation API

        /// <summary>修改指定坐标的地形。</summary>
        public void SetTileTerrain(MapHexCoord coord, TerrainData terrain)
        {
            terrainGrid?.SetTerrain(coord, terrain);
            groundRenderer?.OnCellTerrainChanged(coord);
            // 植被可能因地形变化而需要刷新（如森林->荒地）
            vegetationRenderer?.RefreshRegion(coord, 0);
        }

        /// <summary>批量修改地形（优化：单次通知渲染器）。</summary>
        public void SetTileTerrains(IReadOnlyList<(MapHexCoord coord, TerrainData terrain)> changes)
        {
            if (terrainGrid == null) return;

            foreach (var (coord, terrain) in changes)
                terrainGrid.SetTerrain(coord, terrain);

            // 全量刷新（后续可优化为仅刷新受影响批次）
            groundRenderer?.OnMapLoaded(terrainGrid);
            vegetationRenderer?.OnMapLoaded(terrainGrid);
        }

        /// <summary>设置地块海拔等级（视觉偏移）。</summary>
        public void SetTileElevation(MapHexCoord coord, int elevationLevel)
        {
            var tile = terrainGrid?.GetTile(coord);
            if (tile != null)
            {
                tile.ElevationLevel = elevationLevel;
                terrainGrid?.RefreshTileWorldPosition(coord);
                groundRenderer?.OnCellTerrainChanged(coord);
            }
        }

        /// <summary>设置地块植被密度（0~1）。</summary>
        public void SetTileVegetationDensity(MapHexCoord coord, float density)
        {
            var tile = terrainGrid?.GetTile(coord);
            if (tile != null)
            {
                tile.VegetationDensity = density;
                vegetationRenderer?.RefreshRegion(coord, 0);
            }
        }

        /// <summary>添加一条河流/道路连接。</summary>
        public void AddRouteLink(MapHexCoord from, MapHexCoord to, MapRouteType type,
                                 float widthWorld = 0.3f, bool passable = true)
        {
            if (routeLinks == null) return;

            // 简单检查是否已存在
            foreach (var existing in routeLinks.Links)
            {
                bool same = (existing.From == from && existing.To == to) ||
                            (existing.From == to && existing.To == from);
                if (same) return; // 已存在则不重复添加
            }

            // 由于 MapRouteSetting 是 ScriptableObject，这里假设通过 Inspector 或资源编辑
            // 运行时添加仅做示例：需自行实现动态容器或改用 ScriptableObject 的 List.Add
            // routeLinks.Add(new MapLinkType { From = from, To = to, Type = type, ... });

            // 通知渲染层重建
            routeOverlayRenderer?.RebuildLinks(routeLinks);
        }

        /// <summary>移除一条连接（如炸桥）。</summary>
        public void RemoveRouteLink(MapHexCoord from, MapHexCoord to)
        {
            if (routeLinks == null) return;

            // 同 AddRouteLink，需动态容器支持
            // routeLinks.Remove(...);

            routeOverlayRenderer?.RebuildLinks(routeLinks);
        }

        #endregion

        #region Camera Helpers

        /// <summary>计算相机应聚焦的世界坐标与正交尺寸。</summary>
        public void GetFocusForTile(MapHexCoord coord, out Vector3 worldPos, out float orthoSize)
        {
            if (terrainGrid == null)
            {
                worldPos = Vector3.zero;
                orthoSize = 10f;
                return;
            }

            worldPos = terrainGrid.CoordToWorldPosition(coord);
            // 根据网格大小计算合适的正交尺寸
            float maxDim = Mathf.Max(terrainGrid.Width, terrainGrid.Height);
            orthoSize = Mathf.Clamp(maxDim * 0.5f, 5f, 50f);
        }

        #endregion
    }
}
