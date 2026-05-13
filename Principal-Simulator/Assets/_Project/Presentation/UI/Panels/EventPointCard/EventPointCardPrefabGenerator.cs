using UnityEngine;
using UnityEngine.UI;

namespace TBS.Presentation.UI.Panels.EventPointCard
{
#if UNITY_EDITOR
    public class EventPointCardPrefabGenerator : MonoBehaviour
    {
        [UnityEditor.MenuItem("Tools/生成UI预制体/生成EventPointCard")]
        public static void GenerateEventPointCardPrefab()
        {
            string prefabPath = "Assets/Resources/Prefabs/UI/EventPointCard.prefab";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(prefabPath));

            var existingPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                if (!UnityEditor.EditorUtility.DisplayDialog("预制体已存在",
                    $"预制体已存在: {prefabPath}\n是否覆盖?", "覆盖", "取消"))
                    return;
            }

            GameObject cardGO = null;
            try
            {
                cardGO = new GameObject("EventPointCard");
                cardGO.hideFlags = HideFlags.HideAndDontSave;
                var cardRect = cardGO.AddComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(140, 60);

                // 背景
                var bgImage = cardGO.AddComponent<Image>();
                bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

                // 添加View组件
                var view = cardGO.AddComponent<EventPointCardView>();

                // 左侧类型色条
                var iconGO = new GameObject("TypeIcon");
                iconGO.transform.SetParent(cardGO.transform, false);
                var iconImage = iconGO.AddComponent<Image>();
                iconImage.color = Color.white;
                var iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchoredPosition = new Vector2(-60, 0);
                iconRect.sizeDelta = new Vector2(8, 50);
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);

                // 事件点名称
                var nameGO = new GameObject("PointNameText");
                nameGO.transform.SetParent(cardGO.transform, false);
                var nameText = nameGO.AddComponent<Text>();
                nameText.text = "事件点";
                nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameText.fontSize = 12;
                nameText.fontStyle = FontStyle.Bold;
                nameText.alignment = TextAnchor.MiddleCenter;
                nameText.color = Color.white;
                nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
                var nameRect = nameGO.GetComponent<RectTransform>();
                nameRect.anchoredPosition = new Vector2(5, 10);
                nameRect.sizeDelta = new Vector2(110, 22);
                nameRect.anchorMin = new Vector2(0.5f, 0.5f);
                nameRect.anchorMax = new Vector2(0.5f, 0.5f);

                // 积分/类型文字
                var scoreGO = new GameObject("ScoreText");
                scoreGO.transform.SetParent(cardGO.transform, false);
                var scoreText = scoreGO.AddComponent<Text>();
                scoreText.text = "0 VP";
                scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                scoreText.fontSize = 10;
                scoreText.alignment = TextAnchor.MiddleCenter;
                scoreText.color = new Color(1f, 0.9f, 0.3f);
                scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
                var scoreRect = scoreGO.GetComponent<RectTransform>();
                scoreRect.anchoredPosition = new Vector2(5, -12);
                scoreRect.sizeDelta = new Vector2(110, 18);
                scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
                scoreRect.anchorMax = new Vector2(0.5f, 0.5f);

                // 关联组件
                view.SetUIElements(iconImage, nameText, scoreText, bgImage);

                // 保存
                cardGO.hideFlags = HideFlags.None;
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(cardGO, prefabPath);
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.EditorUtility.DisplayDialog("成功", $"EventPointCard预制体已生成到: {prefabPath}", "确定");
            }
            catch (System.Exception ex)
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", $"生成Prefab失败: {ex.Message}", "确定");
            }
            finally
            {
                if (cardGO != null)
                    DestroyImmediate(cardGO);
            }
        }
    }
#endif
}
