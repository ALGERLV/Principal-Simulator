using System;
using System.Collections.Generic;
using UnityEngine;

namespace TBS.Map.Tools
{
    /// <summary>
    /// 六边形方向枚举（6个方向）。
    /// </summary>
    public enum HexDirection
    {
        North,      // 北 (0°)
        NorthEast,  // 东北 (60°)
        SouthEast,  // 东南 (120°)
        South,      // 南 (180°)
        SouthWest,  // 西南 (240°)
        NorthWest   // 西北 (300°)
    }

    /// <summary>
    /// 偏移坐标类型（用于坐标转换）。
    /// </summary>
    public enum OffsetCoordType
    {
        OddR,   // 奇数行偏移
        EvenR,  // 偶数行偏移
        OddQ,   // 奇数列偏移
        EvenQ   // 偶数列偏移
    }

    /// <summary>
    /// 六边形轴向坐标结构体（Q, R，S = -Q-R）。
    /// </summary>
    [Serializable]
    public readonly struct MapHexCoord : IEquatable<MapHexCoord>
    {
        public readonly int Q;
        public readonly int R;

        public int S => -Q - R;

        private static readonly MapHexCoord[] Directions = new MapHexCoord[]
        {
            new MapHexCoord(0, -1),   // 北
            new MapHexCoord(1, -1),   // 东北
            new MapHexCoord(1, 0),    // 东南
            new MapHexCoord(0, 1),    // 南
            new MapHexCoord(-1, 1),   // 西南
            new MapHexCoord(-1, 0)    // 西北
        };

        public MapHexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }

        public static MapHexCoord FromAxial(int q, int r) => new MapHexCoord(q, r);

        public static MapHexCoord FromCube(int x, int y, int z)
        {
            if (x + y + z != 0)
                throw new ArgumentException("立方坐标必须满足 x + y + z = 0");
            return new MapHexCoord(x, z);
        }

        public static MapHexCoord FromOffset(int col, int row, OffsetCoordType type)
        {
            int q, r;
            switch (type)
            {
                case OffsetCoordType.OddR:
                    q = col - (row - (row & 1)) / 2;
                    r = row;
                    break;
                case OffsetCoordType.EvenR:
                    q = col - (row + (row & 1)) / 2;
                    r = row;
                    break;
                case OffsetCoordType.OddQ:
                    q = col;
                    r = row - (col - (col & 1)) / 2;
                    break;
                case OffsetCoordType.EvenQ:
                    q = col;
                    r = row - (col + (col & 1)) / 2;
                    break;
                default:
                    throw new ArgumentException("未知的偏移坐标类型");
            }
            return new MapHexCoord(q, r);
        }

        public void Deconstruct(out int q, out int r)
        {
            q = Q;
            r = R;
        }

        public int DistanceTo(MapHexCoord other)
        {
            return (Mathf.Abs(Q - other.Q) + Mathf.Abs(Q + R - other.Q - other.R) + Mathf.Abs(R - other.R)) / 2;
        }

        public MapHexCoord[] GetNeighbors()
        {
            var neighbors = new MapHexCoord[6];
            for (int i = 0; i < 6; i++)
                neighbors[i] = this + Directions[i];
            return neighbors;
        }

        public MapHexCoord GetNeighbor(HexDirection direction)
        {
            return this + Directions[(int)direction];
        }

        public MapHexCoord[] GetRing(int radius)
        {
            if (radius == 0)
                return new MapHexCoord[] { this };

            var result = new List<MapHexCoord>();
            var current = this + Directions[(int)HexDirection.NorthWest] * radius;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    result.Add(current);
                    current = current.GetNeighbor((HexDirection)i);
                }
            }

            return result.ToArray();
        }

        public MapHexCoord[] GetSpiral(int maxRadius)
        {
            var result = new List<MapHexCoord> { this };
            for (int r = 1; r <= maxRadius; r++)
                result.AddRange(GetRing(r));
            return result.ToArray();
        }

        public MapHexCoord[] GetLine(MapHexCoord target)
        {
            int distance = DistanceTo(target);
            if (distance == 0)
                return new MapHexCoord[] { this };

            var result = new List<MapHexCoord>();
            for (int i = 0; i <= distance; i++)
            {
                float t = i / (float)distance;
                result.Add(Lerp(this, target, t));
            }
            return result.ToArray();
        }

        public MapHexCoord Rotate(int steps)
        {
            steps = ((steps % 6) + 6) % 6;
            if (steps == 0) return this;

            int x = Q;
            int z = R;
            int y = -x - z;

            for (int i = 0; i < steps; i++)
            {
                int newX = -z;
                int newZ = -x;
                int newY = -y;
                x = newX;
                y = newY;
                z = newZ;
            }

            return new MapHexCoord(x, z);
        }

        public MapHexCoord Reflect(HexDirection axis)
        {
            int x = Q;
            int z = R;
            int y = -x - z;

            return axis switch
            {
                HexDirection.North or HexDirection.South => new MapHexCoord(x, y), // Q轴
                HexDirection.NorthEast or HexDirection.SouthWest => new MapHexCoord(z, y), // R轴
                HexDirection.NorthWest or HexDirection.SouthEast => new MapHexCoord(z, x), // S轴
                _ => this
            };
        }

        public Vector2Int ToOffset(OffsetCoordType type)
        {
            return type switch
            {
                OffsetCoordType.OddR => new Vector2Int(Q + (R - (R & 1)) / 2, R),
                OffsetCoordType.EvenR => new Vector2Int(Q + (R + (R & 1)) / 2, R),
                OffsetCoordType.OddQ => new Vector2Int(Q, R + (Q - (Q & 1)) / 2),
                OffsetCoordType.EvenQ => new Vector2Int(Q, R + (Q + (Q & 1)) / 2),
                _ => new Vector2Int(Q, R)
            };
        }

        public static MapHexCoord operator +(MapHexCoord a, MapHexCoord b)
            => new MapHexCoord(a.Q + b.Q, a.R + b.R);

        public static MapHexCoord operator -(MapHexCoord a, MapHexCoord b)
            => new MapHexCoord(a.Q - b.Q, a.R - b.R);

        public static MapHexCoord operator *(MapHexCoord a, int factor)
            => new MapHexCoord(a.Q * factor, a.R * factor);

        public static bool operator ==(MapHexCoord a, MapHexCoord b)
            => a.Q == b.Q && a.R == b.R;

        public static bool operator !=(MapHexCoord a, MapHexCoord b)
            => !(a == b);

        public bool Equals(MapHexCoord other)
            => Q == other.Q && R == other.R;

        public override bool Equals(object obj)
            => obj is MapHexCoord coord && Equals(coord);

        public override int GetHashCode()
            => HashCode.Combine(Q, R);

        public override string ToString()
            => $"MapHexCoord({Q}, {R})";

        private static MapHexCoord Lerp(MapHexCoord a, MapHexCoord b, float t)
        {
            float x = Mathf.Lerp(a.Q, b.Q, t);
            float z = Mathf.Lerp(a.R, b.R, t);
            float y = Mathf.Lerp(-a.Q - a.R, -b.Q - b.R, t);

            int rx = Mathf.RoundToInt(x);
            int rz = Mathf.RoundToInt(z);
            int ry = Mathf.RoundToInt(y);

            float xDiff = Mathf.Abs(rx - x);
            float zDiff = Mathf.Abs(rz - z);
            float yDiff = Mathf.Abs(ry - y);

            if (xDiff > zDiff && xDiff > yDiff)
                rx = -rz - ry;
            else if (zDiff > yDiff)
                rz = -rx - ry;

            return new MapHexCoord(rx, rz);
        }
    }

    /// <summary>
    /// 六边形拓扑工具类 - 提供静态拓扑计算方法。
    /// </summary>
    public static class MapHexTopology
    {
        /// <summary>获取两个坐标之间的所有坐标（线性插值）。</summary>
        public static MapHexCoord[] GetLine(MapHexCoord from, MapHexCoord to)
        {
            return from.GetLine(to);
        }

        /// <summary>获取指定范围内的所有坐标（六边形距离）。</summary>
        public static MapHexCoord[] GetRange(MapHexCoord center, int radius)
        {
            return center.GetSpiral(radius);
        }

        /// <summary>检查两个坐标是否相邻。</summary>
        public static bool IsNeighbor(MapHexCoord a, MapHexCoord b)
        {
            return a.DistanceTo(b) == 1;
        }

        /// <summary>获取从a到b的方向（若相邻），否则返回null。</summary>
        public static HexDirection? GetDirection(MapHexCoord from, MapHexCoord to)
        {
            var diff = to - from;
            for (int i = 0; i < 6; i++)
            {
                if (diff == GetDirectionDelta((HexDirection)i))
                    return (HexDirection)i;
            }
            return null;
        }

        private static MapHexCoord GetDirectionDelta(HexDirection dir)
        {
            return dir switch
            {
                HexDirection.North => new MapHexCoord(0, -1),
                HexDirection.NorthEast => new MapHexCoord(1, -1),
                HexDirection.SouthEast => new MapHexCoord(1, 0),
                HexDirection.South => new MapHexCoord(0, 1),
                HexDirection.SouthWest => new MapHexCoord(-1, 1),
                HexDirection.NorthWest => new MapHexCoord(-1, 0),
                _ => new MapHexCoord(0, 0)
            };
        }
    }
}
