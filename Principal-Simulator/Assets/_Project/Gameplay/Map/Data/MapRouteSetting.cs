using System;
using System.Collections.Generic;
using TBS.Map.Tools;
using UnityEngine;

namespace TBS.Map.Data
{
    /// <summary>
    /// 地图系统设计 3.3：连接类型（河流/道路）。
    /// </summary>
    public enum MapRouteType
    {
        River,
        Road
    }

    /// <summary>
    /// 地图系统设计 3.3：单条连接的数据结构。
    /// </summary>
    [Serializable]
    public struct MapLinkType
    {
        public MapHexCoord From;
        public MapHexCoord To;
        public MapRouteType Type;
        public float WidthWorld;
        public bool Passable;
    }

    /// <summary>
    /// 地图系统设计 3.3：河流/道路连接配置集合。
    /// 由 MapManager 在编辑器中拖入引用，不通过 CreateAssetMenu 创建。
    /// </summary>
    public class MapRouteSetting : ScriptableObject
    {
        [SerializeField] private List<MapLinkType> links = new List<MapLinkType>();

        public IReadOnlyList<MapLinkType> Links => links;

        /// <summary>
        /// 添加连接（运行时动态构建配置用）
        /// </summary>
        public void AddLink(MapLinkType link)
        {
            if (links == null)
                links = new List<MapLinkType>();
            links.Add(link);
        }

        /// <summary>
        /// 添加多个连接
        /// </summary>
        public void AddLinks(IEnumerable<MapLinkType> newLinks)
        {
            if (links == null)
                links = new List<MapLinkType>();
            links.AddRange(newLinks);
        }

        /// <summary>
        /// 清空所有连接
        /// </summary>
        public void ClearLinks()
        {
            links?.Clear();
        }
    }
}
