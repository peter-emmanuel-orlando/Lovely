// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Unlit/StandardUnlitShader"
{
    Properties
    {
        _FlatCutoutExtraAlpha("FlatCutoutExtraAlpha", Range (0.0,1.0)) = 0
        _FlatCutoutAlpha("FlatCutoutAlpha", Range (0.0,1.0)) = 1
        [HDR] _FlatCutoutTint ("FlatCutoutTint", Color) = (1,1,1,1)
   		_ZoomLevel("ZoomLevel", Range(0.0001, 100)) = 1
		_ScrollXSpeed ("X Scroll Speed", Range (-10,10)) = 0
		_ScrollYSpeed ("Y Scroll Speed", Range (-10,10)) = 2
        _FlatCutoutTex ("FlatCutoutTex", 2D) = "white"{}
        _MainTexAlpha("MainTexAlpha", Range (0.0,1.0)) = 1
        _MainTex ("MainTex", 2D) = "white"{}
        _MatcapTex ("MatcapTex", 2D) = "white"{}
        _RimLightTop ("RimLightTop", Color) = (1,1,1,1)
        _RimLightBottom ("RimLightBottom", Color) = (1,1,1,1)
        _RimLightLeft ("RimLightLeft", Color) = (1,1,1,1)
        _RimLightRight ("RimLightRight", Color) = (1,1,1,1)
        [Toggle] _EnableRimLight ("EnableRimLight", Float) = 1
        [Toggle] _BreakRimLight ("BreakRimLight", Float) = 0
        _RimLightIntensity("RimLightIntensity", Range (0,18)) = 0
        _RimLightPulseFreq("RimLightPulseFreq", Range (0,500)) = 0

    }
 
    SubShader
    {
	    Tags {  "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	    ZWrite Off
	    Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off

    	Pass
    	{
			Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _FlatCutoutTint;
            fixed _FlatCutoutAlpha;
            fixed _FlatCutoutExtraAlpha;
            sampler2D _FlatCutoutTex;
			float4 _FlatCutoutTex_ST;
		    fixed _ScrollXSpeed;
		    fixed _ScrollYSpeed;
		    fixed _ZoomLevel;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

            struct v2f
            {
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
            };

            //add outline option
 
            v2f vert (appdata v)
            {
            	v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _FlatCutoutTex);
				o.screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex));
            	return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
		    {
				fixed xscrollValue = _ScrollXSpeed * _Time;
				fixed yscrollValue = _ScrollYSpeed * _Time;
					
				fixed2 uv_Scroll_Offset = fixed2( xscrollValue, yscrollValue);

       			float2 screenUV = i.screenPos.xy / i.screenPos.w;

				fixed4 col = tex2D(_FlatCutoutTex, (screenUV + uv_Scroll_Offset) * _ZoomLevel) * _FlatCutoutTint;
				col.a *= _FlatCutoutAlpha;
				col.a += _FlatCutoutExtraAlpha;
				col.a = clamp(col.a, 0, 1);
            	return col;
            	//return fixed4(0,0,0,0);
            }
            ENDCG
    	}

        Pass
        {
            CGPROGRAM
	    	// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members coord)
	   		//#pragma exclude_renderers d3d11
	   		// #pragma surface surf Lambert vertex:vert
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"


            fixed  _MainTexAlpha;
            sampler2D _MainTex;
            sampler2D _MatcapTex;
            float4 _MainTex_ST;
       		fixed4 _RimLightTop;
       		fixed4 _RimLightBottom;
       		fixed4 _RimLightLeft;
       		fixed4 _RimLightRight;
       		bool   _EnableRimLight;
       		bool   _BreakRimLight;
       		fixed  _RimLightIntensity;
       		fixed  _RimLightPulseFreq;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 cap : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float2 uv : TEXTCOORD2;
            };
 
            v2f vert (appdata_full v)
            {
                v2f o;
                o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
                o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                           
                float3 worldNorm = UnityObjectToWorldNormal(v.normal);
                float3 viewNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
 
                // get view space position of vertex
                float3 viewPos = UnityObjectToViewPos(v.vertex);
                float3 viewDir = normalize(viewPos);
 
                // get vector perpendicular to both view direction and view normal
                float3 viewCross = cross(viewDir, viewNorm);
                   
                // swizzle perpendicular vector components to create a new perspective corrected view normal
                viewNorm = float3(-viewCross.y, viewCross.x, 0.0);
           
                o.cap = viewNorm.xy/2 + 0.5;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                col  = tex2D(_MainTex, i.uv);
                col *= tex2D(_MatcapTex, i.cap);
                col.a *= _MainTexAlpha;

                fixed rimLightIntensity;
                fixed4 rimLight = fixed4(0,0,0,0);
                if(_EnableRimLight)
                {
	                if(_RimLightPulseFreq != 0)
	               		rimLightIntensity = _RimLightIntensity * (abs((_Time % (_RimLightPulseFreq*0.001)) - _RimLightPulseFreq*0.001/2)*(2*1000/_RimLightPulseFreq));//abs((_Time % pulseFreq) - pulseFreq/2);//every 2 sec the pulse reaches max
	               	else
	               		rimLightIntensity = _RimLightIntensity;
	               	if(_BreakRimLight)
	               		rimLightIntensity *= 4;

	                rimLight += _RimLightLeft * ((1 - i.cap.x)) * float4(1,1,1,/*rimLightIntensity/5 */ pow((1 - i.cap.x), 20 - rimLightIntensity));// * .5;// - i.cap.y));
	                rimLight += _RimLightRight * ((i.cap.x)) * float4(1,1,1,/*rimLightIntensity/5 */ pow((i.cap.x), 20 - rimLightIntensity));
	                rimLight += _RimLightTop * ((i.cap.y)) * float4(1,1,1,/*rimLightIntensity/5 */ pow((i.cap.y), 20 - rimLightIntensity)); 
	                rimLight += _RimLightBottom * ((1 - i.cap.y)) * float4(1,1,1,/*rimLightIntensity/5 */ pow((1 - i.cap.y), 20 - rimLightIntensity));
	            }

                return lerp(col, rimLight, (-col.a + rimLight.a + 1)/2);//rimLight;
            }
 
            ENDCG
        }
    }
}