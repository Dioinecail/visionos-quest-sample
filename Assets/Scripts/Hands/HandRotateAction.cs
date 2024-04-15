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
        public float CurrentValue => m_PalmDirection;
        public float RotationThreshold => m_RotationThreshold;

        [SerializeField] private Handedness m_TargetHand;
        [SerializeField] private HandPoseProvider m_HandPoseProvider;
        [SerializeField] private GameObject m_RotationControls;
        [SerializeField] private SnapTurnProvider m_TargetTurnProvider;
        [SerializeField] private float m_GeneralFingersThreshold = -0.25f;
        [SerializeField, Range(0f, 90f)] private float m_RotationThreshold = 0.25f;
        [SerializeField] private float m_DirectionLerpSpeed = 0.25f;

        private XRInputValueReader<Vector2> m_TurnValueReader;

        private float m_RotationThresholdDot;
        private bool m_State;
        private float m_IndexDirection;
        private float m_MiddleDirection;
        private float m_RingDirection;
        private float m_PinkyDirection;
        private float m_PalmDirection;



        private void OnEnable()
        {
            m_TurnValueReader = m_TargetHand == Handedness.Left ? m_TargetTurnProvider.leftHandTurnInput : m_TargetTurnProvider.rightHandTurnInput;
            m_RotationThresholdDot = 1f - Vector3.Dot(Vector3.up, Quaternion.Euler(0f, 0f, m_RotationThreshold) * Vector3.up);
        }

        private void Update()
        {
            var isFoundFingers = m_HandPoseProvider.TryGetJoint(TrackedHandJoint.Palm, out var palmPose);

            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.IndexIntermediate, palmPose, out var indexDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.MiddleIntermediate, palmPose, out var middleDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.RingIntermediate, palmPose, out var ringDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.LittleIntermediate, palmPose, out var pinkyDirection);

            m_IndexDirection = Mathf.Lerp(m_IndexDirection, indexDirection, m_DirectionLerpSpeed);
            m_MiddleDirection = Mathf.Lerp(m_MiddleDirection, middleDirection, m_DirectionLerpSpeed);
            m_RingDirection = Mathf.Lerp(m_RingDirection, ringDirection, m_DirectionLerpSpeed);
            m_PinkyDirection = Mathf.Lerp(m_PinkyDirection, pinkyDirection, m_DirectionLerpSpeed);
            m_PalmDirection = Mathf.Lerp(m_PalmDirection, Camera.main.transform.InverseTransformDirection(palmPose.Forward).x, m_DirectionLerpSpeed);

            if (!isFoundFingers || !(Vector3.Dot(Vector3.up, palmPose.Up) < -0.5f ))
            {
                m_IndexDirection
                    = m_MiddleDirection
                    = m_RingDirection
                    = m_PinkyDirection
                    = m_PalmDirection = 0f;

                m_RotationControls.SetActive(false);
                m_TurnValueReader.manualValue = Vector2.zero;
                return;
            }

            var isRotationGestureActive = m_IndexDirection > m_GeneralFingersThreshold
                && m_MiddleDirection > m_GeneralFingersThreshold
                && m_RingDirection > m_GeneralFingersThreshold
                && m_PinkyDirection > m_GeneralFingersThreshold;

            m_RotationControls.SetActive(isRotationGestureActive);

            if(isRotationGestureActive)
            {
                if(Mathf.Abs(m_PalmDirection) > m_RotationThresholdDot)
                {
                    m_TurnValueReader.manualValue = new Vector2(m_PalmDirection, 0f);
                }
                else
                {
                    m_TurnValueReader.manualValue = Vector2.zero;
                }
            }
            else
            {
                m_TurnValueReader.manualValue = Vector2.zero;
            }
        }

        private bool TryGetFingerDirection(TrackedHandJoint finger, HandJointPose palmRef, out float direction)
        {
            bool gotData = m_HandPoseProvider.TryGetJoint(finger, out var fingerPose);

            if (gotData)
            {
                var cameraForward = palmRef.Forward;
                var fingerDirection = fingerPose.Forward;

                direction = Vector3.Dot(fingerDirection, cameraForward);

                return true;
            }

            direction = 0f;
            return false;
        }

        private bool TryGetThumbDirection(out float direction)
        {
            bool gotData = m_HandPoseProvider.TryGetJoint(TrackedHandJoint.ThumbTip, out var fingerPose);
            gotData &= m_HandPoseProvider.TryGetJoint(TrackedHandJoint.Palm, out var palmPose);

            if (gotData)
            {
                var cameraToPalm = (palmPose.Position - Camera.main.transform.position).normalized;
                var thumbUp = fingerPose.Forward;
                var thumbCross = Vector3.Cross(cameraToPalm, Vector3.up);

                direction = Vector3.Dot(thumbUp, thumbCross);

                return true;
            }

            direction = 0f;
            return false;
        }
    }
}
