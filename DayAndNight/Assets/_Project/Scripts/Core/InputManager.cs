using System;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// 输入定义常量
    /// </summary>
    public static class InputActions
    {
        #region 移动输入

        /// <summary>
        /// 水平移动轴
        /// </summary>
        public const string HORIZONTAL = "Horizontal";

        /// <summary>
        /// 垂直移动轴
        /// </summary>
        public const string VERTICAL = "Vertical";

        /// <summary>
        /// 冲刺/奔跑
        /// </summary>
        public const string SPRINT = "Sprint";

        /// <summary>
        /// 跳跃
        /// </summary>
        public const string JUMP = "Jump";

        /// <summary>
        /// 蹲下
        /// </summary>
        public const string CROUCH = "Crouch";

        #endregion

        #region 交互输入

        /// <summary>
        /// 交互/确认
        /// </summary>
        public const string INTERACT = "Interact";

        /// <summary>
        /// 取消/返回
        /// </summary>
        public const string CANCEL = "Cancel";

        #endregion

        #region 战斗输入

        /// <summary>
        /// 攻击
        /// </summary>
        public const string ATTACK = "Attack";

        /// <summary>
        /// 闪避/翻滚
        /// </summary>
        public const string DODGE = "Dodge";

        /// <summary>
        /// 特殊技能
        /// </summary>
        public const string SPECIAL = "Special";

        /// <summary>
        /// 格挡
        /// </summary>
        public const string BLOCK = "Block";

        #endregion

        #region UI输入

        /// <summary>
        /// 菜单
        /// </summary>
        public const string MENU = "Menu";

        /// <summary>
        /// 背包
        /// </summary>
        public const string INVENTORY = "Inventory";

        /// <summary>
        /// 地图
        /// </summary>
        public const string MAP = "Map";

        /// <summary>
        /// 物品快捷栏1
        /// </summary>
        public const string HOTKEY_1 = "Hotkey1";

        /// <summary>
        /// 物品快捷栏2
        /// </summary>
        public const string HOTKEY_2 = "Hotkey2";

        /// <summary>
        /// 物品快捷栏3
        /// </summary>
        public const string HOTKEY_3 = "Hotkey3";

        #endregion

        #region 系统输入

        /// <summary>
        /// 暂停
        /// </summary>
        public const string PAUSE = "Pause";

        /// <summary>
        /// 快速保存
        /// </summary>
        public const string QUICK_SAVE = "QuickSave";

        /// <summary>
        /// 快速加载
        /// </summary>
        public const string QUICK_LOAD = "QuickLoad";

        #endregion

        #region 鼠标输入

        /// <summary>
        /// 鼠标X轴
        /// </summary>
        public const string MOUSE_X = "Mouse X";

        /// <summary>
        /// 鼠标Y轴
        /// </summary>
        public const string MOUSE_Y = "Mouse Y";

        /// <summary>
        /// 鼠标滚轮
        /// </summary>
        public const string MOUSE_SCROLL = "Mouse ScrollWheel";

        #endregion
    }

    /// <summary>
    /// 输入管理器
    /// 负责处理所有玩家输入，支持键盘、鼠标和手柄
    /// 提供输入检测、组合键、手柄支持和输入重映射功能
    /// </summary>
    public class InputManager : BaseManager<InputManager>
    {
        #region 常量

        /// <summary>
        /// 输入设置保存键名
        /// </summary>
        private const string INPUT_SETTINGS_KEY = "InputManager_Settings";

        /// <summary>
        /// 鼠标灵敏度保存键名
        /// </summary>
        private const string MOUSE_SENSITIVITY_KEY = "InputManager_MouseSensitivity";

        /// <summary>
        /// 手柄灵敏度保存键名
        /// </summary>
        private const string JOYSTICK_SENSITIVITY_KEY = "InputManager_JoystickSensitivity";

        #endregion

        #region 私有字段

        /// <summary>
        /// 鼠标灵敏度
        /// </summary>
        [SerializeField]
        private float _mouseSensitivity = 2f;

        /// <summary>
        /// 手柄灵敏度
        /// </summary>
        [SerializeField]
        private float _joystickSensitivity = 2f;

        /// <summary>
        /// 是否启用鼠标
        /// </summary>
        private bool _mouseEnabled = true;

        /// <summary>
        /// 是否启用键盘
        /// </summary>
        private bool _keyboardEnabled = true;

        /// <summary>
        /// 是否启用手柄
        /// </summary>
        private bool _joystickEnabled = true;

        /// <summary>
        /// 当前输入设备类型
        /// </summary>
        private InputDeviceType _currentDevice = InputDeviceType.Keyboard;

        /// <summary>
        /// 输入设置字典
        /// </summary>
        private Dictionary<string, InputBinding> _inputBindings = new Dictionary<string, InputBinding>();

        /// <summary>
        /// 手柄连接状态
        /// </summary>
        private bool _isJoystickConnected = false;

        /// <summary>
        /// 上一次检测的手柄名称
        /// </summary>
        private string _lastJoystickName = string.Empty;

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取鼠标灵敏度
        /// </summary>
        public float MouseSensitivity
        {
            get => _mouseSensitivity;
            set
            {
                _mouseSensitivity = Mathf.Max(0.1f, value);
                SaveInputSettings();
            }
        }

        /// <summary>
        /// 获取手柄灵敏度
        /// </summary>
        public float JoystickSensitivity
        {
            get => _joystickSensitivity;
            set
            {
                _joystickSensitivity = Mathf.Max(0.1f, value);
                SaveInputSettings();
            }
        }

        /// <summary>
        /// 获取是否启用鼠标
        /// </summary>
        public bool MouseEnabled
        {
            get => _mouseEnabled;
            set => _mouseEnabled = value;
        }

        /// <summary>
        /// 获取是否启用键盘
        /// </summary>
        public bool KeyboardEnabled
        {
            get => _keyboardEnabled;
            set => _keyboardEnabled = value;
        }

        /// <summary>
        /// 获取是否启用手柄
        /// </summary>
        public bool JoystickEnabled
        {
            get => _joystickEnabled;
            set => _joystickEnabled = value;
        }

        /// <summary>
        /// 获取当前输入设备类型
        /// </summary>
        public InputDeviceType CurrentDevice => _currentDevice;

        /// <summary>
        /// 获取是否手柄已连接
        /// </summary>
        public bool IsJoystickConnected => _isJoystickConnected;

        /// <summary>
        /// 获取已连接的手柄名称
        /// </summary>
        public string JoystickName => _lastJoystickName;

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 执行初始化逻辑
        /// </summary>
        protected override void OnInitialize()
        {
            Debug.Log("[InputManager] 开始初始化...");

            // 初始化输入绑定
            InitializeInputBindings();

            // 加载输入设置
            LoadInputSettings();

            // 检测输入设备
            DetectInputDevice();

            // 设置默认输入

            Debug.Log("[InputManager] 初始化完成");
        }

        /// <summary>
        /// 执行关闭逻辑
        /// </summary>
        protected override void OnShutdown()
        {
            // 保存输入设置
            SaveInputSettings();
            Debug.Log("[InputManager] 已关闭");
        }

        #endregion

        #region 公共方法 - 输入检测

        /// <summary>
        /// 检测按键是否按下（单次）
        /// </summary>
        /// <param name="action">输入动作名称</param>
        /// <returns>是否按下</returns>
        public bool GetButtonDown(string action)
        {
            if (!_keyboardEnabled)
            {
                return false;
            }

            return Input.GetButtonDown(GetActionButton(action));
        }

        /// <summary>
        /// 检测按键是否按住（持续）
        /// </summary>
        /// <param name="action">输入动作名称</param>
        /// <returns>是否按住</returns>
        public bool GetButton(string action)
        {
            if (!_keyboardEnabled)
            {
                return false;
            }

            return Input.GetButton(GetActionButton(action));
        }

        /// <summary>
        /// 检测按键是否释放
        /// </summary>
        /// <param name="action">输入动作名称</param>
        /// <returns>是否释放</returns>
        public bool GetButtonUp(string action)
        {
            if (!_keyboardEnabled)
            {
                return false;
            }

            return Input.GetButtonUp(GetActionButton(action));
        }

        /// <summary>
        /// 获取轴输入值
        /// </summary>
        /// <param name="action">输入动作名称</param>
        /// <returns>轴值 (-1 到 1)</returns>
        public float GetAxis(string action)
        {
            if (!_keyboardEnabled && !_joystickEnabled)
            {
                return 0f;
            }

            return Input.GetAxis(GetActionAxis(action));
        }

        /// <summary>
        /// 获取轴输入值（无平滑）
        /// </summary>
        /// <param name="action">输入动作名称</param>
        /// <returns>轴值 (-1 到 1)</returns>
        public float GetAxisRaw(string action)
        {
            if (!_keyboardEnabled && !_joystickEnabled)
            {
                return 0f;
            }

            return Input.GetAxisRaw(GetActionAxis(action));
        }

        #endregion

        #region 公共方法 - 鼠标输入

        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        /// <returns>鼠标屏幕位置</returns>
        public Vector3 GetMousePosition()
        {
            if (!_mouseEnabled)
            {
                return Vector3.zero;
            }

            return Input.mousePosition;
        }

        /// <summary>
        /// 获取鼠标位置（转换为世界坐标）
        /// </summary>
        /// <param name="camera">相机</param>
        /// <param name="z">Z深度</param>
        /// <returns>鼠标世界坐标</returns>
        public Vector3 GetMouseWorldPosition(Camera camera = null, float z = 0f)
        {
            if (!_mouseEnabled)
            {
                return Vector3.zero;
            }

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = z;

            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera != null)
            {
                return camera.ScreenToWorldPoint(mousePos);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 获取鼠标移动增量
        /// </summary>
        /// <returns>鼠标移动向量</returns>
        public Vector3 GetMouseDelta()
        {
            if (!_mouseEnabled)
            {
                return Vector3.zero;
            }

            return new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f);
        }

        /// <summary>
        /// 获取鼠标滚轮滚动值
        /// </summary>
        /// <returns>滚轮值</returns>
        public float GetMouseScroll()
        {
            if (!_mouseEnabled)
            {
                return 0f;
            }

            return Input.GetAxis("Mouse ScrollWheel");
        }

        /// <summary>
        /// 检测鼠标按键是否按下
        /// </summary>
        /// <param name="button">按键编号 (0=左键, 1=右键, 2=中键)</param>
        /// <returns>是否按下</returns>
        public bool GetMouseButtonDown(int button)
        {
            if (!_mouseEnabled)
            {
                return false;
            }

            return Input.GetMouseButtonDown(button);
        }

        /// <summary>
        /// 检测鼠标按键是否按住
        /// </summary>
        /// <param name="button">按键编号 (0=左键, 1=右键, 2=中键)</param>
        /// <returns>是否按住</returns>
        public bool GetMouseButton(int button)
        {
            if (!_mouseEnabled)
            {
                return false;
            }

            return Input.GetMouseButton(button);
        }

        /// <summary>
        /// 检测鼠标按键是否释放
        /// </summary>
        /// <param name="button">按键编号 (0=左键, 1=右键, 2=中键)</param>
        /// <returns>是否释放</returns>
        public bool GetMouseButtonUp(int button)
        {
            if (!_mouseEnabled)
            {
                return false;
            }

            return Input.GetMouseButtonUp(button);
        }

        #endregion

        #region 公共方法 - 键盘输入

        /// <summary>
        /// 检测按键是否按下
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns>是否按下</returns>
        public bool GetKeyDown(KeyCode key)
        {
            if (!_keyboardEnabled)
            {
                return false;
            }

            return Input.GetKeyDown(key);
        }

        /// <summary>
        /// 检测按键是否按住
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns>是否按住</returns>
        public bool GetKey(KeyCode key)
        {
            if (!_keyboardEnabled)
            {
                return false;
            }

            return Input.GetKey(key);
        }

        /// <summary>
        /// 检测按键是否释放
        /// </summary>
        /// <param name="key">按键</param>
        /// <returns>是否释放</returns>
        public bool GetKeyUp(KeyCode key)
        {
            if (!_keyboardEnabled)
            {
                return false;
            }

            return Input.GetKeyUp(key);
        }

        #endregion

        #region 公共方法 - 手柄输入

        /// <summary>
        /// 检测手柄按键是否按下
        /// </summary>
        /// <param name="button">按键名称</param>
        /// <param name="playerIndex">手柄索引</param>
        /// <returns>是否按下</returns>
        public bool GetJoystickButtonDown(string button, int playerIndex = 0)
        {
            if (!_joystickEnabled || !_isJoystickConnected)
            {
                return false;
            }

            return Input.GetButtonDown($"{GetJoystickPrefix(playerIndex)}{button}");
        }

        /// <summary>
        /// 检测手柄按键是否按住
        /// </summary>
        /// <param name="button">按键名称</param>
        /// <param name="playerIndex">手柄索引</param>
        /// <returns>是否按住</returns>
        public bool GetJoystickButton(string button, int playerIndex = 0)
        {
            if (!_joystickEnabled || !_isJoystickConnected)
            {
                return false;
            }

            return Input.GetButton($"{GetJoystickPrefix(playerIndex)}{button}");
        }

        /// <summary>
        /// 获取手柄轴值
        /// </summary>
        /// <param name="axisName">轴名称</param>
        /// <param name="playerIndex">手柄索引</param>
        /// <returns>轴值 (-1 到 1)</returns>
        public float GetJoystickAxis(string axisName, int playerIndex = 0)
        {
            if (!_joystickEnabled || !_isJoystickConnected)
            {
                return 0f;
            }

            return Input.GetAxis($"{GetJoystickPrefix(playerIndex)}{axisName}");
        }

        /// <summary>
        /// 获取手柄轴值（无平滑）
        /// </summary>
        /// <param name="axisName">轴名称</param>
        /// <param name="playerIndex">手柄索引</param>
        /// <returns>轴值 (-1 到 1)</returns>
        public float GetJoystickAxisRaw(string axisName, int playerIndex = 0)
        {
            if (!_joystickEnabled || !_isJoystickConnected)
            {
                return 0f;
            }

            return Input.GetAxisRaw($"{GetJoystickPrefix(playerIndex)}{axisName}");
        }

        #endregion

        #region 公共方法 - 组合键

        /// <summary>
        /// 检测组合键是否按下
        /// </summary>
        /// <param name="primaryAction">主按键动作</param>
        /// <param name="modifierKey">修饰键</param>
        /// <returns>是否按下</returns>
        public bool GetKeyCombo(string primaryAction, KeyCode modifierKey)
        {
            if (!_keyboardEnabled)
            {
                return false;
            }

            return GetButtonDown(primaryAction) && GetKey(modifierKey);
        }

        /// <summary>
        /// 检测Alt+组合键
        /// </summary>
        /// <param name="action">动作名称</param>
        /// <returns>是否按下</returns>
        public bool GetAltCombo(string action)
        {
            return GetKeyCombo(action, KeyCode.LeftAlt) || GetKeyCombo(action, KeyCode.RightAlt);
        }

        /// <summary>
        /// 检测Ctrl+组合键
        /// </summary>
        /// <param name="action">动作名称</param>
        /// <returns>是否按下</returns>
        public bool GetCtrlCombo(string action)
        {
            return GetKeyCombo(action, KeyCode.LeftControl) || GetKeyCombo(action, KeyCode.RightControl);
        }

        /// <summary>
        /// 检测Shift+组合键
        /// </summary>
        /// <param name="action">动作名称</param>
        /// <returns>是否按下</returns>
        public bool GetShiftCombo(string action)
        {
            return GetKeyCombo(action, KeyCode.LeftShift) || GetKeyCombo(action, KeyCode.RightShift);
        }

        #endregion

        #region 公共方法 - 输入重映射

        /// <summary>
        /// 重映射输入动作
        /// </summary>
        /// <param name="action">动作名称</param>
        /// <param name="newButton">新按键名称</param>
        public void RebindInput(string action, string newButton)
        {
            if (_inputBindings.ContainsKey(action))
            {
                _inputBindings[action].ButtonName = newButton;
                Debug.Log($"[InputManager] 重映射输入: {action} -> {newButton}");
            }
            else
            {
                _inputBindings.Add(action, new InputBinding { ActionName = action, ButtonName = newButton });
                Debug.Log($"[InputManager] 添加输入绑定: {action} -> {newButton}");
            }

            SaveInputSettings();
        }

        /// <summary>
        /// 重置输入设置为默认值
        /// </summary>
        public void ResetInputSettings()
        {
            _mouseSensitivity = 2f;
            _joystickSensitivity = 2f;
            _inputBindings.Clear();
            InitializeInputBindings();
            SaveInputSettings();
            Debug.Log("[InputManager] 输入设置已重置");
        }

        #endregion

        #region 公共方法 - 输入设备

        /// <summary>
        /// 检测当前输入设备
        /// </summary>
        public void DetectInputDevice()
        {
            // 检测手柄
            string[] joysticks = Input.GetJoystickNames();
            bool wasJoystickConnected = _isJoystickConnected;
            _isJoystickConnected = joysticks.Length > 0 && !string.IsNullOrEmpty(joysticks[0]);

            if (_isJoystickConnected)
            {
                _lastJoystickName = joysticks[0];
                if (!wasJoystickConnected)
                {
                    Debug.Log($"[InputManager] 手柄已连接: {_lastJoystickName}");
                }
                _currentDevice = InputDeviceType.Joystick;
            }
            else
            {
                _lastJoystickName = string.Empty;
                if (wasJoystickConnected)
                {
                    Debug.Log("[InputManager] 手柄已断开");
                }

                // 检测鼠标/键盘活动
                if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                {
                    _currentDevice = InputDeviceType.Keyboard;
                }
                else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    _currentDevice = InputDeviceType.Mouse;
                }
            }

            if (CoreConfig.ENABLE_DEBUG_MODE)
            {
                Debug.Log($"[InputManager] 当前输入设备: {_currentDevice}");
            }
        }

        /// <summary>
        /// 获取当前设备类型名称
        /// </summary>
        /// <returns>设备类型名称</returns>
        public string GetDeviceName()
        {
            switch (_currentDevice)
            {
                case InputDeviceType.Keyboard:
                    return "键盘";

                case InputDeviceType.Mouse:
                    return "鼠标";

                case InputDeviceType.Joystick:
                    return $"手柄 ({_lastJoystickName})";

                default:
                    return "未知";
            }
        }

        #endregion

        #region 设置保存与加载

        /// <summary>
        /// 保存输入设置
        /// </summary>
        public void SaveInputSettings()
        {
            PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, _mouseSensitivity);
            PlayerPrefs.SetFloat(JOYSTICK_SENSITIVITY_KEY, _joystickSensitivity);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载输入设置
        /// </summary>
        public void LoadInputSettings()
        {
            if (PlayerPrefs.HasKey(MOUSE_SENSITIVITY_KEY))
            {
                _mouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY);
            }

            if (PlayerPrefs.HasKey(JOYSTICK_SENSITIVITY_KEY))
            {
                _joystickSensitivity = PlayerPrefs.GetFloat(JOYSTICK_SENSITIVITY_KEY);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化输入绑定
        /// </summary>
        private void InitializeInputBindings()
        {
            _inputBindings.Clear();

            // 移动
            AddBinding(InputActions.HORIZONTAL, "Horizontal");
            AddBinding(InputActions.VERTICAL, "Vertical");
            AddBinding(InputActions.SPRINT, "LeftShift");
            AddBinding(InputActions.JUMP, "Space");
            AddBinding(InputActions.CROUCH, "LeftControl");

            // 交互
            AddBinding(InputActions.INTERACT, "e");
            AddBinding(InputActions.CANCEL, "Escape");

            // 战斗
            AddBinding(InputActions.ATTACK, "Mouse0");
            AddBinding(InputActions.DODGE, "Space");
            AddBinding(InputActions.SPECIAL, "q");
            AddBinding(InputActions.BLOCK, "Mouse1");

            // UI
            AddBinding(InputActions.MENU, "Escape");
            AddBinding(InputActions.INVENTORY, "i");
            AddBinding(InputActions.MAP, "m");
            AddBinding(InputActions.HOTKEY_1, "1");
            AddBinding(InputActions.HOTKEY_2, "2");
            AddBinding(InputActions.HOTKEY_3, "3");

            // 系统
            AddBinding(InputActions.PAUSE, "Escape");
            AddBinding(InputActions.QUICK_SAVE, "f5");
            AddBinding(InputActions.QUICK_LOAD, "f9");
        }

        /// <summary>
        /// 添加输入绑定
        /// </summary>
        private void AddBinding(string actionName, string buttonName)
        {
            if (!_inputBindings.ContainsKey(actionName))
            {
                _inputBindings.Add(actionName, new InputBinding
                {
                    ActionName = actionName,
                    ButtonName = buttonName
                });
            }
        }

        /// <summary>
        /// 获取动作对应的按钮名称
        /// </summary>
        private string GetActionButton(string action)
        {
            if (_inputBindings.TryGetValue(action, out InputBinding binding))
            {
                return binding.ButtonName;
            }
            return action;
        }

        /// <summary>
        /// 获取动作对应的轴名称
        /// </summary>
        private string GetActionAxis(string action)
        {
            if (_inputBindings.TryGetValue(action, out InputBinding binding))
            {
                return binding.ButtonName;
            }
            return action;
        }

        /// <summary>
        /// 获取手柄前缀
        /// </summary>
        private string GetJoystickPrefix(int playerIndex)
        {
            return $"Joy{playerIndex + 1} ";
        }

        #endregion
    }

    /// <summary>
    /// 输入设备类型
    /// </summary>
    public enum InputDeviceType
    {
        /// <summary>
        /// 键盘
        /// </summary>
        Keyboard,

        /// <summary>
        /// 鼠标
        /// </summary>
        Mouse,

        /// <summary>
        /// 手柄
        /// </summary>
        Joystick
    }

    /// <summary>
    /// 输入绑定类
    /// </summary>
    [Serializable]
    public class InputBinding
    {
        /// <summary>
        /// 动作名称
        /// </summary>
        public string ActionName;

        /// <summary>
        /// 按钮名称
        /// </summary>
        public string ButtonName;
    }
}
