using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DayAndNight.Dialogue;

/// <summary>
/// 对话 UI 显示组件 - 管理对话的图形界面显示
/// 放置位置：Assets/_Project/Scripts/UI/DialogueUI.cs
/// 
/// 使用方法：
/// 1. 创建包含以下元素的 UI 面板：
///    - 对话框背景 (Panel)
///    - 说话者名称 (TextMeshProUGUI)
///    - 对话内容 (TextMeshProUGUI)
///    - 选项容器 (VerticalLayoutGroup)
///    - 选项按钮预设 (Button)
///    - 继续提示 (Image/Text)
/// 2. 将 DialogueUI 组件添加到面板
/// 3. 订阅 DialogueManager 的事件
/// </summary>
public class DialogueUI : MonoBehaviour
{
    #region UI 引用
    [Header("核心组件")]
    [Tooltip("对话框背景")]
    [SerializeField] private GameObject _dialoguePanel;
    
    [Tooltip("说话者名称文本")]
    [SerializeField] private TextMeshProUGUI _speakerNameText;
    
    [Tooltip("对话内容文本")]
    [SerializeField] private TextMeshProUGUI _dialogueText;
    
    [Tooltip("选项容器")]
    [SerializeField] private GameObject _optionsContainer;
    
    [Tooltip("选项按钮预设")]
    [SerializeField] private GameObject _optionButtonPrefab;
    
    [Tooltip("继续提示图标")]
    [SerializeField] private GameObject _continueHint;

    [Header("角色头像")]
    [Tooltip("头像图片")]
    [SerializeField] private Image _portraitImage;
    
    [Header("打字机效果")]
    [Tooltip("是否启用打字机效果")]
    [SerializeField] private bool _useTypewriter = true;
    
    [Tooltip("打字速度（字符/秒）")]
    [SerializeField] private float _typewriterSpeed = 30f;

    [Header("对话历史")]
    [Tooltip("历史记录按钮")]
    [SerializeField] private Button _historyButton;
    
    [Tooltip("历史记录面板")]
    [SerializeField] private GameObject _historyPanel;
    
    [Tooltip("历史记录文本")]
    [SerializeField] private TextMeshProUGUI _historyText;
    #endregion

    #region 私有变量
    /// <summary>
    /// 选项按钮池
    /// </summary>
    private List<GameObject> _optionButtonPool;
    
    /// <summary>
    /// 打字机协程
    /// </summary>
    private Coroutine _typewriterCoroutine;
    
    /// <summary>
    /// 完整文本缓存
    /// </summary>
    private string _fullText;
    
    /// <summary>
    /// 对话历史记录
    /// </summary>
    private List<string> _dialogueHistory;
    
    /// <summary>
    /// 是否正在显示打字效果
    /// </summary>
    private bool _isTypewriting;
    
    /// <summary>
    /// 是否已显示完整文本
    /// </summary>
    private bool _textCompleted;
    
    /// <summary>
    /// 自动继续计时器
    /// </summary>
    private float _autoContinueTimer;
    
    /// <summary>
    /// 自动继续时间
    /// </summary>
    private float _autoContinueTime = 3f;
    #endregion

    #region 生命周期
    /// <summary>
    /// 初始化
    /// </summary>
    private void Awake()
    {
        // 验证必要组件
        ValidateComponents();
        
        // 初始化变量
        _optionButtonPool = new List<GameObject>();
        _dialogueHistory = new List<string>();
        
        // 初始化 UI 状态
        _dialoguePanel.SetActive(false);
        _optionsContainer.SetActive(false);
        _historyPanel.SetActive(false);
        
        GameDebug.Log("[DialogueUI] 对话 UI 已初始化");
    }

    /// <summary>
    /// 组件验证
    /// </summary>
    private void ValidateComponents()
    {
        if (_dialoguePanel == null)
            _dialoguePanel = transform.Find("DialoguePanel")?.gameObject;
        if (_speakerNameText == null)
            _speakerNameText = transform.Find("DialoguePanel/SpeakerName")?.GetComponent<TextMeshProUGUI>();
        if (_dialogueText == null)
            _dialogueText = transform.Find("DialoguePanel/DialogueText")?.GetComponent<TextMeshProUGUI>();
        if (_optionsContainer == null)
            _optionsContainer = transform.Find("DialoguePanel/OptionsContainer")?.gameObject;
        if (_portraitImage == null)
            _portraitImage = transform.Find("DialoguePanel/Portrait")?.GetComponent<Image>();
    }

    /// <summary>
    /// 订阅事件
    /// </summary>
    private void OnEnable()
    {
        SubscribeToDialogueEvents();
    }

    /// <summary>
    /// 取消订阅
    /// </summary>
    private void OnDisable()
    {
        UnsubscribeFromDialogueEvents();
    }
    #endregion

    #region 事件订阅
    /// <summary>
    /// 订阅对话事件
    /// </summary>
    private void SubscribeToDialogueEvents()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart += HandleDialogueStart;
            DialogueManager.Instance.OnNodeShow += HandleNodeShow;
            DialogueManager.Instance.OnOptionsShow += HandleOptionsShow;
            DialogueManager.Instance.OnOptionSelected += HandleOptionSelected;
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        }
    }

    /// <summary>
    /// 取消订阅对话事件
    /// </summary>
    private void UnsubscribeFromDialogueEvents()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart -= HandleDialogueStart;
            DialogueManager.Instance.OnNodeShow -= HandleNodeShow;
            DialogueManager.Instance.OnOptionsShow -= HandleOptionsShow;
            DialogueManager.Instance.OnOptionSelected -= HandleOptionSelected;
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }
    #endregion

    #region 事件处理
    /// <summary>
    /// 处理对话开始
    /// </summary>
    private void HandleDialogueStart(DialogueEventArgs args)
    {
        ShowPanel();
        _dialogueHistory.Clear();
        
        GameDebug.Log("[DialogueUI] 对话开始显示");
    }

    /// <summary>
    /// 处理节点显示
    /// </summary>
    private void HandleNodeShow(DialogueEventArgs args)
    {
        if (args.CurrentNode == null) return;
        
        var node = args.CurrentNode;
        
        // 更新说话者名称
        _speakerNameText.text = node.speaker ?? "???";
        
        // 更新对话内容
        _fullText = node.text ?? "";
        
        // 开始打字机效果
        if (_useTypewriter && !args.IsStarting)
        {
            StartTypewriter(_fullText);
        }
        else
        {
            _dialogueText.text = _fullText;
            _textCompleted = true;
        }
        
        // 更新头像
        if (!string.IsNullOrEmpty(node.portrait))
        {
            LoadPortrait(node.portrait);
        }
        
        // 添加到历史记录
        string historyEntry = $"<color=#888888>[{node.speaker}]</color> {node.text}";
        _dialogueHistory.Add(historyEntry);
        
        // 重置自动继续计时器
        _autoContinueTimer = 0f;
        
        // 隐藏继续提示
        _continueHint.SetActive(false);
        
        GameDebug.Log($"[DialogueUI] 显示节点: {node.id}");
    }

    /// <summary>
    /// 处理选项显示
    /// </summary>
    private void HandleOptionsShow(DialogueEventArgs args)
    {
        if (args.CurrentNode == null || args.CurrentNode.options == null) return;
        
        // 停止打字机效果
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _dialogueText.text = _fullText;
            _textCompleted = true;
        }
        
        // 显示完整文本
        _dialogueText.text = _fullText;
        _textCompleted = true;
        
        // 创建选项按钮
        CreateOptionButtons(args.CurrentNode.options);
        
        // 显示选项容器
        _optionsContainer.SetActive(true);
        
        // 显示继续提示
        _continueHint.SetActive(true);
        
        GameDebug.Log($"[DialogueUI] 显示 {args.CurrentNode.options.Count} 个选项");
    }

    /// <summary>
    /// 处理选项选中
    /// </summary>
    private void HandleOptionSelected(DialogueEventArgs args)
    {
        // 隐藏选项容器
        _optionsContainer.SetActive(false);
        ClearOptionButtons();
        
        GameDebug.Log($"[DialogueUI] 选中选项: {args.SelectedOption?.text}");
    }

    /// <summary>
    /// 处理对话结束
    /// </summary>
    private void HandleDialogueEnd(DialogueEventArgs args)
    {
        StartCoroutine(HidePanelDelayed(0.5f));
        
        GameDebug.Log("[DialogueUI] 对话结束隐藏");
    }
    #endregion

    #region UI 控制
    /// <summary>
    /// 显示面板
    /// </summary>
    private void ShowPanel()
    {
        _dialoguePanel.SetActive(true);
        _isTypewriting = false;
        _textCompleted = false;
    }

    /// <summary>
    /// 延迟隐藏面板
    /// </summary>
    private IEnumerator HidePanelDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        _dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// 加载头像
    /// </summary>
    private void LoadPortrait(string portraitPath)
    {
        if (_portraitImage == null) return;
        
        var sprite = Resources.Load<Sprite>(portraitPath);
        if (sprite != null)
        {
            _portraitImage.sprite = sprite;
            _portraitImage.gameObject.SetActive(true);
        }
        else
        {
            _portraitImage.gameObject.SetActive(false);
        }
    }
    #endregion

    #region 打字机效果
    /// <summary>
    /// 开始打字机效果
    /// </summary>
    private void StartTypewriter(string text)
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
        }
        
        _typewriterCoroutine = StartCoroutine(TypewriterRoutine(text));
    }

    /// <summary>
    /// 打字机协程
    /// </summary>
    private IEnumerator TypewriterRoutine(string text)
    {
        _isTypewriting = true;
        _textCompleted = false;
        _dialogueText.text = "";
        
        int charIndex = 0;
        float timer = 0f;
        
        while (charIndex < text.Length)
        {
            timer += Time.unscaledDeltaTime;
            
            // 根据字符类型调整速度
            float charDelay = 1f / _typewriterSpeed;
            if (char.IsPunctuation(text[charIndex]))
            {
                charDelay *= 2f; // 标点符号暂停更久
            }
            
            if (timer >= charDelay)
            {
                timer = 0f;
                _dialogueText.text += text[charIndex];
                charIndex++;
                
                // 可选：播放打字音效
                // PlayTypeSound();
            }
            
            yield return null;
        }
        
        // 打字完成
        _dialogueText.text = text;
        _isTypewriting = false;
        _textCompleted = true;
    }

    /// <summary>
    /// 跳过打字机效果
    /// </summary>
    public void SkipTypewriter()
    {
        if (_isTypewriting)
        {
            StopCoroutine(_typewriterCoroutine);
            _dialogueText.text = _fullText;
            _isTypewriting = false;
            _textCompleted = true;
        }
    }
    #endregion

    #region 选项按钮
    /// <summary>
    /// 创建选项按钮
    /// </summary>
    private void CreateOptionButtons(List<DialogueOption> options)
    {
        ClearOptionButtons();
        
        if (options == null || options.Count == 0) return;
        
        foreach (var option in options)
        {
            CreateOptionButton(option);
        }
    }

    /// <summary>
    /// 创建单个选项按钮
    /// </summary>
    private void CreateOptionButton(DialogueOption option)
    {
        GameObject buttonObj;
        
        // 从池中获取或创建新按钮
        if (_optionButtonPool.Count > 0)
        {
            buttonObj = _optionButtonPool[0];
            _optionButtonPool.RemoveAt(0);
        }
        else
        {
            buttonObj = Instantiate(_optionButtonPrefab, _optionsContainer.transform);
        }
        
        // 设置按钮文本
        var button = buttonObj.GetComponent<Button>();
        var textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = option.text;
            
            // 设置禁用状态
            button.interactable = !option.disabled;
            if (option.disabled)
            {
                textComponent.color = Color.gray;
                if (!string.IsNullOrEmpty(option.disableReason))
                {
                    textComponent.text += $" <size=80%>({option.disableReason})</size>";
                }
            }
            else
            {
                textComponent.color = Color.white;
            }
        }
        
        // 添加点击事件
        int index = _optionsContainer.transform.childCount - 1;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnOptionButtonClick(index));
        
        buttonObj.SetActive(true);
    }

    /// <summary>
    /// 清除选项按钮
    /// </summary>
    private void ClearOptionButtons()
    {
        foreach (Transform child in _optionsContainer.transform)
        {
            child.gameObject.SetActive(false);
            _optionButtonPool.Add(child.gameObject);
        }
    }

    /// <summary>
    /// 选项按钮点击事件
    /// </summary>
    private void OnOptionButtonClick(int index)
    {
        DialogueManager.Instance.SelectOption(index);
    }
    #endregion

    #region 用户输入
    /// <summary>
    /// 处理用户输入
    /// </summary>
    private void Update()
    {
        if (!_dialoguePanel.activeSelf) return;
        
        // 鼠标点击或空格键继续
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (_isTypewriting)
            {
                // 跳过打字机效果
                SkipTypewriter();
            }
            else if (DialogueManager.Instance.CurrentOptions == null || 
                     DialogueManager.Instance.CurrentOptions.Count == 0)
            {
                // 没有选项，继续下一个节点
                DialogueManager.Instance.SkipCurrentNode();
            }
        }
        
        // ESC 取消对话
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DialogueManager.Instance.EndDialogue();
        }
    }
    #endregion

    #region 公开方法
    /// <summary>
    /// 切换历史记录显示
    /// </summary>
    public void ToggleHistory()
    {
        _historyPanel.SetActive(!_historyPanel.activeSelf);
        
        if (_historyPanel.activeSelf)
        {
            UpdateHistoryDisplay();
        }
    }

    /// <summary>
    /// 更新历史记录显示
    /// </summary>
    private void UpdateHistoryDisplay()
    {
        _historyText.text = string.Join("\n\n", _dialogueHistory);
    }

    /// <summary>
    /// 设置打字机效果开关
    /// </summary>
    public void SetTypewriterEnabled(bool enabled)
    {
        _useTypewriter = enabled;
    }

    /// <summary>
    /// 设置打字速度
    /// </summary>
    public void SetTypewriterSpeed(float speed)
    {
        _typewriterSpeed = Mathf.Max(1f, speed);
    }

    /// <summary>
    /// 显示调试信息
    /// </summary>
    public void ShowDebugInfo()
    {
        GameDebug.Log($"[DialogueUI] 状态: 面板={_dialoguePanel.activeSelf}, " +
                     $"打字中={_isTypewriting}, " +
                     $"选项数={DialogueManager.Instance.CurrentOptions?.Count ?? 0}");
    }
    #endregion
}
