using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Presentation.Camera
{
    /// <summary>
    /// 相机功能演示脚本 - 用于测试和展示相机控制器功能
    /// </summary>
    public class CameraDemo : MonoBehaviour
    {
        [Header("演示设置")]
        [SerializeField] private bool enableAutoDemo = false;
        [SerializeField] private float demoInterval = 3f;

        private BoardCameraController cameraController;
        private float demoTimer;
        private int demoPhase;

        private void Start()
        {
            cameraController = FindObjectOfType<BoardCameraController>();

            if (cameraController == null)
            {
                Debug.LogWarning("[CameraDemo] 未找到 BoardCameraController，演示模式未启动");
                enabled = false;
                return;
            }

            Debug.Log("[CameraDemo] 相机演示已启动");
            Debug.Log("[CameraDemo] 操作说明:");
            Debug.Log("  - 右键拖拽: 平移视角");
            Debug.Log("  - WASD/方向键: 键盘移动");
            Debug.Log("  - 鼠标滚轮: 缩放");
            Debug.Log("  - Q/E: 旋转视角");
            Debug.Log("  - 屏幕边缘: 边缘滚动");

            if (enableAutoDemo)
            {
                Debug.Log("[CameraDemo] 自动演示模式已启用");
            }
        }

        private void Update()
        {
            // 测试功能快捷键
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestZoom();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestRotation();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestFocus();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestReset();
            }

            // 自动演示
            if (enableAutoDemo)
            {
                RunAutoDemo();
            }
        }

        private void RunAutoDemo()
        {
            demoTimer += Time.deltaTime;

            if (demoTimer >= demoInterval)
            {
                demoTimer = 0f;
                demoPhase = (demoPhase + 1) % 4;

                switch (demoPhase)
                {
                    case 0:
                        Debug.Log("[CameraDemo] 自动演示: 缩放测试");
                        TestZoom();
                        break;
                    case 1:
                        Debug.Log("[CameraDemo] 自动演示: 旋转测试");
                        TestRotation();
                        break;
                    case 2:
                        Debug.Log("[CameraDemo] 自动演示: 聚焦测试");
                        TestFocus();
                        break;
                    case 3:
                        Debug.Log("[CameraDemo] 自动演示: 重置测试");
                        TestReset();
                        break;
                }
            }
        }

        /// <summary>
        /// 测试缩放功能
        /// </summary>
        [ContextMenu("Test/Zoom")]
        public void TestZoom()
        {
            if (cameraController == null) return;

            float randomZoom = Random.Range(0.2f, 0.8f);
            cameraController.SetZoomLevel(randomZoom);
            Debug.Log($"[CameraDemo] 设置缩放到: {randomZoom:P0}");
        }

        /// <summary>
        /// 测试旋转功能
        /// </summary>
        [ContextMenu("Test/Rotation")]
        public void TestRotation()
        {
            if (cameraController == null) return;

            float randomPitch = Random.Range(35f, 75f);
            float randomYaw = Random.Range(0f, 360f);
            cameraController.SetRotation(randomPitch, randomYaw);
            Debug.Log($"[CameraDemo] 设置旋转: 俯仰={randomPitch:F0}°, 偏航={randomYaw:F0}°");
        }

        /// <summary>
        /// 测试聚焦功能
        /// </summary>
        [ContextMenu("Test/Focus")]
        public void TestFocus()
        {
            if (cameraController == null) return;

            // 随机选择一个坐标聚焦（假设网格中心在0,0）
            int q = Random.Range(-5, 5);
            int r = Random.Range(-5, 5);
            var coord = new HexCoord(q, r);

            cameraController.FocusOnCoord(coord);
            Debug.Log($"[CameraDemo] 聚焦到坐标: {coord}");
        }

        /// <summary>
        /// 测试重置功能
        /// </summary>
        [ContextMenu("Test/Reset")]
        public void TestReset()
        {
            if (cameraController == null) return;

            cameraController.ResetCamera();
            Debug.Log("[CameraDemo] 相机已重置到初始状态");
        }

        /// <summary>
        /// 获取当前视野内的地块数量
        /// </summary>
        [ContextMenu("Test/Get Visible Tiles Count")]
        public void GetVisibleTilesCount()
        {
            if (cameraController == null) return;

            var tiles = cameraController.GetVisibleTiles();
            Debug.Log($"[CameraDemo] 当前视野内可见地块数量: {tiles.Length}");
        }
    }
}
