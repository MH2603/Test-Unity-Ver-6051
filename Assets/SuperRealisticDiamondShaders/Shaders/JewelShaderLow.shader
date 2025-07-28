
Shader "Diamonds/DiamondShaderLow(Max3Reflection)" {
	Properties {

		_Color ("Color", Color) = (1,1,1,0.1)
		_ShapeTex("ShapeTex", 2D) = "white" {}

		_Scale("Scale", Float) = 1
		_SizeX("SizeX", Int) = 16
		_SizeY("SizeY", Int) = 16
		_PlaneCount("PlaneCount", Int) = 6
		_MaxReflection("MaxReflection", Int) = 3
		_Environment ("Environment", CUBE) = "white" {}
		MipLevel("MipLevel", Range(0.0, 10.0)) = 0.0

		_RefractiveIndex("RefractiveIndex", Range(1,5)) = 1.5

	 [KeywordEnum( CubeMap, ReflectionProbe)] _CubeMapMode("CubeMapMode", Float) = 1
		_BaseReflection("BaseReflection", Range(0,1)) = 0.5
			Dispersion("Dispersion", Range(0,1)) = 1.5
			[Space(10)]
			DispersionR("DispersionR", Range(-0.5,1)) = 1.5
			DispersionG("DispersionG", Range(-0.5,1)) = 1.5
			DispersionB("DispersionB", Range(-0.5,1)) = 1.5

				//
	//	FresnelDispersionPower("FresnelDispersionPower", Range(-0.5,5)) = 1.5
				//	FresnelDispersionScale("FresnelDispersionScale", Range(-1.5,5)) = 1.5

			[Space(10)]
			DispersionIntensity("DispersionIntensity", Range(0,10)) = 0.5

			TotalInternalReflection("TotalInternalReflection", Range(0,1.5)) = 0.7
			Spec("spec",Range(0,10)) = 1
				//	DispersionLimitedAngle("DispersionLimitedAngle",Range(0,2)) = 0
			//	DispersionHueHandler("DispersionHueHandler",Range(0,2)) = 0.5
				//	lightEstimation("lightEstimation", Float) = 0.4
				
			//	FixedlightEstimation("FixedlightEstimation", Float) = 0.8 

				[Space(20)]
			_Scale("Scale", Float) = 1
				
				Brightness("Brightness",Range(0,2)) = 1
				Power("Power",Range(0,2)) = 1
				Contrast("Contrast",Range(0,2)) = 1
					_Disaturate("_Disaturate",Range(0,2)) = 1
				_Min("Min",Range(-1,1)) = 0
				_Max("Max",Range(0,2)) = 1
					PostExposure("PostExposure",Float) = 1

				//	ID_("INt",Float) = 1

				//		CentreIntensity("CentreIntensity",Range(0,1)) = 0

				ReflectionCube("ReflectionCube", CUBE) = "black" {}
	}
		SubShader{
			
				Tags { "RenderType" = "Jewel" }
				LOD 200

		Pass
			{
				CGPROGRAM
		// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
		#pragma exclude_renderers gles


		#pragma multi_compile _CUBEMAPMODE_CUBEMAP  _CUBEMAPMODE_REFLECTIONPROBE


					#pragma vertex vert
					#pragma fragment frag
					#include "UnityCG.cginc"
					#include "JewelShaderInc.cginc"
		#include "Lighting.cginc"

		
			 float  Spec;
		sampler2D _Pos;
		float Reflection;
		float Contrast;
		float PostExposure;
		float _Min;
		float _Max;
		float _Disaturate;
		float DispersionHueHandler;
		//	float lightEstimation;
		float ID_;
		float FresnelPower;
		float FresnelScale;
		 float ior_r = 2.408;
		 float ior_g = 2.424;
		 float ior_b = 2.432;
		 float4 _Color;
		 float lighttransmission;

		fixed4 frag(v2f i) : SV_Target
		{
			float3 cameraLocalPos;
		 
		//	cameraLocalPos = lerp(_WorldSpaceCameraPos,mul(MatrixWorldToObject, float4(_WorldSpaceCameraPos, 1)), CentreIntensity);

				//	 cameraLocalPos = mul(unity_WorldToObject, float4(cameraLocalPos,1));

				 //	 cameraLocalPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
				
				 cameraLocalPos = i.Pos2;

				float3 pos = i.Pos;

			//	pos = lerp(pos, (pos + CentrePivotDiamond2.xyz), CentreIntensity);

				//

				

				float3 localRay = normalize(pos - cameraLocalPos) ;

				float3 normal = i.Normal;
				float4 plane = float4(normal, dot(pos, normal));

				float reflectionRate = 0;
				float reflectionRate2 = 0;
				float3 reflectionRay;
				float3 refractionRay;
								
				float tmpR = _RefractiveIndex;

				//tmpR = tmpR * 2;
				float  PlaneNull;
				_MaxReflection = 3;
					CollideRayWithPlane(pos,0,localRay, plane, 1.0/tmpR, reflectionRate, reflectionRate2, reflectionRay, refractionRay, PlaneNull);
				float4 refractionColor = GetColorByRay(pos, refractionRay, tmpR, _MaxReflection, _Color, lighttransmission);
				refractionColor.w = 1.0;

			


				//	lightEstimation = Remap(lightEstimation, 0.05, 0.4, 0, 1);

			//	refractionColor = refractionColor * (1 + lightEstimation) / 2;


				//	refractionColor.rgb = refractionColor * (_LightColor0.rgb * _LightColor0.a);


				

				//float4 reflectionColor = max(texCUBE(_Environment, reflectionRay) * Spec, 2);

			//	float4 reflectionColor = pow(texCUBE(ReflectionCube, reflectionRay) * Spec * (1 + lightEstimation) / 2, 1);

				float3 _worldViewDir = UnityWorldSpaceViewDir(i.worldPos);
				_worldViewDir = normalize(_worldViewDir);

				float fresnelNdotV5 = dot(normal, _worldViewDir);
				float fresnelNode5 = (1 * pow(1.0 - fresnelNdotV5,1));


				//			float4 reflectionColor = clamp((texCUBE(ReflectionCube, reflectionRay) * Spec ) * fresnelNode5,0,1) * (1 + lightEstimation) / 2;


			//	float3 ase_worldNormal = i.ase_texcoord1.xyz;
			//	float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
			//	ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldReflection = reflect(-_worldViewDir, normal);


				float4 reflectionColor = texCUBE(ReflectionCube, ase_worldReflection) * Spec * fresnelNode5;


				////////////	float4 reflectionColor = clamp((texCUBE(ReflectionCube, reflectionRay) * Spec) * fresnelNode5, 0, 1) ;

				//float4 reflectionColor = max(texCUBE(_Environment, reflectionRay) * Spec, 2);


				
				//	return i.Color; 
			//	return float4(vertNormal, 1);
			//	return tex2D(_Pos,i.uv);
				//return reflectionColor;
				 

				float Hue = rgb2hsv(refractionColor.rgb).r;

				float Dis = dot(refractionColor.rgb, float3(0.299, 0.587, 0.114));

			//	refractionColor.rgb = lerp(refractionColor.rgb, Dis, Hue * DispersionHueHandler);

				float4 Fin = lerp(reflectionColor,  refractionColor * (1.0 - reflectionRate), 1 - reflectionColor);

				Fin = refractionColor + reflectionColor;

				//	float4 Fin = lerp(reflectionColor, refractionColor * , 1 - reflectionColor);

			//	return reflectionColor;

				/*
				if (Fin.r > 0.9) {
					Fin.rgb = Fin.rgb * 10;
				}

				if (Fin.g > 0.9) {
					Fin.rgb = Fin.rgb * 10;
				}

				if (Fin.b > 0.9) {
					Fin.rgb = Fin.rgb * 10;
				}
				*/

				//		Fin = max(reflectionRate + refractionColor * (1.0 - reflectionRate), reflectionColor);

				//	Fin = max(reflectionRate + refractionColor * (1.0 - reflectionRate), reflectionColor);

				//Fin = reflectionRate;

				//	Fin = refractionColor;

				Fin = ToneMap(Fin, PostExposure, _Disaturate, _Max, _Min, Contrast, 1);
				

				
				if (Fin.r > 1) {
					Fin.rgb = Fin.rgb * 2;
				}

				if (Fin.r > 1) {
					Fin.rgb = Fin.rgb * 2;
				}

				if (Fin.r > 1) {
					Fin.rgb = Fin.rgb * 2;
				}
				

				if ((Fin.r + Fin.g + Fin.b) > 2.9) {
			//		Fin.rgb =  5;
				}

				//		float4 norm_;

				//		for (int i = 0; i < _PlaneCount; ++i)
					//		{
					
				//	float tmpTime = CheckCollideRayWithPlane(rayStart, rayDirection, plane);


			//	}
		//		norm_ = GetUnpackedPlaneByIndex(ID_);


			//	float3x3 mT = unity_WorldToObject;

				//	return float4(unity_WorldToObject[2][0], unity_WorldToObject[2][1], unity_WorldToObject[2][2],1);




			//	return float4(i.Color);

						return Fin;

						//	return float4(cameraLocalPos.x, cameraLocalPos.x, cameraLocalPos.x,1);


			//	return lerp (Fin,pow(Fin,2) * 1.2,Contrast);



			//	 return float4(dot(Fin.rgb, float3(0.299, 0.587, 0.114)), dot(Fin.rgb, float3(0.299, 0.587, 0.114)),,1);

				//return reflectionColor*reflectionRate + refractionColor*(1.0-reflectionRate);
			}

			ENDCG
		}
		}


		SubShader{

			Tags { "RenderType" = "Jewel" }
		//	LOD 200

	Pass
		{
			CGPROGRAM
			// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
			#pragma exclude_renderers gles


			#pragma multi_compile _CUBEMAPMODE_CUBEMAP  _CUBEMAPMODE_REFLECTIONPROBE


						#pragma vertex vert
						#pragma fragment frag
						#include "UnityCG.cginc"
						#include "JewelShaderInc.cginc"
			#include "Lighting.cginc"


				 float  Spec;
			sampler2D _Pos;
			float Reflection;
			float Contrast;
			float PostExposure;
			float _Min;
			float _Max;
			float _Disaturate;
			float DispersionHueHandler;
			//	float lightEstimation;
			float ID_;
			float FresnelPower;
			float FresnelScale;
			 float ior_r = 2.408;
			 float ior_g = 2.424;
			 float ior_b = 2.432;
			 float4 _Color;
			 float lighttransmission;

			fixed4 frag(v2f i) : SV_Target
			{
				float3 cameraLocalPos;

			//	cameraLocalPos = lerp(_WorldSpaceCameraPos,mul(MatrixWorldToObject, float4(_WorldSpaceCameraPos, 1)), CentreIntensity);

					//	 cameraLocalPos = mul(unity_WorldToObject, float4(cameraLocalPos,1));

					 //	 cameraLocalPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));

					 cameraLocalPos = i.Pos2;

					float3 pos = i.Pos;

					//	pos = lerp(pos, (pos + CentrePivotDiamond2.xyz), CentreIntensity);

						//



						float3 localRay = normalize(pos - cameraLocalPos);

						float3 normal = i.Normal;
						float4 plane = float4(normal, dot(pos, normal));

						float reflectionRate = 0;
						float reflectionRate2 = 0;
						float3 reflectionRay;
						float3 refractionRay;

						float tmpR = _RefractiveIndex;

						//tmpR = tmpR * 2;
						float  PlaneNull;

							CollideRayWithPlane(pos,0,localRay, plane, 1.0 / tmpR, reflectionRate, reflectionRate2, reflectionRay, refractionRay, PlaneNull);
						float4 refractionColor = GetColorByRay(pos, refractionRay, tmpR,i.uv, _Color, lighttransmission);
						refractionColor.w = 1.0;




						//	lightEstimation = Remap(lightEstimation, 0.05, 0.4, 0, 1);

					//	refractionColor = refractionColor * (1 + lightEstimation) / 2;


						//	refractionColor.rgb = refractionColor * (_LightColor0.rgb * _LightColor0.a);




						//float4 reflectionColor = max(texCUBE(_Environment, reflectionRay) * Spec, 2);

					//	float4 reflectionColor = pow(texCUBE(ReflectionCube, reflectionRay) * Spec * (1 + lightEstimation) / 2, 1);

						float3 _worldViewDir = UnityWorldSpaceViewDir(i.worldPos);
						_worldViewDir = normalize(_worldViewDir);

						float fresnelNdotV5 = dot(normal, _worldViewDir);
						float fresnelNode5 = (1 * pow(1.0 - fresnelNdotV5,1));


						//			float4 reflectionColor = clamp((texCUBE(ReflectionCube, reflectionRay) * Spec ) * fresnelNode5,0,1) * (1 + lightEstimation) / 2;


					//	float3 ase_worldNormal = i.ase_texcoord1.xyz;
					//	float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
					//	ase_worldViewDir = normalize(ase_worldViewDir);
						float3 ase_worldReflection = reflect(-_worldViewDir, normal);


						float4 reflectionColor = texCUBE(ReflectionCube, ase_worldReflection) * Spec * fresnelNode5;


						////////////	float4 reflectionColor = clamp((texCUBE(ReflectionCube, reflectionRay) * Spec) * fresnelNode5, 0, 1) ;

						//float4 reflectionColor = max(texCUBE(_Environment, reflectionRay) * Spec, 2);



						//	return i.Color; 
					//	return float4(vertNormal, 1);
					//	return tex2D(_Pos,i.uv);
						//return reflectionColor;


						float Hue = rgb2hsv(refractionColor.rgb).r;

						float Dis = dot(refractionColor.rgb, float3(0.299, 0.587, 0.114));

						//	refractionColor.rgb = lerp(refractionColor.rgb, Dis, Hue * DispersionHueHandler);

							float4 Fin = lerp(reflectionColor,  refractionColor * (1.0 - reflectionRate), 1 - reflectionColor);

							Fin = refractionColor + reflectionColor;

							//	float4 Fin = lerp(reflectionColor, refractionColor * , 1 - reflectionColor);

						//	return reflectionColor;

							/*
							if (Fin.r > 0.9) {
								Fin.rgb = Fin.rgb * 10;
							}

							if (Fin.g > 0.9) {
								Fin.rgb = Fin.rgb * 10;
							}

							if (Fin.b > 0.9) {
								Fin.rgb = Fin.rgb * 10;
							}
							*/

							//		Fin = max(reflectionRate + refractionColor * (1.0 - reflectionRate), reflectionColor);

							//	Fin = max(reflectionRate + refractionColor * (1.0 - reflectionRate), reflectionColor);

							//Fin = reflectionRate;

							//	Fin = refractionColor;

							Fin = ToneMap(Fin, PostExposure, _Disaturate, _Max, _Min, Contrast, 1);



							if (Fin.r > 1) {
								Fin.rgb = Fin.rgb * 2;
							}

							if (Fin.r > 1) {
								Fin.rgb = Fin.rgb * 2;
							}

							if (Fin.r > 1) {
								Fin.rgb = Fin.rgb * 2;
							}


							if ((Fin.r + Fin.g + Fin.b) > 2.9) {
								//		Fin.rgb =  5;
									}

							//		float4 norm_;

							//		for (int i = 0; i < _PlaneCount; ++i)
								//		{

							//	float tmpTime = CheckCollideRayWithPlane(rayStart, rayDirection, plane);


						//	}
					//		norm_ = GetUnpackedPlaneByIndex(ID_);


						//	float3x3 mT = unity_WorldToObject;

							//	return float4(unity_WorldToObject[2][0], unity_WorldToObject[2][1], unity_WorldToObject[2][2],1);




							return float4(1,0,0,1);

							//	return Fin;

									//	return float4(cameraLocalPos.x, cameraLocalPos.x, cameraLocalPos.x,1);


						//	return lerp (Fin,pow(Fin,2) * 1.2,Contrast);



						//	 return float4(dot(Fin.rgb, float3(0.299, 0.587, 0.114)), dot(Fin.rgb, float3(0.299, 0.587, 0.114)),,1);

							//return reflectionColor*reflectionRate + refractionColor*(1.0-reflectionRate);
						}

						ENDCG
					}
		}


		/*
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"


			float4 CentrePivotDiamond;
float4 CentrePivotDiamond2;
float CentreIntensity;
float4x4 MatrixWorldToObject;
float4x4 MatrixWorldToObject2;
float4 CentreModel;

			struct v2f {
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				float4 pos = v.vertex;

				pos.xyz = lerp(pos.xyz, (pos.xyz - CentreModel.xyz), CentreIntensity);

				o.pos = UnityObjectToClipPos(pos);



				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					o.pos = UnityObjectToClipPos(pos);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}

	}*/
	
		


	FallBack "Diffuse"
}