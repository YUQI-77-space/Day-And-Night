using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DayAndNight.Core
{
    /// <summary>
    /// 场景加载器，负责管理场景的异步加载、卸载和过渡
    /// 继承自BaseManager以获得统一的初始化流程
    /// </summary>
    public class SceneLoader : BaseManager<SceneLoader>
    {
        #region 私有字段

        /// <summary>
        /// 当前加载的异步操作
        /// </summary>
        private AsyncOperation _currentAsyncOperation;

        /// <summary>
        /// 当前加载的场景名称
        /// </summary>
        private string _currentLoadingScene;

        /// <summary>
        /// 加载进度（0-1）
        /// </summary>
        private float _loadProgress;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        private bool _isLoading;

        /// <summary>
        /// 是否正在卸载
        /// </summary>
        private bool _isUnloading;

        /// <summary>
        /// 加载UI面板
        /// </summary>
        [SerializeField] private GameObject _loadingPanel;

        /// <summary>
        /// 进度条
        /// </summary>
        [SerializeField] private UnityEngine.UI.Slider _progressBar;

        /// <summary>
        /// 进度文本
        /// </summary>
        [SerializeField] private UnityEngine.UI.Text _progressText;

        /// <summary>
        /// 加载超时时间
        /// </summary>
        [SerializeField] private float _loadTimeout = CoreConfig.SCENE_LOAD_TIMEOUT;

        /// <summary>
        /// 加载开始时间
        /// </summary>
        private float _loadStartTime;

        /// <summary>
        /// 场景数据参数（传递给目标场景）
        /// </summary>
        private Dictionary<string, object> _sceneData = new Dictionary<string, object>();

        #endregion

        #region 属性

        /// <summary>
        /// 获取加载进度（0-1）
        /// </summary>
        public float LoadProgress => _loadProgress;

        /// <summary>
        /// 获取是否正在加载
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// 获取是否正在卸载
        /// </summary>
        public bool IsUnloading => _isUnloading;

        /// <summary>
        /// 获取当前正在加载的场景名称
        /// </summary>
        public string CurrentLoadingScene => _currentLoadingScene;

        /// <summary>
        /// 获取当前已加载的场景名称
        /// </summary>
        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        /// <summary>
        /// 获取场景总数
        /// </summary>
        public int SceneCount => SceneManager.sceneCountInBuildSettings;

        /// <summary>
        /// 是否允许场景激活
        /// </summary>
        public bool AllowSceneActivation { get; set; } = true;

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 初始化场景加载器
        /// </summary>
        protected override void OnInitialize()
        {
            // 注册场景加载事件
            SceneManager.sceneLoaded += _OnSceneLoaded;
            SceneManager.sceneUnloaded += _OnSceneUnloaded;

            // 确保 DontDestroyOnLoad
            if (this.transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            // 隐藏加载面板
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }

            _isLoading = false;
            _isUnloading = false;

            Debug.Log($"[SceneLoader] 场景加载器初始化完成，当前场景: {CurrentSceneName}");
        }

        /// <summary>
        /// 销毁时清理
        /// </summary>
        protected override void OnDestroy()
        {
            // 注销事件
            SceneManager.sceneLoaded -= _OnSceneLoaded;
            SceneManager.sceneUnloaded -= _OnSceneUnloaded;

            base.OnDestroy();
        }

        /// <summary>
        /// 每帧检查超时
        /// </summary>
        protected override void OnUpdate()
        {
            // 检查加载超时
            if (_isLoading && _currentAsyncOperation != null)
            {
                if (Time.time - _loadStartTime > _loadTimeout)
                {
                    Debug.LogError($"[SceneLoader] 场景加载超时: {_currentLoadingScene}");
                    _OnLoadComplete(false, "加载超时");
                }
            }
        }

        #endregion

        #region 公共方法 - 场景加载

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="showLoadingUI">是否显示加载UI</param>
        /// <param name="sceneData">传递给场景的数据</param>
        public void LoadScene(string sceneName, bool showLoadingUI = true, Dictionary<string, object> sceneData = null)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] 场景正在加载中，无法重复加载: {_currentLoadingScene}");
                return;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneLoader] 场景名称不能为空");
                return;
            }

            // 检查场景是否在BuildSettings中
            if (!IsSceneInBuildSettings(sceneName))
            {
                Debug.LogError($"[SceneLoader] 场景未在BuildSettings中注册: {sceneName}");
                return;
            }

            _currentLoadingScene = sceneName;
            _sceneData = sceneData ?? new Dictionary<string, object>();
            _loadStartTime = Time.time;

            StartCoroutine(LoadSceneAsyncRoutine(sceneName, showLoadingUI));
        }

        /// <summary>
        /// 加载场景（使用场景索引）
        /// </summary>
        /// <param name="sceneBuildIndex">场景索引</param>
        /// <param name="showLoadingUI">是否显示加载UI</param>
        public void LoadScene(int sceneBuildIndex, bool showLoadingUI = true)
        {
            if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"[SceneLoader] 场景索引超出范围: {sceneBuildIndex}");
                return;
            }

            string sceneName = SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name;
            LoadScene(sceneName, showLoadingUI);
        }

        /// <summary>
        /// 重新加载当前场景
        /// </summary>
        /// <param name="showLoadingUI">是否显示加载UI</param>
        public void ReloadCurrentScene(bool showLoadingUI = true)
        {
            string currentScene = CurrentSceneName;
            if (!string.IsNullOrEmpty(currentScene))
            {
                LoadScene(currentScene, showLoadingUI);
            }
        }

        /// <summary>
        /// 加载下一个场景（按Build Settings顺序）
        /// </summary>
        /// <param name="showLoadingUI">是否显示加载UI</param>
        public void LoadNextScene(bool showLoadingUI = true)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            int currentBuildIndex = currentScene.buildIndex;

            if (currentBuildIndex < SceneManager.sceneCountInBuildSettings - 1)
            {
                LoadScene(currentBuildIndex + 1, showLoadingUI);
            }
            else
            {
                Debug.LogWarning("[SceneLoader] 没有更多的场景可以加载");
            }
        }

        /// <summary>
        /// 加载主菜单场景
        /// </summary>
        /// <param name="showLoadingUI">是否显示加载UI</param>
        public void LoadMainMenu(bool showLoadingUI = true)
        {
            LoadScene(CoreConfig.MAIN_MENU_SCENE, showLoadingUI);
        }

        /// <summary>
        /// 加载游戏场景
        /// </summary>
        /// <param name="showLoadingUI">是否显示加载UI</param>
        public void LoadGameScene(bool showLoadingUI = true)
        {
            LoadScene(CoreConfig.GAMEPLAY_SCENE, showLoadingUI);
        }

        #endregion

        #region 公共方法 - 场景卸载

        /// <summary>
        /// 卸载指定场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void UnloadScene(string sceneName)
        {
            if (_isUnloading)
            {
                Debug.LogWarning("[SceneLoader] 场景正在卸载中...");
                return;
            }

            StartCoroutine(UnloadSceneAsyncRoutine(sceneName));
        }

        /// <summary>
        /// 卸载当前场景并加载新场景
        /// </summary>
        /// <param name="newSceneName">新场景名称</param>
        /// <param name="showLoadingUI">是否显示加载UI</param>
        public void UnloadAndLoadScene(string newSceneName, bool showLoadingUI = true)
        {
            if (_isLoading || _isUnloading)
            {
                Debug.LogWarning("[SceneLoader] 场景正在加载或卸载中...");
                return;
            }

            _currentLoadingScene = newSceneName;
            StartCoroutine(UnloadAndLoadSceneRoutine(newSceneName, showLoadingUI));
        }

        #endregion

        #region 公共方法 - 场景信息

        /// <summary>
        /// 检查场景是否在BuildSettings中
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>是否在BuildSettings中</returns>
        public bool IsSceneInBuildSettings(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (scenePath.Contains(sceneName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据场景名称获取Build索引
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>Build索引，不存在返回-1</returns>
        public int GetSceneBuildIndex(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (scenePath.Contains(sceneName))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取所有已加载的场景
        /// </summary>
        /// <returns>已加载场景列表</returns>
        public List<string> GetLoadedScenes()
        {
            List<string> loadedScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }
            return loadedScenes;
        }

        /// <summary>
        /// 检查场景是否已加载
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>是否已加载</returns>
        public bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }

        #endregion

        #region 公共方法 - 数据传递

        /// <summary>
        /// 设置场景数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetSceneData(string key, object value)
        {
            _sceneData[key] = value;
        }

        /// <summary>
        /// 获取场景数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>数据值</returns>
        public T GetSceneData<T>(string key, T defaultValue = default)
        {
            if (_sceneData.TryGetValue(key, out object value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 清除场景数据
        /// </summary>
        public void ClearSceneData()
        {
            _sceneData.Clear();
        }

        #endregion

        #region 公共方法 - UI控制

        /// <summary>
        /// 显示加载面板
        /// </summary>
        public void ShowLoadingPanel()
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 隐藏加载面板
        /// </summary>
        public void HideLoadingPanel()
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 设置进度条值
        /// </summary>
        /// <param name="progress">进度值（0-1）</param>
        public void SetProgress(float progress)
        {
            _loadProgress = Mathf.Clamp01(progress);
            _UpdateLoadingUI(_loadProgress);
        }

        /// <summary>
        /// 获取当前加载进度
        /// </summary>
        /// <returns>加载进度（0-1）</returns>
        public float GetProgress()
        {
            return _loadProgress;
        }

        /// <summary>
        /// 异步加载场景（协程封装，供外部使用）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="showLoadingUI">是否显示加载UI</param>
        /// <returns>协程枚举器</returns>
        public IEnumerator LoadSceneAsync(string sceneName, bool showLoadingUI = true)
        {
            yield return LoadSceneAsyncRoutine(sceneName, showLoadingUI);
        }

        #endregion

        #region 私有方法 - 协程

        /// <summary>
        /// 异步加载场景协程
        /// </summary>
        private IEnumerator LoadSceneAsyncRoutine(string sceneName, bool showLoadingUI)
        {
            _isLoading = true;
            _loadProgress = 0f;

            // 触发加载开始事件
            EventManager.Trigger(CoreEvents.SCENE_LOADING, new SceneEventArgs(sceneName, 0f));

            // 显示加载UI
            if (showLoadingUI)
            {
                ShowLoadingPanel();
            }

            // 开始异步加载
            _currentAsyncOperation = SceneManager.LoadSceneAsync(sceneName);
            _currentAsyncOperation.allowSceneActivation = AllowSceneActivation;

            // 等待加载完成
            while (!_currentAsyncOperation.isDone)
            {
                // 进度从0到0.9
                float progress = Mathf.Clamp01(_currentAsyncOperation.progress / 0.9f);
                _loadProgress = progress;

                // 更新UI
                _UpdateLoadingUI(progress);

                // 触发加载进度事件
                EventManager.Trigger(CoreEvents.SCENE_LOADING, new SceneEventArgs(sceneName, progress));

                yield return null;
            }

            // 加载完成
            _OnLoadComplete(true);

            // 触发场景加载完成事件
            EventManager.Trigger(CoreEvents.SCENE_LOADED, new SceneEventArgs(sceneName, 1f));

            // 隐藏加载UI
            if (showLoadingUI)
            {
                HideLoadingPanel();
            }

            Debug.Log($"[SceneLoader] 场景加载完成: {sceneName}");
        }

        /// <summary>
        /// 异步卸载场景协程
        /// </summary>
        private IEnumerator UnloadSceneAsyncRoutine(string sceneName)
        {
            _isUnloading = true;

            Debug.Log($"[SceneLoader] 开始卸载场景: {sceneName}");

            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);

            if (asyncOperation != null)
            {
                while (!asyncOperation.isDone)
                {
                    yield return null;
                }
            }

            _isUnloading = false;

            // 触发场景卸载完成事件
            EventManager.Trigger(CoreEvents.SCENE_UNLOADED, new SceneEventArgs(sceneName, 1f));

            Debug.Log($"[SceneLoader] 场景卸载完成: {sceneName}");
        }

        /// <summary>
        /// 卸载并加载新场景协程
        /// </summary>
        private IEnumerator UnloadAndLoadSceneRoutine(string newSceneName, bool showLoadingUI)
        {
            string currentScene = CurrentSceneName;

            // 先卸载当前场景
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(currentScene);

            if (unloadOperation != null)
            {
                while (!unloadOperation.isDone)
                {
                    yield return null;
                }
            }

            // 触发卸载完成事件
            EventManager.Trigger(CoreEvents.SCENE_UNLOADED, new SceneEventArgs(currentScene, 1f));

            // 加载新场景
            LoadScene(newSceneName, showLoadingUI);
        }

        #endregion

        #region 私有方法 - 事件处理

        /// <summary>
        /// 场景加载完成回调
        /// </summary>
        private void _OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _isLoading = false;
            _currentLoadingScene = null;
            _loadProgress = 1f;

            Debug.Log($"[SceneLoader] 场景已激活: {scene.name}");
        }

        /// <summary>
        /// 场景卸载完成回调
        /// </summary>
        private void _OnSceneUnloaded(Scene scene)
        {
            _isUnloading = false;
            Debug.Log($"[SceneLoader] 场景已卸载: {scene.name}");
        }

        /// <summary>
        /// 加载完成处理
        /// </summary>
        private void _OnLoadComplete(bool success, string errorMessage = null)
        {
            _isLoading = false;

            if (!success)
            {
                if (_loadingPanel != null)
                {
                    _loadingPanel.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 更新加载UI
        /// </summary>
        private void _UpdateLoadingUI(float progress)
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

        #region 公共方法 - 保存数据（ISaveable接口实现）

        /// <summary>
        /// 获取存档数据
        /// </summary>
        /// <returns>存档数据对象</returns>
        public object GetSaveData()
        {
            return new SceneLoaderSaveData
            {
                currentSceneName = CurrentSceneName,
                loadProgress = _loadProgress,
                isLoading = _isLoading,
                isUnloading = _isUnloading
            };
        }

        /// <summary>
        /// 加载存档数据
        /// </summary>
        /// <param name="data">存档数据对象</param>
        public void LoadSaveData(object data)
        {
            if (data is SceneLoaderSaveData saveData)
            {
                Debug.Log($"[SceneLoader] 存档场景: {saveData.currentSceneName}");
                // 通常不恢复加载状态，而是让玩家选择加载哪个存档
            }
        }

        /// <summary>
        /// 重置存档数据
        /// </summary>
        public void ResetSaveData()
        {
            ClearSceneData();
            _loadProgress = 0f;
            Debug.Log("[SceneLoader] 场景数据已重置");
        }

        #endregion
    }

    #region 存档数据类

    /// <summary>
    /// 场景加载器存档数据
    /// </summary>
    [Serializable]
    public class SceneLoaderSaveData
    {
        /// <summary>
        /// 当前场景名称
        /// </summary>
        public string currentSceneName;

        /// <summary>
        /// 加载进度
        /// </summary>
        public float loadProgress;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool isLoading;

        /// <summary>
        /// 是否正在卸载
        /// </summary>
        public bool isUnloading;
    }

    #endregion
}
