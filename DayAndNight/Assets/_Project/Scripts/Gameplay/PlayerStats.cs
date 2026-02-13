using UnityEngine;

namespace DayAndNight.Gameplay
{
    /// <summary>
    /// 玩家属性数据类
    /// 管理玩家的生命值、耐力、攻击力等基础属性
    /// </summary>
    [System.Serializable]
    public class PlayerStats
    {
        #region 基础属性

        /// <summary>
        /// 最大生命值
        /// </summary>
        [Header("基础属性")]
        [SerializeField]
        private float _maxHealth = 100f;

        /// <summary>
        /// 当前生命值
        /// </summary>
        [SerializeField]
        private float _currentHealth = 100f;

        /// <summary>
        /// 最大耐力值
        /// </summary>
        [SerializeField]
        private float _maxStamina = 50f;

        /// <summary>
        /// 当前耐力值
        /// </summary>
        [SerializeField]
        private float _currentStamina = 50f;

        /// <summary>
        /// 基础攻击力
        /// </summary>
        [SerializeField]
        private float _baseAttackDamage = 10f;

        /// <summary>
        /// 移动速度
        /// </summary>
        [SerializeField]
        private float _moveSpeed = 5f;

        #endregion

        #region 战斗属性

        /// <summary>
        /// 攻击冷却时间（秒）
        /// </summary>
        [Header("战斗属性")]
        [SerializeField]
        private float _attackCooldown = 0.5f;

        /// <summary>
        /// 闪避冷却时间（秒）
        /// </summary>
        [SerializeField]
        private float _dodgeCooldown = 1f;

        /// <summary>
        /// 闪避消耗耐力
        /// </summary>
        [SerializeField]
        private float _dodgeStaminaCost = 15f;

        /// <summary>
        /// 攻击消耗耐力
        /// </summary>
        [SerializeField]
        private float _attackStaminaCost = 10f;

        /// <summary>
        /// 闪避位移距离
        /// </summary>
        [SerializeField]
        private float _dodgeDistance = 3f;

        /// <summary>
        /// 闪避无敌时间（秒）
        /// </summary>
        [SerializeField]
        private float _dodgeInvincibleTime = 0.3f;

        #endregion

        #region 耐力恢复属性

        /// <summary>
        /// 耐力恢复速率（每秒）
        /// </summary>
        [Header("耐力恢复")]
        [SerializeField]
        private float _staminaRegenRate = 10f;

        /// <summary>
        /// 耐力恢复冷却时间（秒，停止行动后开始恢复）
        /// </summary>
        [SerializeField]
        private float _staminaRegenDelay = 1f;

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取最大生命值
        /// </summary>
        public float MaxHealth => _maxHealth;

        /// <summary>
        /// 获取当前生命值
        /// </summary>
        public float CurrentHealth
        {
            get => _currentHealth;
            set => _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
        }

        /// <summary>
        /// 获取生命值百分比（0-1）
        /// </summary>
        public float HealthPercent => _maxHealth > 0 ? _currentHealth / _maxHealth : 0;

        /// <summary>
        /// 获取是否死亡
        /// </summary>
        public bool IsDead => _currentHealth <= 0;

        /// <summary>
        /// 获取最大耐力值
        /// </summary>
        public float MaxStamina => _maxStamina;

        /// <summary>
        /// 获取当前耐力值
        /// </summary>
        public float CurrentStamina
        {
            get => _currentStamina;
            set => _currentStamina = Mathf.Clamp(value, 0, _maxStamina);
        }

        /// <summary>
        /// 获取耐力值百分比（0-1）
        /// </summary>
        public float StaminaPercent => _maxStamina > 0 ? _currentStamina / _maxStamina : 0;

        /// <summary>
        /// 获取基础攻击力
        /// </summary>
        public float BaseAttackDamage => _baseAttackDamage;

        /// <summary>
        /// 获取移动速度
        /// </summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>
        /// 获取攻击冷却时间
        /// </summary>
        public float AttackCooldown => _attackCooldown;

        /// <summary>
        /// 获取闪避冷却时间
        /// </summary>
        public float DodgeCooldown => _dodgeCooldown;

        /// <summary>
        /// 获取闪避消耗耐力
        /// </summary>
        public float DodgeStaminaCost => _dodgeStaminaCost;

        /// <summary>
        /// 获取攻击消耗耐力
        /// </summary>
        public float AttackStaminaCost => _attackStaminaCost;

        /// <summary>
        /// 获取闪避位移距离
        /// </summary>
        public float DodgeDistance => _dodgeDistance;

        /// <summary>
        /// 获取闪避无敌时间
        /// </summary>
        public float DodgeInvincibleTime => _dodgeInvincibleTime;

        /// <summary>
        /// 获取耐力恢复速率
        /// </summary>
        public float StaminaRegenRate => _staminaRegenRate;

        /// <summary>
        /// 获取耐力恢复冷却时间
        /// </summary>
        public float StaminaRegenDelay => _staminaRegenDelay;

        #endregion

        #region 构造函数

        /// <summary>
        /// 使用默认属性创建玩家属性
        /// </summary>
        public PlayerStats()
        {
        }

        /// <summary>
        /// 使用自定义属性创建玩家属性
        /// </summary>
        /// <param name="maxHealth">最大生命值</param>
        /// <param name="maxStamina">最大耐力值</param>
        /// <param name="moveSpeed">移动速度</param>
        public PlayerStats(float maxHealth, float maxStamina, float moveSpeed)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _maxStamina = maxStamina;
            _currentStamina = maxStamina;
            _moveSpeed = moveSpeed;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 造成伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <returns>实际受到的伤害</returns>
        public float TakeDamage(float damage)
        {
            if (IsDead) return 0;

            float actualDamage = Mathf.Min(damage, _currentHealth);
            _currentHealth -= actualDamage;

            if (IsDead)
            {
                _currentHealth = 0;
            }

            return actualDamage;
        }

        /// <summary>
        /// 治疗
        /// </summary>
        /// <param name="healAmount">治疗量</param>
        /// <returns>实际治疗量</returns>
        public float Heal(float healAmount)
        {
            float actualHeal = Mathf.Min(healAmount, _maxHealth - _currentHealth);
            _currentHealth += actualHeal;
            return actualHeal;
        }

        /// <summary>
        /// 消耗耐力
        /// </summary>
        /// <param name="amount">消耗量</param>
        /// <returns>是否成功消耗（耐力不足返回false）</returns>
        public bool ConsumeStamina(float amount)
        {
            if (_currentStamina >= amount)
            {
                _currentStamina -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 恢复耐力
        /// </summary>
        /// <param name="amount">恢复量</param>
        /// <returns>实际恢复量</returns>
        public float RestoreStamina(float amount)
        {
            float actualRestore = Mathf.Min(amount, _maxStamina - _currentStamina);
            _currentStamina += actualRestore;
            return actualRestore;
        }

        /// <summary>
        /// 重置属性（复活时使用）
        /// </summary>
        public void ResetStats()
        {
            _currentHealth = _maxHealth;
            _currentStamina = _maxStamina;
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        public void Initialize()
        {
            _currentHealth = _maxHealth;
            _currentStamina = _maxStamina;
        }

        #endregion
    }
}
