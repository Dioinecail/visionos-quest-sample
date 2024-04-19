using Project.Core;
using UnityEngine;

namespace Project.Hands
{
    public class HandTeleportActionVisuals : MonoBehaviour
    {
        [SerializeField] private HandTeleportAction m_TargetAction;
        [SerializeField] private AdaptiveRayVisuals m_RayLineVisuals;
        [SerializeField] private LineRenderer m_RayLine;



        private void Awake()
        {
            m_TargetAction.OnStateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(bool state)
        {
            m_RayLine.enabled = (state);
            m_RayLineVisuals.enabled = state;
        }
    }
}