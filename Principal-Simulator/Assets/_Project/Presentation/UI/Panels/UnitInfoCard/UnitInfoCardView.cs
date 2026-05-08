using UnityEngine;
using UnityEngine.UI;
using TBS.Unit;
using TBS.UnitSystem;

namespace TBS.Presentation.UI.Panels.UnitInfoCard
{
    public class UnitInfoCardView : MonoBehaviour
    {
        [SerializeField] private Image _factionFlag;
        [SerializeField] private Text _unitNameText;
        [SerializeField] private Text _strengthText;
        [SerializeField] private Text _moraleText;
        [SerializeField] private Text _supplyText;
        [SerializeField] private Slider _strengthSlider;
        [SerializeField] private Slider _moraleSlider;
        [SerializeField] private Slider _supplySlider;

        [Header("缩放设置")]
        [SerializeField] private float _minScale = 0.2f;
        [SerializeField] private float _maxScale = 1f;
        [SerializeField] private float _minCameraHeight = 5f;
        [SerializeField] private float _maxCameraHeight = 50f;

        public UnitToken TrackedUnit { get; private set; }
        private RectTransform _rectTransform;
        private UnityEngine.Camera _cam;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _cam = UnityEngine.Camera.main;
        }

        public void Bind(UnitToken unit)
        {
            TrackedUnit = unit;
            Refresh();
        }

        public void Refresh()
        {
            if (TrackedUnit == null || TrackedUnit.UnitLogic == null) return;
            var unit = TrackedUnit.UnitLogic;

            // 更新文本
            _unitNameText.text = unit.DisplayName;
            _strengthText.text = $"兵力:{unit.Strength}";
            _moraleText.text = $"士气:{unit.Morale}";
            _supplyText.text = $"弹药:{unit.Supply}";

            // 更新 Slider
            _strengthSlider.value = unit.Strength / 5f;
            _moraleSlider.value = unit.Morale / 100f;
            _supplySlider.value = unit.Supply / 5f;

            // 更新旗帜
            UpdateFactionFlag(unit.Faction);
        }

        private void UpdateFactionFlag(Faction faction)
        {
            string flagSpritePath = faction switch
            {
                Faction.KMT => "Sprites/Flags/KMT",
                Faction.PLA => "Sprites/Flags/PLA",
                Faction.Japan => "Sprites/Flags/Japan",
                _ => "Sprites/Flags/KMT"
            };

            var sprite = Resources.Load<Sprite>(flagSpritePath);
            if (sprite != null)
            {
                _factionFlag.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"[UnitInfoCardView] 无法加载旗帜图片: {flagSpritePath}，使用纯色代替");
                _factionFlag.sprite = null;
                _factionFlag.color = faction switch
                {
                    Faction.KMT => new Color(0.8f, 0.2f, 0.2f, 1f),
                    Faction.PLA => new Color(0.9f, 0.1f, 0.1f, 1f),
                    Faction.Japan => new Color(0.7f, 0.7f, 0.7f, 1f),
                    _ => new Color(0.5f, 0.5f, 0.5f, 1f)
                };
            }
        }

        void LateUpdate()
        {
            if (_cam == null || _rectTransform == null) return;

            // 根据相机高度计算动态缩放倍数（从_maxScale到_minScale）
            float cameraHeight = _cam.transform.position.y;
            float normalizedHeight = Mathf.Clamp01((cameraHeight - _minCameraHeight) / (_maxCameraHeight - _minCameraHeight));
            float dynamicScale = Mathf.Lerp(_maxScale, _minScale, normalizedHeight);

            // 基础缩放 * 动态缩放 = 最终缩放
            float baseScale = 0.5f;
            float finalScale = baseScale * dynamicScale;

            _rectTransform.localScale = new Vector3(finalScale, finalScale, 1f);
        }

        public void SetUIElements(Image factionFlag, Text unitNameText, Text strengthText, Text moraleText, Text supplyText, Slider strengthSlider, Slider moraleSlider, Slider supplySlider)
        {
            _factionFlag = factionFlag;
            _unitNameText = unitNameText;
            _strengthText = strengthText;
            _moraleText = moraleText;
            _supplyText = supplyText;
            _strengthSlider = strengthSlider;
            _moraleSlider = moraleSlider;
            _supplySlider = supplySlider;
        }
    }
}

