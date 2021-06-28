Shader "Volume"
{
    Properties
    {
        _Alpha ("Alpha", float) = 0.01
        _AlphaThreshold ("Alpha Threshold", float) = 0.95
        _StepDistance ("Step Distance", float) = 0.01
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
            #pragma multi_compile _CLIP_OFF _CLIP_ON
            #pragma multi_compile _ALPHATHRESHOLD_OFF _ALPHATHRESHOLD_ON
            #pragma multi_compile _OCTVOLUME_OFF _OCTVOLUME_ON
            #pragma multi_compile _COLOR_OFF _COLOR_ON

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
            
#ifdef _OCTVOLUME_OFF
            uniform sampler3D _Volume;
            
            float4 SampleVolume(float3 pos){
                return tex3D(_Volume, pos + float3(0.5,0.5,0.5));
            }
#endif
            
#ifdef _OCTVOLUME_ON
            uniform sampler3D _Volume000;
            uniform sampler3D _Volume001;
            uniform sampler3D _Volume010;
            uniform sampler3D _Volume011;
            uniform sampler3D _Volume100;
            uniform sampler3D _Volume101;
            uniform sampler3D _Volume110;
            uniform sampler3D _Volume111;
            
            float4 SampleVolume(float3 pos){
                pos = (pos + float3(0.5,0.5,0.5)) * 2;
                if(pos.z < 1){
                    if(pos.y < 1){
                        if(pos.x < 1){
                            return tex3D(_Volume000, pos - float3(0.0, 0.0, 0.0));
                        }else{
                            return tex3D(_Volume100, pos - float3(1.0, 0.0, 0.0));
                        }
                    }else{
                        if(pos.x < 1){
                            return tex3D(_Volume010, pos - float3(0.0, 1.0, 0.0));
                        }else{
                            return tex3D(_Volume110, pos - float3(1.0, 1.0, 0.0));
                        }
                    }
                }else{
                    if(pos.y < 1){
                        if(pos.x < 1){
                            return tex3D(_Volume001, pos - float3(0.0, 0.0, 1.0));
                        }else{
                            return tex3D(_Volume101, pos - float3(1.0, 0.0, 1.0));
                        }
                    }else{
                        if(pos.x < 1){
                            return tex3D(_Volume011, pos - float3(0.0, 1.0, 1.0));
                        }else{
                            return tex3D(_Volume111, pos - float3(1.0, 1.0, 1.0));
                        }
                    }
                }
            }
#endif
            uniform float _StepDistance;
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
                
                float dstTravelled = 0.0;
                float4 color = 0;
                
                [loop]
                while (dstTravelled < dstInsideBox) {
                    float3 samplePosition = entryPoint + rayDirection * dstTravelled;
                    
#ifdef _COLOR_ON
                    float4 sampleColor = SampleVolume(samplePosition);
#endif
                    
#ifdef _COLOR_OFF
                    float sampledValue = SampleVolume(samplePosition).r;
                    float4 sampleColor = float4(sampledValue, sampledValue, sampledValue, sampledValue);
#endif

#ifdef _CLIP_ON
                    if(_ClipMin <= sampleColor.a && sampleColor.a <= _ClipMax){
#endif
                        sampleColor.a *= _Alpha;
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
