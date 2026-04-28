# HexTile - 六边形地块

> 服从：01-项目级文档/技术栈描述.md  
> 服从：02-系统级文档/地图系统/地图系统设计.md

---

## 1. 类职责

`HexTile` 表示六边形网格中的一个地块，负责：
- 存储地块的坐标位置
- 存储地形类型及其属性
- 管理地块上的单位引用
- 提供地块状态查询
- 实现地形效果的计算接口

---

## 2. 继承关系

```
UnityEngine.Object
    └── MonoBehaviour
            └── HexTile
```

- 继承自 `MonoBehaviour` — 可以挂载到GameObject
- 实现 `ITerrainEffect` 接口 — 提供地形效果查询

---

## 3. 属性定义

### 3.1 私有字段

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `coord` | `HexCoord` | 地块坐标（序列化存储） |
| `terrainData` | `TerrainData` | 地形数据引用（ScriptableObject） |
| `occupyingUnit` | `IUnit` | 占据此地块的单位（可空） |
| `visibilityState` | `VisibilityState` | 可见性状态 |

### 3.2 公有属性

| 属性名 | 类型 | 访问级别 | 说明 |
|--------|------|----------|------|
| `Coord` | `HexCoord` | `public get` | 地块坐标 |
| `TerrainId` | `string` | `public get` | 地形类型ID |
| `TerrainName` | `string` | `public get` | 地形显示名称 |
| `MovementCost` | `float` | `public get` | 移动消耗（来自ITerrainEffect） |
| `DefenseBonus` | `float` | `public get` | 防御加成（来自ITerrainEffect） |
| `VisibilityModifier` | `float` | `public get` | 视野修正（来自ITerrainEffect） |
| `IsPassable` | `bool` | `public get` | 是否可通过 |
| `OccupyingUnit` | `IUnit` | `public get/set` | 占据此地块的单位 |
| `IsOccupied` | `bool` | `public get` | 是否被单位占据 |

---

## 4. 方法清单

### 4.1 初始化方法

| 方法签名 | 说明 |
|----------|------|
| `void Initialize(HexCoord coord, TerrainData terrain)` | 初始化地块（坐标和地形） |
| `void OnValidate()` | Unity验证回调，检查数据完整性 |

### 4.2 单位管理

| 方法签名 | 说明 |
|----------|------|
| `void SetOccupyingUnit(IUnit unit)` | 设置占据单位 |
| `void ClearOccupyingUnit()` | 清除占据单位 |
| `bool CanEnter(IUnit unit)` | 检查单位是否可以进入 |

### 4.3 地形效果接口（ITerrainEffect）

| 方法签名 | 说明 |
|----------|------|
| `float GetMovementCost(IUnit unit)` | 获取指定单位的移动消耗（可考虑单位特性） |
| `float GetDefenseBonus()` | 获取防御加成 |
| `float GetVisibilityModifier()` | 获取视野修正 |

### 4.4 状态查询

| 方法签名 | 说明 |
|----------|------|
| `bool HasTerrainFeature(string featureId)` | 检查是否拥有特定地形特性 |
| `T GetTerrainProperty<T>(string propertyName)` | 获取地形属性值 |

---

## 5. 封装设计

### 5.1 对外暴露

- 坐标和地形基础属性
- 占据单位信息
- ITerrainEffect接口实现
- 状态查询方法

### 5.2 内部实现

- 地形数据引用管理
- 可见性状态内部维护
- 事件通知（当单位进入/离开）

### 5.3 事件

| 事件名 | 签名 | 说明 |
|--------|------|------|
| `OnUnitEntered` | `Action<IUnit>` | 单位进入时触发 |
| `OnUnitExited` | `Action<IUnit>` | 单位离开时触发 |

---

## 6. 依赖类

| 依赖类 | 依赖说明 |
|--------|----------|
| `HexCoord` | 坐标位置 |
| `ITerrainEffect` | 实现的地形效果接口 |
| `IUnit` | 占据单位接口（来自单位系统） |
| `TerrainData` | 地形数据ScriptableObject |

---

## 7. 数据配置

### 7.1 TerrainData ScriptableObject

```csharp
[CreateAssetMenu(fileName = "TerrainData", menuName = "Game/Terrain Data")]
public class TerrainData : ScriptableObject, ITerrainEffect
{
    [SerializeField] private string terrainId;
    [SerializeField] private string terrainName;
    [SerializeField] private float movementCost = 1f;
    [SerializeField] private float defenseBonus = 0f;
    [SerializeField] private float visibilityModifier = 0f;
    [SerializeField] private bool isPassable = true;
    [SerializeField] private List<string> features = new List<string>();
    
    // ITerrainEffect 实现...
}
```

### 7.2 地形类型预设

| 地形ID | 移动消耗 | 防御加成 | 视野修正 | 备注 |
|--------|----------|----------|----------|------|
| plain | 1.0 | 0.0 | 0.0 | 平原，默认地形 |
| forest | 1.5 | 0.1 | -0.3 | 森林 |
| mountain | 2.0 | 0.3 | 0.5 | 山地 |
| river | 2.0 | 0.0 | 0.0 | 河流（可配置阻挡） |
| road | 0.5 | 0.0 | 0.0 | 道路 |
| swamp | 2.5 | -0.1 | -0.2 | 沼泽 |

---

## 8. 使用示例

```csharp
// 创建地块（通常在HexGrid中完成）
var tile = gameObject.AddComponent<HexTile>();
tile.Initialize(new HexCoord(3, 4), plainTerrainData);

// 查询地形信息
float cost = tile.MovementCost;
float defense = tile.DefenseBonus;

// 检查单位移动
if (tile.CanEnter(myUnit))
{
    tile.SetOccupyingUnit(myUnit);
}

// 订阅事件
tile.OnUnitEntered += (unit) => Debug.Log($"{unit.Name} entered tile at {tile.Coord}");
```

---

**文档版本**：v1.0  
**创建日期**：2026-04-28  
**关联文档**：HexCoord_六边形坐标系统.md, HexGrid_六边形网格管理.md, ITerrainEffect_地形效果接口.md
