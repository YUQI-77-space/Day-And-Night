using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 游戏日志查看器窗口 - 在Unity Editor中查看游戏日志
/// 放置位置：Assets/_Project/Scripts/Editor/GameDebugWindow.cs
/// 
/// 打开方法：菜单栏 -> Tools -> DayAndNight -> Log Viewer
/// </summary>
public class GameDebugWindow : EditorWindow
{
    #region 窗口实例
    /// <summary>
    /// 打开日志查看器窗口
    /// </summary>
    [MenuItem("Tools/DayAndNight/日志查看器 &L")]
    public static void ShowWindow()
    {
        GetWindow<GameDebugWindow>("日志查看器");
    }
    #endregion

    #region 日志数据
    private List<LogEntry> _logs = new List<LogEntry>();
    private Vector2 _scrollPosition;
    private GameDebug.LogLevel _filterLevel = GameDebug.LogLevel.Info;
    private string _searchText = "";
    private bool _autoScroll = true;
    private int _errorCount = 0;
    private int _warningCount = 0;
    private int _infoCount = 0;
    #endregion

    #region GUI样式
    private GUIStyle _logEntryStyle;
    private GUIStyle _toolbarStyle;
    #endregion

    #region 生命周期
    /// <summary>
    /// 窗口创建时
    /// </summary>
    private void OnEnable()
    {
        // 订阅日志消息
        Application.logMessageReceived += HandleLog;
        
        // 加载现有日志文件
        LoadLogFiles();
    }

    /// <summary>
    /// 窗口关闭时
    /// </summary>
    private void OnDisable()
    {
        // 取消订阅
        Application.logMessageReceived -= HandleLog;
    }

    /// <summary>
    /// 绘制窗口GUI
    /// </summary>
    private void OnGUI()
    {
        InitStyles();
        
        // 绘制工具栏
        DrawToolbar();
        
        // 绘制日志列表
        DrawLogList();
        
        // 绘制状态栏
        DrawStatusBar();
    }

    /// <summary>
    /// 初始化样式
    /// </summary>
    private void InitStyles()
    {
        if (_logEntryStyle == null)
        {
            _logEntryStyle = new GUIStyle(EditorStyles.textArea)
            {
                richText = true,
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                fontSize = 12,
                wordWrap = true
            };
        }

        if (_toolbarStyle == null)
        {
            _toolbarStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                fontSize = 12
            };
        }
    }
    #endregion

    #region 工具栏
    /// <summary>
    /// 绘制工具栏
    /// </summary>
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // 过滤级别
        GUILayout.Label("过滤:", GUILayout.Width(40));
        GameDebug.LogLevel newLevel = (GameDebug.LogLevel)EditorGUILayout.EnumPopup(_filterLevel, EditorStyles.toolbarPopup, GUILayout.Width(80));
        if (newLevel != _filterLevel)
        {
            _filterLevel = newLevel;
        }
        
        GUILayout.Space(10);
        
        // 搜索框
        GUILayout.Label("搜索:", GUILayout.Width(40));
        string newSearch = GUILayout.TextField(_searchText, EditorStyles.toolbarTextField, GUILayout.Width(200));
        if (newSearch != _searchText)
        {
            _searchText = newSearch;
        }
        
        GUILayout.Space(10);
        
        // 自动滚动
        _autoScroll = GUILayout.Toggle(_autoScroll, "自动滚动", EditorStyles.toolbarButton);
        
        GUILayout.FlexibleSpace();
        
        // 清空按钮
        if (GUILayout.Button("清空", EditorStyles.toolbarButton))
        {
            ClearLogs();
        }
        
        // 刷新按钮
        if (GUILayout.Button("刷新", EditorStyles.toolbarButton))
        {
            LoadLogFiles();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region 日志列表
    /// <summary>
    /// 绘制日志列表
    /// </summary>
    private void DrawLogList()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        // 计算可见日志
        List<LogEntry> visibleLogs = GetFilteredLogs();
        
        foreach (var log in visibleLogs)
        {
            DrawLogEntry(log);
        }
        
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制单条日志
    /// </summary>
    private void DrawLogEntry(LogEntry log)
    {
        Color bgColor = GetBackgroundColor(log.Level);
        Color textColor = GetTextColor(log.Level);
        
        EditorGUILayout.BeginHorizontal();
        
        // 级别标签
        string levelLabel = GetLevelLabel(log.Level);
        Color oldColor = GUI.contentColor;
        GUI.contentColor = textColor;
        GUILayout.Label(levelLabel, GUILayout.Width(60), GUILayout.Height(40));
        GUI.contentColor = oldColor;
        
        // 日志消息
        string message = log.ToString();
        if (!string.IsNullOrEmpty(_searchText))
        {
            message = HighlightSearchText(message, _searchText);
        }
        
        GUIContent content = new GUIContent(message);
        
        // 使用标签绘制可选择的消息
        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            richText = true,
            wordWrap = true
        };
        
        EditorGUILayout.SelectableLabel(message, style, GUILayout.Height(EditorStyles.label.CalcHeight(content, position.width - 80)));
        
        // 复制按钮
        if (GUILayout.Button("复制", GUILayout.Width(50)))
        {
            GUIUtility.systemCopyBuffer = log.Message;
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 分割线
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    /// <summary>
    /// 获取背景颜色
    /// </summary>
    private Color GetBackgroundColor(GameDebug.LogLevel level)
    {
        switch (level)
        {
            case GameDebug.LogLevel.Error:
                return new Color(1f, 0.8f, 0.8f);
            case GameDebug.LogLevel.Warning:
                return new Color(1f, 1f, 0.8f);
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// 获取文字颜色
    /// </summary>
    private Color GetTextColor(GameDebug.LogLevel level)
    {
        switch (level)
        {
            case GameDebug.LogLevel.Error:
                return Color.red;
            case GameDebug.LogLevel.Warning:
                return new Color(1f, 0.6f, 0f);
            case GameDebug.LogLevel.Info:
                return Color.green;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// 获取级别标签
    /// </summary>
    private string GetLevelLabel(GameDebug.LogLevel level)
    {
        switch (level)
        {
            case GameDebug.LogLevel.Error:
                return "<color=red>ERROR</color>";
            case GameDebug.LogLevel.Warning:
                return "<color=orange>WARN</color>";
            case GameDebug.LogLevel.Info:
                return "<color=green>INFO</color>";
            case GameDebug.LogLevel.Debug:
                return "<color=gray>DEBUG</color>";
            default:
                return "UNKNOWN";
        }
    }

    /// <summary>
    /// 高亮搜索文本
    /// </summary>
    private string HighlightSearchText(string text, string search)
    {
        if (string.IsNullOrEmpty(search)) return text;
        
        int index = text.ToLower().IndexOf(search.ToLower());
        if (index >= 0)
        {
            return text.Substring(0, index) + 
                   "<mark=#FFFF00AA>" + 
                   text.Substring(index, search.Length) + 
                   "</mark>" + 
                   text.Substring(index + search.Length);
        }
        return text;
    }
    #endregion

    #region 状态栏
    /// <summary>
    /// 绘制状态栏
    /// </summary>
    private void DrawStatusBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // 统计信息
        GUILayout.Label($"总日志: {_logs.Count}", EditorStyles.toolbarButton);
        GUILayout.Label($"<color=red>错误: {_errorCount}</color>", EditorStyles.toolbarButton);
        GUILayout.Label($"<color=orange>警告: {_warningCount}</color>", EditorStyles.toolbarButton);
        GUILayout.Label($"<color=green>信息: {_infoCount}</color>", EditorStyles.toolbarButton);
        
        GUILayout.FlexibleSpace();
        
        // 日志路径
        if (!string.IsNullOrEmpty(GameDebug.GetLogFilePath()))
        {
            GUILayout.Label($"日志文件: {GameDebug.GetLogFilePath()}", EditorStyles.toolbarButton);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region 日志处理
    /// <summary>
    /// 处理日志消息
    /// </summary>
    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        GameDebug.LogLevel level = GameDebug.LogLevel.Info;
        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                level = GameDebug.LogLevel.Error;
                break;
            case LogType.Warning:
                level = GameDebug.LogLevel.Warning;
                break;
            case LogType.Log:
                level = GameDebug.LogLevel.Info;
                break;
        }
        
        AddLog(condition, level, stackTrace);
        
        // 自动滚动
        if (_autoScroll)
        {
            Repaint();
        }
    }

    /// <summary>
    /// 添加日志条目
    /// </summary>
    private void AddLog(string message, GameDebug.LogLevel level, string stackTrace = "")
    {
        LogEntry entry = new LogEntry
        {
            Time = System.DateTime.Now,
            Message = message,
            Level = level,
            StackTrace = stackTrace
        };
        
        _logs.Add(entry);
        
        // 更新统计
        UpdateCounts();
    }

    /// <summary>
    /// 获取过滤后的日志列表
    /// </summary>
    private List<LogEntry> GetFilteredLogs()
    {
        List<LogEntry> result = new List<LogEntry>();
        
        foreach (var log in _logs)
        {
            // 级别过滤
            if (log.Level > _filterLevel) continue;
            
            // 搜索过滤
            if (!string.IsNullOrEmpty(_searchText))
            {
                string searchLower = _searchText.ToLower();
                if (!log.Message.ToLower().Contains(searchLower) &&
                    !log.Time.ToString().ToLower().Contains(searchLower))
                {
                    continue;
                }
            }
            
            result.Add(log);
        }
        
        return result;
    }

    /// <summary>
    /// 更新统计计数
    /// </summary>
    private void UpdateCounts()
    {
        _errorCount = 0;
        _warningCount = 0;
        _infoCount = 0;
        
        foreach (var log in _logs)
        {
            switch (log.Level)
            {
                case GameDebug.LogLevel.Error:
                    _errorCount++;
                    break;
                case GameDebug.LogLevel.Warning:
                    _warningCount++;
                    break;
                case GameDebug.LogLevel.Info:
                case GameDebug.LogLevel.Debug:
                    _infoCount++;
                    break;
            }
        }
    }

    /// <summary>
    /// 清空日志
    /// </summary>
    private void ClearLogs()
    {
        _logs.Clear();
        _errorCount = 0;
        _warningCount = 0;
        _infoCount = 0;
    }
    #endregion

    #region 日志文件加载
    /// <summary>
    /// 加载日志文件
    /// </summary>
    private void LoadLogFiles()
    {
        string logDirectory = Path.Combine(Application.persistentDataPath, "logs");
        
        if (!Directory.Exists(logDirectory)) return;
        
        string[] files = Directory.GetFiles(logDirectory, "*.log");
        
        foreach (string file in files)
        {
            try
            {
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    ParseLogLine(line);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameDebugWindow] 读取日志文件失败: {ex.Message}");
            }
        }
        
        UpdateCounts();
    }

    /// <summary>
    /// 解析日志行
    /// </summary>
    private void ParseLogLine(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        
        // 格式: [yyyy-MM-dd HH:mm:ss.fff] [Level] Message
        try
        {
            // 提取时间戳
            int timeEnd = line.IndexOf(']');
            if (timeEnd < 0) return;
            
            string timeStr = line.Substring(1, timeEnd - 1);
            if (!DateTime.TryParse(timeStr, out DateTime time)) return;
            
            // 提取级别
            int levelStart = line.IndexOf("][", timeEnd);
            if (levelStart < 0) return;
            levelStart += 2;
            int levelEnd = line.IndexOf(']', levelStart);
            if (levelEnd < 0) return;
            
            string levelStr = line.Substring(levelStart, levelEnd - levelStart).Trim();
            if (!System.Enum.TryParse<GameDebug.LogLevel>(levelStr, out GameDebug.LogLevel level))
            {
                level = GameDebug.LogLevel.Info;
            }
            
            // 提取消息
            string message = line.Substring(levelEnd + 1).Trim();
            
            AddLog(message, level);
        }
        catch
        {
            // 解析失败，忽略该行
        }
    }
    #endregion

    #region 日志条目类
    /// <summary>
    /// 日志条目类
    /// </summary>
    private class LogEntry
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public GameDebug.LogLevel Level { get; set; }
        public string StackTrace { get; set; }

        public override string ToString()
        {
            string timeStr = Time.ToString("HH:mm:ss.fff");
            return $"[{timeStr}] {Message}";
        }
    }
    #endregion
}
