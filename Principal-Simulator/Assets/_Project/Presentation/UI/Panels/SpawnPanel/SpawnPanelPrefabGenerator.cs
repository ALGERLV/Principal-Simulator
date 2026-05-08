using UnityEngine;
using UnityEngine.UI;

namespace TBS.Presentation.UI.Panels.SpawnPanel
{
    public class SpawnPanelPrefabGenerator : MonoBehaviour
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/生成UI预制体/生成SpawnPanel")]
        public static void GenerateSpawnPanelPrefab()
        {
            string prefabPath = "Assets/Resources/Prefabs/UI/SpawnPanel/SpawnPanel.prefab";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(prefabPath));

            var existingPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                if (!UnityEditor.EditorUtility.DisplayDialog("预制体已存在",
                    $"预制体已存在: {prefabPath}\n是否覆盖?", "覆盖", "取消"))
                {
                    return;
                }
            }

            GameObject canvasGO = null;
            try
            {
                // 创建Canvas根节点
                canvasGO = new GameObject("SpawnPanel");
                canvasGO.hideFlags = HideFlags.HideAndDontSave;
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();

                var rectTransform = canvasGO.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                // 添加SpawnPanelView组件
                var view = canvasGO.AddComponent<SpawnPanelView>();
                view.SetUIId("SpawnPanelView");

                // 创建右侧面板
                var panelGO = new GameObject("Panel");
                panelGO.transform.SetParent(canvasGO.transform, false);
                var panelImage = panelGO.AddComponent<Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
                var panelRect = panelGO.GetComponent<RectTransform>();
                panelRect.anchoredPosition = new Vector2(-100, 0);
                panelRect.sizeDelta = new Vector2(200, 300);
                panelRect.anchorMin = new Vector2(1f, 0.5f);
                panelRect.anchorMax = new Vector2(1f, 0.5f);

                // 标题
                var titleGO = new GameObject("Title");
                titleGO.transform.SetParent(panelGO.transform, false);
                var titleText = titleGO.AddComponent<Text>();
                titleText.text = "单位列表";
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                titleText.fontSize = 18;
                titleText.fontStyle = FontStyle.Bold;
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.color = Color.white;
                var titleRect = titleGO.GetComponent<RectTransform>();
                titleRect.anchoredPosition = new Vector2(0, 120);
                titleRect.sizeDelta = new Vector2(180, 30);

                // 列表容器
                var containerGO = new GameObject("ListContainer");
                containerGO.transform.SetParent(panelGO.transform, false);
                var containerRect = containerGO.AddComponent<RectTransform>();
                containerRect.anchoredPosition = new Vector2(0, 40);
                containerRect.sizeDelta = new Vector2(180, 160);

                // 状态文本
                var statusGO = new GameObject("StatusText");
                statusGO.transform.SetParent(panelGO.transform, false);
                var statusText = statusGO.AddComponent<Text>();
                statusText.text = "请选择单位";
                statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                statusText.fontSize = 12;
                statusText.alignment = TextAnchor.MiddleCenter;
                statusText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
                var statusRect = statusGO.GetComponent<RectTransform>();
                statusRect.anchoredPosition = new Vector2(0, -130);
                statusRect.sizeDelta = new Vector2(180, 50);

                // 关联View中的引用
                view.SetUIElements(containerGO.transform, statusText);

                // 保存预制体
                canvasGO.hideFlags = HideFlags.None;
                string assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(prefabPath);
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(canvasGO, assetPath);

                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.EditorUtility.DisplayDialog("成功", $"SpawnPanel预制体已生成到: {assetPath}", "确定");
            }
            catch (System.Exception ex)
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", $"生成Prefab失败: {ex.Message}", "确定");
            }
            finally
            {
                if (canvasGO != null)
                    DestroyImmediate(canvasGO);
            }
        }

#endif
    }
}
