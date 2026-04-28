using UnityEngine;

namespace TBS.Unit
{
    /// <summary>
    /// 单位渲染器 — 负责根据阵营/状态刷新胶囊体颜色
    /// 国军=蓝，日军=红，八路军=灰；状态叠加颜色变化
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class UnitRenderer : MonoBehaviour
    {
        // 阵营基础色（文档§9.2简化为三色）
        private static readonly Color ColorKMT    = new Color(0.18f, 0.38f, 0.78f); // 蓝
        private static readonly Color ColorJapan  = new Color(0.82f, 0.15f, 0.15f); // 红
        private static readonly Color ColorPLA    = new Color(0.55f, 0.55f, 0.55f); // 灰

        // 状态叠加色（叠加到基础色上）
        private static readonly Color TintInspired     = new Color(1f,   1f,   0f,   0.35f); // 金黄
        private static readonly Color TintSuppressed   = new Color(1f,   0.45f,0f,   0.5f);  // 橙红
        private static readonly Color TintShaken       = new Color(1f,   0.6f, 0f,   0.5f);  // 橙
        private static readonly Color TintRouted       = new Color(1f,   0f,   0f,   0.7f);  // 深红
        private static readonly Color TintRecuperating = new Color(0.3f, 1f,   0.3f, 0.4f);  // 绿

        private Unit unit;
        private Renderer cachedRenderer;
        private MaterialPropertyBlock mpb;

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public void Initialize(Unit u)
        {
            unit = u;
            cachedRenderer = GetComponent<Renderer>();
            mpb = new MaterialPropertyBlock();
            Refresh();
        }

        private void Update()
        {
            if (unit != null) Refresh();
        }

        private void Refresh()
        {
            Color baseColor = unit.Faction switch
            {
                Faction.KMT   => ColorKMT,
                Faction.Japan => ColorJapan,
                Faction.PLA   => ColorPLA,
                _             => Color.white
            };

            Color tint = unit.State switch
            {
                UnitState.Inspired     => TintInspired,
                UnitState.Suppressed   => TintSuppressed,
                UnitState.Shaken       => TintShaken,
                UnitState.Routed       => TintRouted,
                UnitState.Recuperating => TintRecuperating,
                _                      => Color.clear
            };

            // 线性叠加：base + tint.a * (tint.rgb - base)
            Color final = Color.Lerp(baseColor, new Color(tint.r, tint.g, tint.b), tint.a);

            cachedRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(ColorId, final);
            cachedRenderer.SetPropertyBlock(mpb);
        }
    }
}
