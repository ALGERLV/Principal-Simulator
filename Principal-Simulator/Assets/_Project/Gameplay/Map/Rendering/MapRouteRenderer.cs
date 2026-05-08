using System.Collections.Generic;
using TBS.Map.Data;
using TBS.Map.Runtime;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.Rendering
{
    /// <summary>
    /// 地图系统设计 4.2：河流/道路带状 Mesh 渲染器。
    /// </summary>
    public class MapRouteRenderer : MonoBehaviour
    {
        [Header("材质配置")]
        [SerializeField] private Material riverMaterial;
        [SerializeField] private Material roadMaterial;

        [Header("网格配置")]
        [SerializeField, Tooltip("每单位长度的分段数，越高越平滑")]
        private int segmentsPerUnit = 4;

        private MapTerrainGrid currentGrid;
        private Dictionary<MapLinkType, Mesh> linkMeshes = new Dictionary<MapLinkType, Mesh>();
        private List<(MapLinkType link, Mesh mesh, Material material)> activeLinks = new List<(MapLinkType, Mesh, Material)>();

        /// <summary>地图加载时由 MapManager 调用。</summary>
        public void OnMapLoaded(MapRouteSetting routeSetting, MapTerrainGrid grid)
        {
            if (routeSetting == null) return;
            SetGrid(grid);
            RebuildLinks(routeSetting);
        }

        /// <summary>重建所有道路/河流连接。</summary>
        public void RebuildLinks(MapRouteSetting routeSetting)
        {
            ClearMeshes();

            if (routeSetting == null || currentGrid == null) return;

            foreach (var link in routeSetting.Links)
            {
                Mesh mesh = CreateStripMesh(link);
                Material mat = link.Type == MapRouteType.River ? riverMaterial : roadMaterial;

                if (mesh != null)
                {
                    linkMeshes[link] = mesh;
                    activeLinks.Add((link, mesh, mat));
                }
            }
        }

        /// <summary>设置当前网格引用（用于坐标转换）。</summary>
        public void SetGrid(MapTerrainGrid grid)
        {
            currentGrid = grid;
        }

        void OnDestroy()
        {
            ClearMeshes();
        }

        void Update()
        {
            // 绘制所有激活的连接
            foreach (var (link, mesh, material) in activeLinks)
            {
                if (mesh == null || material == null) continue;

                Vector3 pos = Vector3.zero;
                Quaternion rot = Quaternion.identity;

                // 河流纹理滚动动画
                if (link.Type == MapRouteType.River)
                {
                    Vector2 offset = new Vector2(0, -Time.time * 0.5f);
                    material.SetTextureOffset("_MainTex", offset);
                }

                Graphics.DrawMesh(mesh, pos, rot, material, 0);
            }
        }

        Mesh CreateStripMesh(MapLinkType link)
        {
            if (currentGrid == null) return null;

            MapTileCell fromTile = currentGrid.GetTile(link.From);
            MapTileCell toTile = currentGrid.GetTile(link.To);

            if (fromTile == null || toTile == null) return null;

            Vector3 fromPos = currentGrid.CoordToWorldPosition(link.From);
            Vector3 toPos = currentGrid.CoordToWorldPosition(link.To);

            // 应用海拔偏移
            fromPos.y += (fromTile?.ElevationLevel ?? 0) * MapTerrainGrid.ElevationWorldStep;
            toPos.y += (toTile?.ElevationLevel ?? 0) * MapTerrainGrid.ElevationWorldStep;

            // 微量抬高确保在地块之上
            float zLift = 0.02f;
            fromPos.y += zLift;
            toPos.y += zLift;

            float distance = Vector3.Distance(fromPos, toPos);
            int segments = Mathf.Max(2, Mathf.CeilToInt(distance * segmentsPerUnit));

            float halfWidth = link.WidthWorld * 0.5f;

            Vector3[] vertices = new Vector3[segments * 2];
            Vector2[] uvs = new Vector2[segments * 2];
            int[] triangles = new int[(segments - 1) * 6];

            Vector3 forward = (toPos - fromPos).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)(segments - 1);
                Vector3 center = Vector3.Lerp(fromPos, toPos, t);

                vertices[i * 2] = center - right * halfWidth;
                vertices[i * 2 + 1] = center + right * halfWidth;

                uvs[i * 2] = new Vector2(0, t);
                uvs[i * 2 + 1] = new Vector2(1, t);
            }

            for (int i = 0; i < segments - 1; i++)
            {
                int baseIdx = i * 2;

                triangles[i * 6] = baseIdx;
                triangles[i * 6 + 1] = baseIdx + 1;
                triangles[i * 6 + 2] = baseIdx + 2;

                triangles[i * 6 + 3] = baseIdx + 1;
                triangles[i * 6 + 4] = baseIdx + 3;
                triangles[i * 6 + 5] = baseIdx + 2;
            }

            Mesh mesh = new Mesh();
            mesh.name = $"Route_{link.From}_{link.To}";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        void ClearMeshes()
        {
            foreach (var kvp in linkMeshes)
            {
                if (kvp.Value != null)
                {
                    if (Application.isPlaying)
                        Destroy(kvp.Value);
                    else
                        DestroyImmediate(kvp.Value);
                }
            }

            linkMeshes.Clear();
            activeLinks.Clear();
        }

        /// <summary>设置河流材质（运行时动态更换）。</summary>
        public void SetRiverMaterial(Material material)
        {
            riverMaterial = material;
        }

        /// <summary>设置道路材质（运行时动态更换）。</summary>
        public void SetRoadMaterial(Material material)
        {
            roadMaterial = material;
        }

        /// <summary>获取当前连接统计（调试用）。</summary>
        public string GetRouteStats()
        {
            return $"Active Routes: {activeLinks.Count}, Meshes: {linkMeshes.Count}";
        }
    }
}
