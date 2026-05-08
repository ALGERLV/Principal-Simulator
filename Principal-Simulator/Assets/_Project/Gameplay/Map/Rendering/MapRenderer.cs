using System.Collections.Generic;
using TBS.Map.Runtime;
using TBS.Map.Tools;

using TerrainData = TBS.Map.Data.TerrainData;
using UnityEngine;

namespace TBS.Map.Rendering
{
    /// <summary>
    /// 地图系统设计 4.1：地块批次绘制 / GPU 实例化渲染器。
    /// </summary>
    public class MapRenderer : MonoBehaviour
    {
        [Header("渲染配置")]
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private Mesh tileMesh;

        private MapTerrainGrid currentGrid;
        private Dictionary<string, List<Matrix4x4>> batchTransforms = new Dictionary<string, List<Matrix4x4>>();
        private Dictionary<string, List<Vector4>> batchColors = new Dictionary<string, List<Vector4>>();

        /// <summary>地图加载完成时由 MapManager 调用。</summary>
        public void OnMapLoaded(MapTerrainGrid grid)
        {
            if (grid == null)
            {
                Debug.LogWarning("[MapRenderer] OnMapLoaded 传入的 grid 为 null");
                return;
            }
            currentGrid = grid;
            Debug.Log($"[MapRenderer] 地图加载完成，地块数: {grid.TileCount}");
            RebuildAllBatches();
        }

        /// <summary>单个地块地形变化时调用。</summary>
        public void OnCellTerrainChanged(MapHexCoord coord)
        {
            // 简化处理：全量重建（后续可优化为局部批次更新）
            RebuildAllBatches();
        }

        /// <summary>强制重建所有批次。</summary>
        public void RebuildAllBatches()
        {
            batchTransforms.Clear();
            batchColors.Clear();

            if (currentGrid == null)
            {
                Debug.LogWarning("[MapRenderer] RebuildAllBatches: currentGrid 为 null");
                return;
            }

            Debug.Log($"[MapRenderer] 重建批次，地块数: {currentGrid.TileCount}");

            // 按地形类型分组收集变换矩阵和颜色
            foreach (var tile in currentGrid.AllTiles)
            {
                if (tile == null) continue;

                string terrainId = tile.TerrainId;
                if (!batchTransforms.ContainsKey(terrainId))
                {
                    batchTransforms[terrainId] = new List<Matrix4x4>();
                    batchColors[terrainId] = new List<Vector4>();
                }

                // 构建变换矩阵（位置 + 海拔偏移 + 缩放）
                Vector3 worldPos = currentGrid.CoordToWorldPosition(tile.Coord);
                worldPos.y += tile.ElevationLevel * MapTerrainGrid.ElevationWorldStep;

                Quaternion rotation = Quaternion.identity;
                // HexTile.asset 的外接圆半径为 1
                // 缩放后外接圆半径 = hexSize，实现紧密排列
                Vector3 scale = Vector3.one * currentGrid.HexSize;

                Matrix4x4 matrix = Matrix4x4.TRS(worldPos, rotation, scale);
                batchTransforms[terrainId].Add(matrix);

                // 收集颜色（地形色调 + 海拔影响）
                Color tint = tile.TerrainData?.TerrainColor ?? Color.white;
                batchColors[terrainId].Add(new Vector4(tint.r, tint.g, tint.b, tint.a));
            }
        }

        void Update()
        {
            // 使用 GPU Instancing 绘制所有批次
            if (tileMesh == null || terrainMaterial == null)
            {
                Debug.LogWarning($"[MapRenderer] 无法绘制: tileMesh={(tileMesh==null?"null":"ok")}, terrainMaterial={(terrainMaterial==null?"null":"ok")}");
                return;
            }

            if (batchTransforms.Count == 0)
            {
                Debug.LogWarning($"[MapRenderer] 无批次数据，跳过绘制");
                return;
            }

            foreach (var kvp in batchTransforms)
            {
                string terrainId = kvp.Key;
                List<Matrix4x4> matrices = kvp.Value;

                if (matrices.Count == 0) continue;

                // 准备实例化数据
                Matrix4x4[] matrixArray = matrices.ToArray();

                // 如果支持 GPU Instancing，使用 DrawMeshInstanced
                if (SystemInfo.supportsInstancing && matrixArray.Length <= 1023)
                {
                    Graphics.DrawMeshInstanced(tileMesh, 0, terrainMaterial, matrixArray);
                }
                else
                {
                    // 回退到逐个绘制
                    for (int i = 0; i < matrixArray.Length; i++)
                    {
                        Graphics.DrawMesh(tileMesh, matrixArray[i], terrainMaterial, 0);
                    }
                }
            }
        }

        /// <summary>设置渲染材质（运行时动态更换主题）。</summary>
        public void SetMaterial(Material material)
        {
            terrainMaterial = material;
        }

        /// <summary>设置地块网格模板。</summary>
        public void SetTileMesh(Mesh mesh)
        {
            tileMesh = mesh;
        }

        /// <summary>获取当前批次统计信息（调试用）。</summary>
        public string GetBatchStats()
        {
            if (batchTransforms.Count == 0) return "No batches";

            int totalInstances = 0;
            foreach (var kvp in batchTransforms)
            {
                totalInstances += kvp.Value.Count;
            }

            return $"Batches: {batchTransforms.Count}, Total Instances: {totalInstances}";
        }
    }
}
