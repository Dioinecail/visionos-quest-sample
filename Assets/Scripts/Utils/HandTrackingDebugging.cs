using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Hands;

public class HandTrackingDebugging : MonoBehaviour
{
    [SerializeField] private XRHandSkeletonDriver m_TargetDriver;
    [SerializeField] TMPro.TMP_Text m_Debug;



    private void Update()
    {
        var joints = m_TargetDriver.jointTransformReferences;
        var sb = new System.Text.StringBuilder();
        var palm = joints.FirstOrDefault(j => j.xrHandJointID == XRHandJointID.Palm);

        foreach (var j in joints)
        {
            var positionToPalmLocal = palm.jointTransform.InverseTransformPoint(j.jointTransform.position);

            sb.Append($"i: '{j.xrHandJointID}'");
            sb.Append($" | pos: '{positionToPalmLocal}'");
            sb.Append($" | rot: '{j.jointTransform.localRotation.eulerAngles}'\n");
        }

        m_Debug.text = sb.ToString();
    }

}
