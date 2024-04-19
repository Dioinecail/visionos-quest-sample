using System;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace Project.Hands
{
    public class HandRotateAction : MonoBehaviour
    {
        public event Action<bool> OnStateChanged;

        public bool State
        {
            get => m_State;
            set
            {
                if (m_State == value)
                    return;

                m_State = value;
                OnStateChanged?.Invoke(m_State);
            }
        }

        public float DistanceToActivate => m_DistanceToActivate;
        public Vector3 OriginPositionWorldSpace => Camera.main.transform.TransformPoint(m_ActionOrigin);

        [SerializeField] private Handedness m_TargetHand;
        [SerializeField] private HandPoseProvider m_HandPoseProvider;
        [SerializeField] private SnapTurnProvider m_TargetTurnProvider;
        [SerializeField] private float m_FingersDirectionThreshold = -0.65f;
        [SerializeField] private float m_DistanceToActivate = 0.25f;
        [SerializeField] private float m_DirectionLerpSpeed = 0.25f;
        [SerializeField] private float m_PinchOpenThreshold = 0.75f;
        [SerializeField] private float m_PinchClosedThreshold = 0.25f;
        [SerializeField] private float m_HandRaiseCameraFov = 45f;
        [SerializeField] private float m_DirectionSmoothing = 0.25f;

        private XRInputValueReader<Vector2> m_TurnValueReader;

        private bool m_State;
        private bool m_WasPalmReadyLastFrame;
        private bool m_IsActionExecuted;
        private Vector3 m_ActionOrigin;
        private Vector3 m_ActionPosition;

        private float m_IndexDirection;
        private float m_MiddleDirection;
        private float m_RingDirection;
        private float m_PinkyDirection;



        private void OnEnable()
        {
            m_TurnValueReader = m_TargetHand == Handedness.Left ? m_TargetTurnProvider.leftHandTurnInput : m_TargetTurnProvider.rightHandTurnInput;
        }

        private void Update()
        {
            bool gotData = m_HandPoseProvider.TryGetJoint(TrackedHandJoint.Palm, out var palmPose);

            gotData &= TryGetFingerDirection(TrackedHandJoint.IndexTip, palmPose, out var indexDirection);
            gotData &= TryGetFingerDirection(TrackedHandJoint.MiddleTip, palmPose, out var middleDirection);
            gotData &= TryGetFingerDirection(TrackedHandJoint.RingTip, palmPose, out var ringDirection);
            gotData &= TryGetFingerDirection(TrackedHandJoint.LittleTip, palmPose, out var pinkyDirection);

            if (!gotData)
            {
                State = false;
                m_TurnValueReader.manualValue = Vector2.zero;
                m_WasPalmReadyLastFrame = false;
                m_IndexDirection = 0f;
                m_MiddleDirection = 0f;
                m_RingDirection = 0f;
                m_PinkyDirection = 0f;

                return;
            }

            m_IndexDirection = Mathf.Lerp(m_IndexDirection, indexDirection, m_DirectionSmoothing);
            m_MiddleDirection = Mathf.Lerp(m_MiddleDirection, middleDirection, m_DirectionSmoothing);
            m_RingDirection = Mathf.Lerp(m_RingDirection, ringDirection, m_DirectionSmoothing);
            m_PinkyDirection = Mathf.Lerp(m_PinkyDirection, pinkyDirection, m_DirectionSmoothing);

            var isPalmReady = m_IndexDirection < m_FingersDirectionThreshold
                && m_MiddleDirection < m_FingersDirectionThreshold
                && m_RingDirection < m_FingersDirectionThreshold
                && m_PinkyDirection < m_FingersDirectionThreshold;

            State = (isPalmReady && !m_IsActionExecuted);

            if (isPalmReady)
            {
                if(!m_WasPalmReadyLastFrame)
                    m_ActionOrigin = Camera.main.transform.InverseTransformPoint(palmPose.Position);

                m_ActionPosition = Camera.main.transform.InverseTransformPoint(palmPose.Position);

                var delta = m_ActionPosition - m_ActionOrigin;

                if (Mathf.Abs(delta.x) > m_DistanceToActivate)
                {
                    m_IsActionExecuted = true;
                    m_TurnValueReader.manualValue = new Vector2(delta.x, 0f);
                }
                else
                    m_TurnValueReader.manualValue = Vector2.zero;
            }
            else
            {
                m_TurnValueReader.manualValue = Vector2.zero;
            }

            m_WasPalmReadyLastFrame = isPalmReady;

            if(m_IsActionExecuted && !m_WasPalmReadyLastFrame && !isPalmReady)
            {
                m_IsActionExecuted = false;
            }
        }

        private bool TryGetFingerDirection(TrackedHandJoint finger, HandJointPose palmPose, out float direction)
        {
            var isValid = m_HandPoseProvider.TryGetJoint(finger, out var fingerPose);

            if (!isValid)
            {
                direction = 0f;
                return false;
            }

            var palmForward = palmPose.Forward;
            var fingerForward = fingerPose.Forward;

            direction = Vector3.Dot(fingerForward, palmForward);
            return true;
        }
    }
}
