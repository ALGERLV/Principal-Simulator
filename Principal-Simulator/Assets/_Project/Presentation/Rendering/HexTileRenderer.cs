using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.Components
{
    /// <summary>
    /// 六边形地块渲染器 - 简单的可视化表示
    /// </summary>
    public class HexTileRenderer : MonoBehaviour
    {
        [SerializeField] private HexTile tile;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshFilter meshFilter;

        private void Awake()
        {
            tile = GetComponent<HexTile>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();

            // 如果没有mesh，创建一个简单的六边形
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = CreateHexagonMesh(1f, 0.2f);
            }

            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
                // 使用更通用的shader，确保兼容性
                Shader shader = Shader.Find("Standard") ?? Shader.Find("Diffuse") ?? Shader.Find("VertexLit");
                if (shader != null)
                {
                    meshRenderer.material = new Material(shader);
                    meshRenderer.material.color = Color.white;
                }
                else
                {
                    Debug.LogError("HexTileRenderer: 无法找到合适的Shader", this);
                }
            }

            // 确保有mesh
            if (meshFilter.sharedMesh == null)
            {
                meshFilter.mesh = CreateHexagonMesh(1f, 0.2f);
            }
        }

        private void Start()
        {
            UpdateVisuals();
        }

        /// <summary>
        /// 根据地形更新视觉表现
        /// </summary>
        public void UpdateVisuals()
        {
            if (tile == null || tile.TerrainData == null) return;

            // 应用地形颜色
            if (meshRenderer != null)
            {
                meshRenderer.material.color = tile.TerrainData.TerrainColor;
            }
        }

        /// <summary>
        /// 创建六边形网格（带厚度）
        /// </summary>
        private Mesh CreateHexagonMesh(float radius, float height = 0.2f)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Hexagon_Thick";

            Vector3[] vertices = new Vector3[7];
            vertices[0] = Vector3.zero; // 中心

            // 6个顶点
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                vertices[i + 1] = new Vector3(
                    Mathf.Sin(angle) * radius,
                    0,
                    Mathf.Cos(angle) * radius
                );
            }

            // 三角形（6个扇形）
            int[] triangles = new int[18];
            for (int i = 0; i < 6; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % 6 + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// 高亮显示
        /// </summary>
        public void Highlight(Color color)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = color;
            }
        }

        /// <summary>
        /// 恢复默认颜色
        /// </summary>
        public void ResetColor()
        {
            UpdateVisuals();
        }
    }
}
