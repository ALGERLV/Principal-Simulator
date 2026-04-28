using UnityEngine;
using TBS.Map.Tools;

namespace TBS.UnitSystem
{
    /// <summary>
    /// 移动高亮标记 - 存储高亮对应的地块坐标
    /// </summary>
    public class MoveHighlight : MonoBehaviour
    {
        public HexCoord TargetCoord { get; private set; }

        public void Initialize(HexCoord coord)
        {
            TargetCoord = coord;
        }
    }
}