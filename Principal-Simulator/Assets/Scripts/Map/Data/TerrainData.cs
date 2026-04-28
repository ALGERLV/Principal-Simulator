using System.Collections.Generic;
using TBS.Map.API;
using UnityEngine;

namespace TBS.Map.Data
{
    /// <summary>
    /// 地形数据 - ScriptableObject，定义地形的属性和效果
    /// </summary>
    [CreateAssetMenu(fileName = "NewTerrain", menuName = "Game/Terrain Data", order = 1)]
    public class TerrainData : ScriptableObject, ITerrainEffect
    {
        #region Serialized Fields

        [Header("基础信息")]
        [SerializeField] private string terrainId = "plain";
        [SerializeField] private string terrainName = "平原";
        [SerializeField, TextArea(2, 4)] private string description = "普通地形，无特殊效果";

        [Header("移动属性")]
        [SerializeField, Tooltip("移动消耗倍数，1.0为基准")]
        private float movementCost = 1f;
        [SerializeField, Tooltip("是否可通过")]
        private bool isPassable = true;

        [Header("战斗属性")]
        [SerializeField, Tooltip("防御加成 (0.0 ~ 1.0)"), Range(-0.5f, 1f)]
        private float defenseBonus = 0f;
        [SerializeField, Tooltip("攻击加成 (0.0 ~ 1.0)"), Range(-0.5f, 1f)]
        private float attackBonus = 0f;

        [Header("视野属性")]
        [SerializeField, Tooltip("视野修正（正值增加视野，负值减少）")]
        private float visibilityModifier = 0f;

        [Header("视觉效果")]
        [SerializeField] private Color terrainColor = Color.white;
        [SerializeField] private Material terrainMaterial;

        [Header("扩展属性")]
        [SerializeField] private List<string> features = new List<string>();
        [SerializeField] private SerializedDictionary terrainProperties;

        #endregion

        #region Public Properties

        public string TerrainId => terrainId;
        public string TerrainName => terrainName;
        public string Description => description;
        public float MovementCost => movementCost;
        public float DefenseBonus => defenseBonus;
        public float AttackBonus => attackBonus;
        public float VisibilityModifier => visibilityModifier;
        public Color TerrainColor => terrainColor;
        public Material TerrainMaterial => terrainMaterial;
        public IReadOnlyList<string> Features => features;

        #endregion

        #region ITerrainEffect Implementation

        /// <summary>
        /// 获取移动消耗
        /// </summary>
        public float GetMovementCost(IUnit unit = null)
        {
            float cost = movementCost;

            // 如有单位，计算单位特性影响
            if (unit != null)
            {
                cost = unit.ApplyTerrainMovementModifier(cost, terrainId);
            }

            return cost;
        }

        /// <summary>
        /// 获取防御加成
        /// </summary>
        public float GetDefenseBonus()
        {
            return defenseBonus;
        }

        /// <summary>
        /// 获取视野修正
        /// </summary>
        public float GetVisibilityModifier()
        {
            return visibilityModifier;
        }

        /// <summary>
        /// 检查是否可通过
        /// </summary>
        public bool IsPassable(IUnit unit = null)
        {
            if (!isPassable) return false;

            if (unit != null)
            {
                return unit.CanTraverseTerrain(terrainId);
            }

            return true;
        }

        /// <summary>
        /// 获取扩展属性值
        /// </summary>
        public T GetProperty<T>(string key)
        {
            if (terrainProperties == null)
                return default;

            return terrainProperties.GetValue<T>(key);
        }

        /// <summary>
        /// 检查是否拥有特定特性
        /// </summary>
        public bool HasFeature(string featureId)
        {
            return features != null && features.Contains(featureId);
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // 确保地形ID不为空
            if (string.IsNullOrWhiteSpace(terrainId))
            {
                terrainId = "unknown";
            }

            // 确保移动消耗为正数
            if (movementCost < 0.1f)
            {
                movementCost = 0.1f;
            }
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        /// <summary>
        /// 创建预设地形 - 平原
        /// </summary>
        [UnityEditor.MenuItem("Assets/Create/Game/Terrain Preset")]
        public static void CreatePlainTerrain()
        {
            var terrain = ScriptableObject.CreateInstance<TerrainData>();
            terrain.terrainId = "plain";
            terrain.terrainName = "平原";
            terrain.movementCost = 1f;
            terrain.defenseBonus = 0f;
            terrain.terrainColor = new Color(0.4f, 0.7f, 0.4f);
            terrain.isPassable = true;

            string path = "Assets/Resources/Terrain/Plain.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            UnityEditor.AssetDatabase.CreateAsset(terrain, path);
            UnityEditor.AssetDatabase.SaveAssets();

            // 选中新创建的资源
            UnityEditor.Selection.activeObject = terrain;
        }
#endif

        #endregion
    }

    #region Helper Types

    /// <summary>
    /// 序列化字典（用于扩展属性）
    /// </summary>
    [System.Serializable]
    public class SerializedDictionary
    {
        [SerializeField] private List<SerializedPair> pairs = new List<SerializedPair>();

        private Dictionary<string, object> cache;

        public T GetValue<T>(string key)
        {
            if (cache == null) BuildCache();

            if (cache.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        private void BuildCache()
        {
            cache = new Dictionary<string, object>();
            foreach (var pair in pairs)
            {
                if (!string.IsNullOrEmpty(pair.key) && !cache.ContainsKey(pair.key))
                {
                    cache[pair.key] = pair.value;
                }
            }
        }
    }

    [System.Serializable]
    public class SerializedPair
    {
        public string key;
        public string value;
    }

    #endregion
}
