
Shader "Puzzled/Wire" {
    Properties{
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)
    }
    SubShader{
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
        LOD 100
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
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO
                };
                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;

                v2f vert (appdata_t v) {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID (v);
                    o.vertex = UnityObjectToClipPos (v.vertex);
                    o.texcoord = TRANSFORM_TEX (v.texcoord, _MainTex);
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 col = tex2D (_MainTex, i.texcoord);
                    col *= _Color;
                    // UNITY_OPAQUE_ALPHA(col.a);
                    return col;
                }
            ENDCG
        }
    }
}