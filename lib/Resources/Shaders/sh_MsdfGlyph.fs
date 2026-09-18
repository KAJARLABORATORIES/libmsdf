#ifndef MSDFGLYPH_FS
#define MSDFGLYPH_FS

#include "sh_Utils.h"
#include "sh_Masking.h"
#include "sh_TextureWrapping.h"

layout(location = 2) in mediump vec2 v_TexCoord;

layout(std140, set = 0, binding = 0) uniform m_MsdfParameters
{
    mediump float g_DistanceRange;
};

layout(set = 1, binding = 0) uniform lowp texture2D m_Texture;
layout(set = 1, binding = 1) uniform lowp sampler m_Sampler;

layout(location = 0) out vec4 o_Colour;

float medianOfThree(float r, float g, float b)
{
    return max(min(r, g), min(max(r, g), b));
}

float screenPxRange(vec2 texCoord)
{
    vec2 unitRange = vec2(g_DistanceRange) / vec2(textureSize(sampler2D(m_Texture, m_Sampler), 0));
    vec2 screenTexSize = vec2(1.0) / fwidth(texCoord);
    return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}

void main(void)
{
    vec2 wrappedCoord = wrap(v_TexCoord, v_TexRect);
    vec3 msd = texture(sampler2D(m_Texture, m_Sampler), wrappedCoord).rgb;

    float signedDistance = medianOfThree(msd.r, msd.g, msd.b) - 0.5;
    float coverage = clamp(signedDistance * screenPxRange(v_TexCoord) + 0.5, 0.0, 1.0);

    o_Colour = getRoundedColor(vec4(1.0, 1.0, 1.0, coverage), wrappedCoord);
}

#endif
