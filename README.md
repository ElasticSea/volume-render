![brain slice](https://user-images.githubusercontent.com/36990593/123840410-935c4780-d90e-11eb-95c1-f0c49d661f2f.png)
# Volume Render
Visualizing volumetric data in VR and AR!

## How to use

### Import volume
1) Start `_Project/Import` scene.
2) Choose a volume name.
2) Select volume source. *(only `.nii` float volumes are supported)*
3) Select volume format. For example Gray8 for grayscale and RGBA32 for color.
4) Click 'Import'.

### Use existing volume
TODO...

### Render volume in VR
1) Start `_Project/VR` scene.
2) Select volume in volumes list.
3) Click 'Load'.
4) Put on your VR headset.
5) Holding the right grip button, you can cut the volume.
6) Pressing the primary button on right controller, you toggle between render quality modes.

## Rendering

The volume occupies a space cube with size `(1, 1, 1)` at position `(0, 0, 0)`. The faces of the cube are inverted so its possible to move the camera inside the cube and render the volume.

In the fragment shader we will create a ray based on the local position and direction of the camera. Raycasting the cube we get the distance to travel inside the volume.

In order to achieve the cutting effect, the ray is clamped to the plane defined by `_CutOrigin` and `_CutNormal`

Traversing the volume in discreat steps until we cover the distance, blending the sampled color at each step.

### Shader
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
    
    // Clamp ray to plane
    exitPoint = ClampRayToPlane(entryPoint, exitPoint, _CutOrigin, _CutNormal);
    entryPoint = ClampRayToPlane(exitPoint, entryPoint, _CutOrigin, _CutNormal);
    dstToBox = length(entryPoint - rayOrigin);
    dstInsideBox = length(exitPoint - entryPoint);

    float dstTravelled = 0.0;
    float4 color = 0;
    
    // Step through the volume in small steps and blend the colors
    [loop]
    while (dstTravelled < dstInsideBox) {
        float3 samplePosition = entryPoint + rayDirection * dstTravelled;
        
        float4 sampleColor = SampleVolume(samplePosition);

        sampleColor.a *= _Alpha;
        color = BlendUnder(color, sampleColor);
            
        dstTravelled += _StepDistance;
    }
    
    return color;
}
```
### Params
| Type | Name | Description |
|-|-|-|
| sampler3D | _Volume | Used only for smaller volumes, single volume texture up to `2048x2048x2048` and `2GB` |
| sampler3D | _Volume000-111 | Used only for larger volumes split to `8` textures up to `4096x4096x4096` and `16GB` |
| float | _StepDistance | Distance betweens steps while traversing the volume, lower number takes samples faster |
| float | _ClipMin | Voxels with alpha below this threshold are discarted |
| float | _ClipMax | Voxels with alpha above this threshold are discarted |
| float | _Alpha | Each voxel's alpha is multiplied by this, change opacity of the resulting render |
| float | _AlphaThreshold | When resulting color's alpha is higher than this threshold the ray is stopped and program exists saving compute time |
| float3 | _CutNormal | Normal of plane that cuts the volume |
| float3 | _CutOrigin | Origin of plane that cuts the volume |

TODO presets with images

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


## Volumes library

- 100micron brain scan at 1263x1651x1148 at 8bit per voxel 2.23 GiB
https://xrvolumerender.azureedge.net/volumes/brain.vlm

## Source Datasets
- 100micron brain scan source:
https://datadryad.org/stash/dataset/doi:10.5061/dryad.119f80q
