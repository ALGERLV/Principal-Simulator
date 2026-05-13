using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TBS.Map.Data;
using TBS.Map.Managers;
using TBS.Map.Runtime;
using TBS.Core.Events;
using TBS.Contracts.Events;

namespace TBS.Presentation.UI.Panels.EventPointCard
{
    public class EventPointCardManager : MonoBehaviour
    {
        [SerializeField] private Canvas _battleHUDCanvas;
        [SerializeField] private RectTransform _cardsContainer;
        private GameObject _cardPrefab;
        private List<EventPointCardView> _cards = new List<EventPointCardView>();
        private UnityEngine.Camera _cam;

        public void Setup(Canvas battleHUDCanvas, RectTransform cardsContainer)
        {
            _battleHUDCanvas = battleHUDCanvas;
            _cardsContainer = cardsContainer;
        }

        void Start()
        {
            _cardPrefab = Resources.Load<GameObject>("Prefabs/UI/EventPointCard");
            _cam = UnityEngine.Camera.main;

            EventBus.On<LevelLoadedEvent>(OnLevelLoaded);

            // 如果地图已在本帧加载完毕（事件先于订阅），主动检查
            TryCreateCardsFromExistingMarkers();
        }

        void OnDestroy()
        {
            EventBus.Off<LevelLoadedEvent>(OnLevelLoaded);
            ClearCards();
        }

        void OnLevelLoaded(LevelLoadedEvent evt)
        {
            var config = evt.LevelConfig;
            if (config == null || config.EventPoints == null) return;

            var markerManager = FindObjectOfType<MapEventPointManager>();
            if (markerManager == null || markerManager.ActivePoints.Count == 0)
            {
                Debug.LogWarning("[EventPointCardManager] 未找到 MapEventPointManager 或无标记");
                return;
            }

            SpawnCardsFromMarkers(config, markerManager);
        }

        void TryCreateCardsFromExistingMarkers()
        {
            var markerManager = FindObjectOfType<MapEventPointManager>();
            if (markerManager == null || markerManager.ActivePoints.Count == 0) return;

            // 查找当前关卡配置（从 MapManager 关联的 LevelConfig）
            // 通过匹配已生成的 marker 数据直接创建卡片
            var markers = markerManager.ActivePoints;
            if (_cardsContainer == null || _cards.Count > 0) return;

            if (_cam == null) _cam = UnityEngine.Camera.main;

            foreach (var marker in markers)
            {
                var data = new MapEventPointData
                {
                    Q = marker.Coord.Q,
                    R = marker.Coord.R,
                    PointType = marker.PointType,
                    PointName = marker.PointName,
                    ScoreValue = marker.ScoreValue
                };

                GameObject cardGo;
                if (_cardPrefab != null)
                    cardGo = Instantiate(_cardPrefab, _cardsContainer);
                else
                {
                    cardGo = CreateCardFallback();
                    cardGo.transform.SetParent(_cardsContainer, false);
                }

                var card = cardGo.GetComponent<EventPointCardView>();
                if (card == null)
                {
                    Destroy(cardGo);
                    continue;
                }

                card.Bind(marker.transform, data);
                _cards.Add(card);
            }

            if (_cards.Count > 0)
                Debug.Log($"[EventPointCardManager] 从已有标记生成了 {_cards.Count} 个事件点卡片");
        }

        void SpawnCardsFromMarkers(LevelConfig config, MapEventPointManager markerManager)
        {
            ClearCards();

            if (_cardsContainer == null)
            {
                Debug.LogError("[EventPointCardManager] CardsContainer为空");
                return;
            }

            if (_cam == null)
                _cam = UnityEngine.Camera.main;

            var markers = markerManager.ActivePoints;
            for (int i = 0; i < config.EventPoints.Count && i < markers.Count; i++)
            {
                var data = config.EventPoints[i];
                var marker = markers[i];

                GameObject cardGo;
                if (_cardPrefab != null)
                    cardGo = Instantiate(_cardPrefab, _cardsContainer);
                else
                {
                    cardGo = CreateCardFallback();
                    cardGo.transform.SetParent(_cardsContainer, false);
                }

                var card = cardGo.GetComponent<EventPointCardView>();
                if (card == null)
                {
                    Destroy(cardGo);
                    continue;
                }

                card.Bind(marker.transform, data);
                _cards.Add(card);
            }

            Debug.Log($"[EventPointCardManager] 生成了 {_cards.Count} 个事件点卡片");
        }

        void ClearCards()
        {
            foreach (var card in _cards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            _cards.Clear();
        }

        void LateUpdate()
        {
            if (_cardsContainer == null || _cam == null) return;

            for (int i = _cards.Count - 1; i >= 0; i--)
            {
                var card = _cards[i];
                if (card == null || card.TrackedTarget == null)
                {
                    _cards.RemoveAt(i);
                    continue;
                }

                Vector3 screenPos = _cam.WorldToScreenPoint(card.TrackedTarget.position);
                if (screenPos.z < 0)
                {
                    card.gameObject.SetActive(false);
                    continue;
                }

                card.gameObject.SetActive(true);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _cardsContainer, screenPos, null, out Vector2 localPos))
                {
                    var cardRect = card.GetComponent<RectTransform>();
                    cardRect.anchoredPosition = localPos + new Vector2(0, 40);
                }
            }
        }

        GameObject CreateCardFallback()
        {
            var go = new GameObject("EventPointCard");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(140, 60);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            var view = go.AddComponent<EventPointCardView>();

            var iconGo = new GameObject("TypeIcon");
            iconGo.transform.SetParent(go.transform, false);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.color = Color.white;
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchoredPosition = new Vector2(-55, 0);
            iconRt.sizeDelta = new Vector2(8, 50);

            var nameGo = new GameObject("PointName");
            nameGo.transform.SetParent(go.transform, false);
            var nameText = nameGo.AddComponent<Text>();
            nameText.text = "";
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 12;
            nameText.fontStyle = FontStyle.Bold;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = Color.white;
            var nameRt = nameGo.GetComponent<RectTransform>();
            nameRt.anchoredPosition = new Vector2(5, 10);
            nameRt.sizeDelta = new Vector2(110, 20);

            var scoreGo = new GameObject("ScoreText");
            scoreGo.transform.SetParent(go.transform, false);
            var scoreText = scoreGo.AddComponent<Text>();
            scoreText.text = "";
            scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreText.fontSize = 10;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.color = Color.white;
            var scoreRt = scoreGo.GetComponent<RectTransform>();
            scoreRt.anchoredPosition = new Vector2(5, -12);
            scoreRt.sizeDelta = new Vector2(110, 18);

            view.SetUIElements(iconImg, nameText, scoreText, bg);
            return go;
        }
    }
}
