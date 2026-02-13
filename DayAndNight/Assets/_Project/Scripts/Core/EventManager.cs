using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 事件管理器，提供事件的注册、注销和触发功能
    /// 使用C# Action实现类型安全的事件系统
    /// </summary>
    public class EventManager : BaseManager<EventManager>
    {
        #region 私有字段

        /// <summary>
        /// 无参数事件字典
        /// </summary>
        private Dictionary<string, Action> _eventDictionary = new Dictionary<string, Action>();

        /// <summary>
        /// 带参数事件字典
        /// </summary>
        private Dictionary<string, Delegate> _parameterizedEventDictionary = new Dictionary<string, Delegate>();

        /// <summary>
        /// 是否启用事件历史记录
        /// </summary>
        [SerializeField] private bool _enableEventHistory = false;

        /// <summary>
        /// 事件历史记录最大数量
        /// </summary>
        [SerializeField] private int _maxEventHistory = 100;

        /// <summary>
        /// 事件历史记录
        /// </summary>
        private Queue<string> _eventHistory = new Queue<string>();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized = false;

        #endregion

        #region 属性

        /// <summary>
        /// 是否启用事件历史记录
        /// </summary>
        public bool EnableEventHistory
        {
            get => _enableEventHistory;
            set => _enableEventHistory = value;
        }

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 初始化事件管理器
        /// </summary>
        protected override void OnInitialize()
        {
            _isInitialized = true;
            Debug.Log("[EventManager] 事件管理器已初始化");
        }

        /// <summary>
        /// 销毁时清理所有事件
        /// </summary>
        protected override void OnDestroy()
        {
            ClearAllEvents();
            _eventDictionary.Clear();
            _parameterizedEventDictionary.Clear();
            base.OnDestroy();
        }

        #endregion

        #region 公共方法 - 无参数事件

        /// <summary>
        /// 注册无参数事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void RegisterEvent(string eventName, Action listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning("[EventManager] 注册事件时，事件名称不能为空");
                return;
            }

            if (listener == null)
            {
                Debug.LogWarning($"[EventManager] 注册事件 {eventName} 时，监听器为null");
                return;
            }

            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName] += listener;
            }
            else
            {
                _eventDictionary.Add(eventName, listener);
            }

            _LogEventAction("Registered", eventName);
        }

        /// <summary>
        /// 注销无参数事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void UnregisterEvent(string eventName, Action listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (listener == null)
            {
                return;
            }

            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName] -= listener;

                if (_eventDictionary[eventName] == null)
                {
                    _eventDictionary.Remove(eventName);
                }
            }

            _LogEventAction("Unregistered", eventName);
        }

        /// <summary>
        /// 触发无参数事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void TriggerEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning("[EventManager] 触发事件时，事件名称不能为空");
                return;
            }

            if (_eventDictionary.ContainsKey(eventName))
            {
                try
                {
                    _eventDictionary[eventName]?.Invoke();
                    _LogEventAction("Triggered", eventName);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventManager] 触发事件 {eventName} 时发生错误: {ex.Message}");
                }
            }
            else
            {
                if (_enableEventHistory)
                {
                    _LogEventAction("Triggered (No Listeners)", eventName);
                }
            }
        }

        #endregion

        #region 公共方法 - 带参数事件

        /// <summary>
        /// 注册带参数事件
        /// </summary>
        /// <typeparam name="T">事件参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void RegisterEvent<T>(string eventName, Action<T> listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning("[EventManager] 注册事件时，事件名称不能为空");
                return;
            }

            if (listener == null)
            {
                Debug.LogWarning($"[EventManager] 注册事件 {eventName} 时，监听器为null");
                return;
            }

            if (_parameterizedEventDictionary.ContainsKey(eventName))
            {
                _parameterizedEventDictionary[eventName] = Delegate.Combine(_parameterizedEventDictionary[eventName], listener);
            }
            else
            {
                _parameterizedEventDictionary.Add(eventName, listener);
            }

            _LogEventAction("Registered", eventName);
        }

        /// <summary>
        /// 注册带两个参数的事件
        /// </summary>
        /// <typeparam name="T1">第一个参数类型</typeparam>
        /// <typeparam name="T2">第二个参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void RegisterEvent<T1, T2>(string eventName, Action<T1, T2> listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (listener == null)
            {
                return;
            }

            if (_parameterizedEventDictionary.ContainsKey(eventName))
            {
                _parameterizedEventDictionary[eventName] = Delegate.Combine(_parameterizedEventDictionary[eventName], listener);
            }
            else
            {
                _parameterizedEventDictionary.Add(eventName, listener);
            }
        }

        /// <summary>
        /// 注销带参数事件
        /// </summary>
        /// <typeparam name="T">事件参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void UnregisterEvent<T>(string eventName, Action<T> listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (listener == null)
            {
                return;
            }

            if (_parameterizedEventDictionary.ContainsKey(eventName))
            {
                _parameterizedEventDictionary[eventName] = Delegate.Remove(_parameterizedEventDictionary[eventName], listener);

                if (_parameterizedEventDictionary[eventName] == null)
                {
                    _parameterizedEventDictionary.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// 注销带两个参数的事件
        /// </summary>
        /// <typeparam name="T1">第一个参数类型</typeparam>
        /// <typeparam name="T2">第二个参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听器</param>
        public void UnregisterEvent<T1, T2>(string eventName, Action<T1, T2> listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (listener == null)
            {
                return;
            }

            if (_parameterizedEventDictionary.ContainsKey(eventName))
            {
                _parameterizedEventDictionary[eventName] = Delegate.Remove(_parameterizedEventDictionary[eventName], listener);

                if (_parameterizedEventDictionary[eventName] == null)
                {
                    _parameterizedEventDictionary.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// 触发带参数事件
        /// </summary>
        /// <typeparam name="T">事件参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="arg1">事件参数</param>
        public void TriggerEvent<T>(string eventName, T arg1)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning("[EventManager] 触发事件时，事件名称不能为空");
                return;
            }

            if (_parameterizedEventDictionary.ContainsKey(eventName))
            {
                try
                {
                    (_parameterizedEventDictionary[eventName] as Action<T>)?.Invoke(arg1);
                    _LogEventAction("Triggered", eventName);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventManager] 触发事件 {eventName} 时发生错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 触发带两个参数的事件
        /// </summary>
        /// <typeparam name="T1">第一个参数类型</typeparam>
        /// <typeparam name="T2">第二个参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="arg1">第一个事件参数</param>
        /// <param name="arg2">第二个事件参数</param>
        public void TriggerEvent<T1, T2>(string eventName, T1 arg1, T2 arg2)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (_parameterizedEventDictionary.ContainsKey(eventName))
            {
                try
                {
                    (_parameterizedEventDictionary[eventName] as Action<T1, T2>)?.Invoke(arg1, arg2);
                    _LogEventAction("Triggered", eventName);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventManager] 触发事件 {eventName} 时发生错误: {ex.Message}");
                }
            }
        }

        #endregion

        #region 公共方法 - 事件管理

        /// <summary>
        /// 检查事件是否已注册
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <returns>是否已注册</returns>
        public bool HasEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return false;
            }

            return _eventDictionary.ContainsKey(eventName) || _parameterizedEventDictionary.ContainsKey(eventName);
        }

        /// <summary>
        /// 清除所有事件监听
        /// </summary>
        public void ClearAllEvents()
        {
            foreach (var key in _eventDictionary.Keys)
            {
                _eventDictionary[key] = null;
            }
            _eventDictionary.Clear();

            foreach (var key in _parameterizedEventDictionary.Keys)
            {
                _parameterizedEventDictionary[key] = null;
            }
            _parameterizedEventDictionary.Clear();

            Debug.Log("[EventManager] 已清除所有事件监听");
        }

        /// <summary>
        /// 清除指定事件的所有监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void ClearEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName] = null;
                _eventDictionary.Remove(eventName);
            }

            if (_parameterizedEventDictionary.ContainsKey(eventName))
            {
                _parameterizedEventDictionary[eventName] = null;
                _parameterizedEventDictionary.Remove(eventName);
            }
        }

        /// <summary>
        /// 获取事件监听器数量
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <returns>监听器数量</returns>
        public int GetListenerCount(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return 0;
            }

            if (_eventDictionary.TryGetValue(eventName, out Action action))
            {
                return action?.GetInvocationList().Length ?? 0;
            }

            if (_parameterizedEventDictionary.TryGetValue(eventName, out Delegate del))
            {
                return del?.GetInvocationList().Length ?? 0;
            }

            return 0;
        }

        /// <summary>
        /// 获取所有已注册的事件名称
        /// </summary>
        /// <returns>事件名称列表</returns>
        public List<string> GetRegisteredEvents()
        {
            var events = new List<string>();

            foreach (var key in _eventDictionary.Keys)
            {
                events.Add(key);
            }

            foreach (var key in _parameterizedEventDictionary.Keys)
            {
                if (!events.Contains(key))
                {
                    events.Add(key);
                }
            }

            return events;
        }

        #endregion

        #region 事件历史

        /// <summary>
        /// 获取事件历史记录
        /// </summary>
        /// <returns>事件历史记录列表</returns>
        public List<string> GetEventHistory()
        {
            return new List<string>(_eventHistory);
        }

        /// <summary>
        /// 清空事件历史记录
        /// </summary>
        public void ClearEventHistory()
        {
            _eventHistory.Clear();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 记录事件操作日志
        /// </summary>
        /// <param name="action">操作类型</param>
        /// <param name="eventName">事件名称</param>
        private void _LogEventAction(string action, string eventName)
        {
            if (!_enableEventHistory)
            {
                return;
            }

            var logEntry = $"[{Time.time:F2}] {action}: {eventName}";
            _eventHistory.Enqueue(logEntry);

            while (_eventHistory.Count > _maxEventHistory)
            {
                _eventHistory.Dequeue();
            }
        }

        #endregion

        #region 静态便捷方法

        /// <summary>
        /// 便捷方法：注册无参数事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">监听器</param>
        public static void Register(string eventName, Action listener)
        {
            Instance?.RegisterEvent(eventName, listener);
        }

        /// <summary>
        /// 便捷方法：注销无参数事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">监听器</param>
        public static void Unregister(string eventName, Action listener)
        {
            Instance?.UnregisterEvent(eventName, listener);
        }

        /// <summary>
        /// 便捷方法：触发无参数事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public static void Trigger(string eventName)
        {
            Instance?.TriggerEvent(eventName);
        }

        /// <summary>
        /// 便捷方法：注册带参数事件
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">监听器</param>
        public static void Register<T>(string eventName, Action<T> listener)
        {
            Instance?.RegisterEvent(eventName, listener);
        }

        /// <summary>
        /// 便捷方法：注销带参数事件
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">监听器</param>
        public static void Unregister<T>(string eventName, Action<T> listener)
        {
            Instance?.UnregisterEvent(eventName, listener);
        }

        /// <summary>
        /// 便捷方法：触发带参数事件
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventName">事件名称</param>
        /// <param name="arg">参数</param>
        public static void Trigger<T>(string eventName, T arg)
        {
            Instance?.TriggerEvent(eventName, arg);
        }

        #endregion
    }
}
