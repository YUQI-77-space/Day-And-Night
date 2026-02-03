using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using DayAndNight.Utilities;

/// <summary>
/// 对话管理器 - 负责加载和管理游戏对话
/// 放置位置：Assets/_Project/Scripts/Core/DialogueManager.cs
/// 
/// 使用方法：
/// 1. 将 JSON 对话文件放在 Assets/_Project/Data/Dialogues/ 目录下
/// 2. 调用 DialogueManager.Instance.StartDialogue("对话文件名") 开始对话
/// 3. 通过事件监听对话状态变化
/// </summary>
public class DialogueManager : MonoBehaviour
{
    #region 单例模式
    /// <summary>
    /// 单例实例
    /// </summary>
    public static DialogueManager Instance { get; private set; }
    #endregion

    #region 事件
    /// <summary>
    /// 对话开始事件
    /// </summary>
    public event Action<DialogueEventArgs> OnDialogueStart;
    
    /// <summary>
    /// 对话节点显示事件
    /// </summary>
    public event Action<DialogueEventArgs> OnNodeShow;
    
    /// <summary>
    /// 对话选项显示事件
    /// </summary>
    public event Action<DialogueEventArgs> OnOptionsShow;
    
    /// <summary>
    /// 选项被选中事件
    /// </summary>
    public event Action<DialogueEventArgs> OnOptionSelected;
    
    /// <summary>
    /// 对话结束事件
    /// </summary>
    public event Action<DialogueEventArgs> OnDialogueEnd;
    #endregion

    #region 私有变量
    /// <summary>
    /// 已加载的对话缓存
    /// </summary>
    private Dictionary<string, DialogueFile> _dialogueCache;
    
    /// <summary>
    /// 当前对话状态
    /// </summary>
    private RuntimeDialogueState _currentState;
    
    /// <summary>
    /// 对话数据目录路径
    /// </summary>
    private string _dialoguePath;
    
    /// <summary>
    /// 变量条件求值器
    /// </summary>
    private DialogueConditionEvaluator _conditionEvaluator;
    #endregion

    #region 公共属性
    /// <summary>
    /// 是否正在播放对话
    /// </summary>
    public bool IsPlaying => _currentState != null && !_currentState.isEnded;
    
    /// <summary>
    /// 当前对话文件
    /// </summary>
    public DialogueFile CurrentDialogue => _currentState?.dialogueFile;
    
    /// <summary>
    /// 当前节点
    /// </summary>
    public DialogueNode CurrentNode => _currentState?.currentNode;
    
    /// <summary>
    /// 当前说话者
    /// </summary>
    public string CurrentSpeaker => _currentState?.currentNode?.speaker;
    
    /// <summary>
    /// 当前对话文本
    /// </summary>
    public string CurrentText => _currentState?.currentNode?.text;
    
    /// <summary>
    /// 当前选项列表
    /// </summary>
    public List<DialogueOption> CurrentOptions => _currentState?.currentNode?.options;
    #endregion

    #region 生命周期
    /// <summary>
    /// 初始化
    /// </summary>
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 初始化变量
        _dialogueCache = new Dictionary<string, DialogueFile>();
        _currentState = new RuntimeDialogueState();
        _conditionEvaluator = new DialogueConditionEvaluator();
        
        // 设置对话数据路径
        _dialoguePath = Path.Combine(Application.dataPath, "_Project/Data/Dialogues");
        
        GameDebug.Log("[DialogueManager] 对话管理器已初始化");
    }

    /// <summary>
    /// 销毁时清理
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            _dialogueCache.Clear();
            GameDebug.Log("[DialogueManager] 对话管理器已销毁");
        }
    }
    #endregion

    #region 对话加载
    /// <summary>
    /// 加载对话文件
    /// </summary>
    /// <param name="dialogueName">对话文件名（不含扩展名）</param>
    /// <returns>对话文件</returns>
    public DialogueFile LoadDialogue(string dialogueName)
    {
        // 检查缓存
        if (_dialogueCache.TryGetValue(dialogueName, out DialogueFile cached))
        {
            return cached;
        }
        
        string filePath = Path.Combine(_dialoguePath, $"{dialogueName}.json");
        
        if (!File.Exists(filePath))
        {
            GameDebug.LogError($"[DialogueManager] 对话文件不存在: {filePath}");
            return null;
        }
        
        try
        {
            string json = File.ReadAllText(filePath);
            DialogueFile dialogue = JsonConvert.DeserializeObject<DialogueFile>(json);
            
            // 验证对话文件
            if (ValidateDialogue(dialogue))
            {
                _dialogueCache[dialogueName] = dialogue;
                GameDebug.Log($"[DialogueManager] 加载对话文件成功: {dialogueName}");
                return dialogue;
            }
            else
            {
                GameDebug.LogError($"[DialogueManager] 对话文件验证失败: {dialogueName}");
                return null;
            }
        }
        catch (Exception ex)
        {
            GameDebug.LogError($"[DialogueManager] 加载对话文件失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 验证对话文件
    /// </summary>
    private bool ValidateDialogue(DialogueFile dialogue)
    {
        if (dialogue == null) return false;
        if (dialogue.nodes == null || dialogue.nodes.Count == 0) return false;
        if (string.IsNullOrEmpty(dialogue.startNode)) return false;
        
        // 检查起始节点是否存在
        foreach (var node in dialogue.nodes)
        {
            if (node.id == dialogue.startNode)
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// 预加载对话文件
    /// </summary>
    /// <param name="dialogueNames">对话文件名列表</param>
    public void PreloadDialogues(List<string> dialogueNames)
    {
        foreach (var name in dialogueNames)
        {
            LoadDialogue(name);
        }
        GameDebug.Log($"[DialogueManager] 预加载了 {dialogueNames.Count} 个对话文件");
    }
    #endregion

    #region 对话控制
    /// <summary>
    /// 开始对话
    /// </summary>
    /// <param name="dialogueName">对话文件名</param>
    /// <param name="startNode">起始节点ID（可选，默认使用对话文件配置）</param>
    /// <returns>是否成功开始</returns>
    public bool StartDialogue(string dialogueName, string startNode = null)
    {
        DialogueFile dialogue = LoadDialogue(dialogueName);
        if (dialogue == null) return false;
        
        // 重置状态
        _currentState.Reset();
        _currentState.dialogueFile = dialogue;
        _currentState.startTime = Time.time;
        
        // 确定起始节点
        string nodeId = startNode ?? dialogue.startNode;
        
        // 开始第一个节点
        return ShowNode(nodeId, true);
    }

    /// <summary>
    /// 显示指定节点
    /// </summary>
    private bool ShowNode(string nodeId, bool isStarting = false)
    {
        if (_currentState.dialogueFile == null)
        {
            GameDebug.LogError("[DialogueManager] 没有正在进行的对话");
            return false;
        }
        
        // 查找节点
        DialogueNode node = null;
        foreach (var n in _currentState.dialogueFile.nodes)
        {
            if (n.id == nodeId)
            {
                node = n;
                break;
            }
        }
        
        if (node == null)
        {
            GameDebug.LogError($"[DialogueManager] 找不到节点: {nodeId}");
            EndDialogue();
            return false;
        }
        
        // 处理离开事件
        if (_currentState.currentNode != null && !string.IsNullOrEmpty(_currentState.currentNode.onExitEvent))
        {
            TriggerEvent(_currentState.currentNode.onExitEvent);
        }
        
        // 更新状态
        _currentState.currentNodeId = nodeId;
        _currentState.currentNode = node;
        _currentState.history.Add(node);
        
        // 触发进入事件
        if (!string.IsNullOrEmpty(node.onEnterEvent))
        {
            TriggerEvent(node.onEnterEvent);
        }
        
        // 发送事件
        var args = new DialogueEventArgs
        {
            DialogueFile = _currentState.dialogueFile,
            CurrentNode = node,
            IsStarting = isStarting
        };
        
        OnNodeShow?.Invoke(args);
        
        // 检查是否有选项
        if (node.options != null && node.options.Count > 0)
        {
            // 过滤可用选项
            List<DialogueOption> availableOptions = FilterOptions(node.options);
            _currentState.isWaitingForInput = true;
            
            if (availableOptions.Count > 0)
            {
                OnOptionsShow?.Invoke(args);
                return true;
            }
        }
        
        // 检查是否结束
        if (string.IsNullOrEmpty(node.nextNode) || node.nextNode == "-1")
        {
            EndDialogue();
            return false;
        }
        
        // 自动进入下一个节点
        _currentState.isWaitingForInput = false;
        StartCoroutine(DelayNextNode(node.nextNode));
        return true;
    }

    /// <summary>
    /// 延迟进入下一个节点
    /// </summary>
    private IEnumerator DelayNextNode(string nodeId)
    {
        yield return new WaitForSeconds(0.5f);
        ShowNode(nodeId);
    }

    /// <summary>
    /// 选择选项
    /// </summary>
    /// <param name="optionIndex">选项索引</param>
    public void SelectOption(int optionIndex)
    {
        if (!IsPlaying || !_currentState.isWaitingForInput)
        {
            GameDebug.LogWarning("[DialogueManager] 当前没有等待玩家选择");
            return;
        }
        
        var options = _currentState.currentNode.options;
        if (optionIndex < 0 || optionIndex >= options.Count)
        {
            GameDebug.LogError($"[DialogueManager] 无效的选项索引: {optionIndex}");
            return;
        }
        
        DialogueOption option = options[optionIndex];
        
        // 发送选项选中事件
        var args = new DialogueEventArgs
        {
            DialogueFile = _currentState.dialogueFile,
            CurrentNode = _currentState.currentNode,
            SelectedOption = option
        };
        OnOptionSelected?.Invoke(args);
        
        // 触发选项事件
        if (!string.IsNullOrEmpty(option.onSelectEvent))
        {
            TriggerEvent(option.onSelectEvent);
        }
        
        _currentState.isWaitingForInput = false;
        
        // 进入目标节点
        if (!string.IsNullOrEmpty(option.targetNode))
        {
            ShowNode(option.targetNode);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// 跳过当前节点（继续到下一个）
    /// </summary>
    public void SkipCurrentNode()
    {
        if (!IsPlaying || _currentState.isWaitingForInput) return;
        
        var node = _currentState.currentNode;
        if (node == null) return;
        
        if (!string.IsNullOrEmpty(node.nextNode) && node.nextNode != "-1")
        {
            ShowNode(node.nextNode);
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        if (_currentState.dialogueFile == null) return;
        
        // 发送结束事件
        var args = new DialogueEventArgs
        {
            DialogueFile = _currentState.dialogueFile,
            CurrentNode = _currentState.currentNode,
            IsEnding = true
        };
        OnDialogueEnd?.Invoke(args);
        
        // 重置状态
        _currentState.Reset();
        
        GameDebug.Log($"[DialogueManager] 对话结束");
    }
    #endregion

    #region 条件处理
    /// <summary>
    /// 过滤可用选项
    /// </summary>
    private List<DialogueOption> FilterOptions(List<DialogueOption> options)
    {
        List<DialogueOption> available = new List<DialogueOption>();
        
        foreach (var option in options)
        {
            // 跳过隐藏选项
            if (option.hidden) continue;
            
            // 检查条件
            if (!string.IsNullOrEmpty(option.condition))
            {
                if (!_conditionEvaluator.Evaluate(option.condition))
                {
                    continue;
                }
            }
            
            available.Add(option);
        }
        
        return available;
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    private void TriggerEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        
        GameDebug.Log($"[DialogueManager] 触发事件: {eventName}");
        
        // 事件处理可以通过事件系统扩展
        // 这里使用简单的事件名解析
        if (eventName.StartsWith("GiveGold:"))
        {
            string amountStr = eventName.Replace("GiveGold:", "");
            if (int.TryParse(amountStr, out int amount))
            {
                // 通知游戏系统给予金币
                // GameManager.Instance.AddGold(amount);
            }
        }
        else if (eventName.StartsWith("SetFlag:"))
        {
            string flagName = eventName.Replace("SetFlag:", "");
            // PlayerPrefs.SetInt(flagName, 1);
        }
        // 可以根据需要扩展更多事件类型
    }
    #endregion

    #region 调试功能
    /// <summary>
    /// 获取所有已加载的对话名称
    /// </summary>
    public List<string> GetLoadedDialogueNames()
    {
        return new List<string>(_dialogueCache.Keys);
    }

    /// <summary>
    /// 获取对话统计信息
    /// </summary>
    public string GetDialogueStats()
    {
        return $"已加载对话: {_dialogueCache.Count} 个, " +
               $"当前对话: {(CurrentDialogue?.name ?? "无")}, " +
               $"当前节点: {_currentState.currentNodeId ?? "无"}";
    }
    #endregion
}

/// <summary>
/// 条件求值器 - 用于解析和执行条件表达式
/// </summary>
public class DialogueConditionEvaluator
{
    /// <summary>
    /// 求值条件表达式
    /// 支持的语法：
    /// - $variable == value (等于)
    /// - $variable != value (不等于)
    /// - $variable > value (大于)
    /// - $variable < value (小于)
    /// - $variable >= value (大于等于)
    /// - $variable <= value (小于等于)
    /// - $variable (变量存在且为真)
    /// </summary>
    public bool Evaluate(string condition)
    {
        if (string.IsNullOrEmpty(condition)) return true;
        
        condition = condition.Trim();
        
        try
        {
            // 处理等于
            if (condition.Contains("=="))
            {
                var parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
                return Compare(parts[0].Trim(), parts[1].Trim(), "==");
            }
            
            // 处理不等于
            if (condition.Contains("!="))
            {
                var parts = condition.Split(new[] { "!=" }, StringSplitOptions.None);
                return Compare(parts[0].Trim(), parts[1].Trim(), "!=");
            }
            
            // 处理大于等于
            if (condition.Contains(">="))
            {
                var parts = condition.Split(new[] { ">=" }, StringSplitOptions.None);
                return Compare(parts[0].Trim(), parts[1].Trim(), ">=");
            }
            
            // 处理小于等于
            if (condition.Contains("<="))
            {
                var parts = condition.Split(new[] { "<=" }, StringSplitOptions.None);
                return Compare(parts[0].Trim(), parts[1].Trim(), "<=");
            }
            
            // 处理大于
            if (condition.Contains(">"))
            {
                var parts = condition.Split(new[] { ">" }, StringSplitOptions.None);
                return Compare(parts[0].Trim(), parts[1].Trim(), ">");
            }
            
            // 处理小于
            if (condition.Contains("<"))
            {
                var parts = condition.Split(new[] { "<" }, StringSplitOptions.None);
                return Compare(parts[0].Trim(), parts[1].Trim(), "<");
            }
            
            // 简单的变量检查
            if (condition.StartsWith("$"))
            {
                return GetBoolValue(condition);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            GameDebug.LogError($"[DialogueConditionEvaluator] 条件求值失败: {condition}, 错误: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// 比较两个值
    /// </summary>
    private bool Compare(string left, string right, string op)
    {
        // 尝试数值比较
        if (float.TryParse(left, out float leftNum) && float.TryParse(right, out float rightNum))
        {
            switch (op)
            {
                case "==": return leftNum == rightNum;
                case "!=": return leftNum != rightNum;
                case ">": return leftNum > rightNum;
                case "<": return leftNum < rightNum;
                case ">=": return leftNum >= rightNum;
                case "<=": return leftNum <= rightNum;
            }
        }
        
        // 字符串比较
        switch (op)
        {
            case "==": return left == right;
            case "!=": return left != right;
        }
        
        return false;
    }

    /// <summary>
    /// 获取布尔值（从游戏状态系统获取）
    /// </summary>
    private bool GetBoolValue(string variableName)
    {
        // 这里应该从游戏状态系统获取变量值
        // 临时实现：使用 PlayerPrefs
        return PlayerPrefs.GetInt(variableName, 0) == 1;
    }
}
