Shader "Custom/StylizedWater"
{
    Properties
    {
        _DepthColor ("Depth Color", Color) = (0.0, 0.4, 0.8, 0.9)
        _ShallowColor ("Shallow Color", Color) = (0.2, 0.7, 1.0, 0.8)
        _DepthDistance ("Depth Distance", Float) = 2.0
        
        _FoamColor ("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FoamAmount ("Foam Amount", Range(0.01, 5.0)) = 0.5
        _FoamCutoff ("Foam Edge Hardness", Range(0.0, 1.0)) = 0.5
        
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveScale ("Wave Scale", Float) = 1.0
        _WaveHeight ("Wave Height", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 screenPos    : TEXCOORD1;
                float3 positionWS   : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _DepthColor;
                half4 _ShallowColor;
                float _DepthDistance;
                
                half4 _FoamColor;
                float _FoamAmount;
                float _FoamCutoff;
                
                float _WaveSpeed;
                float _WaveScale;
                float _WaveHeight;
            CBUFFER_END

            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // Vertex animation
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float wave = sin(positionWS.x * _WaveScale + _Time.y * _WaveSpeed) + cos(positionWS.z * _WaveScale + _Time.y * _WaveSpeed);
                positionWS.y += wave * _WaveHeight;
                
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.uv = input.uv;
                
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // Calculate screen position
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                
                // Sample depth texture
                float rawDepth = SampleSceneDepth(screenUV);
                
                // Calculate eye depth based on camera projection (Orthographic vs Perspective)
                bool isOrtho = unity_OrthoParams.w == 1.0;
                float sceneZ;
                
                #if UNITY_REVERSED_Z
                    sceneZ = isOrtho ? lerp(_ProjectionParams.z, _ProjectionParams.y, rawDepth) : LinearEyeDepth(rawDepth, _ZBufferParams);
                #else
                    sceneZ = isOrtho ? lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth) : LinearEyeDepth(rawDepth, _ZBufferParams);
                #endif
                
                // Surface depth (w component is eye depth in perspective; in ortho we use view space Z)
                float surfZ = isOrtho ? -TransformWorldToView(input.positionWS).z : input.screenPos.w;
                
                float depthDiff = max(0, sceneZ - surfZ);
                
                float foam = 1.0 - saturate(depthDiff / _FoamAmount);
                float noise = sin(input.positionWS.x * 5.0 + _Time.y * 3.0) * cos(input.positionWS.z * 5.0 + _Time.y * 2.0);
                foam = saturate(foam + noise * 0.2);
                foam = step(_FoamCutoff, foam); 
                
                float depthGradient = saturate(depthDiff / _DepthDistance);
                half4 waterColor = lerp(_ShallowColor, _DepthColor, depthGradient);
                
                half4 finalColor = lerp(waterColor, _FoamColor, foam);
                
                finalColor.a = lerp(waterColor.a, 1.0, foam);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
