Shader "Custom/Portal"
{
    Properties
    {
        _InactiveTexture ("Inactive Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
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
                float2 uv : TEXCOORD0; // Adiciona as coordenadas UV
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float2 uv : TEXCOORD1; // Adiciona as coordenadas UV
            };

            sampler2D _MainTex;
            sampler2D _InactiveTexture;
            int displayMask; // set to 1 to display texture, otherwise will draw test colour
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                fixed4 portalCol = tex2D(_MainTex, uv);
                fixed4 inactiveCol = tex2D(_InactiveTexture, i.uv);
                return portalCol * displayMask + inactiveCol * (1-displayMask);
            }
            ENDCG
        }
    }
    Fallback "Standard" // for shadows
}
