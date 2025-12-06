Shader "Custom/AttackRangeHighlight"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (1,0,0,0.8)
        _EdgeWidth ("Edge Width", Range(0, 0.1)) = 0.02
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _EdgeColor;
            float _EdgeWidth;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                OUT.worldPos = IN.vertex.xy;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 计算到中心的距离
                float2 center = float2(0.5, 0.5);
                float2 uv = IN.texcoord;
                float dist = distance(uv, center);
                
                // 计算边缘
                float edgeStart = 0.5 - _EdgeWidth;
                float edgeEnd = 0.5;
                
                fixed4 c = fixed4(0, 0, 0, 0);
                
                // 边缘区域
                if (dist >= edgeStart && dist <= edgeEnd)
                {
                    float edgeFactor = (dist - edgeStart) / _EdgeWidth;
                    c = lerp(_EdgeColor, fixed4(0, 0, 0, 0), edgeFactor);
                }
                // 内部区域
                else if (dist < edgeStart)
                {
                    c = _Color;
                }
                
                // 应用主纹理（如果有）
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                c.rgb = lerp(c.rgb, texColor.rgb, texColor.a);
                c.a *= texColor.a;
                
                return c * IN.color;
            }
            ENDCG
        }
    }
}

