using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace Project.Hands
{
    public class HandController : MonoBehaviour
    {
        [SerializeField] private XRHandTrackingEvents m_TargetHand;
    }
}