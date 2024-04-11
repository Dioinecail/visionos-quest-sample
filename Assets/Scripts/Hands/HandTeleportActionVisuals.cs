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
        [SerializeField] private Transform m_VisualsRoot;
        [SerializeField] private LineRenderer m_Line;

        [SerializeField] private int m_LineQuality = 50;
        [SerializeField] private float m_Radius = 0.25f;



        private void OnEnable()
        {
            m_TargetAction.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            m_TargetAction.OnStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            var positions = new Vector3[m_LineQuality + 1];

            for (int i = 0; i <= m_LineQuality; i++)
            {
                var angle = (i / (float)m_LineQuality) * 360f;

                positions[i] = m_VisualsRoot.TransformPoint(Quaternion.Euler(0f, 0f, angle) * Vector3.up * m_Radius);
            }

            m_Line.SetPositions(positions);
        }

        private void HandleStateChanged(bool state)
        {
            m_VisualsGameObject.SetActive(state);
        }
    }
}