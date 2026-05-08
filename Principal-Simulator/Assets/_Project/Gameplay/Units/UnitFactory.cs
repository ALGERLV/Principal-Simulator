using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Unit
{
    /// <summary>
    /// 单位工厂 — 从 UnitData 实例化 Unit 游戏对象并挂载 UnitRenderer
    /// </summary>
    public static class UnitFactory
    {
        private static GameObject unitPrefab;

        /// <summary>
        /// 注册通用单位预制体（由场景启动时调用）
        /// </summary>
        public static void RegisterPrefab(GameObject prefab) => unitPrefab = prefab;

        /// <summary>
        /// 在指定格坐标生成单位
        /// </summary>
        public static Unit Create(UnitData data, MapHexCoord coord, Vector3 worldPos, Transform parent = null)
        {
            if (unitPrefab == null)
            {
                Debug.LogError("UnitFactory: 未注册 unitPrefab，请调用 RegisterPrefab()");
                return null;
            }

            var go = Object.Instantiate(unitPrefab, worldPos, Quaternion.identity, parent);
            go.name = $"Unit_{data.UnitId}_{coord.Q}_{coord.R}";

            var unit = go.GetComponent<Unit>() ?? go.AddComponent<Unit>();
            unit.Initialize(data, coord);

            var renderer = go.GetComponent<UnitRenderer>() ?? go.AddComponent<UnitRenderer>();
            renderer.Initialize(unit);

            return unit;
        }
    }
}
