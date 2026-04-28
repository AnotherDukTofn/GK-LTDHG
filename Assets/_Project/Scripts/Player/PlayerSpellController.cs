using System;
using UnityEngine;
using SpellStrike.Data;
using SpellStrike.Core.EventChannels;

namespace SpellStrike.Player
{
    /// <summary>
    /// Component gắn trên Player, xử lý logic Equip/Cast phép và tracking cooldown.
    /// Hoạt động độc lập với StateMachine (Player có thể vừa chạy vừa bắn).
    /// </summary>
    public class PlayerSpellController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Weapon/Cast Point")]
        [SerializeField] private Transform m_CastPoint;
        [Tooltip("Khoảng cách đẩy projectile ra phía trước mặt để tránh dính người.")]
        [SerializeField] private float m_SpawnForwardOffset = 0.5f;

        [Header("Physics")]
        [Tooltip("Layer mask dùng để raycast xuống mặt đất (Terrain, Environment). Nếu set Nothing, sẽ fall back về mặt phẳng ảo.")]
        [SerializeField] private LayerMask m_GroundLayerMask = ~0;

        [Header("Global Events")]
        [SerializeField] private SpellDataEventChannelSO m_OnSpellEquippedChannel;
        // Event truyền float (ratio cooldown còn lại)
        [SerializeField] private FloatEventChannelSO m_OnCooldownTickChannel;

        #endregion

        #region Private Fields

        [SerializeField] private SpellDataSO m_EquippedSpell;
        private float m_CurrentCooldown;
        private PlayerInputHandler m_Input;
        private PlayerController m_PlayerController;
        private UnityEngine.Camera m_Camera;
        
        // Hỗ trợ phép dạng Hold
        private Combat.Spells.SpellBase m_HoldSpellInstance;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            m_Input = GetComponent<PlayerInputHandler>();
            if (m_Input == null) m_Input = GetComponentInParent<PlayerInputHandler>();

            if (m_Input == null)
            {
                Debug.LogError("[PlayerSpellController] Không tìm thấy PlayerInputHandler! Có thể bạn đang gắn script này sai Object.");
            }

            m_PlayerController = GetComponent<PlayerController>();
            if (m_PlayerController == null) m_PlayerController = GetComponentInParent<PlayerController>();

            m_Camera = UnityEngine.Camera.main;
            if (m_Camera == null) m_Camera = FindObjectOfType<UnityEngine.Camera>();
        }

        private void Start()
        {
            if (m_EquippedSpell != null)
            {
                EquipSpell(m_EquippedSpell);
            }
        }

        private void OnEnable()
        {
            if (m_Input != null)
            {
                m_Input.OnCastStarted += HandleCastStart;
                m_Input.OnCastCanceled += HandleCastEnd;
            }
        }

        private void OnDisable()
        {
            if (m_Input != null)
            {
                m_Input.OnCastStarted -= HandleCastStart;
                m_Input.OnCastCanceled -= HandleCastEnd;
            }
        }

        private void Update()
        {
            if (m_CurrentCooldown > 0f)
            {
                m_CurrentCooldown -= Time.deltaTime;
                m_OnCooldownTickChannel?.RaiseEvent(m_CurrentCooldown / m_EquippedSpell.Cooldown);
            }
            else if (m_EquippedSpell != null)
            {
                m_OnCooldownTickChannel?.RaiseEvent(0f);
                
                // Track dạng hold 
                if (m_EquippedSpell.InputType == SpellInputType.Hold && m_Input.IsCasting && m_HoldSpellInstance != null)
                {
                    m_HoldSpellInstance.HoldTick();
                }
            }

            }


        #endregion

        #region Public Methods

        public void EquipSpell(SpellDataSO _spellData)
        {
            EndHoldSpell(); // Hủy phép hold cũ đang dang dở

            m_EquippedSpell = _spellData;
            m_CurrentCooldown = 0f;
            
            m_OnSpellEquippedChannel?.RaiseEvent(m_EquippedSpell);

            // Bắt đầu aura nếu là phép thụ động
            if (m_EquippedSpell.InputType == SpellInputType.Passive)
            {
                CastPassive();
            }
        }

        /// <summary>
        /// Lấy vị trí chuột trên mặt đất (world space).
        /// </summary>
        public Vector3 GetMouseWorldPoint()
        {
            if (m_Camera == null)
            {
                m_Camera = UnityEngine.Camera.main;
                if (m_Camera == null) return transform.position + transform.forward * 5f;
            }

            Ray ray = m_Camera.ScreenPointToRay(m_Input.MousePosition);
            
            // 1. Thử raycast với Physics (Terrain/Ground)
            if (m_GroundLayerMask.value != 0 && Physics.Raycast(ray, out RaycastHit hit, 1000f, m_GroundLayerMask))
            {
                return hit.point;
            }

            // 2. Fallback về mặt phẳng ảo
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

            if (groundPlane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return transform.position + transform.forward * 5f;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Tính toán vị trí ra đòn dựa trên CastPoint và Forward Offset.
        /// </summary>
        private Vector3 GetSpawnPosition()
        {
            if (m_CastPoint == null) return transform.position + transform.forward * m_SpawnForwardOffset;
            return m_CastPoint.position + transform.forward * m_SpawnForwardOffset;
        }

        private void HandleCastStart()
        {
            Debug.Log($"[PlayerSpellController] HandleCastStart triggered. Equipped: {(m_EquippedSpell != null ? m_EquippedSpell.DisplayName : "NULL")}");
            
            if (m_EquippedSpell == null) return;
            if (m_EquippedSpell.InputType == SpellInputType.Passive) return; // Không cần click

            if (m_CurrentCooldown <= 0f)
            {
                // Snap xoay Player về hướng chuột trước khi cast
                SnapRotateToMouse();

                if (m_EquippedSpell.InputType == SpellInputType.Click)
                {
                    CastClick();
                }
                else if (m_EquippedSpell.InputType == SpellInputType.Hold)
                {
                    StartHoldSpell();
                }
            }
            else
            {
                Debug.Log($"[PlayerSpellController] Cast failed: Cooldown active ({m_CurrentCooldown:F2}s)");
            }
        }

        private void HandleCastEnd()
        {
            if (m_EquippedSpell != null && m_EquippedSpell.InputType == SpellInputType.Hold)
            {
                EndHoldSpell();
            }
        }

        /// <summary>
        /// Snap xoay Player ngay lập tức về hướng chuột (không lerp).
        /// </summary>
        private void SnapRotateToMouse()
        {
            Vector3 mouseWorld = GetMouseWorldPoint();
            Vector3 dir = mouseWorld - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(dir.normalized);
            }
        }

        private void CastClick()
        {
            Debug.Log($"[PlayerSpellController] Attempting to CastClick: {m_EquippedSpell.DisplayName}");
            
            var spellObj = Combat.Spells.SpellPoolService.Instance.GetSpell(m_EquippedSpell);
            if (spellObj != null)
            {
                spellObj.transform.position = GetSpawnPosition();
                spellObj.transform.rotation = transform.rotation;
                spellObj.Setup(m_EquippedSpell, transform);

                // Truyền vị trí chuột cho phép AreaLob (Fireball)
                if (m_EquippedSpell.Behavior == SpellBehavior.AreaLob)
                {
                    if (spellObj is Combat.Spells.FireballSpell fireball)
                    {
                        fireball.SetTargetPoint(GetMouseWorldPoint());
                    }
                }

                spellObj.Launch();
                Debug.Log($"[PlayerSpellController] Spell Launched: {m_EquippedSpell.DisplayName}");
                
                m_CurrentCooldown = m_EquippedSpell.Cooldown;
            }
            else
            {
                Debug.LogError($"[PlayerSpellController] CastClick FAILED: SpellPoolService returned NULL for {m_EquippedSpell.DisplayName}. Check if SpellPrefab is assigned in SO!");
            }
        }

        private void StartHoldSpell()
        {
            if (m_HoldSpellInstance != null) return; // Anti-spam check: prevent multiple hold instances

            m_HoldSpellInstance = Combat.Spells.SpellPoolService.Instance.GetSpell(m_EquippedSpell);
            if (m_HoldSpellInstance != null)
            {
                m_HoldSpellInstance.transform.position = GetSpawnPosition();
                // Có thể cần Parent nó vào tay nhân vật
                m_HoldSpellInstance.transform.SetParent(m_CastPoint); 
                m_HoldSpellInstance.transform.localRotation = Quaternion.identity;

                m_HoldSpellInstance.Setup(m_EquippedSpell, transform);
                m_HoldSpellInstance.Launch(); // Gọi lúc bắt đầu Hold
            }
        }

        private void EndHoldSpell()
        {
            if (m_HoldSpellInstance != null)
            {
                m_HoldSpellInstance.EndHold();
                Combat.Spells.SpellPoolService.Instance.ReturnSpell(m_EquippedSpell.Id, m_HoldSpellInstance);
                m_HoldSpellInstance = null;

                m_CurrentCooldown = m_EquippedSpell.Cooldown; // Tính CD khi NGỪNG hold
            }
        }

        private void CastPassive()
        {
            var spellObj = Combat.Spells.SpellPoolService.Instance.GetSpell(m_EquippedSpell);
            if (spellObj != null)
            {
                // Aura đi theo player
                spellObj.transform.SetParent(transform);
                spellObj.transform.localPosition = Vector3.zero;
                
                spellObj.Setup(m_EquippedSpell, transform);
                spellObj.Launch();
                // Không có cooldown
            }
        }

        #endregion
    }
}
