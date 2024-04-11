using Project.Core;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace Project.Hands
{
    public class HandTeleportActionVisuals : MonoBehaviour
    {
        [SerializeField] private HandTeleportAction m_TargetAction;
        [SerializeField] private GameObject m_VisualsGameObject;
        [SerializeField] private GameObject m_RayLine;
        [SerializeField] private Transform m_VisualsRoot;
        [SerializeField] private LineRenderer m_Line;
        [SerializeField] private LineRenderer m_Direction;
        [SerializeField] private LineRenderer m_OffsetLeft;

        [SerializeField] private int m_LineQuality = 50;
        [SerializeField] private float m_Radius = 0.1f;

        private float m_RotationAngle;



        private void Awake()
        {
            m_TargetAction.OnStateChanged += HandleStateChanged;
            m_RotationAngle = m_TargetAction.RotationThreshold;

            var positions = new Vector3[m_LineQuality + 1];
            var offset1 = Quaternion.Euler(0f, 0f, m_RotationAngle) * -Vector3.right * m_Radius * 0.75f;
            var offset2 = Quaternion.Euler(0f, 0f, m_RotationAngle) * -Vector3.right * m_Radius;

            for (int i = 0; i <= m_LineQuality; i++)
            {
                var angle = (i / (float)m_LineQuality) * m_RotationAngle;

                positions[i] = Quaternion.Euler(0f, 0f, angle) * -Vector3.right * m_Radius;
            }

            m_OffsetLeft.SetPosition(0, offset1);
            m_OffsetLeft.SetPosition(1, offset2);
            m_Line.SetPositions(positions);
        }

        private void Update()
        {
            var currentValue = m_TargetAction.CurrentValue;
            var directionAngle = Mathf.Lerp(0f, 90f, currentValue);
            var direction1 = Quaternion.Euler(0f, 0f, directionAngle) * -Vector3.right * m_Radius * 0.75f;
            var direction2 = Quaternion.Euler(0f, 0f, directionAngle) * -Vector3.right * m_Radius;

            m_Direction.SetPosition(0, direction1);
            m_Direction.SetPosition(1, direction2);
        }

        private void HandleStateChanged(bool state)
        {
            m_VisualsGameObject.SetActive(state);
            m_RayLine.SetActive(state);
        }
    }
}