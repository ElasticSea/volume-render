Shader "Volume"
{
    Properties
    {
        _Volume ("Volume", 3D) = "white" {}
        _Alpha ("Alpha", float) = 0.01
        _AlphaThreshold ("Alpha Threshold", float) = 0.95
        _StepDistance ("Step Distance", float) = 0.01
        _MaxStepThreshold ("Max Step Threshold", int) = 512
        _ClipMin ("Clip Minimum Threashold", Range (0, 1)) = 0
        _ClipMax ("Clip Maximum Threashold", Range (0, 1)) = 1
        _CutNormal ("Cut Normal", Vector) = (0,1,0)
        _CutOrigin ("Cut Origin", Vector) = (0,0,0)
    }
    SubShader
    {
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Front
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ _CLIP_ON __ _ALPHATHRESHOLD_ON

            // Allowed floating point inaccuracy
            #define EPSILON 0.00001f

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 cameraLocalPos : TEXCOORD1;
                float4 vertexLocalPos : TEXCOORD2;
            };
            
            uniform sampler3D _Volume;
            uniform float _StepDistance;
            uniform int _MaxStepThreshold;
            uniform float _ClipMin;
            uniform float _ClipMax;
            uniform float _Alpha;
            uniform float _AlphaThreshold;
            uniform float3 _CutNormal;
            uniform float3 _CutOrigin;

            float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir) {
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                // If ray misses box, dstInsideBox will be zero
                return float2(dstToBox, dstInsideBox);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertexLocalPos = v.vertex;
                o.cameraLocalPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
                return o;
            }

            float4 BlendUnder(float4 color, float4 newColor)
            {
                color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                color.a += (1.0 - color.a) * newColor.a;
                return color;
            }
            
            float3 ClampRayToPlane(float3 from, float3 to, float3 planeOrigin, float3 planeNormal)
            {
                float3 vec = to - from;
                float denom = dot(planeNormal, vec);
                if (denom > EPSILON)
                {
                    float t = dot(planeOrigin - from, planeNormal) / denom;
                    return from + vec * clamp(t, 0, 1);
                }
                        
                return to;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Create ray
                float3 rayOrigin = i.cameraLocalPos;
                float3 rayDirection = normalize(i.vertexLocalPos - rayOrigin);
                
                // Raycast a box
                float2 rayToContainerInfo = rayBoxDst(float3(-.5,-.5,-.5), float3(.5,.5,.5), rayOrigin, 1/rayDirection);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;
                
                // Get entry exit points
                float3 entryPoint = rayOrigin + rayDirection * dstToBox;
                float3 exitPoint = entryPoint + rayDirection * dstInsideBox;
                
                // Raycast plane
                exitPoint = ClampRayToPlane(entryPoint, exitPoint, _CutOrigin, _CutNormal);
                entryPoint = ClampRayToPlane(exitPoint, entryPoint, _CutOrigin, _CutNormal);
                dstToBox = length(entryPoint - rayOrigin);
                dstInsideBox = length(exitPoint - entryPoint);
                
                dstInsideBox = min(_StepDistance * _MaxStepThreshold, dstInsideBox);
                
                float dstTravelled = 0.0;
                float4 color = 0;

                [loop]
                while (dstTravelled < dstInsideBox) {
                    rayOrigin = entryPoint + rayDirection * dstTravelled;
                    
                    float sampledValue = tex3D(_Volume, rayOrigin + float3(0.5,0.5,0.5)).r;
#ifdef _CLIP_ON
                    if(_ClipMin <= sampledValue && sampledValue <= _ClipMax){
#endif
                        float density = sampledValue * _Alpha;
                        
                        // Blend Color
                        float4 sampleColor = float4(sampledValue, sampledValue, sampledValue, density);
                        color = BlendUnder(color, sampleColor);
                        
#ifdef _ALPHATHRESHOLD_ON
                        if(color.a >= _AlphaThreshold){
                            color.a = 1;
                            break;
                        }
#endif

#ifdef _CLIP_ON
                    }
#endif
                    
                    dstTravelled += _StepDistance;
                }
                
                return color;
            }
            ENDCG
        }
    }
}
