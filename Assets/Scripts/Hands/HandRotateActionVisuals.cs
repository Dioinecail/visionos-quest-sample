using UnityEngine;

namespace Project.Hands
{
    public class HandRotateActionVisuals : MonoBehaviour
    {
        [SerializeField] private HandRotateAction m_TargetAction;
        [SerializeField] private GameObject m_VisualsGameObject;
        [SerializeField] private Transform m_VisualsRoot;
        [SerializeField] private LineRenderer m_Line;

        [SerializeField] private int m_LineQuality = 50;
        [SerializeField] private float m_Radius = 0.25f;

        private Transform m_CameraTransform;
        private float m_RotationAngle;



        private void Awake()
        {
            m_TargetAction.OnStateChanged += HandleStateChanged;
            m_CameraTransform = Camera.main.transform;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            m_VisualsRoot.rotation = Quaternion.LookRotation(m_CameraTransform.forward, Vector3.up);
            m_VisualsRoot.position = m_TargetAction.OriginPositionWorldSpace;
        }

        [ContextMenu("Set Size")]
        private void SetSize()
        {
            var up = Vector3.up;
            var positions = new Vector3[m_LineQuality];

            for (int i = 0; i < m_LineQuality; i++)
            {
                var dir = Quaternion.Euler(0f, 0f, ((float)i / m_LineQuality) * 360f) * up * m_TargetAction.DistanceToActivate;
                positions[i] = dir;
            }

            m_Line.positionCount = m_LineQuality;
            m_Line.SetPositions(positions);
        }

        private void HandleStateChanged(bool state)
        {
            m_VisualsGameObject.SetActive(state);
        }
    }
}