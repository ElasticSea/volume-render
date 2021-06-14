using UnityEngine;

namespace Render
{
    public interface IVolumeRenderer
    {
        float Alpha { get; set; }
        float AlphaThreshold { get; set; }
        float StepDistance { get; set; }
        float ClipMinimumThreashold { get; set; }
        float ClipMaximumThreashold { get; set; }
        int MaxStepThreshold { get; set; }
        void SetCutPlane(Vector3 position, Vector3 normal);
    }
}