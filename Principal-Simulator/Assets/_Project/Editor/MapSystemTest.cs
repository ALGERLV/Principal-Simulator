using System.Collections.Generic;
using TBS.Map.Components;
using TBS.Map.Data;
using TBS.Map.Runtime;
using TBS.Map.Tools;
using UnityEngine;

// 明确指定使用我们的TerrainData，避免与UnityEngine.TerrainData冲突
using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Test
{
    /// <summary>
    /// 地图系统测试组件 - 在运行时验证地图功能
    /// </summary>
    public class MapSystemTest : MonoBehaviour
    {
        [Header("测试参数")]
        [SerializeField] private bool autoGenerateOnStart = true;
        [SerializeField] private int testMapWidth = 8;
        [SerializeField] private int testMapHeight = 6;
        [SerializeField] private GridShape testShape = GridShape.Rectangle;

        [Header("引用")]
        [SerializeField] private MapTerrainGrid hexGrid;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private TerrainData defaultTerrain;

        [Header("测试地形")]
        [SerializeField] private TerrainData[] testTerrains;

        private void Start()
        {
            if (autoGenerateOnStart)
            {
                InitializeAndGenerate();
            }
        }

        /// <summary>
        /// 初始化并生成地图
        /// </summary>
        public void InitializeAndGenerate()
        {
            // 查找或创建HexGrid
            if (hexGrid == null)
            {
                hexGrid = FindObjectOfType<MapTerrainGrid>();
                if (hexGrid == null)
                {
                    CreateHexGrid();
                }
            }

            // 设置参数
            hexGrid.GetType().GetField("mapWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(hexGrid, testMapWidth);
            hexGrid.GetType().GetField("mapHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(hexGrid, testMapHeight);

            // 生成地图
            GenerateTestMap();

            // 运行测试
            RunTests();
        }

        /// <summary>
        /// 创建HexGrid对象
        /// </summary>
        private void CreateHexGrid()
        {
            GameObject gridGO = new GameObject("MapTerrainGrid");
            hexGrid = gridGO.AddComponent<MapTerrainGrid>();

            // 通过反射设置私有字段
            var type = hexGrid.GetType();
            type.GetField("tilePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(hexGrid, tilePrefab);
            type.GetField("defaultTerrain", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(hexGrid, defaultTerrain);

            Debug.Log("HexGrid已创建");
        }

        /// <summary>
        /// 生成测试地图
        /// </summary>
        private void GenerateTestMap()
        {
            switch (testShape)
            {
                case GridShape.Rectangle:
                    hexGrid.GenerateRectangle(testMapWidth, testMapHeight);
                    break;
                case GridShape.Circle:
                    hexGrid.GenerateCircle(Mathf.Min(testMapWidth, testMapHeight) / 2);
                    break;
                case GridShape.Hexagon:
                    hexGrid.GenerateHexagon(Mathf.Min(testMapWidth, testMapHeight) / 2);
                    break;
            }

            // 如果有多种地形，随机设置一些特殊地形
            if (testTerrains != null && testTerrains.Length > 0)
            {
                ApplyRandomTerrain();
            }

            Debug.Log($"测试地图生成完成: {hexGrid.TileCount}个地块");
        }

        /// <summary>
        /// 随机应用地形
        /// </summary>
        private void ApplyRandomTerrain()
        {
            var tiles = hexGrid.GetAllTiles();
            foreach (var tile in tiles)
            {
                // 随机选择地形（20%几率不是默认地形）
                if (Random.value < 0.2f && testTerrains.Length > 0)
                {
                    int randomIndex = Random.Range(0, testTerrains.Length);
                    tile.GetType().GetField("terrainData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(tile, testTerrains[randomIndex]);

                    // 更新渲染
                    var renderer = tile.GetComponent<HexTileRenderer>();
                    if (renderer != null)
                    {
                        renderer.UpdateVisuals();
                    }
                }
            }
        }

        /// <summary>
        /// 运行功能测试
        /// </summary>
        private void RunTests()
        {
            Debug.Log("========== 地图系统测试开始 ==========");

            // 测试1: 坐标系统
            TestCoordinateSystem();

            // 测试2: 地图查询
            TestMapQueries();

            // 测试3: 地形效果
            TestTerrainEffects();

            // 测试4: 范围查询
            TestRangeQueries();

            Debug.Log("========== 地图系统测试完成 ==========");
        }

        /// <summary>
        /// 测试坐标系统
        /// </summary>
        private void TestCoordinateSystem()
        {
            Debug.Log("[测试1] 坐标系统测试");

            // 测试坐标创建
            var coord1 = new MapHexCoord(3, 4);
            var coord2 = MapHexCoord.FromAxial(3, 4);
            Debug.Log($"  坐标创建: {coord1} = {coord2} ? {coord1 == coord2}");

            // 测试邻接坐标
            var neighbors = coord1.GetNeighbors();
            Debug.Log($"  邻接坐标数量: {neighbors.Length} (预期: 6)");

            // 测试距离计算
            var coord3 = new MapHexCoord(0, 0);
            int distance = coord3.DistanceTo(coord1);
            Debug.Log($"  距离计算: (0,0) 到 (3,4) = {distance}");

            // 测试范围查询
            var spiral = coord3.GetSpiral(2);
            Debug.Log($"  半径2范围内坐标数: {spiral.Length}");
        }

        /// <summary>
        /// 测试地图查询
        /// </summary>
        private void TestMapQueries()
        {
            Debug.Log("[测试2] 地图查询测试");

            // 测试获取地块
            var center = new MapHexCoord(0, 0);
            var tile = hexGrid.GetTile(center);
            Debug.Log($"  中心地块: {tile}");

            // 测试邻接地块
            var neighbors = hexGrid.GetNeighbors(center);
            Debug.Log($"  中心邻接地块数: {neighbors.Length}");

            // 测试地图边界
            var bounds = hexGrid.Bounds;
            Debug.Log($"  地图边界: {bounds}");

            // 测试地块总数
            Debug.Log($"  地块总数: {hexGrid.TileCount}");
        }

        /// <summary>
        /// 测试地形效果
        /// </summary>
        private void TestTerrainEffects()
        {
            Debug.Log("[测试3] 地形效果测试");

            var tiles = hexGrid.GetAllTiles();
            int testCount = Mathf.Min(3, tiles.Length);

            for (int i = 0; i < testCount; i++)
            {
                var tile = tiles[i];
                Debug.Log($"  地块 {tile.Coord}:");
                Debug.Log($"    地形: {tile.TerrainName}");
                Debug.Log($"    移动消耗: {tile.MovementCost}");
                Debug.Log($"    防御加成: {tile.DefenseBonus}");
                Debug.Log($"    视野修正: {tile.VisibilityModifier}");
            }
        }

        /// <summary>
        /// 测试范围查询
        /// </summary>
        private void TestRangeQueries()
        {
            Debug.Log("[测试4] 范围查询测试");

            var center = new MapHexCoord(0, 0);

            // 测试不同范围
            for (int range = 1; range <= 3; range++)
            {
                var tilesInRange = hexGrid.GetTilesInRange(center, range);
                Debug.Log($"  范围{range}: {tilesInRange.Length}个地块");
            }

            // 测试环形查询
            var ringTiles = hexGrid.GetTilesInRing(center, 2);
            Debug.Log($"  半径2的环: {ringTiles.Length}个地块");
        }

        /// <summary>
        /// 可视化高亮测试
        /// </summary>
        [ContextMenu("高亮测试")]
        public void HighlightTest()
        {
            var center = new MapHexCoord(0, 0);
            var tilesInRange = hexGrid.GetTilesInRange(center, 2);

            foreach (var tile in tilesInRange)
            {
                var renderer = tile.GetComponent<HexTileRenderer>();
                if (renderer != null)
                {
                    renderer.Highlight(Color.yellow);
                }
            }

            Debug.Log($"已高亮 {tilesInRange.Length} 个地块");
        }

        /// <summary>
        /// 清除高亮
        /// </summary>
        [ContextMenu("清除高亮")]
        public void ClearHighlight()
        {
            var tiles = hexGrid.GetAllTiles();
            foreach (var tile in tiles)
            {
                var renderer = tile.GetComponent<HexTileRenderer>();
                if (renderer != null)
                {
                    renderer.ResetColor();
                }
            }
        }
    }
}
