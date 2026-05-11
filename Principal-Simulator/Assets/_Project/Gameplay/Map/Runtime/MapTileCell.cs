using System;
using TBS.Map.API;
using TBS.Map.Tools;
using UnityEngine;

using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Runtime
{
    /// <summary>
    /// 可见性状态枚举。
    /// </summary>
    public enum VisibilityState
    {
        Fog,       // 迷雾
        Explored,  // 已探索
        Visible    // 当前可见
    }

    /// <summary>
    /// 单个六边形地块表现体 - 挂载在每个地块 GameObject 上。
    /// 负责存储地块属性、单位占位、地形逻辑入口。
    /// </summary>
    public class MapTileCell : MonoBehaviour
    {
        #region Events

        public event Action<IUnit> OnUnitEntered;
        public event Action<IUnit> OnUnitExited;

        #endregion

        #region Serialized Fields

        [Header("视觉组件")]
        [SerializeField] private MeshRenderer meshRenderer;

        [SerializeField] private MapHexCoord coord;
        [SerializeField] private TerrainData terrainData;
        [SerializeField, Tooltip("海拔等级，用于 Y 轴视觉抬高"), Range(-5, 10)]
        private int elevationLevel;
        [SerializeField, Range(0f, 1f), Tooltip("植被密度，供植被渲染器采样")]
        private float vegetationDensity = 0.35f;
        [SerializeField] private VisibilityState visibilityState = VisibilityState.Fog;

        #endregion

        #region Private Fields

        private IUnit occupyingUnit;

        #endregion

        #region Public Properties

        public MapHexCoord Coord => coord;

        public int ElevationLevel
        {
            get => elevationLevel;
            set
            {
                if (elevationLevel != value)
                {
                    elevationLevel = value;
                    RefreshVisuals();
                }
            }
        }

        public float VegetationDensity
        {
            get => vegetationDensity;
            set => vegetationDensity = Mathf.Clamp01(value);
        }

        public TerrainData TerrainData => terrainData;

        public string TerrainId => terrainData?.TerrainId ?? "unknown";

        public string TerrainName => terrainData?.TerrainName ?? "未知地形";

        public float MovementCost => terrainData?.GetMovementCost(null) ?? 1f;

        public float DefenseBonus => terrainData?.GetDefenseBonus() ?? 0f;

        public float VisibilityModifier => terrainData?.GetVisibilityModifier() ?? 0f;

        public bool IsPassable => terrainData?.IsPassable(null) ?? true;

        public IUnit OccupyingUnit
        {
            get => occupyingUnit;
            private set => occupyingUnit = value;
        }

        public bool IsOccupied => occupyingUnit != null;

        public VisibilityState Visibility
        {
            get => visibilityState;
            set => visibilityState = value;
        }

        #endregion

        #region Initialization

        /// <summary>初始化地块（创建时调用）。</summary>
        public void Initialize(MapHexCoord coordinate, TerrainData terrain)
        {
            Debug.Log($"[MapTileCell] Initialize 被调用: coord={coordinate}, terrain={(terrain?.TerrainName ?? "null")}");

            coord = coordinate;
            terrainData = terrain;
            visibilityState = VisibilityState.Fog;
            occupyingUnit = null;
            gameObject.name = $"Tile_{coord.Q}_{coord.R}";
            RefreshVisuals();
        }

        /// <summary>运行时替换地形（不依赖 UnityEditor 序列化）。</summary>
        public void ApplyTerrain(TerrainData terrain)
        {
            terrainData = terrain;
            RefreshVisuals();
        }

        void RefreshVisuals()
        {
            Debug.Log($"[MapTileCell {coord}] RefreshVisuals 被调用 - terrainData={(terrainData?.TerrainName ?? "null")}");

            // 找到Hex子对象并设置材质颜色
            Transform hexTransform = transform.Find("Hex");
            if (hexTransform != null)
            {
                if (meshRenderer == null)
                {
                    meshRenderer = hexTransform.GetComponent<MeshRenderer>();
                    Debug.Log($"[MapTileCell {coord}] 在Hex子对象上查找 MeshRenderer: {(meshRenderer != null ? "找到" : "未找到")}");
                }

                if (meshRenderer != null && terrainData != null)
                {
                    // 创建材质实例，避免影响其他地块
                    if (meshRenderer.material == null || meshRenderer.sharedMaterial == meshRenderer.material)
                    {
                        meshRenderer.material = new Material(meshRenderer.sharedMaterial);
                    }
                    meshRenderer.material.color = terrainData.TerrainColor;

                    Debug.Log($"[MapTileCell {coord}] 在Hex子对象上应用地形: {terrainData.TerrainName}, 颜色: {terrainData.TerrainColor}");
                }
                else
                {
                    Debug.LogWarning($"[MapTileCell {coord}] 无法应用颜色: meshRenderer={meshRenderer != null}, terrainData={terrainData != null}");
                }
            }
            else
            {
                Debug.LogWarning($"[MapTileCell {coord}] 找不到Hex子对象");
            }

            // 设置海拔高度（Y轴偏移）
            float elevationY = elevationLevel * 0.06f; // ElevationWorldStep
            Vector3 pos = transform.position;
            pos.y = elevationY;
            transform.position = pos;
        }

        private void OnValidate()
        {
            if (terrainData == null)
                Debug.LogWarning($"[MapTileCell] {coord} 缺少地形数据", this);
        }

        #endregion

        #region Unit Occupancy

        /// <summary>设置占位单位。</summary>
        public void SetOccupyingUnit(IUnit unit)
        {
            if (unit == null)
            {
                ClearOccupyingUnit();
                return;
            }

            var previousUnit = occupyingUnit;
            occupyingUnit = unit;

            if (previousUnit != null && previousUnit != unit)
                OnUnitExited?.Invoke(previousUnit);

            OnUnitEntered?.Invoke(unit);
        }

        /// <summary>清除占位单位。</summary>
        public void ClearOccupyingUnit()
        {
            if (occupyingUnit != null)
            {
                var unit = occupyingUnit;
                occupyingUnit = null;
                OnUnitExited?.Invoke(unit);
            }
        }

        /// <summary>检查单位是否可进入此地块。</summary>
        public bool CanEnter(IUnit unit)
        {
            if (unit == null) return false;
            if (terrainData != null && !terrainData.IsPassable(unit))
                return false;
            if (IsOccupied && OccupyingUnit != unit)
                return false;
            return true;
        }

        #endregion

        #region Terrain Queries

        public float GetMovementCostForUnit(IUnit unit) =>
            terrainData?.GetMovementCost(unit) ?? 1f;

        public float GetDefenseBonus() => terrainData?.GetDefenseBonus() ?? 0f;

        public float GetVisibilityModifier() => terrainData?.GetVisibilityModifier() ?? 0f;

        public bool HasTerrainFeature(string featureId) =>
            terrainData?.HasFeature(featureId) ?? false;

        public T GetTerrainProperty<T>(string propertyName)
        {
            if (terrainData == null) return default;
            return terrainData.GetProperty<T>(propertyName);
        }

        #endregion

        #region Overrides

        public override string ToString() =>
            $"MapTileCell[{coord}] - {TerrainName} (Unit: {(IsOccupied ? OccupyingUnit.UnitName : "None")})";

        #endregion
    }
}
