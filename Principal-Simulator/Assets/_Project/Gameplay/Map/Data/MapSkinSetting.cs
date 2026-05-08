using System.Collections.Generic;
using UnityEngine;

namespace TBS.Map.Data
{
    /// <summary>
    /// 地图系统设计 3.2：地形主题库配置。
    /// 定义每种地形类型的视觉表现：纹理、法线、色调、海拔偏移、装饰物。
    /// 由 MapManager 在编辑器中拖入引用，不通过 CreateAssetMenu 创建。
    /// </summary>
    public class MapSkinSetting : ScriptableObject
    {
        [Header("标识")]
        [SerializeField] private string terrainId = "plain";

        [Header("视觉纹理")]
        [SerializeField] private Texture2D baseAlbedo;
        [SerializeField] private Texture2D normalMap;

        [Header("色调与偏移")]
        [SerializeField] private Color tintColor = Color.white;
        [SerializeField, Tooltip("海拔对应的额外视觉抬高（单位：世界单位）")]
        private float elevationVisualOffset = 0f;

        [Header("装饰物（可选）")]
        [SerializeField] private GameObject[] randomDecorations;
        [SerializeField, Tooltip("每个地块装饰物生成概率")]
        [Range(0f, 1f)] private float decorationChance = 0.2f;

        public string TerrainId => terrainId;
        public Texture2D BaseAlbedo => baseAlbedo;
        public Texture2D NormalMap => normalMap;
        public Color TintColor => tintColor;
        public float ElevationVisualOffset => elevationVisualOffset;
        public IReadOnlyList<GameObject> RandomDecorations => randomDecorations;
        public float DecorationChance => decorationChance;
    }
}
