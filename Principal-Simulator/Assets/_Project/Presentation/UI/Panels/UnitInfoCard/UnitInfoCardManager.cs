using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TBS.Core.Events;
using TBS.Contracts.Events;
using TBS.Unit;
using TBS.UnitSystem;

namespace TBS.Presentation.UI.Panels.UnitInfoCard
{
    public class UnitInfoCardManager : MonoBehaviour
    {
        [SerializeField] private Canvas _battleHUDCanvas;
        [SerializeField] private RectTransform _cardsContainer;
        private GameObject _cardPrefab;
        private Dictionary<UnitToken, UnitInfoCardView> _cards = new Dictionary<UnitToken, UnitInfoCardView>();
        private UnityEngine.Camera _cam;

        public void Setup(Canvas battleHUDCanvas, RectTransform cardsContainer)
        {
            _battleHUDCanvas = battleHUDCanvas;
            _cardsContainer = cardsContainer;
        }

        void Awake()
        {
            _cardPrefab = Resources.Load<GameObject>("Prefabs/UI/UnitInfoCard");
            if (_cardPrefab == null)
            {
                Debug.LogError("[UnitInfoCardManager] 无法加载UnitInfoCard预制体，请检查路径: Assets/Resources/Prefabs/UI/UnitInfoCard.prefab");
                return;
            }

            _cam = UnityEngine.Camera.main;
            if (_cam == null)
            {
                Debug.LogError("[UnitInfoCardManager] 无法找到主摄像机");
                return;
            }

            EventBus.On<UnitSpawnedEvent>(OnUnitSpawned);
            EventBus.On<UnitDespawnedEvent>(OnUnitDespawned);
            Debug.Log("[UnitInfoCardManager] 已初始化");
        }

        void OnDestroy()
        {
            EventBus.Off<UnitSpawnedEvent>(OnUnitSpawned);
            EventBus.Off<UnitDespawnedEvent>(OnUnitDespawned);
        }

        private void OnUnitSpawned(UnitSpawnedEvent evt)
        {
            if (evt.Token == null)
            {
                Debug.LogWarning("[UnitInfoCardManager] 收到空的UnitSpawnedEvent");
                return;
            }

            if (_cardsContainer == null)
            {
                Debug.LogError("[UnitInfoCardManager] CardsContainer为空");
                return;
            }

            var go = Instantiate(_cardPrefab, _cardsContainer);
            var card = go.GetComponent<UnitInfoCardView>();
            if (card == null)
            {
                Debug.LogError("[UnitInfoCardManager] UnitInfoCard预制体没有UnitInfoCardView组件");
                Destroy(go);
                return;
            }

            card.Bind(evt.Token);
            _cards[evt.Token] = card;
            Debug.Log($"[UnitInfoCardManager] 为单位 {evt.Token.gameObject.name} 创建了信息卡");
        }

        private void OnUnitDespawned(UnitDespawnedEvent evt)
        {
            if (evt.Token == null) return;

            if (_cards.TryGetValue(evt.Token, out var card))
            {
                Destroy(card.gameObject);
                _cards.Remove(evt.Token);
                Debug.Log($"[UnitInfoCardManager] 销毁了单位 {evt.Token.gameObject.name} 的信息卡");
            }
        }

        void LateUpdate()
        {
            if (_cardsContainer == null || _cam == null) return;

            var keysToRemove = new List<UnitToken>();
            foreach (var kvp in _cards)
            {
                var unit = kvp.Key;
                var card = kvp.Value;

                if (unit == null)
                {
                    keysToRemove.Add(unit);
                    Destroy(card.gameObject);
                    continue;
                }

                Vector3 screenPos = _cam.WorldToScreenPoint(unit.transform.position);
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
                    cardRect.anchoredPosition = localPos + new Vector2(0, 50);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cards.Remove(key);
            }
        }
    }
}
