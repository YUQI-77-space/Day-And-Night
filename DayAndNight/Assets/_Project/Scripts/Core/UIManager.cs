using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DayAndNight.Core
{
    /// <summary>
    /// UI面板类型枚举
    /// </summary>
    public enum UIPanelType
    {
        /// <summary>
        /// 无（默认值）
        /// </summary>
        None,

        /// <summary>
        /// 主菜单
        /// </summary>
        MainMenu,

        /// <summary>
        /// 游戏界面
        /// </summary>
        HUD,

        /// <summary>
        /// 暂停菜单
        /// </summary>
        PauseMenu,

        /// <summary>
        /// 背包界面
        /// </summary>
        Inventory,

        /// <summary>
        /// 设置菜单
        /// </summary>
        Settings,

        /// <summary>
        /// 加载界面
        /// </summary>
        Loading,

        /// <summary>
        /// 对话界面
        /// </summary>
        Dialogue,

        /// <summary>
        /// 提示信息
        /// </summary>
        Toast,

        /// <summary>
        /// 游戏结束界面
        /// </summary>
        GameOver
    }

    /// <summary>
    /// UI管理器，负责游戏中所有UI面板的显示和隐藏
    /// 支持面板栈管理、层级管理、面板切换动画等功能
    /// </summary>
    public class UIManager : BaseManager<UIManager>
    {
        #region 常量

        /// <summary>
        /// UI根物体名称
        /// </summary>
        private const string UI_ROOT_NAME = "UIRoot";

        /// <summary>
        /// 面板根物体名称
        /// </summary>
        private const string PANELS_ROOT_NAME = "Panels";

        /// <summary>
        /// 堆叠面板根物体名称
        /// </summary>
        private const string OVERLAY_ROOT_NAME = "Overlay";

        /// <summary>
        /// 默认面板层级
        /// </summary>
        private const float DEFAULT_PANEL_SORTING_ORDER = 100;

        #endregion

        #region 私有字段

        /// <summary>
        /// UI根物体
        /// </summary>
        private GameObject _uiRoot;

        /// <summary>
        /// 面板根物体
        /// </summary>
        private Transform _panelsRoot;

        /// <summary>
        /// 堆叠/模态面板根物体
        /// </summary>
        private Transform _overlayRoot;

        /// <summary>
        /// 已注册的面板字典
        /// </summary>
        private Dictionary<UIPanelType, UIBasePanel> _registeredPanels = new Dictionary<UIPanelType, UIBasePanel>();

        /// <summary>
        /// 当前显示的面板列表
        /// </summary>
        private List<UIBasePanel> _activePanels = new List<UIBasePanel>();

        /// <summary>
        /// 面板栈（用于模态面板管理）
        /// </summary>
        private Stack<UIBasePanel> _panelStack = new Stack<UIBasePanel>();

        /// <summary>
        /// 当前正在显示的模态面板
        /// </summary>
        private UIBasePanel _currentModalPanel;

        /// <summary>
        /// 是否允许输入
        /// </summary>
        private bool _inputEnabled = true;

        /// <summary>
        /// 暂停时的背景面板
        /// </summary>
        [SerializeField]
        private UIBasePanel _pauseMenuPanel;

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取当前是否允许输入
        /// </summary>
        public bool InputEnabled => _inputEnabled;

        /// <summary>
        /// 获取当前显示的面板数量
        /// </summary>
        public int ActivePanelCount => _activePanels.Count;

        /// <summary>
        /// 获取栈顶面板
        /// </summary>
        public UIBasePanel TopPanel => _panelStack.Count > 0 ? _panelStack.Peek() : null;

        /// <summary>
        /// 获取或设置暂停菜单面板
        /// </summary>
        public UIBasePanel PauseMenuPanel
        {
            get => _pauseMenuPanel;
            set => _pauseMenuPanel = value;
        }

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 唤醒时初始化UI系统
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // 创建UI根物体
            CreateUIRoot();

            // 订阅事件
            SubscribeEvents();
        }

        /// <summary>
        /// 销毁时清理
        /// </summary>
        protected override void OnDestroy()
        {
            // 取消订阅事件
            UnsubscribeEvents();

            // 清理所有面板
            ClearAllPanels();

            base.OnDestroy();
        }

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 执行初始化逻辑
        /// </summary>
        protected override void OnInitialize()
        {
            Debug.Log("[UIManager] 开始初始化...");

            // 注册所有面板
            RegisterAllPanels();

            Debug.Log($"[UIManager] 初始化完成，共注册 {_registeredPanels.Count} 个面板");
        }

        /// <summary>
        /// 执行关闭逻辑
        /// </summary>
        protected override void OnShutdown()
        {
            // 清理所有面板
            ClearAllPanels();

            Debug.Log("[UIManager] 已关闭");
        }

        #endregion

        #region 面板注册方法

        /// <summary>
        /// 注册UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <param name="panel">面板组件</param>
        /// <returns>是否注册成功</returns>
        public bool RegisterPanel(UIPanelType panelType, UIBasePanel panel)
        {
            if (panel == null)
            {
                Debug.LogWarning("[UIManager] 注册面板失败：面板为null");
                return false;
            }

            if (_registeredPanels.ContainsKey(panelType))
            {
                Debug.LogWarning($"[UIManager] 注册面板失败：{panelType} 已存在");
                return false;
            }

            // 设置面板父物体
            panel.transform.SetParent(_panelsRoot, false);

            // 初始化面板
            panel.Initialize(this);

            // 注册到字典
            _registeredPanels.Add(panelType, panel);

            // 初始时隐藏面板
            panel.Hide(false);

            Debug.Log($"[UIManager] 已注册面板: {panelType}");
            return true;
        }

        /// <summary>
        /// 注销UI面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>是否注销成功</returns>
        public bool UnregisterPanel(UIPanelType panelType)
        {
            if (!_registeredPanels.TryGetValue(panelType, out UIBasePanel panel))
            {
                Debug.LogWarning($"[UIManager] 注销面板失败：{panelType} 不存在");
                return false;
            }

            // 如果面板正在显示，先隐藏
            if (_activePanels.Contains(panel))
            {
                HidePanel(panelType);
            }

            // 从字典中移除
            _registeredPanels.Remove(panelType);

            Debug.Log($"[UIManager] 已注销面板: {panelType}");
            return true;
        }

        /// <summary>
        /// 获取面板
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="panelType">面板类型</param>
        /// <returns>面板组件</returns>
        public T GetPanel<T>(UIPanelType panelType) where T : UIBasePanel
        {
            if (_registeredPanels.TryGetValue(panelType, out UIBasePanel panel))
            {
                return panel as T;
            }
            return null;
        }

        /// <summary>
        /// 检查面板是否已注册
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>是否已注册</returns>
        public bool IsPanelRegistered(UIPanelType panelType)
        {
            return _registeredPanels.ContainsKey(panelType);
        }

        #endregion

        #region 面板显示方法

        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <param name="animate">是否使用动画</param>
        /// <returns>是否显示成功</returns>
        public bool ShowPanel(UIPanelType panelType, bool animate = true)
        {
            if (!_registeredPanels.TryGetValue(panelType, out UIBasePanel panel))
            {
                Debug.LogWarning($"[UIManager] 显示面板失败：{panelType} 未注册");
                return false;
            }

            if (_activePanels.Contains(panel))
            {
                // 面板已在显示中，将其移到最上层
                MovePanelToTop(panel);
                return true;
            }

            // 显示面板
            panel.Show(animate);

            // 添加到活跃列表
            _activePanels.Add(panel);

            // 设置排序层级
            SetPanelSortingOrder(panel);

            // 更新输入状态
            UpdateInputState();

            Debug.Log($"[UIManager] 显示面板: {panelType}");
            return true;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <param name="animate">是否使用动画</param>
        /// <returns>是否隐藏成功</returns>
        public bool HidePanel(UIPanelType panelType, bool animate = true)
        {
            if (!_registeredPanels.TryGetValue(panelType, out UIBasePanel panel))
            {
                Debug.LogWarning($"[UIManager] 隐藏面板失败：{panelType} 未注册");
                return false;
            }

            if (!_activePanels.Contains(panel))
            {
                return true;
            }

            // 隐藏面板
            panel.Hide(animate);

            // 从活跃列表中移除
            _activePanels.Remove(panel);

            // 更新输入状态
            UpdateInputState();

            Debug.Log($"[UIManager] 隐藏面板: {panelType}");
            return true;
        }

        /// <summary>
        /// 切换面板显示状态
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <param name="animate">是否使用动画</param>
        /// <returns>切换后的显示状态</returns>
        public bool TogglePanel(UIPanelType panelType, bool animate = true)
        {
            if (IsPanelActive(panelType))
            {
                HidePanel(panelType, animate);
                return false;
            }
            else
            {
                ShowPanel(panelType, animate);
                return true;
            }
        }

        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        /// <param name="animate">是否使用动画</param>
        public void HideAllPanels(bool animate = true)
        {
            // 复制列表以避免修改枚举问题
            var panelsToHide = new List<UIBasePanel>(_activePanels);

            foreach (var panel in panelsToHide)
            {
                panel.Hide(animate);
            }

            _activePanels.Clear();
            _panelStack.Clear();
            _currentModalPanel = null;

            UpdateInputState();

            Debug.Log("[UIManager] 已隐藏所有面板");
        }

        /// <summary>
        /// 隐藏除指定面板外的所有面板
        /// </summary>
        /// <param name="exceptPanelType">例外面板类型</param>
        /// <param name="animate">是否使用动画</param>
        public void HideAllPanelsExcept(UIPanelType exceptPanelType, bool animate = true)
        {
            UIBasePanel exceptPanel = null;
            if (_registeredPanels.TryGetValue(exceptPanelType, out UIBasePanel panel))
            {
                exceptPanel = panel;
            }

            var panelsToHide = new List<UIBasePanel>(_activePanels);

            foreach (var p in panelsToHide)
            {
                if (p != exceptPanel)
                {
                    p.Hide(animate);
                }
            }

            _activePanels.Clear();

            if (exceptPanel != null)
            {
                _activePanels.Add(exceptPanel);
                SetPanelSortingOrder(exceptPanel);
            }

            _panelStack.Clear();
            _currentModalPanel = null;

            UpdateInputState();

            Debug.Log($"[UIManager] 已隐藏除 {exceptPanelType} 外的所有面板");
        }

        #endregion

        #region 面板栈方法

        /// <summary>
        /// 压入面板（模态显示）
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <param name="animate">是否使用动画</param>
        /// <returns>是否成功</returns>
        public bool PushPanel(UIPanelType panelType, bool animate = true)
        {
            if (!_registeredPanels.TryGetValue(panelType, out UIBasePanel panel))
            {
                Debug.LogWarning($"[UIManager] 压入面板失败：{panelType} 未注册");
                return false;
            }

            // 如果栈顶有面板，将其移到非模态显示
            if (_panelStack.Count > 0)
            {
                UIBasePanel topPanel = _panelStack.Peek();
                topPanel.SetModalState(false);
            }

            // 显示新面板
            panel.Show(animate);
            panel.SetModalState(true);

            // 压入栈
            _panelStack.Push(panel);

            // 添加到活跃列表
            if (!_activePanels.Contains(panel))
            {
                _activePanels.Add(panel);
            }

            // 设置最高排序层级
            SetPanelSortingOrder(panel, _panelStack.Count);

            // 更新输入状态
            UpdateInputState();

            _currentModalPanel = panel;

            Debug.Log($"[UIManager] 压入面板: {panelType}");
            return true;
        }

        /// <summary>
        /// 弹出面板
        /// </summary>
        /// <param name="animate">是否使用动画</param>
        /// <returns>弹出的面板类型</returns>
        public UIPanelType PopPanel(bool animate = true)
        {
            if (_panelStack.Count == 0)
            {
                Debug.LogWarning("[UIManager] 弹出面板失败：栈为空");
                return UIPanelType.None;
            }

            // 弹出栈顶面板
            UIBasePanel panel = _panelStack.Pop();

            // 隐藏面板
            panel.Hide(animate);
            panel.SetModalState(false);

            // 从活跃列表中移除
            _activePanels.Remove(panel);

            // 如果栈还有面板，将新的栈顶设为模态
            if (_panelStack.Count > 0)
            {
                UIBasePanel newTopPanel = _panelStack.Peek();
                newTopPanel.SetModalState(true);
                SetPanelSortingOrder(newTopPanel, _panelStack.Count);
            }

            // 更新输入状态
            UpdateInputState();

            _currentModalPanel = _panelStack.Count > 0 ? _panelStack.Peek() : null;

            UIPanelType panelType = panel.PanelType;
            Debug.Log($"[UIManager] 弹出面板: {panelType}");
            return panelType;
        }

        /// <summary>
        /// 清空面板栈
        /// </summary>
        /// <param name="animate">是否使用动画</param>
        public void ClearPanelStack(bool animate = true)
        {
            while (_panelStack.Count > 0)
            {
                PopPanel(animate);
            }
            Debug.Log("[UIManager] 已清空面板栈");
        }

        #endregion

        #region 面板状态查询

        /// <summary>
        /// 检查面板是否正在显示
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>是否正在显示</returns>
        public bool IsPanelActive(UIPanelType panelType)
        {
            if (_registeredPanels.TryGetValue(panelType, out UIBasePanel panel))
            {
                return _activePanels.Contains(panel);
            }
            return false;
        }

        /// <summary>
        /// 获取所有活跃面板
        /// </summary>
        /// <returns>活跃面板列表</returns>
        public IReadOnlyList<UIBasePanel> GetActivePanels()
        {
            return _activePanels.AsReadOnly();
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 处理暂停菜单
        /// </summary>
        public void TogglePauseMenu()
        {
            if (IsPanelActive(UIPanelType.PauseMenu))
            {
                HidePanel(UIPanelType.PauseMenu);
                ResumeGame();
            }
            else
            {
                ShowPanel(UIPanelType.PauseMenu);
                PauseGame();
            }
        }

        /// <summary>
        /// 处理返回按钮
        /// </summary>
        public void HandleBackButton()
        {
            if (_panelStack.Count > 0)
            {
                // 如果有模态面板，弹出它
                PopPanel();
            }
            else if (IsPanelActive(UIPanelType.PauseMenu))
            {
                // 如果显示着暂停菜单，关闭它
                TogglePauseMenu();
            }
            else if (IsPanelActive(UIPanelType.Dialogue))
            {
                // 如果显示着对话框，关闭它
                HidePanel(UIPanelType.Dialogue);
            }
            // 其他情况可以根据需要扩展
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建UI根物体
        /// </summary>
        private void CreateUIRoot()
        {
            // 检查是否已存在UI根物体
            _uiRoot = GameObject.Find(UI_ROOT_NAME);

            if (_uiRoot == null)
            {
                // 创建UI根物体
                _uiRoot = new GameObject(UI_ROOT_NAME);
                DontDestroyOnLoad(_uiRoot);

                // 添加Canvas
                Canvas canvas = _uiRoot.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;

                // 添加CanvasScaler
                CanvasScaler scaler = _uiRoot.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                // 添加GraphicRaycaster
                _uiRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                // 创建面板根物体
                _panelsRoot = new GameObject(PANELS_ROOT_NAME).transform;
                _panelsRoot.SetParent(_uiRoot.transform, false);

                // 创建堆叠面板根物体
                _overlayRoot = new GameObject(OVERLAY_ROOT_NAME).transform;
                _overlayRoot.SetParent(_uiRoot.transform, false);

                Debug.Log($"[UIManager] 已创建UI根物体");
            }
            else
            {
                // 获取现有物体
                _panelsRoot = _uiRoot.transform.Find(PANELS_ROOT_NAME);
                _overlayRoot = _uiRoot.transform.Find(OVERLAY_ROOT_NAME);

                if (_panelsRoot == null)
                {
                    _panelsRoot = new GameObject(PANELS_ROOT_NAME).transform;
                    _panelsRoot.SetParent(_uiRoot.transform, false);
                }

                if (_overlayRoot == null)
                {
                    _overlayRoot = new GameObject(OVERLAY_ROOT_NAME).transform;
                    _overlayRoot.SetParent(_uiRoot.transform, false);
                }
            }
        }

        /// <summary>
        /// 注册所有面板
        /// </summary>
        private void RegisterAllPanels()
        {
            // 在Resources中查找所有UIBasePanel预制体并注册
            UIBasePanel[] panels = Resources.LoadAll<UIBasePanel>("UI/Panels");

            foreach (var panel in panels)
            {
                // 创建面板实例
                GameObject panelObj = Instantiate(panel.gameObject, _panelsRoot);
                UIBasePanel panelComp = panelObj.GetComponent<UIPanel>();

                if (panelComp != null)
                {
                    RegisterPanel(panelComp.PanelType, panelComp);
                }
                else
                {
                    Debug.LogWarning($"[UIManager] 面板 {panel.name} 没有UIPanel组件");
                    Destroy(panelObj);
                }
            }

            // 查找场景中的面板
            UIBasePanel[] scenePanels = FindObjectsOfType<UIPanel>();

            foreach (var panel in scenePanels)
            {
                if (!IsPanelRegistered(panel.PanelType))
                {
                    RegisterPanel(panel.PanelType, panel);
                }
            }
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeEvents()
        {
            // 可以在这里订阅其他系统的事件
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeEvents()
        {
            // 取消订阅事件
        }

        /// <summary>
        /// 清理所有面板
        /// </summary>
        private void ClearAllPanels()
        {
            HideAllPanels(false);

            foreach (var kvp in _registeredPanels)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }

            _registeredPanels.Clear();
        }

        /// <summary>
        /// 设置面板排序层级
        /// </summary>
        private void SetPanelSortingOrder(UIBasePanel panel, int stackDepth = 0)
        {
            Canvas canvas = panel.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = (int)(DEFAULT_PANEL_SORTING_ORDER + _activePanels.Count + stackDepth);
            }

            // 确保面板在最上层
            panel.transform.SetAsLastSibling();
        }

        /// <summary>
        /// 将面板移到最上层
        /// </summary>
        private void MovePanelToTop(UIBasePanel panel)
        {
            _activePanels.Remove(panel);
            _activePanels.Add(panel);
            SetPanelSortingOrder(panel);
        }

        /// <summary>
        /// 更新输入状态
        /// </summary>
        private void UpdateInputState()
        {
            // 如果有活跃的模态面板，禁用输入
            _inputEnabled = _panelStack.Count == 0 && !IsPanelActive(UIPanelType.Loading);
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        private void PauseGame()
        {
            Time.timeScale = 0f;
            EventManager.Instance.TriggerEvent(CoreEvents.GAME_PAUSED);
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        private void ResumeGame()
        {
            Time.timeScale = 1f;
            EventManager.Instance.TriggerEvent(CoreEvents.GAME_RESUMED);
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 获取UIManager实例
        /// </summary>
        public static UIManager Get()
        {
            return Instance;
        }

        #endregion
    }
}
