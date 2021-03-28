using UnityEngine;

namespace Render
{
    public class VolumeRender : MonoBehaviour
    {
        private Material material;

        public Material Material
        {
            get => material;
            set
            {
                material = value;
                GetComponent<Renderer>().material = material;
            }
        }

        private void Awake()
        {
            material = GetComponent<Renderer>().material;
        }
    }
}