using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using SpellStrike.Data;
using SpellStrike.Combat;
using SpellStrike.Core.StateMachine;

namespace SpellStrike.Enemy
{
    /// <summary>
    /// Hub trung tâm cho mọi loại Enemy. Cung cấp FSM và các components cơ bản.
    /// Kế thừa MonoBehaviour cục bộ, chi tiết logic nằm trong các State.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyBase : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] protected EnemyDataSO m_Data;
        
        [Header("Components (Auto-fetched)")]
        [SerializeField] protected HealthComponent m_Health;
        [SerializeField] protected StatusEffectController m_StatusEffect;
        [SerializeField] protected Animator m_Animator;

        #endregion

        #region Protected Fields

        protected StateMachine m_StateMachine;
        protected NavMeshAgent m_NavAgent;
        [Header("Manual Target (Optional)")]
        [SerializeField] protected Transform m_PlayerTarget;
        protected CancellationTokenSource m_LifetimeCts;

        #endregion

        #region Public Properties

        public EnemyDataSO Data => m_Data;
        public NavMeshAgent NavAgent => m_NavAgent;
        public Animator Animator => m_Animator;
        public HealthComponent Health => m_Health;
        public StatusEffectController StatusEffect => m_StatusEffect;
        public Transform PlayerTarget
        {
            get
            {
                if (m_PlayerTarget == null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                        m_PlayerTarget = player.transform;
                }
                return m_PlayerTarget;
            }
        }

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            m_NavAgent = GetComponent<NavMeshAgent>();
            m_Health = GetComponentInChildren<HealthComponent>();
            m_StatusEffect = GetComponentInChildren<StatusEffectController>();
            m_Animator = GetComponentInChildren<Animator>();

            // Setup Data initial stats
            if (m_Data != null)
            {
                m_NavAgent.speed = m_Data.Speed;
                // Thiết lập stoppingDistance bằng tầm đánh để NavAgent tự dừng đúng chỗ mượt mà
                m_NavAgent.stoppingDistance = m_Data.AttackRange * 0.9f; 
                m_NavAgent.acceleration = 20f; // Tăng gia tốc để quái linh hoạt hơn
                
                if (m_Health != null) m_Health.Initialize(m_Data.HP);
            }
        }

        protected virtual void OnEnable()
        {
            if (m_LifetimeCts != null)
            {
                m_LifetimeCts.Cancel();
                m_LifetimeCts.Dispose();
            }
            m_LifetimeCts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            if (m_LifetimeCts != null)
            {
                m_LifetimeCts.Cancel();
                m_LifetimeCts.Dispose();
                m_LifetimeCts = null;
            }
        }

        protected virtual void Start()
        {
            SetupStateMachine();
        }

        protected virtual void Update()
        {
            // Áp dụng StatusEffect Speed (Slow)
            if (m_Data != null && m_StatusEffect != null)
            {
                m_NavAgent.speed = m_Data.Speed * m_StatusEffect.SpeedMultiplier;
            }

            m_StateMachine?.Tick();
        }

        protected virtual void FixedUpdate()
        {
            m_StateMachine?.FixedTick();
        }

        #endregion

        #region FSM Configuration

        /// <summary>
        /// Ghi đè ở các class con (Melee / Ranged) để thiết lập Transition behavior riêng.
        /// </summary>
        protected abstract void SetupStateMachine();

        #endregion

        #region Helper Methods
        
        public bool IsPlayerInRange(float _range)
        {
            if (m_PlayerTarget == null) return false;
            float sqrDistance = (m_PlayerTarget.position - transform.position).sqrMagnitude;
            return sqrDistance <= _range * _range;
        }

        public void Die()
        {
            Debug.Log($"[EnemyBase] {gameObject.name} is dying. Disabling colliders and NavMesh.");

            // 1. Tắt toàn bộ Collider ở cha và các con
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var c in colliders) c.enabled = false;

            // 2. Tắt CharacterController nếu có
            if (TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;

            // 3. Khóa Rigidbody để quái không bị rơi xuyên đất sau khi tắt Collider
            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true; 
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
            }

            // 4. Vô hiệu hóa NavMeshAgent để không cản đường AI khác
            if (m_NavAgent != null)
            {
                if (m_NavAgent.isOnNavMesh) m_NavAgent.isStopped = true;
                m_NavAgent.enabled = false;
            }
            
            // Trả quái về Pool, drop loot
            Combat.LootDropperService.Instance?.TryDropEnemyLoot(transform.position, m_Data.DropTable);

            // Bật anim Die nếu có
            if (m_Animator != null) m_Animator.SetTrigger("Die");
            
            // Thực hiện đếm ngược biến mất
            WaitAndReturnPoolAsync().Forget();
        }

        private async UniTaskVoid WaitAndReturnPoolAsync()
        {
            float delay = m_Data != null ? m_Data.DeathDestroyDelay : 1.5f;
            Debug.Log($"[EnemyBase] {gameObject.name} will disappear in {delay} seconds.");

            if (delay <= 0f)
            {
                DestroyEnemyInstance();
                return;
            }

            // Dùng token của chính object này thay vì lifetime token nếu muốn chắc chắn biến mất 
            // kể cả khi bị disable (trừ khi object bị xóa hoàn toàn trước đó)
            bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            
            if (!isCanceled)
            {
                DestroyEnemyInstance();
            }
        }

        private void DestroyEnemyInstance()
        {
            Debug.Log($"[EnemyBase] Destroying {gameObject.name} now.");
            // Tạm dùng Destroy, sau này có Pool thì gọi ReturnPool ở đây
            Destroy(gameObject);
        }

        #endregion
    }
}
