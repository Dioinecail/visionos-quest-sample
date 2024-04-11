using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace Project.Core
{
    public class AdaptiveSnapTurnProvider : SnapTurnProvider
    {
        private XRInputModalityManager m_InputManager;
        private XRInputModalityManager.InputMode m_MotionControllerMode;
        private XRInputModalityManager.InputMode m_TrackedHandMode;



        protected new void OnEnable()
        {
            base.OnEnable();

            m_InputManager = FindObjectOfType<XRInputModalityManager>();

            if (m_InputManager != null)
            {
                m_InputManager.trackedHandModeStarted.AddListener(HandleTrackedHandModeStarted);
                m_InputManager.trackedHandModeEnded.AddListener(HandleTrackedHandModeEnded);
                m_InputManager.motionControllerModeStarted.AddListener(HandleMotionControllerModeStarted);
                m_InputManager.motionControllerModeEnded.AddListener(HandleMotionControllerModeEnded);

                if (XRInputModalityManager.currentInputMode.Value != XRInputModalityManager.InputMode.None)
                {
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
        }

        protected new void OnDisable()
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

        private void HandleMotionControllerModeStarted()
        {
            m_MotionControllerMode = XRInputModalityManager.InputMode.MotionController;

            // if motion controller tracking started, always switch to motion controllers
            leftHandTurnInput.inputSourceMode = XRInputValueReader.InputSourceMode.InputActionReference;
            rightHandTurnInput.inputSourceMode = XRInputValueReader.InputSourceMode.InputActionReference;
        }

        private void HandleMotionControllerModeEnded()
        {
            m_MotionControllerMode = XRInputModalityManager.InputMode.None;

            // if motion controllers tracking lost, fallback to hand tracking if possible
            if (m_TrackedHandMode == XRInputModalityManager.InputMode.TrackedHand)
            {
                leftHandTurnInput.inputSourceMode = XRInputValueReader.InputSourceMode.ManualValue;
                rightHandTurnInput.inputSourceMode = XRInputValueReader.InputSourceMode.ManualValue;
            }
        }

        private void HandleTrackedHandModeStarted()
        {
            m_TrackedHandMode = XRInputModalityManager.InputMode.TrackedHand;

            // Priority to motion controllers
            // since we can have hand tracking on top of controllers
            if (m_MotionControllerMode == XRInputModalityManager.InputMode.None)
            {
                leftHandTurnInput.inputSourceMode = XRInputValueReader.InputSourceMode.ManualValue;
                rightHandTurnInput.inputSourceMode = XRInputValueReader.InputSourceMode.ManualValue;
            }
        }

        private void HandleTrackedHandModeEnded()
        {
            m_TrackedHandMode = XRInputModalityManager.InputMode.None;

            // if hand tracking lost, fallback to motion controllers if possible
            if (m_MotionControllerMode == XRInputModalityManager.InputMode.MotionController)
            {
                leftHandTurnInput.inputSourceMode = XRInputValueReader.InputSourceMode.InputActionReference;
                rightHandTurnInput.inputSourceMode = XRInputValueReader.InputSourceMode.InputActionReference;
            }
        }
    }
}