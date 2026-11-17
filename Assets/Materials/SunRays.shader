Shader "Unlit/SunRays"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _iChannel0("iChannel0 (Texture)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _iChannel0;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

//sourced from https://www.shadertoy.com/view/4sX3Wr
// MrSurly
            
            fixed4 frag (v2f i) : SV_Target
            {
                    const bool leftToRight = false;
                    float slopeSign = (leftToRight ? -1.0 : 1.0);
                    float slope1 = 5.0 * slopeSign;
                    float slope2 = 7.0 * slopeSign;	
                    
                    float2 uv = i.uv;
                    float bright = 
                    - sin(uv.y * slope1 + uv.x * 30.0+ _Time.y *3.10) *.2 
                    - sin(uv.y * slope2 + uv.x * 37.0 + _Time.y *3.10) *.1
                    - cos(              + uv.x * 2.0 * slopeSign + _Time.y *2.10) *.1 
                    - sin(              - uv.x * 5.0 * slopeSign + _Time.y * 2.0) * .3;
                    
                    float modulate = abs(cos(_Time.y*.1) *.5 + sin(_Time.y * .7)) *.15;
                    bright *= modulate;
                    float4 pix = tex2D(_iChannel0,uv);
                    pix.rgb += clamp(bright / 1.0,0.0,1.0);
                    return pix;
            }
            ENDCG
        }
    }
}
