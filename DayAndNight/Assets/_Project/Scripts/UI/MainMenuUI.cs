using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DayAndNight.Core
{
    /// <summary>
    /// 主菜单界面
    /// 提供开始游戏、继续游戏、设置、关于和退出游戏功能
    /// </summary>
    public class MainMenuUI : UIBasePanel
    {
        #region 序列化字段

        [Header("主菜单按钮")]
        [SerializeField]
        private Button _newGameButton;

        [SerializeField]
        private Button _continueGameButton;

        [SerializeField]
        private Button _settingsButton;

        [SerializeField]
        private Button _aboutButton;

        [SerializeField]
        private Button _quitButton;

        [Header("设置面板")]
        [SerializeField]
        private GameObject _settingsPanel;

        [SerializeField]
        private Button _settingsCloseButton;

        [Header("关于面板")]
        [SerializeField]
        private GameObject _aboutPanel;

        [SerializeField]
        private Text _versionText;

        [SerializeField]
        private Button _aboutCloseButton;

        [Header("存档选择面板")]
        [SerializeField]
        private GameObject _saveSlotPanel;

        [SerializeField]
        private Transform _saveSlotContainer;

        [SerializeField]
        private Button _saveSlotCloseButton;

        #endregion

        #region 私有字段

        private bool _isSettingsOpen = false;
        private bool _isAboutOpen = false;
        private bool _isSaveSlotOpen = false;

        #endregion

        #region Unity生命周期方法

        private void Awake()
        {
            // 如果没有设置面板类型，使用默认值
            if (_panelType == UIPanelType.None)
            {
                _panelType = UIPanelType.MainMenu;
            }
        }

        private void OnDestroy()
        {
            // 移除按钮监听
            RemoveButtonListeners();
        }

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 初始化回调
        /// </summary>
        protected override void OnInitialize()
        {
            // 添加按钮监听
            AddButtonListeners();

            // 初始化版本信息
            if (_versionText != null)
            {
                _versionText.text = $"Version {Application.version}";
            }

            // 更新继续游戏按钮状态
            UpdateContinueButtonState();

            // 初始时隐藏子面板
            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(false);
            }

            if (_aboutPanel != null)
            {
                _aboutPanel.SetActive(false);
            }

            if (_saveSlotPanel != null)
            {
                _saveSlotPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示回调
        /// </summary>
        protected override void OnShow()
        {
            // 更新继续游戏按钮状态
            UpdateContinueButtonState();

            // 播放背景音乐
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic("MainMenu", true, true);
            }

            Debug.Log("[MainMenuUI] 主菜单已显示");
        }

        /// <summary>
        /// 隐藏回调
        /// </summary>
        protected override void OnHide()
        {
            // 关闭所有子面板
            CloseAllSubPanels();
        }

        #endregion

        #region 按钮事件处理

        /// <summary>
        /// 开始新游戏按钮点击
        /// </summary>
        public void OnNewGameButtonClicked()
        {
            Debug.Log("[MainMenuUI] 开始新游戏");

            // 播放按钮音效
            PlayButtonSound();

            // 显示存档槽位选择
            ShowSaveSlotPanel(true);
        }

        /// <summary>
        /// 继续游戏按钮点击
        /// </summary>
        public void OnContinueGameButtonClicked()
        {
            Debug.Log("[MainMenuUI] 继续游戏");

            // 播放按钮音效
            PlayButtonSound();

            // 检查是否有存档
            if (SaveManager.Instance != null && SaveManager.Instance.GetAllSaveSlots().Exists(s => s.HasSave))
            {
                // 显示存档槽位选择
                ShowSaveSlotPanel(false);
            }
            else
            {
                // 没有存档，显示提示
                ShowToast("没有存档可继续");
            }
        }

        /// <summary>
        /// 设置按钮点击
        /// </summary>
        public void OnSettingsButtonClicked()
        {
            Debug.Log("[MainMenuUI] 打开设置");

            // 播放按钮音效
            PlayButtonSound();

            // 显示设置面板
            ToggleSettingsPanel();
        }

        /// <summary>
        /// 关于按钮点击
        /// </summary>
        public void OnAboutButtonClicked()
        {
            Debug.Log("[MainMenuUI] 打开关于");

            // 播放按钮音效
            PlayButtonSound();

            // 显示关于面板
            ToggleAboutPanel();
        }

        /// <summary>
        /// 退出游戏按钮点击
        /// </summary>
        public void OnQuitButtonClicked()
        {
            Debug.Log("[MainMenuUI] 退出游戏");

            // 播放按钮音效
            PlayButtonSound();

            // 确认退出
            StartCoroutine(QuitGameRoutine());
        }

        /// <summary>
        /// 设置面板关闭按钮点击
        /// </summary>
        public void OnSettingsCloseButtonClicked()
        {
            ToggleSettingsPanel();
        }

        /// <summary>
        /// 关于面板关闭按钮点击
        /// </summary>
        public void OnAboutCloseButtonClicked()
        {
            ToggleAboutPanel();
        }

        /// <summary>
        /// 存档槽位关闭按钮点击
        /// </summary>
        public void OnSaveSlotCloseButtonClicked()
        {
            ShowSaveSlotPanel(false);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 添加按钮监听
        /// </summary>
        private void AddButtonListeners()
        {
            if (_newGameButton != null)
            {
                _newGameButton.onClick.AddListener(OnNewGameButtonClicked);
            }

            if (_continueGameButton != null)
            {
                _continueGameButton.onClick.AddListener(OnContinueGameButtonClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }

            if (_aboutButton != null)
            {
                _aboutButton.onClick.AddListener(OnAboutButtonClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitButtonClicked);
            }

            if (_settingsCloseButton != null)
            {
                _settingsCloseButton.onClick.AddListener(OnSettingsCloseButtonClicked);
            }

            if (_aboutCloseButton != null)
            {
                _aboutCloseButton.onClick.AddListener(OnAboutCloseButtonClicked);
            }

            if (_saveSlotCloseButton != null)
            {
                _saveSlotCloseButton.onClick.AddListener(OnSaveSlotCloseButtonClicked);
            }
        }

        /// <summary>
        /// 移除按钮监听
        /// </summary>
        private void RemoveButtonListeners()
        {
            if (_newGameButton != null)
            {
                _newGameButton.onClick.RemoveListener(OnNewGameButtonClicked);
            }

            if (_continueGameButton != null)
            {
                _continueGameButton.onClick.RemoveListener(OnContinueGameButtonClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            }

            if (_aboutButton != null)
            {
                _aboutButton.onClick.RemoveListener(OnAboutButtonClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }

            if (_settingsCloseButton != null)
            {
                _settingsCloseButton.onClick.RemoveListener(OnSettingsCloseButtonClicked);
            }

            if (_aboutCloseButton != null)
            {
                _aboutCloseButton.onClick.RemoveListener(OnAboutCloseButtonClicked);
            }

            if (_saveSlotCloseButton != null)
            {
                _saveSlotCloseButton.onClick.RemoveListener(OnSaveSlotCloseButtonClicked);
            }
        }

        /// <summary>
        /// 切换设置面板
        /// </summary>
        private void ToggleSettingsPanel()
        {
            if (_settingsPanel != null)
            {
                _isSettingsOpen = !_isSettingsOpen;
                _settingsPanel.SetActive(_isSettingsOpen);
            }
        }

        /// <summary>
        /// 切换关于面板
        /// </summary>
        private void ToggleAboutPanel()
        {
            if (_aboutPanel != null)
            {
                _isAboutOpen = !_isAboutOpen;
                _aboutPanel.SetActive(_isAboutOpen);
            }
        }

        /// <summary>
        /// 显示/隐藏存档槽位面板
        /// </summary>
        private void ShowSaveSlotPanel(bool show)
        {
            if (_saveSlotPanel != null)
            {
                _isSaveSlotOpen = show;
                _saveSlotPanel.SetActive(_isSaveSlotOpen);

                if (show)
                {
                    // 刷新存档槽位列表
                    RefreshSaveSlotList();
                }
            }
        }

        /// <summary>
        /// 关闭所有子面板
        /// </summary>
        private void CloseAllSubPanels()
        {
            _isSettingsOpen = false;
            _isAboutOpen = false;
            _isSaveSlotOpen = false;

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(false);
            }

            if (_aboutPanel != null)
            {
                _aboutPanel.SetActive(false);
            }

            if (_saveSlotPanel != null)
            {
                _saveSlotPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 刷新存档槽位列表
        /// </summary>
        private void RefreshSaveSlotList()
        {
            if (SaveManager.Instance == null || _saveSlotContainer == null)
            {
                return;
            }

            // 清除现有项
            foreach (Transform child in _saveSlotContainer)
            {
                Destroy(child.gameObject);
            }

            // 创建存档槽位项
            var saveSlots = SaveManager.Instance.GetAllSaveSlots();

            foreach (var saveSlot in saveSlots)
            {
                CreateSaveSlotItem(saveSlot);
            }
        }

        /// <summary>
        /// 创建存档槽位项
        /// </summary>
        private void CreateSaveSlotItem(SaveSlotInfo saveSlot)
        {
            // 这里可以实例化一个预制体，或者直接创建UI元素
            // 由于是示例代码，这里只记录日志
            Debug.Log($"[MainMenuUI] 存档槽位 {saveSlot.SlotIndex}: {(saveSlot.HasSave ? $"保存于 {saveSlot.SaveTime}" : "空")}");
        }

        /// <summary>
        /// 选择存档槽位
        /// </summary>
        public void SelectSaveSlot(int slotIndex)
        {
            Debug.Log($"[MainMenuUI] 选择存档槽位: {slotIndex}");

            // 播放按钮音效
            PlayButtonSound();

            // 关闭存档面板
            ShowSaveSlotPanel(false);

            // 开始游戏
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(slotIndex);
            }
        }

        /// <summary>
        /// 更新继续游戏按钮状态
        /// </summary>
        private void UpdateContinueButtonState()
        {
            if (_continueGameButton != null)
            {
                bool hasSave = false;
                if (SaveManager.Instance != null)
                {
                    hasSave = SaveManager.Instance.GetAllSaveSlots().Exists(s => s.HasSave);
                }
                _continueGameButton.interactable = hasSave;
            }
        }

        /// <summary>
        /// 播放按钮音效
        /// </summary>
        private void PlayButtonSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ButtonClick");
            }
        }

        /// <summary>
        /// 显示提示信息
        /// </summary>
        private void ShowToast(string message)
        {
            Debug.Log($"[MainMenuUI] 提示: {message}");
            // TODO: 实现Toast显示
        }

        /// <summary>
        /// 退出游戏协程
        /// </summary>
        private IEnumerator QuitGameRoutine()
        {
            // 播放退出音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("QuitGame");
            }

            // 等待音效播放
            yield return new WaitForSeconds(0.5f);

            // 退出游戏
            Application.Quit();
        }

        #endregion
    }
}
