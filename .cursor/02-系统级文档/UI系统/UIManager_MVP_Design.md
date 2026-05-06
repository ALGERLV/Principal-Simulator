# UIManager MVP 架构设计文档

## 1. 设计目标

- **简单轻量**：不引入过度设计，保持代码可读性
- **MVP架构**：清晰的分层责任，便于测试和维护
- **双向绑定**：VM <-> View 自动同步数据变化
- **全局管理**：UIManager统一管理所有UI的显示/隐藏/生命周期
- **统一结构**：所有UI组件（面板、弹窗、HUD）都是 MVP + Prefab 的统一结构

---

## 2. 核心架构

```
┌─────────────────────────────────────────────────────────────┐
│                        UIManager (单例)                        │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  - 管理所有已创建的UI实例（MVP组合）                      │  │
│  │  - 提供 Show/Create/Hide/Destroy 全局接口               │  │
│  │  - 处理UI层级（Sorting Order）                          │  │
│  │  - 维护实例缓存                                         │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
           ┌──────────────────┼──────────────────┐
           ▼                  ▼                  ▼
    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
    │ MainMenuUI  │    │  BattleHUD  │    │ DamagePopup │
    │             │    │             │    │             │
    │ ┌─────────┐ │    │ ┌─────────┐ │    │ ┌─────────┐ │
    │ │ Prefab  │ │    │ │ Prefab  │ │    │ │ Prefab  │ │
    │ │ (根节点) │ │    │ │ (根节点) │ │    │ │ (根节点) │ │
    │ └────┬────┘ │    │ └────┬────┘ │    │ └────┬────┘ │
    │      │      │    │      │      │    │      │      │
    │ ┌────┴────┐ │    │ ┌────┴────┐ │    │ ┌────┴────┐ │
    │ │  View   │ │    │ │  View   │ │    │ │  View   │ │
    │ │(MonoBehaviour)│  │ │(MonoBehaviour)│  │ │(MonoBehaviour)│
    │ │-UI组件  │ │    │ │-UI组件  │ │    │ │-UI组件  │ │
    │ │-用户事件 │ │    │ │-用户事件 │ │    │ │-用户事件 │ │
    │ └────┬────┘ │    │ └────┬────┘ │    │ └────┬────┘ │
    │      │      │    │      │      │    │      │      │
    │ ┌────┴────┐ │    │ ┌────┴────┐ │    │ ┌────┴────┐ │
    │ │Presenter│ │    │ │Presenter│ │    │ │Presenter│ │
    │ │-业务逻辑 │ │    │ │-业务逻辑 │ │    │ │-业务逻辑 │ │
    │ └────┬────┘ │    │ └────┬────┘ │    │ └────┬────┘ │
    │      │      │    │      │      │    │      │      │
    │ ┌────┴────┐ │    │ ┌────┴────┐ │    │ ┌────┴────┐ │
    │ │   VM    │ │    │ │   VM    │ │    │ │   VM    │ │
    │ │-数据状态 │ │    │ │-数据状态 │ │    │ │-数据状态 │ │
    │ │-属性通知 │ │    │ │-属性通知 │ │    │ │-属性通知 │ │
    │ └─────────┘ │    │ └─────────┘ │    │ └─────────┘ │
    └─────────────┘    └─────────────┘    └─────────────┘

    所有UI组件都是统一的 MVP + Prefab 结构，没有额外包装层
```

---

## 3. 核心组件定义

### 3.1 UIManager（全局管理器）

```
位置: Assets/_Project/Presentation/UI/Core/UIManager.cs
```

**职责**：
- 单例模式，全局访问点
- 管理所有已创建的UI实例
- 处理UI层级计算（自动分配Sorting Order）
- 提供异步加载/实例化Prefab的接口
- 支持实例配置（是否缓存、是否模态、关闭策略等）

**核心接口**：
```csharp
// 创建并显示UI（自动实例化Prefab并组装MVP）
void Create<T>(Action<T> onReady = null) where T : IUIView;
void Create(string uiId, Action<IUIView> onReady = null);

// 显示/隐藏（控制GameObject.active）
void Show<T>() where T : IUIView;
void Hide<T>() where T : IUIView;

// 销毁（销毁GameObject，清理MVP）
void Destroy<T>() where T : IUIView;
void Destroy(string uiId);

// 获取已创建的实例
T Get<T>() where T : IUIView;
```

**管理策略**：
```csharp
// UI实例配置
public class UIConfig
{
    public string UIId;               // 唯一标识
    public GameObject Prefab;          // UI预制体
    public Transform Parent;           // 父节点（默认UIManager根节点）
    public bool CacheOnHide;           // 隐藏时是否缓存（不销毁）
    public bool IsModal;               // 是否为模态（阻止下层交互）
    public int BaseSortingOrder;        // 基础排序层级
}
```

---

### 3.2 IPresenter / BasePresenter（调度者）

```
位置: Assets/_Project/Presentation/UI/Core/IPresenter.cs
       Assets/_Project/Presentation/UI/Core/BasePresenter.cs
```

**职责**：
- 连接VM和View的中间层
- 处理用户交互的业务逻辑
- 订阅VM的事件，转发给View
- 订阅View的事件，调用VM命令或执行业务逻辑
- 无状态，纯逻辑调度

**生命周期**：
```csharp
void Initialize(TView view, TViewModel vm);
void OnShow();      // UI显示时调用（GameObject.SetActive(true)）
void OnHide();      // UI隐藏时调用（GameObject.SetActive(false)）
void OnDestroy();   // UI销毁时调用（清理订阅，释放资源）
```

---

### 3.3 IViewModel / ViewModelBase（视图模型）

```
位置: Assets/_Project/Presentation/UI/Core/IViewModel.cs
       Assets/_Project/Presentation/UI/Core/ViewModelBase.cs
```

**职责**：
- 持有UI所需的全部数据状态
- 属性变更时发出通知（PropertyChanged）
- 定义命令（Command）供View调用
- 通过EventBus与GamePlay层通信

**关键特性**：
```csharp
// 属性通知示例
public string Title
{
    get => _title;
    set => SetProperty(ref _title, value);  // 自动触发PropertyChanged
}

// 命令定义
public ICommand ConfirmCommand { get; }
```

---

### 3.4 IUIView / BaseView（视图）

```
位置: Assets/_Project/Presentation/UI/Core/IUIView.cs
       Assets/_Project/Presentation/UI/Core/BaseView.cs
```

**职责**：
- MonoBehaviour，挂载在Prefab根节点
- 作为MVP组合的入口点，持有View、Presenter、ViewModel的组装关系
- 持有并绑定所有UI组件引用（Button, Text, Image等）
- 监听VM属性变化，更新UI显示
- 响应用户输入，触发事件或调用Presenter方法
- 控制自身的显示/隐藏/动画

**绑定模式**：
```csharp
// View → Presenter → VM（用户操作）
ConfirmButton.onClick.AddListener(() => Presenter.OnConfirmClicked());

// VM → View（数据变化）
VM.Subscribe(nameof(VM.Title), () => TitleText.text = VM.Title);
```

**关键特性**：
```csharp
// View作为MVP的组装点
public class MainMenuView : BaseView<MainMenuViewModel>
{
    protected override void CreatePresenter()
    {
        _presenter = new MainMenuPresenter(this, ViewModel);
    }
    
    // UIManager直接操作View
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
```

---

## 4. 项目目录结构

```
Assets/
├── Prefabs/
│   └── UI/                         # UI预制体，层级与代码对应
│       ├── MainMenu/
│       │   └── MainMenu.prefab     # 根节点挂MainMenuView.cs
│       │
│       ├── BattleHUD/
│       │   └── BattleHUD.prefab    # 根节点挂BattleHUDView.cs
│       │
│       ├── UnitInfoPopup/
│       │   └── UnitInfoPopup.prefab
│       │
│       ├── DamageFloatText/
│       │   └── DamageFloatText.prefab
│       │
│       └── ...
│
└── _Project/Presentation/UI/
    ├── Core/                       # UI框架核心
    │   ├── UIManager.cs
    │   ├── IUIView.cs
    │   ├── BaseView.cs
    │   ├── IPresenter.cs
    │   ├── BasePresenter.cs
    │   ├── IViewModel.cs
    │   ├── ViewModelBase.cs
    │   ├── ICommand.cs
    │   └── UIBinding.cs
    │
    ├── Components/                 # 各UI组件代码
    │   ├── MainMenu/
    │   │   ├── MainMenuView.cs
    │   │   ├── MainMenuViewModel.cs
    │   │   └── MainMenuPresenter.cs
    │   │
    │   ├── BattleHUD/
    │   │   ├── BattleHUDView.cs
    │   │   ├── BattleHUDViewModel.cs
    │   │   └── BattleHUDPresenter.cs
    │   │
    │   ├── UnitInfoPopup/
    │   │   ├── UnitInfoPopupView.cs
    │   │   ├── UnitInfoPopupViewModel.cs
    │   │   └── UnitInfoPopupPresenter.cs
    │   │
    │   ├── DamageFloatText/
    │   │   ├── DamageFloatTextView.cs
    │   │   ├── DamageFloatTextViewModel.cs
    │   │   └── DamageFloatTextPresenter.cs
    │   │
    │   └── ...
    │
    └── Common/                     # 通用UI工具
        ├── BindableButton.cs
        └── BindableText.cs
```

---

## 5. 核心代码实现

### 5.1 ViewModel（数据层）

#### IViewModel.cs - 接口定义

```csharp
using System;
using System.ComponentModel;

namespace TBS.Presentation.UI
{
    public interface IViewModel : INotifyPropertyChanged
    {
        void RaisePropertyChanged(string propertyName);
        void RaisePropertiesChanged(params string[] propertyNames);
        void Clear();
    }
}
```

#### ViewModelBase.cs - 基类实现

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TBS.Presentation.UI
{
    public abstract class ViewModelBase : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly Dictionary<string, object> _propertyValues = new();

        protected T GetProperty<T>([CallerMemberName] string propertyName = null, T defaultValue = default)
        {
            return _propertyValues.TryGetValue(propertyName, out var value) ? (T)value : defaultValue;
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (_propertyValues.TryGetValue(propertyName, out var existing) &&
                EqualityComparer<T>.Default.Equals((T)existing, value))
                return false;

            _propertyValues[propertyName] = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertiesChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
                RaisePropertyChanged(name);
        }

        public virtual void Clear()
        {
            _propertyValues.Clear();
        }

        public void Subscribe(string propertyName, Action callback)
        {
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                    callback?.Invoke();
            };
        }
    }
}
```

---

### 5.2 Presenter（调度层）

#### IPresenter.cs - 接口定义

```csharp
namespace TBS.Presentation.UI
{
    public interface IPresenter
    {
        void OnShow();
        void OnHide();
        void OnDestroy();
    }

    public interface IPresenter<out TView, out TViewModel> : IPresenter
        where TView : IUIView
        where TViewModel : IViewModel
    {
        TView View { get; }
        TViewModel ViewModel { get; }
    }
}
```

#### BasePresenter.cs - 基类实现

```csharp
namespace TBS.Presentation.UI
{
    public abstract class BasePresenter<TView, TViewModel> : IPresenter<TView, TViewModel>
        where TView : IUIView
        where TViewModel : IViewModel
    {
        public TView View { get; private set; }
        public TViewModel ViewModel { get; private set; }

        private bool _isInitialized;

        public virtual void Initialize(TView view, TViewModel viewModel)
        {
            if (_isInitialized) return;
            View = view;
            ViewModel = viewModel;
            _isInitialized = true;
            OnInitialize();
        }

        protected abstract void OnInitialize();
        public virtual void OnShow() { }
        public virtual void OnHide() { }

        public virtual void OnDestroy()
        {
            View = null;
            ViewModel = null;
            _isInitialized = false;
        }
    }
}
```

---

### 5.3 View（视图层）

#### IUIView.cs - 接口定义

```csharp
using UnityEngine;

namespace TBS.Presentation.UI
{
    public interface IUIView
    {
        string UIId { get; }
        bool IsVisible { get; }
        GameObject gameObject { get; }
        Transform transform { get; }

        void OnShow();
        void OnHide();
        void OnBeforeDestroy();
    }
}
```

#### BaseView.cs - 基类实现

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TBS.Presentation.UI
{
    public abstract class BaseView<TViewModel> : MonoBehaviour, IUIView
        where TViewModel : IViewModel, new()
    {
        [SerializeField] private string _uiId;

        public string UIId => string.IsNullOrEmpty(_uiId) ? GetType().Name : _uiId;
        public bool IsVisible => gameObject != null && gameObject.activeInHierarchy;
        public TViewModel ViewModel { get; private set; }

        protected IPresenter Presenter { get; private set; }
        private readonly Dictionary<string, List<Action>> _bindings = new();
        private bool _isInitialized;

        protected virtual void Awake()
        {
            if (_isInitialized) return;

            ViewModel = new TViewModel();
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            Presenter = CreatePresenter();
            Presenter?.Initialize(this, ViewModel);
            _isInitialized = true;
        }

        protected virtual void Start()
        {
            OnBind();
        }

        protected virtual void OnDestroy()
        {
            OnBeforeDestroy();
            Presenter?.OnDestroy();
            Presenter = null;

            if (ViewModel != null)
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;

            _bindings.Clear();
        }

        protected abstract IPresenter CreatePresenter();
        protected abstract void OnBind();

        protected void Bind(string propertyName, Action callback)
        {
            if (!_bindings.TryGetValue(propertyName, out var list))
            {
                list = new List<Action>();
                _bindings[propertyName] = list;
            }
            list.Add(callback);
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_bindings.TryGetValue(e.PropertyName, out var callbacks))
            {
                foreach (var callback in callbacks)
                    callback?.Invoke();
            }
        }

        public virtual void OnShow()
        {
            gameObject.SetActive(true);
            Presenter?.OnShow();
        }

        public virtual void OnHide()
        {
            Presenter?.OnHide();
            gameObject.SetActive(false);
        }

        public virtual void OnBeforeDestroy() { }
    }
}
```

---

### 5.4 Command（命令层）

#### ICommand.cs

```csharp
using System;

namespace TBS.Presentation.UI
{
    public interface ICommand
    {
        bool CanExecute { get; }
        void Execute();
        event Action<bool> OnCanExecuteChanged;
    }

    public interface ICommand<T>
    {
        bool CanExecute(T parameter);
        void Execute(T parameter);
        event Action<bool> OnCanExecuteChanged;
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public bool CanExecute => _canExecute?.Invoke() ?? true;
        public event Action<bool> OnCanExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public void Execute()
        {
            if (CanExecute)
                _execute.Invoke();
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged?.Invoke(CanExecute);
        }
    }

    public class RelayCommand<T> : ICommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event Action<bool> OnCanExecuteChanged;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(T parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(T parameter)
        {
            if (CanExecute(parameter))
                _execute.Invoke(parameter);
        }

        public void RaiseCanExecuteChanged() => OnCanExecuteChanged?.Invoke(true);
    }
}
```

---

### 5.5 UIManager（管理器）

#### UIManager.cs

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TBS.Presentation.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                    CreateInstance();
                return _instance;
            }
        }

        private static void CreateInstance()
        {
            var go = new GameObject("UIManager");
            _instance = go.AddComponent<UIManager>();
            DontDestroyOnLoad(go);
        }

        [SerializeField] private Transform _uiRoot;
        [SerializeField] private int _baseSortingOrder = 100;
        [SerializeField] private int _sortingOrderStep = 10;

        private readonly Dictionary<string, IUIView> _uiInstances = new();
        private readonly Dictionary<string, UIConfig> _uiConfigs = new();
        private readonly List<IUIView> _visibleUIStack = new();

        public Transform UIRoot
        {
            get
            {
                if (_uiRoot == null)
                    _uiRoot = transform;
                return _uiRoot;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            if (_uiRoot == null) _uiRoot = transform;
        }

        #region 配置管理

        public void RegisterConfig(UIConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.UIId)) return;
            _uiConfigs[config.UIId] = config;
        }

        public void RegisterConfigs(params UIConfig[] configs)
        {
            foreach (var config in configs)
                RegisterConfig(config);
        }

        public UIConfig GetConfig(string uiId)
        {
            _uiConfigs.TryGetValue(uiId, out var config);
            return config;
        }

        #endregion

        #region 创建/显示/隐藏/销毁

        public void Create<T>(Action<T> onReady = null) where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            Create(uiId, view => onReady?.Invoke(view as T));
        }

        public void Create(string uiId, Action<IUIView> onReady = null)
        {
            if (_uiInstances.TryGetValue(uiId, out var existingUI))
            {
                onReady?.Invoke(existingUI);
                return;
            }

            var config = GetConfig(uiId);
            if (config?.Prefab == null)
            {
                Debug.LogError($"[UIManager] UI配置或Prefab为空: {uiId}");
                return;
            }

            var parent = config.Parent ?? UIRoot;
            var go = Instantiate(config.Prefab, parent);
            go.name = uiId;

            var view = go.GetComponent<IUIView>();
            if (view == null)
            {
                Debug.LogError($"[UIManager] Prefab根节点缺少IUIView组件: {uiId}");
                Destroy(go);
                return;
            }

            _uiInstances[uiId] = view;
            go.SetActive(false);
            onReady?.Invoke(view);
        }

        public void Show<T>() where T : MonoBehaviour, IUIView
        {
            Show(typeof(T).Name);
        }

        public void Show(string uiId)
        {
            if (!_uiInstances.TryGetValue(uiId, out var view))
            {
                Create(uiId, v => ShowInternal(v, uiId));
                return;
            }
            ShowInternal(view, uiId);
        }

        private void ShowInternal(IUIView view, string uiId)
        {
            if (view == null) return;

            var config = GetConfig(uiId);
            if (config?.IsModal == true)
                SetModalBlocking(true);

            SetSortingOrder(view);
            view.OnShow();

            if (!_visibleUIStack.Contains(view))
                _visibleUIStack.Add(view);
        }

        public void Hide<T>() where T : MonoBehaviour, IUIView
        {
            Hide(typeof(T).Name);
        }

        public void Hide(string uiId)
        {
            if (!_uiInstances.TryGetValue(uiId, out var view)) return;

            var config = GetConfig(uiId);
            view.OnHide();
            _visibleUIStack.Remove(view);

            if (config?.IsModal == true)
                UpdateModalBlocking();

            if (config?.CacheOnHide == false)
                Destroy(uiId);
        }

        public void Toggle<T>() where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            if (_uiInstances.TryGetValue(uiId, out var view) && view.IsVisible)
                Hide(uiId);
            else
                Show(uiId);
        }

        public void Destroy<T>() where T : MonoBehaviour, IUIView
        {
            Destroy(typeof(T).Name);
        }

        public void Destroy(string uiId)
        {
            if (!_uiInstances.TryGetValue(uiId, out var view)) return;

            if (view.IsVisible)
            {
                view.OnHide();
                _visibleUIStack.Remove(view);
            }

            view.OnBeforeDestroy();
            if (view.gameObject != null)
                Object.Destroy(view.gameObject);

            _uiInstances.Remove(uiId);
            UpdateModalBlocking();
        }

        public void DestroyAll()
        {
            foreach (var uiId in new List<string>(_uiInstances.Keys))
                Destroy(uiId);
            _uiInstances.Clear();
            _visibleUIStack.Clear();
        }

        #endregion

        #region 获取/查询

        public T Get<T>() where T : MonoBehaviour, IUIView
        {
            return Get(typeof(T).Name) as T;
        }

        public IUIView Get(string uiId)
        {
            _uiInstances.TryGetValue(uiId, out var view);
            return view;
        }

        public bool IsCreated<T>() where T : MonoBehaviour, IUIView
        {
            return _uiInstances.ContainsKey(typeof(T).Name);
        }

        public bool IsVisible<T>() where T : MonoBehaviour, IUIView
        {
            return _uiInstances.TryGetValue(typeof(T).Name, out var view) && view.IsVisible;
        }

        #endregion

        #region 层级管理

        private void SetSortingOrder(IUIView view)
        {
            if (view == null) return;

            var order = _baseSortingOrder + _visibleUIStack.Count * _sortingOrderStep;
            var canvas = view.gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = view.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;

            if (view.gameObject.GetComponent<GraphicRaycaster>() == null)
                view.gameObject.AddComponent<GraphicRaycaster>();
        }

        #endregion

        #region 模态管理

        private GameObject _modalBlocker;

        private void SetModalBlocking(bool enable)
        {
            if (enable)
            {
                if (_modalBlocker == null)
                {
                    _modalBlocker = new GameObject("ModalBlocker");
                    _modalBlocker.transform.SetParent(UIRoot, false);
                    var rect = _modalBlocker.AddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    var image = _modalBlocker.AddComponent<Image>();
                    image.color = new Color(0, 0, 0, 0);
                }
                _modalBlocker.SetActive(true);
            }
            else if (_modalBlocker != null)
            {
                _modalBlocker.SetActive(false);
            }
        }

        private void UpdateModalBlocking()
        {
            bool hasModalVisible = false;
            foreach (var ui in _visibleUIStack)
            {
                var config = GetConfig(ui.UIId);
                if (config?.IsModal == true)
                {
                    hasModalVisible = true;
                    break;
                }
            }
            SetModalBlocking(hasModalVisible);
        }

        #endregion
    }

    [Serializable]
    public class UIConfig
    {
        public string UIId;
        public GameObject Prefab;
        public Transform Parent;
        public bool CacheOnHide = true;
        public bool IsModal = false;
        public int CustomSortingOrder = -1;
    }
}
```

---

## 6. 通信机制

### 6.1 双向绑定实现

```
┌─────────────┐      PropertyChanged       ┌─────────────┐
│    View     │ ───────────────────────────►│     VM      │
│             │   (VM变化时更新UI)           │  (数据层)   │
│  - 订阅通知  │                            │  - 存储数据  │
│  - 更新组件  │◄───────────────────────────│  - 验证逻辑  │
│             │      用户事件/命令调用       │  - 业务命令  │
└─────────────┘                            └─────────────┘
      │                                          ▲
      │         ┌──────────────┐                 │
      └────────►│  Presenter   │─────────────────┘
                │  (调度逻辑)   │
                └──────────────┘
```

### 6.2 具体通信流程

| 场景 | 流程 |
|------|------|
| 数据变化更新UI | VM属性Setter → SetProperty → 触发PropertyChanged → View订阅回调 → 更新UI组件 |
| 用户操作改数据 | View点击事件 → Presenter方法 → 调用VM属性Setter → 数据更新 → 触发UI更新 |
| 与GamePlay通信 | Presenter调用 → EventBus.Emit → GamePlay层订阅处理 → EventBus.Emit结果 → Presenter更新VM |

---

## 7. 扩展建议

| 功能 | 建议 |
|------|------|
| 动画系统 | BaseView中集成DOTween或Animator，提供PlayShowAnimation/PlayHideAnimation虚方法 |
| 导航栈 | UIManager维护面板导航栈，支持返回键自动关闭顶层面板 |
| 事件穿透 | 模态面板自动阻挡下层点击，通过CanvasGroup实现 |
| 本地化 | ViewModel中存储本地化Key，View层通过LocalizationManager.GetText()解析 |
| 动态加载 | 支持Addressables动态加载Prefab，UIManager中提供异步加载接口 |

---

## 8. 设计总结

| 组件 | 职责 | 是否可复用 | 依赖 |
|------|------|----------|------|
| UIManager | 全局管理，控制生命周期 | 单例，全局唯一 | IUIView |
| View | MVP组装点，UI组件绑定，用户事件 | 每个UI一个 | VM, Presenter |
| Presenter | 业务逻辑调度 | 每个UI一个 | View, VM, EventBus |
| ViewModel | 数据状态，属性通知 | 每个UI一个 | 无 |

### 关键设计原则

1. **无UIPanel基类**：所有UI都是统一的 MVP + Prefab 结构
2. **View作为入口**：预制体根节点挂载View脚本，View组装自己的Presenter和VM
3. **UIManager只认IUIView**：通过接口管理所有UI，不关心具体是什么类型的UI
4. **生命周期清晰**：Create/Show/Hide/Destroy 四个状态，UIManager统一管理

---

**文档版本**: v1.0  
**创建日期**: 2026-04-28  
**适用项目**: Principal-Simulator (Unity TBS Game)
