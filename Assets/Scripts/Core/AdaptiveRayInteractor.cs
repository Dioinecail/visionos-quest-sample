using Project.Hands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Project.Core
{
    public class AdaptiveRayInteractor : XRRayInteractor
    {
        [SerializeField] private PoseProvider[] m_AimProviders;
        [SerializeField] private PoseProvider[] m_DevicePoseProviders;

        public PoseProvider[] PoseProviders => m_AimProviders;
        //public float SelectProgress => xrController.selectInteractionState.value;

        private PoseProvider m_ActiveAimPoseProvider;
        private PoseProvider m_ActiveDevicePoseProvider;
        private XRInputModalityManager m_InputManager;
        private XRInputModalityManager.InputMode m_MotionControllerMode;
        private XRInputModalityManager.InputMode m_TrackedHandMode;

        private Transform m_Transform;
        private Pose m_InitialLocalAttach = Pose.identity;
        private float m_RefDistance = 0;
        private static readonly ProfilerMarker ProcessInteractorPerfMarker = new ProfilerMarker("[Anasaea] AdaptiveRayInteractor.ProcessInteractor");
        private bool isRelaxedBeforeSelect = false;
        private bool IsTracked => true;

        protected internal float relaxationThreshold = 0.5f;



        /// <inheritdoc />
        public override bool CanHover(IXRHoverInteractable interactable)
        {
            // We stay hovering if we have selected anything.
            bool stickyHover = hasSelection && IsSelecting(interactable);
            if (stickyHover)
            {
                return true;
            }

            // We are ready to pinch if we are in the PinchReady position,
            // or if we are already selecting something.
            bool ready = isHoverActive || isSelectActive;

            // Is this a new interactable that we aren't already hovering?
            bool isNew = !IsHovering(interactable);

            // If so, should we be allowed to initiate a new hover on it?
            // This prevents us from "rolling off" one target and immediately
            // semi-pressing another.
            bool canHoverNew = !isNew;
                //|| SelectProgress < relaxationThreshold;

            return ready && base.CanHover(interactable) && canHoverNew;
        }

        /// <inheritdoc />
        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return base.CanSelect(interactable) && (!hasSelection || IsSelecting(interactable)) && isRelaxedBeforeSelect;
        }

        /// <inheritdoc />
        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            // When selection is active, force valid targets to be the current selection. This is done to ensure that selected objects remained hovered.
            if (hasSelection && isActiveAndEnabled)
            {
                targets.Clear();
                for (int i = 0; i < interactablesSelected.Count; i++)
                {
                    targets.Add(interactablesSelected[i]);
                }
            }
            else
            {
                base.GetValidTargets(targets);
            }
        }

        /// <inheritdoc />
        public override bool isHoverActive
        {
            get
            {
                // When the gaze pinch interactor is already selecting an object, use the default interactor behavior
                if (hasSelection)
                {
                    return base.isHoverActive && IsTracked;
                }
                // Otherwise, this selector is only allowed to hover if we can tell that the palm for the corresponding hand/controller is facing away from the user.
                else
                {
                    bool hoverActive = base.isHoverActive;
                    if (hoverActive)
                    {
                        if (m_ActiveAimPoseProvider is HandAimProvider handPose)
                        {
                            if (handPose.TryGetPalmFacingAway(out var isPalmFacingAway))
                            {
                                hoverActive &= isPalmFacingAway;
                            }
                        }
                    }

                    return hoverActive && IsTracked;
                }
            }
        }

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

        /// <inheritdoc />
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            using (ProcessInteractorPerfMarker.Auto())
            {
                if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                {
                    // If we've fully relaxed, we can begin hovering/selecting a new target.
                    //if (SelectProgress < relaxationThreshold)
                    //{
                    //    isRelaxedBeforeSelect = true;
                    //}
                    //// If we're not relaxed, and we aren't currently hovering or selecting anything,
                    //// we can't initiate new hovers or selections.
                    //else if (!hasHover && !hasSelection)
                    //{
                    //    isRelaxedBeforeSelect = false;
                    //}
                }
            }
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);

            m_InitialLocalAttach = new Pose(attachTransform.localPosition, attachTransform.localRotation);
            m_RefDistance = GetDistanceToBody(new Pose(transform.position, transform.rotation));
        }

        private void Update()
        {
            if (m_ActiveAimPoseProvider != null && m_ActiveAimPoseProvider.TryGetPose(out var pose))
            {
                m_Transform.SetPositionAndRotation(pose.position, pose.rotation);

                if (hasSelection)
                {
                    float distanceRatio = GetDistanceToBody(pose) / m_RefDistance;
                    attachTransform.localPosition = new Vector3(m_InitialLocalAttach.position.x, m_InitialLocalAttach.position.y, m_InitialLocalAttach.position.z * distanceRatio);
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
        }

        private void HandleMotionControllerModeEnded()
        {
            m_MotionControllerMode = XRInputModalityManager.InputMode.None;

            // if motion controllers tracking lost, fallback to hand tracking if possible
            if (m_TrackedHandMode == XRInputModalityManager.InputMode.TrackedHand)
            {
                m_ActiveAimPoseProvider = m_AimProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.TrackedHand);
                m_ActiveDevicePoseProvider = m_DevicePoseProviders.FirstOrDefault(p => p.TargetMode == XRInputModalityManager.InputMode.TrackedHand);
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
            }
            else
            {
                m_ActiveAimPoseProvider = null;
                m_ActiveDevicePoseProvider = null;
            }
        }
    }
}