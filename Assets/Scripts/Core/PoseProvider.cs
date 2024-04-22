using Project.Hands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using System.Linq;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager;

namespace Project.Core
{
    public abstract class PoseProvider : MonoBehaviour
    {
        [SerializeField] protected InputMode m_TargetMode;

        public InputMode TargetMode => m_TargetMode;



        public abstract bool TryGetPose(out Pose aimPose);
    }
}