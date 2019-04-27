Shader "Lovely/FlatArt"{
	Properties{
		_NormalInfluence("Normals Influence", Range(-5, 5)) = 0.2
		_LightPenetration("Light Penetration", Range(-1, 1)) = 0
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_EmissionTex("Emission (RGB)", 2D) = "black" {}
		_Metallic("Metallic", Range(-10, 10)) = 0
		_Smoothness("Smoothness", Range(0, 1)) = 0.33
		_Occlusion("Occlusion", Float) = 1
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }

			CGPROGRAM
			#pragma surface surf StandardToneMappedGI

			#include "UnityPBSLighting.cginc"

			half _NormalInfluence;
			half _LightPenetration;
			sampler2D _MainTex;
			sampler2D _EmissionTex;
			half _Metallic;
			half _Smoothness;
			half _Occlusion;

			inline half3 Lerp3(half3 val1, half3 val2, half lerpFactor)
			{
				return ((val2 * lerpFactor) + (val1 * (1 - lerpFactor)));
			}
			inline half4 Lerp4(half4 val1, half4 val2, half lerpFactor)
			{
				return (val2 * lerpFactor) + (val1 * (1 - lerpFactor));
			}


			/*inline half4 LightingStandardToneMappedGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
			{
				half4 result;
				s.Normal = Lerp3(viewDir, s.Normal, _NormalInfluence);
				result = LightingStandard(s, viewDir, gi);
				s.Normal = Lerp4(s.Normal, s.Occlusion, _LightPenetration); // no shadow from any angle
				half4 origOcclusion = s.Occlusion;
				s.Occlusion = Lerp4(s.Occlusion, s.Normal, _LightPenetration);//dont correct occlusion if light is penetrating
				s.Occlusion = Lerp4(s.Occlusion, origOcclusion, _NormalInfluence);//dont correct occlusion if doing normals
				return result;
			}*/

			inline half4 LightingStandardToneMappedGI_Deferred (SurfaceOutputStandard s, float3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
			{
				half4 result;
				s.Normal = Lerp3( viewDir, s.Normal, _NormalInfluence);
				result = LightingStandard_Deferred(s, viewDir, gi, outDiffuseOcclusion, outSpecSmoothness, outNormal);
				outNormal = Lerp4( outNormal, outDiffuseOcclusion, _LightPenetration); // no shadow from any angle

				half4 origOcclusion = outDiffuseOcclusion;
				outDiffuseOcclusion = Lerp4(outDiffuseOcclusion, outNormal, _LightPenetration);//dont correct occlusion if light is penetrating
				outDiffuseOcclusion = Lerp4(outDiffuseOcclusion, origOcclusion, _NormalInfluence);//dont correct occlusion if doing normals
				return result;
			}

			inline void LightingStandardToneMappedGI_GI( SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
			{
				LightingStandard_GI(s, data, gi);
			}

			struct Input {
				float2 uv_MainTex;
				float2 uv_EmissionTex;
			};

			void surf(Input IN, inout SurfaceOutputStandard o) {
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex);;      // base (diffuse or specular) color
				//float3 Normal;      // tangent space normal, if written
				o.Emission = tex2D(_EmissionTex, IN.uv_EmissionTex);
				o.Metallic = _Metallic;      // 0=non-metal, 1=metal
				o.Smoothness = _Smoothness;    // 0=rough, 1=smooth
				o.Occlusion = _Occlusion;     // occlusion (default 1)
			}
			ENDCG
		}
			//FallBack "Diffuse"
}