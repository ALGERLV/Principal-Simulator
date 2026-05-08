using TBS.Map.Data;
using TBS.Map.Runtime;
using UnityEditor;
using UnityEngine;

// 明确指定使用我们的TerrainData，避免与UnityEngine.TerrainData冲突
using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Editor
{
    /// <summary>
    /// 地图系统测试编辑器工具
    /// </summary>
    public class MapTestEditor : EditorWindow
    {
        private MapTerrainGrid hexGrid;
        private int mapWidth = 10;
        private int mapHeight = 8;
        private GameObject tilePrefab;
        private TerrainData defaultTerrain;

        [MenuItem("Tools/Map System/Test")]
        public static void ShowWindow()
        {
            GetWindow<MapTestEditor>("地图系统测试");
        }

        private void OnGUI()
        {
            GUILayout.Label("地图系统测试工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 查找或创建HexGrid
            hexGrid = FindObjectOfType<MapTerrainGrid>();
            if (hexGrid == null)
            {
                EditorGUILayout.HelpBox("场景中未找到HexGrid，点击下面的按钮创建", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"找到HexGrid: {hexGrid.name}\n地块数: {hexGrid.TileCount}", MessageType.Info);
            }

            EditorGUILayout.Space();

            // 生成参数
            GUILayout.Label("生成参数", EditorStyles.boldLabel);
            mapWidth = EditorGUILayout.IntField("宽度", mapWidth);
            mapHeight = EditorGUILayout.IntField("高度", mapHeight);

            EditorGUILayout.Space();

            // 引用设置
            tilePrefab = EditorGUILayout.ObjectField("Tile预制体", tilePrefab, typeof(GameObject), false) as GameObject;
            defaultTerrain = EditorGUILayout.ObjectField("默认地形", defaultTerrain, typeof(TerrainData), false) as TerrainData;

            EditorGUILayout.Space();

            // 操作按钮
            GUILayout.Label("操作", EditorStyles.boldLabel);

            if (GUILayout.Button("创建HexGrid"))
            {
                CreateHexGrid();
            }

            EditorGUI.BeginDisabledGroup(hexGrid == null);

            if (GUILayout.Button("生成矩形地图"))
            {
                GenerateMap();
            }

            if (GUILayout.Button("生成圆形地图"))
            {
                GenerateCircleMap();
            }

            if (GUILayout.Button("清除地图"))
            {
                ClearMap();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            // 状态显示
            if (hexGrid != null)
            {
                GUILayout.Label("状态", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("已初始化:", hexGrid.IsInitialized ? "是" : "否");
                EditorGUILayout.LabelField("地块数量:", hexGrid.TileCount.ToString());
                EditorGUILayout.LabelField("地图边界:", hexGrid.Bounds.ToString());
            }
        }

        private void CreateHexGrid()
        {
            // 检查是否已存在
            var existing = FindObjectOfType<MapTerrainGrid>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorUtility.DisplayDialog("提示", "HexGrid已存在", "确定");
                return;
            }

            // 创建HexGrid游戏对象
            var gridGO = new GameObject("MapTerrainGrid");
            hexGrid = gridGO.AddComponent<MapTerrainGrid>();

            // 设置引用
            SerializedObject so = new SerializedObject(hexGrid);
            so.FindProperty("tilePrefab").objectReferenceValue = tilePrefab;
            so.FindProperty("defaultTerrain").objectReferenceValue = defaultTerrain;
            so.ApplyModifiedProperties();

            // 标记为脏
            EditorUtility.SetDirty(gridGO);
            Undo.RegisterCreatedObjectUndo(gridGO, "创建HexGrid");

            Selection.activeGameObject = gridGO;
            Debug.Log("HexGrid已创建");
        }

        private void GenerateMap()
        {
            if (hexGrid == null) return;

            // 确保有必要的引用
            if (hexGrid.GetType().GetField("tilePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(hexGrid) == null && tilePrefab != null)
            {
                SerializedObject so = new SerializedObject(hexGrid);
                so.FindProperty("tilePrefab").objectReferenceValue = tilePrefab;
                so.ApplyModifiedProperties();
            }

            Undo.RecordObject(hexGrid.gameObject, "生成地图");
            hexGrid.GenerateRectangle(mapWidth, mapHeight);
            EditorUtility.SetDirty(hexGrid);

            Debug.Log($"地图生成完成: {mapWidth}x{mapHeight}, 共{hexGrid.TileCount}个地块");
        }

        private void GenerateCircleMap()
        {
            if (hexGrid == null) return;

            Undo.RecordObject(hexGrid.gameObject, "生成圆形地图");
            hexGrid.GenerateCircle(Mathf.Min(mapWidth, mapHeight) / 2);
            EditorUtility.SetDirty(hexGrid);

            Debug.Log($"圆形地图生成完成: 半径{Mathf.Min(mapWidth, mapHeight) / 2}, 共{hexGrid.TileCount}个地块");
        }

        private void ClearMap()
        {
            if (hexGrid == null) return;

            Undo.RecordObject(hexGrid.gameObject, "清除地图");
            hexGrid.Clear();
            EditorUtility.SetDirty(hexGrid);

            Debug.Log("地图已清除");
        }
    }
}
