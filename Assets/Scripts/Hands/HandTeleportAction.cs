using Project.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

        public bool HasValidTargets => m_ValidTargets.Count > 0;

        [SerializeField] private HandPoseProvider m_HandPoseProvider;
        [SerializeField] private AdaptiveRayInteractor m_TargetRayInteractor;
        [SerializeField] private float m_PalmUpThreshold = 0.25f;
        [SerializeField] private float m_DirectionLerpSpeed = 0.25f;
        [SerializeField] private float m_PinchOpenThreshold = 0.75f;
        [SerializeField] private float m_PinchClosedThreshold = 0.25f;
        [SerializeField] private float m_HandRaiseCameraFov = 45f;
        [SerializeField] private float m_PinchThreshold = 0.7f;
        [SerializeField] private float m_PinchSmoothing = 0.25f;

        [SerializeField] private InputActionProperty m_ActivateTeleportAction;
        [SerializeField] private InputActionProperty m_InvokeTeleportAction;

        private XRInputButtonReader m_TargetRayActivateTeleportInput;
        private bool m_TeleportInvokedLastFrame;
        private bool m_State = true;
        private float m_PinchAmount;
        private HandJointPose m_PalmPose;
        private List<IXRInteractable> m_ValidTargets = new List<IXRInteractable>();



        private void OnEnable()
        {
            m_TargetRayActivateTeleportInput = m_TargetRayInteractor.selectInput;
        }

        private void Update()
        {
            m_TargetRayInteractor.GetValidTargets(m_ValidTargets);

            var isFoundFingers = TryGetPinchProgress(
                out bool isPinchReady,
                out bool isPinching,
                out float pinchAmount
            );

            isFoundFingers &= m_HandPoseProvider.TryGetJoint(TrackedHandJoint.Palm, out var palmPose);

            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.IndexTip, palmPose, out var indexDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.MiddleTip, palmPose, out var middleDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.RingTip, palmPose, out var ringDirection);
            isFoundFingers &= TryGetFingerDirection(TrackedHandJoint.LittleTip, palmPose, out var pinkyDirection);

            m_PinchAmount = Mathf.Lerp(m_PinchAmount, pinchAmount, m_PinchSmoothing);

            isFoundFingers &= m_HandPoseProvider.TryGetPalmFacingAway(out var isPalmFacingAway);

            var isHandFist = false;

            if(isFoundFingers)
            {
                isHandFist = indexDirection < m_PalmUpThreshold
                    && middleDirection < m_PalmUpThreshold
                    && ringDirection < m_PalmUpThreshold
                    && pinkyDirection < m_PalmUpThreshold;
            }

            if (!HasValidTargets 
                || !isFoundFingers 
                || !isPalmFacingAway 
                || isHandFist)
            {
                State = false;
                return;
            }

            var isPinched = m_PinchAmount >= (m_TeleportInvokedLastFrame ? m_PinchThreshold : 0.95f);

            // Only send data to teleport value reader if teleport is pre-activated
            m_TargetRayActivateTeleportInput.QueueManualState(isPinched,
                m_PinchAmount,
                isPinched && !m_TeleportInvokedLastFrame,
                !isPinched && m_TeleportInvokedLastFrame);

            m_TeleportInvokedLastFrame = isPinched;

            if (HasValidTargets)
            {
                State = !isPinched;
            }
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