Shader "Custom/GridRenderer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridStateTex ("Grid State Texture", 2D) = "white" {}
        _CellSize ("Cell Size", Vector) = (1, 1, 0, 0)
        _GridOrigin ("Grid Origin", Vector) = (0, 0, 0, 0)
        _GridSize ("Grid Size", Vector) = (50, 50, 0, 0)
        _ValidColor ("Valid Color", Color) = (0, 1, 0, 0.3)
        _InvalidColor ("Invalid Color", Color) = (1, 0, 0, 0.3)
        _OccupiedColor ("Occupied Color", Color) = (0.5, 0.5, 0.5, 0.3)
        _GridLineColor ("Grid Line Color", Color) = (1, 1, 1, 0.3)
        _DragHighlightColor ("Drag Highlight Color", Color) = (0, 0.7, 0, 0.6)
        _ShowGridLines ("Show Grid Lines", Float) = 1
        _LineWidth ("Line Width", Range(0, 0.1)) = 0.02
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
                float2 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            sampler2D _GridStateTex;
            sampler2D _HighlightTex;
            float4 _MainTex_ST;
            float4 _CellSize;
            float4 _GridOrigin;
            float4 _GridSize;
            float4 _ValidColor;
            float4 _InvalidColor;
            float4 _OccupiedColor;
            float4 _GridLineColor;
            float4 _DragHighlightColor;
            float _ShowGridLines;
            float _LineWidth;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // 获取世界坐标（XY平面）
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xy;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 使用世界坐标
                float2 worldPos = i.worldPos;
                
                // 计算相对于网格原点的位置
                float2 localPos = worldPos - _GridOrigin.xy;
                
                // 计算网格坐标（浮点数）
                float2 gridCoord = localPos / _CellSize.xy;
                
                // 计算当前cell的索引
                int2 cellIndex = int2(floor(gridCoord.x), floor(gridCoord.y));
                
                // 检查是否在网格范围内
                if (cellIndex.x < 0 || cellIndex.x >= _GridSize.x || 
                    cellIndex.y < 0 || cellIndex.y >= _GridSize.y)
                {
                    discard;
                }
                
                // 从纹理中读取cell状态
                // 纹理坐标：每个像素代表一个cell
                float2 stateUV = float2(
                    (cellIndex.x + 0.5) / _GridSize.x,
                    (cellIndex.y + 0.5) / _GridSize.y
                );
                float4 state = tex2D(_GridStateTex, stateUV);
                float4 highlight = tex2D(_HighlightTex, stateUV);
                
                // state.r = isOccupied (1.0 = occupied, 0.0 = not occupied)
                // state.g = isPlaceable (1.0 = placeable, 0.0 = not placeable)
                // highlight.b = isDragHighlighted (1.0 = highlighted, 0.0 = not highlighted)
                
                // 根据状态选择颜色
                fixed4 cellColor;
                if (highlight.b > 0.5) // 拖拽高亮优先显示
                {
                    cellColor = _DragHighlightColor;
                }
                else if (state.r > 0.5) // isOccupied
                {
                    cellColor = _OccupiedColor;
                }
                else if (state.g < 0.5) // !isPlaceable
                {
                    cellColor = _InvalidColor;
                }
                else // isPlaceable && !isOccupied
                {
                    cellColor = _ValidColor;
                }
                
                // 绘制网格线
                fixed4 finalColor = cellColor;
                if (_ShowGridLines > 0.5)
                {
                    // 计算到cell边界的距离
                    float2 cellLocalPos = frac(gridCoord);
                    float2 distToEdge = min(cellLocalPos, 1.0 - cellLocalPos);
                    float minDist = min(distToEdge.x, distToEdge.y);
                    
                    // 如果接近边界，绘制网格线
                    if (minDist < _LineWidth)
                    {
                        finalColor = lerp(_GridLineColor, cellColor, minDist / _LineWidth);
                    }
                }
                
                return finalColor;
            }
            ENDCG
        }
    }
}

