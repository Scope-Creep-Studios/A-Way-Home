Shader "Custom/VisionHoleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
        _HoleCenter ("Hole Center", Vector) = (0,0,0,0)
        _Radius ("Hole Radius", Float) = 2
        _Softness ("Edge Softness", Float) = 0.2
        _HoleActive ("Hole Active", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _HoleCenter;
            float _Radius;
            float _Softness;
            float _HoleActive;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float2 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                o.worldPos =
                    mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                float distanceFromCenter =
                    distance(i.worldPos, _HoleCenter.xy);

                float radius = max(_Radius, 0.001);
                float softness = max(_Softness, 0.001);

                float hole = smoothstep(
                    radius - softness,
                    radius + softness,
                    distanceFromCenter
                );

                hole = lerp(1.0, hole, _HoleActive);

                col.a *= hole;
                return col;
            }
            ENDCG
        }
    }
}