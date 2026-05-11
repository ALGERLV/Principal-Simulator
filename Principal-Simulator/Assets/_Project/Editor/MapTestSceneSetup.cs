using UnityEngine;
using UnityEditor;
using TBS.Map.Managers;
using TBS.Map.Data;
using TBS.Presentation.Camera;

using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Editor
{
    /// <summary>
    /// 地图测试场景设置工具
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
            GUILayout.Label("地图测试场景设置", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox("请拖拽以下资源到对应字段", MessageType.Info);
            EditorGUILayout.Space(10);

            gridSetting = EditorGUILayout.ObjectField("网格配置", gridSetting, typeof(MapGridSetting), false) as MapGridSetting;
            routeSetting = EditorGUILayout.ObjectField("道路配置", routeSetting, typeof(MapRouteSetting), false) as MapRouteSetting;
            skinSetting = EditorGUILayout.ObjectField("皮肤配置", skinSetting, typeof(MapSkinSetting), false) as MapSkinSetting;
            defaultTerrain = EditorGUILayout.ObjectField("默认地形", defaultTerrain, typeof(TerrainData), false) as TerrainData;
            tilePrefab = EditorGUILayout.ObjectField("地块预制体", tilePrefab, typeof(GameObject), false) as GameObject;

            EditorGUILayout.Space(20);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("设置场景", GUILayout.Height(40)))
            {
                SetupScene();
            }
            GUI.backgroundColor = Color.white;
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
                managerSO.FindProperty("gridSetting").objectReferenceValue = gridSetting;
            if (routeSetting != null)
                managerSO.FindProperty("routeSetting").objectReferenceValue = routeSetting;
            if (skinSetting != null)
                managerSO.FindProperty("skinSetting").objectReferenceValue = skinSetting;
            if (tilePrefab != null)
                managerSO.FindProperty("tilePrefab").objectReferenceValue = tilePrefab;

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
            }

            // 选择 MapManager
            Selection.activeGameObject = mapManagerGO;

            EditorUtility.DisplayDialog("完成", "地图测试场景设置完成！\n\n点击Play运行测试。", "确定");
        }
    }
}
