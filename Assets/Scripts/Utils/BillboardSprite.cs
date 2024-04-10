using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    [SerializeField] private bool m_Invert;

    private Transform m_CameraTransform;



    private void OnEnable()
    {
        m_CameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        var direction = m_Invert ? m_CameraTransform.position - transform.position : transform.position - m_CameraTransform.position;

        m_CameraTransform.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}
