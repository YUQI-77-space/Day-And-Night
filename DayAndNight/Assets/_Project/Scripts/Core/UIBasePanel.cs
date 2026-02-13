using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// UI面板基类
    /// </summary>
    public abstract class UIBasePanel : MonoBehaviour
    {
        #region 保护字段

        /// <summary>
        /// 所属UI管理器
        /// </summary>
        protected UIManager _uiManager;

        /// <summary>
        /// 面板类型
        /// </summary>
        [SerializeField]
        protected UIPanelType _panelType;

        /// <summary>
        /// 是否正在显示
        /// </summary>
        protected bool _isShowing = false;

        /// <summary>
        /// 是否使用动画
        /// </summary>
        protected bool _useAnimation = true;

        /// <summary>
        /// 显示动画时长（秒）
        /// </summary>
        [SerializeField]
        protected float _showAnimationDuration = 0.3f;

        /// <summary>
        /// 隐藏动画时长（秒）
        /// </summary>
        [SerializeField]
        protected float _hideAnimationDuration = 0.3f;

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取面板类型
        /// </summary>
        public UIPanelType PanelType => _panelType;

        /// <summary>
        /// 获取是否正在显示
        /// </summary>
        public bool IsShowing => _isShowing;

        #endregion

        #region 虚方法

        /// <summary>
        /// 初始化面板
        /// </summary>
        /// <param name="uiManager">UI管理器</param>
        public virtual void Initialize(UIManager uiManager)
        {
            _uiManager = uiManager;
            OnInitialize();
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="animate">是否使用动画</param>
        public virtual void Show(bool animate = true)
        {
            _useAnimation = animate;
            _isShowing = true;
            gameObject.SetActive(true);
            OnShow();
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="animate">是否使用动画</param>
        public virtual void Hide(bool animate = true)
        {
            _useAnimation = animate;
            _isShowing = false;
            OnHide();
        }

        /// <summary>
        /// 设置模态状态
        /// </summary>
        /// <param name="isModal">是否为模态</param>
        public virtual void SetModalState(bool isModal)
        {
            // 子类可以重写此方法实现模态效果
        }

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 初始化回调
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// 显示回调
        /// </summary>
        protected virtual void OnShow()
        {
        }

        /// <summary>
        /// 隐藏回调
        /// </summary>
        protected virtual void OnHide()
        {
        }

        /// <summary>
        /// 刷新面板数据
        /// </summary>
        public virtual void RefreshData()
        {
        }

        #endregion
    }
}
