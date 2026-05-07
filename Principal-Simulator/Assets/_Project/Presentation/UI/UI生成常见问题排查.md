# UI 生成常见问题排查

## 问题 1：UI 按钮点击无响应

**症状：** 
- 按钮成功加载和显示
- 点击监听器已添加
- 但点击时完全没有反应

**根本原因：** 
场景中缺少 **EventSystem** 组件，导致 UI 事件系统无法工作。

**解决方案：**
1. 自动方案（推荐）：运行 `Tools → 生成UI预制体 → 生成MainMenu` 时会自动创建 EventSystem
2. 手动方案：菜单 → GameObject → UI → Event System

**预防方式：**
- 所有 UI 生成脚本都应该在 `GenerateMainMenuPrefab()` 开始时调用 `EnsureEventSystem()`
- 参考：[MainMenuPrefabGenerator.cs](../MainMenuPrefabGenerator.cs) 第 14-30 行

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

- [ ] EventSystem 存在（自动检查：`EnsureEventSystem()`）
- [ ] Canvas.renderMode = ScreenSpaceOverlay
- [ ] Canvas 有 GraphicRaycaster 组件
- [ ] Button.interactable = true
- [ ] Prefab 路径正确（Resources 下）
- [ ] 所有 UI 元素都是 Canvas 的子物体

---

## 代码片段（复制到新的 UI 生成脚本）

```csharp
[UnityEditor.MenuItem("Tools/生成UI预制体/生成XXX")]
public static void GenerateXXXPrefab()
{
    // 第一步：确保 EventSystem 存在
    EnsureEventSystem();

    // 后续生成逻辑...
}

private static void EnsureEventSystem()
{
    var eventSystem = UnityEngine.EventSystems.EventSystem.current;
    if (eventSystem != null) return;

    var eventSystemGO = new GameObject("EventSystem");
    eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
    eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    Debug.Log("[UIGenerator] 自动创建 EventSystem");
}
```

---

**最后更新：** 2026-05-07  
**相关文件：** 
- MainMenuPrefabGenerator.cs
- MainMenuView.cs
- UIManager.cs
