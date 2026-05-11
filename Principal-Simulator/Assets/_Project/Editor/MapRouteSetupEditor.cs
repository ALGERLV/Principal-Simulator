using System.Collections.Generic;
using TBS.Map.Data;
using TBS.Map.Runtime;
using TBS.Map.Tools;
using UnityEngine;
using UnityEditor;

namespace TBS.Map.Editor
{
    /// <summary>
    /// 地图河流/道路配置工具 - 快速创建测试用的连接
    /// </summary>
    public class MapRouteSetupEditor : EditorWindow
    {
        private MapRouteSetting routeSetting;
        private MapRouteType routeType = MapRouteType.River;
        private float width = 0.3f;
        private bool autoGenerateLinks = true;
        private int linkCount = 5;

        [MenuItem("TBS/Map/Setup Routes (河流/道路)")]
        public static void ShowWindow()
        {
            GetWindow<MapRouteSetupEditor>("路线配置");
        }

        private void OnGUI()
        {
            GUILayout.Label("河流/道路配置工具", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            routeSetting = EditorGUILayout.ObjectField("路线配置", routeSetting, typeof(MapRouteSetting), false) as MapRouteSetting;
            routeType = (MapRouteType)EditorGUILayout.EnumPopup("路线类型", routeType);
            width = EditorGUILayout.FloatField("路线宽度", width);

            EditorGUILayout.Space(10);

            autoGenerateLinks = EditorGUILayout.Toggle("自动生成连接", autoGenerateLinks);
            if (autoGenerateLinks)
            {
                linkCount = EditorGUILayout.IntSlider("连接数量", linkCount, 1, 20);
            }

            EditorGUILayout.Space(20);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("生成测试路线", GUILayout.Height(40)))
            {
                GenerateTestRoutes();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("清空所有连接", GUILayout.Height(30)))
            {
                ClearAllLinks();
            }
        }

        private void GenerateTestRoutes()
        {
            if (routeSetting == null)
            {
                EditorUtility.DisplayDialog("错误", "请先创建或选择一个 MapRouteSetting 配置文件！", "确定");
                return;
            }

            // 生成测试用的连接数据
            var links = new List<MapLinkType>();

            if (autoGenerateLinks)
            {
                // 生成随机连接
                for (int i = 0; i < linkCount; i++)
                {
                    var from = new MapHexCoord(Random.Range(0, 8), Random.Range(0, 6));
                    var to = new MapHexCoord(Random.Range(0, 8), Random.Range(0, 6));

                    if (from != to)
                    {
                        links.Add(new MapLinkType
                        {
                            From = from,
                            To = to,
                            Type = routeType,
                            WidthWorld = width,
                            Passable = routeType == MapRouteType.Road
                        });
                    }
                }
            }
            else
            {
                // 手动创建一些固定的测试连接
                // 水平河流
                links.Add(new MapLinkType
                {
                    From = new MapHexCoord(1, 3),
                    To = new MapHexCoord(4, 3),
                    Type = MapRouteType.River,
                    WidthWorld = 0.4f,
                    Passable = false
                });

                // 垂直道路
                links.Add(new MapLinkType
                {
                    From = new MapHexCoord(3, 1),
                    To = new MapHexCoord(3, 5),
                    Type = MapRouteType.Road,
                    WidthWorld = 0.3f,
                    Passable = true
                });

                // 对角线道路
                links.Add(new MapLinkType
                {
                    From = new MapHexCoord(0, 0),
                    To = new MapHexCoord(2, 2),
                    Type = MapRouteType.Road,
                    WidthWorld = 0.25f,
                    Passable = true
                });
            }

            // 添加到配置
            foreach (var link in links)
            {
                routeSetting.AddLink(link);
            }

            EditorUtility.SetDirty(routeSetting);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", $"已生成 {links.Count} 条路线连接！", "确定");
        }

        private void ClearAllLinks()
        {
            if (routeSetting == null) return;

            if (EditorUtility.DisplayDialog("确认", "确定要清空所有路线连接吗？", "是", "否"))
            {
                routeSetting.ClearLinks();
                EditorUtility.SetDirty(routeSetting);
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Assets/Create/Game/Map Route Setting")]
        public static void CreateRouteSettingAsset()
        {
            var setting = ScriptableObject.CreateInstance<MapRouteSetting>();

            string path = "Assets/Resources/MapConfigs/TestRouteSetting.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

            AssetDatabase.CreateAsset(setting, path);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", "已创建 MapRouteSetting 配置文件！", "确定");
            Selection.activeObject = setting;
        }
    }
}
