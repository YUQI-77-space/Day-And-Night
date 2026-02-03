using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景加载器 - 负责异步加载场景和显示加载进度
/// 放置位置：Assets/_Project/Scripts/Core/SceneLoader.cs
/// </summary>
public class SceneLoader : MonoBehaviour
{
    #region 单例模式
    public static SceneLoader Instance { get; private set; }
    #endregion

    #region 变量声明
    // 加载进度（0-1）
    private float _loadProgress;
    public float LoadProgress => _loadProgress;

    // 是否正在加载
    private bool _isLoading;
    public bool IsLoading => _isLoading;

    // 当前加载的场景名称
    private string _currentSceneName;
    public string CurrentSceneName => _currentSceneName;

    // 加载UI面板（可选）
    [Header("UI引用")]
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private UnityEngine.UI.Slider _progressBar;
    [SerializeField] private UnityEngine.UI.Text _progressText;
    #endregion

    #region 生命周期方法
    private void Awake()
    {
        // 单例模式初始化
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

        // 确保只有一个SceneLoader
        SceneLoader[] loaders = FindObjectsOfType<SceneLoader>();
        if (loaders.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 可选：初始化时隐藏加载面板
        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(false);
        }
    }
    #endregion

    #region 公开方法
    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneName">场景名称（不带.unity后缀）</param>
    /// <param name="showLoadingUI">是否显示加载UI</param>
    public void LoadScene(string sceneName, bool showLoadingUI = true)
    {
        if (_isLoading)
        {
            Debug.LogWarning($"场景正在加载中，无法重复加载：{_currentSceneName}");
            return;
        }

        StartCoroutine(LoadSceneAsyncRoutine(sceneName, showLoadingUI));
    }

    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadCurrentScene(bool showLoadingUI = true)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene, showLoadingUI);
    }

    /// <summary>
    /// 加载下一个场景（按Build Settings顺序）
    /// </summary>
    public void LoadNextScene(bool showLoadingUI = true)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        int currentBuildIndex = currentScene.buildIndex;
        
        if (currentBuildIndex < SceneManager.sceneCountInBuildSettings - 1)
        {
            string nextSceneName = SceneManager.GetSceneByBuildIndex(currentBuildIndex + 1).name;
            LoadScene(nextSceneName, showLoadingUI);
        }
        else
        {
            Debug.LogWarning("没有更多的场景可以加载");
        }
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 异步加载场景的协程
    /// </summary>
    private IEnumerator LoadSceneAsyncRoutine(string sceneName, bool showLoadingUI)
    {
        _isLoading = true;
        _currentSceneName = sceneName;
        _loadProgress = 0f;

        // 显示加载UI（如果需要）
        if (showLoadingUI && _loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
        }

        // 开始异步加载
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        
        // 设置场景加载完成后不立即激活（可选，用于更好的加载体验）
        asyncOperation.allowSceneActivation = true;

        // 等待加载完成
        while (!asyncOperation.isDone)
        {
            // 进度从0到0.9（allowSceneActivation = true时）
            // 如果设置为false，进度到0.9后会停止，等待场景激活
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            _loadProgress = progress;

            // 更新UI
            UpdateLoadingUI(progress);

            yield return null;
        }

        // 加载完成
        _isLoading = false;
        _loadProgress = 1f;
        _currentSceneName = sceneName;

        // 隐藏加载UI
        if (showLoadingUI && _loadingPanel != null)
        {
            _loadingPanel.SetActive(false);
        }

        Debug.Log($"场景加载完成：{sceneName}");
    }

    /// <summary>
    /// 更新加载UI
    /// </summary>
    private void UpdateLoadingUI(float progress)
    {
        if (_progressBar != null)
        {
            _progressBar.value = progress;
        }

        if (_progressText != null)
        {
            _progressText.text = $"加载中... {Mathf.RoundToInt(progress * 100)}%";
        }
    }
    #endregion

    #region 场景激活设置
    /// <summary>
    /// 设置是否允许场景激活（用于更平滑的加载过渡）
    /// </summary>
    public void SetAllowSceneActivation(bool allow)
    {
        // 这个需要在LoadSceneAsyncRoutine中配合使用
        // 需要将asyncOperation保存为成员变量
    }
    #endregion
}
