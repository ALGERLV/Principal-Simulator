# MainMenu UI 快速开始指南

## 快速设置（3步完成）

### Step 1: 生成Prefab（在Unity编辑器中）
```
菜单 → Tools → 生成UI预制体 → 生成MainMenu
```
脚本会自动生成完整的UI界面并直接保存到 `Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab`

**注意**：Prefab现在直接生成到Resources文件夹中，无需手动移动！

### Step 2: 配置场景

1. 在游戏启动场景中创建空GameObject，命名为 `UIInitializer`
2. 添加 `Assets/_Project/Presentation/UIInitializer.cs` 脚本到该GameObject
3. 该脚本会自动：
   - 初始化UIManager
   - 注册所有UI配置
   - 加载MainMenu预制体

### Step 3: 运行游戏

启动游戏时，UIInitializer会自动通过UIManager加载并显示MainMenu界面。

## UI界面构成

```
┌─────────────────────────────────┐
│      电子战棋 (标题)              │
│                                   │
│     ┌──────────────────┐          │
│     │   开始游戏        │          │
│     │  (蓝色可点击)     │          │
│     └──────────────────┘          │
│                                   │
│     ┌──────────────────┐          │
│     │   退出游戏        │          │
│     │  (红色可点击)     │          │
│     └──────────────────┘          │
└─────────────────────────────────┘
```

## 核心流程

```
游戏启动
  ↓
UIInitializer.Awake() - 初始化UIManager和配置
  ↓
UIInitializer.Start() - 调用UIManager.Show<MainMenuView>()
  ↓
UIManager从Resources加载Prefab
  ↓
MainMenuView初始化 - 创建Presenter和ViewModel
  ↓
MainMenu显示在屏幕上
```

## 核心组件说明

### 1. MainMenuViewModel（数据层）
- `Title` 属性：游戏标题
- `StartGameCommand`：开始游戏命令
- `ExitGameCommand`：退出游戏命令

### 2. MainMenuPresenter（调度层）
- 绑定按钮点击事件
- `OnStartClicked()`：处理开始游戏
- `OnExitClicked()`：处理退出游戏

### 3. MainMenuView（视图层）
- 自动同步ViewModel数据到UI显示
- 响应用户按钮点击

## 常见命令

```csharp
// 创建（首次加载）
UIManager.Instance.Create<MainMenuView>();

// 显示（从隐藏状态显示）
UIManager.Instance.Show<MainMenuView>();

// 隐藏（保留在内存中）
UIManager.Instance.Hide<MainMenuView>();

// 销毁（完全删除）
UIManager.Instance.Destroy<MainMenuView>();

// 切换显示/隐藏
UIManager.Instance.Toggle<MainMenuView>();

// 查询状态
bool exists = UIManager.Instance.IsCreated<MainMenuView>();
bool visible = UIManager.Instance.IsVisible<MainMenuView>();
```

## 文件结构

```
Assets/
├── Resources/Prefabs/UI/MainMenu/
│   └── MainMenu.prefab             # UIManager加载的文件
│
└── _Project/Presentation/
    ├── UI/
    │   ├── Core/                   # UI框架核心
    │   └── Panels/MainMenu/
    │       ├── MainMenuView.cs
    │       ├── MainMenuPresenter.cs
    │       ├── MainMenuViewModel.cs
    │       └── MainMenuPrefabGenerator.cs
    └── UIInitializer.cs            # UI系统初始化
```

## 常见问题

### Q: 为什么需要生成Prefab？
A: Prefab包含完整的UI结构（Canvas、按钮、文本等），并将其与MainMenuView脚本绑定，这样UIManager才能正确加载和管理它。

### Q: 如何修改按钮的样式？
A: 生成Prefab后，直接在Unity编辑器中打开Prefab编辑模式，修改按钮的Image、Text等组件。修改会自动保存。

### Q: 按钮点击没有反应？
A: 检查以下几点：
1. UIInitializer脚本是否在场景中
2. Prefab是否正确生成到 `Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab`
3. Console中是否有错误日志

### Q: 如何自定义按钮行为？
A: 修改MainMenuPresenter中的`OnStartClicked()`和`OnExitClicked()`方法，添加你的游戏逻辑。

---

现在你可以在Unity编辑器中运行Prefab生成脚本，启动游戏看到完整的开始菜单界面！

更多技术细节和经验教训，请查看 [UI_Generation_Experience.md](UI_Generation_Experience.md)


