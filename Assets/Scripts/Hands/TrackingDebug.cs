using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Hands;

public class TrackingDebug : MonoBehaviour
{
    [SerializeField] private TMP_Text m_DebugText;
    [SerializeField] private XRHandTrackingEvents m_LeftHandEvents;
    [SerializeField] private XRHandTrackingEvents m_RightHandEvents;

    private bool m_LeftHandTrackingState;
    private bool m_RightHandTrackingState;



    private void OnEnable()
    {
        m_LeftHandEvents.trackingAcquired.AddListener(HandleLeftHandTrackingAcquired);
        m_LeftHandEvents.trackingLost.AddListener(HandleLeftHandTrackingLost);
        m_RightHandEvents.trackingAcquired.AddListener(HandleRightHandTrackingAcquired);
        m_RightHandEvents.trackingLost.AddListener(HandleRightHandTrackingLost);
    }

    private void HandleLeftHandTrackingAcquired()
    {
        m_LeftHandTrackingState = true;
        HandleTrackingChanged();
    }

    private void HandleLeftHandTrackingLost()
    {
        m_LeftHandTrackingState = false;
        HandleTrackingChanged();
    }

    private void HandleRightHandTrackingAcquired()
    {
        m_RightHandTrackingState = true;
        HandleTrackingChanged();
    }

    private void HandleRightHandTrackingLost()
    {
        m_RightHandTrackingState = false;
        HandleTrackingChanged();
    }

    private void HandleTrackingChanged()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Tracking Left: {m_LeftHandTrackingState}");
        sb.AppendLine($"Tracking Right: {m_RightHandTrackingState}");

        m_DebugText.text = sb.ToString();
    }
}
