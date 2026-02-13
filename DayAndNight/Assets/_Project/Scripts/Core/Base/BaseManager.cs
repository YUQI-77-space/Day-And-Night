using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 管理器泛型基类，提供统一的初始化流程和生命周期管理
    /// </summary>
    /// <typeparam name="T">继承此类的管理器类型</typeparam>
    public abstract class BaseManager<T> : SingletonMono<T>, IManager where T : MonoBehaviour
    {
        #region 保护字段

        /// <summary>
        /// 管理器是否已完成初始化
        /// </summary>
        protected bool _isInitialized = false;

        /// <summary>
        /// 管理器名称
        /// </summary>
        protected string _managerName;

        #endregion

        #region 属性

        /// <summary>
        /// 获取管理器是否已完成初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 获取管理器名称
        /// </summary>
        public string ManagerName => _managerName;

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 启动时执行初始化
        /// </summary>
        private void Start()
        {
            base.Awake();  // 调用父类的 Awake

            if (!_isInitialized)
            {
                _managerName = typeof(T).Name;
                _isInitialized = Initialize();
            }
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public virtual void Update()
        {
            if (_isInitialized)
            {
                OnUpdate();
            }
        }

        /// <summary>
        /// 禁用时取消事件订阅
        /// </summary>
        private void OnDisable()
        {
            if (_isInitialized)
            {
                Shutdown();
            }
        }

        #endregion

        #region 虚方法

        /// <summary>
        /// 初始化管理器
        /// </summary>
        /// <returns>初始化是否成功</returns>
        public virtual bool Initialize()
        {
            Debug.Log($"[{_managerName}] 开始初始化...");

            try
            {
                OnInitialize();
                Debug.Log($"[{_managerName}] 初始化完成");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[{_managerName}] 初始化失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 每帧更新逻辑
        /// </summary>
        protected virtual void OnUpdate()
        {
        }

        /// <summary>
        /// 执行初始化逻辑，子类重写此方法实现具体初始化
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// 关闭管理器
        /// </summary>
        public virtual void Shutdown()
        {
            Debug.Log($"[{_managerName}] 正在关闭...");
            OnShutdown();
            _isInitialized = false;
            Debug.Log($"[{_managerName}] 已关闭");
        }

        /// <summary>
        /// 执行关闭逻辑，子类重写此方法实现具体清理
        /// </summary>
        protected virtual void OnShutdown()
        {
        }

        /// <summary>
        /// 获取管理器名称
        /// </summary>
        /// <returns>管理器名称</returns>
        public virtual string GetManagerName()
        {
            return _managerName;
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 获取当前管理器实例
        /// </summary>
        /// <returns>管理器实例</returns>
        public static T Get()
        {
            return Instance;
        }

        #endregion
    }
}
