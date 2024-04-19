using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace Project.Hands
{
    public class HandSelectActionVisuals : MonoBehaviour
    {
        [SerializeField] private HandSelectAction m_TargetAction;
        [SerializeField] private XRInteractorLineVisual m_RayLine;
        [SerializeField] private LineRenderer m_Line;



        private void Awake()
        {
            m_TargetAction.OnStateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(bool state)
        {
            m_RayLine.enabled = state;
            m_Line.enabled = state;
        }
    }
}