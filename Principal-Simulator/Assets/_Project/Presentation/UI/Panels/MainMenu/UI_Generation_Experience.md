# MainMenu UI 生成经验总结

## 本次生成遇到的问题与解决方案

### 问题1：CS0122 编译错误 - 访问权限不足
**错误信息**：
```
error CS0122: 'BaseView<MainMenuViewModel>._uiId' is inaccessible due to its protection level
```

**原因**：
- BaseView中的`_uiId`是私有字段，Prefab生成脚本无法直接访问

**解决方案**：
- 在BaseView中添加公共方法`SetUIId(string uiId)`
- 在MainMenuView中添加公共方法`SetUIElements(Text, Button, Button)`
- Prefab生成脚本通过这些公共方法设置值，而不是直接访问私有字段

**代码范例**：
```csharp
// BaseView.cs
public void SetUIId(string uiId)
{
    _uiId = uiId;
}

// MainMenuView.cs
public void SetUIElements(Text titleText, Button startButton, Button exitButton)
{
    _titleText = titleText;
    _startButton = startButton;
    _exitButton = exitButton;
}
```

**预防措施**：
- ✅ 保持UI元素为private
- ✅ 通过public方法暴露设置接口
- ✅ 编辑器脚本永远不要直接访问private字段

---

### 问题2：字体废弃警告 - Arial.ttf 无效
**错误信息**：
```
生成Prefab失败: Arial.ttf is no longer a valid built in font. Please use LegacyRuntime.ttf
```

**原因**：
- Unity 2022.3 LTS版本已移除对Arial.ttf的支持
- 新版本需要使用LegacyRuntime.ttf作为默认字体

**解决方案**：
- 将所有`Resources.GetBuiltinResource<Font>("Arial.ttf")`改为
- `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`

**文件修改位置**：
- MainMenuPrefabGenerator.cs - 标题文本字体
- MainMenuPrefabGenerator.cs - 开始按钮文本字体
- MainMenuPrefabGenerator.cs - 退出按钮文本字体

**预防措施**：
- ✅ 检查项目Unity版本的内置字体支持
- ✅ 使用LegacyRuntime.ttf而不是Arial.ttf
- ✅ 建立字体常量避免重复：
```csharp
private const string BUILTIN_FONT = "LegacyRuntime.ttf";
titleText.font = Resources.GetBuiltinResource<Font>(BUILTIN_FONT);
```

---

### 问题3：Prefab生成后留在场景中
**症状**：
- 运行生成脚本后，Hierarchy中仍然显示MainMenu GameObject
- 保存场景时会把生成的临时对象保存进去

**原因**：
- `new GameObject()`会自动添加到当前活跃场景中
- 没有使用HideFlags防止显示和保存

**解决方案**：
```csharp
// 防止在Hierarchy中显示和保存到场景
canvasGO = new GameObject("MainMenu");
canvasGO.hideFlags = HideFlags.HideAndDontSave;

// 保存前清除标记
canvasGO.hideFlags = HideFlags.None;
UnityEditor.PrefabUtility.SaveAsPrefabAsset(canvasGO, assetPath);

// try-finally确保一定会清理
finally
{
    if (canvasGO != null)
    {
        DestroyImmediate(canvasGO);
    }
}
```

**预防措施**：
- ✅ 使用HideFlags.HideAndDontSave保护临时GameObject
- ✅ 使用try-finally确保清理
- ✅ 异常处理，确保即使出错也会销毁临时对象

---

### 问题4：Resources文件夹结构缺失
**症状**：
- UIManager无法通过Resources.Load加载Prefab
- 提示找不到资源文件

**原因**：
- Resources.Load()加载的Prefab必须在Assets/Resources/目录下
- 初次生成脚本是生成到Assets/Prefabs/目录
- 需要额外移动到Assets/Resources/目录

**解决方案**（已改进）：
- 改进的生成脚本直接生成到`Assets/Resources/Prefabs/UI/MainMenu/`
- 避免了后续的移动操作

```csharp
// 直接生成到Resources目录
string prefabPath = "Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab";
```

**预防措施**：
- ✅ Prefab生成脚本直接输出到Resources文件夹
- ✅ 无需手动移动文件
- ✅ 生成后立即可用于UIManager.Load

---

## 快速检查清单

### 生成Prefab前检查
- [ ] 项目Unity版本是否为2022.3 LTS或更高版本
- [ ] 项目中是否存在UIManager并正确初始化
- [ ] MainMenuView、MainMenuPresenter、MainMenuViewModel是否编译无误
- [ ] BaseView中是否有SetUIId和SetUIElements方法

### 生成Prefab时检查
- [ ] 执行菜单：Tools → 生成UI预制体 → 生成MainMenu
- [ ] 是否有弹窗提示成功生成
- [ ] 检查生成路径是否为 `Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab`
- [ ] Hierarchy中是否为空（不应该有MainMenu GameObject）

### 生成完成后检查
- [ ] 检查Prefab文件是否存在
- [ ] 打开Prefab预览，确认：
  - 背景显示正常（深灰色）
  - 标题文本"电子战棋"显示正常
  - 两个按钮都存在（蓝色开始按钮、红色退出按钮）
- [ ] 场景文件未被修改（.unity文件git状态不变）

---

## UI生成规范

### 文件组织结构
```
Assets/
├── Resources/Prefabs/UI/MainMenu/
│   └── MainMenu.prefab              ← UIManager读取的文件
│
└── _Project/Presentation/
    ├── UI/
    │   ├── Core/                    ← 框架核心
    │   │   ├── BaseView.cs
    │   │   ├── BasePresenter.cs
    │   │   ├── UIManager.cs
    │   │   └── ...
    │   └── Panels/MainMenu/         ← UI实现
    │       ├── MainMenuView.cs
    │       ├── MainMenuPresenter.cs
    │       ├── MainMenuViewModel.cs
    │       └── MainMenuPrefabGenerator.cs
    └── UIInitializer.cs
```

### Prefab生成脚本规范

每个UI的Prefab生成脚本应该遵循以下规范：

```csharp
#if UNITY_EDITOR
[UnityEditor.MenuItem("Tools/生成UI预制体/生成XXX")]
public static void GenerateXXXPrefab()
{
    string prefabPath = "Assets/Resources/Prefabs/UI/XXX/XXX.prefab";
    
    GameObject xxxGO = null;
    try
    {
        // 创建游戏对象
        xxxGO = new GameObject("XXX");
        xxxGO.hideFlags = HideFlags.HideAndDontSave;
        
        // 构建UI结构
        // ...
        
        // 添加View组件并关联元素
        var view = xxxGO.AddComponent<XXXView>();
        view.SetUIElements(...);
        
        // 保存Prefab
        xxxGO.hideFlags = HideFlags.None;
        string assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(prefabPath);
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(xxxGO, assetPath);
        
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.EditorUtility.DisplayDialog("成功", $"Prefab已生成到: {assetPath}", "确定");
    }
    catch (System.Exception ex)
    {
        UnityEditor.EditorUtility.DisplayDialog("错误", $"生成失败: {ex.Message}", "确定");
    }
    finally
    {
        if (xxxGO != null) DestroyImmediate(xxxGO);
    }
}
#endif
```

### MVP组件规范

**ViewModel**：
```csharp
public class XXXViewModel : ViewModelBase
{
    // 属性和命令定义
    // 无UI逻辑
}
```

**Presenter**：
```csharp
public class XXXPresenter : BasePresenter<XXXView, XXXViewModel>
{
    protected override void OnInitialize()
    {
        // 绑定事件
    }
    
    // 事件处理方法
}
```

**View**：
```csharp
public class XXXView : BaseView<XXXViewModel>
{
    [SerializeField] private Button _button;
    
    protected override IPresenter CreatePresenter()
    {
        return new XXXPresenter();
    }
    
    protected override void OnBind()
    {
        // 绑定UI和VM
    }
    
    public void SetUIElements(Button button)
    {
        _button = button;
    }
}
```

---

## 常见错误速查表

| 错误信息 | 原因 | 解决方案 |
|---------|------|--------|
| CS0122 - 无访问权限 | 直接访问private字段 | 添加public方法 |
| Arial.ttf无效 | 字体版本不支持 | 改为LegacyRuntime.ttf |
| 生成后在Hierarchy中显示 | 缺少HideFlags | 添加`hideFlags = HideFlags.HideAndDontSave` |
| Resources.Load返回null | Prefab不在Resources目录 | 改用`Assets/Resources/`路径生成 |
| 按钮无响应 | EventSystem缺失 | 确保Canvas存在EventSystem |
| 文本显示为空 | 字体加载失败 | 检查字体资源是否正确 |

---

## 后续改进方向

1. **自动验证**：生成后自动检查Prefab结构是否完整
2. **通用模板**：创建通用的UI生成模板，减少重复代码
3. **自动测试**：添加编辑器测试验证生成的Prefab是否正常工作
4. **配置化**：将UI配置提取为ScriptableObject，减少硬编码

---

**文档版本**: v1.0  
**创建日期**: 2026-05-07  
**最后更新**: 2026-05-07
