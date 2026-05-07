# MainMenu UI 实现指南

## 文件结构

```
Assets/
├── _Project/Presentation/
│   ├── UI/
│   │   ├── Core/                 # UI框架核心（已存在）
│   │   └── Panels/
│   │       └── MainMenu/
│   │           ├── MainMenuView.cs          # 视图层
│   │           ├── MainMenuPresenter.cs     # 调度层
│   │           ├── MainMenuViewModel.cs     # 数据层
│   │           └── MainMenuPrefabGenerator.cs # Prefab生成工具
│   ├── UIInitializer.cs          # UI系统初始化
│   └── MainMenuExample.cs        # 使用示例
└── Prefabs/UI/MainMenu/
    └── MainMenu.prefab           # UI预制体（自动生成）
```

## 使用步骤

### 1. 生成Prefab

在Unity编辑器中：
1. 点击菜单栏 `Tools` → `生成UI预制体` → `生成MainMenu`
2. 脚本会自动创建完整的UI界面并保存为Prefab
3. 确保 `Assets/Prefabs/UI/MainMenu/` 目录存在（脚本会自动创建）

### 2. 配置场景

1. 在游戏启动场景中创建一个空的GameObject
2. 添加 `UIInitializer` 脚本到该GameObject
3. UIInitializer会自动初始化UIManager和所有UI配置

### 3. 显示MainMenu

**方式一：在启动脚本中显示**

```csharp
using TBS.Presentation.UI;
using TBS.Presentation.UI.Panels.MainMenu;

public class GameStartup : MonoBehaviour
{
    private void Start()
    {
        // 创建并显示MainMenu
        UIManager.Instance.Create<MainMenuView>();
        UIManager.Instance.Show<MainMenuView>();
    }
}
```

**方式二：通过UIId显示**

```csharp
UIManager.Instance.Create("MainMenuView");
UIManager.Instance.Show("MainMenuView");
```

### 4. 处理按钮事件

MainMenuPresenter已包含两个按钮的事件处理：

- **开始游戏**：调用ViewModel的StartGameCommand
- **退出游戏**：关闭应用或返回菜单

你可以在Presenter中的 `OnStartClicked()` 和 `OnExitClicked()` 方法中添加具体逻辑。

## MVP架构说明

### MainMenuViewModel
- **职责**：存储UI所需的数据状态
- **字段**：Title（游戏标题）
- **命令**：StartGameCommand、ExitGameCommand

### MainMenuPresenter
- **职责**：处理业务逻辑和事件分发
- **关键方法**：
  - `OnInitialize()`：初始化时绑定按钮事件
  - `OnStartClicked()`：处理开始游戏逻辑
  - `OnExitClicked()`：处理退出游戏逻辑
  - `OnShow()`：UI显示时调用
  - `OnHide()`：UI隐藏时调用

### MainMenuView
- **职责**：UI视图层，管理UI组件和用户交互
- **绑定**：
  - Title文本自动同步ViewModel中的Title属性
  - 按钮事件绑定到Presenter方法

## 扩展建议

### 1. 添加事件通知

修改Presenter中的 `OnStartClicked()` 方法，发送事件到GamePlay层：

```csharp
private void OnStartClicked()
{
    ViewModel.StartGameCommand.Execute();
    EventBus.Emit("GameStart"); // 或使用你的事件系统
}
```

### 2. 添加动画效果

在MainMenuView中重写显示/隐藏方法：

```csharp
public override void OnShow()
{
    base.OnShow();
    // 添加显示动画
    gameObject.GetComponent<CanvasGroup>().DOFade(1, 0.3f);
}

public override void OnHide()
{
    base.OnHide();
    // 添加隐藏动画
    gameObject.GetComponent<CanvasGroup>().DOFade(0, 0.3f);
}
```

### 3. 本地化支持

在ViewModel中使用本地化键：

```csharp
public string Title
{
    get => LocalizationManager.GetText("ui_main_menu_title");
}
```

### 4. 响应式布局

使用LayoutGroup组件美化按钮布局，例如使用VerticalLayoutGroup。

## 常见问题

### Q: 按钮点击没有反应？
A: 检查以下几点：
1. UIInitializer脚本是否在场景中并且Awake被调用
2. Prefab是否正确保存到 `Assets/Prefabs/UI/MainMenu/MainMenu.prefab`
3. Presenter中的 `OnInitialize()` 方法是否被调用

### Q: 如何修改按钮样式？
A: 在生成Prefab后，直接在Unity编辑器中修改按钮的Image和Text组件，修改会自动保存到Prefab中。

### Q: 如何添加背景音乐？
A: 在Presenter的 `OnShow()` 方法中播放音乐：

```csharp
public override void OnShow()
{
    base.OnShow();
    AudioManager.PlayBackground("menu_music");
}
```

## 技术架构优势

✅ **清晰的职责分离**：View、Presenter、ViewModel各司其职  
✅ **易于测试**：业务逻辑集中在Presenter和ViewModel中  
✅ **易于扩展**：添加新UI只需创建相应的MVP三个类  
✅ **自动化生成**：Prefab生成脚本提高开发效率  
✅ **全局管理**：UIManager统一管理所有UI的生命周期  

---

**文档版本**: v1.0  
**创建日期**: 2026-05-07
