using System;
using System.Collections.Generic;
using UnityEngine;

namespace TBS.Map.Tools
{
    /// <summary>
    /// 六边形坐标 - 轴向坐标系统（Axial Coordinates）
    /// </summary>
    [Serializable]
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        #region Fields

        [SerializeField] private readonly int q;
        [SerializeField] private readonly int r;

        #endregion

        #region Properties

        public int Q => q;
        public int R => r;

        /// <summary>
        /// S轴分量（立方体坐标），计算属性：S = -Q - R
        /// </summary>
        public int S => -q - r;

        /// <summary>
        /// 立方体坐标X轴（同Q）
        /// </summary>
        public int X => q;

        /// <summary>
        /// 立方体坐标Y轴（同S）
        /// </summary>
        public int Y => -q - r;

        /// <summary>
        /// 立方体坐标Z轴（同R）
        /// </summary>
        public int Z => r;

        #endregion

        #region Static Direction Vectors

        /// <summary>
        /// 六边形的6个方向（从正上方开始顺时针）
        /// 索引：0=北, 1=东北, 2=东南, 3=南, 4=西南, 5=西北
        /// </summary>
        private static readonly HexCoord[] Directions = new HexCoord[]
        {
            new HexCoord(0, -1),   // 北 (0°)
            new HexCoord(1, -1),   // 东北 (60°)
            new HexCoord(1, 0),    // 东南 (120°)
            new HexCoord(0, 1),    // 南 (180°)
            new HexCoord(-1, 1),   // 西南 (240°)
            new HexCoord(-1, 0),   // 西北 (300°)
        };

        #endregion

        #region Constructor

        public HexCoord(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// 从轴向坐标创建
        /// </summary>
        public static HexCoord FromAxial(int q, int r)
        {
            return new HexCoord(q, r);
        }

        /// <summary>
        /// 从立方体坐标创建（自动转换为轴向）
        /// </summary>
        public static HexCoord FromCube(int x, int y, int z)
        {
            if (x + y + z != 0)
            {
                throw new ArgumentException($"无效的立方体坐标: ({x}, {y}, {z})，三数之和必须等于0");
            }
            return new HexCoord(x, z);
        }

        /// <summary>
        /// 从偏移坐标创建（支持奇数行和偶数行偏移）
        /// </summary>
        public static HexCoord FromOffset(int col, int row, OffsetCoordType type)
        {
            int q, r;

            switch (type)
            {
                case OffsetCoordType.OddR:
                    // 奇数行偏移（Odd-R）
                    q = col - (row - (row & 1)) / 2;
                    r = row;
                    break;
                case OffsetCoordType.EvenR:
                    // 偶数行偏移（Even-R）
                    q = col - (row + (row & 1)) / 2;
                    r = row;
                    break;
                case OffsetCoordType.OddQ:
                    // 奇数列偏移（Odd-Q）
                    q = col;
                    r = row - (col - (col & 1)) / 2;
                    break;
                case OffsetCoordType.EvenQ:
                    // 偶数列偏移（Even-Q）
                    q = col;
                    r = row - (col + (col & 1)) / 2;
                    break;
                default:
                    throw new ArgumentException($"不支持的偏移坐标类型: {type}");
            }

            return new HexCoord(q, r);
        }

        #endregion

        #region Conversion Methods

        /// <summary>
        /// 转换为偏移坐标
        /// </summary>
        public Vector2Int ToOffset(OffsetCoordType type)
        {
            int col, row;

            switch (type)
            {
                case OffsetCoordType.OddR:
                    col = q + (r - (r & 1)) / 2;
                    row = r;
                    break;
                case OffsetCoordType.EvenR:
                    col = q + (r + (r & 1)) / 2;
                    row = r;
                    break;
                case OffsetCoordType.OddQ:
                    col = q;
                    row = r + (q - (q & 1)) / 2;
                    break;
                case OffsetCoordType.EvenQ:
                    col = q;
                    row = r + (q + (q & 1)) / 2;
                    break;
                default:
                    throw new ArgumentException($"不支持的偏移坐标类型: {type}");
            }

            return new Vector2Int(col, row);
        }

        /// <summary>
        /// 转换为世界坐标（平面XZ）
        /// </summary>
        public Vector3 ToWorldPosition(float hexSize, HexOrientation orientation)
        {
            float x, z;

            if (orientation == HexOrientation.PointyTop)
            {
                // 尖顶朝向
                x = hexSize * (Mathf.Sqrt(3) * q + Mathf.Sqrt(3) / 2 * r);
                z = hexSize * (3f / 2 * r);
            }
            else
            {
                // 平顶朝向
                x = hexSize * (3f / 2 * q);
                z = hexSize * (Mathf.Sqrt(3) / 2 * q + Mathf.Sqrt(3) * r);
            }

            return new Vector3(x, 0, z);
        }

        #endregion

        #region Calculation Methods

        /// <summary>
        /// 计算到另一坐标的距离
        /// 立方体坐标距离 = (|x1-x2| + |y1-y2| + |z1-z2|) / 2
        /// </summary>
        public int DistanceTo(HexCoord other)
        {
            return (Mathf.Abs(Q - other.Q) + Mathf.Abs(S - other.S) + Mathf.Abs(R - other.R)) / 2;
        }

        /// <summary>
        /// 获取所有6个邻接坐标
        /// </summary>
        public HexCoord[] GetNeighbors()
        {
            var neighbors = new HexCoord[6];
            for (int i = 0; i < 6; i++)
            {
                neighbors[i] = GetNeighbor((HexDirection)i);
            }
            return neighbors;
        }

        /// <summary>
        /// 获取指定方向的邻接坐标
        /// </summary>
        public HexCoord GetNeighbor(HexDirection direction)
        {
            return this + Directions[(int)direction];
        }

        /// <summary>
        /// 获取指定半径的环上所有坐标
        /// </summary>
        public HexCoord[] GetRing(int radius)
        {
            if (radius < 0)
                throw new ArgumentException("半径不能为负数");

            if (radius == 0)
                return new HexCoord[] { this };

            var result = new List<HexCoord>();
            var current = this + Directions[(int)HexDirection.NorthWest] * radius;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    result.Add(current);
                    current = current.GetNeighbor((HexDirection)((i + 2) % 6));
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取螺旋范围（从半径0到maxRadius）的所有坐标
        /// </summary>
        public HexCoord[] GetSpiral(int maxRadius)
        {
            var result = new List<HexCoord> { this };

            for (int radius = 1; radius <= maxRadius; radius++)
            {
                result.AddRange(GetRing(radius));
            }

            return result.ToArray();
        }

        /// <summary>
        /// 绕原点旋转（60度整数倍）
        /// </summary>
        /// <param name="steps">旋转步数，正数为顺时针，负数为逆时针</param>
        public HexCoord Rotate(int steps)
        {
            steps = ((steps % 6) + 6) % 6; // 归一化到 0-5

            var x = X;
            var y = Y;
            var z = Z;

            for (int i = 0; i < steps; i++)
            {
                // 顺时针旋转60度: (x, y, z) -> (-z, -x, -y)
                var newX = -z;
                var newY = -x;
                var newZ = -y;
                x = newX;
                y = newY;
                z = newZ;
            }

            return FromCube(x, y, z);
        }

        /// <summary>
        /// 沿指定方向轴反射
        /// </summary>
        public HexCoord Reflect(HexDirection axis)
        {
            // 沿Q轴反射: (q, r, s) -> (q, s, r)
            // 沿R轴反射: (q, r, s) -> (s, r, q)
            // 沿S轴反射: (q, r, s) -> (r, q, s)

            return axis switch
            {
                HexDirection.North or HexDirection.South => new HexCoord(Q, S), // Q轴
                HexDirection.NorthEast or HexDirection.SouthWest => new HexCoord(S, R), // R轴
                HexDirection.NorthWest or HexDirection.SouthEast => new HexCoord(R, Q), // S轴
                _ => this
            };
        }

        #endregion

        #region Operators

        public static HexCoord operator +(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.Q + b.Q, a.R + b.R);
        }

        public static HexCoord operator -(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.Q - b.Q, a.R - b.R);
        }

        public static HexCoord operator *(HexCoord a, int factor)
        {
            return new HexCoord(a.Q * factor, a.R * factor);
        }

        public static bool operator ==(HexCoord a, HexCoord b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(HexCoord a, HexCoord b)
        {
            return !a.Equals(b);
        }

        #endregion

        #region Equality

        public bool Equals(HexCoord other)
        {
            return q == other.q && r == other.r;
        }

        public override bool Equals(object obj)
        {
            return obj is HexCoord coord && Equals(coord);
        }

        public override int GetHashCode()
        {
            // Cantor配对函数生成哈希码
            return (q + r) * (q + r + 1) / 2 + r;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"HexCoord({Q}, {R})";
        }

        public string ToCubeString()
        {
            return $"Cube({X}, {Y}, {Z})";
        }

        #endregion
    }

    /// <summary>
    /// 偏移坐标类型
    /// </summary>
    public enum OffsetCoordType
    {
        OddR,   // 奇数行偏移（行偏移）
        EvenR,  // 偶数行偏移（行偏移）
        OddQ,   // 奇数列偏移（列偏移）
        EvenQ   // 偶数列偏移（列偏移）
    }

    /// <summary>
    /// 六边形朝向
    /// </summary>
    public enum HexOrientation
    {
        PointyTop,  // 尖顶朝上
        FlatTop     // 平顶朝上
    }

    /// <summary>
    /// 六边形方向（从正上方开始顺时针）
    /// </summary>
    public enum HexDirection
    {
        North = 0,
        NorthEast = 1,
        SouthEast = 2,
        South = 3,
        SouthWest = 4,
        NorthWest = 5
    }
}
