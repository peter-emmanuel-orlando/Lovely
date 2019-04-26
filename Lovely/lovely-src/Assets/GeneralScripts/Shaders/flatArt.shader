Shader "Lovely/FlatArt"{
	Properties{
		_NormalInfluence("Normals Influence", Range(-5, 5)) = 0.33
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_EmissionTex("Emission (RGB)", 2D) = "black" {}
		_Metallic("Metallic", Range(0, 1)) = 0
		_Smoothness("Smoothness", Range(-5, 5)) = 0.33
		_Occlusion("Normals Influence", Float) = 1
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }

			CGPROGRAM
			#pragma surface surf StandardToneMappedGI

			#include "UnityPBSLighting.cginc"

			half _NormalInfluence;
			sampler2D _MainTex;
			sampler2D _EmissionTex;

			inline half4 LightingStandardToneMappedGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
			{
				float3 prevNormal = s.Normal;
				s.Normal = ((s.Normal * _NormalInfluence) + (viewDir * (1 - _NormalInfluence))) / 2;
				half4 result = LightingStandard(s, viewDir, gi);
				s.Normal = prevNormal;
				return result;
			}

			inline half4 LightingStandardToneMappedGI_Deferred (SurfaceOutputStandard s, float3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
			{
				float3 prevNormal = s.Normal;
				s.Normal = ((s.Normal* _NormalInfluence) + (viewDir*(1- _NormalInfluence)))/2;
				half4 result = LightingStandard_Deferred(s, viewDir, gi, outDiffuseOcclusion, outSpecSmoothness, outNormal);
				s.Normal = prevNormal;
				return result;
			}

			inline void LightingStandardToneMappedGI_GI( SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
			{}

			struct Input {
				float2 uv_MainTex;
				float2 uv_EmissionTex;
			};

			void surf(Input IN, inout SurfaceOutputStandard o) {
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex);
				o.Emission = tex2D(_EmissionTex, IN.uv_EmissionTex);
			}
			ENDCG
		}
			//FallBack "Diffuse"
}