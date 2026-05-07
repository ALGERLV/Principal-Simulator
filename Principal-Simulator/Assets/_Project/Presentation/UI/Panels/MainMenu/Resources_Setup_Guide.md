# Resources文件夹配置指南

## 问题说明

UIManager使用`Resources.Load<T>()`加载Prefab，这要求Prefab必须放在`Assets/Resources/`目录下。

## 配置步骤

### Step 1: 创建Resources目录结构

确保以下目录存在（如不存在则创建）：

```
Assets/
└── Resources/
    └── Prefabs/
        └── UI/
            └── MainMenu/
```

### Step 2: 生成Prefab

在Unity编辑器中执行：
```
Tools → 生成UI预制体 → 生成MainMenu
```

这会在 `Assets/Prefabs/UI/MainMenu/` 中生成 `MainMenu.prefab`

### Step 3: 移动Prefab到Resources

**方式一：在编辑器中复制**
1. 打开Project窗口
2. 找到 `Assets/Prefabs/UI/MainMenu/MainMenu.prefab`
3. 复制 (Ctrl+D) 或拖拽到 `Assets/Resources/Prefabs/UI/MainMenu/`
4. 可选：删除原来的 `Assets/Prefabs/UI/MainMenu/MainMenu.prefab`

**方式二：使用Windows文件管理器**
1. 打开Windows文件浏览器
2. 导航到项目目录 `d:/project/Principal-Simulator/Principal-Simulator/`
3. 复制 `Assets/Prefabs/UI/MainMenu/MainMenu.prefab` 
4. 粘贴到 `Assets/Resources/Prefabs/UI/MainMenu/`
5. 在Unity编辑器中按 `Ctrl+R` 刷新资源

### Step 4: 验证路径

在Unity编辑器Console中执行以下代码验证：

```csharp
var prefab = Resources.Load<GameObject>("Prefabs/UI/MainMenu/MainMenu");
if (prefab != null)
    Debug.Log("Prefab加载成功！");
else
    Debug.LogError("Prefab加载失败，请检查路径");
```

## Resources.Load加载路径规则

**Resources.Load()的路径规则：**

```
// 文件系统路径              Resources.Load路径
Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab  →  "Prefabs/UI/MainMenu/MainMenu"
                ↑                                         ↑
          从这里开始              不包括Assets/Resources/和文件扩展名
```

- 路径不需要包含 `Assets/Resources/`
- 路径不需要包含文件扩展名 `.prefab`
- 使用 `/` 分隔目录（不是 `\`）

## 常见错误

### ❌ 错误示例

```csharp
// 错误：包含了Assets/Resources/
Resources.Load<GameObject>("Assets/Resources/Prefabs/UI/MainMenu/MainMenu");

// 错误：包含了文件扩展名
Resources.Load<GameObject>("Prefabs/UI/MainMenu/MainMenu.prefab");

// 错误：使用了反斜杠
Resources.Load<GameObject>("Prefabs\\UI\\MainMenu\\MainMenu");
```

### ✅ 正确示例

```csharp
// 正确
Resources.Load<GameObject>("Prefabs/UI/MainMenu/MainMenu");
```

## 目录结构检查清单

运行这个脚本确保目录结构正确：

```csharp
#if UNITY_EDITOR
[UnityEditor.MenuItem("Tools/检查Resources目录")]
public static void CheckResourcesPath()
{
    var prefab = Resources.Load<GameObject>("Prefabs/UI/MainMenu/MainMenu");
    
    if (prefab == null)
    {
        Debug.LogError("❌ Prefab未找到，请检查:");
        Debug.LogError("1. 文件是否存在: Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab");
        Debug.LogError("2. 是否拼写错误");
        Debug.LogError("3. 是否需要重新刷新Unity资源库 (Ctrl+R)");
    }
    else
    {
        Debug.Log("✅ Prefab加载成功！");
        Debug.Log($"Prefab名称: {prefab.name}");
        Debug.Log($"Prefab路径: Assets/Resources/Prefabs/UI/MainMenu/MainMenu.prefab");
    }
}
#endif
```

## Resources vs AssetDatabase

| 方法 | 用途 | 性能 | 何时使用 |
|------|------|------|---------|
| `Resources.Load()` | 运行时加载 | 一次 | 游戏运行时动态加载 |
| `AssetDatabase.Load()` | 编辑器加载 | 较慢 | 编辑器脚本、工具 |
| Prefab序列化 | 编辑器指派 | 最快 | 预先在Inspector中配置 |

对于UIManager，使用`Resources.Load()`是最适合的。

## 文件大小与内存优化

- Resources中的所有资源会在项目打包时被包含
- 未使用的Resources不会被自动删除，增加包体积
- 建议定期清理不使用的Resources文件

---

如果按照以上步骤配置后仍然有问题，请检查Console中的错误日志。
