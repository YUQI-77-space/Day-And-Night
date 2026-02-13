using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// 初始状态
        /// </summary>
        None,

        /// <summary>
        /// 初始化中
        /// </summary>
        Initializing,

        /// <summary>
        /// 主菜单
        /// </summary>
        MainMenu,

        /// <summary>
        /// 游戏中
        /// </summary>
        Playing,

        /// <summary>
        /// 暂停中
        /// </summary>
        Paused,

        /// <summary>
        /// 保存中
        /// </summary>
        Saving,

        /// <summary>
        /// 加载中
        /// </summary>
        Loading,

        /// <summary>
        /// 游戏结束
        /// </summary>
        GameOver,

        /// <summary>
        /// 退出中
        /// </summary>
        Quitting
    }

    /// <summary>
    /// 游戏主管理器
    /// 负责整个游戏的生命周期管理、系统初始化和状态控制
    /// </summary>
    public class GameManager : BaseManager<GameManager>
    {
        #region 私有字段

        /// <summary>
        /// 当前游戏状态
        /// </summary>
        private GameState _currentState = GameState.None;

        /// <summary>
        /// 上一帧的游戏状态
        /// </summary>
        private GameState _previousState = GameState.None;

        /// <summary>
        /// 当前存档槽位
        /// </summary>
        private int _currentSaveSlot = 0;

        /// <summary>
        /// 游戏是否已初始化
        /// </summary>
        private bool _isGameStarted = false;

        /// <summary>
        /// 退出确认是否已显示
        /// </summary>
        private bool _quitConfirmationShown = false;

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取当前游戏状态
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// 获取上一帧的游戏状态
        /// </summary>
        public GameState PreviousState => _previousState;

        /// <summary>
        /// 获取当前存档槽位
        /// </summary>
        public int CurrentSaveSlot => _currentSaveSlot;

        /// <summary>
        /// 获取是否在主菜单
        /// </summary>
        public bool IsInMainMenu => _currentState == GameState.MainMenu;

        /// <summary>
        /// 获取是否在游戏中
        /// </summary>
        public bool IsPlaying => _currentState == GameState.Playing;

        /// <summary>
        /// 获取是否已暂停
        /// </summary>
        public bool IsPaused => _currentState == GameState.Paused;

        /// <summary>
        /// 获取是否正在加载
        /// </summary>
        public bool IsLoading => _currentState == GameState.Loading;

        /// <summary>
        /// 获取游戏是否已启动
        /// </summary>
        public bool IsGameStarted => _isGameStarted;

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 唤醒时初始化游戏管理器
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
        }

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 执行初始化逻辑
        /// </summary>
        protected override void OnInitialize()
        {
            Debug.Log("[GameManager] 开始初始化...");

            // 初始化各系统
            InitializeSystems();

            // 进入主菜单
            EnterMainMenu();

            _isGameStarted = true;

            Debug.Log("[GameManager] 初始化完成");
        }

        /// <summary>
        /// 执行关闭逻辑
        /// </summary>
        protected override void OnShutdown()
        {
            Debug.Log("[GameManager] 正在关闭...");

            // 关闭各系统
            ShutdownSystems();

            Debug.Log("[GameManager] 已关闭");
        }

        #endregion

        #region 公共方法 - 游戏状态控制

        /// <summary>
        /// 开始新游戏
        /// </summary>
        /// <param name="saveSlot">存档槽位</param>
        public void StartGame(int saveSlot = 0)
        {
            if (_currentState == GameState.Initializing)
            {
                Debug.LogWarning("[GameManager] 正在初始化中，无法开始游戏");
                return;
            }

            Debug.Log($"[GameManager] 开始新游戏，槽位: {saveSlot}");
            _currentSaveSlot = saveSlot;

            // 切换到加载状态
            ChangeState(GameState.Loading);

            // 进入游戏场景
            EnterGameplay();
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        /// <param name="saveSlot">存档槽位</param>
        public void ContinueGame(int saveSlot = 0)
        {
            if (_currentState == GameState.Initializing)
            {
                Debug.LogWarning("[GameManager] 正在初始化中，无法继续游戏");
                return;
            }

            Debug.Log($"[GameManager] 继续游戏，槽位: {saveSlot}");
            _currentSaveSlot = saveSlot;

            // 切换到加载状态
            ChangeState(GameState.Loading);

            // 加载存档
            LoadGame(saveSlot);
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (_currentState != GameState.Playing)
            {
                Debug.LogWarning("[GameManager] 当前状态无法暂停游戏");
                return;
            }

            Debug.Log("[GameManager] 暂停游戏");
            ChangeState(GameState.Paused);
            Time.timeScale = 0f;

            // 显示暂停菜单
            ShowPauseMenu();
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState != GameState.Paused)
            {
                Debug.LogWarning("[GameManager] 当前状态无法恢复游戏");
                return;
            }

            Debug.Log("[GameManager] 恢复游戏");
            ChangeState(GameState.Playing);
            Time.timeScale = 1f;

            // 隐藏暂停菜单
            HidePauseMenu();
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        /// <param name="saveSlot">存档槽位</param>
        public void SaveGame(int saveSlot = -1)
        {
            if (_currentState != GameState.Playing && _currentState != GameState.Paused)
            {
                Debug.LogWarning("[GameManager] 当前状态无法保存游戏");
                return;
            }

            int slot = saveSlot >= 0 ? saveSlot : _currentSaveSlot;

            Debug.Log($"[GameManager] 保存游戏，槽位: {slot}");
            ChangeState(GameState.Saving);

            if (SaveManager.Instance != null)
            {
                bool success = SaveManager.Instance.SaveGame(slot);
                if (success)
                {
                    _currentSaveSlot = slot;
                    Debug.Log($"[GameManager] 保存成功，槽位: {slot}");
                }
            }

            // 恢复之前的状态
            ChangeState(Time.timeScale == 0 ? GameState.Paused : GameState.Playing);
        }

        /// <summary>
        /// 加载游戏
        /// </summary>
        /// <param name="saveSlot">存档槽位</param>
        private void LoadGame(int saveSlot)
        {
            if (SaveManager.Instance != null)
            {
                bool success = SaveManager.Instance.LoadGame(saveSlot);
                if (success)
                {
                    _currentSaveSlot = saveSlot;
                    Debug.Log($"[GameManager] 加载成功，槽位: {saveSlot}");
                }
            }

            // 进入游戏
            EnterGameplay();
        }

        /// <summary>
        /// 重新开始游戏
        /// </summary>
        public void RestartGame()
        {
            Debug.Log("[GameManager] 重新开始游戏");

            // 退出当前游戏
            ExitGameplay();

            // 开始新游戏
            StartGame(_currentSaveSlot);
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[GameManager] 返回主菜单");

            // 退出当前游戏
            ExitGameplay();

            // 进入主菜单
            EnterMainMenu();
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            if (_quitConfirmationShown)
            {
                return;
            }

            Debug.Log("[GameManager] 退出游戏");
            ChangeState(GameState.Quitting);

            // 触发退出事件
            EventManager.Instance.TriggerEvent(CoreEvents.GAME_QUIT);

#if UNITY_EDITOR
            // 编辑器模式下停止播放
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // 退出应用程序
            Application.Quit();
#endif
        }

        #endregion

        #region 公共方法 - 状态查询

        /// <summary>
        /// 检查是否在指定状态
        /// </summary>
        /// <param name="state">游戏状态</param>
        /// <returns>是否在指定状态</returns>
        public bool IsInState(GameState state)
        {
            return _currentState == state;
        }

        /// <summary>
        /// 检查是否可以进行指定操作
        /// </summary>
        /// <param name="action">操作类型</param>
        /// <returns>是否可以操作</returns>
        public bool CanPerformAction(GameAction action)
        {
            switch (action)
            {
                case GameAction.Pause:
                    return _currentState == GameState.Playing;

                case GameAction.Save:
                    return _currentState == GameState.Playing || _currentState == GameState.Paused;

                case GameAction.Load:
                    return !IsLoading;

                case GameAction.OpenMenu:
                    return _currentState == GameState.Playing;

                case GameAction.Quit:
                    return !_quitConfirmationShown;

                default:
                    return false;
            }
        }

        #endregion

        #region 私有方法 - 系统初始化

        /// <summary>
        /// 初始化各系统
        /// </summary>
        private void InitializeSystems()
        {
            Debug.Log("[GameManager] 初始化各系统...");

            // 各系统的初始化顺序：
            // 1. EventManager - 无依赖，最先初始化
            // 2. DataManager - 无依赖
            // 3. TimeManager - 依赖DataManager
            // 4. AudioManager - 依赖DataManager
            // 5. SaveManager - 依赖DataManager、EventManager
            // 6. SceneLoader - 依赖EventManager
            // 7. UIManager - 依赖EventManager、SceneLoader

            // 所有管理器已经在BaseManager中按顺序初始化
            // 这里可以添加额外的初始化逻辑

            Debug.Log("[GameManager] 各系统初始化完成");
        }

        /// <summary>
        /// 关闭各系统
        /// </summary>
        private void ShutdownSystems()
        {
            Debug.Log("[GameManager] 关闭各系统...");

            // 按相反顺序关闭系统
            // ...

            Debug.Log("[GameManager] 各系统已关闭");
        }

        #endregion

        #region 私有方法 - 状态转换

        /// <summary>
        /// 切换游戏状态
        /// </summary>
        private void ChangeState(GameState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            _previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameManager] 状态变更: {_previousState} -> {_currentState}");

            // 触发状态变更事件
            OnGameStateChanged(_previousState, _currentState);
        }

        /// <summary>
        /// 状态变更回调
        /// </summary>
        private void OnGameStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.MainMenu:
                    OnEnterMainMenu();
                    break;

                case GameState.Playing:
                    OnEnterPlaying();
                    break;

                case GameState.Paused:
                    OnEnterPaused();
                    break;

                case GameState.GameOver:
                    OnEnterGameOver();
                    break;
            }
        }

        /// <summary>
        /// 进入主菜单
        /// </summary>
        private void EnterMainMenu()
        {
            Debug.Log("[GameManager] 进入主菜单");

            // 切换状态
            ChangeState(GameState.MainMenu);

            // 确保时间正常流动
            Time.timeScale = 1f;

            // 显示主菜单
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideAllPanels(false);
                UIManager.Instance.ShowPanel(UIPanelType.MainMenu);
            }

            // 播放主菜单音乐
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic("MainMenu", true, true);
            }

            // 触发事件
            EventManager.Instance.TriggerEvent(CoreEvents.GAME_STARTED);
        }

        /// <summary>
        /// 进入游戏
        /// </summary>
        private void EnterGameplay()
        {
            Debug.Log("[GameManager] 进入游戏");

            // 切换状态
            ChangeState(GameState.Playing);

            // 确保时间正常流动
            Time.timeScale = 1f;

            // 隐藏主菜单，显示HUD
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideAllPanels(false);
                UIManager.Instance.ShowPanel(UIPanelType.HUD);
            }

            // 加载游戏场景
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(CoreConfig.GAMEPLAY_SCENE);
            }

            // 播放游戏音乐
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic("Gameplay", true, true);
            }

            // 触发事件
            EventManager.Instance.TriggerEvent(CoreEvents.GAME_STARTED);
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        private void ExitGameplay()
        {
            Debug.Log("[GameManager] 退出游戏");

            // 隐藏所有UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideAllPanels(false);
            }

            // 停止背景音乐
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic();
            }

            // 恢复时间
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 显示暂停菜单
        /// </summary>
        private void ShowPauseMenu()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPanel(UIPanelType.PauseMenu);
            }
        }

        /// <summary>
        /// 隐藏暂停菜单
        /// </summary>
        private void HidePauseMenu()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HidePanel(UIPanelType.PauseMenu);
            }
        }

        #endregion

        #region 状态变更回调

        /// <summary>
        /// 进入主菜单回调
        /// </summary>
        private void OnEnterMainMenu()
        {
            Debug.Log("[GameManager] 已进入主菜单");
        }

        /// <summary>
        /// 进入游戏中回调
        /// </summary>
        private void OnEnterPlaying()
        {
            Debug.Log("[GameManager] 已进入游戏");
        }

        /// <summary>
        /// 进入暂停回调
        /// </summary>
        private void OnEnterPaused()
        {
            Debug.Log("[GameManager] 已暂停游戏");
        }

        /// <summary>
        /// 进入游戏结束回调
        /// </summary>
        private void OnEnterGameOver()
        {
            Debug.Log("[GameManager] 游戏结束");

            // 显示游戏结束界面
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPanel(UIPanelType.GameOver);
            }
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 每帧更新
        /// </summary>
        protected override void OnUpdate()
        {
            // 处理暂停键
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Pause))
            {
                HandleEscapeKey();
            }
        }

        /// <summary>
        /// 处理Escape键
        /// </summary>
        private void HandleEscapeKey()
        {
            // 如果在主菜单，不处理
            if (_currentState == GameState.MainMenu)
            {
                return;
            }

            // 如果正在加载，不处理
            if (_currentState == GameState.Loading)
            {
                return;
            }

            // 如果正在保存，不处理
            if (_currentState == GameState.Saving)
            {
                return;
            }

            // 如果正在退出，不处理
            if (_currentState == GameState.Quitting)
            {
                return;
            }

            // 切换暂停状态
            if (_currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (_currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        #endregion

        #region Unity回调

        /// <summary>
        /// 当应用失去焦点
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && _currentState == GameState.Playing && !_isGameStarted)
            {
                // 游戏未启动时失去焦点，不处理
                return;
            }

            if (!hasFocus && _currentState == GameState.Playing)
            {
                // 可选：自动暂停
                // PauseGame();
            }
        }

        /// <summary>
        /// 当应用退出
        /// </summary>
        private void OnApplicationQuit()
        {
            if (_currentState != GameState.Quitting)
            {
                QuitGame();
            }
        }

        #endregion
    }

    /// <summary>
    /// 游戏操作类型
    /// </summary>
    public enum GameAction
    {
        /// <summary>
        /// 暂停
        /// </summary>
        Pause,

        /// <summary>
        /// 保存
        /// </summary>
        Save,

        /// <summary>
        /// 加载
        /// </summary>
        Load,

        /// <summary>
        /// 打开菜单
        /// </summary>
        OpenMenu,

        /// <summary>
        /// 退出
        /// </summary>
        Quit
    }
}
