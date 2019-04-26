Shader "Example/LightingStandardToneMappedGI"{
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_NormalInfluence("Normals Influence", Float) = 0.33
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }

			CGPROGRAM
			#pragma surface surf StandardToneMappedGI

			#include "UnityPBSLighting.cginc"

			half _NormalInfluence;
			sampler2D _MainTex;
			inline half4 LightingStandardToneMappedGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
			{
				s.Normal = viewDir;
				return LightingStandard(s, viewDir, gi);
			}

			inline half4 LightingStandardToneMappedGI_Deferred (SurfaceOutputStandard s, float3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
			{
				s.Normal = ((s.Normal* _NormalInfluence) + (viewDir*(1- _NormalInfluence)))/2;
				half4 result = LightingStandard_Deferred(s, viewDir, gi, outDiffuseOcclusion, outSpecSmoothness, outNormal);
				return result;
			}

			inline void LightingStandardToneMappedGI_GI( SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
			{}

			struct Input {
				float2 uv_MainTex;
			};

			void surf(Input IN, inout SurfaceOutputStandard o) {
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex);
			}
			ENDCG
		}
			//FallBack "Diffuse"
}