#ifndef HEXMAPS_RIVER_INPUT_INCLUDED
#define HEXMAPS_RIVER_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _Color;
CBUFFER_END

TEXTURE2D(_NoiseTex);    SAMPLER(sampler_NoiseTex);

#endif // HEXMAPS_RIVER_INPUT_INCLUDED