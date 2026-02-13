using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 数据管理器，负责加载和管理游戏配置数据
    /// 提供统一的配置访问接口，支持ScriptableObject和资源配置
    /// </summary>
    public class DataManager : BaseManager<DataManager>
    {
        #region 私有字段

        /// <summary>
        /// 游戏配置ScriptableObject
        /// </summary>
        [SerializeField] private GameConfig _gameConfig;

        /// <summary>
        /// 配置缓存字典
        /// </summary>
        private Dictionary<Type, ScriptableObject> _configCache = new Dictionary<Type, ScriptableObject>();

        /// <summary>
        /// 资源缓存字典
        /// </summary>
        private Dictionary<string, UnityEngine.Object> _resourceCache = new Dictionary<string, UnityEngine.Object>();

        /// <summary>
        /// 配置资源路径
        /// </summary>
        private const string CONFIG_RESOURCE_PATH = "Data/GameConfig";

        /// <summary>
        /// 是否已加载配置
        /// </summary>
        private bool _isConfigLoaded = false;

        #endregion

        #region 属性

        /// <summary>
        /// 获取游戏配置
        /// </summary>
        public GameConfig GameConfig => _gameConfig;

        /// <summary>
        /// 获取游戏版本号
        /// </summary>
        public string GameVersion => _gameConfig != null ? _gameConfig.GameVersion : "Unknown";

        /// <summary>
        /// 获取游戏名称
        /// </summary>
        public string GameName => _gameConfig != null ? _gameConfig.GameName : "DayAndNight";

        /// <summary>
        /// 获取默认语言
        /// </summary>
        public string DefaultLanguage => _gameConfig != null ? _gameConfig.DefaultLanguage : "zh-CN";

        /// <summary>
        /// 获取默认难度
        /// </summary>
        public int DefaultDifficulty => _gameConfig != null ? _gameConfig.DefaultDifficulty : 1;

        /// <summary>
        /// 获取是否显示调试信息
        /// </summary>
        public bool ShowDebugInfo => _gameConfig != null && _gameConfig.ShowDebugInfo;

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 初始化数据管理器
        /// </summary>
        protected override void OnInitialize()
        {
            _LoadGameConfig();
            _CacheAllConfigs();
            Debug.Log($"[DataManager] 数据管理器初始化完成，配置版本: {GameVersion}");
        }

        /// <summary>
        /// 应用帧率设置
        /// </summary>
        protected override void OnUpdate()
        {
            if (_gameConfig != null && _gameConfig.TargetFrameRate > 0)
            {
                Application.targetFrameRate = _gameConfig.TargetFrameRate;
            }
        }

        #endregion

        #region 公共方法 - 配置加载

        /// <summary>
        /// 加载游戏配置
        /// </summary>
        private void _LoadGameConfig()
        {
            try
            {
                _gameConfig = Resources.Load<GameConfig>(CONFIG_RESOURCE_PATH);

                if (_gameConfig == null)
                {
                    Debug.LogWarning("[DataManager] 未找到GameConfig资源，将使用默认值");
                    _gameConfig = ScriptableObject.CreateInstance<GameConfig>();
                }
                else
                {
                    Debug.Log($"[DataManager] 已加载GameConfig: {GameName} v{GameVersion}");
                }

                _isConfigLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataManager] 加载GameConfig失败: {ex.Message}");
                _gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            }
        }

        /// <summary>
        /// 重新加载所有配置
        /// </summary>
        public void ReloadAllData()
        {
            Debug.Log("[DataManager] 正在重新加载所有配置...");

            // 清除缓存
            _configCache.Clear();
            _resourceCache.Clear();

            // 重新加载配置
            _LoadGameConfig();
            _CacheAllConfigs();

            // 触发配置重载事件
            EventManager.Trigger(CoreEvents.SYSTEM_INITIALIZED);

            Debug.Log("[DataManager] 配置重载完成");
        }

        /// <summary>
        /// 缓存所有配置
        /// </summary>
        private void _CacheAllConfigs()
        {
            if (_gameConfig != null)
            {
                _configCache[typeof(GameConfig)] = _gameConfig;
            }
        }

        #endregion

        #region 公共方法 - 配置访问

        /// <summary>
        /// 获取指定类型的配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <returns>配置实例，不存在返回null</returns>
        public T GetConfig<T>() where T : ScriptableObject
        {
            Type configType = typeof(T);

            if (_configCache.TryGetValue(configType, out ScriptableObject config))
            {
                return config as T;
            }

            // 尝试从Resources加载
            string resourcePath = $"Data/{configType.Name}";
            T loadedConfig = Resources.Load<T>(resourcePath);

            if (loadedConfig != null)
            {
                _configCache[configType] = loadedConfig;
                Debug.Log($"[DataManager] 已加载配置: {configType.Name}");
                return loadedConfig;
            }

            Debug.LogWarning($"[DataManager] 未找到配置: {configType.Name}");
            return null;
        }

        /// <summary>
        /// 获取或创建指定类型的配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <returns>配置实例，不存在则创建</returns>
        public T GetOrCreateConfig<T>() where T : ScriptableObject, new()
        {
            T config = GetConfig<T>();

            if (config == null)
            {
                config = new T();
                _configCache[typeof(T)] = config;
                Debug.Log($"[DataManager] 已创建配置: {typeof(T).Name}");
            }

            return config;
        }

        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <returns>是否存在</returns>
        public bool HasConfig<T>() where T : ScriptableObject
        {
            return _configCache.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 添加配置到缓存
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="config">配置实例</param>
        public void AddConfig<T>(T config) where T : ScriptableObject
        {
            if (config != null)
            {
                _configCache[typeof(T)] = config;
            }
        }

        #endregion

        #region 公共方法 - 资源加载

        /// <summary>
        /// 加载指定路径的资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <returns>资源实例</returns>
        public T LoadResource<T>(string path) where T : UnityEngine.Object
        {
            // 先检查缓存
            if (_resourceCache.TryGetValue(path, out UnityEngine.Object cached))
            {
                return cached as T;
            }

            // 从Resources加载
            T resource = Resources.Load<T>(path);

            if (resource != null)
            {
                _resourceCache[path] = resource;
                Debug.Log($"[DataManager] 已加载资源: {path}");
            }
            else
            {
                Debug.LogWarning($"[DataManager] 未找到资源: {path}");
            }

            return resource;
        }

        /// <summary>
        /// 加载指定路径的所有资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源目录路径</param>
        /// <returns>资源数组</returns>
        public T[] LoadAllResources<T>(string path) where T : UnityEngine.Object
        {
            T[] resources = Resources.LoadAll<T>(path);

            foreach (var resource in resources)
            {
                string resourcePath = $"{path}/{resource.name}";
                if (!_resourceCache.ContainsKey(resourcePath))
                {
                    _resourceCache[resourcePath] = resource;
                }
            }

            Debug.Log($"[DataManager] 已加载 {resources.Length} 个{typeof(T).Name}资源");
            return resources;
        }

        /// <summary>
        /// 检查资源是否存在
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>是否存在</returns>
        public bool HasResource(string path)
        {
            return _resourceCache.ContainsKey(path) || Resources.Load(path) != null;
        }

        #endregion

        #region 公共方法 - 资源清理

        /// <summary>
        /// 清理未使用的资源
        /// </summary>
        public void UnloadUnusedAssets()
        {
            Debug.Log("[DataManager] 正在清理未使用的资源...");
            Resources.UnloadUnusedAssets();
            Debug.Log("[DataManager] 资源清理完成");
        }

        /// <summary>
        /// 清除资源缓存
        /// </summary>
        /// <param name="unloadUnused">是否同时卸载未使用的资源</param>
        public void ClearResourceCache(bool unloadUnused = false)
        {
            _resourceCache.Clear();

            if (unloadUnused)
            {
                UnloadUnusedAssets();
            }

            Debug.Log("[DataManager] 资源缓存已清除");
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void ClearAllCache()
        {
            _configCache.Clear();
            _resourceCache.Clear();
            Debug.Log("[DataManager] 所有缓存已清除");
        }

        #endregion

        #region 公共方法 - 获取信息

        /// <summary>
        /// 获取当前缓存的配置数量
        /// </summary>
        /// <returns>配置数量</returns>
        public int GetConfigCount()
        {
            return _configCache.Count;
        }

        /// <summary>
        /// 获取当前缓存的资源数量
        /// </summary>
        /// <returns>资源数量</returns>
        public int GetResourceCount()
        {
            return _resourceCache.Count;
        }

        /// <summary>
        /// 获取所有已缓存的配置类型
        /// </summary>
        /// <returns>配置类型列表</returns>
        public List<Type> GetCachedConfigTypes()
        {
            return new List<Type>(_configCache.Keys);
        }

        #endregion

        #region 公共方法 - 保存数据（ISaveable接口实现）

        /// <summary>
        /// 获取存档数据
        /// </summary>
        /// <returns>存档数据对象</returns>
        public object GetSaveData()
        {
            return new DataManagerSaveData
            {
                cachedConfigCount = _configCache.Count,
                cachedResourceCount = _resourceCache.Count,
                lastLoadedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        /// <summary>
        /// 加载存档数据
        /// </summary>
        /// <param name="data">存档数据对象</param>
        public void LoadSaveData(object data)
        {
            if (data is DataManagerSaveData saveData)
            {
                Debug.Log($"[DataManager] 存档数据: 配置缓存 {saveData.cachedConfigCount} 个");
                // DataManager通常不需要恢复缓存，因为配置是静态的
            }
        }

        /// <summary>
        /// 重置存档数据
        /// </summary>
        public void ResetSaveData()
        {
            ClearAllCache();
            _LoadGameConfig();
            Debug.Log("[DataManager] 数据已重置");
        }

        #endregion
    }

    #region 存档数据类

    /// <summary>
    /// 数据管理器存档数据
    /// </summary>
    [Serializable]
    public class DataManagerSaveData
    {
        /// <summary>
        /// 缓存的配置数量
        /// </summary>
        public int cachedConfigCount;

        /// <summary>
        /// 缓存的资源数量
        /// </summary>
        public int cachedResourceCount;

        /// <summary>
        /// 最后加载时间
        /// </summary>
        public string lastLoadedTime;
    }

    #endregion
}
