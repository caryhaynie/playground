#ifndef HEXMAPS_TERRAIN_INPUT_INCLUDED
#define HEXMAPS_TERRAIN_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _Color;
CBUFFER_END

#endif // HEXMAPS_TERRAIN_INPUT_INCLUDED