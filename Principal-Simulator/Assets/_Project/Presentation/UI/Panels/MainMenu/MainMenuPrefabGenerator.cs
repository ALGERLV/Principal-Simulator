using UnityEngine;
using UnityEngine.UI;

namespace TBS.Presentation.UI.Panels.MainMenu
{
    /// <summary>
    /// 用于生成MainMenu Prefab的编辑器脚本
    /// 在Unity编辑器中运行此脚本即可自动生成完整的MainMenu界面Prefab到工程目录
    /// </summary>
    public class MainMenuPrefabGenerator : MonoBehaviour
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/生成UI预制体/生成MainMenu")]
        public static void GenerateMainMenuPrefab()
        {
            // 定义Prefab保存路径（直接生成到Resources文件夹）
            string prefabPath = "Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(prefabPath));

            // 检查预制体是否已存在
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
                canvasGO = new GameObject("MainMenu");
                canvasGO.hideFlags = HideFlags.HideAndDontSave; // 防止保存到场景中
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();

                var rectTransform = canvasGO.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                // 添加MainMenuView组件
                var view = canvasGO.AddComponent<MainMenuView>();
                view.SetUIId("MainMenuView");

                // 创建背景
                var bgGO = new GameObject("Background");
                bgGO.transform.SetParent(canvasGO.transform, false);
                var bgImage = bgGO.AddComponent<Image>();
                bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                var bgRect = bgGO.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;

                // 创建标题文本
                var titleGO = new GameObject("Title");
                titleGO.transform.SetParent(canvasGO.transform, false);
                var titleText = titleGO.AddComponent<Text>();
                titleText.text = "电子战棋";
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                titleText.fontSize = 60;
                titleText.fontStyle = FontStyle.Bold;
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.color = Color.white;
                var titleRect = titleGO.GetComponent<RectTransform>();
                titleRect.anchoredPosition = new Vector2(0, 150);
                titleRect.sizeDelta = new Vector2(800, 100);

                // 创建开始按钮
                var startBtnGO = new GameObject("StartButton");
                startBtnGO.transform.SetParent(canvasGO.transform, false);
                var startButton = startBtnGO.AddComponent<Button>();
                startButton.targetGraphic = startBtnGO.AddComponent<Image>();
                startButton.targetGraphic.color = new Color(0.2f, 0.5f, 0.9f, 1f);
                var startBtnRect = startBtnGO.GetComponent<RectTransform>();
                startBtnRect.anchoredPosition = new Vector2(0, 50);
                startBtnRect.sizeDelta = new Vector2(250, 80);

                // 添加按钮的颜色转换效果
                var startColors = startButton.colors;
                startColors.normalColor = new Color(0.2f, 0.5f, 0.9f, 1f);
                startColors.highlightedColor = new Color(0.3f, 0.6f, 1f, 1f);
                startColors.pressedColor = new Color(0.15f, 0.4f, 0.8f, 1f);
                startButton.colors = startColors;

                var startTextGO = new GameObject("Text");
                startTextGO.transform.SetParent(startBtnGO.transform, false);
                var startBtnText = startTextGO.AddComponent<Text>();
                startBtnText.text = "开始游戏";
                startBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                startBtnText.fontSize = 36;
                startBtnText.fontStyle = FontStyle.Bold;
                startBtnText.alignment = TextAnchor.MiddleCenter;
                startBtnText.color = Color.white;
                var startTextRect = startTextGO.GetComponent<RectTransform>();
                startTextRect.anchorMin = Vector2.zero;
                startTextRect.anchorMax = Vector2.one;
                startTextRect.offsetMin = Vector2.zero;
                startTextRect.offsetMax = Vector2.zero;

                // 创建退出按钮
                var exitBtnGO = new GameObject("ExitButton");
                exitBtnGO.transform.SetParent(canvasGO.transform, false);
                var exitButton = exitBtnGO.AddComponent<Button>();
                exitButton.targetGraphic = exitBtnGO.AddComponent<Image>();
                exitButton.targetGraphic.color = new Color(0.9f, 0.3f, 0.3f, 1f);
                var exitBtnRect = exitBtnGO.GetComponent<RectTransform>();
                exitBtnRect.anchoredPosition = new Vector2(0, -50);
                exitBtnRect.sizeDelta = new Vector2(250, 80);

                // 添加按钮的颜色转换效果
                var exitColors = exitButton.colors;
                exitColors.normalColor = new Color(0.9f, 0.3f, 0.3f, 1f);
                exitColors.highlightedColor = new Color(1f, 0.4f, 0.4f, 1f);
                exitColors.pressedColor = new Color(0.8f, 0.2f, 0.2f, 1f);
                exitButton.colors = exitColors;

                var exitTextGO = new GameObject("Text");
                exitTextGO.transform.SetParent(exitBtnGO.transform, false);
                var exitBtnText = exitTextGO.AddComponent<Text>();
                exitBtnText.text = "退出游戏";
                exitBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                exitBtnText.fontSize = 36;
                exitBtnText.fontStyle = FontStyle.Bold;
                exitBtnText.alignment = TextAnchor.MiddleCenter;
                exitBtnText.color = Color.white;
                var exitTextRect = exitTextGO.GetComponent<RectTransform>();
                exitTextRect.anchorMin = Vector2.zero;
                exitTextRect.anchorMax = Vector2.one;
                exitTextRect.offsetMin = Vector2.zero;
                exitTextRect.offsetMax = Vector2.zero;

                // 关联View中的引用
                view.SetUIElements(titleText, startButton, exitButton);

                // 保存预制体到工程目录（清除HideFlags以允许保存）
                canvasGO.hideFlags = HideFlags.None;
                string assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(prefabPath);
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(canvasGO, assetPath);

                // 刷新资源数据库
                UnityEditor.AssetDatabase.Refresh();

                UnityEditor.EditorUtility.DisplayDialog("成功", $"MainMenu预制体已生成到: {assetPath}", "确定");
            }
            catch (System.Exception ex)
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", $"生成Prefab失败: {ex.Message}", "确定");
            }
            finally
            {
                // 确保临时GameObject一定会被销毁
                if (canvasGO != null)
                {
                    DestroyImmediate(canvasGO);
                }
            }
        }
#endif
    }
}
