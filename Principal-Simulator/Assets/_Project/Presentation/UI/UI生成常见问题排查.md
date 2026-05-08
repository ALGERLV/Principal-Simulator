# UI 生成常见问题排查

## 前置要求

**所有 UI 生成脚本都要求场景中已存在 EventSystem。**

步骤：
1. 菜单 → GameObject → UI → Event System
2. 创建一次后，所有 UI Prefab 生成脚本都能正常工作

**注意：** UI 生成脚本 **不再自动创建 EventSystem**（已移除此逻辑）。原因是：
- EventSystem 应该在场景启动时就存在，而非由 Prefab 生成脚本创建
- 防止重复创建多个 EventSystem

---

## 问题 1：UI 按钮点击无响应

**症状：** 
- 按钮成功加载和显示
- 点击监听器已添加
- 但点击时完全没有反应

**根本原因：** 
场景中缺少 **EventSystem** 组件，导致 UI 事件系统无法工作。

**解决方案：**
菜单 → GameObject → UI → Event System

**预防方式：**
- 在开发任何 UI 前，先确保场景中有 EventSystem
- 参考 [前置要求](#前置要求) 部分

---

## 问题 2：按钮不可交互

**症状：** 
- 按钮显示但无法点击
- Console 显示 `Interactable: False`

**根本原因：** 
按钮在生成时 `interactable` 属性未设置为 true

**解决方案：**
在生成脚本中确保按钮设置正确：
```csharp
var button = buttonGO.AddComponent<Button>();
button.interactable = true;  // 必须显式设置
```

---

## 问题 3：Canvas 配置错误

**症状：** 
- 按钮位置显示错误
- 点击位置和显示位置不匹配

**根本原因：** 
Canvas 的 RenderMode 或 GraphicRaycaster 配置不正确

**必需配置：**
```csharp
var canvas = canvasGO.AddComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;  // 必须是 ScreenSpaceOverlay
canvasGO.AddComponent<GraphicRaycaster>();         // 必须有 GraphicRaycaster
```

---

## 完整检查清单

在生成任何 UI 时必须确保：

- [ ] **EventSystem 已在场景中存在**（菜单 → GameObject → UI → Event System）
- [ ] Canvas.renderMode = ScreenSpaceOverlay
- [ ] Canvas 有 GraphicRaycaster 组件
- [ ] Button.interactable = true
- [ ] Prefab 路径正确（Resources 下）
- [ ] 所有 UI 元素都是 Canvas 的子物体

---

## UI 生成脚本模板

创建新 UI 时，**不要** 调用 EnsureEventSystem()，代码应该看起来像这样：

```csharp
[UnityEditor.MenuItem("Tools/生成UI预制体/生成XXX")]
public static void GenerateXXXPrefab()
{
    // 直接开始生成逻辑，无需创建 EventSystem
    string prefabPath = "Assets/Resources/Prefabs/UI/XXX/XXX.prefab";
    // ... 生成逻辑 ...
}
```

---

**最后更新：** 2026-05-07  
**相关文件：** 
- BattleHUDPrefabGenerator.cs
- SpawnPanelPrefabGenerator.cs
- MainMenuPrefabGenerator.cs

