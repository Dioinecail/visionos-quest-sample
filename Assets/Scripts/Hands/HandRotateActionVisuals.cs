using UnityEngine;

namespace Project.Hands
{
    public class HandRotateActionVisuals : MonoBehaviour
    {
        [SerializeField] private HandRotateAction m_TargetAction;
        [SerializeField] private GameObject m_VisualsGameObject;
        [SerializeField] private Transform m_VisualsRoot;
        [SerializeField] private LineRenderer m_Line;
        [SerializeField] private LineRenderer m_Direction;
        [SerializeField] private LineRenderer m_RotationOffsetLeft;
        [SerializeField] private LineRenderer m_RotationOffsetRight;

        [SerializeField] private int m_LineQuality = 50;
        [SerializeField] private float m_Radius = 0.25f;

        private Transform m_CameraTransform;
        private float m_RotationAngle;



        private void Awake()
        {
            m_TargetAction.OnStateChanged += HandleStateChanged;
            m_CameraTransform = Camera.main.transform;

            m_RotationAngle = m_TargetAction.RotationThreshold;

            var offsetLeft = Quaternion.Euler(0f, 0f, -m_RotationAngle) * Vector3.up * m_Radius;
            var offsetRight = Quaternion.Euler(0f, 0f, m_RotationAngle) * Vector3.up * m_Radius;

            m_RotationOffsetLeft.SetPosition(1, offsetLeft);
            m_RotationOffsetRight.SetPosition(1, offsetRight);
        }

        private void Update()
        {
            m_VisualsRoot.rotation = Quaternion.LookRotation(m_CameraTransform.forward, Vector3.up);
            SetCircle();
        }

        [ContextMenu("Test")]
        private void SetCircle()
        {
            var positions = new Vector3[m_LineQuality + 1];

            for (int i = 0; i <= m_LineQuality; i++)
            {
                var angle = (i / (float)m_LineQuality) * 360f;

                positions[i] = Quaternion.Euler(0f, 0f, angle) * Vector3.up * m_Radius;
            }

            var currentAngle = Mathf.LerpUnclamped(0f, 90f, -m_TargetAction.CurrentValue);
            var currentValueDirection = Quaternion.Euler(0f, 0f, currentAngle) * Vector3.up * m_Radius;

            m_Direction.SetPosition(1, currentValueDirection);
            m_Line.SetPositions(positions);

            m_RotationAngle = m_TargetAction.RotationThreshold;

            var offsetLeft1 = Quaternion.Euler(0f, 0f, -m_RotationAngle) * Vector3.up * (m_Radius * 0.75f);
            var offsetRight1 = Quaternion.Euler(0f, 0f, m_RotationAngle) * Vector3.up * (m_Radius * 0.75f);

            var offsetLeft2 = Quaternion.Euler(0f, 0f, -m_RotationAngle) * Vector3.up * m_Radius;
            var offsetRight2 = Quaternion.Euler(0f, 0f, m_RotationAngle) * Vector3.up * m_Radius;

            m_RotationOffsetLeft.SetPosition(0, offsetLeft1);
            m_RotationOffsetLeft.SetPosition(1, offsetLeft2);
            m_RotationOffsetRight.SetPosition(0, offsetRight1);
            m_RotationOffsetRight.SetPosition(1, offsetRight2);
        }

        private void HandleStateChanged(bool state)
        {
            m_VisualsGameObject.SetActive(state);
        }
    }
}