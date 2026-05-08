using UnityEngine;
using UnityEditor;
using TBS.Map.Managers;
using TBS.Map.Runtime;
using TBS.Map.Rendering;
using TBS.Map.Data;
using TBS.Map.Components;
using TBS.Presentation.Camera;

using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Editor
{
    /// <summary>
    /// 地图测试场景快速设置工具
    /// </summary>
    public class MapTestSceneSetup : EditorWindow
    {
        private MapGridSetting gridSetting;
        private MapRouteSetting routeSetting;
        private MapSkinSetting skinSetting;
        private TerrainData defaultTerrain;
        private GameObject tilePrefab;

        [MenuItem("TBS/Map/Setup Test Scene")]
        public static void ShowWindow()
        {
            GetWindow<MapTestSceneSetup>("地图测试场景设置");
        }

        private void OnGUI()
        {
            GUILayout.Label("地图测试场景快速设置", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox("拖拽资源或点击'加载已有资源'自动填充，然后点击'设置场景'按钮", MessageType.Info);
            EditorGUILayout.Space(10);

            gridSetting = EditorGUILayout.ObjectField("网格配置", gridSetting, typeof(MapGridSetting), false) as MapGridSetting;
            routeSetting = EditorGUILayout.ObjectField("道路配置", routeSetting, typeof(MapRouteSetting), false) as MapRouteSetting;
            skinSetting = EditorGUILayout.ObjectField("皮肤配置", skinSetting, typeof(MapSkinSetting), false) as MapSkinSetting;
            defaultTerrain = EditorGUILayout.ObjectField("默认地形", defaultTerrain, typeof(TerrainData), false) as TerrainData;
            tilePrefab = EditorGUILayout.ObjectField("地块预制体", tilePrefab, typeof(GameObject), false) as GameObject;

            EditorGUILayout.Space(10);

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("加载已有资源", GUILayout.Height(30)))
            {
                LoadExistingResources();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("设置场景", GUILayout.Height(40)))
            {
                SetupScene();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("创建默认资源", GUILayout.Height(30)))
            {
                CreateDefaultResources();
            }
            GUI.backgroundColor = Color.white;
        }

        private void LoadExistingResources()
        {
            // 尝试从 Resources 文件夹加载
            gridSetting = Resources.Load<MapGridSetting>("MapConfigs/TestGridSetting");
            routeSetting = Resources.Load<MapRouteSetting>("MapConfigs/TestRouteSetting");
            skinSetting = Resources.Load<MapSkinSetting>("MapConfigs/TestSkinSetting");
            defaultTerrain = Resources.Load<TerrainData>("Terrain/Plain");

            // 尝试加载预制体
            tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/HexTilePrefab.prefab");

            EditorUtility.DisplayDialog("完成", "资源加载完成！", "确定");
            Repaint();
        }

        private void SetupScene()
        {
            // 检查是否已有 MapManager
            MapManager existingManager = FindObjectOfType<MapManager>();
            if (existingManager != null)
            {
                if (!EditorUtility.DisplayDialog("提示", "场景中已存在 MapManager，是否重新创建？", "是", "否"))
                {
                    return;
                }
                DestroyImmediate(existingManager.gameObject);
            }

            // 创建 MapManager
            GameObject mapManagerGO = new GameObject("MapManager");
            MapManager mapManager = mapManagerGO.AddComponent<MapManager>();

            // 设置序列化字段
            SerializedObject managerSO = new SerializedObject(mapManager);
            
            if (gridSetting != null)
                managerSO.FindProperty("defaultGridSettings").objectReferenceValue = gridSetting;
            if (routeSetting != null)
                managerSO.FindProperty("routeLinks").objectReferenceValue = routeSetting;

            managerSO.ApplyModifiedProperties();

            // 创建 MapTerrainGrid
            GameObject terrainGridGO = new GameObject("MapTerrainGrid");
            terrainGridGO.transform.SetParent(mapManagerGO.transform);
            MapTerrainGrid terrainGrid = terrainGridGO.AddComponent<MapTerrainGrid>();

            // 设置 MapTerrainGrid 字段
            SerializedObject gridSO = new SerializedObject(terrainGrid);
            if (tilePrefab != null)
                gridSO.FindProperty("tilePrefab").objectReferenceValue = tilePrefab;
            if (defaultTerrain != null)
                gridSO.FindProperty("defaultTerrain").objectReferenceValue = defaultTerrain;
            gridSO.ApplyModifiedProperties();

            // 设置 MapManager 的 terrainGrid 引用
            managerSO = new SerializedObject(mapManager);
            managerSO.FindProperty("terrainGrid").objectReferenceValue = terrainGrid;
            managerSO.ApplyModifiedProperties();

            // 创建渲染器子对象
            GameObject renderersGO = new GameObject("Renderers");
            renderersGO.transform.SetParent(mapManagerGO.transform);

            // 添加 MapRenderer
            GameObject groundRendererGO = new GameObject("MapRenderer");
            groundRendererGO.transform.SetParent(renderersGO.transform);
            MapRenderer mapRenderer = groundRendererGO.AddComponent<MapRenderer>();

            // 添加 MapRouteRenderer
            GameObject routeRendererGO = new GameObject("MapRouteRenderer");
            routeRendererGO.transform.SetParent(renderersGO.transform);
            MapRouteRenderer routeRenderer = routeRendererGO.AddComponent<MapRouteRenderer>();

            // 添加 MapVegetationRenderer
            GameObject vegetationRendererGO = new GameObject("MapVegetationRenderer");
            vegetationRendererGO.transform.SetParent(renderersGO.transform);
            MapVegetationRenderer vegetationRenderer = vegetationRendererGO.AddComponent<MapVegetationRenderer>();

            // 设置渲染器引用
            managerSO = new SerializedObject(mapManager);
            managerSO.FindProperty("groundRenderer").objectReferenceValue = mapRenderer;
            managerSO.FindProperty("routeOverlayRenderer").objectReferenceValue = routeRenderer;
            managerSO.FindProperty("vegetationRenderer").objectReferenceValue = vegetationRenderer;
            managerSO.ApplyModifiedProperties();

            // 设置相机
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                BoardCameraController cameraController = mainCamera.GetComponent<BoardCameraController>();
                if (cameraController == null)
                {
                    cameraController = mainCamera.gameObject.AddComponent<BoardCameraController>();
                }

                SerializedObject cameraSO = new SerializedObject(cameraController);
                cameraSO.FindProperty("targetGrid").objectReferenceValue = terrainGrid;
                cameraSO.ApplyModifiedProperties();
            }

            // 选择 MapManager
            Selection.activeGameObject = mapManagerGO;

            EditorUtility.DisplayDialog("完成", "地图测试场景设置完成！\n\n点击Play运行测试。", "确定");
        }

        private void CreateDefaultResources()
        {
            // 创建目录
            string terrainPath = "Assets/Resources/Terrain";
            string configPath = "Assets/Resources/MapConfigs";

            if (!System.IO.Directory.Exists(terrainPath))
                System.IO.Directory.CreateDirectory(terrainPath);
            if (!System.IO.Directory.Exists(configPath))
                System.IO.Directory.CreateDirectory(configPath);

            // 创建地形数据
            CreateTerrainAsset("Plain", "平原", "普通地形", Color.green, 1f, 0f);
            CreateTerrainAsset("Forest", "森林", "森林地形", new Color(0.133f, 0.545f, 0.133f), 1.5f, 0.3f);
            CreateTerrainAsset("Mountain", "山地", "山地地形", new Color(0.545f, 0.353f, 0.169f), 2f, 0.5f);
            CreateTerrainAsset("River", "河流", "河流地形", new Color(0.2f, 0.6f, 0.8f), 3f, -0.2f, false);

            // 创建网格配置
            MapGridSetting gridSetting = ScriptableObject.CreateInstance<MapGridSetting>();
            gridSetting.name = "TestGridSetting";
            AssetDatabase.CreateAsset(gridSetting, configPath + "/TestGridSetting.asset");

            // 创建道路配置
            MapRouteSetting routeSetting = ScriptableObject.CreateInstance<MapRouteSetting>();
            routeSetting.name = "TestRouteSetting";
            AssetDatabase.CreateAsset(routeSetting, configPath + "/TestRouteSetting.asset");

            // 创建皮肤配置
            MapSkinSetting skinSetting = ScriptableObject.CreateInstance<MapSkinSetting>();
            skinSetting.name = "TestSkinSetting";
            AssetDatabase.CreateAsset(skinSetting, configPath + "/TestSkinSetting.asset");

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", "默认资源创建完成！", "确定");
        }

        private void CreateTerrainAsset(string id, string name, string desc, Color color, float moveCost, float defense, bool passable = true)
        {
            string path = $"Assets/Resources/Terrain/{id}.asset";
            if (System.IO.File.Exists(path)) return;

            TerrainData terrain = ScriptableObject.CreateInstance<TerrainData>();
            SerializedObject so = new SerializedObject(terrain);
            so.FindProperty("terrainId").stringValue = id;
            so.FindProperty("terrainName").stringValue = name;
            so.FindProperty("description").stringValue = desc;
            so.FindProperty("terrainColor").colorValue = color;
            so.FindProperty("movementCost").floatValue = moveCost;
            so.FindProperty("defenseBonus").floatValue = defense;
            so.FindProperty("isPassable").boolValue = passable;
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(terrain, path);
        }
    }
}
