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
                // 检查/添加 MapTileCell（在父对象上）
                MapTileCell tileCell = prefabRoot.GetComponent<MapTileCell>();
                if (tileCell == null)
                {
                    tileCell = prefabRoot.AddComponent<MapTileCell>();
                    Debug.Log("[TilePrefabSetup] 添加了 MapTileCell 组件");
                }

                // 移除父对象上多余的渲染组件（避免和Hex子对象冲突）
                MeshFilter parentMeshFilter = prefabRoot.GetComponent<MeshFilter>();
                if (parentMeshFilter != null)
                {
                    DestroyImmediate(parentMeshFilter);
                    Debug.Log("[TilePrefabSetup] 移除父对象上的 MeshFilter");
                }

                MeshRenderer parentMeshRenderer = prefabRoot.GetComponent<MeshRenderer>();
                if (parentMeshRenderer != null)
                {
                    DestroyImmediate(parentMeshRenderer);
                    Debug.Log("[TilePrefabSetup] 移除父对象上的 MeshRenderer");
                }

                MeshCollider parentMeshCollider = prefabRoot.GetComponent<MeshCollider>();
                if (parentMeshCollider != null)
                {
                    DestroyImmediate(parentMeshCollider);
                    Debug.Log("[TilePrefabSetup] 移除父对象上的 MeshCollider");
                }

                // 找到或创建 Hex 子对象
                Transform hexTransform = prefabRoot.transform.Find("Hex");
                GameObject hexObject;
                if (hexTransform == null)
                {
                    hexObject = new GameObject("Hex");
                    hexObject.transform.SetParent(prefabRoot.transform, false);
                    hexObject.transform.localPosition = Vector3.zero;
                    hexObject.transform.localRotation = Quaternion.identity;
                    hexObject.transform.localScale = Vector3.one;
                    Debug.Log("[TilePrefabSetup] 创建 Hex 子对象");
                }
                else
                {
                    hexObject = hexTransform.gameObject;
                    Debug.Log("[TilePrefabSetup] 找到现有的 Hex 子对象");
                }

                // 在 Hex 子对象上添加/更新 MeshFilter
                MeshFilter meshFilter = hexObject.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = hexObject.AddComponent<MeshFilter>();
                }
                meshFilter.sharedMesh = CreateHexMesh(hexSize);
                Debug.Log($"[TilePrefabSetup] 在 Hex 子对象上添加六边形 Mesh (大小: {hexSize})");

                // 在 Hex 子对象上添加/更新 MeshRenderer
                MeshRenderer meshRenderer = hexObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = hexObject.AddComponent<MeshRenderer>();
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
                Debug.Log("[TilePrefabSetup] 在 Hex 子对象上添加 MeshRenderer");

                // 在 Hex 子对象上添加/更新 MeshCollider
                MeshCollider meshCollider = hexObject.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = hexObject.AddComponent<MeshCollider>();
                }
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = false;
                Debug.Log("[TilePrefabSetup] 在 Hex 子对象上添加 MeshCollider");

                // 设置标签
                prefabRoot.tag = "Tile";

                // 保存预制体修改
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

                EditorUtility.DisplayDialog("完成", 
                    "Tile Prefab 设置完成！\n\n" +
                    "已更新:\n" +
                    "- 移除父对象上多余的渲染组件\n" +
                    "- 在 Hex 子对象上添加正六边形 Mesh\n" +
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

        /// <summary>创建带厚度的正六边形网格（pointy-top，无拉伸）。
        /// 使用外接圆半径（circumradius）作为大小参数，与坐标计算一致。
        /// 生成比理论尺寸略小的 mesh（scaleFactor=0.95），留出缝隙避免重叠闪烁。</summary>
        private Mesh CreateHexMesh(float size)
        {
            Mesh mesh = new Mesh();
            mesh.name = "HexTile_Thick";

            // 使用外接圆半径作为六边形大小基准（与网格坐标系统一致）
            // 坐标计算中：水平间距 = sqrt(3) * size, 垂直间距 = 1.5 * size
            // 这里的 size 是外接圆半径（中心到顶点的距离）
            float r = size; // 外接圆半径 = 中心到顶点的距离
            float a = r * Mathf.Sqrt(3) / 2f; // 内切圆半径 = 中心到边的距离（半边宽）
            float w = a; // 半边宽
            float thickness = 0.2f; // 地块厚度
            
            // 缩放因子：生成比理论尺寸略小的 mesh，留出缝隙避免重叠闪烁
            float scaleFactor = 0.95f;
            r *= scaleFactor;
            w *= scaleFactor;

            // 14个顶点：上面7个 + 下面7个
            Vector3[] vertices = new Vector3[14];
            
            // 上面（Y = thickness/2）
            vertices[0] = new Vector3(0, thickness/2, 0);           // 中心 [0]
            vertices[1] = new Vector3(0, thickness/2, r);           // 顶部（顶点朝上） [1]
            vertices[2] = new Vector3(w, thickness/2, r * 0.5f);   // 右上 [2]
            vertices[3] = new Vector3(w, thickness/2, -r * 0.5f); // 右下 [3]
            vertices[4] = new Vector3(0, thickness/2, -r);        // 底部 [4]
            vertices[5] = new Vector3(-w, thickness/2, -r * 0.5f);  // 左下 [5]
            vertices[6] = new Vector3(-w, thickness/2, r * 0.5f);  // 左上 [6]
            
            // 下面（Y = -thickness/2）
            vertices[7] = new Vector3(0, -thickness/2, 0);          // 中心 [7]
            vertices[8] = new Vector3(0, -thickness/2, r);          // 顶部 [8]
            vertices[9] = new Vector3(w, -thickness/2, r * 0.5f);  // 右上 [9]
            vertices[10] = new Vector3(w, -thickness/2, -r * 0.5f);// 右下 [10]
            vertices[11] = new Vector3(0, -thickness/2, -r);       // 底部 [11]
            vertices[12] = new Vector3(-w, -thickness/2, -r * 0.5f);// 左下 [12]
            vertices[13] = new Vector3(-w, -thickness/2, r * 0.5f); // 左上 [13]

            // 三角形索引：上面6个 + 下面6个 + 侧面12个
            int[] triangles = new int[144];
            int triIdx = 0;
            
            // 上面（顺时针，从上方看）
            triangles[triIdx++] = 0; triangles[triIdx++] = 1; triangles[triIdx++] = 2;
            triangles[triIdx++] = 0; triangles[triIdx++] = 2; triangles[triIdx++] = 3;
            triangles[triIdx++] = 0; triangles[triIdx++] = 3; triangles[triIdx++] = 4;
            triangles[triIdx++] = 0; triangles[triIdx++] = 4; triangles[triIdx++] = 5;
            triangles[triIdx++] = 0; triangles[triIdx++] = 5; triangles[triIdx++] = 6;
            triangles[triIdx++] = 0; triangles[triIdx++] = 6; triangles[triIdx++] = 1;
            
            // 下面（逆时针，从下方看是正面）
            triangles[triIdx++] = 7; triangles[triIdx++] = 9; triangles[triIdx++] = 8;
            triangles[triIdx++] = 7; triangles[triIdx++] = 10; triangles[triIdx++] = 9;
            triangles[triIdx++] = 7; triangles[triIdx++] = 11; triangles[triIdx++] = 10;
            triangles[triIdx++] = 7; triangles[triIdx++] = 12; triangles[triIdx++] = 11;
            triangles[triIdx++] = 7; triangles[triIdx++] = 13; triangles[triIdx++] = 12;
            triangles[triIdx++] = 7; triangles[triIdx++] = 8; triangles[triIdx++] = 13;
            
            // 侧面6条边（每条边2个三角形）
            // 边1: 顶-右上 (1-2 -> 8-9)
            triangles[triIdx++] = 1; triangles[triIdx++] = 8; triangles[triIdx++] = 9;
            triangles[triIdx++] = 1; triangles[triIdx++] = 9; triangles[triIdx++] = 2;
            // 边2: 右上-右下 (2-3 -> 9-10)
            triangles[triIdx++] = 2; triangles[triIdx++] = 9; triangles[triIdx++] = 10;
            triangles[triIdx++] = 2; triangles[triIdx++] = 10; triangles[triIdx++] = 3;
            // 边3: 右下-底 (3-4 -> 10-11)
            triangles[triIdx++] = 3; triangles[triIdx++] = 10; triangles[triIdx++] = 11;
            triangles[triIdx++] = 3; triangles[triIdx++] = 11; triangles[triIdx++] = 4;
            // 边4: 底-左下 (4-5 -> 11-12)
            triangles[triIdx++] = 4; triangles[triIdx++] = 11; triangles[triIdx++] = 12;
            triangles[triIdx++] = 4; triangles[triIdx++] = 12; triangles[triIdx++] = 5;
            // 边5: 左下-左上 (5-6 -> 12-13)
            triangles[triIdx++] = 5; triangles[triIdx++] = 12; triangles[triIdx++] = 13;
            triangles[triIdx++] = 5; triangles[triIdx++] = 13; triangles[triIdx++] = 6;
            // 边6: 左上-顶 (6-1 -> 13-8)
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
