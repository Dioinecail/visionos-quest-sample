using Project.Hands;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace Project.Core
{
    public class AdaptireRayVisuals : XRInteractorLineVisual
    {
        [SerializeField] private HandTeleportAction m_TeleportAction;
        [SerializeField] private LineRenderer m_LineVisual;



        private new void OnEnable()
        {
            base.OnEnable();

            m_TeleportAction.OnStateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(bool state)
        {
            //m_LineVisual.enabled = state;
        }
    }
}