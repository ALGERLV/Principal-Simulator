using UnityEngine;
using UnityEngine.UI;

namespace TBS.Presentation.UI.Panels.UnitInfoCard
{
    public class UnitInfoCardPrefabGenerator : MonoBehaviour
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/生成UI预制体/生成UnitInfoCard")]
        public static void GenerateUnitInfoCardPrefab()
        {
            string prefabPath = "Assets/Resources/Prefabs/UI/UnitInfoCard/UnitInfoCard.prefab";
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

            GameObject cardGO = null;
            try
            {
                // 创建卡片根节点
                cardGO = new GameObject("UnitInfoCard");
                cardGO.hideFlags = HideFlags.HideAndDontSave;
                var cardRect = cardGO.AddComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(200, 120);

                // 背景
                var bgImage = cardGO.AddComponent<Image>();
                bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

                // 添加 UnitInfoCardView 组件
                var view = cardGO.AddComponent<UnitInfoCardView>();

                // 左侧旗帜
                var flagGO = new GameObject("FactionFlag");
                flagGO.transform.SetParent(cardGO.transform, false);
                var flagImage = flagGO.AddComponent<Image>();
                flagImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
                var flagRect = flagGO.GetComponent<RectTransform>();
                flagRect.anchoredPosition = new Vector2(-95, 35);
                flagRect.sizeDelta = new Vector2(10, 80);
                flagRect.anchorMin = Vector2.zero;
                flagRect.anchorMax = Vector2.zero;

                // 单位名称
                var nameGO = new GameObject("UnitNameText");
                nameGO.transform.SetParent(cardGO.transform, false);
                var nameText = nameGO.AddComponent<Text>();
                nameText.text = "单位";
                nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                nameText.fontSize = 11;
                nameText.fontStyle = FontStyle.Bold;
                nameText.alignment = TextAnchor.UpperCenter;
                nameText.color = Color.white;
                var nameRect = nameGO.GetComponent<RectTransform>();
                nameRect.anchoredPosition = new Vector2(0, 45);
                nameRect.sizeDelta = new Vector2(160, 20);
                nameRect.anchorMin = Vector2.zero;
                nameRect.anchorMax = Vector2.zero;

                // 创建内容容器（用于放置兵力、士气、弹药的行）
                CreateSliderRow(cardGO, "Strength", "兵力:", -5, out var strengthText, out var strengthSlider);
                CreateSliderRow(cardGO, "Morale", "士气:", -30, out var moraleText, out var moraleSlider);
                CreateSliderRow(cardGO, "Supply", "弹药:", -55, out var supplyText, out var supplySlider);

                // 关联组件
                view.SetUIElements(flagImage, nameText, strengthText, moraleText, supplyText, strengthSlider, moraleSlider, supplySlider);

                // 保存预制体
                cardGO.hideFlags = HideFlags.None;
                string assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(prefabPath);
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(cardGO, assetPath);

                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.EditorUtility.DisplayDialog("成功", $"UnitInfoCard预制体已生成到: {assetPath}", "确定");
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

        private static void CreateSliderRow(GameObject parent, string rowName, string labelText, float yPos, out Text labelTextComponent, out Slider slider)
        {
            // 创建行容器
            var rowGO = new GameObject(rowName);
            rowGO.transform.SetParent(parent.transform, false);
            var rowRect = rowGO.AddComponent<RectTransform>();
            rowRect.anchoredPosition = new Vector2(0, yPos);
            rowRect.sizeDelta = new Vector2(180, 18);
            rowRect.anchorMin = Vector2.zero;
            rowRect.anchorMax = Vector2.zero;

            // 标签
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            labelTextComponent = labelGO.AddComponent<Text>();
            labelTextComponent.text = labelText;
            labelTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelTextComponent.fontSize = 9;
            labelTextComponent.alignment = TextAnchor.MiddleLeft;
            labelTextComponent.color = Color.white;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(-75, 0);
            labelRect.sizeDelta = new Vector2(35, 18);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.zero;

            // Slider 背景
            var sliderBgGO = new GameObject("SliderBG");
            sliderBgGO.transform.SetParent(rowGO.transform, false);
            var sliderBgImage = sliderBgGO.AddComponent<Image>();
            sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var sliderBgRect = sliderBgGO.GetComponent<RectTransform>();
            sliderBgRect.anchoredPosition = new Vector2(10, 0);
            sliderBgRect.sizeDelta = new Vector2(100, 8);
            sliderBgRect.anchorMin = Vector2.zero;
            sliderBgRect.anchorMax = Vector2.zero;

            // Slider Handle
            var handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(sliderBgGO.transform, false);
            var handleImage = handleGO.AddComponent<Image>();
            handleImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            var handleRect = handleGO.GetComponent<RectTransform>();
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = new Vector2(8, 8);
            handleRect.anchorMin = new Vector2(0, 0.5f);
            handleRect.anchorMax = new Vector2(0, 0.5f);

            // Slider 组件
            slider = sliderBgGO.AddComponent<Slider>();
            slider.targetGraphic = handleImage;
            slider.handleRect = handleRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;
            slider.interactable = false;
        }

#endif
    }
}
