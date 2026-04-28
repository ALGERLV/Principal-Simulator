# 地图系统测试说明

## 快速开始

### 方法一：使用编辑器窗口（推荐）

1. 打开Unity编辑器
2. 打开 `Tools > Map System > Test` 菜单
3. 点击"创建HexGrid"按钮创建网格管理器
4. 拖拽 `Assets/Prefabs/Map/HexTile` 到 Tile预制体字段
5. （可选）创建地形数据资源并拖拽到默认地形字段
6. 点击"生成矩形地图"或"生成圆形地图"

### 方法二：使用测试场景

1. 打开场景 `Assets/Scenes/TestScene/TestScene`
2. 场景中已经包含 `MapSystemTest` 和 `HexGrid` 对象
3. 为MapSystemTest组件设置引用：
   - Tile预制体: `Assets/Prefabs/Map/HexTile`
   - （可选）默认地形数据
4. 点击Play运行，地图将自动生成
5. 查看Console窗口的测试输出

## 功能测试

### 自动测试项目

运行时会自动执行以下测试：

1. **坐标系统测试**
   - 坐标创建与比较
   - 邻接坐标计算
   - 距离计算
   - 范围查询

2. **地图查询测试**
   - 获取地块
   - 获取邻接地块
   - 地图边界查询
   - 地块总数统计

3. **地形效果测试**
   - 地形名称
   - 移动消耗
   - 防御加成
   - 视野修正

4. **范围查询测试**
   - 不同半径的范围查询
   - 环形查询

### 手动测试

在Inspector中右键点击MapSystemTest组件：

- **高亮测试** - 高亮中心点周围半径2范围内的地块
- **清除高亮** - 恢复所有地块的默认颜色

## 创建地形数据

### 方法一：使用菜单

1. `Assets > Create > Game > Terrain Presets` 选择预设
   - Plain (平原)
   - Forest (森林)
   - Mountain (山地)
   - River (河流)
   - Road (道路)
   - Swamp (沼泽)

### 方法二：手动创建

1. `Assets > Create > Game > Terrain Data`
2. 设置地形属性：
   - Terrain Id: 唯一标识（如"forest"）
   - Terrain Name: 显示名称
   - Movement Cost: 移动消耗（1.0为基准）
   - Defense Bonus: 防御加成（-0.5 ~ 1.0）
   - Visibility Modifier: 视野修正
   - Terrain Color: 地块颜色

## 验证检查清单

- [ ] HexGrid对象已创建
- [ ] Tile预制体已设置
- [ ] （可选）地形数据已创建
- [ ] 运行后能生成六边形地块
- [ ] 地块显示不同颜色（对应不同地形）
- [ ] Console显示测试通过信息
- [ ] 地图查询功能正常

## 常见问题

### Q: 地块不显示或位置错误
A: 检查HexTile预制体的HexTileRenderer组件是否正确配置，MeshFilter是否有网格数据。

### Q: 地形颜色不生效
A: 确保TerrainData的TerrainColor已设置，且HexTileRenderer已添加到预制体。

### Q: 范围查询结果不正确
A: 坐标系统使用轴向坐标（Axial Coordinates），确保查询中心点坐标正确。

## 测试输出示例

```
========== 地图系统测试开始 ==========
[测试1] 坐标系统测试
  坐标创建: HexCoord(3, 4) = HexCoord(3, 4) ? True
  邻接坐标数量: 6 (预期: 6)
  距离计算: (0,0) 到 (3,4) = 7
  半径2范围内坐标数: 19
[测试2] 地图查询测试
  中心地块: HexTile[HexCoord(0, 0)] - 平原 (Unit: None)
  中心邻接地块数: 6
  地图边界: (x:0, y:0, z:0, sizeX:10, sizeY:1, sizeZ:8)
  地块总数: 80
[测试3] 地形效果测试
  地块 HexCoord(0, 0):
    地形: 平原
    移动消耗: 1
    防御加成: 0
    视野修正: 0
[测试4] 范围查询测试
  范围1: 7个地块
  范围2: 19个地块
  范围3: 37个地块
  半径2的环: 12个地块
========== 地图系统测试完成 ==========
```

## 文件清单

| 文件 | 路径 | 说明 |
|------|------|------|
| MapSystemTest.cs | Scripts/Map/ | 测试主组件 |
| MapTestEditor.cs | Scripts/Map/Editor/ | 编辑器工具 |
| HexTile.prefab | Prefabs/Map/ | 地块预制体 |
| TerrainData.asset | Resources/Terrain/ | 地形数据（需创建） |
