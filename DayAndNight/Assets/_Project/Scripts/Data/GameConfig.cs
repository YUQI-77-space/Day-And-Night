using UnityEngine;

/// <summary>
/// 游戏配置数据 - 使用ScriptableObject创建可配置的游戏参数
/// 放置位置：Assets/_Project/Scripts/Data/GameConfig.cs
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "DayAndNight/Game Config", order = 1)]
public class GameConfig : ScriptableObject
{
    #region 基本信息
    [Header("=== 基本信息 ===")]
    [Tooltip("游戏版本号")]
    public string GameVersion = "1.0.0";

    [Tooltip("游戏名称")]
    public string GameName = "DayAndNight";
    #endregion

    #region 游戏设置
    [Header("=== 游戏设置 ===")]
    [Tooltip("目标帧率")]
    [Range(30, 120)]
    public int TargetFrameRate = 60;

    [Tooltip("默认语言设置 (zh-CN, en-US, etc.)")]
    public string DefaultLanguage = "zh-CN";

    [Tooltip("默认难度 (0=简单, 1=普通, 2=困难)")]
    [Range(0, 2)]
    public int DefaultDifficulty = 1;

    [Tooltip("是否开启垂直同步")]
    public bool VSyncEnabled = true;
    #endregion

    #region 时间系统
    [Header("=== 时间系统 ===")]
    [Tooltip("游戏时间流速（1=正常，2=加速，0.5=减速）")]
    [Range(0.1f, 10f)]
    public float TimeScale = 1f;

    [Tooltip("一天的游戏内分钟数")]
    [Range(1f, 60f)]
    public float MinutesPerGameDay = 10f;

    [Tooltip("白天开始时间（小时，0-24）")]
    [Range(0f, 24f)]
    public float DayStartHour = 6f;

    [Tooltip("夜晚开始时间（小时，0-24）")]
    [Range(0f, 24f)]
    public float NightStartHour = 18f;
    #endregion

    #region 自动保存
    [Header("=== 自动保存 ===")]
    [Tooltip("自动保存间隔（秒）")]
    [Range(30, 600)]
    public float AutoSaveInterval = 60f;

    [Tooltip("是否启用自动保存")]
    public bool AutoSaveEnabled = true;

    [Tooltip("最大保存文件数量")]
    [Range(1, 20)]
    public int MaxSaveFiles = 5;
    #endregion

    #region 音频设置
    [Header("=== 音频设置 ===")]
    [Tooltip("主音量")]
    [Range(0f, 1f)]
    public float MasterVolume = 1f;

    [Tooltip("背景音乐音量")]
    [Range(0f, 1f)]
    public float MusicVolume = 0.7f;

    [Tooltip("音效音量")]
    [Range(0f, 1f)]
    public float SFXVolume = 1f;

    [Tooltip("语音音量")]
    [Range(0f, 1f)]
    public float VoiceVolume = 1f;
    #endregion

    #region 玩家设置
    [Header("=== 玩家设置 ===")]
    [Tooltip("玩家初始生命值")]
    public int InitialPlayerHealth = 100;

    [Tooltip("玩家初始魔法值")]
    public int InitialPlayerMana = 50;

    [Tooltip("玩家初始金币")]
    public int InitialPlayerGold = 100;

    [Tooltip("玩家移动速度")]
    public float PlayerMoveSpeed = 5f;

    [Tooltip("玩家奔跑速度")]
    public float PlayerRunSpeed = 8f;
    #endregion

    #region 调试设置
    [Header("=== 调试设置 ===")]
    [Tooltip("是否显示调试信息")]
    public bool ShowDebugInfo = false;

    [Tooltip("是否启用作弊模式")]
    public bool CheatModeEnabled = false;

    [Tooltip("日志级别 (0=Error, 1=Warning, 2=Info, 3=Debug)")]
    [Range(0, 3)]
    public int LogLevel = 2;
    #endregion

    #region 公开方法
    /// <summary>
    /// 获取格式化的时间字符串
    /// </summary>
    public string GetFormattedTime()
    {
        return $"{GameName} v{GameVersion}";
    }

    /// <summary>
    /// 获取难度名称
    /// </summary>
    public string GetDifficultyName()
    {
        switch (DefaultDifficulty)
        {
            case 0: return "简单";
            case 1: return "普通";
            case 2: return "困难";
            default: return "未知";
        }
    }

    /// <summary>
    /// 获取游戏内一天的实际秒数
    /// </summary>
    public float GetRealSecondsPerGameDay()
    {
        return MinutesPerGameDay * 60f / TimeScale;
    }
    #endregion
}
