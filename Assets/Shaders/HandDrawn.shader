Shader "Shaders/HandDrawn"
{
    HLSLINCLUDE
    #include "Packages/com.yetman.render-pipelines.universal.postprocess/ShaderLibrary/Core.hlsl"

    TEXTURE2D_X(_MainTex);

    float _Blend;

    float4 getCol(float2 pos)
    {
        // take aspect ratio into account
        float2 uv = pos / _ScreenSize.xy;

        float4 c1 = LOAD_TEXTURE2D_X(_MainTex, pos);
        float4 e = smoothstep(float4(-0.05, -0.05, -0.05, -0.05), float4(0.0, 0.0, 0.0, 0.0), float4(uv.x, uv.y, float2(1, 1) - uv));
        c1 = lerp(float4(1, 1, 1, 0), c1, e.x * e.y * e.z * e.w);
        float d = clamp(dot(c1.xyz, float3(-.5, 1., -.5)), 0.0, 1.0);
        float4 c2 = float4(.7, .7, .7, .7);
        return min(lerp(c1, c2, 1.8 * d), .7);
    }

    float4 getColHT(float2 pos)
    {
        return smoothstep(0.795, 1.05, getCol(pos) * .8 + .2 + 1.0);
    }

    float getVal(float2 pos)
    {
        float4 c = getCol(pos);
        return pow(dot(c.xyz, float3(.333, .333, .333)), 1.) * 1.;
    }

    float2 getGrad(float2 pos, float eps)
    {
        float2 d = float2(eps, 0.);
        return float2(
            getVal(pos + d.xy) - getVal(pos - d.xy),
            getVal(pos + d.yx) - getVal(pos - d.yx)
            ) / eps / 2.;
    }

    float lum(float3 c) {
        return dot(c, float3(0.3, 0.59, 0.11));
    }

    float3 clipcolor(float3 c) {
        float l = lum(c);
        float n = min(min(c.r, c.g), c.b);
        float x = max(max(c.r, c.g), c.b);

        if (n < 0.0) {
            c.r = l + ((c.r - l) * l) / (l - n);
            c.g = l + ((c.g - l) * l) / (l - n);
            c.b = l + ((c.b - l) * l) / (l - n);
        }
        if (x > 1.25) {
            c.r = l + ((c.r - l) * (1.0 - l)) / (x - l);
            c.g = l + ((c.g - l) * (1.0 - l)) / (x - l);
            c.b = l + ((c.b - l) * (1.0 - l)) / (x - l);
        }
        return c;
    }

    float3 setlum(float3 c, float l) {
        float d = l - lum(c);
        c = c + float3(d, d, d);
        return clipcolor(0.85 * c);
    }

#define AngleNum 3
#define SampNum 9
#define PI2 6.28318530717959

    float4 HandDrawnFragmentProgram (PostProcessVaryings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
        float2 pos = uv * _ScreenSize.xy;

        float4 origColor = LOAD_TEXTURE2D_X(_MainTex, pos);
        
        float3 col = float3(0, 0, 0);
        float3 col2 = float3(0, 0, 0);
        float sum = 0.;

        for (int i = 0; i < AngleNum; i++)
        {
            float ang = PI2 / float(AngleNum) * (float(i) + 0.8);
            float2 v = float2(cos(ang), sin(ang));
            for (int j = 0; j < SampNum; j++)
            {
                float2 dpos = v.yx * float2(1, -1) * float(j);
                float2 dpos2 = (v.xy * float(j * j) / float(SampNum) * .5);
                float2 g;
                float fact;
                float fact2;
                float s = 0.65;
                float s2 = 1.;
                float2 pos2 = pos + s * dpos + s2 * dpos2;

                g = getGrad(pos2, 0.08);
                fact = dot(g, v) - .5 * abs(dot(g, v.yx * float2(1, -1)));
                fact2 = dot(normalize(g + float2(.0001, .0001)), v.yx * float2(1, -1));

                fact = clamp(fact, 0., .05);
                fact2 = abs(fact2);

                fact *= 1. - float(j) / float(SampNum);
                col += fact;
                col2 += fact2;
                sum += fact2;

            }
        }

        col /= float(SampNum * AngleNum) * 0.65 / sqrt(_ScreenSize.y);
        col2 /= sum;
        col.x *= 2.6;
        col.x = 1. - col.x;
        col.x *= col.x * col.x;

        float2 s = sin(pos.xy * .05 / sqrt(_ScreenSize.y / 720.f));
        float3 karo = float3(1, 1, 1);
        karo -= .75755 * float3(.25, .1, .1) * dot(exp(-s * s * 80.), float2(1., 1.));
        float r = length(pos - _ScreenSize.xy * .5) / _ScreenSize.x;
        
        float3 newColor = col.x * col2;// * karo;

        float3 overlayColor = float3(0.8, 0.5, .5) * origColor.rgb;

        newColor = float3(setlum(1.25 * overlayColor, lum(newColor)) * 1.0);
        newColor -= 0.75 - clamp(origColor.r + origColor.g + origColor.b, 0.0, 0.75);

        // Blend between the original and the grayscale color
        float4 color = float4(0,0,0,1);

        color.rgb = lerp(origColor.rgb, newColor.rgb, _Blend);

        return color;
    }
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment HandDrawnFragmentProgram
            ENDHLSL
        }
    }
    Fallback Off
}
