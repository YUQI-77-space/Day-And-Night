using UnityEngine;

namespace DayAndNight.Gameplay
{
    /// <summary>
    /// 玩家方向
    /// </summary>
    public enum PlayerDirection
    {
        Down = 0,   // 向下（默认朝向）
        Up = 1,     // 向上
        Left = 2,   // 向左
        Right = 3   // 向右
    }

    /// <summary>
    /// 玩家状态
    /// </summary>
    public enum PlayerState
    {
        Idle,       // 待机
        Moving,     // 移动
        Attacking,  // 攻击中
        Dodging,    // 闪避中
        Dead        // 死亡
    }

    /// <summary>
    /// 玩家控制器
    /// 处理玩家移动、动画和基础战斗功能
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        #region 序列化字段

        [Header("玩家属性")]
        [SerializeField]
        private PlayerStats _stats = new PlayerStats();

        [Header("移动设置")]
        [SerializeField]
        private bool _faceMovementDirection = true;

        [Header("动画设置 - Idle")]
        [SerializeField]
        private Sprite[] _idleDownSprites = null;
        [SerializeField]
        private Sprite[] _idleUpSprites = null;
        [SerializeField]
        private Sprite[] _idleLeftSprites = null;
        [SerializeField]
        private Sprite[] _idleRightSprites = null;

        [Header("动画设置 - 移动")]
        [SerializeField]
        private Sprite[] _moveDownSprites = null;
        [SerializeField]
        private Sprite[] _moveUpSprites = null;
        [SerializeField]
        private Sprite[] _moveLeftSprites = null;
        [SerializeField]
        private Sprite[] _moveRightSprites = null;

        [Header("动画速度")]
        [SerializeField]
        private float _idleFrameRate = 4f;
        [SerializeField]
        private float _moveFrameRate = 8f;

        #endregion

        #region 私有字段

        // 组件引用
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody2D;

        // 状态管理
        private PlayerState _currentState = PlayerState.Idle;
        private PlayerDirection _currentDirection = PlayerDirection.Down;
        private Vector2 _moveInput;
        private Vector2 _lastMoveDirection;

        // 动画相关
        private float _animationTimer;
        private int _currentFrameIndex;
        private Sprite[] _currentAnimationSprites;

        // 冷却计时器
        private float _attackCooldownTimer;
        private float _dodgeCooldownTimer;
        private float _staminaRegenTimer;

        // 闪避相关
        private Vector2 _dodgeDirection;
        private float _dodgeTimer;
        private bool _isInvincible;

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取玩家属性
        /// </summary>
        public PlayerStats Stats => _stats;

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public PlayerState CurrentState => _currentState;

        /// <summary>
        /// 获取当前方向
        /// </summary>
        public PlayerDirection CurrentDirection => _currentDirection;

        /// <summary>
        /// 获取是否处于无敌状态
        /// </summary>
        public bool IsInvincible => _isInvincible;

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rigidbody2D = GetComponent<Rigidbody2D>();

            // 确保Rigidbody2D设置正确
            _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;  // 确保是动态刚体
            _rigidbody2D.gravityScale = 0f;
            _rigidbody2D.freezeRotation = true;
            _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void Start()
        {
            // 初始化玩家属性
            _stats.Initialize();

            // 设置初始状态
            _currentState = PlayerState.Idle;
            _currentDirection = PlayerDirection.Down;
            _lastMoveDirection = Vector2.down;

            // 播放初始动画
            PlayAnimation(GetIdleSprites(_currentDirection));

            Debug.Log("[PlayerController] 玩家初始化完成");
        }

        private void Update()
        {
            if (_stats.IsDead)
            {
                return;
            }

            // 更新冷却
            UpdateCooldowns();

            // 处理输入
            ProcessInput();

            // 更新状态
            UpdateState();

            // 更新动画
            UpdateAnimation();
        }

        private void FixedUpdate()
        {
            if (_stats.IsDead)
            {
                _rigidbody2D.velocity = Vector2.zero;
                return;
            }

            // 处理移动
            ProcessMovement();
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 处理输入
        /// </summary>
        private void ProcessInput()
        {
            // 获取移动输入
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector2(horizontal, vertical).normalized;

            // 攻击输入
            if (Input.GetMouseButtonDown(0) && _currentState != PlayerState.Attacking)
            {
                TryAttack();
            }

            // 闪避输入
            if (Input.GetKeyDown(KeyCode.Space) && _currentState != PlayerState.Dodging)
            {
                TryDodge();
            }
        }

        #endregion

        #region 状态管理

        /// <summary>
        /// 更新冷却计时器
        /// </summary>
        private void UpdateCooldowns()
        {
            // 攻击冷却
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }

            // 闪避冷却
            if (_dodgeCooldownTimer > 0)
            {
                _dodgeCooldownTimer -= Time.deltaTime;
            }

            // 闪避状态计时
            if (_currentState == PlayerState.Dodging)
            {
                _dodgeTimer -= Time.deltaTime;
                if (_dodgeTimer <= 0)
                {
                    SetState(PlayerState.Idle);
                    _isInvincible = false;
                }
            }

            // 耐力恢复
            UpdateStaminaRegeneration();
        }

        /// <summary>
        /// 更新耐力恢复
        /// </summary>
        private void UpdateStaminaRegeneration()
        {
            // 如果正在消耗耐力，重置恢复计时器
            if (_moveInput.magnitude > 0.1f || _currentState == PlayerState.Attacking)
            {
                _staminaRegenTimer = _stats.StaminaRegenDelay;
            }
            else
            {
                _staminaRegenTimer -= Time.deltaTime;
                if (_staminaRegenTimer <= 0 && _stats.CurrentStamina < _stats.MaxStamina)
                {
                    _stats.RestoreStamina(_stats.StaminaRegenRate * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// 更新玩家状态
        /// </summary>
        private void UpdateState()
        {
            if (_currentState == PlayerState.Attacking || _currentState == PlayerState.Dodging)
            {
                return;
            }

            // 记录之前的方向
            PlayerDirection previousDirection = _currentDirection;

            // 更新移动方向
            if (_moveInput.magnitude > 0.1f)
            {
                _lastMoveDirection = _moveInput;

                // 根据移动方向确定动画方向
                UpdateFacingDirection(_moveInput);

                if (_currentState != PlayerState.Moving)
                {
                    SetState(PlayerState.Moving);
                }
                // 方向变化时立即更新动画（中断当前动画）
                else if (_currentDirection != previousDirection)
                {
                    _currentFrameIndex = 0;
                    _animationTimer = 0f;
                    PlayAnimation(GetMoveSprites(_currentDirection));
                }
            }
            else
            {
                if (_currentState != PlayerState.Idle)
                {
                    SetState(PlayerState.Idle);
                }
                // 方向变化时立即更新动画（中断当前动画）
                else if (_currentDirection != previousDirection)
                {
                    _currentFrameIndex = 0;
                    _animationTimer = 0f;
                    PlayAnimation(GetIdleSprites(_currentDirection));
                }
            }
        }

        /// <summary>
        /// 根据移动方向更新朝向
        /// </summary>
        private void UpdateFacingDirection(Vector2 direction)
        {
            // 确定主要方向（4向）
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // 水平方向
                _currentDirection = direction.x > 0 ? PlayerDirection.Right : PlayerDirection.Left;
            }
            else
            {
                // 垂直方向
                _currentDirection = direction.y > 0 ? PlayerDirection.Up : PlayerDirection.Down;
            }
        }

        /// <summary>
        /// 设置玩家状态
        /// </summary>
        private void SetState(PlayerState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;

            // 状态改变时重置动画帧
            _currentFrameIndex = 0;
            _animationTimer = 0f;

            // 根据新状态播放对应动画
            switch (newState)
            {
                case PlayerState.Idle:
                    PlayAnimation(GetIdleSprites(_currentDirection));
                    break;
                case PlayerState.Moving:
                    PlayAnimation(GetMoveSprites(_currentDirection));
                    break;
                case PlayerState.Attacking:
                    // 攻击动画逻辑后续添加
                    break;
                case PlayerState.Dodging:
                    // 闪避动画逻辑后续添加
                    break;
                case PlayerState.Dead:
                    Debug.Log("[PlayerController] 玩家死亡！");
                    break;
            }
        }

        #endregion

        #region 移动处理

        /// <summary>
        /// 处理移动逻辑
        /// </summary>
        private void ProcessMovement()
        {
            if (_currentState == PlayerState.Dodging)
            {
                // 闪避时按固定方向移动
                _rigidbody2D.velocity = _dodgeDirection * _stats.MoveSpeed * 2f;
            }
            else if (_currentState != PlayerState.Attacking)
            {
                // 正常移动
                _rigidbody2D.velocity = _moveInput * _stats.MoveSpeed;
            }
            else
            {
                // 攻击时停止移动
                _rigidbody2D.velocity = Vector2.zero;
            }

            // 根据移动方向翻转Sprite
            if (_faceMovementDirection && _moveInput.x != 0)
            {
                _spriteRenderer.flipX = _moveInput.x < 0;
            }
        }

        #endregion

        #region 战斗功能

        /// <summary>
        /// 尝试攻击
        /// </summary>
        private void TryAttack()
        {
            if (_attackCooldownTimer > 0)
            {
                return;
            }

            if (!_stats.ConsumeStamina(_stats.AttackStaminaCost))
            {
                Debug.Log("[PlayerController] 耐力不足，无法攻击！");
                return;
            }

            _attackCooldownTimer = _stats.AttackCooldown;
            SetState(PlayerState.Attacking);

            Debug.Log($"[PlayerController] 发动攻击！造成 {_stats.BaseAttackDamage} 点伤害");

            // 攻击完成后回到Idle
            Invoke(nameof(ReturnToIdleAfterAttack), _stats.AttackCooldown);
        }

        /// <summary>
        /// 攻击完成后返回Idle
        /// </summary>
        private void ReturnToIdleAfterAttack()
        {
            if (_currentState == PlayerState.Attacking)
            {
                SetState(_moveInput.magnitude > 0.1f ? PlayerState.Moving : PlayerState.Idle);
            }
        }

        /// <summary>
        /// 尝试闪避
        /// </summary>
        private void TryDodge()
        {
            if (_dodgeCooldownTimer > 0)
            {
                return;
            }

            if (!_stats.ConsumeStamina(_stats.DodgeStaminaCost))
            {
                Debug.Log("[PlayerController] 耐力不足，无法闪避！");
                return;
            }

            if (_moveInput.magnitude < 0.1f && _lastMoveDirection.magnitude < 0.1f)
            {
                Debug.Log("[PlayerController] 没有移动方向，无法闪避！");
                return;
            }

            _dodgeCooldownTimer = _stats.DodgeCooldown;
            _dodgeTimer = _stats.DodgeInvincibleTime;
            _dodgeDirection = _lastMoveDirection.normalized;
            _isInvincible = true;

            SetState(PlayerState.Dodging);

            Debug.Log("[PlayerController] 闪避！");
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        public void TakeDamage(float damage)
        {
            if (_isInvincible || _stats.IsDead)
            {
                return;
            }

            float actualDamage = _stats.TakeDamage(damage);
            Debug.Log($"[PlayerController] 受到 {actualDamage} 点伤害，剩余HP: {_stats.CurrentHealth}");

            if (_stats.IsDead)
            {
                OnDeath();
            }
        }

        /// <summary>
        /// 死亡处理
        /// </summary>
        private void OnDeath()
        {
            SetState(PlayerState.Dead);
            _rigidbody2D.velocity = Vector2.zero;
            Debug.Log("[PlayerController] 玩家已死亡！");
        }

        #endregion

        #region 动画系统

        /// <summary>
        /// 更新动画
        /// </summary>
        private void UpdateAnimation()
        {
            if (_currentAnimationSprites == null || _currentAnimationSprites.Length == 0)
            {
                return;
            }

            // 确定帧率
            float frameRate = _currentState == PlayerState.Idle ? _idleFrameRate : _moveFrameRate;
            float frameDuration = 1f / frameRate;

            // 更新动画计时
            _animationTimer += Time.deltaTime;

            if (_animationTimer >= frameDuration)
            {
                _animationTimer = 0f;
                _currentFrameIndex = (_currentFrameIndex + 1) % _currentAnimationSprites.Length;

                // 更新Sprite
                if (_currentAnimationSprites[_currentFrameIndex] != null)
                {
                    _spriteRenderer.sprite = _currentAnimationSprites[_currentFrameIndex];
                }
            }
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        private void PlayAnimation(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0)
            {
                _currentAnimationSprites = null;
                return;
            }

            _currentAnimationSprites = sprites;
            _currentFrameIndex = 0;
            _animationTimer = 0f;

            // 立即显示第一帧
            if (sprites[0] != null)
            {
                _spriteRenderer.sprite = sprites[0];
            }
        }

        /// <summary>
        /// 获取Idle动画Sprite数组
        /// </summary>
        private Sprite[] GetIdleSprites(PlayerDirection direction)
        {
            return direction switch
            {
                PlayerDirection.Down => _idleDownSprites,
                PlayerDirection.Up => _idleUpSprites,
                PlayerDirection.Left => _idleLeftSprites,
                PlayerDirection.Right => _idleRightSprites,
                _ => _idleDownSprites
            };
        }

        /// <summary>
        /// 获取移动动画Sprite数组
        /// </summary>
        private Sprite[] GetMoveSprites(PlayerDirection direction)
        {
            return direction switch
            {
                PlayerDirection.Down => _moveDownSprites,
                PlayerDirection.Up => _moveUpSprites,
                PlayerDirection.Left => _moveLeftSprites,
                PlayerDirection.Right => _moveRightSprites,
                _ => _moveDownSprites
            };
        }

        #endregion

        #region 调试工具

        /// <summary>
        /// 在编辑器中绘制调试信息
        /// </summary>
        private void OnGUI()
        {
            if (!DayAndNight.Core.CoreConfig.ENABLE_DEBUG_MODE)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"状态: {_currentState}");
            GUILayout.Label($"方向: {_currentDirection}");
            GUILayout.Label($"HP: {_stats.CurrentHealth}/{_stats.MaxHealth}");
            GUILayout.Label($"耐力: {_stats.CurrentStamina:F1}/{_stats.MaxStamina}");
            GUILayout.Label($"移动输入: {_moveInput}");
            GUILayout.EndArea();
        }

        #endregion
    }
}
