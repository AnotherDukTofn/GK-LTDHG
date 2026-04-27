using UnityEngine;
using SpellStrike.Core.StateMachine;
using SpellStrike.Data;
using SpellStrike.Combat;

namespace SpellStrike.Player
{
    /// <summary>
    /// Hub trung tâm của Player. Chứa references đến các component và FSM.
    /// Kế thừa MonoBehaviour nhưng logic thực thi nằm trong các State.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameConfigSO m_Config;

        #endregion

        #region Private Fields

        private StateMachine m_StateMachine;
        
        // Components
        private CharacterController m_CharController;
        private PlayerInputHandler m_InputHandler;
        private StaminaComponent m_Stamina;
        private DashController m_Dash;
        private HealthComponent m_Health;
        private StatusEffectController m_StatusEffect;
        private Animator m_Animator;

        // Bề mặt ảo để raycast tìm điểm xoay chuột
        private Plane m_GroundPlane = new Plane(Vector3.up, Vector3.zero);

        #endregion

        #region Public Properties (For States)

        public GameConfigSO Config => m_Config;
        public CharacterController CharController => m_CharController;
        public PlayerInputHandler Input => m_InputHandler;
        public StaminaComponent Stamina => m_Stamina;
        public DashController Dash => m_Dash;
        public HealthComponent Health => m_Health;
        public StatusEffectController StatusEffect => m_StatusEffect;
        public Animator Animator => m_Animator;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            m_CharController = GetComponent<CharacterController>();
            m_InputHandler = GetComponent<PlayerInputHandler>();
            m_Stamina = GetComponent<StaminaComponent>();
            m_Dash = GetComponent<DashController>();
            m_Health = GetComponentInChildren<HealthComponent>();
            m_StatusEffect = GetComponentInChildren<StatusEffectController>();
            m_Animator = GetComponentInChildren<Animator>();

            SetupStateMachine();
        }

        private void Start()
        {
            // Init HP inside Start to ensure UI elements (like HPBarUI) 
            // have already subscribed to the Global Event Channel in their OnEnable.
            if (m_Health != null && m_Config != null)
                m_Health.Initialize(m_Config.PlayerMaxHP);
        }

        private void Update()
        {
            m_StateMachine.Tick();
        }

        private void FixedUpdate()
        {
            m_StateMachine.FixedTick();
        }

        #endregion

        #region FSM Setup

        private void SetupStateMachine()
        {
            m_StateMachine = new StateMachine();

            // Khởi tạo các state
            var idleState = new PlayerIdleState(this);
            var moveState = new PlayerMoveState(this);
            var dashState = new PlayerDashState(this);
            var deadState = new PlayerDeadState(this);

            // Cấu hình Transition (Deklative FSM)
            
            // Điều kiện Transition: Idle <-> Move
            m_StateMachine.AddTransition(idleState, moveState, () => m_InputHandler.MoveInput.sqrMagnitude > 0.01f);
            m_StateMachine.AddTransition(moveState, idleState, () => m_InputHandler.MoveInput.sqrMagnitude <= 0.01f);

            // Điều kiện Transition: Bất kỳ -> Dash (Phím Space, không đang dash, đủ stamina)
            bool CanDash() => m_InputHandler.IsDashing && !m_Dash.IsDashing && m_Stamina != null && m_Stamina.HasEnoughStamina(m_Config.DashStaminaCost);
            m_StateMachine.AddTransition(idleState, dashState, CanDash);
            m_StateMachine.AddTransition(moveState, dashState, CanDash);

            // Điều kiện Transition: Dash -> Idle/Move (Hết dash -> trở về tùy input)
            m_StateMachine.AddTransition(dashState, moveState, () => !m_Dash.IsDashing && m_InputHandler.MoveInput.sqrMagnitude > 0.01f);
            m_StateMachine.AddTransition(dashState, idleState, () => !m_Dash.IsDashing && m_InputHandler.MoveInput.sqrMagnitude <= 0.01f);

            // Điều kiện Any -> Dead
            if (m_Health != null)
            {
                m_StateMachine.AddAnyTransition(deadState, () => m_Health.IsDead);
            }

            // State ban đầu
            m_StateMachine.SetState(idleState);
        }

        #endregion

        #region Helper Methods (For States)

        /// <summary>
        /// Xoay mượt tới vị trí chuột trên mặt phẳng y=0
        /// </summary>
        public void RotateTowardsMouse()
        {
            if (UnityEngine.Camera.main == null) return;

            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(m_InputHandler.MousePosition);
            
            // Xây dựng mặt phẳng (Plane) dựa theo độ cao (Y) hiện tại của Player
            // để raycast không bị lệch vị trí khi camera là Isometric và player nằm trên mặt đất.
            Plane playerPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
            
            if (playerPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 dir = hitPoint - transform.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                    float speed = m_Config != null && m_Config.PlayerRotateSpeed > 0 ? m_Config.PlayerRotateSpeed : 720f;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, speed * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Xoay theo hướng di chuyển của phím điều hướng (relative to camera)
        /// </summary>
        public void RotateTowardsMoveDirection()
        {
            Vector3 moveDir = GetCameraRelativeMoveDirection();
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                float speed = m_Config != null && m_Config.PlayerRotateSpeed > 0 ? m_Config.PlayerRotateSpeed : 720f;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, speed * Time.deltaTime);
            }
        }

        // Camera cache để tránh FindObjectOfType liên tục
        private UnityEngine.Camera m_CachedCamera;

        /// <summary>
        /// Chuyển đổi Input 2D sang Vector3 dựa trên góc quay của Camera
        /// </summary>
        public Vector3 GetCameraRelativeMoveDirection()
        {
            Vector2 input = m_InputHandler.MoveInput;
            
            if (m_CachedCamera == null)
            {
                m_CachedCamera = UnityEngine.Camera.main;
                if (m_CachedCamera == null)
                {
                    m_CachedCamera = FindObjectOfType<UnityEngine.Camera>(); // Dự phòng nếu quên gắn tag MainCamera
                }
            }

            if (m_CachedCamera == null)
            {
                return new Vector3(input.x, 0f, input.y).normalized;
            }

            // Lấy forward và right thực tế của Camera, ném bỏ trục Y rồi chuẩn hoá lại
            // Đảm bảo mapping X/Y input khớp 100% với màn hình của người chơi
            Vector3 forward = m_CachedCamera.transform.forward;
            Vector3 right = m_CachedCamera.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = forward * input.y + right * input.x;

            if (moveDir.sqrMagnitude > 1f)
                moveDir.Normalize();

            return moveDir;
        }

        #endregion
    }
}
