using System.Collections.Generic;
using TBS.Map.Managers;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.Rendering
{
    public class MarchLineRenderer : MonoBehaviour
    {
        [SerializeField] private Material marchLineMaterial;
        [SerializeField] private Color lineColor = new Color(1f, 0.3f, 0.1f, 0.85f);
        [SerializeField] private float lineWidth = 0.15f;
        [SerializeField] private float yOffset = 0.25f;
        [SerializeField] private int segmentsPerUnit = 4;

        private Mesh currentMesh;
        private MapManager mapManager;

        // 实时跟踪数据
        private Transform trackedUnit;
        private List<MapHexCoord> trackedCoords;

        public void Initialize(MapManager manager)
        {
            mapManager = manager;
        }

        public void SetMaterial(Material mat)
        {
            marchLineMaterial = mat;
        }

        void EnsureMaterial()
        {
            if (marchLineMaterial != null) return;
            marchLineMaterial = new Material(Shader.Find("Sprites/Default"));
            marchLineMaterial.name = "MarchLine_Auto";
            marchLineMaterial.color = lineColor;
            marchLineMaterial.renderQueue = 4000;
        }

        /// <summary>
        /// 设置实时跟踪：行军线第一个点跟随单位世界坐标，后续点为格子中心
        /// </summary>
        public void TrackUnit(Transform unit, List<MapHexCoord> remainingCoords)
        {
            trackedUnit = unit;
            trackedCoords = remainingCoords;
            EnsureMaterial();
        }

        /// <summary>
        /// 显示静态路径（不跟踪单位）
        /// </summary>
        public void ShowPath(List<MapHexCoord> path)
        {
            ClearPath();
            if (path == null || path.Count < 2 || mapManager == null) return;
            EnsureMaterial();

            var points = new List<Vector3>();
            for (int i = 0; i < path.Count; i++)
                points.Add(GetCoordWorldPos(path[i]));

            RebuildMesh(points);
        }

        public void ClearPath()
        {
            trackedUnit = null;
            trackedCoords = null;
            DestroyMesh();
        }

        void Update()
        {
            // 实时跟踪模式：每帧重建mesh
            if (trackedUnit != null && trackedCoords != null && trackedCoords.Count >= 1)
            {
                var points = new List<Vector3>();
                // 第一个点：单位当前世界坐标（仅调整y）
                var unitPos = trackedUnit.position;
                unitPos.y += yOffset;
                points.Add(unitPos);

                for (int i = 0; i < trackedCoords.Count; i++)
                    points.Add(GetCoordWorldPos(trackedCoords[i]));

                RebuildMesh(points);
            }

            if (currentMesh != null && marchLineMaterial != null)
                Graphics.DrawMesh(currentMesh, Vector3.zero, Quaternion.identity, marchLineMaterial, 0);
        }

        void OnDestroy()
        {
            DestroyMesh();
        }

        void DestroyMesh()
        {
            if (currentMesh != null)
            {
                if (Application.isPlaying) Destroy(currentMesh);
                else DestroyImmediate(currentMesh);
                currentMesh = null;
            }
        }

        Vector3 GetCoordWorldPos(MapHexCoord coord)
        {
            var pos = mapManager.CoordToWorldPosition(coord);
            var tile = mapManager.GetTile(coord);
            pos.y += (tile?.ElevationLevel ?? 0) * MapManager.ElevationWorldStep + yOffset;
            return pos;
        }

        void RebuildMesh(List<Vector3> worldPoints)
        {
            DestroyMesh();
            if (worldPoints.Count < 2) return;

            var allVerts = new List<Vector3>();
            var allUvs = new List<Vector2>();
            var allTris = new List<int>();
            float halfW = lineWidth * 0.5f;
            float accDist = 0f;

            for (int i = 0; i < worldPoints.Count - 1; i++)
            {
                Vector3 from = worldPoints[i];
                Vector3 to = worldPoints[i + 1];
                float segLen = Vector3.Distance(from, to);
                if (segLen < 0.001f) continue;

                int segs = Mathf.Max(2, Mathf.CeilToInt(segLen * segmentsPerUnit));

                Vector3 fwd = (to - from).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
                if (right.sqrMagnitude < 0.001f)
                    right = Vector3.right;

                for (int s = 0; s < segs; s++)
                {
                    float t = s / (float)(segs - 1);
                    Vector3 center = Vector3.Lerp(from, to, t);
                    float dist = accDist + segLen * t;
                    int baseIdx = allVerts.Count;

                    allVerts.Add(center - right * halfW);
                    allVerts.Add(center + right * halfW);
                    allUvs.Add(new Vector2(0, dist));
                    allUvs.Add(new Vector2(1, dist));

                    if (baseIdx >= 2)
                    {
                        allTris.Add(baseIdx - 2);
                        allTris.Add(baseIdx - 1);
                        allTris.Add(baseIdx);
                        allTris.Add(baseIdx - 1);
                        allTris.Add(baseIdx + 1);
                        allTris.Add(baseIdx);
                    }
                }

                accDist += segLen;
            }

            if (allVerts.Count < 4) return;

            currentMesh = new Mesh { name = "MarchLine" };
            currentMesh.SetVertices(allVerts);
            currentMesh.SetUVs(0, allUvs);
            currentMesh.SetTriangles(allTris, 0);
            currentMesh.RecalculateNormals();
            currentMesh.RecalculateBounds();
        }
    }
}
