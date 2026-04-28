using UnityEngine;
using UnityEngine.UI;

namespace TBS.Presentation.Camera
{
    /// <summary>
    /// 相机HUD显示 - 显示相机状态和操作提示
    /// </summary>
    public class CameraHUD : MonoBehaviour
    {
        [Header("UI引用")]
        [SerializeField] private Text zoomText;
        [SerializeField] private Text positionText;
        [SerializeField] private Text rotationText;
        [SerializeField] private GameObject controlsPanel;

        [Header("显示设置")]
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private float hideAfterSeconds = 5f;
        [SerializeField] private KeyCode toggleKey = KeyCode.H;

        private BoardCameraController cameraController;
        private bool isVisible = true;
        private float hideTimer;

        private void Start()
        {
            cameraController = FindObjectOfType<BoardCameraController>();

            if (cameraController == null)
            {
                Debug.LogWarning("[CameraHUD] 未找到 BoardCameraController");
                enabled = false;
                return;
            }

            // 订阅事件
            cameraController.OnZoomChanged += OnZoomChanged;
            cameraController.OnCameraMoved += OnCameraMoved;
            cameraController.OnRotationChanged += OnRotationChanged;

            // 初始显示设置
            isVisible = showOnStart;
            hideTimer = hideAfterSeconds;

            UpdateUI();
            UpdateVisibility();
        }

        private void Update()
        {
            // 切换显示
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
                UpdateVisibility();
            }

            // 自动隐藏
            if (isVisible && hideAfterSeconds > 0)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0)
                {
                    isVisible = false;
                    UpdateVisibility();
                }
            }
        }

        private void OnDestroy()
        {
            if (cameraController != null)
            {
                cameraController.OnZoomChanged -= OnZoomChanged;
                cameraController.OnCameraMoved -= OnCameraMoved;
                cameraController.OnRotationChanged -= OnRotationChanged;
            }
        }

        private void OnZoomChanged(float zoomLevel)
        {
            UpdateUI();
            ResetHideTimer();
        }

        private void OnCameraMoved()
        {
            UpdateUI();
            ResetHideTimer();
        }

        private void OnRotationChanged()
        {
            UpdateUI();
            ResetHideTimer();
        }

        private void ResetHideTimer()
        {
            hideTimer = hideAfterSeconds;
            if (!isVisible)
            {
                isVisible = true;
                UpdateVisibility();
            }
        }

        private void UpdateUI()
        {
            if (cameraController == null) return;

            // 更新缩放显示
            if (zoomText != null)
            {
                float zoomPercent = cameraController.ZoomLevel * 100f;
                zoomText.text = $"缩放: {zoomPercent:F0}%";
            }

            // 更新位置显示
            if (positionText != null)
            {
                Vector3 pos = cameraController.transform.position;
                positionText.text = $"位置: ({pos.x:F1}, {pos.z:F1})";
            }

            // 更新旋转显示
            if (rotationText != null)
            {
                rotationText.text = $"俯角: {cameraController.CurrentPitch:F0}° 旋转: {cameraController.CurrentYaw:F0}°";
            }
        }

        private void UpdateVisibility()
        {
            if (controlsPanel != null)
            {
                controlsPanel.SetActive(isVisible);
            }
        }

        /// <summary>
        /// 显示/隐藏HUD
        /// </summary>
        public void Show(bool show)
        {
            isVisible = show;
            UpdateVisibility();
        }
    }
}
