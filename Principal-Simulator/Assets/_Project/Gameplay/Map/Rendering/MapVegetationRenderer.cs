using System.Collections.Generic;
using TBS.Map.Managers;
using TBS.Map.Runtime;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.Rendering
{
    /// <summary>
    /// 地图系统设计 4.3：植被 GPU 实例化渲染器。
    /// </summary>
    public class MapVegetationRenderer : MonoBehaviour
    {
        [Header("植被配置")]
        [SerializeField] private Mesh vegetationMesh;
        [SerializeField] private Material vegetationMaterial;

        [Header("密度与范围")]
        [SerializeField, Tooltip("每个地块的基础植被数量")]
        private int baseInstancesPerTile = 5;

        [SerializeField, Tooltip("最大可见距离（世界单位）")]
        private float maxViewDistance = 50f;

        [SerializeField, Tooltip("LOD 切换距离")]
        private float lodDistance = 30f;

        [Header("尺寸随机")]
        [SerializeField] private float minScale = 0.3f;
        [SerializeField] private float maxScale = 0.6f;
        [SerializeField, Tooltip("高度缩放因子，用于压扁植被")] 
        private float heightScale = 0.3f;

        private MapManager currentManager;
        private Camera mainCamera;

        // 实例数据缓冲区
        private List<Matrix4x4> instanceMatrices = new List<Matrix4x4>();
        private List<Vector4> instanceColors = new List<Vector4>();

        // 植被类型（简单的颜色区分）
        private static readonly Color[] VegetationTypes = new Color[]
        {
            new Color(0.2f, 0.6f, 0.2f),  // 深绿（树木）
            new Color(0.4f, 0.7f, 0.3f),  // 浅绿（灌木）
            new Color(0.6f, 0.5f, 0.2f),  // 枯黄（草丛）
        };

        /// <summary>地图加载时由 MapManager 调用。</summary>
        public void OnMapLoaded(MapManager manager)
        {
            currentManager = manager;
            mainCamera = Camera.main;
        }

        /// <summary>刷新指定区域的植被。</summary>
        public void RefreshRegion(MapHexCoord center, int radius)
        {
            // 简化处理：下一帧会自动重新计算可见区域
        }

        void Update()
        {
            if (currentManager == null || mainCamera == null) return;
            if (vegetationMesh == null || vegetationMaterial == null) return;

            // 获取相机位置
            Vector3 cameraPos = mainCamera.transform.position;

            // 清空上一帧的实例数据
            instanceMatrices.Clear();
            instanceColors.Clear();

            // 遍历所有地块，收集可见且需要植被的实例
            foreach (var tile in currentManager.Tiles.Values)
            {
                if (tile == null) continue;

                // 根据植被密度决定是否生成植被
                float density = tile.VegetationDensity;
                if (density <= 0.01f) continue;

                Vector3 tileCenter = currentManager.CoordToWorldPosition(tile.Coord);

                // 距离检查
                float distance = Vector3.Distance(cameraPos, tileCenter);
                if (distance > maxViewDistance) continue;

                // LOD：远距离减少实例数量
                int instanceCount = Mathf.RoundToInt(baseInstancesPerTile * density);
                if (distance > lodDistance)
                {
                    instanceCount = Mathf.Max(1, instanceCount / 2);
                }

                // 生成随机分布的植被实例
                GenerateTileVegetation(tile, tileCenter, instanceCount);
            }

            // GPU Instancing 绘制
            if (instanceMatrices.Count > 0)
            {
                DrawVegetationInstanced();
            }
        }

        void GenerateTileVegetation(MapTileCell tile, Vector3 tileCenter, int count)
        {
            float hexSize = currentManager.HexSize * 0.4f; // 在地块内随机分布
            float elevationOffset = tile.ElevationLevel * MapManager.ElevationWorldStep;

            // 使用固定的随机种子确保同一地块生成的植被位置稳定
            int seed = tile.Coord.Q * 10000 + tile.Coord.R;
            Random.InitState(seed);

            for (int i = 0; i < count; i++)
            {
                // 地块内随机偏移
                float angle = Random.Range(0f, Mathf.PI * 2);
                float radius = Random.Range(0f, hexSize);
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );

                Vector3 position = tileCenter + offset;
                position.y += elevationOffset + 0.1f; // 微量抬高避免与地块穿插

                // 随机旋转（Y轴）
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                // 随机缩放（Y轴使用heightScale压扁，使植被更矮）
                float scale = Random.Range(minScale, maxScale);
                Vector3 scaleVec = new Vector3(scale, scale * heightScale, scale);

                // 构建变换矩阵
                Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scaleVec);
                instanceMatrices.Add(matrix);

                // 随机植被类型颜色
                Color color = VegetationTypes[Random.Range(0, VegetationTypes.Length)];
                instanceColors.Add(new Vector4(color.r, color.g, color.b, 1f));
            }
        }

        void DrawVegetationInstanced()
        {
            int batchSize = 1023; // Graphics.DrawMeshInstanced 最大批次
            int totalInstances = instanceMatrices.Count;

            for (int i = 0; i < totalInstances; i += batchSize)
            {
                int count = Mathf.Min(batchSize, totalInstances - i);

                Matrix4x4[] batchMatrices = new Matrix4x4[count];
                Vector4[] batchColors = new Vector4[count];
                for (int j = 0; j < count; j++)
                {
                    batchMatrices[j] = instanceMatrices[i + j];
                    batchColors[j] = instanceColors[i + j];
                }

                // 创建属性块传递颜色数据
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                propertyBlock.SetVectorArray("_Color", batchColors);

                Graphics.DrawMeshInstanced(
                    vegetationMesh,
                    0,
                    vegetationMaterial,
                    batchMatrices,
                    count,
                    propertyBlock
                );
            }
        }

        /// <summary>设置植被网格（公告牌或交叉平面）。</summary>
        public void SetVegetationMesh(Mesh mesh)
        {
            vegetationMesh = mesh;
        }

        /// <summary>设置植被材质。</summary>
        public void SetVegetationMaterial(Material material)
        {
            vegetationMaterial = material;
        }

        /// <summary>获取当前实例统计（调试用）。</summary>
        public string GetVegetationStats()
        {
            return $"Active Vegetation Instances: {instanceMatrices.Count}";
        }
    }
}
