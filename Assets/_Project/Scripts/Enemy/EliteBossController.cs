using UnityEngine;
using System.Collections.Generic;
using SpellStrike.Data;
using SpellStrike.Enemy.States;
using SpellStrike.Core.StateMachine;
using SpellStrike.Core.EventChannels;

namespace SpellStrike.Enemy
{
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent), typeof(Combat.HealthComponent))]
    public class EliteBossController : EnemyBase
    {
        [Header("Elite Config")]
        [SerializeField] private EliteBossDataSO m_EliteData;
        [SerializeField] private HPEventChannelSO m_BossHPEventChannel;
        [SerializeField] private StringEventChannelSO m_BossNameEventChannel;

        private List<EliteSpellRuntime> m_SpellsRuntime = new List<EliteSpellRuntime>();

        protected override void Awake()
        {
            base.Awake();
            if (m_EliteData != null)
            {
                m_Health.Initialize(m_EliteData.HP);
                m_NavAgent.speed = m_EliteData.Speed;

                if (m_EliteData.SpellList != null)
                {
                    foreach (var entry in m_EliteData.SpellList)
                    {
                        m_SpellsRuntime.Add(new EliteSpellRuntime(entry));
                    }
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            
            // Broadcast Event khi HP đổi để cập nhật EliteHPBarUI
            m_Health.OnDamageTakenLocal += (amount, current) => 
            {
                m_BossHPEventChannel?.RaiseEvent(new HPData(current, m_EliteData.HP));
            };
            
            // Ban đầu RaiseEvent để hiển thị Tên và HP đầy
            m_BossHPEventChannel?.RaiseEvent(new HPData(m_Health.CurrentHP, m_EliteData.HP));
            if (m_EliteData != null)
            {
                m_BossNameEventChannel?.RaiseEvent(m_EliteData.DisplayName);
            }
        }

        protected override void Update()
        {
            base.Update();
            // Decrease cooldown timers for elite spells
            if (m_EliteData != null)
            {
                foreach (var spellRun in m_SpellsRuntime)
                {
                    if (spellRun.CurrentCooldown > 0f)
                    {
                        spellRun.CurrentCooldown -= Time.deltaTime;
                    }
                }
            }
        }

        protected override void SetupStateMachine()
        {
            m_StateMachine = new StateMachine();

            var chaseState = new EnemyChaseState(this);
            // AttackState dùng một PerformEliteAttack chung để lựa chọn skill
            var attackState = new EnemyAttackState(this, 1f, PerformEliteAttack); 
            var deadState = new EnemyDeadState(this);

            // Cấu hình linh hoạt: Luôn đuổi Player, nếu đủ gần (hoặc kĩ năng tầm xa ss) -> Đánh
            // Đơn giản FSM: nếu trong tầm tấn công cơ bản thì attack, ngoài thì chase
            
            float baseAttackRange = m_Data.AttackRange;

            m_StateMachine.AddTransition(chaseState, attackState, () => IsPlayerInRange(baseAttackRange));
            m_StateMachine.AddTransition(attackState, chaseState, () => !IsPlayerInRange(baseAttackRange));
            
            if (m_Health != null)
                m_StateMachine.AddAnyTransition(deadState, () => m_Health.IsDead);

            m_StateMachine.SetState(chaseState);
        }

        private void PerformEliteAttack()
        {
            // Lựa chọn kĩ năng đặc biệt nếu có sẵn 
            foreach (var runtime in m_SpellsRuntime)
            {
                if (runtime.CurrentCooldown <= 0f)
                {
                    CastEliteSpell(runtime);
                    return; // Cast xong 1 skill là nghỉ turn
                }
            }

            // Mặc định đánh thường
            if (m_Animator != null) m_Animator.SetTrigger("Attack");

            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 2f, 3f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player") && hit.TryGetComponent<Combat.HealthComponent>(out var hp))
                {
                    hp.TakeDamage(10); // Hardcoded base damage or fetch from new stat
                }
            }
        }

        private void CastEliteSpell(EliteSpellRuntime runtime)
        {
            if (m_Animator != null) m_Animator.SetTrigger("CastSpell");

            var spellData = runtime.Entry.SpellData;
            // Dùng pool để xả skill.
            var spellObj = Combat.Spells.SpellPoolService.Instance?.GetSpell(spellData);
            
            if (spellObj != null)
            {
                // Thay vì transform mặc định, áp dụng Override Settings từ Elite Boss Data
                spellObj.transform.position = transform.position + transform.forward * 2f;
                spellObj.transform.rotation = transform.rotation;

                // Tăng damage nhờ Multiplier
                spellObj.Setup(spellData, transform, runtime.Entry.DamageOverride);
                spellObj.Launch();
                
                // Mặc định spell có tự trả vào pool qua FireballSpell hay Blast ko?
                // Nếu phép là AOE tĩnh, nó tự quản lý thời gian sống.
            }

            runtime.CurrentCooldown = runtime.Entry.CooldownOverride;
        }

        // Thay vì biến mất luôn như EnemyBase, Boss chết sẽ drop đồ và báo LevelManager
        public new void Die()
        {
            var levelManager = FindAnyObjectByType<Level.LevelManager>();
            levelManager?.OnBossDefeated();

            // Thả boss reward (hoặc UnlockSpell script)
            Combat.LootDropperService.Instance?.TryDropLoot(transform.position, m_Data.DropTable);

            // Bật Boss Death VFX 
            Destroy(gameObject, 2f);
        }
    }
}
