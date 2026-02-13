using System;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 玩家状态枚举
    /// </summary>
    public enum PlayerState
    {
        /// <summary>
        /// 正常状态
        /// </summary>
        Normal,

        /// <summary>
        /// 战斗状态
        /// </summary>
        Combat,

        /// <summary>
        /// 对话状态
        /// </summary>
        Dialogue,

        /// <summary>
        /// 菜单状态
        /// </summary>
        Menu,

        /// <summary>
        /// 眩晕状态
        /// </summary>
        Stunned,

        /// <summary>
        /// 冰冻状态
        /// </summary>
        Frozen,

        /// <summary>
        /// 中毒状态
        /// </summary>
        Poisoned,

        /// <summary>
        /// 死亡状态
        /// </summary>
        Dead,

        /// <summary>
        /// 无敌状态
        /// </summary>
        Invincible
    }

    /// <summary>
    /// 游戏模式枚举
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// 探索模式
        /// </summary>
        Exploration,

        /// <summary>
        /// 战斗模式
        /// </summary>
        Combat,

        /// <summary>
        /// 剧情模式
        /// </summary>
        Story,

        /// <summary>
        /// 菜单模式
        /// </summary>
        Menu
    }

    /// <summary>
    /// 难度等级枚举
    /// </summary>
    public enum DifficultyLevel
    {
        /// <summary>
        /// 简单
        /// </summary>
        Easy,

        /// <summary>
        /// 普通
        /// </summary>
        Normal,

        /// <summary>
        /// 困难
        /// </summary>
        Hard,

        /// <summary>
        /// 专家
        /// </summary>
        Expert
    }

    /// <summary>
    /// 游戏状态管理器
    /// 负责管理游戏状态、玩家状态、难度设置等
    /// </summary>
    public class GameStateManager : BaseManager<GameStateManager>
    {
        #region 常量

        /// <summary>
        /// 难度倍率 - 简单
        /// </summary>
        private const float EASY_MULTIPLIER = 0.75f;

        /// <summary>
        /// 难度倍率 - 普通
        /// </summary>
        private const float NORMAL_MULTIPLIER = 1.0f;

        /// <summary>
        /// 难度倍率 - 困难
        /// </summary>
        private const float HARD_MULTIPLIER = 1.5f;

        /// <summary>
        /// 难度倍率 - 专家
        /// </summary>
        private const float EXPERT_MULTIPLIER = 2.0f;

        #endregion

        #region 私有字段

        /// <summary>
        /// 当前玩家状态
        /// </summary>
        private PlayerState _currentPlayerState = PlayerState.Normal;

        /// <summary>
        /// 玩家状态堆栈（用于状态恢复）
        /// </summary>
        private Stack<PlayerState> _playerStateStack = new Stack<PlayerState>();

        /// <summary>
        /// 当前游戏模式
        /// </summary>
        private GameMode _currentGameMode = GameMode.Exploration;

        /// <summary>
        /// 当前难度等级
        /// </summary>
        private DifficultyLevel _currentDifficulty = DifficultyLevel.Normal;

        /// <summary>
        /// 当前难度倍率
        /// </summary>
        private float _difficultyMultiplier = NORMAL_MULTIPLIER;

        /// <summary>
        /// 是否为时间停止状态
        /// </summary>
        private bool _isTimeStopped = false;

        /// <summary>
        /// 是否为无敌状态
        /// </summary>
        private bool _isGodMode = false;

        /// <summary>
        /// 是否为开发者模式
        /// </summary>
        private bool _isDevMode = false;

        /// <summary>
        /// 当前剧情进度
        /// </summary>
        private int _storyProgress = 0;

        /// <summary>
        /// 游戏开始时间
        /// </summary>
        private DateTime _gameStartTime;

        /// <summary>
        /// 总游戏时间
        /// </summary>
        private TimeSpan _totalPlayTime;

        /// <summary>
        /// 玩家当前生命值
        /// </summary>
        private float _playerHealth = 100f;

        /// <summary>
        /// 玩家最大生命值
        /// </summary>
        private float _playerMaxHealth = 100f;

        /// <summary>
        /// 玩家当前魔法值
        /// </summary>
        private float _playerMana = 50f;

        /// <summary>
        /// 玩家最大魔法值
        /// </summary>
        private float _playerMaxMana = 50f;

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取当前玩家状态
        /// </summary>
        public PlayerState CurrentPlayerState => _currentPlayerState;

        /// <summary>
        /// 获取当前游戏模式
        /// </summary>
        public GameMode CurrentGameMode => _currentGameMode;

        /// <summary>
        /// 获取当前难度等级
        /// </summary>
        public DifficultyLevel CurrentDifficulty => _currentDifficulty;

        /// <summary>
        /// 获取当前难度倍率
        /// </summary>
        public float DifficultyMultiplier => _difficultyMultiplier;

        /// <summary>
        /// 获取是否为时间停止状态
        /// </summary>
        public bool IsTimeStopped => _isTimeStopped;

        /// <summary>
        /// 获取是否为无敌状态
        /// </summary>
        public bool IsGodMode => _isGodMode;

        /// <summary>
        /// 获取是否为开发者模式
        /// </summary>
        public bool IsDevMode => _isDevMode;

        /// <summary>
        /// 获取当前剧情进度
        /// </summary>
        public int StoryProgress => _storyProgress;

        /// <summary>
        /// 获取总游戏时间
        /// </summary>
        public TimeSpan TotalPlayTime => _totalPlayTime;

        /// <summary>
        /// 获取玩家当前生命值
        /// </summary>
        public float PlayerHealth => _playerHealth;

        /// <summary>
        /// 获取玩家最大生命值
        /// </summary>
        public float PlayerMaxHealth => _playerMaxHealth;

        /// <summary>
        /// 获取玩家当前生命百分比
        /// </summary>
        public float PlayerHealthPercent => _playerMaxHealth > 0 ? _playerHealth / _playerMaxHealth : 0f;

        /// <summary>
        /// 获取玩家当前魔法值
        /// </summary>
        public float PlayerMana => _playerMana;

        /// <summary>
        /// 获取玩家最大魔法值
        /// </summary>
        public float PlayerMaxMana => _playerMaxMana;

        /// <summary>
        /// 获取玩家当前魔法百分比
        /// </summary>
        public float PlayerManaPercent => _playerMaxMana > 0 ? _playerMana / _playerMaxMana : 0f;

        /// <summary>
        /// 获取是否为战斗模式
        /// </summary>
        public bool IsCombatMode => _currentGameMode == GameMode.Combat;

        /// <summary>
        /// 获取是否为探索模式
        /// </summary>
        public bool IsExplorationMode => _currentGameMode == GameMode.Exploration;

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 执行初始化逻辑
        /// </summary>
        protected override void OnInitialize()
        {
            Debug.Log("[GameStateManager] 开始初始化...");

            // 初始化游戏时间
            _gameStartTime = DateTime.Now;
            _totalPlayTime = TimeSpan.Zero;

            // 初始化难度
            UpdateDifficultyMultiplier();

            // 初始化状态
            _currentPlayerState = PlayerState.Normal;
            _currentGameMode = GameMode.Exploration;

            Debug.Log($"[GameStateManager] 初始化完成，难度: {_currentDifficulty}");
        }

        /// <summary>
        /// 执行关闭逻辑
        /// </summary>
        protected override void OnShutdown()
        {
            // 更新总游戏时间
            UpdateTotalPlayTime();
            Debug.Log($"[GameStateManager] 已关闭，总游戏时间: {_totalPlayTime}");
        }

        #endregion

        #region 公共方法 - 玩家状态管理

        /// <summary>
        /// 切换玩家状态
        /// </summary>
        /// <param name="newState">新状态</param>
        public void SetPlayerState(PlayerState newState)
        {
            if (_currentPlayerState == newState)
            {
                return;
            }

            Debug.Log($"[GameStateManager] 玩家状态变更: {_currentPlayerState} -> {newState}");
            _currentPlayerState = newState;

            // 触发状态变更事件
            EventManager.Instance.TriggerEvent(CoreEvents.PLAYER_STATE_CHANGED, new PlayerStateEventArgs(newState));
        }

        /// <summary>
        /// 推送玩家状态（用于临时状态）
        /// </summary>
        /// <param name="tempState">临时状态</param>
        public void PushPlayerState(PlayerState tempState)
        {
            _playerStateStack.Push(_currentPlayerState);
            SetPlayerState(tempState);
        }

        /// <summary>
        /// 弹出玩家状态
        /// </summary>
        public void PopPlayerState()
        {
            if (_playerStateStack.Count > 0)
            {
                PlayerState previousState = _playerStateStack.Pop();
                SetPlayerState(previousState);
            }
        }

        /// <summary>
        /// 检查是否为指定状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns>是否为指定状态</returns>
        public bool IsPlayerInState(PlayerState state)
        {
            return _currentPlayerState == state;
        }

        /// <summary>
        /// 检查是否可以进行动作
        /// </summary>
        /// <param name="action">动作类型</param>
        /// <returns>是否可以动作</returns>
        public bool CanPlayerAct()
        {
            // 某些状态下玩家无法行动
            switch (_currentPlayerState)
            {
                case PlayerState.Dead:
                case PlayerState.Frozen:
                case PlayerState.Stunned:
                    return false;
                default:
                    return true;
            }
        }

        #endregion

        #region 公共方法 - 游戏模式管理

        /// <summary>
        /// 设置游戏模式
        /// </summary>
        /// <param name="newMode">新模式</param>
        public void SetGameMode(GameMode newMode)
        {
            if (_currentGameMode == newMode)
            {
                return;
            }

            Debug.Log($"[GameStateManager] 游戏模式变更: {_currentGameMode} -> {newMode}");
            _currentGameMode = newMode;

            // 触发模式变更事件
            EventManager.Instance.TriggerEvent(CoreEvents.GAME_MODE_CHANGED, new GameModeEventArgs(newMode));
        }

        /// <summary>
        /// 进入战斗模式
        /// </summary>
        public void EnterCombatMode()
        {
            SetGameMode(GameMode.Combat);
        }

        /// <summary>
        /// 退出战斗模式
        /// </summary>
        public void ExitCombatMode()
        {
            SetGameMode(GameMode.Exploration);
        }

        /// <summary>
        /// 检查是否为指定模式
        /// </summary>
        /// <param name="mode">模式</param>
        /// <returns>是否为指定模式</returns>
        public bool IsInGameMode(GameMode mode)
        {
            return _currentGameMode == mode;
        }

        #endregion

        #region 公共方法 - 难度管理

        /// <summary>
        /// 设置难度等级
        /// </summary>
        /// <param name="difficulty">难度等级</param>
        public void SetDifficulty(DifficultyLevel difficulty)
        {
            if (_currentDifficulty == difficulty)
            {
                return;
            }

            Debug.Log($"[GameStateManager] 难度变更: {_currentDifficulty} -> {difficulty}");
            _currentDifficulty = difficulty;
            UpdateDifficultyMultiplier();

            // 触发难度变更事件
            EventManager.Instance.TriggerEvent(CoreEvents.DIFFICULTY_CHANGED, new DifficultyEventArgs(difficulty));
        }

        /// <summary>
        /// 更新难度倍率
        /// </summary>
        private void UpdateDifficultyMultiplier()
        {
            switch (_currentDifficulty)
            {
                case DifficultyLevel.Easy:
                    _difficultyMultiplier = EASY_MULTIPLIER;
                    break;
                case DifficultyLevel.Normal:
                    _difficultyMultiplier = NORMAL_MULTIPLIER;
                    break;
                case DifficultyLevel.Hard:
                    _difficultyMultiplier = HARD_MULTIPLIER;
                    break;
                case DifficultyLevel.Expert:
                    _difficultyMultiplier = EXPERT_MULTIPLIER;
                    break;
            }
        }

        /// <summary>
        /// 获取难度名称
        /// </summary>
        /// <returns>难度名称</returns>
        public string GetDifficultyName()
        {
            switch (_currentDifficulty)
            {
                case DifficultyLevel.Easy:
                    return "简单";
                case DifficultyLevel.Normal:
                    return "普通";
                case DifficultyLevel.Hard:
                    return "困难";
                case DifficultyLevel.Expert:
                    return "专家";
                default:
                    return "未知";
            }
        }

        #endregion

        #region 公共方法 - 特殊状态

        /// <summary>
        /// 设置时间停止状态
        /// </summary>
        /// <param name="stopped">是否停止</param>
        public void SetTimeStopped(bool stopped)
        {
            if (_isTimeStopped == stopped)
            {
                return;
            }

            _isTimeStopped = stopped;
            Debug.Log($"[GameStateManager] 时间停止: {_isTimeStopped}");

            // 设置时间缩放
            Time.timeScale = _isTimeStopped ? 0f : 1f;

            // 触发事件
            EventManager.Instance.TriggerEvent(CoreEvents.TIME_STOPPED_CHANGED, new TimeStoppedEventArgs(_isTimeStopped));
        }

        /// <summary>
        /// 切换时间停止状态
        /// </summary>
        public void ToggleTimeStopped()
        {
            SetTimeStopped(!_isTimeStopped);
        }

        /// <summary>
        /// 设置无敌状态
        /// </summary>
        /// <param name="godMode">是否无敌</param>
        public void SetGodMode(bool godMode)
        {
            if (_isGodMode == godMode)
            {
                return;
            }

            _isGodMode = godMode;
            Debug.Log($"[GameStateManager] 无敌模式: {_isGodMode}");
        }

        /// <summary>
        /// 设置开发者模式
        /// </summary>
        /// <param name="devMode">是否开发者模式</param>
        public void SetDevMode(bool devMode)
        {
            _isDevMode = devMode;
            Debug.Log($"[GameStateManager] 开发者模式: {_isDevMode}");
        }

        #endregion

        #region 公共方法 - 玩家属性

        /// <summary>
        /// 造成伤害给玩家
        /// </summary>
        /// <param name="damage">伤害值</param>
        public void DamagePlayer(float damage)
        {
            if (_isGodMode)
            {
                Debug.Log("[GameStateManager] 无敌模式，跳过伤害");
                return;
            }

            _playerHealth -= damage;
            _playerHealth = Mathf.Max(0f, _playerHealth);

            Debug.Log($"[GameStateManager] 玩家受到伤害: {damage}, 剩余生命: {_playerHealth}");

            // 触发玩家受伤事件
            EventManager.Instance.TriggerEvent(CoreEvents.PLAYER_DAMAGED, new PlayerDamageEventArgs(damage));

            // 检查是否死亡
            if (_playerHealth <= 0)
            {
                KillPlayer();
            }
        }

        /// <summary>
        /// 治疗玩家
        /// </summary>
        /// <param name="amount">治疗量</param>
        public void HealPlayer(float amount)
        {
            _playerHealth += amount;
            _playerHealth = Mathf.Min(_playerMaxHealth, _playerHealth);

            Debug.Log($"[GameStateManager] 玩家恢复生命: {amount}, 当前生命: {_playerHealth}");
        }

        /// <summary>
        /// 设置玩家最大生命值
        /// </summary>
        /// <param name="maxHealth">最大生命值</param>
        public void SetPlayerMaxHealth(float maxHealth)
        {
            _playerMaxHealth = Mathf.Max(1f, maxHealth);
            _playerHealth = Mathf.Min(_playerHealth, _playerMaxHealth);
            Debug.Log($"[GameStateManager] 玩家最大生命值: {_playerMaxHealth}");
        }

        /// <summary>
        /// 消耗魔法值
        /// </summary>
        /// <param name="amount">消耗量</param>
        /// <returns>是否成功</returns>
        public bool ConsumeMana(float amount)
        {
            if (_playerMana >= amount)
            {
                _playerMana -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 恢复魔法值
        /// </summary>
        /// <param name="amount">恢复量</param>
        public void RestoreMana(float amount)
        {
            _playerMana += amount;
            _playerMana = Mathf.Min(_playerMaxMana, _playerMana);
        }

        /// <summary>
        /// 杀死玩家
        /// </summary>
        public void KillPlayer()
        {
            Debug.Log("[GameStateManager] 玩家死亡");
            SetPlayerState(PlayerState.Dead);
            EventManager.Instance.TriggerEvent(CoreEvents.PLAYER_DIED);
        }

        /// <summary>
        /// 复活玩家
        /// </summary>
        /// <param name="healthPercent">复活时生命百分比</param>
        public void RevivePlayer(float healthPercent = 0.5f)
        {
            _playerHealth = _playerMaxHealth * healthPercent;
            _playerMana = _playerMaxMana;
            SetPlayerState(PlayerState.Normal);
            Debug.Log($"[GameStateManager] 玩家已复活，生命: {_playerHealth}");
            EventManager.Instance.TriggerEvent(CoreEvents.PLAYER_RESPAWNED);
        }

        #endregion

        #region 公共方法 - 剧情进度

        /// <summary>
        /// 推进剧情进度
        /// </summary>
        /// <param name="progress">推进量</param>
        public void AdvanceStoryProgress(int progress = 1)
        {
            int oldProgress = _storyProgress;
            _storyProgress += progress;
            Debug.Log($"[GameStateManager] 剧情进度: {oldProgress} -> {_storyProgress}");

            // 触发剧情进度事件
            EventManager.Instance.TriggerEvent(CoreEvents.STORY_PROGRESS_CHANGED, new StoryProgressEventArgs(oldProgress, _storyProgress));
        }

        /// <summary>
        /// 设置剧情进度
        /// </summary>
        /// <param name="progress">进度</param>
        public void SetStoryProgress(int progress)
        {
            _storyProgress = Mathf.Max(0, progress);
            Debug.Log($"[GameStateManager] 剧情进度设置为: {_storyProgress}");
        }

        #endregion

        #region 公共方法 - 游戏时间

        /// <summary>
        /// 更新总游戏时间
        /// </summary>
        private void UpdateTotalPlayTime()
        {
            _totalPlayTime = DateTime.Now - _gameStartTime;
        }

        /// <summary>
        /// 获取格式化总游戏时间
        /// </summary>
        /// <returns>格式化的时间字符串</returns>
        public string GetFormattedPlayTime()
        {
            UpdateTotalPlayTime();
            return $"{_totalPlayTime.Hours:D2}:{_totalPlayTime.Minutes:D2}:{_totalPlayTime.Seconds:D2}";
        }

        #endregion

        #region 公共方法 - 存档集成

        /// <summary>
        /// 获取存档数据
        /// </summary>
        /// <returns>游戏状态存档数据</returns>
        public GameStateSaveData GetSaveData()
        {
            UpdateTotalPlayTime();
            return new GameStateSaveData
            {
                PlayerState = _currentPlayerState,
                GameMode = _currentGameMode,
                Difficulty = _currentDifficulty,
                StoryProgress = _storyProgress,
                TotalPlayTime = _totalPlayTime,
                PlayerHealth = _playerHealth,
                PlayerMaxHealth = _playerMaxHealth,
                PlayerMana = _playerMana,
                PlayerMaxMana = _playerMaxMana,
                IsTimeStopped = _isTimeStopped,
                IsGodMode = _isGodMode
            };
        }

        /// <summary>
        /// 加载存档数据
        /// </summary>
        /// <param name="data">游戏状态存档数据</param>
        public void LoadSaveData(GameStateSaveData data)
        {
            _currentPlayerState = data.PlayerState;
            _currentGameMode = data.GameMode;
            _currentDifficulty = data.Difficulty;
            _storyProgress = data.StoryProgress;
            _totalPlayTime = data.TotalPlayTime;
            _playerHealth = data.PlayerHealth;
            _playerMaxHealth = data.PlayerMaxHealth;
            _playerMana = data.PlayerMana;
            _playerMaxMana = data.PlayerMaxMana;
            _isTimeStopped = data.IsTimeStopped;
            _isGodMode = data.IsGodMode;

            UpdateDifficultyMultiplier();
            Debug.Log("[GameStateManager] 已加载存档数据");
        }

        /// <summary>
        /// 重置存档数据
        /// </summary>
        public void ResetSaveData()
        {
            _currentPlayerState = PlayerState.Normal;
            _currentGameMode = GameMode.Exploration;
            _currentDifficulty = DifficultyLevel.Normal;
            _storyProgress = 0;
            _playerHealth = _playerMaxHealth;
            _playerMana = _playerMaxMana;
            _isTimeStopped = false;
            _isGodMode = false;

            UpdateDifficultyMultiplier();
            Debug.Log("[GameStateManager] 已重置存档数据");
        }

        #endregion
    }

    #region 存档数据类

    /// <summary>
    /// 游戏状态存档数据
    /// </summary>
    [Serializable]
    public class GameStateSaveData
    {
        /// <summary>
        /// 玩家状态
        /// </summary>
        public PlayerState PlayerState;

        /// <summary>
        /// 游戏模式
        /// </summary>
        public GameMode GameMode;

        /// <summary>
        /// 难度等级
        /// </summary>
        public DifficultyLevel Difficulty;

        /// <summary>
        /// 剧情进度
        /// </summary>
        public int StoryProgress;

        /// <summary>
        /// 总游戏时间
        /// </summary>
        public TimeSpan TotalPlayTime;

        /// <summary>
        /// 玩家当前生命值
        /// </summary>
        public float PlayerHealth;

        /// <summary>
        /// 玩家最大生命值
        /// </summary>
        public float PlayerMaxHealth;

        /// <summary>
        /// 玩家当前魔法值
        /// </summary>
        public float PlayerMana;

        /// <summary>
        /// 玩家最大魔法值
        /// </summary>
        public float PlayerMaxMana;

        /// <summary>
        /// 是否时间停止
        /// </summary>
        public bool IsTimeStopped;

        /// <summary>
        /// 是否无敌模式
        /// </summary>
        public bool IsGodMode;
    }

    #endregion

    #region 事件参数类

    /// <summary>
    /// 玩家状态变更事件参数
    /// </summary>
    public class PlayerStateEventArgs : EventArgs
    {
        /// <summary>
        /// 新状态
        /// </summary>
        public PlayerState NewState { get; }

        public PlayerStateEventArgs(PlayerState newState)
        {
            NewState = newState;
        }
    }

    /// <summary>
    /// 游戏模式变更事件参数
    /// </summary>
    public class GameModeEventArgs : EventArgs
    {
        /// <summary>
        /// 新模式
        /// </summary>
        public GameMode NewMode { get; }

        public GameModeEventArgs(GameMode newMode)
        {
            NewMode = newMode;
        }
    }

    /// <summary>
    /// 难度变更事件参数
    /// </summary>
    public class DifficultyEventArgs : EventArgs
    {
        /// <summary>
        /// 新难度
        /// </summary>
        public DifficultyLevel NewDifficulty { get; }

        public DifficultyEventArgs(DifficultyLevel newDifficulty)
        {
            NewDifficulty = newDifficulty;
        }
    }

    /// <summary>
    /// 时间停止变更事件参数
    /// </summary>
    public class TimeStoppedEventArgs : EventArgs
    {
        /// <summary>
        /// 是否停止
        /// </summary>
        public bool IsStopped { get; }

        public TimeStoppedEventArgs(bool isStopped)
        {
            IsStopped = isStopped;
        }
    }

    /// <summary>
    /// 玩家受伤事件参数
    /// </summary>
    public class PlayerDamageEventArgs : EventArgs
    {
        /// <summary>
        /// 伤害值
        /// </summary>
        public float Damage { get; }

        public PlayerDamageEventArgs(float damage)
        {
            Damage = damage;
        }
    }

    /// <summary>
    /// 剧情进度变更事件参数
    /// </summary>
    public class StoryProgressEventArgs : EventArgs
    {
        /// <summary>
        /// 旧进度
        /// </summary>
        public int OldProgress { get; }

        /// <summary>
        /// 新进度
        /// </summary>
        public int NewProgress { get; }

        public StoryProgressEventArgs(int oldProgress, int newProgress)
        {
            OldProgress = oldProgress;
            NewProgress = newProgress;
        }
    }

    #endregion
}
