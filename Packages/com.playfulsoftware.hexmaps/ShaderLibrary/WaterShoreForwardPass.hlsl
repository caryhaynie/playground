#ifndef HEXMAPS_WATER_SHORE_FORWARD_PASS_INCLUDED
#define HEXMAPS_WATER_SHORE_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
    float3 positionWS   : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings WaterShoreVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv = input.texcoord;

    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);

    return output;
}

float4 WaterShoreFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float shore = input.uv.y;
    shore = sqrt(shore) * 0.9;

    float foam = Foam(shore, input.positionWS.xz);
    float waves = Waves(input.positionWS.xz);
    waves *= 1 - shore;

    // Albedo comes from a texture tinted by color
    return saturate(_Color + max(foam, waves));
}

#endif // HEXMAPS_WATER_SHORE_FORWARD_PASS_INCLUDED