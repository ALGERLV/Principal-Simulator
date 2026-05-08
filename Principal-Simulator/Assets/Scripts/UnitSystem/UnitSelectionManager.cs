using UnityEngine;
using System.Collections.Generic;
using TBS.Map.Tools;
using TBS.Map.Runtime;
using TBS.Core.Events;
using TBS.Contracts.Events;

namespace TBS.UnitSystem
{
    /// <summary>
    /// 单位选择管理器 - 处理兵牌的点击选中和移动
    /// </summary>
    public class UnitSelectionManager : MonoBehaviour
    {
        [Header("引用")]
        public MapTerrainGrid hexGrid;
        public Camera gameCamera;

        [Header("高亮设置")]
        public GameObject highlightPrefab; // 可移动地块高亮预制体
        public Color validMoveColor = new Color(0, 1, 0, 0.3f); // 绿色半透明
        public Color invalidMoveColor = new Color(1, 0, 0, 0.3f); // 红色半透明
        public float highlightHeight = 0.1f; // 高亮效果离地高度

        [Header("点击设置")]
        public LayerMask unitLayer; // 单位层
        public LayerMask tileLayer; // 地块层
        public float raycastDistance = 100f;

        // 当前选中的单位
        private UnitToken selectedUnit;
        public UnitToken SelectedUnit => selectedUnit;

        // 高亮对象池
        private List<GameObject> activeHighlights = new List<GameObject>();

        void Start()
        {
            // 获取引用
            if (hexGrid == null)
                hexGrid = FindObjectOfType<MapTerrainGrid>();
            if (gameCamera == null)
                gameCamera = Camera.main;

            // 默认Layer
            if (unitLayer == 0)
                unitLayer = LayerMask.GetMask("Unit");
            if (tileLayer == 0)
                tileLayer = LayerMask.GetMask("Tile");

            // 订阅输入事件
            EventBus.On<MouseButtonDownEvent>(OnMouseButtonDown);
        }

        void Update()
        {
            // Update 中不再处理输入，全部通过 EventBus 事件处理
        }

        private void OnMouseButtonDown(MouseButtonDownEvent evt)
        {
            // 处理鼠标点击输入
            if (evt.Button == 0)
            {
                Ray ray = gameCamera.ScreenPointToRay(evt.ScreenPos);
                RaycastHit hit;

                Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 1f);

                if (Physics.Raycast(ray, out hit, raycastDistance))
                {
                    Debug.Log($"点击检测：{hit.collider.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                    var unit = hit.collider.GetComponentInParent<UnitToken>();
                    if (unit != null)
                    {
                        Debug.Log($"点击了单位: {unit.UnitName}");
                        SelectUnit(unit);
                        return;
                    }

                    if (selectedUnit != null)
                    {
                        var moveHighlight = hit.collider.GetComponent<MoveHighlight>();
                        if (moveHighlight != null)
                        {
                            Debug.Log($"点击了移动高亮: {moveHighlight.TargetCoord}");
                            TryMoveSelectedUnit(moveHighlight.TargetCoord);
                            return;
                        }

                        var tile = hit.collider.GetComponent<MapTileCell>();
                        if (tile != null)
                        {
                            Debug.Log($"点击了地块: {tile.Coord}");
                            TryMoveSelectedUnit(tile.Coord);
                            return;
                        }

                        Debug.Log("点击空白处，取消选择");
                        DeselectCurrentUnit();
                    }
                }
                else
                {
                    Debug.Log("射线未命中任何物体");
                    if (selectedUnit != null)
                    {
                        DeselectCurrentUnit();
                    }
                }
            }
            else if (evt.Button == 1)
            {
                // 右键取消选择
                DeselectCurrentUnit();
            }
        }

        /// <summary>
        /// 选中单位
        /// </summary>
        public void SelectUnit(UnitToken unit)
        {
            // 如果已有选中单位，先取消
            if (selectedUnit != null && selectedUnit != unit)
            {
                selectedUnit.SetSelected(false);
            }

            selectedUnit = unit;
            selectedUnit.SetSelected(true);

            // 显示可移动范围
            ShowValidMoveHighlights();

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

            // 清除高亮
            ClearHighlights();
        }

        /// <summary>
        /// 尝试移动选中单位到目标坐标
        /// </summary>
        void TryMoveSelectedUnit(MapHexCoord targetCoord)
        {
            if (selectedUnit == null) return;

            // 移动中禁止下令
            if (selectedUnit.IsMoving)
            {
                Debug.Log("UnitSelectionManager: 单位正在移动中");
                return;
            }

            if (selectedUnit.CanMoveTo(targetCoord))
            {
                selectedUnit.MoveTo(targetCoord);
                ClearHighlights();
                // 移动完成后高亮会由下一次选中触发，不在此轮询
            }
            else
            {
                Debug.Log($"UnitSelectionManager: 无法移动到 {targetCoord}");
            }
        }

        /// <summary>
        /// 显示可移动范围高亮
        /// </summary>
        void ShowValidMoveHighlights()
        {
            if (selectedUnit == null || hexGrid == null) return;

            // 清除旧高亮
            ClearHighlights();

            // 获取可移动目标
            var validTargets = selectedUnit.GetValidMoveTargets();

            // 为每个可移动目标创建高亮
            foreach (var coord in validTargets)
            {
                CreateHighlight(coord);
            }
        }

        /// <summary>
        /// 创建高亮效果
        /// </summary>
        void CreateHighlight(MapHexCoord coord)
        {
            Vector3 worldPos = hexGrid.CoordToWorldPosition(coord);
            worldPos.y += highlightHeight;

            GameObject highlight;
            if (highlightPrefab != null)
            {
                highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity);
                // 给预制体添加MoveHighlight组件
                var moveHighlight = highlight.GetComponent<MoveHighlight>();
                if (moveHighlight == null)
                {
                    moveHighlight = highlight.AddComponent<MoveHighlight>();
                }
                moveHighlight.Initialize(coord);
            }
            else
            {
                // 创建默认高亮（六边形平面）
                highlight = CreateDefaultHighlight(worldPos, coord);
            }

            activeHighlights.Add(highlight);
        }

        /// <summary>
        /// 创建默认高亮对象 - 使用圆柱体作为六边形近似
        /// </summary>
        GameObject CreateDefaultHighlight(Vector3 position, MapHexCoord coord)
        {
            // 使用圆柱体创建六边形高亮（6边形圆柱）
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = $"MoveHighlight_{coord.Q}_{coord.R}";
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.8f, 0.01f, 1.8f); // 扁平圆柱
            
            // 移除默认碰撞器，添加MeshCollider用于精确点击
            DestroyImmediate(go.GetComponent<Collider>());
            var meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;

            // 创建半透明材质
            var renderer = go.GetComponent<MeshRenderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = validMoveColor;
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            renderer.material = material;

            // 添加移动高亮标记组件
            var moveHighlight = go.AddComponent<MoveHighlight>();
            moveHighlight.Initialize(coord);

            return go;
        }

        /// <summary>
        /// 清除所有高亮
        /// </summary>
        void ClearHighlights()
        {
            foreach (var highlight in activeHighlights)
            {
                if (highlight != null)
                {
                    Destroy(highlight);
                }
            }
            activeHighlights.Clear();
        }

        void OnDestroy()
        {
            // 取消输入事件订阅
            EventBus.Off<MouseButtonDownEvent>(OnMouseButtonDown);
            ClearHighlights();
        }

        /// <summary>
        /// 通过射线检测获取点击的地块坐标
        /// </summary>
        public bool GetTileAtScreenPosition(Vector2 screenPos, out MapHexCoord coord)
        {
            coord = default;
            
            if (gameCamera == null || hexGrid == null) return false;

            Ray ray = gameCamera.ScreenPointToRay(screenPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance, tileLayer))
            {
                var tile = hit.collider.GetComponent<MapTileCell>();
                if (tile != null)
                {
                    coord = tile.Coord;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 通过射线检测获取点击的单位
        /// </summary>
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