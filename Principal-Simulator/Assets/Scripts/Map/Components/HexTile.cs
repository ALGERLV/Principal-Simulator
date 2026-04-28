using System;
using TBS.Map.API;
using TBS.Map.Data;
using TBS.Map.Tools;
using UnityEngine;

// 明确指定使用我们的TerrainData，避免与UnityEngine.TerrainData冲突
using TerrainData = TBS.Map.Data.TerrainData;

namespace TBS.Map.Components
{
    /// <summary>
    /// 六边形地块组件 - 表示网格中的一个地块
    /// </summary>
    public class HexTile : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// 单位进入地块时触发
        /// </summary>
        public event Action<IUnit> OnUnitEntered;

        /// <summary>
        /// 单位离开地块时触发
        /// </summary>
        public event Action<IUnit> OnUnitExited;

        #endregion

        #region Serialized Fields

        [SerializeField] private HexCoord coord;
        [SerializeField] private TerrainData terrainData;
        private IUnit occupyingUnit;
        [SerializeField] private VisibilityState visibilityState = VisibilityState.Fog;

        #endregion

        #region Public Properties

        /// <summary>
        /// 地块坐标
        /// </summary>
        public HexCoord Coord => coord;

        /// <summary>
        /// 地形数据
        /// </summary>
        public TerrainData TerrainData => terrainData;

        /// <summary>
        /// 地形类型ID
        /// </summary>
        public string TerrainId => terrainData?.TerrainId ?? "unknown";

        /// <summary>
        /// 地形显示名称
        /// </summary>
        public string TerrainName => terrainData?.TerrainName ?? "未知地形";

        /// <summary>
        /// 移动消耗
        /// </summary>
        public float MovementCost => terrainData?.GetMovementCost(null) ?? 1f;

        /// <summary>
        /// 防御加成
        /// </summary>
        public float DefenseBonus => terrainData?.GetDefenseBonus() ?? 0f;

        /// <summary>
        /// 视野修正
        /// </summary>
        public float VisibilityModifier => terrainData?.GetVisibilityModifier() ?? 0f;

        /// <summary>
        /// 是否可通过
        /// </summary>
        public bool IsPassable => terrainData?.IsPassable(null) ?? true;

        /// <summary>
        /// 占据此地块的单位
        /// </summary>
        public IUnit OccupyingUnit
        {
            get => occupyingUnit;
            private set => occupyingUnit = value;
        }

        /// <summary>
        /// 是否被单位占据
        /// </summary>
        public bool IsOccupied => OccupyingUnit != null;

        /// <summary>
        /// 可见性状态
        /// </summary>
        public VisibilityState VisibilityState
        {
            get => visibilityState;
            set => visibilityState = value;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化地块
        /// </summary>
        public void Initialize(HexCoord coordinate, TerrainData terrain)
        {
            coord = coordinate;
            terrainData = terrain;
            visibilityState = VisibilityState.Fog;
            occupyingUnit = null;

            // 设置GameObject名称
            gameObject.name = $"Tile_{coord.Q}_{coord.R}";

            // 确保有渲染器并更新视觉
            var renderer = GetComponent<HexTileRenderer>();
            if (renderer == null)
            {
                renderer = gameObject.AddComponent<HexTileRenderer>();
            }
            renderer.UpdateVisuals();
        }

        private void OnValidate()
        {
            // Unity编辑器验证
            if (terrainData == null)
            {
                Debug.LogWarning($"HexTile at {coord} 缺少地形数据", this);
            }
        }

        #endregion

        #region Unit Management

        /// <summary>
        /// 设置占据单位
        /// </summary>
        public void SetOccupyingUnit(IUnit unit)
        {
            if (unit == null)
            {
                ClearOccupyingUnit();
                return;
            }

            var previousUnit = occupyingUnit;
            occupyingUnit = unit;

            if (previousUnit != null)
            {
                OnUnitExited?.Invoke(previousUnit);
            }

            OnUnitEntered?.Invoke(unit);
        }

        /// <summary>
        /// 清除占据单位
        /// </summary>
        public void ClearOccupyingUnit()
        {
            if (occupyingUnit != null)
            {
                var unit = occupyingUnit;
                occupyingUnit = null;
                OnUnitExited?.Invoke(unit);
            }
        }

        /// <summary>
        /// 检查单位是否可以进入此地块
        /// </summary>
        public bool CanEnter(IUnit unit)
        {
            if (unit == null) return false;

            // 检查地形是否可通过
            if (terrainData != null && !terrainData.IsPassable(unit))
            {
                return false;
            }

            // 检查是否已被其他单位占据
            if (IsOccupied && OccupyingUnit != unit)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Terrain Effect Methods

        /// <summary>
        /// 获取指定单位的移动消耗
        /// </summary>
        public float GetMovementCostForUnit(IUnit unit)
        {
            return terrainData?.GetMovementCost(unit) ?? 1f;
        }

        /// <summary>
        /// 获取防御加成
        /// </summary>
        public float GetDefenseBonus()
        {
            return terrainData?.GetDefenseBonus() ?? 0f;
        }

        /// <summary>
        /// 获取视野修正
        /// </summary>
        public float GetVisibilityModifier()
        {
            return terrainData?.GetVisibilityModifier() ?? 0f;
        }

        /// <summary>
        /// 检查是否拥有特定地形特性
        /// </summary>
        public bool HasTerrainFeature(string featureId)
        {
            return terrainData?.HasFeature(featureId) ?? false;
        }

        /// <summary>
        /// 获取地形属性值
        /// </summary>
        public T GetTerrainProperty<T>(string propertyName)
        {
            if (terrainData == null) return default;
            return terrainData.GetProperty<T>(propertyName);
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return $"HexTile[{coord}] - {TerrainName} (Unit: {(IsOccupied ? OccupyingUnit.UnitName : "None")})";
        }

        #endregion
    }

    /// <summary>
    /// 可见性状态
    /// </summary>
    public enum VisibilityState
    {
        /// <summary>
        /// 战争迷雾 - 完全未知
        /// </summary>
        Fog,

        /// <summary>
        /// 曾探索过 - 知道地形但无当前视野
        /// </summary>
        Explored,

        /// <summary>
        /// 当前可见
        /// </summary>
        Visible
    }
}
