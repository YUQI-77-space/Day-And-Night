using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 管理器接口，定义所有管理器应实现的通用方法
    /// </summary>
    public interface IManager
    {
        #region 公共方法

        /// <summary>
        /// 初始化管理器
        /// </summary>
        /// <returns>初始化是否成功</returns>
        bool Initialize();

        /// <summary>
        /// 每帧更新
        /// </summary>
        void Update();

        /// <summary>
        /// 关闭管理器
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 获取管理器名称
        /// </summary>
        /// <returns>管理器名称</returns>
        string GetManagerName();

        #endregion

        #region 属性

        /// <summary>
        /// 获取管理器是否已完成初始化
        /// </summary>
        bool IsInitialized { get; }

        #endregion
    }
}
