using System;
using TBS.Map.Managers;
using TBS.Map.Runtime;
using TBS.Map.Tools;
using UnityEngine;
using TBS.Core.Events;
using TBS.Contracts.Events;

namespace TBS.Presentation.Camera
{
    /// <summary>
    /// 棋盘相机控制器 - 实现俯视棋盘的移动、缩放功能
    /// </summary>
    public class BoardCameraController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("目标设置")]
        [SerializeField] private MapManager targetManager;
        [SerializeField] private Transform targetTransform;

        [Header("移动设置")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float dragSensitivity = 0.5f;
        [SerializeField] private bool useEdgeScrolling = true;
        [SerializeField] private float edgeScrollThreshold = 20f;
        [SerializeField] private float edgeScrollSpeed = 3f;

        [Header("缩放设置")]
        [Tooltip("滚轮缩放速度")]
        [SerializeField] private float zoomSpeed = 5f;
        [Tooltip("缩放平滑度")]
        [SerializeField] private float zoomSmoothness = 10f;
        [Tooltip("最小距离（相机到目标的最近距离）")]
        [SerializeField] private float minZoomDistance = 5f;
        [Tooltip("最大距离（相机到目标的最远距离）")]
        [SerializeField] private float maxZoomDistance = 40f;

        [Header("旋转设置")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float minPitchAngle = 80f;  // 限制最小俯仰角，保持接近垂直俯视
        [SerializeField] private float maxPitchAngle = 90f; // 最大90度（垂直俯视）

        [Header("边界限制")]
        [SerializeField] private bool limitToMapBounds = true;
        [SerializeField] private Vector2 mapPadding = new Vector2(2f, 2f);

        [Header("初始视角")]
        [SerializeField] private float initialPitch = 90f; // 垂直俯视，确保六边形显示为正六边形
        [SerializeField] private float initialYaw = 0f;
        [SerializeField] private float initialZoom = 0.3f; // 0-1 表示最小和最大之间（正交模式下控制orthographicSize）

        #endregion

        #region Private Fields

        private UnityEngine.Camera cam;
        private Vector3 targetPosition;
        private float currentZoomLevel;
        private float currentPitch;
        private float currentYaw;
        private Vector3 lastMousePosition;
        private bool isLeftDragging;    // 左键拖拽移动
        private bool isRightDragging;   // 右键拖拽旋转
        private Bounds mapWorldBounds;
        private bool isInitialized;

        #endregion

        #region Public Properties

        /// <summary>
        /// 当前缩放级别 (0-1)
        /// </summary>
        public float ZoomLevel => Mathf.InverseLerp(minZoomDistance, maxZoomDistance, currentZoomLevel);

        /// <summary>
        /// 当前俯仰角
        /// </summary>
        public float CurrentPitch => currentPitch;

        /// <summary>
        /// 当前水平旋转角
        /// </summary>
        public float CurrentYaw => currentYaw;

        #endregion

        #region Events

        /// <summary>
        /// 相机位置变化时触发
        /// </summary>
        public event Action OnCameraMoved;

        /// <summary>
        /// 相机缩放变化时触发
        /// </summary>
        public event Action<float> OnZoomChanged;

        /// <summary>
        /// 相机旋转变化时触发
        /// </summary>
        public event Action OnRotationChanged;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            if (cam == null)
            {
                cam = UnityEngine.Camera.main;
                if (cam != null && cam.transform != transform)
                {
                    Debug.LogWarning("BoardCameraController: 当前物体没有Camera组件，将使用主相机", this);
                }
            }

            // 订阅输入事件
            EventBus.On<MouseButtonDownEvent>(OnMouseButtonDown);
            EventBus.On<MouseButtonUpEvent>(OnMouseButtonUp);
            EventBus.On<MouseDragEvent>(OnMouseDrag);
            EventBus.On<ScrollEvent>(OnScroll);
            EventBus.On<KeyHeldEvent>(OnKeyHeld);
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!isInitialized) return;

            HandleEdgeScrolling();
            UpdateCameraPosition();
        }

        private void LateUpdate()
        {
            if (!isInitialized) return;

            // 确保相机在边界内
            if (limitToMapBounds)
            {
                ClampPositionToBounds();
            }
        }

        /// <summary>
        /// 启用/禁用屏幕边缘滚动功能
        /// </summary>
        public void SetEdgeScrollingEnabled(bool enabled)
        {
            useEdgeScrolling = enabled;
            Debug.Log($"[BoardCameraController] 屏幕边缘滚动已{(enabled ? "启用" : "禁用")}");
        }

        /// <summary>
        /// 获取屏幕边缘滚动是否启用
        /// </summary>
        public bool IsEdgeScrollingEnabled => useEdgeScrolling;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化相机控制器
        /// </summary>
        public void Initialize()
        {
            // 如果没有指定目标管理器，尝试自动查找
            if (targetManager == null)
            {
                targetManager = MapManager.Instance;
            }

            // 计算地图边界
            CalculateMapBounds();

            // 设置初始值
            currentPitch = initialPitch;
            currentYaw = initialYaw;
            currentZoomLevel = Mathf.Lerp(minZoomDistance, maxZoomDistance, initialZoom);

            // 如果没有目标位置，设置为地图中心
            if (targetTransform == null)
            {
                targetPosition = mapWorldBounds.center;
                targetPosition.y = 0;
            }
            else
            {
                targetPosition = targetTransform.position;
            }

            // 应用初始相机位置
            UpdateCameraPosition();

            isInitialized = true;
        }

        /// <summary>
        /// 设置目标管理器
        /// </summary>
        public void SetTargetManager(MapManager manager)
        {
            targetManager = manager;
            CalculateMapBounds();
        }

        /// <summary>
        /// 设置目标跟随对象
        /// </summary>
        public void SetTargetTransform(Transform target)
        {
            targetTransform = target;
            if (target != null)
            {
                targetPosition = target.position;
            }
        }

        #endregion

        #region Input Event Handlers

        private void OnMouseButtonDown(MouseButtonDownEvent evt)
        {
            if (evt.IsOverUI) return;

            if (evt.Button == 0)
            {
                isLeftDragging = true;
                lastMousePosition = evt.ScreenPos;
            }
            else if (evt.Button == 1)
            {
                isRightDragging = true;
                lastMousePosition = evt.ScreenPos;
            }
        }

        private void OnMouseButtonUp(MouseButtonUpEvent evt)
        {
            if (evt.Button == 0)
                isLeftDragging = false;
            else if (evt.Button == 1)
                isRightDragging = false;
        }

        private void OnMouseDrag(MouseDragEvent evt)
        {
            if (evt.Button == 0 && isLeftDragging)
            {
                Vector3 delta = evt.Delta;
                Vector3 right = transform.right;
                Vector3 forward = Vector3.Cross(right, Vector3.up);

                Vector3 move = (right * -delta.x + forward * -delta.y) * dragSensitivity * 0.01f;
                targetPosition += move;

                OnCameraMoved?.Invoke();
            }
            else if (evt.Button == 1 && isRightDragging)
            {
                Vector3 delta = evt.Delta;

                currentYaw += delta.x * rotationSpeed * 0.05f * Time.deltaTime;
                currentPitch += delta.y * rotationSpeed * 0.05f * Time.deltaTime;
                currentPitch = Mathf.Clamp(currentPitch, minPitchAngle, maxPitchAngle);

                OnRotationChanged?.Invoke();
            }
        }

        private void OnScroll(ScrollEvent evt)
        {
            float scroll = evt.Delta;
            float newHeight = currentZoomLevel - scroll * zoomSpeed;
            currentZoomLevel = Mathf.Clamp(newHeight, minZoomDistance, maxZoomDistance);

            OnZoomChanged?.Invoke(ZoomLevel);
        }

        private void OnKeyHeld(KeyHeldEvent evt)
        {
            Vector3 moveDirection = Vector3.zero;

            // WASD 移动
            if (evt.Key == KeyCode.W) moveDirection += Vector3.forward;
            if (evt.Key == KeyCode.S) moveDirection += Vector3.back;
            if (evt.Key == KeyCode.A) moveDirection += Vector3.left;
            if (evt.Key == KeyCode.D) moveDirection += Vector3.right;

            // 方向键移动
            if (evt.Key == KeyCode.UpArrow) moveDirection += Vector3.forward;
            if (evt.Key == KeyCode.DownArrow) moveDirection += Vector3.back;
            if (evt.Key == KeyCode.LeftArrow) moveDirection += Vector3.left;
            if (evt.Key == KeyCode.RightArrow) moveDirection += Vector3.right;

            if (moveDirection != Vector3.zero)
            {
                Vector3 right = transform.right;
                Vector3 forward = Vector3.Cross(right, Vector3.up);

                Vector3 worldMove = (right * moveDirection.x + forward * moveDirection.z).normalized;
                targetPosition += worldMove * moveSpeed * Time.deltaTime;

                OnCameraMoved?.Invoke();
            }

            // Q/E 旋转
            if (evt.Key == KeyCode.Q)
            {
                currentYaw -= rotationSpeed * Time.deltaTime;
                OnRotationChanged?.Invoke();
            }
            else if (evt.Key == KeyCode.E)
            {
                currentYaw += rotationSpeed * Time.deltaTime;
                OnRotationChanged?.Invoke();
            }
        }

        #endregion

        #region Edge Scrolling

        private void HandleEdgeScrolling()
        {
            if (!useEdgeScrolling) return;
            if (isLeftDragging || isRightDragging) return;

            Vector3 moveDirection = Vector3.zero;
            Vector3 mousePos = Input.mousePosition;

            // 检测屏幕边缘
            if (mousePos.x < edgeScrollThreshold)
                moveDirection += Vector3.left;
            else if (mousePos.x > Screen.width - edgeScrollThreshold)
                moveDirection += Vector3.right;

            if (mousePos.y < edgeScrollThreshold)
                moveDirection += Vector3.back;
            else if (mousePos.y > Screen.height - edgeScrollThreshold)
                moveDirection += Vector3.forward;

            if (moveDirection != Vector3.zero)
            {
                Vector3 right = transform.right;
                Vector3 forward = Vector3.Cross(right, Vector3.up);

                Vector3 worldMove = (right * moveDirection.x + forward * moveDirection.z).normalized;
                targetPosition += worldMove * edgeScrollSpeed * Time.deltaTime;

                OnCameraMoved?.Invoke();
            }
        }

        #endregion

        #region Camera Position Update

        private void UpdateCameraPosition()
        {
            // 如果有目标跟随对象，更新目标位置
            if (targetTransform != null)
            {
                targetPosition = targetTransform.position;
            }

            // 根据相机投影类型选择不同的位置计算方式
            if (cam != null && cam.orthographic)
            {
                // 正交相机：相机始终垂直俯视（或固定角度），只调整位置
                // 正交大小由 orthographicSize 控制
                cam.orthographicSize = currentZoomLevel;

                // 计算相机位置（保持俯仰角和水平旋转）
                float pitchRad = currentPitch * Mathf.Deg2Rad;
                float yawRad = currentYaw * Mathf.Deg2Rad;

                // 固定相机高度，根据俯仰角和旋转计算位置
                float height = 20f; // 固定高度
                Vector3 offset = new Vector3(
                    height * Mathf.Sin(pitchRad) * Mathf.Sin(yawRad),
                    height * Mathf.Cos(pitchRad),
                    height * Mathf.Sin(pitchRad) * Mathf.Cos(yawRad)
                );

                Vector3 desiredPosition = targetPosition + offset;
                transform.position = Vector3.Lerp(transform.position, desiredPosition, zoomSmoothness * Time.deltaTime);
                transform.LookAt(targetPosition);
            }
            else
            {
                // 透视相机：使用球坐标系
                float pitchRad = currentPitch * Mathf.Deg2Rad;
                float yawRad = currentYaw * Mathf.Deg2Rad;

                // 半径（相机到目标点的直线距离）
                float radius = currentZoomLevel;

                // 球坐标转换为直角坐标
                Vector3 offset = new Vector3(
                    radius * Mathf.Sin(pitchRad) * Mathf.Sin(yawRad),
                    radius * Mathf.Cos(pitchRad),
                    radius * Mathf.Sin(pitchRad) * Mathf.Cos(yawRad)
                );

                Vector3 desiredPosition = targetPosition + offset;
                transform.position = Vector3.Lerp(transform.position, desiredPosition, zoomSmoothness * Time.deltaTime);
                transform.LookAt(targetPosition + Vector3.up * 0.5f);
            }
        }

        private float GetCameraHeight()
        {
            return transform.position.y;
        }

        private void ClampPositionToBounds()
        {
            if (mapWorldBounds.size == Vector3.zero) return;

            // 计算相机水平偏移（基于半径和俯仰角）
            float pitchRad = currentPitch * Mathf.Deg2Rad;
            float horizontalDistance = currentZoomLevel * Mathf.Sin(pitchRad);

            // 计算当前视角的可见范围偏移
            Vector2 viewOffset = new Vector2(
                horizontalDistance * Mathf.Sin(currentYaw * Mathf.Deg2Rad),
                horizontalDistance * Mathf.Cos(currentYaw * Mathf.Deg2Rad)
            );

            // 限制目标位置（考虑相机偏移）
            Vector3 minBounds = mapWorldBounds.min - new Vector3(mapPadding.x, 0, mapPadding.y);
            Vector3 maxBounds = mapWorldBounds.max + new Vector3(mapPadding.x, 0, mapPadding.y);

            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.z, maxBounds.z);
        }

        #endregion

        #region Map Bounds

        private void CalculateMapBounds()
        {
            if (targetManager == null || targetManager.Tiles.Count == 0)
            {
                mapWorldBounds = new Bounds(Vector3.zero, Vector3.zero);
                return;
            }

            // 获取所有地块的世界位置
            var tiles = targetManager.Tiles;
            if (tiles.Count == 0) return;

            Vector3 min = Vector3.one * float.MaxValue;
            Vector3 max = Vector3.one * float.MinValue;

            foreach (var tile in tiles.Values)
            {
                if (tile == null) continue;
                Vector3 pos = tile.transform.position;
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }

            mapWorldBounds = new Bounds((min + max) / 2f, max - min);
        }

        #endregion

        #region Public Control Methods

        /// <summary>
        /// 聚焦到指定坐标
        /// </summary>
        public void FocusOnCoord(MapHexCoord coord)
        {
            if (targetManager == null) return;

            Vector3 worldPos = targetManager.CoordToWorldPosition(coord);
            targetPosition = worldPos;
            targetTransform = null; // 清除跟随目标
        }

        /// <summary>
        /// 聚焦到地图中心
        /// </summary>
        public void FocusOnCenter()
        {
            targetPosition = mapWorldBounds.center;
            targetPosition.y = 0;
            targetTransform = null;
        }

        /// <summary>
        /// 设置缩放级别 (0-1)
        /// </summary>
        public void SetZoomLevel(float normalizedZoom)
        {
            currentZoomLevel = Mathf.Lerp(minZoomDistance, maxZoomDistance, Mathf.Clamp01(normalizedZoom));
            OnZoomChanged?.Invoke(ZoomLevel);
        }

        /// <summary>
        /// 设置旋转角度
        /// </summary>
        public void SetRotation(float pitch, float yaw)
        {
            currentPitch = Mathf.Clamp(pitch, minPitchAngle, maxPitchAngle);
            currentYaw = yaw;
            OnRotationChanged?.Invoke();
        }

        /// <summary>
        /// 重置相机到初始状态
        /// </summary>
        public void ResetCamera()
        {
            currentPitch = initialPitch;
            currentYaw = initialYaw;
            currentZoomLevel = Mathf.Lerp(minZoomDistance, maxZoomDistance, initialZoom);
            FocusOnCenter();
        }

        /// <summary>
        /// 获取当前视野范围内的地块
        /// </summary>
        public MapTileCell[] GetVisibleTiles()
        {
            if (targetManager == null) return new MapTileCell[0];

            // 使用视锥体检测可见地块
            var allTiles = targetManager.Tiles;
            var visibleTiles = new System.Collections.Generic.List<MapTileCell>();

            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

            foreach (var tile in allTiles.Values)
            {
                if (tile == null) continue;

                Bounds bounds = new Bounds(tile.transform.position, Vector3.one * targetManager.HexSize);
                if (GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
                {
                    visibleTiles.Add(tile);
                }
            }

            return visibleTiles.ToArray();
        }

        #endregion

        private void OnDestroy()
        {
            // 取消输入事件订阅
            EventBus.Off<MouseButtonDownEvent>(OnMouseButtonDown);
            EventBus.Off<MouseButtonUpEvent>(OnMouseButtonUp);
            EventBus.Off<MouseDragEvent>(OnMouseDrag);
            EventBus.Off<ScrollEvent>(OnScroll);
            EventBus.Off<KeyHeldEvent>(OnKeyHeld);
        }

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            // 绘制地图边界
            if (mapWorldBounds.size != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(mapWorldBounds.center, mapWorldBounds.size);
            }

            // 绘制目标位置
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.3f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }

        #endregion
    }
}
