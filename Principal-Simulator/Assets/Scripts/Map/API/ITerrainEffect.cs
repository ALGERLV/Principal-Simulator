using UnityEngine;

namespace TBS.Map.API
{
    /// <summary>
    /// 地形效果接口契约 - 定义地形对移动、战斗、视野的影响
    /// </summary>
    public interface ITerrainEffect
    {
        /// <summary>
        /// 地形类型唯一标识
        /// </summary>
        string TerrainId { get; }

        /// <summary>
        /// 地形显示名称
        /// </summary>
        string TerrainName { get; }

        /// <summary>
        /// 获取移动消耗
        /// </summary>
        /// <param name="unit">移动单位（可选，用于计算单位特性影响）</param>
        /// <returns>移动消耗值，1.0为基准</returns>
        float GetMovementCost(IUnit unit = null);

        /// <summary>
        /// 获取防御加成百分比
        /// </summary>
        /// <returns>防御加成（0.0 ~ 1.0）</returns>
        float GetDefenseBonus();

        /// <summary>
        /// 获取视野修正值
        /// </summary>
        /// <returns>视野修正（正值增加视野，负值减少）</returns>
        float GetVisibilityModifier();

        /// <summary>
        /// 检查是否可通过
        /// </summary>
        /// <param name="unit">移动单位（可选）</param>
        /// <returns>是否可通过</returns>
        bool IsPassable(IUnit unit = null);

        /// <summary>
        /// 获取扩展属性值
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="key">属性键</param>
        /// <returns>属性值</returns>
        T GetProperty<T>(string key);

        /// <summary>
        /// 检查是否拥有特定特性
        /// </summary>
        /// <param name="featureId">特性标识</param>
        /// <returns>是否拥有该特性</returns>
        bool HasFeature(string featureId);
    }

    /// <summary>
    /// 单位接口 - 用于地形效果计算时的单位特性查询
    /// </summary>
    public interface IUnit
    {
        string UnitId { get; }
        string UnitName { get; }

        /// <summary>
        /// 应用地形移动消耗修正
        /// </summary>
        float ApplyTerrainMovementModifier(float baseCost, string terrainId);

        /// <summary>
        /// 检查是否可以通过特定地形
        /// </summary>
        bool CanTraverseTerrain(string terrainId);
    }
}
