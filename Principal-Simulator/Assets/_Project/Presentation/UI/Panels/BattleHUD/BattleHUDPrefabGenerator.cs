using UnityEngine;
using UnityEngine.UI;

namespace TBS.Presentation.UI.Panels.BattleHUD
{
    public class BattleHUDPrefabGenerator : MonoBehaviour
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/生成UI预制体/生成BattleHUD")]
        public static void GenerateBattleHUDPrefab()
        {
            string prefabPath = "Assets/Resources/Prefabs/UI/BattleHUD/BattleHUD.prefab";
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
                canvasGO = new GameObject("BattleHUD");
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

                // 添加BattleHUDView组件
                var view = canvasGO.AddComponent<BattleHUDView>();
                view.SetUIId("BattleHUDView");

                // 创建顶部横幅背景
                var topBarGO = new GameObject("TopBar");
                topBarGO.transform.SetParent(canvasGO.transform, false);
                var topBarImage = topBarGO.AddComponent<Image>();
                topBarImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // 深色半透明
                var topBarRect = topBarGO.GetComponent<RectTransform>();
                topBarRect.anchoredPosition = new Vector2(0, -25);
                topBarRect.sizeDelta = new Vector2(960, 50);
                topBarRect.anchorMin = new Vector2(0.5f, 1f);
                topBarRect.anchorMax = new Vector2(0.5f, 1f);

                // 单位信息卡容器
                var cardsContainerGO = new GameObject("UnitInfoCardsContainer");
                cardsContainerGO.transform.SetParent(canvasGO.transform, false);
                var cardsContainerRect = cardsContainerGO.AddComponent<RectTransform>();
                cardsContainerRect.anchorMin = Vector2.zero;
                cardsContainerRect.anchorMax = Vector2.one;
                cardsContainerRect.offsetMin = Vector2.zero;
                cardsContainerRect.offsetMax = Vector2.zero;

                // 标题文本（左）
                var titleGO = new GameObject("TitleText");
                titleGO.transform.SetParent(topBarGO.transform, false);
                var titleText = titleGO.AddComponent<Text>();
                titleText.text = "电子战棋";
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                titleText.fontSize = 28;
                titleText.fontStyle = FontStyle.Bold;
                titleText.alignment = TextAnchor.MiddleLeft;
                titleText.color = Color.white;
                var titleRect = titleGO.GetComponent<RectTransform>();
                titleRect.anchoredPosition = new Vector2(-420, 0);
                titleRect.sizeDelta = new Vector2(200, 50);
                titleRect.anchorMin = Vector2.zero;
                titleRect.anchorMax = Vector2.zero;

                // 日期文本（中）
                var dayGO = new GameObject("DayText");
                dayGO.transform.SetParent(topBarGO.transform, false);
                var dayText = dayGO.AddComponent<Text>();
                dayText.text = "第 1 天";
                dayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                dayText.fontSize = 24;
                dayText.fontStyle = FontStyle.Normal;
                dayText.alignment = TextAnchor.MiddleCenter;
                dayText.color = Color.white;
                var dayRect = dayGO.GetComponent<RectTransform>();
                dayRect.anchoredPosition = new Vector2(0, 0);
                dayRect.sizeDelta = new Vector2(200, 50);

                // 菜单按钮（右）
                var menuBtnGO = new GameObject("MenuButton");
                menuBtnGO.transform.SetParent(topBarGO.transform, false);
                var menuButton = menuBtnGO.AddComponent<Button>();
                var menuImage = menuBtnGO.AddComponent<Image>();
                menuImage.color = new Color(0.2f, 0.5f, 0.9f, 1f);
                menuButton.targetGraphic = menuImage;
                var menuButtonColors = menuButton.colors;
                menuButtonColors.normalColor = new Color(0.2f, 0.5f, 0.9f, 1f);
                menuButtonColors.highlightedColor = new Color(0.3f, 0.6f, 1f, 1f);
                menuButtonColors.pressedColor = new Color(0.15f, 0.4f, 0.8f, 1f);
                menuButton.colors = menuButtonColors;
                var menuBtnRect = menuBtnGO.GetComponent<RectTransform>();
                menuBtnRect.anchoredPosition = new Vector2(420, 0);
                menuBtnRect.sizeDelta = new Vector2(100, 40);
                menuBtnRect.anchorMin = Vector2.zero;
                menuBtnRect.anchorMax = Vector2.zero;

                // 菜单按钮文本
                var menuTextGO = new GameObject("Text");
                menuTextGO.transform.SetParent(menuBtnGO.transform, false);
                var menuBtnText = menuTextGO.AddComponent<Text>();
                menuBtnText.text = "菜单";
                menuBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                menuBtnText.fontSize = 20;
                menuBtnText.fontStyle = FontStyle.Bold;
                menuBtnText.alignment = TextAnchor.MiddleCenter;
                menuBtnText.color = Color.white;
                var menuTextRect = menuTextGO.GetComponent<RectTransform>();
                menuTextRect.anchorMin = Vector2.zero;
                menuTextRect.anchorMax = Vector2.one;
                menuTextRect.offsetMin = Vector2.zero;
                menuTextRect.offsetMax = Vector2.zero;

                // 关联View中的引用
                view.SetUIElements(titleText, dayText, menuButton);

                // 保存预制体
                canvasGO.hideFlags = HideFlags.None;
                string assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(prefabPath);
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(canvasGO, assetPath);

                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.EditorUtility.DisplayDialog("成功", $"BattleHUD预制体已生成到: {assetPath}", "确定");
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
