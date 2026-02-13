using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 可存档数据接口，定义需要支持游戏存档的对象
    /// 实现此接口的类可以通过SaveManager进行存档和读档
    /// </summary>
    public interface ISaveable
    {
        #region 公共方法

        /// <summary>
        /// 获取存档数据
        /// </summary>
        /// <returns>存档数据对象，用于序列化存储</returns>
        object GetSaveData();

        /// <summary>
        /// 加载存档数据
        /// </summary>
        /// <param name="data">从存档中读取的数据对象</param>
        void LoadSaveData(object data);

        /// <summary>
        /// 重置存档数据
        /// </summary>
        /// <remarks>
        /// 当开始新游戏时调用此方法，将数据重置为初始状态
        /// </remarks>
        void ResetSaveData();

        #endregion
    }
}
