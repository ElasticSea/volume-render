using System.Collections;
using System.Collections.Generic;
using ElasticSea.Framework.Extensions;
using Render;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ZoomToBounds : MonoBehaviour
{
    [SerializeField] private BoxCollider bounds;

    private void Update()
    {
        var worldCenter = bounds.transform.TransformPoint(bounds.center);
        var worldRadius = bounds.transform.TransformVector(bounds.size).Abs().magnitude / 2;
        GetComponent<Camera>().FillCameraView(worldCenter, worldRadius);
    }
}
