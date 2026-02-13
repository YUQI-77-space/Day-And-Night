using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 核心游戏事件定义常量
    /// 统一管理所有游戏内事件，避免字符串硬编码
    /// </summary>
    public static class CoreEvents
    {
        #region 游戏状态事件

        /// <summary>
        /// 游戏开始时触发
        /// </summary>
        public const string GAME_STARTED = "CoreEvents.GameStarted";

        /// <summary>
        /// 游戏暂停时触发
        /// </summary>
        public const string GAME_PAUSED = "CoreEvents.GamePaused";

        /// <summary>
        /// 游戏恢复时触发
        /// </summary>
        public const string GAME_RESUMED = "CoreEvents.GameResumed";

        /// <summary>
        /// 游戏结束时触发
        /// </summary>
        public const string GAME_ENDED = "CoreEvents.GameEnded";

        /// <summary>
        /// 退出游戏时触发
        /// </summary>
        public const string GAME_QUIT = "CoreEvents.GameQuit";

        #endregion

        #region 时间相关事件

        /// <summary>
        /// 天数变化时触发
        /// </summary>
        public const string DAY_CHANGED = "CoreEvents.DayChanged";

        /// <summary>
        /// 小时变化时触发
        /// </summary>
        public const string HOUR_CHANGED = "CoreEvents.HourChanged";

        /// <summary>
        /// 昼夜切换时触发
        /// </summary>
        public const string DAY_NIGHT_CHANGED = "CoreEvents.DayNightChanged";

        /// <summary>
        /// 时间暂停状态变化时触发
        /// </summary>
        public const string TIME_SCALE_CHANGED = "CoreEvents.TimeScaleChanged";

        #endregion

        #region 场景相关事件

        /// <summary>
        /// 场景开始加载时触发
        /// </summary>
        public const string SCENE_LOADING = "CoreEvents.SceneLoading";

        /// <summary>
        /// 场景加载完成时触发
        /// </summary>
        public const string SCENE_LOADED = "CoreEvents.SceneLoaded";

        /// <summary>
        /// 场景卸载完成时触发
        /// </summary>
        public const string SCENE_UNLOADED = "CoreEvents.SceneUnloaded";

        #endregion

        #region 存档相关事件

        /// <summary>
        /// 游戏保存成功时触发
        /// </summary>
        public const string GAME_SAVED = "CoreEvents.GameSaved";

        /// <summary>
        /// 游戏读档成功时触发
        /// </summary>
        public const string GAME_LOADED = "CoreEvents.GameLoaded";

        /// <summary>
        /// 保存失败时触发
        /// </summary>
        public const string SAVE_FAILED = "CoreEvents.SaveFailed";

        /// <summary>
        /// 加载失败时触发
        /// </summary>
        public const string LOAD_FAILED = "CoreEvents.LoadFailed";

        /// <summary>
        /// 自动保存触发时
        /// </summary>
        public const string AUTO_SAVE = "CoreEvents.AutoSave";

        /// <summary>
        /// 新游戏开始时触发
        /// </summary>
        public const string NEW_GAME = "CoreEvents.NewGame";

        #endregion

        #region 玩家相关事件

        /// <summary>
        /// 玩家生成/复活时触发
        /// </summary>
        public const string PLAYER_SPAWNED = "CoreEvents.PlayerSpawned";

        /// <summary>
        /// 玩家死亡时触发
        /// </summary>
        public const string PLAYER_DIED = "CoreEvents.PlayerDied";

        /// <summary>
        /// 玩家复活时触发
        /// </summary>
        public const string PLAYER_RESPAWNED = "CoreEvents.PlayerRespawned";

        /// <summary>
        /// 玩家受伤时触发
        /// </summary>
        public const string PLAYER_DAMAGED = "CoreEvents.PlayerDamaged";

        /// <summary>
        /// 玩家获得经验时触发
        /// </summary>
        public const string PLAYER_EXPERIENCE_GAINED = "CoreEvents.PlayerExperienceGained";

        /// <summary>
        /// 玩家升级时触发
        /// </summary>
        public const string PLAYER_LEVELED_UP = "CoreEvents.PlayerLeveledUp";

        #endregion

        #region UI相关事件

        /// <summary>
        /// 打开菜单时触发
        /// </summary>
        public const string MENU_OPENED = "CoreEvents.MenuOpened";

        /// <summary>
        /// 关闭菜单时触发
        /// </summary>
        public const string MENU_CLOSED = "CoreEvents.MenuClosed";

        /// <summary>
        /// 打开暂停菜单时触发
        /// </summary>
        public const string PAUSE_MENU_OPENED = "CoreEvents.PauseMenuOpened";

        #endregion

        #region 游戏状态相关事件

        /// <summary>
        /// 玩家状态变更时触发
        /// </summary>
        public const string PLAYER_STATE_CHANGED = "CoreEvents.PlayerStateChanged";

        /// <summary>
        /// 游戏模式变更时触发
        /// </summary>
        public const string GAME_MODE_CHANGED = "CoreEvents.GameModeChanged";

        /// <summary>
        /// 难度变更时触发
        /// </summary>
        public const string DIFFICULTY_CHANGED = "CoreEvents.DifficultyChanged";

        /// <summary>
        /// 时间停止状态变更时触发
        /// </summary>
        public const string TIME_STOPPED_CHANGED = "CoreEvents.TimeStoppedChanged";

        /// <summary>
        /// 剧情进度变更时触发
        /// </summary>
        public const string STORY_PROGRESS_CHANGED = "CoreEvents.StoryProgressChanged";

        #endregion

        #region 系统相关事件

        /// <summary>
        /// 系统初始化完成时触发
        /// </summary>
        public const string SYSTEM_INITIALIZED = "CoreEvents.SystemInitialized";

        /// <summary>
        /// 系统发生错误时触发
        /// </summary>
        public const string SYSTEM_ERROR = "CoreEvents.SystemError";

        #endregion

        #region 音频相关事件

        /// <summary>
        /// 音乐开始播放时触发
        /// </summary>
        public const string AUDIO_MUSIC_STARTED = "CoreEvents.AudioMusicStarted";

        /// <summary>
        /// 音乐播放完成时触发
        /// </summary>
        public const string AUDIO_MUSIC_ENDED = "CoreEvents.AudioMusicEnded";

        /// <summary>
        /// 音效播放时触发
        /// </summary>
        public const string AUDIO_SFX_PLAYED = "CoreEvents.AudioSFXPlayed";

        #endregion
    }

    #region 事件参数类

    /// <summary>
    /// 事件参数基类
    /// </summary>
    public class EventArgs
    {
        /// <summary>
        /// 事件发生的时间戳
        /// </summary>
        public float Timestamp { get; }

        protected EventArgs()
        {
            Timestamp = Time.time;
        }
    }

    /// <summary>
    /// 场景加载事件参数
    /// </summary>
    public class SceneEventArgs : EventArgs
    {
        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName { get; }

        /// <summary>
        /// 加载进度 (0-1)
        /// </summary>
        public float Progress { get; }

        public SceneEventArgs(string sceneName, float progress = 1f)
        {
            SceneName = sceneName;
            Progress = progress;
        }
    }

    /// <summary>
    /// 保存事件参数
    /// </summary>
    public class SaveEventArgs : EventArgs
    {
        /// <summary>
        /// 存档槽位编号
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 错误信息（如果失败）
        /// </summary>
        public string ErrorMessage { get; }

        public SaveEventArgs(int slotIndex, bool success, string errorMessage = null)
        {
            SlotIndex = slotIndex;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// 玩家状态事件参数
    /// </summary>
    public class PlayerEventArgs : EventArgs
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// 数值变化（如生命值、经验值等）
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// 变化前的数值
        /// </summary>
        public int PreviousValue { get; }

        /// <summary>
        /// 变化后的数值
        /// </summary>
        public int CurrentValue { get; }

        public PlayerEventArgs(string playerId, int value, int previousValue, int currentValue)
        {
            PlayerId = playerId;
            Value = value;
            PreviousValue = previousValue;
            CurrentValue = currentValue;
        }
    }

    /// <summary>
    /// 昼夜切换事件参数
    /// </summary>
    public class DayNightEventArgs : EventArgs
    {
        /// <summary>
        /// 是否是白天
        /// </summary>
        public bool IsDay { get; }

        /// <summary>
        /// 当前天数
        /// </summary>
        public int Day { get; }

        /// <summary>
        /// 当前小时
        /// </summary>
        public int Hour { get; }

        public DayNightEventArgs(bool isDay, int day, int hour)
        {
            IsDay = isDay;
            Day = day;
            Hour = hour;
        }
    }

    #endregion
}
