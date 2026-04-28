# BoardCameraController - 棋盘相机控制器

> **命名空间**: `TBS.Presentation.Camera`
> **文件路径**: `Assets/_Project/Presentation/Camera/BoardCameraController.cs`

---

## 1. 功能概述

棋盘相机控制器是一个专为 2.5D 战棋游戏设计的相机系统，提供俯视棋盘的视角控制，支持以下功能：

- **俯视视角**: 可调节的俯仰角（30°-85°）
- **相机移动**: 鼠标拖拽、键盘控制、边缘滚动
- **缩放控制**: 滚轮缩放，平滑过渡
- **视角旋转**: 水平 360° 旋转
- **边界限制**: 自动限制在地图范围内

---

## 2. 使用方法

### 2.1 快速设置（Unity 编辑器）

1. **菜单设置**: `TBS > Camera > Setup Board Camera`
2. **场景右键**: `GameObject > TBS > Camera > Board Camera`

### 2.2 代码设置

```csharp
// 方法1: 自动创建相机
var controller = BoardCameraSetup.CreateBoardCamera(hexGrid);

// 方法2: 使用配置创建
var config = Resources.Load<BoardCameraConfig>("Camera/DefaultConfig");
var controller = BoardCameraSetup.CreateBoardCameraWithConfig(config, hexGrid);

// 方法3: 转换现有相机
var controller = BoardCameraSetup.ConvertExistingCamera(Camera.main, hexGrid);
```

---

## 3. 操作说明

| 操作 | 输入方式 |
|------|----------|
| 平移视角 | 右键拖拽 |
| 键盘移动 | WASD / 方向键 |
| 缩放 | 鼠标滚轮 |
| 旋转 | Q / E |
| 边缘滚动 | 鼠标移向屏幕边缘 |

---

## 4. 主要接口

### 4.1 公共属性

```csharp
// 当前缩放级别 (0-1)
public float ZoomLevel { get; }

// 当前俯仰角
public float CurrentPitch { get; }

// 当前水平旋转角
public float CurrentYaw { get; }
```

### 4.2 控制方法

```csharp
// 聚焦到指定坐标
void FocusOnCoord(HexCoord coord);

// 聚焦到地图中心
void FocusOnCenter();

// 设置缩放级别 (0-1)
void SetZoomLevel(float normalizedZoom);

// 设置旋转角度
void SetRotation(float pitch, float yaw);

// 重置相机
void ResetCamera();

// 获取视野内的地块
HexTile[] GetVisibleTiles();
```

### 4.3 事件

```csharp
// 相机移动时触发
event Action OnCameraMoved;

// 缩放变化时触发
event Action<float> OnZoomChanged;

// 旋转变化时触发
event Action OnRotationChanged;
```

---

## 5. 配置参数

| 参数 | 说明 | 默认值 |
|------|------|--------|
| `moveSpeed` | 键盘移动速度 | 5 |
| `dragSensitivity` | 拖拽灵敏度 | 0.5 |
| `zoomSpeed` | 缩放速度 | 5 |
| `minZoomHeight` | 最小高度（最近） | 3 |
| `maxZoomHeight` | 最大高度（最远） | 20 |
| `minPitchAngle` | 最小俯仰角 | 30° |
| `maxPitchAngle` | 最大俯仰角 | 85° |
| `initialPitch` | 初始俯仰角 | 60° |
| `initialZoom` | 初始缩放 (0-1) | 0.5 |
| `limitToMapBounds` | 限制在地图边界 | true |

---

## 6. 依赖关系

### 6.1 依赖的模块

| 模块 | 接口/类 | 用途 |
|------|---------|------|
| 地图系统 | `HexGrid` | 获取地图边界和坐标转换 |
| 地图系统 | `HexCoord` | 聚焦指定坐标 |
| 地图系统 | `HexTile` | 获取可见地块 |

### 6.2 被依赖关系

| 依赖方 | 用途 |
|--------|------|
| `CameraHUD` | 显示相机状态 |
| `CameraDemo` | 演示相机功能 |

---

## 7. 架构设计

```
BoardCameraController
    ├── 输入处理
    │   ├── 鼠标拖拽 → 平移
    │   ├── 键盘 → 平移
    │   ├── 滚轮 → 缩放
    │   └── Q/E → 旋转
    ├── 位置更新
    │   ├── 球坐标计算
    │   └── 平滑插值
    └── 边界限制
        └── 地图范围检测
```

---

## 8. 扩展指南

### 8.1 自定义输入

继承 `BoardCameraController` 并重写输入处理方法：

```csharp
public class CustomCameraController : BoardCameraController
{
    protected override void HandleKeyboardInput()
    {
        // 自定义键盘控制
        base.HandleKeyboardInput();
    }
}
```

### 8.2 添加手势支持

```csharp
private void HandleTouchInput()
{
    if (Input.touchCount == 2)
    {
        // 双指缩放
    }
    else if (Input.touchCount == 1)
    {
        // 单指拖拽
    }
}
```

---

**创建日期**: 2026-04-28
**版本**: v1.0
**作者**: AI Assistant
