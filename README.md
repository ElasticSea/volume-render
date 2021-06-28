# Volume Render
Visualizing volumetric data in VR and AR

## Rendering

The volume occupies a space of ordinary cube with (1,1,1) length at (0,0,0). The faces of the cube are inverted so its possible to move the camera inside the cube.

In the framgnet shader we will create a ray based on the local position and direction of the camera. Raycasting the cube we get the distance to travel inside the volume.

Cutting the volume into discreat steps we will travel the volume until we cover the distance, blending the sampled color at each step.

```glsl
v2f vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.vertexLocalPos = v.vertex;
    o.cameraLocalPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));     
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    // Create a ray in local space
    float3 rayOrigin = i.cameraLocalPos;
    float3 rayDirection = normalize(i.vertexLocalPos - rayOrigin);
    
    // Raycast a box and get entry and exit points
    float2 rayToContainerInfo = rayBoxDst(float3(-.5,-.5,-.5), float3(.5,.5,.5), rayOrigin, 1/rayDirection);
    float dstToBox = rayToContainerInfo.x;
    float dstInsideBox = rayToContainerInfo.y;
    float3 entryPoint = rayOrigin + rayDirection * dstToBox;
    float3 exitPoint = entryPoint + rayDirection * dstInsideBox;
    
    float dstTravelled = 0.0;
    float4 color = 0;
    float4 samplePosition = rayOrigin; 
    
    // Step through the volume in small steps and blend the colors
    [loop]
    while (dstTravelled < dstInsideBox) {
        samplePosition = entryPoint + rayDirection * dstTravelled;
        
        float4 sampleColor = SampleVolume(rayOrigin);

        sampleColor.a *= _Alpha;
        color = BlendUnder(color, sampleColor);
            
        dstTravelled += _StepDistance;
    }
    
    return color;
}
```

Volumes are rendered using ray-marching technique where we cast a ray for each pixel of the volume on screen and accumualate color from the interects
..insert sample shader code

## Format

Application can import `.nii` volumes in float format. Volumes itselfs are saved in the `.vlm` format.

### `.vlm`

First 36 bytes include the header

| Name           | Type  | Size |
|----------------|-------|------|
| width          | int   | 4    |
| height         | int   | 4    |
| depth          | int   | 4    |
| min            | float | 4    |
| max            | float | 4    |
| format         | enum  | 4    |
| clustersWidth  | int   | 4    |
| clustersHeight | int   | 4    |
| clustersDepth  | int   | 4    |

Formats:
- Gray8 = 8bit per voxel grayscale
- Gray16 = 16bit per voxel grayscale
- RGBA32 = 32bit per voxel color and alpha
- RGBA64 = 64bit per voxel color and alpha

The rest of the file are clusters of voxels saved in sequence


## Existing datasets

100micron brain scan at 1263x1651x1148 at 8bit per voxel 2.23 GiB
https://xrvolumerender.azureedge.net/volumes/brain.vlm



brain scan source:
https://datadryad.org/stash/dataset/doi:10.5061/dryad.119f80q

### Brain

https://xrvolumerender.azureedge.net/volumes/brain.vlm

https://xrvolumerender.blob.core.windows.net/volumes/brain.vlm

## Source Data

You can find source data-set of the 100micron brain scan here:
https://datadryad.org/stash/dataset/doi:10.5061/dryad.119f80q