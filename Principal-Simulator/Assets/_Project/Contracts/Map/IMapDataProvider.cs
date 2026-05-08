using TBS.Map.Runtime;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.API
{
    /// <summary>
    /// 地图数据提供接口 - 供逻辑层查询地图数据
    /// </summary>
    public interface IMapDataProvider
    {
        /// <summary>
        /// 获取指定坐标的地块
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        /// <returns>地块实例，如不存在返回null</returns>
        MapTileCell GetTile(MapHexCoord coord);

        /// <summary>
        /// 获取指定坐标范围内的所有地块
        /// </summary>
        /// <param name="center">中心坐标</param>
        /// <param name="range">半径范围</param>
        /// <returns>范围内的地块数组</returns>
        MapTileCell[] GetTilesInRange(MapHexCoord center, int range);

        /// <summary>
        /// 获取地图边界
        /// </summary>
        /// <returns>地图边界范围</returns>
        BoundsInt GetMapBounds();
    }

    /// <summary>
    /// 地形查询接口 - 供战斗、移动系统查询地形属性
    /// </summary>
    public interface ITerrainQuery
    {
        /// <summary>
        /// 获取指定坐标的移动消耗
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        /// <returns>移动消耗值，不可通行返回float.MaxValue</returns>
        float GetMovementCost(MapHexCoord coord);

        /// <summary>
        /// 获取指定坐标的防御加成
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        /// <returns>防御加成百分比（0.0 ~ 1.0）</returns>
        float GetDefenseBonus(MapHexCoord coord);

        /// <summary>
        /// 获取指定坐标的视野修正
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        /// <returns>视野修正值（正值增加视野，负值减少）</returns>
        float GetVisibilityModifier(MapHexCoord coord);

        /// <summary>
        /// 检查坐标是否可通过
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        /// <returns>是否可通过</returns>
        bool IsPassable(MapHexCoord coord);
    }
}
