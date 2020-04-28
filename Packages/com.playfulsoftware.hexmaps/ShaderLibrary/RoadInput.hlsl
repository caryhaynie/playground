#ifndef HEXMAPS_ROAD_INPUT_INCLUDED
#define HEXMAPS_ROAD_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _Color;
CBUFFER_END

TEXTURE2D(_NoiseTex);    SAMPLER(sampler_NoiseTex);

float4 Road(float2 uv, float3 worldPos)
{
    float4 noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, worldPos.xz * 0.0025);
    float4 c = _Color * (noise.y * 0.75 + 0.25);
    float4 blend = uv.x;
    blend *= noise.x + 0.5;
    blend = smoothstep(0.4, 0.7, blend);
    c.a = blend;

    return c;
}

#endif // HEXMAPS_ROAD_INPUT_INCLUDED