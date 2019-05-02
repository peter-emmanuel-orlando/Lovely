// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Lovely/FlatArt"{
	Properties{
		_NormalInfluence("Normals Influence", Range(-5, 5)) = 0.2
		_LightPenetration("Light Penetration", Range(-1, 1)) = 0
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump" {}
		_EmissionTex("Emission (RGB)", 2D) = "black" {}
		_Metallic("Metallic", Range(-10, 10)) = 0
		_Smoothness("Smoothness", Range(0, 1)) = 0.33
		_Occlusion("Occlusion", Float) = 1
	}
		SubShader{
			Tags { "RenderType" = "Opaque" "RenderQueue" = "Transparent"}
			LOD 200
			//Cull Off

			CGPROGRAM
			#pragma surface surf StandardToneMappedGI addshadow
			#pragma target 3.0

			#include "UnityPBSLighting.cginc"
			#include "UnityGlobalIllumination.cginc"

			half _NormalInfluence;
			half _LightPenetration;
			sampler2D _MainTex;
			sampler2D _NormalMap;
			sampler2D _EmissionTex;
			half _Metallic;
			half _Smoothness;
			half _Occlusion;

			inline half3 Lerp(half3 val1, half3 val2, half lerpFactor)
			{
				return ((val2 * lerpFactor) + (val1 * (1 - lerpFactor)));
			}
			inline half4 Lerp(half4 val1, half4 val2, half lerpFactor)
			{
				return (val2 * lerpFactor) + (val1 * (1 - lerpFactor));
			}
			inline float3 Lerp(float3 val1, float3 val2, half lerpFactor)
			{
				return (val2 * lerpFactor) + (val1 * (1 - lerpFactor));
			}


			/*inline half4 LightingStandardToneMappedGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
			{
				half4 result;
				s.Normal = Lerp(viewDir, s.Normal, _NormalInfluence);
				result = LightingStandard(s, viewDir, gi);
				s.Normal = Lerp(s.Normal, s.Occlusion, _LightPenetration); // no shadow from any angle
				half4 origOcclusion = s.Occlusion;
				s.Occlusion = Lerp(s.Occlusion, s.Normal, _LightPenetration);//dont correct occlusion if light is penetrating
				s.Occlusion = Lerp(s.Occlusion, origOcclusion, _NormalInfluence);//dont correct occlusion if doing normals
				return result;
			}*/

			inline half4 LightingStandardToneMappedGI_Deferred(SurfaceOutputStandard s, float3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
			{
				//s.Albedo = gi.light.color;
				//gi.light.color = half3(1,1,1);
				half oneMinusReflectivity;
				half3 specColor;
				s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);
				half3 norm = s.Normal;
				s.Normal = Lerp(viewDir, s.Normal, _NormalInfluence);// flattening
				half4 c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);

				s.Normal = Lerp(s.Normal, s.Occlusion, _LightPenetration); // no shadow from any angle
				c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);

				//correct occlusion. 
				//Occlusion could cause a 3d non flat effect if not manually handled here
				half4 origOcclusion = s.Occlusion;
				s.Occlusion = Lerp(s.Occlusion, s.Normal, _LightPenetration);//dont correct occlusion if light is penetrating
				s.Occlusion = Lerp(s.Occlusion, origOcclusion, _NormalInfluence);//dont correct occlusion if doing normals


				UnityStandardData data;
				data.diffuseColor = s.Albedo;
				data.occlusion = s.Occlusion;
				data.specularColor = specColor;
				data.smoothness = s.Smoothness;
				data.normalWorld = s.Normal;

				UnityStandardDataToGbuffer(data, outDiffuseOcclusion, outSpecSmoothness, outNormal);

				half4 emission = half4(s.Emission + c.rgb, 1);
				return normalize(emission);
			}

			inline void LightingStandardToneMappedGI_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
			{
				//data.light.color = half3(0, 1, 0);
				//gi.light.color = half3(0, 0, 1);
				//gi.light.dir = half3(0, 0, 0);
				//gi.light.ndotl = 1;
				//gi.light2.color = half3(0, 1, 0);
				//gi.light3.color = half3(0, 1, 0);
				//gi.indirect.diffuse = half3(1, 0, 0);
				//UnityGI_Base(data, .2, data.normalWorld);
				//data.atten = -1;
				//LightingStandard_GI(s, data, gi);
			}

			struct Input
			{
				float2 uv_MainTex;
				float2 uv_EmissionTex;
				float2 uv_NormalMap;
				float4 screenPos;
				float3 worldPos;
			};

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;      // base (diffuse or specular) color
				o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));      // tangent space normal, if written
				o.Emission = tex2D(_EmissionTex, IN.uv_EmissionTex);
				o.Metallic = _Metallic;      // 0=non-metal, 1=metal
				o.Smoothness = _Smoothness;    // 0=rough, 1=smooth
				//o.Occlusion = _Occlusion;     // occlusion (default 1)
				o.Alpha = 1;
			}
			ENDCG
				
			Pass
			{
				Tags
				{ 
					"Queue" = "Transparent" 
					"LightMode" = "Deferred" 
				}
				Blend One One

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				//#include "AutoLight.cginc"
				

				struct v2f {
					float4 pos : SV_POSITION;
					//SHADOW_COORDS(0)
				};

				v2f vert(appdata_full v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					//TRANSFER_SHADOW(o);
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					//fixed s = SHADOW_ATTENUATION(i);
					//s *= 0.1;
					return 1;//float4(0,0,0,0);
				}
				ENDCG
			} //Pass
		}
		FallBack Off
}