using Project.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Project.Hands
{
    // Based on ArticulatedHandController from MRTK
    public class HandSelectAction : MonoBehaviour
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

        public bool HasValidTargets => m_HasValidTargets;

        [SerializeField] private HandJointPoseProvider m_HandPoseProvider;
        [SerializeField] private AdaptiveRayInteractor m_TargetRayInteractor;
        [SerializeField] private HandTeleportAction m_TeleportAction;
        [SerializeField] private float m_PinchOpenThreshold = 0.75f;
        [SerializeField] private float m_PinchClosedThreshold = 0.25f;
        [SerializeField] private float m_HandRaiseCameraFov = 45f;
        [SerializeField] private float m_PinchThreshold = 0.7f;

        [SerializeField] private InputActionProperty m_SelectAction;
        [SerializeField] private InputActionProperty m_UIPressAction;

        private bool m_State;
        private bool m_PinchedLastFrame;
        private XRInputButtonReader m_TargetRaySelectInput;
        private XRInputButtonReader m_TargetRayActivateInput;
        private XRInputButtonReader m_TargetRayUISelectInput;
        private bool m_HasValidTargets;



        private void OnEnable()
        {
            m_TargetRayActivateInput = m_TargetRayInteractor.activateInput;
            m_TargetRaySelectInput = m_TargetRayInteractor.selectInput;
            m_TargetRayUISelectInput = m_TargetRayInteractor.uiPressInput;
        }

        void Update()
        {
            m_HasValidTargets = m_TargetRayInteractor.TryGetCurrentUIRaycastResult(out var _, out var _);
            m_HandPoseProvider.TryGetPalmFacingAway(out var isPalmFacingAway);

            bool gotPinchData = TryGetPinchProgress(out var _, out var _, out float pinchAmount);

            if (!HasValidTargets || !gotPinchData || !isPalmFacingAway || m_TeleportAction.HasValidTargets)
            {
                State = false;
                return;
            }

            if (HasValidTargets)
            {
                State = true;
            }

            // If we got pinch data, write it into our select interaction state.
            if (gotPinchData)
            {
                // Workaround for missing select actions on devices without interaction profiles
                // for hands, such as Varjo and Quest. Should be removed once we have universal
                // hand interaction profile(s) across vendors.

                // Debounce the polyfill pinch action value.
                bool isPinched = pinchAmount >= (m_PinchedLastFrame ? m_PinchThreshold : 1.0f);

                // Inject our own polyfilled state into the m_TargetRaySelectInput if no other control is bound.
                m_TargetRaySelectInput.QueueManualState(isPinched,
                    pinchAmount,
                    isPinched && !m_PinchedLastFrame,
                    !isPinched && m_PinchedLastFrame);

                // Also make sure we update the UI press state.
                m_TargetRayActivateInput.QueueManualState(isPinched,
                    pinchAmount,
                    isPinched && !m_PinchedLastFrame,
                    !isPinched && m_PinchedLastFrame);

                m_TargetRayUISelectInput.QueueManualState(isPinched,
                    pinchAmount,
                    isPinched && !m_PinchedLastFrame,
                    !isPinched && m_PinchedLastFrame);

                m_PinchedLastFrame = isPinched;
            }
        }

        public bool TryGetPinchProgress(out bool isReadyToPinch, out bool isPinching, out float pinchAmount)
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