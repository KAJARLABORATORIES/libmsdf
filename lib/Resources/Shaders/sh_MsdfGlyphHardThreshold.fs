#ifndef MSDFGLYPHHARDTHRESHOLD_FS
#define MSDFGLYPHHARDTHRESHOLD_FS

#include "sh_Utils.h"
#include "sh_Masking.h"
#include "sh_TextureWrapping.h"

layout(location = 2) in mediump vec2 v_TexCoord;

layout(set = 0, binding = 0) uniform lowp texture2D m_Texture;
layout(set = 0, binding = 1) uniform lowp sampler m_Sampler;

layout(location = 0) out vec4 o_Colour;

float medianOfThree(float r, float g, float b)
{
    return max(min(r, g), min(max(r, g), b));
}

void main(void)
{
    vec2 wrappedCoord = wrap(v_TexCoord, v_TexRect);
    vec3 msd = texture(sampler2D(m_Texture, m_Sampler), wrappedCoord).rgb;

    float coverage = medianOfThree(msd.r, msd.g, msd.b) > 0.5 ? 1.0 : 0.0;

    o_Colour = getRoundedColor(vec4(1.0, 1.0, 1.0, coverage), wrappedCoord);
}

#endif
