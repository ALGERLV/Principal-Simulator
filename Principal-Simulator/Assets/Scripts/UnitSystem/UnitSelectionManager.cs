using UnityEngine;
using TBS.Map.Runtime;
using TBS.Map.Rendering;
using TBS.Map.Tools;
using TBS.Map.Managers;
using TBS.Core.Events;
using TBS.Contracts.Events;

namespace TBS.UnitSystem
{
    public class UnitSelectionManager : MonoBehaviour
    {
        [Header("引用")]
        public MapManager mapManager;
        public Camera gameCamera;

        [Header("点击设置")]
        public LayerMask unitLayer;
        public LayerMask tileLayer;
        public float raycastDistance = 100f;

        [Header("行军线")]
        [SerializeField] private Material marchLineMaterial;

        private UnitToken selectedUnit;
        public UnitToken SelectedUnit => selectedUnit;

        private MarchLineRenderer marchLineRenderer;

        void Start()
        {
            if (mapManager == null)
                mapManager = MapManager.Instance;
            if (mapManager == null)
                mapManager = FindObjectOfType<MapManager>();
            if (gameCamera == null)
                gameCamera = Camera.main;

            if (unitLayer == 0)
                unitLayer = LayerMask.GetMask("Unit");
            if (tileLayer == 0)
                tileLayer = LayerMask.GetMask("Tile");

            EventBus.On<MouseButtonDownEvent>(OnMouseButtonDown);

            marchLineRenderer = gameObject.AddComponent<MarchLineRenderer>();
            marchLineRenderer.Initialize(mapManager);
            if (marchLineMaterial != null)
                marchLineRenderer.SetMaterial(marchLineMaterial);
        }

        void Update()
        {
            if (selectedUnit != null && selectedUnit.IsMoving)
            {
                var remaining = selectedUnit.GetRemainingPath();
                if (remaining != null && remaining.Count >= 1)
                    marchLineRenderer.TrackUnit(selectedUnit.transform, remaining);
                else
                    marchLineRenderer.ClearPath();
            }
        }

        private void OnMouseButtonDown(MouseButtonDownEvent evt)
        {
            if (evt.Button == 0)
            {
                Ray ray = gameCamera.ScreenPointToRay(evt.ScreenPos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    var tile = hit.collider.GetComponent<MapTileCell>()
                            ?? hit.collider.GetComponentInParent<MapTileCell>();

                    var unit = hit.collider.GetComponentInParent<UnitToken>();
                    if (unit != null && unit != selectedUnit)
                    {
                        SelectUnit(unit);
                        return;
                    }

                    if (selectedUnit != null && tile != null)
                    {
                        TryMoveSelectedUnit(tile.Coord);
                        return;
                    }

                    if (selectedUnit != null && unit == null)
                        DeselectCurrentUnit();
                }
                else
                {
                    if (selectedUnit != null)
                        DeselectCurrentUnit();
                }
            }
            else if (evt.Button == 1)
            {
                DeselectCurrentUnit();
            }
        }

        public void SelectUnit(UnitToken unit)
        {
            if (selectedUnit != null && selectedUnit != unit)
                selectedUnit.SetSelected(false);

            selectedUnit = unit;
            selectedUnit.SetSelected(true);

            if (unit.IsMoving)
            {
                var remaining = unit.GetRemainingPath();
                if (remaining != null && remaining.Count >= 1)
                    marchLineRenderer.TrackUnit(unit.transform, remaining);
            }
        }

        public void DeselectCurrentUnit()
        {
            if (selectedUnit != null)
            {
                selectedUnit.SetSelected(false);
                selectedUnit = null;
            }

            marchLineRenderer?.ClearPath();
        }

        void TryMoveSelectedUnit(MapHexCoord targetCoord)
        {
            if (selectedUnit == null) return;

            var fakePath = new System.Collections.Generic.List<MapHexCoord>
            {
                selectedUnit.CurrentCoord,
                targetCoord
            };
            selectedUnit.MoveAlongPath(fakePath, mapManager, () =>
            {
                marchLineRenderer.ClearPath();
            });

            var remaining = selectedUnit.GetRemainingPath();
            if (remaining != null && remaining.Count >= 1)
                marchLineRenderer.TrackUnit(selectedUnit.transform, remaining);
        }

        void OnDestroy()
        {
            EventBus.Off<MouseButtonDownEvent>(OnMouseButtonDown);
        }

        public bool GetTileAtScreenPosition(Vector2 screenPos, out MapHexCoord coord)
        {
            coord = default;

            if (gameCamera == null || mapManager == null) return false;

            Ray ray = gameCamera.ScreenPointToRay(screenPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance, tileLayer))
            {
                var tile = hit.collider.GetComponent<MapTileCell>()
                        ?? hit.collider.GetComponentInParent<MapTileCell>();
                if (tile != null)
                {
                    coord = tile.Coord;
                    return true;
                }
            }

            return false;
        }

        public UnitToken GetUnitAtScreenPosition(Vector2 screenPos)
        {
            if (gameCamera == null) return null;

            Ray ray = gameCamera.ScreenPointToRay(screenPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance, unitLayer))
            {
                return hit.collider.GetComponentInParent<UnitToken>();
            }

            return null;
        }
    }
}
