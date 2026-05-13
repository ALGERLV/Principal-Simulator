using UnityEngine;
using UnityEngine.UI;
using TBS.Map.Data;

namespace TBS.Presentation.UI.Panels.EventPointCard
{
    public class EventPointCardView : MonoBehaviour
    {
        [SerializeField] private Image _typeIcon;
        [SerializeField] private Text _pointNameText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Image _background;

        [Header("缩放设置")]
        [SerializeField] private float _minScale = 0.2f;
        [SerializeField] private float _maxScale = 1f;
        [SerializeField] private float _minCameraHeight = 5f;
        [SerializeField] private float _maxCameraHeight = 50f;

        public Transform TrackedTarget { get; private set; }
        public MapEventPointType PointType { get; private set; }

        private RectTransform _rectTransform;
        private UnityEngine.Camera _cam;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _cam = UnityEngine.Camera.main;
        }

        public void Bind(Transform target, MapEventPointData data)
        {
            TrackedTarget = target;
            PointType = data.PointType;

            _pointNameText.text = data.PointName;

            switch (data.PointType)
            {
                case MapEventPointType.KMTReinforcement:
                    _typeIcon.color = new Color(0.1f, 0.3f, 0.8f);
                    _background.color = new Color(0.05f, 0.1f, 0.3f, 0.9f);
                    _scoreText.text = "增援";
                    _scoreText.color = new Color(0.4f, 0.6f, 1f);
                    break;
                case MapEventPointType.JapanReinforcement:
                    _typeIcon.color = new Color(0.8f, 0.1f, 0.1f);
                    _background.color = new Color(0.3f, 0.05f, 0.05f, 0.9f);
                    _scoreText.text = "增援";
                    _scoreText.color = new Color(1f, 0.4f, 0.4f);
                    break;
                case MapEventPointType.VictoryPoint:
                    _typeIcon.color = new Color(0.9f, 0.8f, 0.1f);
                    _background.color = new Color(0.2f, 0.18f, 0.05f, 0.9f);
                    _scoreText.text = $"{data.ScoreValue} VP";
                    _scoreText.color = new Color(1f, 0.9f, 0.3f);
                    break;
            }
        }

        void LateUpdate()
        {
            if (_cam == null || _rectTransform == null) return;

            float cameraHeight = _cam.transform.position.y;
            float normalizedHeight = Mathf.Clamp01((cameraHeight - _minCameraHeight) / (_maxCameraHeight - _minCameraHeight));
            float dynamicScale = Mathf.Lerp(_maxScale, _minScale, normalizedHeight);
            float baseScale = 0.5f;
            float finalScale = baseScale * dynamicScale;

            _rectTransform.localScale = new Vector3(finalScale, finalScale, 1f);
        }

        public void SetUIElements(Image typeIcon, Text pointNameText, Text scoreText, Image background)
        {
            _typeIcon = typeIcon;
            _pointNameText = pointNameText;
            _scoreText = scoreText;
            _background = background;
        }
    }
}
