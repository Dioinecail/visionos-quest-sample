using Unity.XR.CoreUtils;
using UnityEngine;

namespace Project.Utils
{
    public class CameraOffsetSwitcher : MonoBehaviour
    {
        [SerializeField] private XROrigin m_Origin;



        private void Awake()
        {
            if (Application.platform == RuntimePlatform.VisionOS)
            {
                m_Origin.CameraYOffset = 0f;
            }
        }
    }
}