Shader "Custom/GridCellHighlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (1,1,1,1)
        _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.1
        _Alpha ("Alpha", Range(0, 1)) = 0.5
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
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _EdgeColor;
            float _EdgeWidth;
            float _Alpha;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 如果边缘宽度为0，直接返回纯色（类似Gizmos效果）
                if (_EdgeWidth <= 0.001)
                {
                    fixed4 col = _Color;
                    col.a *= _Alpha;
                    return col;
                }
                
                // 计算到边缘的距离
                float2 dist = min(i.uv, 1.0 - i.uv);
                float minDist = min(dist.x, dist.y);
                
                // 边缘效果（可选，默认关闭以获得纯色效果）
                float edge = smoothstep(_EdgeWidth, _EdgeWidth + 0.05, minDist);
                
                // 混合颜色
                fixed4 col = lerp(_EdgeColor, _Color, edge);
                col.a *= _Alpha;
                
                return col;
            }
            ENDCG
        }
    }
}

