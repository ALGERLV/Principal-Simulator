using System.Collections.Generic;
using TBS.Map.Managers;
using TBS.Map.Tools;
using UnityEngine;
using UnityEngine.Rendering;

namespace TBS.Map.Rendering
{
    public class MarchLineRenderer : MonoBehaviour
    {
        [SerializeField] private Material marchLineMaterial;
        [SerializeField] private Color lineColor = new Color(1f, 0.3f, 0.1f, 0.85f);
        [SerializeField] private float lineWidth = 0.15f;
        [SerializeField] private float yOffset = 0.05f;
        [SerializeField] private int segmentsPerUnit = 4;

        private Mesh currentMesh;
        private List<MapHexCoord> currentPath;
        private MapManager mapManager;

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

            var shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Hidden/InternalErrorShader");

            marchLineMaterial = new Material(shader);
            marchLineMaterial.name = "MarchLine_Auto";
            marchLineMaterial.color = lineColor;

            marchLineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            marchLineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            marchLineMaterial.SetInt("_ZWrite", 0);
            marchLineMaterial.renderQueue = (int)RenderQueue.Transparent;
        }

        public void ShowPath(List<MapHexCoord> path)
        {
            ClearPath();
            if (path == null || path.Count < 2 || mapManager == null) return;

            EnsureMaterial();
            currentPath = path;
            currentMesh = BuildPathMesh(path);
        }

        public void ClearPath()
        {
            if (currentMesh != null)
            {
                if (Application.isPlaying) Destroy(currentMesh);
                else DestroyImmediate(currentMesh);
                currentMesh = null;
            }
            currentPath = null;
        }

        void Update()
        {
            if (currentMesh != null && marchLineMaterial != null)
                Graphics.DrawMesh(currentMesh, Vector3.zero, Quaternion.identity, marchLineMaterial, 0);
        }

        void OnDestroy()
        {
            ClearPath();
        }

        Mesh BuildPathMesh(List<MapHexCoord> path)
        {
            var worldPoints = new List<Vector3>();
            for (int i = 0; i < path.Count; i++)
            {
                var pos = mapManager.CoordToWorldPosition(path[i]);
                var tile = mapManager.GetTile(path[i]);
                pos.y += (tile?.ElevationLevel ?? 0) * MapManager.ElevationWorldStep + yOffset;
                worldPoints.Add(pos);
            }

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

            var mesh = new Mesh { name = "MarchLine" };
            mesh.SetVertices(allVerts);
            mesh.SetUVs(0, allUvs);
            mesh.SetTriangles(allTris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
