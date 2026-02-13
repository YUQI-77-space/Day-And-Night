using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 时间管理器，负责管理游戏内时间的流逝、昼夜循环和时间控制
    /// 继承自BaseManager以获得统一的初始化流程
    /// </summary>
    public class TimeManager : BaseManager<TimeManager>
    {
        #region 私有字段

        /// <summary>
        /// 当前游戏天数（从游戏开始计算）
        /// </summary>
        private int _currentDay = 1;

        /// <summary>
        /// 当前小时（0-23）
        /// </summary>
        private int _currentHour = 6;

        /// <summary>
        /// 当前分钟（0-59）
        /// </summary>
        private int _currentMinute = 0;

        /// <summary>
        /// 游戏时间流速（1秒现实时间 = X秒游戏时间）
        /// </summary>
        private float _timeScale = CoreConfig.DEFAULT_TIME_SCALE;

        /// <summary>
        /// 是否暂停时间流逝
        /// </summary>
        private bool _isPaused = false;

        /// <summary>
        /// 是否是白天
        /// </summary>
        private bool _isDay = true;

        /// <summary>
        /// 上一次的小时数（用于检测小时变化）
        /// </summary>
        private int _lastHour = 6;

        /// <summary>
        /// 上一天数（用于检测天数变化）
        /// </summary>
        private int _lastDay = 1;

        #endregion

        #region Unity生命周期方法

        /// <summary>
        /// 初始化时间管理器
        /// </summary>
        protected override void OnInitialize()
        {
            _currentDay = 1;
            _currentHour = CoreConfig.DAY_START_HOUR;
            _currentMinute = 0;
            _isDay = true;
            _timeScale = CoreConfig.DEFAULT_TIME_SCALE;
            _isPaused = false;

            Debug.Log($"[TimeManager] 时间管理器初始化完成，当前时间: {GetFormattedTime()}");
        }

        /// <summary>
        /// 每帧更新时间
        /// </summary>
        protected override void OnUpdate()
        {
            if (_isPaused)
            {
                return;
            }

            // 根据时间流速增加游戏时间
            float timeIncrement = CoreConfig.DEFAULT_TIME_INCREMENT * (_timeScale / CoreConfig.DEFAULT_TIME_SCALE);
            _currentMinute += Mathf.FloorToInt(timeIncrement);

            // 处理分钟溢出
            while (_currentMinute >= 60)
            {
                _currentMinute -= 60;
                _currentHour++;
            }

            // 处理小时溢出
            while (_currentHour >= 24)
            {
                _currentHour -= 24;
                _currentDay++;
            }

            // 检查时间变化事件
            _CheckTimeEvents();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 获取当前游戏天数
        /// </summary>
        public int CurrentDay => _currentDay;

        /// <summary>
        /// 获取当前小时（0-23）
        /// </summary>
        public int CurrentHour => _currentHour;

        /// <summary>
        /// 获取当前分钟（0-59）
        /// </summary>
        public int CurrentMinute => _currentMinute;

        /// <summary>
        /// 获取当前游戏总分钟数
        /// </summary>
        public int TotalMinutes => _currentDay * 24 * 60 + _currentHour * 60 + _currentMinute;

        /// <summary>
        /// 获取当前游戏总小时数
        /// </summary>
        public int TotalHours => _currentDay * 24 + _currentHour;

        /// <summary>
        /// 获取或设置时间流速
        /// </summary>
        public float TimeScale
        {
            get => _timeScale;
            set
            {
                _timeScale = Mathf.Clamp(value, CoreConfig.MIN_TIME_SCALE, CoreConfig.MAX_TIME_SCALE);
                EventManager.Trigger(CoreEvents.TIME_SCALE_CHANGED, _timeScale);
            }
        }

        /// <summary>
        /// 获取时间流速倍数（相对于默认流速）
        /// </summary>
        public float TimeScaleMultiplier => _timeScale / CoreConfig.DEFAULT_TIME_SCALE;

        /// <summary>
        /// 获取或设置是否暂停
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused != value)
                {
                    _isPaused = value;
                    if (_isPaused)
                    {
                        EventManager.Trigger(CoreEvents.GAME_PAUSED);
                    }
                    else
                    {
                        EventManager.Trigger(CoreEvents.GAME_RESUMED);
                    }
                }
            }
        }

        /// <summary>
        /// 获取是否是白天
        /// </summary>
        public bool IsDay => _isDay;

        /// <summary>
        /// 获取是否是夜晚
        /// </summary>
        public bool IsNight => !_isDay;

        /// <summary>
        /// 获取昼夜百分比（0-1，0为午夜，0.5为正午，1为下一个午夜）
        /// </summary>
        public float DayNightRatio
        {
            get
            {
                float totalMinutes = _currentHour * 60f + _currentMinute;
                return totalMinutes / 1440f;
            }
        }

        #endregion

        #region 公共方法 - 时间控制

        /// <summary>
        /// 暂停时间流逝
        /// </summary>
        public void Pause()
        {
            IsPaused = true;
            Debug.Log("[TimeManager] 时间已暂停");
        }

        /// <summary>
        /// 恢复时间流逝
        /// </summary>
        public void Resume()
        {
            IsPaused = false;
            Debug.Log("[TimeManager] 时间已恢复");
        }

        /// <summary>
        /// 设置时间流速
        /// </summary>
        /// <param name="scale">时间流速（1秒现实时间 = scale秒游戏时间）</param>
        public void SetTimeScale(float scale)
        {
            TimeScale = scale;
            Debug.Log($"[TimeManager] 时间流速设置为: {scale}");
        }

        /// <summary>
        /// 加速时间
        /// </summary>
        /// <param name="multiplier">加速倍数</param>
        public void SpeedUp(float multiplier = 2f)
        {
            SetTimeScale(_timeScale * multiplier);
        }

        /// <summary>
        /// 减速时间
        /// </summary>
        /// <param name="divisor">减速除数</param>
        public void SlowDown(float divisor = 2f)
        {
            SetTimeScale(_timeScale / divisor);
        }

        /// <summary>
        /// 重置为默认时间流速
        /// </summary>
        public void ResetTimeScale()
        {
            SetTimeScale(CoreConfig.DEFAULT_TIME_SCALE);
        }

        #endregion

        #region 公共方法 - 时间跳转

        /// <summary>
        /// 推进指定分钟数
        /// </summary>
        /// <param name="minutes">要推进的分钟数</param>
        public void AdvanceMinutes(int minutes)
        {
            if (minutes < 0)
            {
                Debug.LogWarning("[TimeManager] 无法推进负数分钟");
                return;
            }

            _currentMinute += minutes;
            _NormalizeTime();
            _CheckTimeEvents();

            Debug.Log($"[TimeManager] 推进{minutes}分钟，当前时间: {GetFormattedTime()}");
        }

        /// <summary>
        /// 推进指定小时数
        /// </summary>
        /// <param name="hours">要推进的小时数</param>
        public void AdvanceHours(int hours)
        {
            if (hours < 0)
            {
                Debug.LogWarning("[TimeManager] 无法推进负数小时");
                return;
            }

            _currentHour += hours;
            _NormalizeTime();
            _CheckTimeEvents();

            Debug.Log($"[TimeManager] 推进{hours}小时，当前时间: {GetFormattedTime()}");
        }

        /// <summary>
        /// 推进指定天数
        /// </summary>
        /// <param name="days">要推进的天数</param>
        public void AdvanceDays(int days)
        {
            if (days < 0)
            {
                Debug.LogWarning("[TimeManager] 无法推进负数天");
                return;
            }

            _currentDay += days;
            _CheckTimeEvents();

            Debug.Log($"[TimeManager] 推进{days}天，当前天数: {_currentDay}");
        }

        /// <summary>
        /// 设置指定时间
        /// </summary>
        /// <param name="hour">小时（0-23）</param>
        /// <param name="minute">分钟（0-59）</param>
        public void SetTime(int hour, int minute)
        {
            _currentHour = Mathf.Clamp(hour, 0, 23);
            _currentMinute = Mathf.Clamp(minute, 0, 59);
            _CheckTimeEvents();

            Debug.Log($"[TimeManager] 时间设置为: {GetFormattedTime()}");
        }

        /// <summary>
        /// 设置指定日期
        /// </summary>
        /// <param name="day">天数（从1开始）</param>
        /// <param name="hour">小时（0-23）</param>
        /// <param name="minute">分钟（0-59）</param>
        public void SetDate(int day, int hour = 0, int minute = 0)
        {
            _currentDay = Mathf.Max(1, day);
            _currentHour = Mathf.Clamp(hour, 0, 23);
            _currentMinute = Mathf.Clamp(minute, 0, 59);
            _CheckTimeEvents();

            Debug.Log($"[TimeManager] 日期设置为: 第{_currentDay}天 {GetFormattedTime()}");
        }

        #endregion

        #region 公共方法 - 格式化输出

        /// <summary>
        /// 获取格式化的时间字符串（HH:MM）
        /// </summary>
        /// <returns>格式化的字符串</returns>
        public string GetFormattedTime()
        {
            return $"{_currentHour:D2}:{_currentMinute:D2}";
        }

        /// <summary>
        /// 获取格式化的日期时间字符串
        /// </summary>
        /// <returns>格式化的字符串</returns>
        public string GetFormattedDateTime()
        {
            return $"第{_currentDay}天 {_currentHour:D2}:{_currentMinute:D2}";
        }

        /// <summary>
        /// 获取详细的时间描述
        /// </summary>
        /// <returns>时间描述字符串</returns>
        public string GetTimeDescription()
        {
            if (_currentHour >= 5 && _currentHour < 8)
            {
                return "清晨";
            }
            else if (_currentHour >= 8 && _currentHour < 12)
            {
                return "上午";
            }
            else if (_currentHour >= 12 && _currentHour < 14)
            {
                return "中午";
            }
            else if (_currentHour >= 14 && _currentHour < 18)
            {
                return "下午";
            }
            else if (_currentHour >= 18 && _currentHour < 20)
            {
                return "傍晚";
            }
            else if (_currentHour >= 20 && _currentHour < 24)
            {
                return "夜晚";
            }
            else
            {
                return "深夜";
            }
        }

        /// <summary>
        /// 获取当前时间段的描述
        /// </summary>
        /// <returns>时间段名称</returns>
        public string GetDayPeriodName()
        {
            return _isDay ? "白天" : "夜晚";
        }

        #endregion

        #region 公共方法 - 保存数据

        /// <summary>
        /// 获取存档数据
        /// </summary>
        /// <returns>存档数据对象</returns>
        public object GetSaveData()
        {
            return new TimeSaveData
            {
                Day = _currentDay,
                Hour = _currentHour,
                Minute = _currentMinute,
                TimeScale = _timeScale,
                IsPaused = _isPaused
            };
        }

        /// <summary>
        /// 加载存档数据
        /// </summary>
        /// <param name="data">存档数据对象</param>
        public void LoadSaveData(object data)
        {
            if (data is TimeSaveData saveData)
            {
                _currentDay = saveData.Day;
                _currentHour = saveData.Hour;
                _currentMinute = saveData.Minute;
                _timeScale = saveData.TimeScale;
                _isPaused = saveData.IsPaused;

                _UpdateDayNightState();
                _CheckTimeEvents();

                Debug.Log($"[TimeManager] 存档加载完成，时间: {GetFormattedDateTime()}");
            }
        }

        /// <summary>
        /// 重置存档数据
        /// </summary>
        public void ResetSaveData()
        {
            _currentDay = 1;
            _currentHour = CoreConfig.DAY_START_HOUR;
            _currentMinute = 0;
            _timeScale = CoreConfig.DEFAULT_TIME_SCALE;
            _isPaused = false;
            _isDay = true;

            Debug.Log("[TimeManager] 时间数据已重置");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 规范化时间（处理溢出）
        /// </summary>
        private void _NormalizeTime()
        {
            while (_currentMinute >= 60)
            {
                _currentMinute -= 60;
                _currentHour++;
            }

            while (_currentHour >= 24)
            {
                _currentHour -= 24;
                _currentDay++;
            }

            _UpdateDayNightState();
        }

        /// <summary>
        /// 检查并触发时间变化事件
        /// </summary>
        private void _CheckTimeEvents()
        {
            // 检查小时变化
            if (_currentHour != _lastHour)
            {
                _lastHour = _currentHour;
                EventManager.Trigger(CoreEvents.HOUR_CHANGED, _currentHour);

                // 检查昼夜变化
                _UpdateDayNightState();
            }

            // 检查天数变化
            if (_currentDay != _lastDay)
            {
                _lastDay = _currentDay;
                EventManager.Trigger(CoreEvents.DAY_CHANGED, _currentDay);
            }
        }

        /// <summary>
        /// 更新昼夜状态
        /// </summary>
        private void _UpdateDayNightState()
        {
            bool wasDay = _isDay;
            _isDay = _currentHour >= CoreConfig.DAY_START_HOUR &&
                     _currentHour < CoreConfig.NIGHT_START_HOUR;

            if (wasDay != _isDay)
            {
                EventManager.Trigger(CoreEvents.DAY_NIGHT_CHANGED, new DayNightEventArgs(_isDay, _currentDay, _currentHour));
                Debug.Log($"[TimeManager] 昼夜切换: {(IsDay ? "白天" : "夜晚")}");
            }
        }

        #endregion
    }
}
