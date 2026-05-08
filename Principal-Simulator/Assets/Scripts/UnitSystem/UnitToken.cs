using System.Collections;
using UnityEngine;
using TBS.Map.Tools;
using TBS.Map.Runtime;
using TBS.Map.API;
using TBS.Core;

namespace TBS.UnitSystem
{
    [RequireComponent(typeof(TBS.Unit.Unit))]
    public class UnitToken : MonoBehaviour, IUnit
    {
        public TBS.Unit.Unit UnitLogic { get; private set; }

        [SerializeField] private MapHexCoord currentCoord;
        public MapHexCoord CurrentCoord => currentCoord;

        [Header("UI 引用（可选）")]
        public Renderer BorderRenderer;
        public Renderer BaseRenderer;
        public Renderer SymbolRenderer;
        public GameObject SelectionIndicator;

        [Header("选中效果")]
        public Color SelectedColor = Color.yellow;

        private bool isSelected;
        private bool isMoving;
        private Color originalBorderColor;
        private MapTerrainGrid hexGrid;

        public System.Action<UnitToken> OnUnitSelected;
        public System.Action<UnitToken> OnUnitDeselected;
        public System.Action<MapHexCoord> OnUnitMoved;

        private static readonly Color Color_KMT_Elite  = new Color(0f, 0f, 0.5f);
        private static readonly Color Color_KMT_Normal = new Color(0f, 0.3f, 0.8f);
        private static readonly Color Color_Japan      = new Color(0.5f, 0f, 0f);
        private static readonly Color Color_PLA        = new Color(0.8f, 0f, 0f);
        private static readonly Color Color_Gold       = new Color(1f, 0.84f, 0f);

        // IUnit
        public string UnitId   => UnitLogic != null ? UnitLogic.UnitId : "";
        public string UnitName => UnitLogic != null ? UnitLogic.DisplayName : "";
        public float ApplyTerrainMovementModifier(float baseCost, string terrainId) => baseCost;
        public bool CanTraverseTerrain(string terrainId) => true;

        public bool IsSelected => isSelected;
        public bool IsMoving   => isMoving;

        // ─────────────────────────────────────────────────────────

        void Awake()
        {
            UnitLogic = GetComponent<TBS.Unit.Unit>();
        }

        void Start()
        {
            hexGrid = FindObjectOfType<MapTerrainGrid>();
            if (BorderRenderer != null)
                originalBorderColor = BorderRenderer.material.color;
            EnsureClickable();
        }

        void Update()
        {
            if (Camera.main != null && transform.childCount > 1)
                transform.GetChild(1).rotation = Camera.main.transform.rotation;
        }

        // ─── 初始化 ───────────────────────────────────────────────

        public void InitializeOnTile(MapHexCoord coord)
        {
            if (hexGrid == null) hexGrid = FindObjectOfType<MapTerrainGrid>();
            currentCoord = coord;
            SnapToCoord(coord);
            hexGrid?.GetTile(coord)?.SetOccupyingUnit(this);
        }

        // ─── 移动 ─────────────────────────────────────────────────

        public void MoveTo(MapHexCoord targetCoord)
        {
            if (isMoving) return;
            if (hexGrid == null) hexGrid = FindObjectOfType<MapTerrainGrid>();

            var targetTile = hexGrid?.GetTile(targetCoord);
            if (targetTile == null || !targetTile.CanEnter(this)) return;

            hexGrid.GetTile(currentCoord)?.ClearOccupyingUnit();
            targetTile.SetOccupyingUnit(this);
            StartCoroutine(SmoothMove(targetCoord));
        }

        private IEnumerator SmoothMove(MapHexCoord targetCoord)
        {
            isMoving = true;

            // 打印移动开始时的游戏时间
            float startGameHours = GameTimeSystem.Instance != null
                ? GameTimeSystem.Instance.GameHours
                : 0f;
            Debug.Log($"[UnitToken] {UnitName} 开始移动到 {targetCoord}，游戏时间: {startGameHours:F1}小时");

            Vector3 startPos = transform.position;
            Vector3 endPos   = hexGrid.CoordToWorldPosition(targetCoord);
            endPos.y = startPos.y;

            float speed    = UnitLogic != null ? UnitLogic.EffectiveMoveSpeed : 25f;
            Debug.Log($"[UnitToken] {UnitName} 移动速度(EffectiveMoveSpeed): {speed}km/day");

            float duration = GameTimeSystem.Instance != null
                ? GameTimeSystem.Instance.GetRealSecondsPerHex(speed)
                : 1f;
            Debug.Log($"[UnitToken] {UnitName} GetRealSecondsPerHex返回: {duration}秒");

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            transform.position = endPos;
            currentCoord = targetCoord;
            UnitLogic?.SetPosition(targetCoord);

            // 打印移动完成时的游戏时间
            float endGameHours = GameTimeSystem.Instance != null
                ? GameTimeSystem.Instance.GameHours
                : 0f;
            Debug.Log($"[UnitToken] {UnitName} 移动完成，游戏时间: {endGameHours:F1}小时，耗时: {endGameHours - startGameHours:F1}小时");

            isMoving = false;
            OnUnitMoved?.Invoke(targetCoord);
        }

        public bool CanMoveTo(MapHexCoord targetCoord)
        {
            if (isMoving) return false;
            if (hexGrid == null) hexGrid = FindObjectOfType<MapTerrainGrid>();
            if (hexGrid == null) return false;
            if (currentCoord.DistanceTo(targetCoord) != 1) return false;
            var tile = hexGrid.GetTile(targetCoord);
            return tile != null && tile.CanEnter(this);
        }

        public MapHexCoord[] GetValidMoveTargets()
        {
            if (hexGrid == null) hexGrid = FindObjectOfType<MapTerrainGrid>();
            if (hexGrid == null) return System.Array.Empty<MapHexCoord>();
            var result = new System.Collections.Generic.List<MapHexCoord>();
            foreach (var n in currentCoord.GetNeighbors())
            {
                var tile = hexGrid.GetTile(n);
                if (tile != null && tile.CanEnter(this)) result.Add(n);
            }
            return result.ToArray();
        }

        // ─── 选中 ─────────────────────────────────────────────────

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateSelectionVisual();
            if (isSelected) OnUnitSelected?.Invoke(this);
            else            OnUnitDeselected?.Invoke(this);
        }

        public void ToggleSelection() => SetSelected(!isSelected);

        void OnMouseDown()
        {
            var mgr = FindObjectOfType<UnitSelectionManager>();
            if (mgr != null) mgr.SelectUnit(this);
            else ToggleSelection();
        }

        void UpdateSelectionVisual()
        {
            EnsureSelectionIndicator();
            if (SelectionIndicator != null)
                SelectionIndicator.SetActive(isSelected);
            if (BorderRenderer != null)
                BorderRenderer.material.color = isSelected ? SelectedColor : originalBorderColor;
            Vector3 target = isSelected ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
            transform.localScale = Vector3.Lerp(transform.localScale, target, 0.3f);
        }

        void EnsureSelectionIndicator()
        {
            if (SelectionIndicator != null) return;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "SelectionIndicator";
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0, -0.02f, 0);
            go.transform.localScale    = new Vector3(1.1f, 0.01f, 1.1f);
            DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().material.color = Color.yellow;
            SelectionIndicator = go;
        }

        // ─── 视觉 ─────────────────────────────────────────────────

        void UpdateFactionColors()
        {
            if (UnitLogic == null) return;
            Color border, baseCol;
            switch (UnitLogic.Faction)
            {
                case TBS.Unit.Faction.KMT:
                    border  = UnitLogic.Grade == TBS.Unit.UnitGrade.KMT_Elite ? Color_KMT_Elite : Color_KMT_Normal;
                    baseCol = Color.white;
                    break;
                case TBS.Unit.Faction.Japan:
                    border  = Color_Japan;
                    baseCol = Color.white;
                    break;
                case TBS.Unit.Faction.PLA:
                    border  = Color_PLA;
                    baseCol = Color_Gold;
                    break;
                default:
                    border  = Color.gray;
                    baseCol = Color.white;
                    break;
            }
            originalBorderColor = border;
            if (BorderRenderer != null) BorderRenderer.material.color = border;
            if (BaseRenderer   != null) BaseRenderer.material.color   = baseCol;
            if (SymbolRenderer != null) SymbolRenderer.material.color = Color.black;
        }

        static string GetStateLabel(TBS.Unit.UnitState s) => s switch
        {
            TBS.Unit.UnitState.Inspired     => "高昂",
            TBS.Unit.UnitState.Normal       => "正常",
            TBS.Unit.UnitState.Suppressed   => "压制",
            TBS.Unit.UnitState.Shaken       => "动摇",
            TBS.Unit.UnitState.Routed       => "溃散",
            TBS.Unit.UnitState.Recuperating => "整补",
            _                               => "未知"
        };

        // ─── 辅助 ─────────────────────────────────────────────────

        void SnapToCoord(MapHexCoord coord)
        {
            if (hexGrid == null) return;
            Vector3 pos = hexGrid.CoordToWorldPosition(coord);
            pos.y = transform.position.y;
            transform.position = pos;
        }

        void EnsureClickable()
        {
            if (GetComponent<Collider>() == null)
            {
                var col = gameObject.AddComponent<BoxCollider>();
                col.size   = new Vector3(1.6f, 0.2f, 1.6f);
                col.center = new Vector3(0f, 0.1f, 0f);
            }
        }
    }
}
