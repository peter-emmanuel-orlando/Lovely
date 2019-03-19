Shader "Custom/EdgeGlow"
{
    Properties
    {
        _AlphaOffset("Alpha Offset", Range (0.0,1.0)) = 0.3
        _Matcap ("Color (RGB) Alpha (A)", 2D) = "white"
		_tex1 ("Texture2", 2D) = "white" {}
		[HDR] _ColorIllum("ColorIllum", Color) = (1,1,1,1)
        [Toggle] _PerspectiveCorrection ("Use Perspective Correction", Float) = 1.0
        _reflectionAmount("Reflection amount", Range (0.0,1.0)) = 0.3
    }
 
    SubShader
    {
    	//cull off
        Tags {  "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha



        Pass
        {
            CGPROGRAM
	    // Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members coord)
	    //#pragma exclude_renderers d3d11
	   // #pragma surface surf Lambert vertex:vert
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"


            uniform fixed _reflectionAmount;
            uniform fixed  _AlphaOffset;
            sampler2D _tex1;
	    	samplerCUBE _Cube;
            fixed4 _ColorIllum; 
            sampler2D _Matcap;
            bool _PerspectiveCorrection;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 cap : TEXCOORD0;
                float3 coord : TEXCOORD1;
            };
 
            v2f vert (appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                           
                float3 worldNorm = UnityObjectToWorldNormal(v.normal);
                float3 viewNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
 
                if (_PerspectiveCorrection)
                {
                    // get view space position of vertex
                    float3 viewPos = UnityObjectToViewPos(v.vertex);
                    float3 viewDir = normalize(viewPos);
 
                    // get vector perpendicular to both view direction and view normal
                    float3 viewCross = cross(viewDir, viewNorm);
                   
                    // swizzle perpendicular vector components to create a new perspective corrected view normal
                    viewNorm = float3(-viewCross.y, viewCross.x, 0.0);
                }
           
                o.cap = viewNorm.xy * 0.5 + 0.5;
                o.coord = -v.normal;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                col = tex2D(_tex1, i.cap) * _ColorIllum;;
                col.a = tex2D(_Matcap, i.cap).a + _AlphaOffset;

                float3 coords = normalize(i.coord);
                float4 finalColor = 1.0;
                float4 val = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, coords);
                finalColor.rgb = DecodeHDR(val, unity_SpecCube0_HDR);
                finalColor.a = 1.0;  

                return lerp(col, col + (col.a * finalColor),_reflectionAmount);//col * finalColor;
            }
 
            ENDCG
        }
    }
}