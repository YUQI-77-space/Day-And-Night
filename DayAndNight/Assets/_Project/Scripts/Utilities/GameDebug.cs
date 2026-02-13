using System;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 游戏调试日志工具类 - 提供不同级别的日志方法
/// 放置位置：Assets/_Project/Scripts/Utilities/GameDebug.cs
/// 
/// 使用方法：
/// GameDebug.Log("普通日志信息");
/// GameDebug.LogWarning("警告信息");
/// GameDebug.LogError("错误信息");
/// GameDebug.LogFormatted("玩家 {0} 造成了 {1} 点伤害", playerName, damage);
/// 
/// 发布版本日志会自动保存到文件：
/// PersistentDataPath/logs/game_YYYYMMDD_HHMMSS.log
/// </summary>
public static class GameDebug
{
    #region 日志级别枚举
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Error = 0,
        Warning = 1,
        Info = 2,
        Debug = 3
    }

    /// <summary>
    /// 当前日志级别（可在运行时修改）
    /// </summary>
    public static LogLevel CurrentLogLevel = LogLevel.Debug;

    /// <summary>
    /// 是否启用文件日志（发布版本默认启用）
    /// </summary>
    public static bool EnableFileLogging = !Application.isEditor;

    /// <summary>
    /// 是否在控制台显示日志
    /// </summary>
    public static bool EnableConsoleLogging = true;
    #endregion

    #region 文件日志相关
    private static StreamWriter _logFileWriter;
    private static string _logFilePath;
    private static readonly object _fileLock = new object();
    private static bool _fileLoggingInitialized = false;
    #endregion

    #region 颜色配置
    /// <summary>
    /// 不同日志级别的颜色配置（仅Editor有效）
    /// </summary>
    private static readonly Color[] LevelColors = new Color[]
    {
        Color.red,      // Error - 红色
        Color.yellow,   // Warning - 黄色
        Color.green,    // Info - 绿色
        Color.white     // Debug - 白色
    };
    #endregion

    #region 初始化
    /// <summary>
    /// 静态构造函数 - 初始化文件日志
    /// </summary>
    static GameDebug()
    {
        // 只在非Editor环境自动初始化文件日志
        if (!Application.isEditor && EnableFileLogging)
        {
            InitializeFileLogging();
        }
    }

    /// <summary>
    /// 初始化文件日志功能
    /// </summary>
    public static void InitializeFileLogging()
    {
        if (_fileLoggingInitialized) return;

        try
        {
            // 创建日志目录
            string logDirectory = Path.Combine(Application.persistentDataPath, "logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 创建日志文件名（带时间戳）
            string fileName = $"game_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            _logFilePath = Path.Combine(logDirectory, fileName);

            // 创建文件写入器
            _logFileWriter = new StreamWriter(_logFilePath, false, Encoding.UTF8)
            {
                AutoFlush = true
            };

            // 写入文件头信息
            WriteFileHeader();

            // 订阅Unity日志事件
            Application.logMessageReceived += HandleUnityLog;
            
            _fileLoggingInitialized = true;
            
            LogFormatted("文件日志已初始化", LogLevel.Info);
            LogFormatted("日志文件路径: {0}", _logFilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameDebug] 初始化文件日志失败: {ex.Message}");
            EnableFileLogging = false;
        }
    }

    /// <summary>
    /// 写入日志文件头信息
    /// </summary>
    private static void WriteFileHeader()
    {
        StringBuilder header = new StringBuilder();
        header.AppendLine("===========================================");
        header.AppendLine($"  DayAndNight 游戏日志");
        header.AppendLine($"  生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        header.AppendLine($"  游戏版本: {Application.version}");
        header.AppendLine($"  平台: {Application.platform}");
        header.AppendLine($"  设备: {SystemInfo.deviceName}");
        header.AppendLine($"  Unity版本: {Application.unityVersion}");
        header.AppendLine("===========================================");
        header.AppendLine();

        WriteToFile(header.ToString());
    }

    /// <summary>
    /// 处理Unity日志事件
    /// </summary>
    private static void HandleUnityLog(string condition, string stackTrace, LogType type)
    {
        if (!EnableFileLogging || !_fileLoggingInitialized) return;

        LogLevel level = LogLevel.Info;
        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
                level = LogLevel.Error;
                break;
            case LogType.Warning:
                level = LogLevel.Warning;
                break;
            case LogType.Log:
                level = LogLevel.Info;
                break;
            case LogType.Exception:
                level = LogLevel.Error;
                break;
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string formattedMessage = $"[{timestamp}] [{level}] {condition}";

        // 如果有堆栈跟踪，添加到日志
        if (!string.IsNullOrEmpty(stackTrace))
        {
            formattedMessage += $"\n  StackTrace: {stackTrace}";
        }

        WriteToFile(formattedMessage);
    }

    /// <summary>
    /// 写入文件（线程安全）
    /// </summary>
    private static void WriteToFile(string message)
    {
        if (_logFileWriter == null) return;

        lock (_fileLock)
        {
            try
            {
                _logFileWriter.WriteLine(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDebug] 写入日志文件失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 刷新并关闭日志文件（应用程序退出时调用）
    /// </summary>
    public static void FlushAndClose()
    {
        if (_logFileWriter != null)
        {
            WriteToFile($"\n--- 日志结束 {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
            _logFileWriter.Close();
            _logFileWriter.Dispose();
            _logFileWriter = null;
        }
        _fileLoggingInitialized = false;
    }
    #endregion

    #region 基础日志方法（Conditional特性 - 仅Editor编译）
    /// <summary>
    /// 普通信息日志（仅Editor显示）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(object message)
    {
        LogMessage(message?.ToString() ?? "null", LogLevel.Info);
    }

    /// <summary>
    /// 警告日志（仅Editor显示）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogWarning(object message)
    {
        LogMessage(message?.ToString() ?? "null", LogLevel.Warning);
    }

    /// <summary>
    /// 错误日志（仅Editor显示）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogError(object message)
    {
        LogMessage(message?.ToString() ?? "null", LogLevel.Error);
    }
    #endregion

    #region 格式化日志方法（始终编译）
    /// <summary>
    /// 格式化普通信息日志
    /// </summary>
    public static void LogFormatted(object message, params object[] args)
    {
        if (args == null || args.Length == 0)
        {
            LogMessage(message?.ToString() ?? "null", LogLevel.Info);
        }
        else
        {
            try
            {
                LogMessage(string.Format(message?.ToString() ?? "null", args), LogLevel.Info);
            }
            catch (FormatException)
            {
                LogMessage($"[格式化错误] {message}", LogLevel.Error);
            }
        }
    }

    /// <summary>
    /// 格式化警告日志
    /// </summary>
    public static void LogWarningFormatted(object message, params object[] args)
    {
        if (args == null || args.Length == 0)
        {
            LogMessage(message?.ToString() ?? "null", LogLevel.Warning);
        }
        else
        {
            try
            {
                LogMessage(string.Format(message?.ToString() ?? "null", args), LogLevel.Warning);
            }
            catch (FormatException)
            {
                LogMessage($"[格式化错误] {message}", LogLevel.Error);
            }
        }
    }

    /// <summary>
    /// 格式化错误日志
    /// </summary>
    public static void LogErrorFormatted(object message, params object[] args)
    {
        if (args == null || args.Length == 0)
        {
            LogMessage(message?.ToString() ?? "null", LogLevel.Error);
        }
        else
        {
            try
            {
                LogMessage(string.Format(message?.ToString() ?? "null", args), LogLevel.Error);
            }
            catch (FormatException)
            {
                LogMessage($"[格式化错误] {message}", LogLevel.Error);
            }
        }
    }
    #endregion

    #region 内部日志处理
    /// <summary>
    /// 内部日志消息处理
    /// </summary>
    private static void LogMessage(string message, LogLevel level)
    {
        // 检查日志级别
        if (level > CurrentLogLevel) return;

        // 添加前缀标签
        string taggedMessage = $"[{level}] {message}";

        // 控制台输出
        if (EnableConsoleLogging)
        {
            OutputToConsole(taggedMessage, level);
        }

        // 文件输出
        if (EnableFileLogging && _fileLoggingInitialized)
        {
            OutputToFile(taggedMessage, level);
        }
    }

    /// <summary>
    /// 输出到控制台
    /// </summary>
    private static void OutputToConsole(string message, LogLevel level)
    {
#if UNITY_EDITOR
        // Editor环境下使用彩色标签
        string coloredMessage = ColorizeMessage(message, level);
        switch (level)
        {
            case LogLevel.Error:
                Debug.LogError(coloredMessage);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(coloredMessage);
                break;
            default:
                Debug.Log(coloredMessage);
                break;
        }
#else
        // 发布环境下简单输出
        switch (level)
        {
            case LogLevel.Error:
                Debug.LogError(message);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(message);
                break;
            default:
                Debug.Log(message);
                break;
        }
#endif
    }

    /// <summary>
    /// 为消息添加颜色标签（仅Editor）
    /// </summary>
#if UNITY_EDITOR
    private static string ColorizeMessage(string message, LogLevel level)
    {
        Color color = LevelColors[(int)level];
        return $"<color=#{ColorToHex(color)}>{message}</color>";
    }

    private static string ColorToHex(Color color)
    {
        int r = (int)(color.r * 255);
        int g = (int)(color.g * 255);
        int b = (int)(color.b * 255);
        return $"{r:X2}{g:X2}{b:X2}";
    }
#else
    private static string ColorizeMessage(string message, LogLevel level) => message;
#endif

    /// <summary>
    /// 输出到日志文件
    /// </summary>
    private static void OutputToFile(string message, LogLevel level)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string formattedMessage = $"[{timestamp}] {message}";
        WriteToFile(formattedMessage);
    }
    #endregion

    #region 便捷方法
    /// <summary>
    /// 记录对象信息（调试用）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogObject(object obj, string prefix = "")
    {
        if (obj == null)
        {
            Log($"{prefix}对象为 null");
            return;
        }

        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(prefix))
        {
            sb.AppendLine(prefix);
        }
        sb.AppendLine($"类型: {obj.GetType().Name}");
        
        var fields = obj.GetType().GetFields();
        foreach (var field in fields)
        {
            sb.AppendLine($"  {field.Name}: {field.GetValue(obj)}");
        }

        Log(sb.ToString());
    }

    /// <summary>
    /// 记录性能计时开始
    /// </summary>
    public static void LogTimeStart(string tag)
    {
        LogFormatted("[计时开始] {0}", tag);
    }

    /// <summary>
    /// 记录性能计时结束
    /// </summary>
    public static void LogTimeEnd(string tag)
    {
        LogFormatted("[计时结束] {0}", tag);
    }

    /// <summary>
    /// 分组日志开始
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogGroupStart(string groupName)
    {
        Log($"========== {groupName} ==========");
    }

    /// <summary>
    /// 分组日志结束
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogGroupEnd(string groupName)
    {
        Log($"========== {groupName} 结束 ==========");
    }
    #endregion

    #region 日志级别控制
    /// <summary>
    /// 设置日志级别
    /// </summary>
    public static void SetLogLevel(LogLevel level)
    {
        CurrentLogLevel = level;
        LogFormatted("日志级别已设置为: {0}", level);
    }

    /// <summary>
    /// 启用或禁用文件日志
    /// </summary>
    public static void SetFileLogging(bool enabled)
    {
        if (enabled && !_fileLoggingInitialized)
        {
            InitializeFileLogging();
        }
        EnableFileLogging = enabled;
        LogFormatted("文件日志已{0}", enabled ? "启用" : "禁用");
    }

    /// <summary>
    /// 获取当前日志文件路径
    /// </summary>
    public static string GetLogFilePath()
    {
        return _logFilePath;
    }
    #endregion

    #region 应用程序退出处理
    /// <summary>
    /// 在应用程序退出时调用，确保日志正确保存
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        // 场景加载前初始化（如果需要）
    }

    /// <summary>
    /// 应用程序退出时的处理
    /// </summary>
    private static void OnApplicationQuit()
    {
        FlushAndClose();
    }
    #endregion
}
