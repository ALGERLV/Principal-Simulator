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
            // 获取引用
            if (mapManager == null)
                mapManager = MapManager.Instance;
            if (mapManager == null)
                mapManager = FindObjectOfType<MapManager>();
            if (gameCamera == null)
                gameCamera = Camera.main;

            // 默认Layer
            if (unitLayer == 0)
                unitLayer = LayerMask.GetMask("Unit");
            if (tileLayer == 0)
                tileLayer = LayerMask.GetMask("Tile");

            // 订阅输入事件
            EventBus.On<MouseButtonDownEvent>(OnMouseButtonDown);

            // 初始化行军线渲染器
            marchLineRenderer = gameObject.AddComponent<MarchLineRenderer>();
            marchLineRenderer.Initialize(mapManager);
            if (marchLineMaterial != null)
                marchLineRenderer.SetMaterial(marchLineMaterial);
        }

        void Update()
        {
            // Update 中不再处理输入，全部通过 EventBus 事件处理
        }

        private void OnMouseButtonDown(MouseButtonDownEvent evt)
        {
            if (evt.Button == 0)
            {
                Ray ray = gameCamera.ScreenPointToRay(evt.ScreenPos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    var unit = hit.collider.GetComponentInParent<UnitToken>();
                    if (unit != null)
                    {
                        Debug.Log($"点击了单位: {unit.UnitName}");
                        SelectUnit(unit);
                        return;
                    }

                    if (selectedUnit != null)
                    {
                        var tile = hit.collider.GetComponent<MapTileCell>()
                                ?? hit.collider.GetComponentInParent<MapTileCell>();
                        if (tile != null)
                        {
                            Debug.Log($"点击了地块: {tile.Coord}");
                            TryMoveSelectedUnit(tile.Coord);
                            return;
                        }

                        DeselectCurrentUnit();
                    }
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

        /// <summary>
        /// 选中单位
        /// </summary>
        public void SelectUnit(UnitToken unit)
        {
            if (selectedUnit != null && selectedUnit != unit)
            {
                selectedUnit.SetSelected(false);
            }

            selectedUnit = unit;
            selectedUnit.SetSelected(true);

            Debug.Log($"UnitSelectionManager: 选中单位 {unit.UnitName}");
        }

        /// <summary>
        /// 取消当前选中
        /// </summary>
        public void DeselectCurrentUnit()
        {
            if (selectedUnit != null)
            {
                selectedUnit.SetSelected(false);
                selectedUnit = null;
            }

            marchLineRenderer?.ClearPath();
        }

        /// <summary>
        /// 尝试移动选中单位到目标坐标
        /// </summary>
        void TryMoveSelectedUnit(MapHexCoord targetCoord)
        {
            if (selectedUnit == null) return;

            if (selectedUnit.IsMoving)
            {
                Debug.Log("UnitSelectionManager: 单位正在移动中");
                return;
            }

            var path = HexPathfinding.FindPath(selectedUnit.CurrentCoord, targetCoord, mapManager);
            if (path == null || path.Count < 2)
            {
                Debug.Log($"UnitSelectionManager: 无法找到到 {targetCoord} 的路径");
                return;
            }

            marchLineRenderer.ShowPath(path);
            selectedUnit.MoveAlongPath(path, mapManager, () =>
            {
                marchLineRenderer.ClearPath();
            });
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