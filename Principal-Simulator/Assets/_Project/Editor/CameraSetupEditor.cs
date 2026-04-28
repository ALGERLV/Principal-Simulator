using TBS.Map.Components;
using TBS.Presentation.Camera;
using UnityEditor;
using UnityEngine;

namespace TBS.Editor
{
    /// <summary>
    /// 棋盘相机设置编辑器工具
    /// </summary>
    public class CameraSetupEditor : EditorWindow
    {
        private HexGrid targetGrid;
        private BoardCameraConfig cameraConfig;
        private bool createNewCamera = true;
        private bool showAdvancedOptions = false;

        [MenuItem("TBS/Camera/Setup Board Camera")]
        public static void ShowWindow()
        {
            GetWindow<CameraSetupEditor>("棋盘相机设置");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("棋盘相机设置工具", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 目标网格选择
            targetGrid = EditorGUILayout.ObjectField(
                "目标网格 (HexGrid)",
                targetGrid,
                typeof(HexGrid),
                true
            ) as HexGrid;

            // 自动查找网格
            if (targetGrid == null)
            {
                EditorGUILayout.HelpBox(
                    "未指定目标网格，将自动查找场景中的 HexGrid 组件",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space(10);

            // 配置选择
            cameraConfig = EditorGUILayout.ObjectField(
                "相机配置 (可选)",
                cameraConfig,
                typeof(BoardCameraConfig),
                false
            ) as BoardCameraConfig;

            EditorGUILayout.Space(10);

            // 选项
            createNewCamera = EditorGUILayout.Toggle("创建新相机", createNewCamera);

            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "高级选项");
            if (showAdvancedOptions)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("相机初始位置", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(15);

            // 创建按钮
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("设置棋盘相机", GUILayout.Height(40)))
            {
                SetupCamera();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // 快捷操作
            EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("查找/聚焦现有相机"))
            {
                FocusExistingCamera();
            }

            if (GUILayout.Button("重置相机位置"))
            {
                ResetCameraPosition();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 使用说明
            EditorGUILayout.LabelField("使用说明", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "操作说明:\n" +
                "• 右键拖拽: 平移视角\n" +
                "• WASD/方向键: 键盘移动\n" +
                "• 鼠标滚轮: 缩放\n" +
                "• Q/E: 旋转视角\n" +
                "• 屏幕边缘: 边缘滚动",
                MessageType.Info
            );
        }

        private void SetupCamera()
        {
            // 自动查找网格
            if (targetGrid == null)
            {
                targetGrid = UnityEngine.Object.FindObjectOfType<HexGrid>();
            }

            BoardCameraController controller;

            if (cameraConfig != null)
            {
                controller = BoardCameraSetup.CreateBoardCameraWithConfig(cameraConfig, targetGrid);
            }
            else
            {
                if (createNewCamera || UnityEngine.Camera.main == null)
                {
                    controller = BoardCameraSetup.CreateBoardCamera(targetGrid);
                }
                else
                {
                    controller = BoardCameraSetup.ConvertExistingCamera(UnityEngine.Camera.main, targetGrid);
                }
            }

            if (controller != null)
            {
                // 选中相机
                Selection.activeGameObject = controller.gameObject;

                // 聚焦
                SceneView.FrameLastActiveSceneViewWithLock();

                EditorUtility.DisplayDialog(
                    "成功",
                    "棋盘相机已设置完成！",
                    "确定"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "错误",
                    "相机设置失败，请检查场景配置。",
                    "确定"
                );
            }
        }

        private void FocusExistingCamera()
        {
            var camera = UnityEngine.Camera.main;
            if (camera != null)
            {
                Selection.activeGameObject = camera.gameObject;
                SceneView.FrameLastActiveSceneViewWithLock();
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "提示",
                    "场景中未找到主相机。",
                    "确定"
                );
            }
        }

        private void ResetCameraPosition()
        {
            var controller = UnityEngine.Object.FindObjectOfType<BoardCameraController>();
            if (controller != null)
            {
                controller.ResetCamera();
                EditorUtility.DisplayDialog("成功", "相机位置已重置", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "提示",
                    "场景中未找到 BoardCameraController。",
                    "确定"
                );
            }
        }
    }

    /// <summary>
    /// 在场景视图中的右键菜单快捷设置
    /// </summary>
    public class CameraSceneMenu
    {
        [MenuItem("GameObject/TBS/Camera/Board Camera", false, 10)]
        public static void CreateBoardCamera(MenuCommand menuCommand)
        {
            var grid = UnityEngine.Object.FindObjectOfType<HexGrid>();
            var controller = BoardCameraSetup.CreateBoardCamera(grid);

            if (controller != null)
            {
                Undo.RegisterCreatedObjectUndo(controller.gameObject, "Create Board Camera");
                Selection.activeObject = controller.gameObject;
            }
        }
    }
}
