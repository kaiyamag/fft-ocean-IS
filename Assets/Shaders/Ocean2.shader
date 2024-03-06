/*
* Ocean2.shader
*
* This HLSL shader applies the heightmap textures produces by the Fast Fourier Transform
* compute shader to the ocean mesh and applies a simple diffuse lighting model as
* the fragment shader. The original version of this program included Jacobbian
* foam generation in the fragment shader.
*
* Code References:
* Original project: https://github.com/gasgiant/FFT-Ocean
*
* Kaiya Magnuson, 2024
*
*/

Shader "Ocean/Ocean2"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _LOD_scale("LOD_scale", Range(1,10)) = 0
        _Roughness("Distant Roughness", Range(0,1)) = 0



        [Header(Cascade 0)]
        [HideInInspector]_Displacement_c0("Displacement C0", 2D) = "black" {}
        [HideInInspector]_Derivatives_c0("Derivatives C0", 2D) = "black" {}
        [HideInInspector]_Turbulence_c0("Turbulence C0", 2D) = "white" {}
        [Header(Cascade 1)]
        [HideInInspector]_Displacement_c1("Displacement C1", 2D) = "black" {}
        [HideInInspector]_Derivatives_c1("Derivatives C1", 2D) = "black" {}
        [HideInInspector]_Turbulence_c1("Turbulence C1", 2D) = "white" {}
        [Header(Cascade 2)]
        [HideInInspector]_Displacement_c2("Displacement C2", 2D) = "black" {}
        [HideInInspector]_Derivatives_c2("Derivatives C2", 2D) = "black" {}
        [HideInInspector]_Turbulence_c2("Turbulence C2", 2D) = "white" {}
    }
        SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma multi_compile _ MID CLOSE
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 4.0


        struct Input
        {
            float2 worldUV;
            float4 lodScales;
            float3 viewVector;
            float3 worldNormal;
            float4 screenPos;
            INTERNAL_DATA
        };

        sampler2D _Displacement_c0;
        sampler2D _Derivatives_c0;
        sampler2D _Turbulence_c0;

        sampler2D _Displacement_c1;
        sampler2D _Derivatives_c1;
        sampler2D _Turbulence_c1;

        sampler2D _Displacement_c2;
        sampler2D _Derivatives_c2;
        sampler2D _Turbulence_c2;

        float LengthScale0;
        float LengthScale1;
        float LengthScale2;
        float _LOD_scale;
        float _SSSBase;
        float _SSSScale;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 worldUV = float4(worldPos.xz, 0, 0);
            o.worldUV = worldUV.xy;

            o.viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
            float viewDist = length(o.viewVector);
            
            float lod_c0 = min(_LOD_scale * LengthScale0 / viewDist, 1);
            float lod_c1 = min(_LOD_scale * LengthScale1 / viewDist, 1);
            float lod_c2 = min(_LOD_scale * LengthScale2 / viewDist, 1);
            

            float3 displacement = 0;
            float largeWavesBias = 0;

            
            displacement += tex2Dlod(_Displacement_c0, worldUV / LengthScale0) * lod_c0;
            largeWavesBias = displacement.y;
            #if defined(MID) || defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c1, worldUV / LengthScale1) * lod_c1;
            #endif
            #if defined(CLOSE)
            displacement += tex2Dlod(_Displacement_c2, worldUV / LengthScale2) * lod_c2;
            #endif
            v.vertex.xyz += mul(unity_WorldToObject,displacement);

            o.lodScales = float4(lod_c0, lod_c1, lod_c2, max(displacement.y - largeWavesBias * 0.8 - _SSSBase, 0) / _SSSScale);
        }

        fixed4 _Color;
        float _Roughness;

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float pow5(float f)
        {
            return f * f * f * f * f;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            
            float4 derivatives = tex2D(_Derivatives_c0, IN.worldUV / LengthScale0);
            #if defined(MID) || defined(CLOSE)
            derivatives += tex2D(_Derivatives_c1, IN.worldUV / LengthScale1) * IN.lodScales.y;
            #endif

            #if defined(CLOSE)
            derivatives += tex2D(_Derivatives_c2, IN.worldUV / LengthScale2) * IN.lodScales.z;
            #endif

            float2 slope = float2(derivatives.x / (1 + derivatives.z),
                derivatives.y / (1 + derivatives.w));
            float3 worldNormal = normalize(float3(-slope.x, 1, -slope.y));

            o.Normal = WorldToTangentNormalVector(IN, worldNormal);

            // Simplified diffuse shader
            float3 viewDir = normalize(IN.viewVector);
            float fresnel = dot(worldNormal, viewDir);
            o.Albedo = _Color + (0.1 * fresnel);
            o.Smoothness = 1.0 - (0.01 * (0.1 * fresnel));
            o.Metallic = 0;
        }
        ENDCG
    }
        FallBack "Diffuse"
}