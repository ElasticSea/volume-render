using Render;
using UnityEngine;

public class VolumePlacer : MonoBehaviour
{
    [SerializeField] private Vector3 position;
    [SerializeField] private VolumeRenderManager manager;
    
    private void Awake()
    {
        manager.OnVolumeLoaded += render =>
        {
            render.transform.position = position;
        };
    }
}
