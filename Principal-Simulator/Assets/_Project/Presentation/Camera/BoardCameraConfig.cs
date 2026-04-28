using UnityEngine;

namespace TBS.Presentation.Camera
{
    /// <summary>
    /// 棋盘相机配置 - 用于保存和加载相机设置
    /// </summary>
    [CreateAssetMenu(fileName = "BoardCameraConfig", menuName = "TBS/Camera/Board Camera Config")]
    public class BoardCameraConfig : ScriptableObject
    {
        [Header("移动设置")]
        [Tooltip("键盘移动速度")]
        public float moveSpeed = 5f;

        [Tooltip("鼠标拖拽灵敏度")]
        public float dragSensitivity = 0.5f;

        [Tooltip("是否启用边缘滚动")]
        public bool useEdgeScrolling = true;

        [Tooltip("边缘滚动触发阈值（像素）")]
        public float edgeScrollThreshold = 20f;

        [Tooltip("边缘滚动速度")]
        public float edgeScrollSpeed = 3f;

        [Header("缩放设置")]
        [Tooltip("滚轮缩放速度")]
        public float zoomSpeed = 5f;

        [Tooltip("缩放平滑度")]
        public float zoomSmoothness = 10f;

        [Tooltip("最小高度（最近距离）")]
        public float minZoomHeight = 3f;

        [Tooltip("最大高度（最远距离）")]
        public float maxZoomHeight = 20f;

        [Header("旋转设置")]
        [Tooltip("旋转速度")]
        public float rotationSpeed = 100f;

        [Tooltip("最小俯仰角（度）")]
        [Range(0f, 90f)]
        public float minPitchAngle = 30f;

        [Tooltip("最大俯仰角（度）")]
        [Range(0f, 90f)]
        public float maxPitchAngle = 85f;

        [Header("边界设置")]
        [Tooltip("是否限制在地图边界内")]
        public bool limitToMapBounds = true;

        [Tooltip("地图边界外边距")]
        public Vector2 mapPadding = new Vector2(2f, 2f);

        [Header("初始视角")]
        [Tooltip("初始俯仰角")]
        [Range(0f, 90f)]
        public float initialPitch = 60f;

        [Tooltip("初始水平旋转角")]
        public float initialYaw = 0f;

        [Tooltip("初始缩放 (0=最近, 1=最远)")]
        [Range(0f, 1f)]
        public float initialZoom = 0.5f;

        /// <summary>
        /// 应用配置到相机控制器
        /// </summary>
        public void ApplyToController(BoardCameraController controller)
        {
            if (controller == null) return;

            // 使用反射设置字段值
            var type = typeof(BoardCameraController);

            SetFieldValue(controller, type, "moveSpeed", moveSpeed);
            SetFieldValue(controller, type, "dragSensitivity", dragSensitivity);
            SetFieldValue(controller, type, "useEdgeScrolling", useEdgeScrolling);
            SetFieldValue(controller, type, "edgeScrollThreshold", edgeScrollThreshold);
            SetFieldValue(controller, type, "edgeScrollSpeed", edgeScrollSpeed);
            SetFieldValue(controller, type, "zoomSpeed", zoomSpeed);
            SetFieldValue(controller, type, "zoomSmoothness", zoomSmoothness);
            SetFieldValue(controller, type, "minZoomHeight", minZoomHeight);
            SetFieldValue(controller, type, "maxZoomHeight", maxZoomHeight);
            SetFieldValue(controller, type, "rotationSpeed", rotationSpeed);
            SetFieldValue(controller, type, "minPitchAngle", minPitchAngle);
            SetFieldValue(controller, type, "maxPitchAngle", maxPitchAngle);
            SetFieldValue(controller, type, "limitToMapBounds", limitToMapBounds);
            SetFieldValue(controller, type, "mapPadding", mapPadding);
            SetFieldValue(controller, type, "initialPitch", initialPitch);
            SetFieldValue(controller, type, "initialYaw", initialYaw);
            SetFieldValue(controller, type, "initialZoom", initialZoom);
        }

        private void SetFieldValue(object target, System.Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                field = type.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            }

            field?.SetValue(target, value);
        }
    }
}
