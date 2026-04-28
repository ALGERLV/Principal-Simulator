# HexCoord - 六边形坐标系统

> 服从：01-项目级文档/技术栈描述.md  
> 服从：02-系统级文档/地图系统/地图系统设计.md

---

## 1. 类职责

`HexCoord` 是六边形网格的坐标表示类，负责：
- 轴向坐标（q, r）的存储和验证
- 轴向坐标与立方体坐标（x, y, z）的转换
- 六边形邻接坐标的计算
- 坐标间距离计算
- 坐标哈希和相等性比较（用于Dictionary键）

---

## 2. 继承关系

```
System.ValueType (struct)
    └── HexCoord
```

- 定义为 `readonly struct` — 不可变值类型
- 实现 `IEquatable<HexCoord>` — 高效相等性比较

---

## 3. 属性定义

### 3.1 字段（私有，只读）

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `q` | `int` | 轴向坐标Q轴分量（对应立方体x） |
| `r` | `int` | 轴向坐标R轴分量（对应立方体z） |

### 3.2 公有属性

| 属性名 | 类型 | 访问级别 | 说明 |
|--------|------|----------|------|
| `Q` | `int` | `public get` | Q轴分量 |
| `R` | `int` | `public get` | R轴分量 |
| `S` | `int` | `public get` | 计算属性，S = -Q - R（立方体y轴） |
| `X` | `int` | `public get` | 别名，同Q |
| `Y` | `int` | `public get` | 别名，同S |
| `Z` | `int` | `public get` | 别名，同R |

---

## 4. 方法清单

### 4.1 构造函数

```csharp
public HexCoord(int q, int r)
```

### 4.2 静态工厂方法

| 方法签名 | 说明 |
|----------|------|
| `static HexCoord FromAxial(int q, int r)` | 从轴向坐标创建 |
| `static HexCoord FromCube(int x, int y, int z)` | 从立方体坐标创建（自动转换） |
| `static HexCoord FromOffset(int col, int row, OffsetCoordType type)` | 从偏移坐标创建 |

### 4.3 坐标转换方法

| 方法签名 | 说明 |
|----------|------|
| `Vector2Int ToOffset(OffsetCoordType type)` | 转换为偏移坐标 |
| `Vector3 ToWorldPosition(float hexSize, HexOrientation orientation)` | 转换为世界坐标 |

### 4.4 计算方法

| 方法签名 | 说明 |
|----------|------|
| `int DistanceTo(HexCoord other)` | 计算到另一坐标的距离 |
| `HexCoord[] GetNeighbors()` | 获取6个邻接坐标 |
| `HexCoord GetNeighbor(HexDirection direction)` | 获取指定方向的邻接坐标 |
| `HexCoord[] GetRing(int radius)` | 获取指定半径的环上所有坐标 |
| `HexCoord[] GetSpiral(int maxRadius)` | 获取螺旋范围的所有坐标 |
| `HexCoord Rotate(int steps)` | 绕原点旋转（60度整数倍） |
| `HexCoord Reflect(HexDirection axis)` | 沿指定方向反射 |
| `HexCoord Add(HexCoord other)` | 坐标相加 |
| `HexCoord Scale(int factor)` | 坐标缩放 |

### 4.5 相等性和哈希

| 方法签名 | 说明 |
|----------|------|
| `bool Equals(HexCoord other)` | IEquatable实现 |
| `override bool Equals(object obj)` | Object.Equals覆写 |
| `override int GetHashCode()` | 哈希码生成 |
| `static bool operator ==(HexCoord a, HexCoord b)` | 相等运算符 |
| `static bool operator !=(HexCoord a, HexCoord b)` | 不等运算符 |

---

## 5. 封装设计

### 5.1 对外暴露

- 所有坐标计算和转换方法
- 相等性比较（支持作为Dictionary键）
- 运算符重载（+, -, ==, !=）

### 5.2 内部实现

- 轴向坐标验证（确保 x + y + z = 0）
- 缓存常用计算结果（邻接方向向量）

---

## 6. 依赖类

| 依赖类 | 依赖说明 |
|--------|----------|
| 无直接依赖 | 基础值类型，无外部依赖 |

---

## 7. 实现细节

### 7.1 邻接方向定义

```csharp
// 六边形的6个方向（从正上方开始顺时针）
private static readonly HexCoord[] Directions = new HexCoord[]
{
    new HexCoord(0, -1),   // 北 (0°)
    new HexCoord(1, -1),   // 东北 (60°)
    new HexCoord(1, 0),    // 东南 (120°)
    new HexCoord(0, 1),    // 南 (180°)
    new HexCoord(-1, 1),   // 西南 (240°)
    new HexCoord(-1, 0),   // 西北 (300°)
};
```

### 7.2 距离计算公式

```csharp
// 立方体坐标距离 = (|x1-x2| + |y1-y2| + |z1-z2|) / 2
public int DistanceTo(HexCoord other)
{
    return (Mathf.Abs(Q - other.Q) + Mathf.Abs(S - other.S) + Mathf.Abs(R - other.R)) / 2;
}
```

---

## 8. 使用示例

```csharp
// 创建坐标
var coord = new HexCoord(3, 4);

// 获取邻接坐标
var neighbors = coord.GetNeighbors();

// 计算距离
int dist = coord.DistanceTo(new HexCoord(0, 0)); // = 7

// 作为Dictionary键
var tileMap = new Dictionary<HexCoord, HexTile>();
tileMap[coord] = new HexTile(coord);

// 获取范围内的坐标
var rangeCoords = coord.GetSpiral(3); // 半径3范围内的所有坐标
```

---

## 9. 坐标系统图示

```
           ____
          /    \
    _____/ q=0  \_____
   / -1 \ r=-1  /  0  \
  /      \____/ -1    \
  \ q=-1 /    \ q=0   /
   \ r=0 / q=0 \ r=0  /
    \___/  r=0  \____/
    /    \____ /    \
   / q=-1 /    \ q=1 \
  /  r=1 / q=0  \ r=0/
  \_____/  r=1  \____/
        \____/
```

---

**文档版本**：v1.0  
**创建日期**：2026-04-28  
**关联文档**：HexTile_六边形地块.md, HexGrid_六边形网格管理.md
