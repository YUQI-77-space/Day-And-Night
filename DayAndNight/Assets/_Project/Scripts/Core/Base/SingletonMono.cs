using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 单例MonoBehaviour基类，提供全局访问点和跨场景保持功能
    /// </summary>
    /// <typeparam name="T">继承此类的类型</typeparam>
    public abstract class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region 静态字段

        /// <summary>
        /// 单例实例的全局访问点
        /// </summary>
        public static T Instance { get; private set; }

        #endregion

        #region 保护字段

        /// <summary>
        /// 标记是否已经执行过Awake初始化
        /// </summary>
        protected bool _isAwaken = false;

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 唤醒时初始化单例
        /// </summary>
        protected virtual void Awake()
        {
            if (_isAwaken)
            {
                return;
            }

            _isAwaken = true;

            if (Instance == null)
            {
                Instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else
            {
                Debug.LogWarning($"[Singleton] 发现重复的 {typeof(T).Name} 实例，已自动销毁");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 销毁时清理单例引用
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                OnSingletonDestroy();
            }
        }

        #endregion

        #region 虚方法

        /// <summary>
        /// 单例Awake完成后的回调，可用于子类初始化
        /// </summary>
        protected virtual void OnSingletonAwake()
        {
        }

        /// <summary>
        /// 单例销毁时的回调，可用于子类清理
        /// </summary>
        protected virtual void OnSingletonDestroy()
        {
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 尝试获取单例实例，如果未初始化返回null
        /// </summary>
        /// <returns>单例实例或null</returns>
        public static T TryGetInstance()
        {
            return Instance;
        }

        /// <summary>
        /// 检查单例是否已初始化
        /// </summary>
        /// <returns>是否已初始化</returns>
        public static bool IsInitialized()
        {
            return Instance != null;
        }

        #endregion
    }
}
