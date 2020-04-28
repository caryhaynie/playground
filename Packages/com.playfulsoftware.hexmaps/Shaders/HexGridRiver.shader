﻿Shader "Hex Grid/River"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        [HideInInspector] _Glossiness ("Smoothness", Range(0,1)) = 0.5
        [HideInInspector] _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent+1" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        LOD 200

        Pass
        {
            Name "RiverForward"

            HLSLPROGRAM
            #pragma vertex RiverVertex
            #pragma fragment RiverFragment

            #include "../ShaderLibrary/WaterInput.hlsl"
            #include "../ShaderLibrary/RiverForwardPass.hlsl"

            ENDHLSL
        }
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent+1" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "../ShaderLibrary/WaterLegacy.hlsl"

        sampler2D _NoiseTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float river = River(IN.uv_MainTex, _NoiseTex);

            // Albedo comes from a texture tinted by color
            fixed4 c = saturate(_Color + river);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
