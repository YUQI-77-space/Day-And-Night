using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DayAndNight.Core;

namespace DayAndNight.Core
{
    /// <summary>
    /// 存档管理器，负责游戏的保存和读取功能
    /// 支持多个存档槽、自动保存、存档校验等功能
    /// </summary>
    public class SaveManager : BaseManager<SaveManager>
    {
        #region 常量

        /// <summary>
        /// 存档文件名前缀
        /// </summary>
        private const string SAVE_FILE_PREFIX = "save_";

        /// <summary>
        /// 存档元数据文件名
        /// </summary>
        private const string SAVE_METADATA_SUFFIX = "_meta.json";

        /// <summary>
        /// 自动保存检查间隔（秒）
        /// </summary>
        private const float AUTO_SAVE_CHECK_INTERVAL = 60f;

        #endregion

        #region 私有字段

        /// <summary>
        /// 存档目录路径
        /// </summary>
        private string _saveDirectory;

        /// <summary>
        /// 自动保存协程
        /// </summary>
        private Coroutine _autoSaveCoroutine;

        /// <summary>
        /// 最后保存时间
        /// </summary>
        private DateTime _lastSaveTime;

        /// <summary>
        /// 当前正在保存/加载的槽位
        /// </summary>
        private int _currentOperationSlot = -1;

        /// <summary>
        /// 是否正在保存中
        /// </summary>
        private bool _isSaving = false;

        /// <summary>
        /// 是否正在加载中
        /// </summary>
        private bool _isLoading = false;

        /// <summary>
        /// 自动保存是否已启用
        /// </summary>
        [SerializeField]
        private bool _autoSaveEnabled = true;

        /// <summary>
        /// 自动保存间隔（秒）
        /// </summary>
        [SerializeField]
        private float _autoSaveInterval = CoreConfig.AUTO_SAVE_INTERVAL;

        /// <summary>
        /// 最大自动保存文件数
        /// </summary>
        [SerializeField]
        private int _maxAutoSaveFiles = 3;

        /// <summary>
        /// 存档数据缓存
        /// </summary>
        private Dictionary<int, SaveMetadata> _cachedMetadata = new Dictionary<int, SaveMetadata>();

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取或设置是否启用自动保存
        /// </summary>
        public bool AutoSaveEnabled
        {
            get => _autoSaveEnabled;
            set
            {
                _autoSaveEnabled = value;
                UpdateAutoSaveState();
            }
        }

        /// <summary>
        /// 获取自动保存间隔（秒）
        /// </summary>
        public float AutoSaveInterval => _autoSaveInterval;

        /// <summary>
        /// 获取最后保存时间
        /// </summary>
        public DateTime LastSaveTime => _lastSaveTime;

        /// <summary>
        /// 获取是否正在保存中
        /// </summary>
        public bool IsSaving => _isSaving;

        /// <summary>
        /// 获取是否正在加载中
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// 获取当前操作槽位
        /// </summary>
        public int CurrentOperationSlot => _currentOperationSlot;

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 唤醒时初始化存档系统
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // 设置存档目录
            _saveDirectory = Path.Combine(Application.persistentDataPath, CoreConfig.SAVE_DIRECTORY_NAME);

            // 确保存档目录存在
            EnsureSaveDirectory();
        }

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 执行初始化逻辑
        /// </summary>
        protected override void OnInitialize()
        {
            Debug.Log("[SaveManager] 开始初始化...");

            // 加载存档元数据缓存
            LoadAllMetadata();

            // 启动自动保存
            UpdateAutoSaveState();

            Debug.Log($"[SaveManager] 初始化完成，共找到 {_cachedMetadata.Count} 个存档");
        }

        /// <summary>
        /// 执行关闭逻辑
        /// </summary>
        protected override void OnShutdown()
        {
            // 停止自动保存
            StopAutoSave();

            // 如果正在保存，等待保存完成
            while (_isSaving)
            {
                Debug.Log("[SaveManager] 等待保存完成...");
                System.Threading.Thread.Sleep(100);
            }

            Debug.Log("[SaveManager] 已关闭");
        }

        #endregion

        #region 公共方法 - 保存功能

        /// <summary>
        /// 保存游戏到指定槽位
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>保存是否成功</returns>
        public bool SaveGame(int slot)
        {
            if (_isSaving)
            {
                Debug.LogWarning("[SaveManager] 保存失败：当前正在保存中");
                return false;
            }

            if (slot < 0 || slot >= CoreConfig.MAX_SAVE_SLOTS)
            {
                Debug.LogWarning($"[SaveManager] 保存失败：无效的槽位编号 {slot}");
                return false;
            }

            _isSaving = true;
            _currentOperationSlot = slot;

            try
            {
                Debug.Log($"[SaveManager] 开始保存到槽位 {slot}...");

                // 收集所有可存档数据
                SaveData saveData = CollectSaveData();

                // 创建存档元数据
                SaveMetadata metadata = CreateSaveMetadata(slot);

                // 保存数据
                SaveDataToFile(slot, saveData);
                SaveMetadataToFile(metadata);

                // 更新缓存
                _cachedMetadata[slot] = metadata;

                // 更新最后保存时间
                _lastSaveTime = DateTime.Now;

                // 触发保存成功事件
                EventManager.Instance.TriggerEvent(CoreEvents.GAME_SAVED, new SaveEventArgs(slot, true));

                Debug.Log($"[SaveManager] 保存成功：槽位 {slot}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 保存失败：槽位 {slot}, 错误: {ex.Message}");

                // 触发保存失败事件
                EventManager.Instance.TriggerEvent(CoreEvents.SAVE_FAILED, new SaveEventArgs(slot, false, ex.Message));

                return false;
            }
            finally
            {
                _isSaving = false;
                _currentOperationSlot = -1;
            }
        }

        /// <summary>
        /// 异步保存游戏到指定槽位
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>协程</returns>
        public Coroutine SaveGameAsync(int slot)
        {
            return StartCoroutine(SaveGameAsyncRoutine(slot));
        }

        #endregion

        #region 公共方法 - 加载功能

        /// <summary>
        /// 从指定槽位加载游戏
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>加载是否成功</returns>
        public bool LoadGame(int slot)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[SaveManager] 加载失败：当前正在加载中");
                return false;
            }

            if (slot < 0 || slot >= CoreConfig.MAX_SAVE_SLOTS)
            {
                Debug.LogWarning($"[SaveManager] 加载失败：无效的槽位编号 {slot}");
                return false;
            }

            // 检查存档是否存在
            if (!HasSave(slot))
            {
                Debug.LogWarning($"[SaveManager] 加载失败：槽位 {slot} 没有存档");
                return false;
            }

            _isLoading = true;
            _currentOperationSlot = slot;

            try
            {
                Debug.Log($"[SaveManager] 开始从槽位 {slot} 加载...");

                // 加载数据
                SaveData saveData = LoadDataFromFile(slot);

                if (saveData == null)
                {
                    throw new Exception("无法读取存档数据");
                }

                // 校验存档数据
                if (!ValidateSaveData(saveData))
                {
                    throw new Exception("存档数据校验失败");
                }

                // 应用存档数据
                ApplySaveData(saveData);

                // 触发加载成功事件
                EventManager.Instance.TriggerEvent(CoreEvents.GAME_LOADED, new SaveEventArgs(slot, true));

                Debug.Log($"[SaveManager] 加载成功：槽位 {slot}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 加载失败：槽位 {slot}, 错误: {ex.Message}");

                // 触发加载失败事件
                EventManager.Instance.TriggerEvent(CoreEvents.LOAD_FAILED, new SaveEventArgs(slot, false, ex.Message));

                return false;
            }
            finally
            {
                _isLoading = false;
                _currentOperationSlot = -1;
            }
        }

        /// <summary>
        /// 异步从指定槽位加载游戏
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>协程</returns>
        public Coroutine LoadGameAsync(int slot)
        {
            return StartCoroutine(LoadGameAsyncRoutine(slot));
        }

        #endregion

        #region 公共方法 - 新游戏

        /// <summary>
        /// 开始新游戏
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>是否成功</returns>
        public bool NewGame(int slot = 0)
        {
            if (slot < 0 || slot >= CoreConfig.MAX_SAVE_SLOTS)
            {
                Debug.LogWarning($"[SaveManager] 新游戏失败：无效的槽位编号 {slot}");
                return false;
            }

            try
            {
                Debug.Log($"[SaveManager] 开始新游戏，槽位 {slot}...");

                // 重置所有可存档数据
                ResetAllSaveData();

                // 创建初始存档
                SaveGame(slot);

                // 触发新游戏事件
                EventManager.Instance.TriggerEvent(CoreEvents.NEW_GAME);

                Debug.Log($"[SaveManager] 新游戏创建成功：槽位 {slot}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 新游戏失败：槽位 {slot}, 错误: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 公共方法 - 存档管理

        /// <summary>
        /// 检查指定槽位是否有存档
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>是否有存档</returns>
        public bool HasSave(int slot)
        {
            string savePath = GetSaveFilePath(slot);
            return File.Exists(savePath);
        }

        /// <summary>
        /// 获取所有存档槽位信息
        /// </summary>
        /// <returns>存档槽位信息列表</returns>
        public List<SaveSlotInfo> GetAllSaveSlots()
        {
            List<SaveSlotInfo> slots = new List<SaveSlotInfo>();

            for (int i = 0; i < CoreConfig.MAX_SAVE_SLOTS; i++)
            {
                SaveSlotInfo info = new SaveSlotInfo
                {
                    SlotIndex = i,
                    HasSave = HasSave(i),
                    SaveTime = DateTime.MinValue,
                    PlayTime = TimeSpan.Zero,
                    SceneName = string.Empty
                };

                if (info.HasSave && _cachedMetadata.TryGetValue(i, out SaveMetadata metadata))
                {
                    info.SaveTime = metadata.SaveTime;
                    info.PlayTime = metadata.TotalPlayTime;
                    info.SceneName = metadata.SceneName;
                    info.PlayerLevel = metadata.PlayerLevel;
                }

                slots.Add(info);
            }

            return slots;
        }

        /// <summary>
        /// 获取指定槽位的存档信息
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>存档信息</returns>
        public SaveSlotInfo GetSaveInfo(int slot)
        {
            if (slot < 0 || slot >= CoreConfig.MAX_SAVE_SLOTS)
            {
                return new SaveSlotInfo { SlotIndex = slot, HasSave = false };
            }

            SaveSlotInfo info = new SaveSlotInfo
            {
                SlotIndex = slot,
                HasSave = HasSave(slot)
            };

            if (info.HasSave && _cachedMetadata.TryGetValue(slot, out SaveMetadata metadata))
            {
                info.SaveTime = metadata.SaveTime;
                info.PlayTime = metadata.TotalPlayTime;
                info.SceneName = metadata.SceneName;
                info.PlayerLevel = metadata.PlayerLevel;
            }

            return info;
        }

        /// <summary>
        /// 删除指定槽位的存档
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>是否成功</returns>
        public bool DeleteSave(int slot)
        {
            if (slot < 0 || slot >= CoreConfig.MAX_SAVE_SLOTS)
            {
                Debug.LogWarning($"[SaveManager] 删除存档失败：无效的槽位编号 {slot}");
                return false;
            }

            try
            {
                // 删除存档文件
                string savePath = GetSaveFilePath(slot);
                string metaPath = GetMetadataFilePath(slot);

                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }

                // 从缓存中移除
                if (_cachedMetadata.ContainsKey(slot))
                {
                    _cachedMetadata.Remove(slot);
                }

                Debug.Log($"[SaveManager] 已删除槽位 {slot} 的存档");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 删除存档失败：槽位 {slot}, 错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 复制存档到新槽位
        /// </summary>
        /// <param name="sourceSlot">源槽位</param>
        /// <param name="targetSlot">目标槽位</param>
        /// <returns>是否成功</returns>
        public bool CopySave(int sourceSlot, int targetSlot)
        {
            if (!HasSave(sourceSlot) || targetSlot < 0 || targetSlot >= CoreConfig.MAX_SAVE_SLOTS)
            {
                Debug.LogWarning("[SaveManager] 复制存档失败：参数无效");
                return false;
            }

            try
            {
                // 加载源存档
                SaveData saveData = LoadDataFromFile(sourceSlot);

                // 临时更改槽位保存
                SaveDataToFile(targetSlot, saveData);

                // 复制元数据
                if (_cachedMetadata.TryGetValue(sourceSlot, out SaveMetadata sourceMeta))
                {
                    SaveMetadata targetMeta = sourceMeta.Clone();
                    targetMeta.SlotIndex = targetSlot;
                    SaveMetadataToFile(targetMeta);
                    _cachedMetadata[targetSlot] = targetMeta;
                }

                Debug.Log($"[SaveManager] 已将槽位 {sourceSlot} 复制到 {targetSlot}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 复制存档失败：{ex.Message}");
                return false;
            }
        }

        #endregion

        #region 公共方法 - 自动保存

        /// <summary>
        /// 启动自动保存
        /// </summary>
        public void StartAutoSave()
        {
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
            }
            _autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
            Debug.Log("[SaveManager] 自动保存已启动");
        }

        /// <summary>
        /// 停止自动保存
        /// </summary>
        public void StopAutoSave()
        {
            if (_autoSaveCoroutine != null)
            {
                StopCoroutine(_autoSaveCoroutine);
                _autoSaveCoroutine = null;
                Debug.Log("[SaveManager] 自动保存已停止");
            }
        }

        /// <summary>
        /// 设置自动保存间隔
        /// </summary>
        /// <param name="interval">间隔时间（秒）</param>
        public void SetAutoSaveInterval(float interval)
        {
            _autoSaveInterval = Mathf.Max(30f, interval);
            UpdateAutoSaveState();
        }

        #endregion

        #region 公共方法 - 存档校验

        /// <summary>
        /// 校验存档数据
        /// </summary>
        /// <param name="data">存档数据</param>
        /// <returns>数据是否有效</returns>
        public bool ValidateSaveData(SaveData data)
        {
            if (data == null)
            {
                return false;
            }

            // 验证版本号
            if (string.IsNullOrEmpty(data.Version) || !data.Version.Equals(Application.version))
            {
                Debug.LogWarning($"[SaveManager] 存档版本不匹配：存档版本 {data.Version}, 游戏版本 {Application.version}");
                // 版本不视为无效，但可能需要数据迁移
            }

            // 验证必需字段
            if (data.SaveTime == default)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 修复损坏的存档
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>是否修复成功</returns>
        public bool RepairSave(int slot)
        {
            if (!HasSave(slot))
            {
                Debug.LogWarning($"[SaveManager] 修复存档失败：槽位 {slot} 没有存档");
                return false;
            }

            try
            {
                SaveData data = LoadDataFromFile(slot);

                if (data == null)
                {
                    // 存档完全损坏，删除
                    DeleteSave(slot);
                    Debug.LogWarning($"[SaveManager] 存档 {slot} 已损坏且无法修复，已删除");
                    return false;
                }

                // 尝试修复
                data.Version = Application.version;
                data.RepairTime = DateTime.Now;

                // 重新保存
                SaveDataToFile(slot, data);

                Debug.Log($"[SaveManager] 存档 {slot} 已修复");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] 修复存档失败：{ex.Message}");
                return false;
            }
        }

        #endregion

        #region 公共方法 - 存档数据

        /// <summary>
        /// 注册可存档组件
        /// </summary>
        /// <param name="component">可存档组件</param>
        public void RegisterSaveable(ISaveable component)
        {
            // 这里可以添加注册逻辑，用于自动收集可存档数据
            Debug.Log($"[SaveManager] 注册可存档组件：{component.GetType().Name}");
        }

        /// <summary>
        /// 注销可存档组件
        /// </summary>
        /// <param name="component">可存档组件</param>
        public void UnregisterSaveable(ISaveable component)
        {
            Debug.Log($"[SaveManager] 注销可存档组件：{component.GetType().Name}");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 确保存档目录存在
        /// </summary>
        private void EnsureSaveDirectory()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
                Debug.Log($"[SaveManager] 已创建存档目录：{_saveDirectory}");
            }
        }

        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        private string GetSaveFilePath(int slot)
        {
            return Path.Combine(_saveDirectory, $"{SAVE_FILE_PREFIX}{slot}{CoreConfig.SAVE_FILE_EXTENSION}");
        }

        /// <summary>
        /// 获取元数据文件路径
        /// </summary>
        private string GetMetadataFilePath(int slot)
        {
            return Path.Combine(_saveDirectory, $"{SAVE_FILE_PREFIX}{slot}{SAVE_METADATA_SUFFIX}");
        }

        /// <summary>
        /// 加载所有存档元数据
        /// </summary>
        private void LoadAllMetadata()
        {
            _cachedMetadata.Clear();

            for (int i = 0; i < CoreConfig.MAX_SAVE_SLOTS; i++)
            {
                string metaPath = GetMetadataFilePath(i);

                if (File.Exists(metaPath))
                {
                    try
                    {
                        string json = File.ReadAllText(metaPath);
                        SaveMetadata metadata = JsonUtility.FromJson<SaveMetadata>(json);
                        _cachedMetadata[i] = metadata;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[SaveManager] 加载元数据失败：槽位 {i}, {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 收集所有可存档数据
        /// </summary>
        private SaveData CollectSaveData()
        {
            SaveData data = new SaveData
            {
                Version = Application.version,
                SaveTime = DateTime.Now,
                SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            };

            // 收集玩家数据
            data.PlayerData = CollectPlayerData();

            // 收集时间数据
            data.TimeData = CollectTimeData();

            // 收集背包数据
            data.InventoryData = CollectInventoryData();

            // 收集任务数据
            data.QuestData = CollectQuestData();

            // 收集世界数据
            data.WorldData = CollectWorldData();

            return data;
        }

        /// <summary>
        /// 创建存档元数据
        /// </summary>
        private SaveMetadata CreateSaveMetadata(int slot)
        {
            SaveMetadata metadata = new SaveMetadata
            {
                SlotIndex = slot,
                SaveTime = DateTime.Now,
                SceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                GameVersion = Application.version
            };

            // 计算游玩时间（从元数据中累加）
            if (_cachedMetadata.TryGetValue(slot, out SaveMetadata oldMeta))
            {
                metadata.TotalPlayTime = oldMeta.TotalPlayTime;
            }

            // 添加本次游玩时间
            metadata.TotalPlayTime += TimeSpan.FromSeconds(Time.realtimeSinceStartup);

            return metadata;
        }

        /// <summary>
        /// 收集玩家数据
        /// </summary>
        private PlayerSaveData CollectPlayerData()
        {
            // TODO: 从实际玩家组件收集数据
            return new PlayerSaveData
            {
                Health = 100,
                MaxHealth = 100,
                Level = 1,
                Experience = 0
            };
        }

        /// <summary>
        /// 收集时间数据
        /// </summary>
        private TimeSaveData CollectTimeData()
        {
            // TODO: 从TimeManager收集时间数据
            return new TimeSaveData
            {
                Day = 1,
                Hour = 8,
                Minute = 0
            };
        }

        /// <summary>
        /// 收集背包数据
        /// </summary>
        private InventorySaveData CollectInventoryData()
        {
            // TODO: 从背包系统收集数据
            return new InventorySaveData
            {
                Items = new List<InventoryItem>()
            };
        }

        /// <summary>
        /// 收集任务数据
        /// </summary>
        private QuestSaveData CollectQuestData()
        {
            // TODO: 从任务系统收集数据
            return new QuestSaveData
            {
                ActiveQuests = new List<string>(),
                CompletedQuests = new List<string>()
            };
        }

        /// <summary>
        /// 收集世界数据
        /// </summary>
        private WorldSaveData CollectWorldData()
        {
            // TODO: 从世界管理系统收集数据
            return new WorldSaveData
            {
                NPCStates = new Dictionary<string, bool>(),
                WorldFlags = new Dictionary<string, int>()
            };
        }

        /// <summary>
        /// 应用存档数据到游戏
        /// </summary>
        private void ApplySaveData(SaveData data)
        {
            // 应用玩家数据
            ApplyPlayerData(data.PlayerData);

            // 应用时间数据
            ApplyTimeData(data.TimeData);

            // 应用背包数据
            ApplyInventoryData(data.InventoryData);

            // 应用任务数据
            ApplyQuestData(data.QuestData);

            // 应用世界数据
            ApplyWorldData(data.WorldData);
        }

        /// <summary>
        /// 应用玩家数据
        /// </summary>
        private void ApplyPlayerData(PlayerSaveData data)
        {
            // TODO: 应用到实际玩家组件
            Debug.Log($"[SaveManager] 应用玩家数据：生命 {data.Health}, 等级 {data.Level}");
        }

        /// <summary>
        /// 应用时间数据
        /// </summary>
        private void ApplyTimeData(TimeSaveData data)
        {
            // TODO: 应用到TimeManager
            Debug.Log($"[SaveManager] 应用时间数据：第{data.Day}天 {data.Hour}点");
        }

        /// <summary>
        /// 应用背包数据
        /// </summary>
        private void ApplyInventoryData(InventorySaveData data)
        {
            // TODO: 应用到背包系统
            Debug.Log($"[SaveManager] 应用背包数据：{data.Items.Count} 个物品");
        }

        /// <summary>
        /// 应用任务数据
        /// </summary>
        private void ApplyQuestData(QuestSaveData data)
        {
            // TODO: 应用到任务系统
            Debug.Log($"[SaveManager] 应用任务数据：{data.ActiveQuests.Count} 个进行中任务");
        }

        /// <summary>
        /// 应用世界数据
        /// </summary>
        private void ApplyWorldData(WorldSaveData data)
        {
            // TODO: 应用到世界管理系统
            Debug.Log($"[SaveManager] 应用世界数据：{data.NPCStates.Count} 个NPC状态");
        }

        /// <summary>
        /// 重置所有可存档数据
        /// </summary>
        private void ResetAllSaveData()
        {
            // TODO: 重置所有可存档系统
            Debug.Log("[SaveManager] 已重置所有存档数据");
        }

        /// <summary>
        /// 保存数据到文件
        /// </summary>
        private void SaveDataToFile(int slot, SaveData data)
        {
            string savePath = GetSaveFilePath(slot);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }

        /// <summary>
        /// 从文件加载数据
        /// </summary>
        private SaveData LoadDataFromFile(int slot)
        {
            string savePath = GetSaveFilePath(slot);

            if (!File.Exists(savePath))
            {
                return null;
            }

            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        /// <summary>
        /// 保存元数据到文件
        /// </summary>
        private void SaveMetadataToFile(SaveMetadata metadata)
        {
            string metaPath = GetMetadataFilePath(metadata.SlotIndex);
            string json = JsonUtility.ToJson(metadata, true);
            File.WriteAllText(metaPath, json);
        }

        /// <summary>
        /// 更新自动保存状态
        /// </summary>
        private void UpdateAutoSaveState()
        {
            if (_autoSaveEnabled && _isInitialized)
            {
                StartAutoSave();
            }
            else
            {
                StopAutoSave();
            }
        }

        #endregion

        #region 协程方法

        /// <summary>
        /// 异步保存协程
        /// </summary>
        private IEnumerator SaveGameAsyncRoutine(int slot)
        {
            _isSaving = true;
            _currentOperationSlot = slot;

            Debug.Log($"[SaveManager] 开始异步保存到槽位 {slot}...");

            // 收集数据
            SaveData saveData = CollectSaveData();

            // 异步保存
            yield return StartCoroutine(SaveDataAsync(saveData, slot));

            _isSaving = false;
            _currentOperationSlot = -1;
        }

        /// <summary>
        /// 异步加载协程
        /// </summary>
        private IEnumerator LoadGameAsyncRoutine(int slot)
        {
            _isLoading = true;
            _currentOperationSlot = slot;

            Debug.Log($"[SaveManager] 开始异步加载槽位 {slot}...");

            // 异步加载
            SaveData saveData = null;
            yield return StartCoroutine(LoadDataAsync(slot, result => saveData = result));

            if (saveData != null && ValidateSaveData(saveData))
            {
                ApplySaveData(saveData);
                EventManager.Instance.TriggerEvent(CoreEvents.GAME_LOADED, new SaveEventArgs(slot, true));
                Debug.Log($"[SaveManager] 异步加载成功：槽位 {slot}");
            }
            else
            {
                EventManager.Instance.TriggerEvent(CoreEvents.LOAD_FAILED, new SaveEventArgs(slot, false, "加载失败"));
            }

            _isLoading = false;
            _currentOperationSlot = -1;
        }

        /// <summary>
        /// 自动保存协程
        /// </summary>
        private IEnumerator AutoSaveRoutine()
        {
            while (_autoSaveEnabled)
            {
                yield return new WaitForSecondsRealtime(AUTO_SAVE_CHECK_INTERVAL);

                if (_autoSaveEnabled && !_isLoading && !_isSaving)
                {
                    // 找到最早的非手动存档槽位进行自动保存
                    int autoSaveSlot = FindAutoSaveSlot();

                    if (autoSaveSlot >= 0)
                    {
                        Debug.Log("[SaveManager] 自动保存触发...");
                        SaveGame(autoSaveSlot);

                        // 触发自动保存事件
                        EventManager.Instance.TriggerEvent(CoreEvents.AUTO_SAVE);

                        // 清理旧自动存档
                        CleanupOldAutoSaves();
                    }
                }
            }
        }

        /// <summary>
        /// 异步保存数据
        /// </summary>
        private IEnumerator SaveDataAsync(SaveData data, int slot)
        {
            // 模拟异步操作
            yield return null;

            SaveMetadata metadata = CreateSaveMetadata(slot);
            SaveDataToFile(slot, data);
            SaveMetadataToFile(metadata);
            _cachedMetadata[slot] = metadata;
            _lastSaveTime = DateTime.Now;
        }

        /// <summary>
        /// 异步加载数据
        /// </summary>
        private IEnumerator LoadDataAsync(int slot, System.Action<SaveData> onComplete)
        {
            // 模拟异步操作
            yield return null;

            SaveData data = LoadDataFromFile(slot);
            onComplete?.Invoke(data);
        }

        /// <summary>
        /// 查找自动保存槽位
        /// </summary>
        private int FindAutoSaveSlot()
        {
            // 优先使用0号槽位进行自动保存
            return 0;
        }

        /// <summary>
        /// 清理旧的自动存档
        /// </summary>
        private void CleanupOldAutoSaves()
        {
            // TODO: 实现自动存档清理逻辑
        }

        #endregion
    }

    #region 存档数据结构

    /// <summary>
    /// 存档数据基类
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// 游戏版本号
        /// </summary>
        public string Version;

        /// <summary>
        /// 保存时间
        /// </summary>
        public DateTime SaveTime;

        /// <summary>
        /// 修复时间
        /// </summary>
        public DateTime RepairTime;

        /// <summary>
        /// 当前场景名称
        /// </summary>
        public string SceneName;

        /// <summary>
        /// 玩家数据
        /// </summary>
        public PlayerSaveData PlayerData;

        /// <summary>
        /// 时间数据
        /// </summary>
        public TimeSaveData TimeData;

        /// <summary>
        /// 背包数据
        /// </summary>
        public InventorySaveData InventoryData;

        /// <summary>
        /// 任务数据
        /// </summary>
        public QuestSaveData QuestData;

        /// <summary>
        /// 世界数据
        /// </summary>
        public WorldSaveData WorldData;
    }

    /// <summary>
    /// 存档元数据
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        /// <summary>
        /// 槽位编号
        /// </summary>
        public int SlotIndex;

        /// <summary>
        /// 保存时间
        /// </summary>
        public DateTime SaveTime;

        /// <summary>
        /// 总游玩时间
        /// </summary>
        public TimeSpan TotalPlayTime;

        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName;

        /// <summary>
        /// 游戏版本
        /// </summary>
        public string GameVersion;

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int PlayerLevel;

        /// <summary>
        /// 存档缩略图数据
        /// </summary>
        public byte[] ThumbnailData;

        /// <summary>
        /// 创建元数据副本
        /// </summary>
        public SaveMetadata Clone()
        {
            return (SaveMetadata)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// 存档槽位信息
    /// </summary>
    public class SaveSlotInfo
    {
        /// <summary>
        /// 槽位编号
        /// </summary>
        public int SlotIndex;

        /// <summary>
        /// 是否有存档
        /// </summary>
        public bool HasSave;

        /// <summary>
        /// 保存时间
        /// </summary>
        public DateTime SaveTime;

        /// <summary>
        /// 总游玩时间
        /// </summary>
        public TimeSpan PlayTime;

        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName;

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int PlayerLevel;
    }

    /// <summary>
    /// 玩家存档数据
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int Health;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHealth;

        /// <summary>
        /// 当前魔法值
        /// </summary>
        public int Mana;

        /// <summary>
        /// 最大魔法值
        /// </summary>
        public int MaxMana;

        /// <summary>
        /// 等级
        /// </summary>
        public int Level;

        /// <summary>
        /// 经验值
        /// </summary>
        public int Experience;

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3Serializable Position;

        /// <summary>
        /// 旋转
        /// </summary>
        public Vector3Serializable Rotation;

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string PlayerName;

        /// <summary>
        /// 职业
        /// </summary>
        public string ClassName;

        /// <summary>
        /// 属性点
        /// </summary>
        public int AttributePoints;

        /// <summary>
        /// 技能点
        /// </summary>
        public int SkillPoints;
    }

    /// <summary>
    /// 时间存档数据
    /// </summary>
    [Serializable]
    public class TimeSaveData
    {
        /// <summary>
        /// 天数
        /// </summary>
        public int Day;

        /// <summary>
        /// 小时
        /// </summary>
        public int Hour;

        /// <summary>
        /// 分钟
        /// </summary>
        public int Minute;

        /// <summary>
        /// 时间流速
        /// </summary>
        public float TimeScale;

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused;
    }

    /// <summary>
    /// 背包存档数据
    /// </summary>
    [Serializable]
    public class InventorySaveData
    {
        /// <summary>
        /// 物品列表
        /// </summary>
        public List<InventoryItem> Items;

        /// <summary>
        /// 金钱数量
        /// </summary>
        public int Gold;

        /// <summary>
        /// 当前装备
        /// </summary>
        public Dictionary<string, string> EquippedItems;
    }

    /// <summary>
    /// 背包物品
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId;

        /// <summary>
        /// 物品名称
        /// </summary>
        public string ItemName;

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity;

        /// <summary>
        /// 品质等级
        /// </summary>
        public int Quality;

        /// <summary>
        /// 附加属性
        /// </summary>
        public List<ItemProperty> Properties;

        /// <summary>
        /// 是否已装备
        /// </summary>
        public bool IsEquipped;
    }

    /// <summary>
    /// 物品属性
    /// </summary>
    [Serializable]
    public class ItemProperty
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 属性值
        /// </summary>
        public float Value;

        /// <summary>
        /// 属性类型
        /// </summary>
        public string Type;
    }

    /// <summary>
    /// 任务存档数据
    /// </summary>
    [Serializable]
    public class QuestSaveData
    {
        /// <summary>
        /// 进行中的任务ID列表
        /// </summary>
        public List<string> ActiveQuests;

        /// <summary>
        /// 已完成的任务ID列表
        /// </summary>
        public List<string> CompletedQuests;

        /// <summary>
        /// 已放弃的任务ID列表
        /// </summary>
        public List<string> AbandonedQuests;

        /// <summary>
        /// 任务变量
        /// </summary>
        public Dictionary<string, int> QuestVariables;
    }

    /// <summary>
    /// 世界存档数据
    /// </summary>
    [Serializable]
    public class WorldSaveData
    {
        /// <summary>
        /// NPC状态字典（ID -> 状态）
        /// </summary>
        public Dictionary<string, bool> NPCStates;

        /// <summary>
        /// 世界开关状态字典
        /// </summary>
        public Dictionary<string, int> WorldFlags;

        /// <summary>
        /// 可破坏物状态
        /// </summary>
        public List<DestroyableObjectState> DestroyableObjects;

        /// <summary>
        /// 传送门状态
        /// </summary>
        public List<PortalState> Portals;
    }

    /// <summary>
    /// 可破坏物状态
    /// </summary>
    [Serializable]
    public class DestroyableObjectState
    {
        /// <summary>
        /// 物体ID
        /// </summary>
        public string ObjectId;

        /// <summary>
        /// 是否已破坏
        /// </summary>
        public bool IsDestroyed;

        /// <summary>
        /// 生命值
        /// </summary>
        public int Health;
    }

    /// <summary>
    /// 传送门状态
    /// </summary>
    [Serializable]
    public class PortalState
    {
        /// <summary>
        /// 传送门ID
        /// </summary>
        public string PortalId;

        /// <summary>
        /// 是否已解锁
        /// </summary>
        public bool IsUnlocked;

        /// <summary>
        /// 目标场景
        /// </summary>
        public string TargetScene;
    }

    #endregion

    #region 可序列化Vector3

    /// <summary>
    /// 可序列化的Vector3
    /// </summary>
    [Serializable]
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public Vector3Serializable() { }

        public Vector3Serializable(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static implicit operator Vector3(Vector3Serializable v)
        {
            return v.ToVector3();
        }

        public static implicit operator Vector3Serializable(Vector3 v)
        {
            return new Vector3Serializable(v);
        }
    }

    #endregion
}
