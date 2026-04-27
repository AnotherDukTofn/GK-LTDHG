using UnityEngine;

namespace SpellStrike.Camera
{
    /// <summary>
    /// Camera Controller bám theo mục tiêu với góc nhìn Isometric Top-down.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Transform m_Target;
        [SerializeField] private Vector3 m_Offset = new Vector3(-15f, 15f, -15f);
        [SerializeField] private float m_SmoothSpeed = 5f;
        [SerializeField] private bool m_UseFixedRotation = true;
        [SerializeField] private Vector3 m_FixedRotation = new Vector3(30f, 45f, 0f);
        [SerializeField] private bool m_IsOrthographic = true;
        [SerializeField] private float m_OrthoSize = 8f;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            ApplyCameraSettings();
        }

        private void LateUpdate()
        {
            if (m_Target == null) return;

            // Di chuyển camera tới vị trí bám đuổi
            Vector3 desiredPosition = m_Target.position + m_Offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, m_SmoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // Cập nhật hướng nhìn
            if (m_UseFixedRotation)
            {
                transform.rotation = Quaternion.Euler(m_FixedRotation);
            }
            else
            {
                transform.LookAt(m_Target);
            }
        }

        private void OnValidate()
        {
            ApplyCameraSettings();
        }

        #endregion

        #region Private Methods

        private void ApplyCameraSettings()
        {
            var cam = GetComponent<UnityEngine.Camera>();
            if (cam != null)
            {
                cam.orthographic = m_IsOrthographic;
                if (m_IsOrthographic)
                {
                    cam.orthographicSize = m_OrthoSize;
                }
            }
        }

        #endregion

        #region Public Methods

        public void SetTarget(Transform _target)
        {
            m_Target = _target;
        }

        #endregion
    }
}
