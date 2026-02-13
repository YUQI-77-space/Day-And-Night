using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DayAndNight.Core
{
    /// <summary>
    /// 加载界面
    /// 提供场景加载进度显示、提示信息等功能
    /// </summary>
    public class LoadingUI : UIBasePanel
    {
        #region 序列化字段

        [Header("进度条组件")]
        [SerializeField]
        private Image _progressBarImage;

        [SerializeField]
        private Text _progressText;

        [SerializeField]
        private Text _loadingTipText;

        [Header("进度信息")]
        [SerializeField]
        private Text _sceneNameText;

        [SerializeField]
        private Text _progressPercentText;

        [Header("加载提示")]
        [SerializeField]
        private string[] _loadingTips;

        #endregion

        #region 私有字段

        private Coroutine _loadingCoroutine;
        private float _targetProgress = 0f;
        private string _loadingSceneName;
        private bool _isLoading = false;

        #endregion

        #region Unity生命周期方法

        private void Awake()
        {
            // 如果没有设置面板类型，使用默认值
            if (_panelType == UIPanelType.None)
            {
                _panelType = UIPanelType.Loading;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 初始化回调
        /// </summary>
        protected override void OnInitialize()
        {
            // 初始化进度条
            if (_progressBarImage != null)
            {
                _progressBarImage.fillAmount = 0f;
            }

            // 随机显示加载提示
            ShowRandomTip();

            // 初始时隐藏进度信息
            if (_progressPercentText != null)
            {
                _progressPercentText.text = "0%";
            }
        }

        /// <summary>
        /// 显示回调
        /// </summary>
        protected override void OnShow()
        {
            Debug.Log("[LoadingUI] 加载界面已显示");
        }

        /// <summary>
        /// 隐藏回调
        /// </summary>
        protected override void OnHide()
        {
            // 停止加载协程
            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }

            _isLoading = false;

            Debug.Log("[LoadingUI] 加载界面已隐藏");
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 开始加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="fadeDuration">淡入淡出时间</param>
        public void StartLoading(string sceneName, float fadeDuration = 1f)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[LoadingUI] 已经在加载中");
                return;
            }

            _loadingSceneName = sceneName;
            _targetProgress = 0f;

            // 更新场景名称
            if (_sceneNameText != null)
            {
                _sceneNameText.text = $"正在加载: {sceneName}";
            }

            // 重置进度条
            if (_progressBarImage != null)
            {
                _progressBarImage.fillAmount = 0f;
            }

            if (_progressPercentText != null)
            {
                _progressPercentText.text = "0%";
            }

            // 随机显示加载提示
            ShowRandomTip();

            // 开始加载协程
            _isLoading = true;
            _loadingCoroutine = StartCoroutine(LoadingRoutine(sceneName, fadeDuration));
        }

        /// <summary>
        /// 直接加载场景（同步）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void LoadSceneDirect(string sceneName)
        {
            Debug.Log($"[LoadingUI] 直接加载场景: {sceneName}");

            if (_uiManager != null)
            {
                _uiManager.ShowPanel(UIPanelType.Loading);
            }

            StartLoading(sceneName);
        }

        /// <summary>
        /// 设置加载进度
        /// </summary>
        /// <param name="progress">进度值 (0-1)</param>
        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
            UpdateProgressUI();
        }

        /// <summary>
        /// 增加加载进度
        /// </summary>
        /// <param name="delta">增量</param>
        public void AddProgress(float delta)
        {
            _targetProgress = Mathf.Clamp01(_targetProgress + delta);
            UpdateProgressUI();
        }

        /// <summary>
        /// 完成加载
        /// </summary>
        public void CompleteLoading()
        {
            _targetProgress = 1f;
            UpdateProgressUI();

            // 短暂延迟后隐藏加载界面
            StartCoroutine(CompleteLoadingRoutine());
        }

        /// <summary>
        /// 设置加载提示
        /// </summary>
        /// <param name="tip">提示文本</param>
        public void SetLoadingTip(string tip)
        {
            if (_loadingTipText != null)
            {
                _loadingTipText.text = tip;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 加载协程
        /// </summary>
        private IEnumerator LoadingRoutine(string sceneName, float fadeDuration)
        {
            Debug.Log($"[LoadingUI] 开始加载场景: {sceneName}");

            // 更新提示
            SetLoadingTip("正在准备资源...");

            // 模拟一些加载时间
            yield return new WaitForSeconds(0.5f);

            // 如果有场景加载器，使用它
            if (SceneLoader.Instance != null)
            {
                yield return StartCoroutine(SceneLoader.Instance.LoadSceneAsync(sceneName, true));
            }
            else
            {
                // 使用Unity场景管理器直接加载
                AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

                while (!asyncOperation.isDone)
                {
                    _targetProgress = asyncOperation.progress;
                    UpdateProgressUI();
                    yield return null;
                }

                _targetProgress = 1f;
                UpdateProgressUI();
            }

            // 加载完成
            CompleteLoading();
        }

        /// <summary>
        /// 完成加载协程
        /// </summary>
        private IEnumerator CompleteLoadingRoutine()
        {
            // 播放加载完成音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("LoadingComplete");
            }

            // 短暂延迟
            yield return new WaitForSeconds(0.5f);

            // 隐藏加载界面
            if (_uiManager != null)
            {
                _uiManager.HidePanel(UIPanelType.Loading);
            }

            _isLoading = false;
            _loadingCoroutine = null;

            Debug.Log("[LoadingUI] 加载完成");
        }

        /// <summary>
        /// 更新进度UI
        /// </summary>
        private void UpdateProgressUI()
        {
            // 更新进度条
            if (_progressBarImage != null)
            {
                _progressBarImage.fillAmount = _targetProgress;
            }

            // 更新百分比文本
            if (_progressPercentText != null)
            {
                _progressPercentText.text = $"{Mathf.RoundToInt(_targetProgress * 100)}%";
            }

            // 更新进度文本
            if (_progressText != null)
            {
                string[] statusTexts = { "正在加载资源...", "正在初始化...", "即将完成...", "加载完成" };
                int index = Mathf.RoundToInt(_targetProgress * (statusTexts.Length - 1));
                _progressText.text = statusTexts[Mathf.Clamp(index, 0, statusTexts.Length - 1)];
            }
        }

        /// <summary>
        /// 显示随机加载提示
        /// </summary>
        private void ShowRandomTip()
        {
            if (_loadingTipText == null || _loadingTips == null || _loadingTips.Length == 0)
            {
                return;
            }

            string randomTip = _loadingTips[Random.Range(0, _loadingTips.Length)];
            _loadingTipText.text = randomTip;
        }

        /// <summary>
        /// 显示加载提示
        /// </summary>
        private void ShowLoadingTips()
        {
            // 默认加载提示
            if (_loadingTips == null || _loadingTips.Length == 0)
            {
                _loadingTips = new string[]
                {
                    "探索这个神秘的世界...",
                    "寻找隐藏的宝藏...",
                    "了解古老的传说...",
                    "提升你的技能...",
                    "发现新的朋友...",
                    "面对强大的敌人...",
                    "解开谜题...",
                    "体验精彩的故事..."
                };
            }

            ShowRandomTip();
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 显示加载界面并加载场景
        /// </summary>
        public static void ShowAndLoad(string sceneName)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPanel(UIPanelType.Loading);
                LoadingUI loadingUI = UIManager.Instance.GetPanel<LoadingUI>(UIPanelType.Loading);
                if (loadingUI != null)
                {
                    loadingUI.LoadSceneDirect(sceneName);
                }
            }
        }

        #endregion
    }
}
