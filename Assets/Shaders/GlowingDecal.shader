
Shader "Puzzled/GlowingDecal" {
    Properties{
        _MainTex ("Base (RGB)", 2D) = "white" {}
         _Color ("Tint", Color) = (1,1,1,1)
    }
        SubShader{
            Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
            LOD 100
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            Pass {
                CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma target 2.0
                    #include "UnityCG.cginc"

                    struct appdata_t {
                        float4 vertex : POSITION;
                        float2 texcoord : TEXCOORD0;
                        float4 color    : COLOR;
                    };

                    struct v2f {
                        float4 vertex : SV_POSITION;
                        fixed4 color : COLOR;
                        float2 texcoord : TEXCOORD0;
                    };

                    sampler2D _MainTex;
                    float4 _MainTex_ST;
                    fixed4 _Color;

                    v2f vert (appdata_t v) {
                        v2f o;
                        UNITY_SETUP_INSTANCE_ID (v);
                        o.vertex = UnityObjectToClipPos (v.vertex);
                        o.texcoord = TRANSFORM_TEX (v.texcoord, _MainTex);
                        o.color = v.color * _Color;
                        return o;
                    }

                    fixed4 frag (v2f i) : SV_Target
                    {
                        half2 coord = i.texcoord;
                        half offset = abs(sin(_Time * 30)) * 0.02 + 0.03;
                        fixed4 col = tex2D (_MainTex, coord);
                        fixed4 col1 = tex2D (_MainTex, coord + half2(-offset, 0));
                        fixed4 col2 = tex2D (_MainTex, coord + half2(offset, 0));
                        fixed4 col3 = tex2D (_MainTex, coord + half2(    0, -offset));
                        fixed4 col4 = tex2D (_MainTex, coord + half2(    0,  offset));
                        fixed blur = max(max(col1.a,col2.a),max(col3.a,col4.a)) * 0.25;

                        col *= i.color;
                        col.a = clamp(col.a + blur * i.color,0,1);

                        return col;
                    }
                ENDCG
            }
        }
}