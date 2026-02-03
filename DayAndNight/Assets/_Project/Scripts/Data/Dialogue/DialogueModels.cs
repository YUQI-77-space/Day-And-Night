using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话系统数据模型 - 定义对话数据结构
/// 放置位置：Assets/_Project/Scripts/Data/Dialogue/DialogueModels.cs
/// </summary>
namespace DayAndNight.Dialogue
{
    #region 对话节点
    /// <summary>
    /// 单个对话节点
    /// </summary>
    [Serializable]
    public class DialogueNode
    {
        /// <summary>节点ID（唯一标识符）</summary>
        public string id;
        
        /// <summary>说话者名称</summary>
        public string speaker;
        
        /// <summary>对话内容</summary>
        public string text;
        
        /// <summary>角色头像（可选）</summary>
        public string portrait;
        
        /// <summary>分支选项列表</summary>
        public List<DialogueOption> options;
        
        /// <summary>触发的事件（可选）</summary>
        public string onEnterEvent;
        
        /// <summary>离开时触发的事件（可选）</summary>
        public string onExitEvent;
        
        /// <summary>条件判断（满足条件才显示）</summary>
        public string condition;
        
        /// <summary>下一个节点ID（-1表示对话结束）</summary>
        public string nextNode;
        
        /// <summary>是否随机播放语音</summary>
        public bool playRandomVoice;
        
        /// <summary>对话音效</summary>
        public string soundEffect;
        
        /// <summary>显示时间（0表示无限等待玩家选择）</summary>
        public float displayTime;
        
        public DialogueNode()
        {
            options = new List<DialogueOption>();
            nextNode = "";
            displayTime = 0;
            playRandomVoice = false;
        }
    }
    #endregion

    #region 对话选项
    /// <summary>
    /// 对话选项
    /// </summary>
    [Serializable]
    public class DialogueOption
    {
        /// <summary>选项文本</summary>
        public string text;
        
        /// <summary>目标节点ID</summary>
        public string targetNode;
        
        /// <summary>条件判断（满足条件才显示）</summary>
        public string condition;
        
        /// <summary>是否需要金钱</summary>
        public int cost;
        
        /// <summary>需要获得的物品</summary>
        public string requireItem;
        
        /// <summary>选项被选中时触发的事件</summary>
        public string onSelectEvent;
        
        /// <summary>是否隐藏该选项</summary>
        public bool hidden;
        
        /// <summary>是否禁用该选项</summary>
        public bool disabled;
        
        /// <summary>禁用时的提示文本</summary>
        public string disableReason;
        
        public DialogueOption()
        {
            text = "";
            targetNode = "";
            condition = "";
            cost = 0;
            requireItem = "";
            onSelectEvent = "";
            hidden = false;
            disabled = false;
            disableReason = "";
        }
    }
    #endregion

    #region 对话文件
    /// <summary>
    /// 对话文件（包含多个对话节点）
    /// </summary>
    [Serializable]
    public class DialogueFile
    {
        /// <summary>对话文件版本</summary>
        public string version = "1.0";
        
        /// <summary>对话文件名称</summary>
        public string name;
        
        /// <summary>对话描述</summary>
        public string description;
        
        /// <summary>起始节点ID</summary>
        public string startNode;
        
        /// <summary>对话节点列表</summary>
        public List<DialogueNode> nodes;
        
        /// <summary>变量定义（可选）</summary>
        public Dictionary<string, object> variables;
        
        /// <summary>对话标签（用于分类）</summary>
        public List<string> tags;
        
        /// <summary>作者</summary>
        public string author;
        
        /// <summary>创建时间</summary>
        public string createdAt;
        
        /// <summary>最后修改时间</summary>
        public string updatedAt;
        
        public DialogueFile()
        {
            version = "1.0";
            name = "New Dialogue";
            description = "";
            startNode = "start";
            nodes = new List<DialogueNode>();
            variables = new Dictionary<string, object>();
            tags = new List<string>();
            author = "Unknown";
            createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            updatedAt = createdAt;
        }
    }
    #endregion

    #region 运行时对话状态
    /// <summary>
    /// 运行时对话状态
    /// </summary>
    public class RuntimeDialogueState
    {
        /// <summary>当前对话文件</summary        public DialogueFile dialogueFile;
        
        /// <summary>当前节点ID</summary>
        public string currentNodeId;
        
        /// <summary>当前节点</summary>
        public DialogueNode currentNode;
        
        /// <summary>对话历史记录</summary>
        public List<DialogueNode> history;
        
        /// <summary>是否在显示选项中</summary>
        public bool isWaitingForInput;
        
        /// <summary>是否对话已结束</summary>
        public bool isEnded;
        
        /// <summary>对话开始时间</summary>
        public float startTime;
        
        public RuntimeDialogueState()
        {
            history = new List<DialogueNode>();
            isWaitingForInput = false;
            isEnded = false;
            startTime = 0;
        }
        
        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            currentNodeId = "";
            currentNode = null;
            history.Clear();
            isWaitingForInput = false;
            isEnded = false;
        }
    }
    #endregion

    #region 事件参数
    /// <summary>
    /// 对话事件参数
    /// </summary>
    public class DialogueEventArgs : EventArgs
    {
        /// <summary>当前对话文件</summary>
        public DialogueFile DialogueFile { get; set; }
        
        /// <summary>当前节点</summary>
        public DialogueNode CurrentNode { get; set; }
        
        /// <summary>选中的选项</summary>
        public DialogueOption SelectedOption { get; set; }
        
        /// <summary>是否是对话开始</summary        public bool IsStarting { get; set; }
        
        /// <summary>是否是对话结束</summary>
        public bool IsEnding { get; set; }
        
        public DialogueEventArgs()
        {
            DialogueFile = null;
            CurrentNode = null;
            SelectedOption = null;
            IsStarting = false;
            IsEnding = false;
        }
    }
    #endregion
}
