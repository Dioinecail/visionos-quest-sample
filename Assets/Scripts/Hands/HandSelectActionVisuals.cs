using UnityEngine;

namespace Project.Hands
{
    public class HandSelectActionVisuals : MonoBehaviour
    {
        [SerializeField] private HandSelectAction m_TargetAction;
        [SerializeField] private GameObject m_RayLine;



        private void Awake()
        {
            m_TargetAction.OnStateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(bool state)
        {
            m_RayLine.SetActive(state);
        }
    }
}