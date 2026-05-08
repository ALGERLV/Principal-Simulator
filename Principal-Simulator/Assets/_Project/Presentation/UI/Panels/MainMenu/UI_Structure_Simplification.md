# UI 项目结构规范（简化版）

## 核心理念

**代码：分文件夹组织 | Prefab：集中存放**

所有UI的源代码文件按功能分散在不同文件夹中，方便管理；所有生成的Prefab集中存放在统一目录下，保持简洁。

## 目录结构

```
Assets/
│
├── Resources/
│   └── UI/                              # ← 所有UI Prefab集中放这里，不创建子文件夹
│       ├── MainMenu.prefab
│       ├── BattleHUD.prefab
│       ├── SpawnPanel.prefab
│       ├── UnitInfoCard.prefab
│       └── ...
│
└── _Project/Presentation/
    ├── UI/
    │   ├── Core/                        # UI框架核心代码
    │   │   ├── UIManager.cs
    │   │   ├── BaseView.cs
    │   │   ├── BasePresenter.cs
    │   │   ├── ViewModelBase.cs
    │   │   └── ...
    │   │
    │   └── Panels/                      # ← 所有UI代码这里按面板类型分文件夹
    │       ├── MainMenu/
    │       │   ├── MainMenuView.cs
    │       │   ├── MainMenuPresenter.cs
    │       │   ├── MainMenuViewModel.cs
    │       │   └── MainMenuPrefabGenerator.cs
    │       ├── BattleHUD/
    │       │   ├── BattleHUDView.cs
    │       │   ├── BattleHUDPresenter.cs
    │       │   ├── BattleHUDViewModel.cs
    │       │   └── BattleHUDPrefabGenerator.cs
    │       ├── SpawnPanel/
    │       │   ├── SpawnPanelView.cs
    │       │   ├── SpawnPanelPresenter.cs
    │       │   ├── SpawnPanelViewModel.cs
    │       │   └── SpawnPanelPrefabGenerator.cs
    │       └── UnitInfoCard/
    │           ├── UnitInfoCardView.cs
    │           ├── UnitInfoCardPresenter.cs
    │           ├── UnitInfoCardViewModel.cs
    │           └── UnitInfoCardPrefabGenerator.cs
    │
    └── UIInitializer.cs                # UI系统初始化
```

## 关键点

### ✅ Resources/UI/ 目录规则

```
Assets/Resources/UI/
├── MainMenu.prefab                  # ✅ 直接放在 UI 目录下
├── BattleHUD.prefab                 # ✅ 无子文件夹
├── SpawnPanel.prefab                # ✅ 文件名 = 类名
└── UnitInfoCard.prefab              # ✅ Resources.Load("UI/UIName") 加载
```

**不要这样做**：
```
❌ Assets/Resources/UI/MainMenu/MainMenu.prefab
❌ Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab
❌ Assets/Resources/UI/Panels/MainMenu.prefab
```

### ✅ UIInitializer 加载规则

```csharp
// 加载路径简化为
Resources.Load<GameObject>("UI/MainMenu")          // ✅ 正确
Resources.Load<GameObject>("UI/BattleHUD")
Resources.Load<GameObject>("UI/SpawnPanel")

// 不要这样
Resources.Load<GameObject>("UI/MainMenu/MainMenu")  // ❌ 错误
Resources.Load<GameObject>("Prefabs/UI/MainMenu/MainMenu")  // ❌ 错误
```

### ✅ Prefab生成脚本规范

每个UI的生成脚本应该遵循这个模式：

```csharp
[UnityEditor.MenuItem("Tools/生成UI预制体/生成XXX")]
public static void GenerateXXXPrefab()
{
    // 直接生成到 Assets/Resources/UI/ 下，文件名 = 类名
    string prefabPath = "Assets/Resources/UI/XXX.prefab";
    
    // ... 生成逻辑 ...
    
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(xxxGO, assetPath);
}
```

**规则**：
- 文件名 = 类名（例如 `MainMenu.prefab` 对应 `MainMenuView`）
- 路径 = `Assets/Resources/UI/XXX.prefab`
- 无子文件夹
- 一个UIType一个Prefab文件

## 为什么这样设计

### 优势

| 方面 | 优势 |
|------|------|
| **简洁性** | Resources/UI 目录很干净，快速查找任何Prefab |
| **扩展性** | 添加新UI时，只需在 Panels 创建文件夹，不用修改Resources结构 |
| **加载性能** | Resources.Load("UI/Name") 比 "UI/Type/Name/Name" 快速简洁 |
| **一致性** | 所有UI遵循同一规则，无例外 |

### 代码示例对比

**旧方式（多级子文件夹）**：
```csharp
Resources.Load<GameObject>("Prefabs/UI/MainMenu/MainMenu")
Resources.Load<GameObject>("Prefabs/UI/BattleHUD/BattleHUD")
Resources.Load<GameObject>("Prefabs/UI/Panels/SpawnPanel/SpawnPanel")
```

**新方式（简化路径）**：
```csharp
Resources.Load<GameObject>("UI/MainMenu")
Resources.Load<GameObject>("UI/BattleHUD")
Resources.Load<GameObject>("UI/SpawnPanel")
```

## 迁移现有UI

如果项目中已经有其他UI面板（如BattleHUD、SpawnPanel等），需要迁移到新结构：

### 步骤

1. **删除旧Prefab**
   - 删除 `Assets/Resources/Prefabs/UI/XXX/` 目录及其所有文件

2. **重新生成**
   - 运行对应的生成脚本（例如 BattleHUDPrefabGenerator）
   - 自动生成到新位置 `Assets/Resources/UI/XXX.prefab`

3. **验证加载**
   - 在UIInitializer中验证加载路径是否正确
   - 运行游戏测试UI是否正常加载

4. **清理垃圾**
   - 删除旧的文件夹结构
   - Ctrl+R 刷新Unity资源库

## 常见操作

### 添加新UI

1. 在 `_Project/Presentation/UI/Panels/` 中创建新文件夹：`NewUI/`
2. 创建三个文件：`NewUIView.cs`、`NewUIPresenter.cs`、`NewUIViewModel.cs`
3. 创建生成脚本：`NewUIPrefabGenerator.cs`，路径设为 `Assets/Resources/UI/NewUI.prefab`
4. 在UIInitializer中添加加载配置
5. 运行菜单生成Prefab

### 查找UI文件

```
查找代码 → _Project/Presentation/UI/Panels/XXX/ 文件夹
查找Prefab → Assets/Resources/UI/XXX.prefab
```

### 快速定位

```
Resources.Load("UI/MainMenu")  ← 对应代码在 Panels/MainMenu/ 文件夹中
```

## 最佳实践

### ✅ 应该做的

- 同一类型的UI代码放在同一文件夹
- 使用有意义的文件夹名称（对应Prefab名称）
- Prefab直接放在Resources/UI/
- UIInitializer集中管理所有UI配置

### ❌ 不应该做的

- 在Resources/UI/ 下创建子文件夹
- 将Prefab分散在多个目录
- Prefab文件名与类名不一致
- 直接在代码中硬编码Resources路径

## 清单

生成新UI时：

- [ ] 在 `Panels/` 中创建新文件夹
- [ ] 创建 View/Presenter/ViewModel 三个文件
- [ ] 创建生成脚本，**路径 = `Assets/Resources/UI/UIName.prefab`**
- [ ] 在UIInitializer中添加加载代码
- [ ] 运行生成脚本测试
- [ ] 验证Prefab生成在 `Assets/Resources/UI/` 下
- [ ] 删除Resources中的旧子文件夹
- [ ] 测试UIManager加载是否正常

---

**规则简单原则**：所有Prefab都在 `Resources/UI/` 下，面板名 = 文件名，一层目录，从此不再复杂！
