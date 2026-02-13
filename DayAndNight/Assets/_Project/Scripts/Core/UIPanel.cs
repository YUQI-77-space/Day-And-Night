using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight.Core
{
    /// <summary>
    /// UI面板组件，继承自UIBasePanel
    /// 提供基础的显示/隐藏功能
    /// </summary>
    public class UIPanel : UIBasePanel
    {
        #region Unity生命周期方法

        /// <summary>
        /// 唤醒时初始化
        /// </summary>
        private void Awake()
        {
            // 如果没有设置面板类型，使用游戏物体名称
            if (_panelType == UIPanelType.None)
            {
                _panelType = (UIPanelType)System.Enum.Parse(typeof(UIPanelType), gameObject.name);
            }
        }

        #endregion

        #region 保护虚方法

        /// <summary>
        /// 显示面板
        /// </summary>
        protected override void OnShow()
        {
            if (_useAnimation)
            {
                StartCoroutine(ShowAnimation());
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        protected override void OnHide()
        {
            if (_useAnimation)
            {
                StartCoroutine(HideAnimation());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 设置模态状态
        /// </summary>
        public override void SetModalState(bool isModal)
        {
            // 添加半透明背景
            // 子类可以实现更复杂的效果
        }

        #endregion

        #region 协程方法

        /// <summary>
        /// 显示动画协程
        /// </summary>
        private IEnumerator ShowAnimation()
        {
            gameObject.SetActive(true);

            // 获取RectTransform
            RectTransform rectTransform = GetComponent<RectTransform>();

            // 记录初始位置和缩放
            Vector3 initialPosition = rectTransform.anchoredPosition;
            Vector3 initialScale = rectTransform.localScale;

            // 设置初始状态（缩小并淡出）
            rectTransform.localScale = Vector3.zero;

            float elapsed = 0f;

            while (elapsed < _showAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _showAnimationDuration;

                // 使用平滑曲线
                t = Mathf.SmoothStep(0f, 1f, t);

                rectTransform.localScale = Vector3.Lerp(Vector3.zero, initialScale, t);

                yield return null;
            }

            rectTransform.localScale = initialScale;
        }

        /// <summary>
        /// 隐藏动画协程
        /// </summary>
        private IEnumerator HideAnimation()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector3 initialScale = rectTransform.localScale;

            float elapsed = 0f;

            while (elapsed < _hideAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _hideAnimationDuration;

                // 使用平滑曲线
                t = Mathf.SmoothStep(0f, 1f, t);

                rectTransform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

                yield return null;
            }

            rectTransform.localScale = initialScale;
            gameObject.SetActive(false);
        }

        #endregion
    }
}
