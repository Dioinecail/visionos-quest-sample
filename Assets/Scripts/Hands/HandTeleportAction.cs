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
        [SerializeField] private float m_PalmUpThreshold = 0.25f;
        [SerializeField] private float m_DirectionLerpSpeed = 0.25f;
        [SerializeField] private float m_PinchOpenThreshold = 0.75f;
        [SerializeField] private float m_PinchClosedThreshold = 0.25f;
        [SerializeField] private float m_HandRaiseCameraFov = 45f;

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

            if (isFoundFingers && !CheckPalmFacingUpwards(m_PalmPose))
            {
                m_MiddleDirection
                    = m_RingDirection
                    = m_PinkyDirection = 0f;
                State = false;
                return;
            }

            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.IndexIntermediate, m_PalmPose, out var indexDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.MiddleIntermediate, m_PalmPose, out var middleDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.RingIntermediate, m_PalmPose, out var ringDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.LittleIntermediate, m_PalmPose, out var pinkyDirection);
            isFoundFingers &= TryGetThumbDirection(out var thumbDirection);
            isFoundFingers &= TryGetPinchProgress(out var isReadyToPinch, out var isPinching, out var pinchAmount);

            if (!isFoundFingers)
            {
                m_MiddleDirection
                    = m_RingDirection
                    = m_PinkyDirection = 0f;
                State = false;
                return;
            }

            m_MiddleDirection = Mathf.Lerp(m_MiddleDirection, middleDirection, m_DirectionLerpSpeed);
            m_RingDirection = Mathf.Lerp(m_RingDirection, ringDirection, m_DirectionLerpSpeed);
            m_PinkyDirection = Mathf.Lerp(m_PinkyDirection, pinkyDirection, m_DirectionLerpSpeed);

            var isTeleportActive = m_MiddleDirection < m_OtherFingersDirectionThreshold
                && m_RingDirection < m_OtherFingersDirectionThreshold
                && m_PinkyDirection < m_OtherFingersDirectionThreshold;

            // display the teleport ray visuals but not activate the teleport
            var wasTeleportActive = isTeleportActive;

            if (isTeleportActive)
            {
                State = true;
            }

            isTeleportActive &= !(pinchAmount >= (m_TeleportInvokedLastFrame ? 0.9f : 1.0f));

            if (wasTeleportActive)
            {
                // Only send data to teleport value reader if teleport is pre-activated
                m_TargetRayActivateTeleportInput.QueueManualState(isTeleportActive,
                    pinchAmount,
                    isTeleportActive && !m_TeleportWasActiveLastFrame,
                    !isTeleportActive && m_TeleportWasActiveLastFrame);
            }

            if (!m_TeleportWasActiveLastFrame)
            {
                State = false;
            }

            m_TeleportWasActiveLastFrame = isTeleportActive;
        }

        private bool CheckPalmFacingUpwards(HandJointPose m_PalmPose)
        {
            var upVector = Vector3.up;
            var palmInverseUp = -m_PalmPose.Up;

            return Vector3.Dot(palmInverseUp, upVector) > m_PalmUpThreshold;
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

        private bool TryGetPinchProgress(out bool isReadyToPinch, out bool isPinching, out float pinchAmount)
        {
            bool gotData = m_HandPoseProvider.TryGetJoint(TrackedHandJoint.Palm, out HandJointPose palm);

            // Is the hand far enough up/in view to be eligible for pinching?
            bool handIsUp = Vector3.Angle(Camera.main.transform.forward, (palm.Position - Camera.main.transform.position)) < m_HandRaiseCameraFov;

            gotData &= m_HandPoseProvider.TryGetJoint(TrackedHandJoint.ThumbTip, out HandJointPose thumbTip);
            gotData &= m_HandPoseProvider.TryGetJoint(TrackedHandJoint.IndexTip, out HandJointPose indexTip);

            // Compute index finger length (cached) for normalizing pinch thresholds to different sized hands.
            gotData &= m_HandPoseProvider.TryGetIndexFingerLength(out float indexFingerLength);

            if (!gotData)
            {
                isReadyToPinch = false;
                isPinching = false;
                pinchAmount = 0.0f;

                return false;
            }

            // Is the hand facing away from the head? Pinching is only allowed when this is true.
            m_HandPoseProvider.TryGetPalmFacingAway(out var isPalmFacingAway);

            // Possibly sqr magnitude for performance?
            // Would need to adjust thresholds so that everything works in square-norm
            float pinchDistance = Vector3.Distance(indexTip.Position, thumbTip.Position);
            float normalizedPinch = pinchDistance / indexFingerLength;

            // Is the hand in the ready-pose? Clients may choose to ignore pinch progress
            // if the hand is not yet ready to pinch.
            isReadyToPinch = handIsUp && isPalmFacingAway;

            // Are we actually fully pinching?
            isPinching = (normalizedPinch < m_PinchOpenThreshold);

            // Calculate pinch amount as the inverse lerp of the current pinch norm vs the open/closed thresholds. 
            pinchAmount = 1.0f - Mathf.InverseLerp(m_PinchClosedThreshold, m_PinchOpenThreshold, normalizedPinch);

            return gotData;
        }
    }
}