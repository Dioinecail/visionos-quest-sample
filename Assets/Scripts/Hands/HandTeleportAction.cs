using Project.Core;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace Project.Hands
{
    public class HandTeleportAction : MonoBehaviour
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

        public float CurrentValue => m_ThumbDirection;
        public float RotationThreshold => m_ThumbFingerDirectionThreshold;
        public HandJointPose PalmPose => m_PalmPose;

        [SerializeField] private HandPoseProvider m_HandPoseProvider;
        [SerializeField] private AdaptiveRayInteractor m_TargetRayInteractor;
        [SerializeField, Tooltip("Threshold for the index finger pointing outwards")]
        private float m_IndexFingerDirectionThreshold = 0.65f;
        [SerializeField, Tooltip("Threshold for the rest of the fingers to point inwards")]
        private float m_OtherFingersDirectionThreshold = -0.25f;
        [SerializeField, Range(0f, 90f), Tooltip("Threshold for the thumb finger angle, passing this threshold activates the teleport")]
        private float m_ThumbFingerDirectionThreshold = 0.3f;
        [SerializeField] private float m_DirectionLerpSpeed = 0.25f;

        [SerializeField] private InputActionProperty m_ActivateTeleportAction;
        [SerializeField] private InputActionProperty m_InvokeTeleportAction;

        private XRInputButtonReader m_TargetRayActivateTeleportInput;
        private XRInputButtonReader m_TargetRayInvokeTeleportInput;
        private bool m_TeleportWasActiveLastFrame;
        private bool m_TeleportInvokedLastFrame;
        private bool m_State = true;
        private float m_IndexDirection;
        private float m_MiddleDirection;
        private float m_RingDirection;
        private float m_PinkyDirection;
        private float m_ThumbDirection;
        private float m_ThumbDirectionThresholdDot;
        private HandJointPose m_PalmPose;



        private void OnEnable()
        {
            m_TargetRayActivateTeleportInput = m_TargetRayInteractor.selectInput;
            m_TargetRayInvokeTeleportInput = m_TargetRayInteractor.activateInput;
            m_ThumbDirectionThresholdDot = Vector3.Dot(Vector3.up, Quaternion.Euler(0f, 0f, m_ThumbFingerDirectionThreshold) * Vector3.up);
        }

        private void Update()
        {
            var isFoundFingers = m_HandPoseProvider.TryGetJoint(TrackedHandJoint.Palm, out m_PalmPose);

            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.IndexIntermediate, m_PalmPose, out var indexDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.MiddleIntermediate, m_PalmPose, out var middleDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.RingIntermediate, m_PalmPose, out var ringDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.LittleIntermediate, m_PalmPose, out var pinkyDirection);
            isFoundFingers &= TryGetThumbDirection(out var thumbDirection);

            if (!isFoundFingers)
            {
                m_IndexDirection 
                    = m_MiddleDirection 
                    = m_RingDirection 
                    = m_PinkyDirection 
                    = m_ThumbDirection = 0f;
                return;
            }

            m_IndexDirection = Mathf.Lerp(m_IndexDirection, indexDirection, m_DirectionLerpSpeed);
            m_MiddleDirection = Mathf.Lerp(m_MiddleDirection, middleDirection, m_DirectionLerpSpeed);
            m_RingDirection = Mathf.Lerp(m_RingDirection, ringDirection, m_DirectionLerpSpeed);
            m_PinkyDirection = Mathf.Lerp(m_PinkyDirection, pinkyDirection, m_DirectionLerpSpeed);
            m_ThumbDirection = Mathf.Lerp(m_ThumbDirection, thumbDirection, m_DirectionLerpSpeed);

            var isTeleportActive = m_IndexDirection > m_IndexFingerDirectionThreshold
                && m_MiddleDirection < m_OtherFingersDirectionThreshold
                && m_RingDirection < m_OtherFingersDirectionThreshold
                && m_PinkyDirection < m_OtherFingersDirectionThreshold;

            // display the teleport ray visuals but not activate the teleport
            var wasTeleportActive = isTeleportActive;

            if(isTeleportActive)
            {
                State = true;
            }

            isTeleportActive &= !(m_ThumbDirection >= (m_TeleportInvokedLastFrame ? m_ThumbDirectionThresholdDot + 0.1f : m_ThumbDirectionThresholdDot));

            if(wasTeleportActive)
            {
                // Only send data to teleport value reader if teleport is pre-activated
                m_TargetRayActivateTeleportInput.QueueManualState(isTeleportActive,
                    isTeleportActive ? 1.0f : 0.0f,
                    isTeleportActive && !m_TeleportWasActiveLastFrame,
                    !isTeleportActive && m_TeleportWasActiveLastFrame);
            }

            if (!m_TeleportWasActiveLastFrame)
            {
                State = false;
            }

            m_TeleportWasActiveLastFrame = isTeleportActive;
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
            bool gotData = m_HandPoseProvider.TryGetJoint(TrackedHandJoint.ThumbProximal, out var fingerPose);
            gotData &= m_HandPoseProvider.TryGetJoint(TrackedHandJoint.Palm, out var palmPose);

            if (gotData)
            {
                var palmDirection = -palmPose.Up;
                var fingerDirection = fingerPose.Forward;

                direction = Vector3.Dot(fingerDirection, palmDirection);

                return true;
            }

            direction = 0f;
            return false;
        }
    }
}