using System.Collections.Generic;
using TBS.Map.Components;
using TBS.Map.Data;
using TBS.Map.Managers;
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
        [SerializeField] private MapManager mapManager;
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
            // 查找或创建MapManager
            if (mapManager == null)
            {
                mapManager = MapManager.Instance;
                if (mapManager == null)
                {
                    mapManager = FindObjectOfType<MapManager>();
                    if (mapManager == null)
                    {
                        Debug.LogError("[MapSystemTest] 场景中没有 MapManager，请先创建 MapManager");
                        return;
                    }
                }
            }

            // 运行测试
            RunTests();
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
            var tile = mapManager.GetTile(center);
            Debug.Log($"  中心地块: {tile}");

            // 测试地图边界
            Debug.Log($"  地块总数: {mapManager.Tiles.Count}");
        }

        /// <summary>
        /// 测试地形效果
        /// </summary>
        private void TestTerrainEffects()
        {
            Debug.Log("[测试3] 地形效果测试");

            var tiles = mapManager.Tiles;
            int testCount = Mathf.Min(3, tiles.Count);

            int index = 0;
            foreach (var tile in tiles.Values)
            {
                if (index >= testCount) break;
                Debug.Log($"  地块 {tile.Coord}:");
                Debug.Log($"    地形: {tile.TerrainName}");
                Debug.Log($"    移动消耗: {tile.MovementCost}");
                Debug.Log($"    防御加成: {tile.DefenseBonus}");
                Debug.Log($"    视野修正: {tile.VisibilityModifier}");
                index++;
            }
        }

        /// <summary>
        /// 可视化高亮测试
        /// </summary>
        [ContextMenu("高亮测试")]
        public void HighlightTest()
        {
            var center = new MapHexCoord(0, 0);
            var tiles = mapManager.Tiles;

            foreach (var tile in tiles.Values)
            {
                if (tile.Coord.DistanceTo(center) <= 2)
                {
                    var renderer = tile.GetComponent<HexTileRenderer>();
                    if (renderer != null)
                    {
                        renderer.Highlight(Color.yellow);
                    }
                }
            }

            Debug.Log($"已高亮中心范围2内的地块");
        }

        /// <summary>
        /// 清除高亮
        /// </summary>
        [ContextMenu("清除高亮")]
        public void ClearHighlight()
        {
            var tiles = mapManager.Tiles;
            foreach (var tile in tiles.Values)
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
