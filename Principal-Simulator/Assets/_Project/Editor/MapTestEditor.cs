using TBS.Map.Data;
using TBS.Map.Managers;
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
        private MapManager mapManager;
        private int mapWidth = 10;
        private int mapHeight = 8;

        [MenuItem("Tools/Map System/Test")]
        public static void ShowWindow()
        {
            GetWindow<MapTestEditor>("地图系统测试");
        }

        private void OnGUI()
        {
            GUILayout.Label("地图系统测试工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 查找MapManager
            mapManager = MapManager.Instance;
            if (mapManager == null)
                mapManager = FindObjectOfType<MapManager>();
            
            if (mapManager == null)
            {
                EditorGUILayout.HelpBox("场景中未找到MapManager，请先创建MapManager", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"找到MapManager: {mapManager.name}\n地块数: {mapManager.Tiles.Count}", MessageType.Info);
            }

            EditorGUILayout.Space();

            // 状态显示
            if (mapManager != null)
            {
                GUILayout.Label("状态", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("地块数量:", mapManager.Tiles.Count.ToString());
                EditorGUILayout.LabelField("HexSize:", mapManager.HexSize.ToString());
            }
        }
    }
}
