using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Project.Core
{
    /// <summary>
    /// XRRayInteractor that switches between XRHand posing and XRController posing
    /// </summary>
    public class AdaptiveRayInteractor : XRRayInteractor
    {
        [SerializeField] private PoseProvider[] m_AimProviders;
        [SerializeField] private PoseProvider[] m_DevicePoseProviders;

        public PoseProvider[] PoseProviders => m_AimProviders;

        private PoseProvider m_ActiveAimPoseProvider;
        private PoseProvider m_ActiveDevicePoseProvider;
        private XRInputModalityManager m_InputManager;
        private XRInputModalityManager.InputMode m_MotionControllerMode;
        private XRInputModalityManager.InputMode m_TrackedHandMode;

        private Transform m_Transform;
        private Pose m_InitialLocalAttach = Pose.identity;



        protected override void OnEnable()
        {
            base.OnEnable();

            m_InputManager = FindObjectOfType<XRInputModalityManager>();

            if (m_InputManager != null)
            {
                m_InputManager.trackedHandModeStarted.AddListener(HandleTrackedHandModeStarted);
                m_InputManager.trackedHandModeEnded.AddListener(HandleTrackedHandModeEnded);
                m_InputManager.motionControllerModeStarted.AddListener(HandleMotionControllerModeStarted);
                m_InputManager.motionControllerModeEnded.AddListener(HandleMotionControllerModeEnded);

                if (XRInputModalityManager.currentInputMode.Value != XRInputModalityManager.InputMode.None) {
                    switch (XRInputModalityManager.currentInputMode.Value)
                    {
                        case XRInputModalityManager.InputMode.TrackedHand:
                            HandleTrackedHandModeStarted();
                            break;
                        case XRInputModalityManager.InputMode.MotionController:
                            HandleMotionControllerModeStarted();
                            break;
                        default:
                            break;
                    }
                }
            }

            m_Transform = transform;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_InputManager != null)
            {
                m_InputManager.trackedHandModeStarted.RemoveListener(HandleTrackedHandModeStarted);
                m_InputManager.trackedHandModeEnded.RemoveListener(HandleTrackedHandModeEnded);
                m_InputManager.motionControllerModeStarted.RemoveListener(HandleMotionControllerModeStarted);
                m_InputManager.motionControllerModeEnded.RemoveListener(HandleMotionControllerModeEnded);
            }
        }

        private void Update()
        {
            if (m_ActiveAimPoseProvider != null && m_ActiveAimPoseProvider.TryGetPose(out var pose))
            {
                m_Transform.SetPositionAndRotation(pose.position, pose.rotation);

                if (hasSelection)
                {
                    attachTransform.localPosition = new Vector3(m_InitialLocalAttach.position.x, m_InitialLocalAttach.position.y, m_InitialLocalAttach.position.z);
                }
            }

            if (m_ActiveDevicePoseProvider != null && m_ActiveDevicePoseProvider.TryGetPose(out var devicePose))
            {
                attachTransform.rotation = devicePose.rotation;
            }
        }

        private float GetDistanceToBody(Pose pose)
        {
            if (pose.position.y > Camera.main.transform.position.y)
            {
                return Vector3.Distance(pose.position, Camera.main.transform.position);
            }
            else
            {
                Vector2 headPosXZ = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
                Vector2 pointerPosXZ = new Vector2(pose.position.x, pose.position.z);

                return Vector2.Distance(pointerPosXZ, headPosXZ);
            }
        }

        private void HandleMotionControllerModeStarted()
        {
            m_MotionControllerMode = XRInputModalityManager.InputMode.MotionController;

            // if motion controller tracking started, always switch to motion controllers
            m_ActiveDevicePoseProvider = m_DevicePoseProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.MotionController);
            m_ActiveAimPoseProvider = m_AimProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.MotionController);
            selectInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.InputActionReference;
            activateInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.InputActionReference;
            uiPressInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.InputActionReference;
        }

        private void HandleMotionControllerModeEnded()
        {
            m_MotionControllerMode = XRInputModalityManager.InputMode.None;

            // if motion controllers tracking lost, fallback to hand tracking if possible
            if (m_TrackedHandMode == XRInputModalityManager.InputMode.TrackedHand)
            {
                m_ActiveAimPoseProvider = m_AimProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.TrackedHand);
                m_ActiveDevicePoseProvider = m_DevicePoseProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.TrackedHand);
                selectInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.ManualValue;
                activateInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.ManualValue;
                uiPressInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.ManualValue;
            }
            else
            {
                m_ActiveAimPoseProvider = null;
                m_ActiveDevicePoseProvider = null;
            }
        }

        private void HandleTrackedHandModeStarted()
        {
            m_TrackedHandMode = XRInputModalityManager.InputMode.TrackedHand;

            // Priority to motion controllers
            // since we can have hand tracking on top of controllers
            if (m_MotionControllerMode == XRInputModalityManager.InputMode.None)
            {
                m_ActiveAimPoseProvider = m_AimProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.TrackedHand);
                m_ActiveDevicePoseProvider = m_DevicePoseProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.TrackedHand);
                selectInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.ManualValue;
                activateInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.ManualValue;
                uiPressInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.ManualValue;
            }
        }

        private void HandleTrackedHandModeEnded()
        {
            m_TrackedHandMode = XRInputModalityManager.InputMode.None;

            // if hand tracking lost, fallback to motion controllers if possible
            if (m_MotionControllerMode == XRInputModalityManager.InputMode.MotionController)
            {
                m_ActiveAimPoseProvider = m_AimProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.MotionController);
                m_ActiveDevicePoseProvider = m_DevicePoseProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.MotionController);
                selectInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.InputActionReference;
                activateInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.InputActionReference;
                uiPressInput.inputSourceMode = UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader.InputSourceMode.InputActionReference;
            }
            else
            {
                m_ActiveAimPoseProvider = null;
                m_ActiveDevicePoseProvider = null;
            }
        }
    }
}