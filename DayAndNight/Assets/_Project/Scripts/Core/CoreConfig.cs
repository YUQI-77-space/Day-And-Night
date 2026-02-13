using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 核心配置常量定义
    /// 包含游戏核心系统的各种配置参数
    /// </summary>
    public static class CoreConfig
    {
        #region 时间系统配置

        /// <summary>
        /// 白天开始时间（小时，0-24）
        /// </summary>
        public const int DAY_START_HOUR = 6;

        /// <summary>
        /// 夜晚开始时间（小时，0-24）
        /// </summary>
        public const int NIGHT_START_HOUR = 18;

        /// <summary>
        /// 默认时间流速（1秒现实时间 = X秒游戏时间）
        /// </summary>
        public const float DEFAULT_TIME_SCALE = 60f;

        /// <summary>
        /// 最小时间流速
        /// </summary>
        public const float MIN_TIME_SCALE = 0f;

        /// <summary>
        /// 最大时间流速
        /// </summary>
        public const float MAX_TIME_SCALE = 1000f;

        /// <summary>
        /// 游戏日中现实秒数（用于计算）
        /// </summary>
        public const float REAL_SECONDS_PER_GAME_DAY = 86400f;

        /// <summary>
        /// 默认每帧时间增量（秒）
        /// </summary>
        public const float DEFAULT_TIME_INCREMENT = 0.1f;

        #endregion

        #region 存档配置

        /// <summary>
        /// 最大存档槽位数量
        /// </summary>
        public const int MAX_SAVE_SLOTS = 10;

        /// <summary>
        /// 自动保存间隔（秒）
        /// </summary>
        public const float AUTO_SAVE_INTERVAL = 300f;

        /// <summary>
        /// 存档文件扩展名
        /// </summary>
        public const string SAVE_FILE_EXTENSION = ".save";

        /// <summary>
        /// 存档目录名称
        /// </summary>
        public const string SAVE_DIRECTORY_NAME = "Saves";

        #endregion

        #region 场景配置

        /// <summary>
        /// 默认加载场景名称
        /// </summary>
        public const string DEFAULT_SCENE = "Main";

        /// <summary>
        /// 主菜单场景名称
        /// </summary>
        public const string MAIN_MENU_SCENE = "Main";

        /// <summary>
        /// 游戏主场景名称
        /// </summary>
        public const string GAMEPLAY_SCENE = "Town";

        /// <summary>
        /// 加载超时时间（秒）
        /// </summary>
        public const float SCENE_LOAD_TIMEOUT = 30f;

        #endregion

        #region 音频配置

        /// <summary>
        /// 默认背景音乐音量
        /// </summary>
        public const float DEFAULT_MUSIC_VOLUME = 0.7f;

        /// <summary>
        /// 默认音效音量
        /// </summary>
        public const float DEFAULT_SFX_VOLUME = 0.8f;

        /// <summary>
        /// 默认语音音量
        /// </summary>
        public const float DEFAULT_VOICE_VOLUME = 1.0f;

        /// <summary>
        /// 音乐淡入淡出时间（秒）
        /// </summary>
        public const float MUSIC_FADE_DURATION = 1.0f;

        #endregion

        #region UI配置

        /// <summary>
        /// 默认UI缩放比例
        /// </summary>
        public const float DEFAULT_UI_SCALE = 1.0f;

        /// <summary>
        /// 提示信息显示时间（秒）
        /// </summary>
        public const float TOAST_DURATION = 3.0f;

        /// <summary>
        /// 菜单切换动画时间（秒）
        /// </summary>
        public const float MENU_TRANSITION_DURATION = 0.3f;

        #endregion

        #region 网络配置

        /// <summary>
        /// 默认服务器地址
        /// </summary>
        public const string DEFAULT_SERVER_ADDRESS = "localhost";

        /// <summary>
        /// 默认端口
        /// </summary>
        public const int DEFAULT_PORT = 7777;

        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
        public const int CONNECTION_TIMEOUT = 10;

        #endregion

        #region 调试配置

        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public const bool ENABLE_DEBUG_MODE = false;

        /// <summary>
        /// 是否显示调试UI
        /// </summary>
        public const bool SHOW_DEBUG_UI = false;

        /// <summary>
        /// 日志最大保留数量
        /// </summary>
        public const int MAX_LOG_ENTRIES = 100;

        #endregion
    }
}
