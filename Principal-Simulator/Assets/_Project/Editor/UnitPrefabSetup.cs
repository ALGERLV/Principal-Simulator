#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnitComponent = TBS.Unit.Unit;
using UnitRendererComponent = TBS.Unit.UnitRenderer;

namespace TBS.Editor
{
    /// <summary>
    /// 菜单：Game/Setup Unit Prefab
    /// 在 Resources/Prefabs/Units/ 下生成通用胶囊体单位预制体
    /// 预制体已挂载 Unit + UnitRenderer 组件，阵营色由 UnitRenderer 运行时设置
    /// </summary>
    public static class UnitPrefabSetup
    {
        private const string PrefabPath  = "Assets/Resources/Prefabs/Units/Unit.prefab";
        private const string MaterialDir = "Assets/Resources/Materials/Units/";

        [MenuItem("Game/Setup Unit Prefab")]
        public static void CreateUnitPrefab()
        {
            // 1. 确保目录存在
            System.IO.Directory.CreateDirectory(MaterialDir.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString()));
            System.IO.Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(PrefabPath).Replace("/", System.IO.Path.DirectorySeparatorChar.ToString()));

            // 2. 创建胶囊体 GameObject
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Unit";

            // 3. 调整胶囊体大小（比 MapSurfaceTile 略小）
            go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // 4. 创建默认白色材质（颜色由 UnitRenderer 运行时用 MaterialPropertyBlock 覆盖）
            var mat = CreateMaterial("Unit_Default", Color.white);
            go.GetComponent<Renderer>().sharedMaterial = mat;

            // 5. 挂载组件
            go.AddComponent<UnitComponent>();
            go.AddComponent<UnitRendererComponent>();

            // 6. 保存为预制体
            string dir = System.IO.Path.GetDirectoryName(PrefabPath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                // 逐级创建
                string[] parts = dir.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }

            bool success;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath, out success);
            Object.DestroyImmediate(go);

            if (success)
            {
                Debug.Log($"[UnitPrefabSetup] 预制体已生成：{PrefabPath}");
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            else
            {
                Debug.LogError($"[UnitPrefabSetup] 预制体保存失败");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = MaterialDir + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        [MenuItem("Game/Setup Unit Prefab", validate = true)]
        static bool ValidateCreate() => !EditorApplication.isPlaying;
    }
}
#endif
