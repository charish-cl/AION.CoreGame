Shader "Custom/GridCellHighlight_Mask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HighlightMask ("Highlight Mask", 2D) = "white" {}
        _Color ("Valid Color", Color) = (0,1,0,0.5)
        _InvalidColor ("Invalid Color", Color) = (1,0,0,0.5)
        _EdgeColor ("Edge Color", Color) = (1,1,1,0.8f)
        _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.1
        _Alpha ("Alpha", Range(0, 1)) = 0.5
        _GridSize ("Grid Size", Vector) = (50,50,0,0)
        _GridOrigin ("Grid Origin", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _HighlightMask;
            float4 _Color;
            float4 _InvalidColor;
            float4 _EdgeColor;
            float _EdgeWidth;
            float _Alpha;
            float4 _GridSize;
            float4 _GridOrigin;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // 计算世界位置
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 基础纹理
                fixed4 baseTex = tex2D(_MainTex, i.uv);

                // 计算网格坐标
                float2 worldPos2D = i.worldPos.xy;
                float2 gridPos = (worldPos2D - _GridOrigin.xy) / _GridSize.xy;

                // 从Mask纹理采样高亮信息
                fixed4 maskColor = tex2D(_HighlightMask, gridPos);

                // 判断是否高亮（R通道表示是否激活，G通道表示是否可放置）
                float isHighlighted = maskColor.r;
                float isValid = maskColor.g;

                if (isHighlighted < 0.5f)
                {
                    // 没有高亮，返回基础纹理
                    return baseTex;
                }

                // 选择高亮颜色
                fixed4 highlightColor = lerp(_InvalidColor, _Color, isValid);

                // 计算到边缘的距离（用于边框效果）
                float2 dist = min(i.uv, 1.0 - i.uv);
                float minDist = min(dist.x, dist.y);

                // 边缘效果
                float edge = smoothstep(_EdgeWidth, _EdgeWidth + 0.05, minDist);

                // 混合颜色
                fixed4 col = lerp(_EdgeColor, highlightColor, edge);
                col.a *= _Alpha;

                // 与基础纹理混合
                return lerp(baseTex, col, col.a);
            }
            ENDCG
        }
    }
}