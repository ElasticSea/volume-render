using UnityEngine;

namespace Render.Ui
{
    public class CutSettings : MonoBehaviour
    {
        [SerializeField] private Camera camera;
        [SerializeField] private VolumeRenderManager volumeRenderManager;

        private float cutProgress = 0;
        public float CutProgress
        {
            get => cutProgress;
            set
            {
                cutProgress = value;

                var normal = -camera.transform.forward;
                var position = -normal * (cutProgress * 2 - 1);
                volumeRenderManager.VolumeRender.SetCutPlane(position, normal);
            }
        }
    }
}