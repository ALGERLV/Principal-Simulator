# HexGrid - 六边形网格管理

> 服从：01-项目级文档/技术栈描述.md  
> 服从：02-系统级文档/地图系统/地图系统设计.md

---

## 1. 类职责

`HexGrid` 是六边形网格的管理器，负责：
- 根据配置生成六边形网格
- 存储和管理所有HexTile实例
- 提供高效的坐标查询和范围查询
- 实现地图数据提供接口（IMapDataProvider）
- 支持地图形状（矩形、圆形、不规则）

---

## 2. 继承关系

```
UnityEngine.Object
    └── MonoBehaviour
            └── HexGrid
```

- 继承自 `MonoBehaviour` — 作为主场景管理器
- 实现 `IMapDataProvider` 接口 — 对外提供地图数据
- 实现 `ITerrainQuery` 接口 — 对外提供地形查询

---

## 3. 属性定义

### 3.1 私有字段

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `tiles` | `Dictionary<HexCoord, HexTile>` | 坐标到地块的映射表 |
| `tilePrefab` | `GameObject` | 地块预制体 |
| `mapWidth` | `int` | 地图宽度 |
| `mapHeight` | `int` | 地图高度 |
| `hexSize` | `float` | 六边形大小（外接圆半径） |
| `gridShape` | `GridShape` | 网格形状类型 |
| `defaultTerrain` | `TerrainData` | 默认地形数据 |
| `bounds` | `BoundsInt` | 地图边界范围 |

### 3.2 公有属性

| 属性名 | 类型 | 访问级别 | 说明 |
|--------|------|----------|------|
| `Width` | `int` | `public get` | 地图宽度 |
| `Height` | `int` | `public get` | 地图高度 |
| `TileCount` | `int` | `public get` | 地块总数 |
| `Bounds` | `BoundsInt` | `public get` | 地图边界 |
| `HexSize` | `float` | `public get` | 六边形大小 |
| `AllTiles` | `IEnumerable<HexTile>` | `public get` | 所有地块的枚举 |

---

## 4. 方法清单

### 4.1 生命周期方法

| 方法签名 | 说明 |
|----------|------|
| `void Awake()` | 初始化字典和状态 |
| `void Start()` | 如未初始化则自动生成地图 |
| `void OnDestroy()` | 清理资源 |

### 4.2 生成方法

| 方法签名 | 说明 |
|----------|------|
| `void Generate(MapConfig config)` | 根据配置生成完整地图 |
| `void GenerateRectangle(int width, int height)` | 生成矩形地图 |
| `void GenerateCircle(int radius)` | 生成圆形地图 |
| `void Clear()` | 清除所有地块 |

### 4.3 查询方法（IMapDataProvider）

| 方法签名 | 说明 |
|----------|------|
| `HexTile GetTile(HexCoord coord)` | 获取指定坐标的地块（可能返回null） |
| `bool TryGetTile(HexCoord coord, out HexTile tile)` | 安全获取地块 |
| `bool HasTile(HexCoord coord)` | 检查坐标是否存在地块 |
| `HexTile[] GetTilesInRange(HexCoord center, int range)` | 获取范围内的所有地块 |
| `HexTile[] GetTilesInRing(HexCoord center, int radius)` | 获取环上的地块 |
| `HexTile[] GetAllTiles()` | 获取所有地块数组 |
| `HexTile[] GetNeighbors(HexCoord coord)` | 获取邻接地块（只返回存在的） |
| `BoundsInt GetMapBounds()` | 获取地图边界（IMapDataProvider实现） |

### 4.4 地形查询（ITerrainQuery）

| 方法签名 | 说明 |
|----------|------|
| `float GetMovementCost(HexCoord coord)` | 获取移动消耗 |
| `float GetDefenseBonus(HexCoord coord)` | 获取防御加成 |
| `float GetVisibilityModifier(HexCoord coord)` | 获取视野修正 |
| `bool IsPassable(HexCoord coord)` | 检查是否可通过 |

### 4.5 编辑方法

| 方法签名 | 说明 |
|----------|------|
| `HexTile CreateTile(HexCoord coord, TerrainData terrain)` | 创建单个地块 |
| `void RemoveTile(HexCoord coord)` | 移除指定地块 |
| `void SetTerrain(HexCoord coord, TerrainData terrain)` | 修改地形 |
| `void LoadFromData(GridData data)` | 从数据加载地图 |
| `GridData SaveToData()` | 导出地图数据 |

### 4.6 工具方法

| 方法签名 | 说明 |
|----------|------|
| `Vector3 CoordToWorldPosition(HexCoord coord)` | 坐标转世界位置 |
| `HexCoord WorldPositionToCoord(Vector3 worldPos)` | 世界位置转坐标 |
| `HexCoord[] GetLine(HexCoord from, HexCoord to)` | 获取两点间直线路径 |

---

## 5. 封装设计

### 5.1 对外暴露

- IMapDataProvider 接口实现
- ITerrainQuery 接口实现
- 地图生成和编辑方法
- 坐标转换工具方法

### 5.2 内部实现

- 地块存储管理（Dictionary）
- 生成算法（矩形、圆形等形状）
- 缓存和优化

### 5.3 事件

| 事件名 | 签名 | 说明 |
|--------|------|------|
| `OnGridGenerated` | `Action` | 地图生成完成时触发 |
| `OnTileCreated` | `Action<HexTile>` | 地块创建时触发 |
| `OnTileRemoved` | `Action<HexCoord>` | 地块移除时触发 |

---

## 6. 依赖类

| 依赖类 | 依赖说明 |
|--------|----------|
| `HexCoord` | 坐标系统 |
| `HexTile` | 地块组件 |
| `TerrainData` | 地形数据 |
| `MapConfig` | 地图配置数据 |
| `IMapDataProvider` | 实现的数据提供接口 |
| `ITerrainQuery` | 实现的地形查询接口 |

---

## 7. 生成算法

### 7.1 矩形生成

```csharp
private void GenerateRectangle(int width, int height)
{
    for (int q = 0; q < width; q++)
    {
        for (int r = 0; r < height; r++)
        {
            // 轴向坐标偏移计算
            int rOffset = q / 2;
            var coord = new HexCoord(q, r - rOffset);
            CreateTile(coord, defaultTerrain);
        }
    }
}
```

### 7.2 圆形生成

```csharp
private void GenerateCircle(int radius)
{
    var center = new HexCoord(0, 0);
    for (int q = -radius; q <= radius; q++)
    {
        int r1 = Mathf.Max(-radius, -q - radius);
        int r2 = Mathf.Min(radius, -q + radius);
        for (int r = r1; r <= r2; r++)
        {
            var coord = new HexCoord(q, r);
            CreateTile(coord, defaultTerrain);
        }
    }
}
```

---

## 8. 使用示例

```csharp
// 获取HexGrid实例
var grid = FindObjectOfType<HexGrid>();

// 生成矩形地图
grid.GenerateRectangle(20, 15);

// 查询地块
var tile = grid.GetTile(new HexCoord(5, 3));

// 获取范围内地块
var tilesInRange = grid.GetTilesInRange(centerCoord, 3);

// 查询地形消耗
float cost = grid.GetMovementCost(targetCoord);

// 从配置生成
var config = new MapConfig
{
    width = 20,
    height = 15,
    shape = GridShape.Rectangle,
    defaultTerrain = plainTerrain
};
grid.Generate(config);
```

---

## 9. 性能优化

### 9.1 查询优化

- 使用 `Dictionary<HexCoord, HexTile>` 实现 O(1) 坐标查询
- 范围查询使用预计算的坐标列表

### 9.2 内存优化

- 地块预制体使用对象池（可选扩展）
- 大地图支持分块加载（可选扩展）

---

**文档版本**：v1.0  
**创建日期**：2026-04-28  
**关联文档**：HexCoord_六边形坐标系统.md, HexTile_六边形地块.md
