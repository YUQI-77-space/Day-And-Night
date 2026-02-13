using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DayAndNight.Core
{
    /// <summary>
    /// 暂停菜单界面
    /// 提供继续游戏、保存游戏、设置、返回主菜单和退出游戏功能
    /// </summary>
    public class PauseMenuUI : UIBasePanel
    {
        #region 序列化字段

        [Header("暂停菜单按钮")]
        [SerializeField]
        private Button _continueButton;

        [SerializeField]
        private Button _saveGameButton;

        [SerializeField]
        private Button _settingsButton;

        [SerializeField]
        private Button _mainMenuButton;

        [SerializeField]
        private Button _quitButton;

        [Header("确认对话框")]
        [SerializeField]
        private GameObject _confirmDialog;

        [SerializeField]
        private Text _confirmMessageText;

        [SerializeField]
        private Button _confirmButton;

        [SerializeField]
        private Button _cancelButton;

        [Header("保存成功提示")]
        [SerializeField]
        private GameObject _saveSuccessToast;

        [SerializeField]
        private Text _saveSuccessText;

        #endregion

        #region 私有字段

        private bool _isConfirmDialogOpen = false;
        private ConfirmAction _pendingAction = ConfirmAction.None;
        private Coroutine _toastCoroutine;

        #endregion

        #region 确认操作类型

        private enum ConfirmAction
        {
            None,
            ReturnToMainMenu,
            QuitGame
        }

        #endregion

        #region Unity生命周期方法

        private void Awake()
        {
            // 如果没有设置面板类型，使用默认值
            if (_panelType == UIPanelType.None)
            {
                _panelType = UIPanelType.PauseMenu;
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

            // 初始时隐藏确认对话框
            if (_confirmDialog != null)
            {
                _confirmDialog.SetActive(false);
            }

            if (_saveSuccessToast != null)
            {
                _saveSuccessToast.SetActive(false);
            }
        }

        /// <summary>
        /// 显示回调
        /// </summary>
        protected override void OnShow()
        {
            Debug.Log("[PauseMenuUI] 暂停菜单已显示");

            // 播放暂停音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("PauseMenuOpen");
            }
        }

        /// <summary>
        /// 隐藏回调
        /// </summary>
        protected override void OnHide()
        {
            // 关闭确认对话框
            CloseConfirmDialog();

            Debug.Log("[PauseMenuUI] 暂停菜单已隐藏");
        }

        #endregion

        #region 按钮事件处理

        /// <summary>
        /// 继续游戏按钮点击
        /// </summary>
        public void OnContinueButtonClicked()
        {
            Debug.Log("[PauseMenuUI] 继续游戏");

            // 播放按钮音效
            PlayButtonSound();

            // 关闭暂停菜单
            if (_uiManager != null)
            {
                _uiManager.HidePanel(UIPanelType.PauseMenu);
            }
        }

        /// <summary>
        /// 保存游戏按钮点击
        /// </summary>
        public void OnSaveGameButtonClicked()
        {
            Debug.Log("[PauseMenuUI] 保存游戏");

            // 播放按钮音效
            PlayButtonSound();

            // 执行保存
            SaveGame();
        }

        /// <summary>
        /// 设置按钮点击
        /// </summary>
        public void OnSettingsButtonClicked()
        {
            Debug.Log("[PauseMenuUI] 打开设置");

            // 播放按钮音效
            PlayButtonSound();

            // 显示设置面板
            if (_uiManager != null)
            {
                _uiManager.ShowPanel(UIPanelType.Settings);
            }
        }

        /// <summary>
        /// 返回主菜单按钮点击
        /// </summary>
        public void OnMainMenuButtonClicked()
        {
            Debug.Log("[PauseMenuUI] 返回主菜单");

            // 播放按钮音效
            PlayButtonSound();

            // 显示确认对话框
            ShowConfirmDialog("确定要返回主菜单吗？未保存的进度将会丢失。", ConfirmAction.ReturnToMainMenu);
        }

        /// <summary>
        /// 退出游戏按钮点击
        /// </summary>
        public void OnQuitButtonClicked()
        {
            Debug.Log("[PauseMenuUI] 退出游戏");

            // 播放按钮音效
            PlayButtonSound();

            // 显示确认对话框
            ShowConfirmDialog("确定要退出游戏吗？未保存的进度将会丢失。", ConfirmAction.QuitGame);
        }

        /// <summary>
        /// 确认按钮点击
        /// </summary>
        public void OnConfirmButtonClicked()
        {
            Debug.Log("[PauseMenuUI] 确认操作");

            // 播放按钮音效
            PlayButtonSound();

            // 执行待确认的操作
            ExecutePendingAction();

            // 关闭确认对话框
            CloseConfirmDialog();
        }

        /// <summary>
        /// 取消按钮点击
        /// </summary>
        public void OnCancelButtonClicked()
        {
            Debug.Log("[PauseMenuUI] 取消操作");

            // 播放按钮音效
            PlayButtonSound();

            // 关闭确认对话框
            CloseConfirmDialog();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 添加按钮监听
        /// </summary>
        private void AddButtonListeners()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinueButtonClicked);
            }

            if (_saveGameButton != null)
            {
                _saveGameButton.onClick.AddListener(OnSaveGameButtonClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitButtonClicked);
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }
        }

        /// <summary>
        /// 移除按钮监听
        /// </summary>
        private void RemoveButtonListeners()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(OnContinueButtonClicked);
            }

            if (_saveGameButton != null)
            {
                _saveGameButton.onClick.RemoveListener(OnSaveGameButtonClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
            }
        }

        /// <summary>
        /// 执行保存游戏
        /// </summary>
        private void SaveGame()
        {
            if (SaveManager.Instance == null)
            {
                ShowSaveFailedToast("存档系统不可用");
                return;
            }

            // 保存到默认槽位
            bool success = SaveManager.Instance.SaveGame(0);

            if (success)
            {
                ShowSaveSuccessToast("游戏已保存");
                Debug.Log("[PauseMenuUI] 保存成功");
            }
            else
            {
                ShowSaveFailedToast("保存失败");
                Debug.LogWarning("[PauseMenuUI] 保存失败");
            }
        }

        /// <summary>
        /// 显示保存成功提示
        /// </summary>
        private void ShowSaveSuccessToast(string message)
        {
            if (_saveSuccessToast == null)
            {
                return;
            }

            if (_saveSuccessText != null)
            {
                _saveSuccessText.text = message;
                _saveSuccessText.color = Color.green;
            }

            _saveSuccessToast.SetActive(true);

            if (_toastCoroutine != null)
            {
                StopCoroutine(_toastCoroutine);
            }

            _toastCoroutine = StartCoroutine(HideToastRoutine(2f));
        }

        /// <summary>
        /// 显示保存失败提示
        /// </summary>
        private void ShowSaveFailedToast(string message)
        {
            if (_saveSuccessToast == null)
            {
                return;
            }

            if (_saveSuccessText != null)
            {
                _saveSuccessText.text = message;
                _saveSuccessText.color = Color.red;
            }

            _saveSuccessToast.SetActive(true);

            if (_toastCoroutine != null)
            {
                StopCoroutine(_toastCoroutine);
            }

            _toastCoroutine = StartCoroutine(HideToastRoutine(2f));
        }

        /// <summary>
        /// 隐藏提示协程
        /// </summary>
        private IEnumerator HideToastRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (_saveSuccessToast != null)
            {
                _saveSuccessToast.SetActive(false);
            }
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        private void ShowConfirmDialog(string message, ConfirmAction action)
        {
            if (_confirmDialog == null)
            {
                // 没有确认对话框，直接执行操作
                _pendingAction = action;
                ExecutePendingAction();
                return;
            }

            _pendingAction = action;

            if (_confirmMessageText != null)
            {
                _confirmMessageText.text = message;
            }

            _isConfirmDialogOpen = true;
            _confirmDialog.SetActive(true);
        }

        /// <summary>
        /// 关闭确认对话框
        /// </summary>
        private void CloseConfirmDialog()
        {
            if (_confirmDialog != null)
            {
                _isConfirmDialogOpen = false;
                _confirmDialog.SetActive(false);
            }
            _pendingAction = ConfirmAction.None;
        }

        /// <summary>
        /// 执行待确认的操作
        /// </summary>
        private void ExecutePendingAction()
        {
            switch (_pendingAction)
            {
                case ConfirmAction.ReturnToMainMenu:
                    ReturnToMainMenu();
                    break;

                case ConfirmAction.QuitGame:
                    QuitGame();
                    break;
            }
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        private void ReturnToMainMenu()
        {
            Debug.Log("[PauseMenuUI] 返回主菜单...");

            // 关闭所有面板
            if (_uiManager != null)
            {
                _uiManager.HideAllPanels(false);
            }

            // 恢复时间
            Time.timeScale = 1f;

            // 切换到主菜单场景
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(CoreConfig.MAIN_MENU_SCENE);
            }
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        private void QuitGame()
        {
            Debug.Log("[PauseMenuUI] 退出游戏...");

            // 恢复时间
            Time.timeScale = 1f;

            // 退出应用程序
            Application.Quit();
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

        #endregion
    }
}
