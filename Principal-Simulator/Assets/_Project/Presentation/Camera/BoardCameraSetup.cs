using TBS.Map.Runtime;
using UnityEngine;

namespace TBS.Presentation.Camera
{
    /// <summary>
    /// 棋盘相机设置工具 - 快速在场景中创建和配置相机
    /// </summary>
    public static class BoardCameraSetup
    {
        /// <summary>
        /// 在场景中创建标准棋盘相机
        /// </summary>
        /// <param name="targetGrid">目标六边形网格</param>
        /// <returns>创建的相机控制器</returns>
        public static BoardCameraController CreateBoardCamera(MapTerrainGrid targetGrid = null)
        {
            // 查找或创建相机对象
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            GameObject cameraObj;

            if (cam == null)
            {
                cameraObj = new GameObject("BoardCamera");
                cam = cameraObj.AddComponent<UnityEngine.Camera>();
                cam.tag = "MainCamera";
            }
            else
            {
                cameraObj = cam.gameObject;
                // 移除现有的相机控制器（如果有）
                var existing = cameraObj.GetComponent<BoardCameraController>();
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }

            // 设置相机基本参数
            cam.orthographic = false;
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            // 添加相机控制器
            var controller = cameraObj.AddComponent<BoardCameraController>();
            controller.SetTargetGrid(targetGrid);

            // 设置初始位置
            if (targetGrid != null && targetGrid.IsInitialized)
            {
                controller.Initialize();
            }
            else
            {
                // 默认位置
                cameraObj.transform.position = new Vector3(0, 15, -10);
                cameraObj.transform.rotation = Quaternion.Euler(60, 0, 0);
            }

            // 添加轻量级光照（如果没有）
            EnsureLighting();

            Debug.Log($"[BoardCameraSetup] 棋盘相机已创建: {cameraObj.name}");
            return controller;
        }

        /// <summary>
        /// 创建带预设配置的相机
        /// </summary>
        public static BoardCameraController CreateBoardCameraWithConfig(BoardCameraConfig config, MapTerrainGrid targetGrid = null)
        {
            var controller = CreateBoardCamera(targetGrid);

            if (config != null)
            {
                config.ApplyToController(controller);
            }

            controller.Initialize();
            return controller;
        }

        /// <summary>
        /// 将现有相机转换为棋盘相机
        /// </summary>
        public static BoardCameraController ConvertExistingCamera(UnityEngine.Camera camera, MapTerrainGrid targetGrid = null)
        {
            if (camera == null)
            {
                Debug.LogError("[BoardCameraSetup] 相机为空");
                return null;
            }

            // 移除现有的相机控制器（如果有）
            var existing = camera.GetComponent<BoardCameraController>();
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            // 添加控制器
            var controller = camera.gameObject.AddComponent<BoardCameraController>();
            controller.SetTargetGrid(targetGrid);
            controller.Initialize();

            Debug.Log($"[BoardCameraSetup] 相机已转换: {camera.name}");
            return controller;
        }

        /// <summary>
        /// 确保场景有基本的光照
        /// </summary>
        private static void EnsureLighting()
        {
            // 检查是否有定向光
            Light directionalLight = Object.FindObjectOfType<Light>();
            bool hasDirectional = false;

            if (directionalLight != null && directionalLight.type == LightType.Directional)
            {
                hasDirectional = true;
            }

            if (!hasDirectional)
            {
                GameObject lightObj = new GameObject("Directional Light");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
                light.color = Color.white;
                lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

                Debug.Log("[BoardCameraSetup] 已添加定向光");
            }
        }
    }
}
