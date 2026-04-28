# ITerrainEffect - 地形效果接口

> 服从：01-项目级文档/技术栈描述.md  
> 服从：02-系统级文档/地图系统/地图系统设计.md

---

## 1. 类职责

`ITerrainEffect` 是地形效果的**契约接口（Contract）**，定义在契约层（TBS.Contracts），负责：
- 定义地形效果的标准接口
- 提供移动消耗、防御加成、视野修正的查询方法
- 支持通过模组扩展自定义地形效果
- 实现策略模式，允许不同的地形效果实现

---

## 2. 继承关系

```
ITerrainEffect (interface)
    ├── 默认实现：TerrainData (ScriptableObject)
    └── 可扩展：自定义地形效果实现
```

- 定义在 `TBS.Contracts` 命名空间
- 由 `TerrainData` 提供默认实现
- 可由模组系统扩展自定义实现

---

## 3. 接口定义

### 3.1 基础属性

| 属性签名 | 返回类型 | 说明 |
|----------|----------|------|
| `TerrainId { get; }` | `string` | 地形类型唯一标识 |
| `TerrainName { get; }` | `string` | 地形显示名称 |

### 3.2 效果方法

| 方法签名 | 返回类型 | 说明 |
|----------|----------|------|
| `GetMovementCost(IUnit unit = null)` | `float` | 获取移动消耗（单位可选，用于特性计算） |
| `GetDefenseBonus()` | `float` | 获取防御加成百分比（0.0 ~ 1.0） |
| `GetVisibilityModifier()` | `float` | 获取视野修正（正值增加视野，负值减少） |
| `IsPassable(IUnit unit = null)` | `bool` | 检查是否可通过（单位可选） |

### 3.3 扩展方法（可选实现）

| 方法签名 | 返回类型 | 说明 |
|----------|----------|------|
| `GetProperty<T>(string key)` | `T` | 获取扩展属性值 |
| `HasFeature(string featureId)` | `bool` | 检查是否拥有特定特性 |

---

## 4. 默认实现

### 4.1 TerrainData 实现

```csharp
namespace TBS.DefaultImpl
{
    [CreateAssetMenu(fileName = "TerrainData", menuName = "Game/Terrain Data")]
    public class TerrainData : ScriptableObject, ITerrainEffect
    {
        [SerializeField] private string terrainId;
        [SerializeField] private string terrainName;
        [SerializeField] private float movementCost = 1f;
        [SerializeField] private float defenseBonus = 0f;
        [SerializeField] private float visibilityModifier = 0f;
        [SerializeField] private bool isPassable = true;
        [SerializeField] private SerializedDictionary<string, float> properties;
        
        public string TerrainId => terrainId;
        public string TerrainName => terrainName;
        
        public float GetMovementCost(IUnit unit = null)
        {
            // 基础消耗
            float cost = movementCost;
            
            // 如有单位，计算单位特性影响
            if (unit != null)
            {
                cost = unit.ApplyTerrainMovementModifier(cost, terrainId);
            }
            
            return cost;
        }
        
        public float GetDefenseBonus() => defenseBonus;
        
        public float GetVisibilityModifier() => visibilityModifier;
        
        public bool IsPassable(IUnit unit = null)
        {
            if (!isPassable) return false;
            if (unit != null)
            {
                return unit.CanTraverseTerrain(terrainId);
            }
            return true;
        }
        
        public T GetProperty<T>(string key) { /* 实现... */ }
        
        public bool HasFeature(string featureId) { /* 实现... */ }
    }
}
```

---

## 5. 封装设计

### 5.1 对外暴露

- 所有接口方法对外可访问
- 接口定义稳定，少变更

### 5.2 实现规范

- 所有地形效果类必须实现本接口
- 默认实现使用ScriptableObject便于配置
- 自定义实现可通过模组系统注册

---

## 6. 依赖类

| 依赖类 | 依赖说明 |
|--------|----------|
| `IUnit` | 单位接口（移动消耗计算需要） |

---

## 7. 使用场景

### 7.1 移动系统使用

```csharp
// 计算移动消耗
ITerrainEffect terrain = tile.GetTerrainEffect();
float cost = terrain.GetMovementCost(movingUnit);
```

### 7.2 战斗系统使用

```csharp
// 获取防御加成
ITerrainEffect terrain = defenderTile.GetTerrainEffect();
float defenseBonus = terrain.GetDefenseBonus();
damage *= (1 - defenseBonus);
```

### 7.3 视野系统使用

```csharp
// 获取视野修正
ITerrainEffect terrain = tile.GetTerrainEffect();
float viewRange = baseViewRange + terrain.GetVisibilityModifier();
```

---

## 8. 扩展示例

### 8.1 自定义地形效果（模组）

```csharp
public class SwampTerrainEffect : ITerrainEffect
{
    public string TerrainId => "custom_swamp";
    public string TerrainName => "沼泽地";
    
    public float GetMovementCost(IUnit unit = null)
    {
        // 机械化单位在沼泽移动极慢
        if (unit?.UnitType == UnitType.Mechanized)
            return 5f;
        return 2.5f;
    }
    
    public float GetDefenseBonus() => -0.1f; // 防御劣势
    
    public float GetVisibilityModifier() => -0.2f;
    
    public bool IsPassable(IUnit unit = null) => true;
}
```

---

## 9. 地形效果汇总

| 地形类型 | 移动消耗 | 防御加成 | 视野修正 | 特殊效果 |
|----------|----------|----------|----------|----------|
| 平原 | 1.0 | 0.0 | 0.0 | 无 |
| 森林 | 1.5 | 0.1 | -0.3 | 隐藏单位 |
| 山地 | 2.0 | 0.3 | 0.5 | 制高点 |
| 河流 | 2.0 | 0.0 | 0.0 | 可配置阻挡 |
| 道路 | 0.5 | 0.0 | 0.0 | 快速移动 |
| 沼泽 | 2.5 | -0.1 | -0.2 | 机械化惩罚 |
| 城市 | 1.0 | 0.2 | -0.1 | 补给点 |
| 桥梁 | 1.0 | -0.2 | 0.0 | 战略要地 |

---

## 10. 架构定位

```
契约层 (TBS.Contracts)
    └── ITerrainEffect
            │
            ├── 默认实现层 (TBS.DefaultImpl)
            │       └── TerrainData (ScriptableObject)
            │
            └── 模组扩展层
                    └── 自定义地形效果
```

---

**文档版本**：v1.0  
**创建日期**：2026-04-28  
**关联文档**：HexTile_六边形地块.md, 地图系统设计.md
