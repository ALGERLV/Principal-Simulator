using UnityEngine;
using UnityEditor;
using TBS.Map.Runtime;

namespace TBS.Map.Editor
{
    /// <summary>
    /// TilePrefab 设置工具 - 为空的 TilePrefab 添加六边形网格和碰撞器
    /// </summary>
    public class TilePrefabSetup : EditorWindow
    {
        private GameObject tilePrefab;
        private float hexSize = 1f;
        private Material previewMaterial;

        [MenuItem("TBS/Map/Setup Tile Prefab")]
        public static void ShowWindow()
        {
            GetWindow<TilePrefabSetup>("Tile Prefab 设置");
        }

        private void OnGUI()
        {
            GUILayout.Label("Tile Prefab 六边形设置", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "选择一个只包含 MapTileCell 组件的预制体，\n" +
                "点击按钮添加正六边形网格和碰撞器。", MessageType.Info);

            EditorGUILayout.Space(10);

            tilePrefab = EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false) as GameObject;
            hexSize = EditorGUILayout.FloatField("六边形大小", hexSize);
            previewMaterial = EditorGUILayout.ObjectField("预览材质 (可选)", previewMaterial, typeof(Material), false) as Material;

            EditorGUILayout.Space(20);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("添加六边形内容", GUILayout.Height(40)))
            {
                SetupPrefab();
            }
            GUI.backgroundColor = Color.white;
        }

        private void SetupPrefab()
        {
            if (tilePrefab == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择 Tile Prefab！", "确定");
                return;
            }

            // 确保预制体已实例化以进行编辑
            string prefabPath = AssetDatabase.GetAssetPath(tilePrefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefabRoot == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载预制体内容！", "确定");
                return;
            }

            try
            {
                // 检查/添加 MapTileCell
                MapTileCell tileCell = prefabRoot.GetComponent<MapTileCell>();
                if (tileCell == null)
                {
                    tileCell = prefabRoot.AddComponent<MapTileCell>();
                    Debug.Log("[TilePrefabSetup] 添加了 MapTileCell 组件");
                }

                // 添加/更新 MeshFilter
                MeshFilter meshFilter = prefabRoot.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = prefabRoot.AddComponent<MeshFilter>();
                }
                meshFilter.sharedMesh = CreateHexMesh(hexSize);
                Debug.Log("[TilePrefabSetup] 添加了六边形 Mesh");

                // 添加/更新 MeshRenderer
                MeshRenderer meshRenderer = prefabRoot.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = prefabRoot.AddComponent<MeshRenderer>();
                }

                // 设置材质
                if (previewMaterial != null)
                {
                    meshRenderer.sharedMaterial = previewMaterial;
                }
                else
                {
                    // 创建默认材质
                    Material defaultMat = new Material(Shader.Find("Standard"));
                    defaultMat.color = new Color(0.7f, 0.8f, 0.4f, 1f);
                    meshRenderer.sharedMaterial = defaultMat;
                }
                Debug.Log("[TilePrefabSetup] 添加了 MeshRenderer");

                // 添加/更新 MeshCollider
                MeshCollider meshCollider = prefabRoot.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = prefabRoot.AddComponent<MeshCollider>();
                }
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = false;
                Debug.Log("[TilePrefabSetup] 添加了 MeshCollider");

                // 设置标签
                prefabRoot.tag = "Tile";

                // 保存预制体修改
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

                EditorUtility.DisplayDialog("完成", 
                    "Tile Prefab 设置完成！\n\n" +
                    "已添加:\n" +
                    "- 正六边形 Mesh\n" +
                    "- MeshRenderer (预览用)\n" +
                    "- MeshCollider (点击检测)", 
                    "确定");
            }
            finally
            {
                // 卸载预制体内容
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            // 刷新资源数据库
            AssetDatabase.Refresh();
        }

        /// <summary>创建带厚度的正六边形网格（pointy-top，无拉伸）。</summary>
        private Mesh CreateHexMesh(float size)
        {
            Mesh mesh = new Mesh();
            mesh.name = "HexTile_Thick";

            // 使用内切圆半径作为六边形大小基准（与网格坐标系统一致）
            float a = size; // 内切圆半径 = 中心到边的距离
            float r = a * 2f / Mathf.Sqrt(3); // 外接圆半径 = 中心到顶点的距离
            float w = a; // 半边宽 = 内切圆半径
            float thickness = 0.2f; // 地块厚度

            // 14个顶点：上面7个 + 下面7个
            Vector3[] vertices = new Vector3[14];
            
            // 上面
            vertices[0] = new Vector3(0, thickness/2, 0);
            vertices[1] = new Vector3(0, thickness/2, r);
            vertices[2] = new Vector3(w, thickness/2, r * 0.5f);
            vertices[3] = new Vector3(w, thickness/2, -r * 0.5f);
            vertices[4] = new Vector3(0, thickness/2, -r);
            vertices[5] = new Vector3(-w, thickness/2, -r * 0.5f);
            vertices[6] = new Vector3(-w, thickness/2, r * 0.5f);
            
            // 下面
            vertices[7] = new Vector3(0, -thickness/2, 0);
            vertices[8] = new Vector3(0, -thickness/2, r);
            vertices[9] = new Vector3(w, -thickness/2, r * 0.5f);
            vertices[10] = new Vector3(w, -thickness/2, -r * 0.5f);
            vertices[11] = new Vector3(0, -thickness/2, -r);
            vertices[12] = new Vector3(-w, -thickness/2, -r * 0.5f);
            vertices[13] = new Vector3(-w, -thickness/2, r * 0.5f);

            // 三角形索引
            int[] triangles = new int[144];
            int triIdx = 0;
            
            // 上面
            triangles[triIdx++] = 0; triangles[triIdx++] = 1; triangles[triIdx++] = 2;
            triangles[triIdx++] = 0; triangles[triIdx++] = 2; triangles[triIdx++] = 3;
            triangles[triIdx++] = 0; triangles[triIdx++] = 3; triangles[triIdx++] = 4;
            triangles[triIdx++] = 0; triangles[triIdx++] = 4; triangles[triIdx++] = 5;
            triangles[triIdx++] = 0; triangles[triIdx++] = 5; triangles[triIdx++] = 6;
            triangles[triIdx++] = 0; triangles[triIdx++] = 6; triangles[triIdx++] = 1;
            
            // 下面
            triangles[triIdx++] = 7; triangles[triIdx++] = 9; triangles[triIdx++] = 8;
            triangles[triIdx++] = 7; triangles[triIdx++] = 10; triangles[triIdx++] = 9;
            triangles[triIdx++] = 7; triangles[triIdx++] = 11; triangles[triIdx++] = 10;
            triangles[triIdx++] = 7; triangles[triIdx++] = 12; triangles[triIdx++] = 11;
            triangles[triIdx++] = 7; triangles[triIdx++] = 13; triangles[triIdx++] = 12;
            triangles[triIdx++] = 7; triangles[triIdx++] = 8; triangles[triIdx++] = 13;
            
            // 侧面6条边
            triangles[triIdx++] = 1; triangles[triIdx++] = 8; triangles[triIdx++] = 9;
            triangles[triIdx++] = 1; triangles[triIdx++] = 9; triangles[triIdx++] = 2;
            triangles[triIdx++] = 2; triangles[triIdx++] = 9; triangles[triIdx++] = 10;
            triangles[triIdx++] = 2; triangles[triIdx++] = 10; triangles[triIdx++] = 3;
            triangles[triIdx++] = 3; triangles[triIdx++] = 10; triangles[triIdx++] = 11;
            triangles[triIdx++] = 3; triangles[triIdx++] = 11; triangles[triIdx++] = 4;
            triangles[triIdx++] = 4; triangles[triIdx++] = 11; triangles[triIdx++] = 12;
            triangles[triIdx++] = 4; triangles[triIdx++] = 12; triangles[triIdx++] = 5;
            triangles[triIdx++] = 5; triangles[triIdx++] = 12; triangles[triIdx++] = 13;
            triangles[triIdx++] = 5; triangles[triIdx++] = 13; triangles[triIdx++] = 6;
            triangles[triIdx++] = 6; triangles[triIdx++] = 13; triangles[triIdx++] = 8;
            triangles[triIdx++] = 6; triangles[triIdx++] = 8; triangles[triIdx++] = 1;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
