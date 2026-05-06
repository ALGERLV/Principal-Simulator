using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TBS.Presentation.UI
{
    /// <summary>
    /// UI管理器 - 全局单例，统一管理所有UI组件的创建、显示、隐藏和销毁
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateInstance();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 创建UIManager实例
        /// </summary>
        private static void CreateInstance()
        {
            var go = new GameObject("UIManager");
            _instance = go.AddComponent<UIManager>();
            DontDestroyOnLoad(go);
        }

        [SerializeField] private Transform _uiRoot;
        [SerializeField] private int _baseSortingOrder = 100;
        [SerializeField] private int _sortingOrderStep = 10;

        // UI实例缓存：UIId -> UI实例
        private readonly Dictionary<string, IUIView> _uiInstances = new();
        // UI配置：UIId -> 配置
        private readonly Dictionary<string, UIConfig> _uiConfigs = new();
        // 已显示的UI栈（用于层级管理）
        private readonly List<IUIView> _visibleUIStack = new();
        // 当前最高层级
        private int _currentMaxSortingOrder;

        /// <summary>
        /// UI根节点
        /// </summary>
        public Transform UIRoot
        {
            get
            {
                if (_uiRoot == null)
                {
                    _uiRoot = transform;
                }
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

            if (_uiRoot == null)
            {
                _uiRoot = transform;
            }

            _currentMaxSortingOrder = _baseSortingOrder;
        }

        #region 配置管理

        /// <summary>
        /// 注册UI配置
        /// </summary>
        public void RegisterConfig(UIConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.UIId)) return;
            _uiConfigs[config.UIId] = config;
        }

        /// <summary>
        /// 批量注册UI配置
        /// </summary>
        public void RegisterConfigs(params UIConfig[] configs)
        {
            foreach (var config in configs)
            {
                RegisterConfig(config);
            }
        }

        /// <summary>
        /// 获取UI配置
        /// </summary>
        public UIConfig GetConfig(string uiId)
        {
            _uiConfigs.TryGetValue(uiId, out var config);
            return config;
        }

        #endregion

        #region 创建UI

        /// <summary>
        /// 创建UI（泛型方式，需提前注册配置或使用直接传入Prefab的重载）
        /// </summary>
        public void Create<T>(Action<T> onReady = null) where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            Create(uiId, view => onReady?.Invoke(view as T));
        }

        /// <summary>
        /// 创建UI（通过UIId，需提前注册配置）
        /// </summary>
        public void Create(string uiId, Action<IUIView> onReady = null)
        {
            // 检查是否已存在
            if (_uiInstances.TryGetValue(uiId, out var existingUI))
            {
                onReady?.Invoke(existingUI);
                return;
            }

            // 获取配置
            var config = GetConfig(uiId);
            if (config?.Prefab == null)
            {
                Debug.LogError($"[UIManager] UI配置或Prefab为空: {uiId}");
                return;
            }

            CreateFromPrefab(config.Prefab, uiId, config.Parent, onReady);
        }

        /// <summary>
        /// 直接传入Prefab创建UI（无需提前注册配置）
        /// </summary>
        /// <typeparam name="T">View类型</typeparam>
        /// <param name="prefab">UI预制体</param>
        /// <param name="onReady">创建完成回调</param>
        /// <param name="cacheOnHide">隐藏时是否缓存</param>
        /// <param name="isModal">是否为模态</param>
        public void Create<T>(GameObject prefab, Action<T> onReady = null, bool cacheOnHide = true, bool isModal = false) where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            
            // 检查是否已存在
            if (_uiInstances.TryGetValue(uiId, out var existingUI))
            {
                onReady?.Invoke(existingUI as T);
                return;
            }

            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Prefab为空: {uiId}");
                return;
            }

            // 创建临时配置
            var config = new UIConfig
            {
                UIId = uiId,
                Prefab = prefab,
                CacheOnHide = cacheOnHide,
                IsModal = isModal,
                Parent = UIRoot
            };
            _uiConfigs[uiId] = config;

            CreateFromPrefab(prefab, uiId, UIRoot, view => onReady?.Invoke(view as T));
        }

        /// <summary>
        /// 从Prefab创建UI实例（内部方法）
        /// </summary>
        private void CreateFromPrefab(GameObject prefab, string uiId, Transform parent, Action<IUIView> onReady)
        {
            // 实例化
            var finalParent = parent ?? UIRoot;
            var go = Instantiate(prefab, finalParent);
            go.name = uiId;

            // 获取View组件
            var view = go.GetComponent<IUIView>();
            if (view == null)
            {
                Debug.LogError($"[UIManager] Prefab根节点缺少IUIView组件: {uiId}");
                Destroy(go);
                return;
            }

            // 缓存实例
            _uiInstances[uiId] = view;

            // 初始隐藏
            go.SetActive(false);

            // 回调
            onReady?.Invoke(view);
        }

        #endregion

        #region 显示/隐藏UI

        /// <summary>
        /// 显示UI
        /// </summary>
        public void Show<T>() where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            Show(uiId);
        }

        /// <summary>
        /// 显示UI（通过UIId）
        /// </summary>
        public void Show(string uiId)
        {
            if (!_uiInstances.TryGetValue(uiId, out var view))
            {
                // 未创建则先创建再显示
                Create(uiId, v => ShowInternal(v, uiId));
                return;
            }

            ShowInternal(view, uiId);
        }

        private void ShowInternal(IUIView view, string uiId)
        {
            if (view == null) return;

            var config = GetConfig(uiId);

            // 处理模态（阻止下层交互）
            if (config?.IsModal == true)
            {
                SetModalBlocking(true);
            }

            // 设置层级
            SetSortingOrder(view);

            // 调用View的显示方法
            view.OnShow();

            // 加入显示栈
            if (!_visibleUIStack.Contains(view))
            {
                _visibleUIStack.Add(view);
            }
        }

        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void Hide<T>() where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            Hide(uiId);
        }

        /// <summary>
        /// 隐藏UI（通过UIId）
        /// </summary>
        public void Hide(string uiId)
        {
            if (!_uiInstances.TryGetValue(uiId, out var view)) return;

            var config = GetConfig(uiId);

            // 调用View的隐藏方法
            view.OnHide();

            // 从显示栈移除
            _visibleUIStack.Remove(view);

            // 处理模态恢复
            if (config?.IsModal == true)
            {
                UpdateModalBlocking();
            }

            // 如果不缓存，则销毁
            if (config?.CacheOnHide == false)
            {
                Destroy(uiId);
            }
        }

        /// <summary>
        /// 切换UI显示/隐藏
        /// </summary>
        public void Toggle<T>() where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            Toggle(uiId);
        }

        /// <summary>
        /// 切换UI显示/隐藏
        /// </summary>
        public void Toggle(string uiId)
        {
            if (_uiInstances.TryGetValue(uiId, out var view) && view.IsVisible)
            {
                Hide(uiId);
            }
            else
            {
                Show(uiId);
            }
        }

        #endregion

        #region 销毁UI

        /// <summary>
        /// 销毁UI
        /// </summary>
        public void Destroy<T>() where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            Destroy(uiId);
        }

        /// <summary>
        /// 销毁UI（通过UIId）
        /// </summary>
        public void Destroy(string uiId)
        {
            if (!_uiInstances.TryGetValue(uiId, out var view)) return;

            // 先隐藏
            if (view.IsVisible)
            {
                view.OnHide();
                _visibleUIStack.Remove(view);
            }

            // 调用销毁前清理
            view.OnBeforeDestroy();

            // 销毁GameObject
            if (view.gameObject != null)
            {
                Object.Destroy(view.gameObject);
            }

            // 从缓存移除
            _uiInstances.Remove(uiId);

            // 更新模态阻挡
            UpdateModalBlocking();
        }

        /// <summary>
        /// 销毁所有UI
        /// </summary>
        public void DestroyAll()
        {
            var uiIds = new List<string>(_uiInstances.Keys);
            foreach (var uiId in uiIds)
            {
                Destroy(uiId);
            }
            _uiInstances.Clear();
            _visibleUIStack.Clear();
        }

        #endregion

        #region 获取UI

        /// <summary>
        /// 获取UI实例
        /// </summary>
        public T Get<T>() where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            return Get(uiId) as T;
        }

        /// <summary>
        /// 获取UI实例（通过UIId）
        /// </summary>
        public IUIView Get(string uiId)
        {
            _uiInstances.TryGetValue(uiId, out var view);
            return view;
        }

        /// <summary>
        /// 检查UI是否已创建
        /// </summary>
        public bool IsCreated<T>() where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            return _uiInstances.ContainsKey(uiId);
        }

        /// <summary>
        /// 检查UI是否正在显示
        /// </summary>
        public bool IsVisible<T>() where T : MonoBehaviour, IUIView
        {
            var uiId = typeof(T).Name;
            return _uiInstances.TryGetValue(uiId, out var view) && view.IsVisible;
        }

        #endregion

        #region 层级管理

        /// <summary>
        /// 设置UI的排序层级
        /// </summary>
        private void SetSortingOrder(IUIView view)
        {
            if (view == null) return;

            var order = _baseSortingOrder + _visibleUIStack.Count * _sortingOrderStep;

            // 尝试设置Canvas的SortingOrder
            var canvas = view.gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = view.gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;

            // 确保有Raycaster用于交互
            if (view.gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                view.gameObject.AddComponent<GraphicRaycaster>();
            }

            _currentMaxSortingOrder = order;
        }

        #endregion

        #region 模态管理

        /// <summary>
        /// 模态阻挡层（阻止下层UI交互）
        /// </summary>
        private GameObject _modalBlocker;

        /// <summary>
        /// 设置模态阻挡
        /// </summary>
        private void SetModalBlocking(bool enable)
        {
            if (enable)
            {
                if (_modalBlocker == null)
                {
                    _modalBlocker = new GameObject("ModalBlocker");
                    _modalBlocker.transform.SetParent(UIRoot, false);

                    // 全屏透明阻挡
                    var rect = _modalBlocker.AddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;

                    // 透明图片（用于接收点击）
                    var image = _modalBlocker.AddComponent<Image>();
                    image.color = new Color(0, 0, 0, 0);
                }

                _modalBlocker.SetActive(true);

                // 将阻挡层置于当前显示的模态UI之下
                if (_visibleUIStack.Count > 0)
                {
                    var lastVisible = _visibleUIStack[_visibleUIStack.Count - 1];
                    if (lastVisible?.gameObject != null)
                    {
                        _modalBlocker.transform.SetSiblingIndex(lastVisible.gameObject.transform.GetSiblingIndex());
                    }
                }
            }
            else if (_modalBlocker != null)
            {
                _modalBlocker.SetActive(false);
            }
        }

        /// <summary>
        /// 更新模态阻挡状态
        /// </summary>
        private void UpdateModalBlocking()
        {
            // 检查是否有模态UI正在显示
            bool hasModalVisible = false;
            foreach (var ui in _visibleUIStack)
            {
                if (ui == null) continue;
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

    /// <summary>
    /// UI配置数据
    /// </summary>
    [Serializable]
    public class UIConfig
    {
        public string UIId;                    // 唯一标识
        public GameObject Prefab;              // UI预制体
        public Transform Parent;               // 父节点（默认UIManager根节点）
        public bool CacheOnHide = true;        // 隐藏时是否缓存（不销毁）
        public bool IsModal = false;           // 是否为模态（阻止下层交互）
        public int CustomSortingOrder = -1;    // 自定义排序层级（-1表示使用默认计算）
    }
}
