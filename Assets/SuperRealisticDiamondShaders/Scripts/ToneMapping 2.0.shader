Shader "Hidden/ToneMap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	Flare("Flare", 2D) = "white" {}
		 Saturation("Saturation", Range(0 , 10)) = 0 // насыщеность в ярких местах 
		 _Disaturate("Disaturate", Range(0 , 1)) = 0 // насыщеность в ярких местах 
		PostExposure("PostExposure", Float) = 0 
		Contrast("Contrast", Float) = 0
		_Min("Min", Range(-1 , 3)) = 0
		_Max("Max", Range(-1 , 3)) = 1 
			 Vignette("Vignette", 2D) = "white" {}
		 //		 ScaleFlare1("ScaleFlare1", Range(7 , 0)) = 0
				 //			 ScaleFlare2("ScaleFlare2", Range(7 , 0)) = 0
				 //			 ScaleFlare3("ScaleFlare3", Range(7 , 0)) = 0
				 //	  FlareR("FlareRed", 2D) = "black" {}
	//		  FlareG("FlareGreen", 2D) = "black" {}
	//		   FlareB("FlareBlue", 2D) = "black" {}

		 
			   FlareOffsetCount("FlareOffsetCount", Float) = 30

			   FlareIntensity("FlareIntensity", Range(0 , 5)) = 0

			   Ylevel("Ylevel", Float) = 0

				   SizeTest("SizeTest", Float) = 0
		//	 VectorF("VectorF", Vector) = (0,0,0,0)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always





		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			   float4x4 clipToWorld;
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD1;
				float2 depth : TEXCOORD2;
				float3 worldDirection : TEXCOORD3;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				float4 screenPos = ComputeScreenPos(o.vertex);
				o.screenPos = screenPos;
				float4 clip = float4(o.vertex.xy, 0.0, 1.0);
				o.worldDirection = mul(clipToWorld, clip) - _WorldSpaceCameraPos;

				UNITY_TRANSFER_DEPTH(o.depth);
				return o;
			}
			float SizeTest;
			int FlareOffsetCount;
			int FlareOffsetCount2;
			float FlareIntensity;
			float FlareIntensity2;
		//	float4 VectorF;
			float ScaleFlare1;
			float ScaleFlare2;
			float ScaleFlare3;
			float PostExposure;
			float _Disaturate;
			float _Max;
			float _Min;
			float Contrast;
			sampler2D _MainTex;
			float Saturation;
			sampler2D _BlurTex;
			sampler2D _BlurTex2;
			fixed _BlurAmount;
			fixed _BlurAmount2;
			int FepthOfField;
			sampler2D _CameraDepthTexture;
			float BlurDistance;
			float BlurRange;
			sampler2D Vignette;
			sampler2D FlareR;
			sampler2D FlareG;
			sampler2D FlareB;
			sampler2D Flare;
		//	float4 Flare_ST;
			float VignetteIntensity;
			float4 FlarePositing[30];
			int ScreenSizeY;
			int ScreenSizeX;
			

			float3 mod289(float3 x)
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			float2 mod289(float2 x)
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			float3 permute(float3 x)
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}

			float4 mod289(float4 x)
			{
				return x - floor(x / 289.0) * 289.0;
			}

			float4 _permute(float4 x)
			{
				return mod289((x * 34.0 + 1.0) * x);
			}


			float4 permute(float4 x)
			{
				return fmod((34.0 * x + 1.0) * x, 289.0);
			}
			float random(in float2 st)
			{
				return frac(sin(dot(st.xy,
					float2(12.9498, 78.233)))
					* 43758.5363123);
			}


			void simplexNoise3D_float(float3 v, out float Out)
			{
				const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
				float3 i = floor(v + dot(v, C.yyy));
				float3 x0 = v - i + dot(i, C.xxx);
				float3 g = step(x0.yzx, x0.xyz);
				float3 l = 1.0 - g;
				float3 i1 = min(g.xyz, l.zxy);
				float3 i2 = max(g.xyz, l.zxy);
				float3 x1 = x0 - i1 + C.xxx;
				float3 x2 = x0 - i2 + C.yyy;
				float3 x3 = x0 - 0.5;
				i = mod289(i);
				float4 p =
					_permute(_permute(_permute(i.z + float4(0.0, i1.z, i2.z, 1.0))
						+ i.y + float4(0.0, i1.y, i2.y, 1.0))
						+ i.x + float4(0.0, i1.x, i2.x, 1.0));
				float4 j = p - 49.0 * floor(p / 49.0);
				float4 x_ = floor(j / 7.0);
				float4 y_ = floor(j - 7.0 * x_);
				float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
				float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;
				float4 h = 1.0 - abs(x) - abs(y);
				float4 b0 = float4(x.xy, y.xy);
				float4 b1 = float4(x.zw, y.zw);

				float4 s0 = floor(b0) * 2.0 + 1.0;
				float4 s1 = floor(b1) * 2.0 + 1.0;
				float4 sh = -step(h, 0.0);
				float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
				float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
				float3 g0 = float3(a0.xy, h.x);
				float3 g1 = float3(a0.zw, h.y);
				float3 g2 = float3(a1.xy, h.z);
				float3 g3 = float3(a1.zw, h.w);
				float4 norm = 1.7928429124568 - (float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3))) * 0.95312124162;
				g0 *= norm.x;
				g1 *= norm.y;
				g2 *= norm.z;
				g3 *= norm.w;
				float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
				m = m * m;
				m = m * m;
				float4 px = float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
				Out = 42.0 * dot(m, px);
			}

			float Remap(float value, float min1, float max1, float min2, float max2) {
				return (min2 + (value - min1) * (max2 - min2) / (max1 - min1));
			}

			float4 CalculateContrast(float contrastValue, float4 colorTarget)
			{
				float t = 0.5 * (1.0 - contrastValue);
				return mul(float4x4(contrastValue, 0, 0, t, 0, contrastValue, 0, t, 0, 0, contrastValue, t, 0, 0, 0, 1), colorTarget);
			}
			float3 HSVToRGB(float3 c)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
			}


			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			/*
			float4 RenderRandomFlares(sampler2D tex, float2 uv, float Size) {
				float4 Coll = 0;

				float2 tt1;

				float4 Uv = 0;
				//Uv.xy = uv;
				for (int i = 0;i < 500;i++) {
					
					
					tt1.x = random( i);
					tt1.y = random(i + 2);

					tt1 = tt1 / 2;

					
					Uv.xy = ((uv + tt1) * Size) - (Size * 0.5);
					
					

					
					if (Uv.x > 1 || Uv.y > 1|| Uv.x < 0 || Uv.y < 0) {
						Uv = 0;
					}
					

					Coll += Uv;
					
					//	Coll += Uv;


				//	if (distance(Uv, uv) < 0.3) {
			//			Coll = Coll + tex2Dlod(tex, Uv);
				//	}

				//	Coll = Coll +(  distance(Uv,uv));


				//	Coll = Coll + tex2Dlod(tex, Uv);



					
					if(distance(Uv.xy,uv) < 0.2){

					Coll = Coll + tex2Dlod(tex, Uv);

					}
					

				}

				return Coll;
			}*/

			float4 Blur(sampler2D tex, float2 uv, float2 offset, float blur_) {
				float ads = 1;
				fixed4 col =  tex2D(tex, uv);

				for (int i = 1;i < FlareOffsetCount;i++) {

					float2 ofs = float2(i * blur_ * offset);

					int ii = max(1, i);

					float3 FG = tex2D(tex, float2(uv.x + ofs.x, uv.y + ofs.y)) / lerp((ii / 1.9), 1, 0.95);

					float FlareOffsetCount_ = Remap(i, 0, FlareOffsetCount, 0.5, 1);

					//col += pow( tex2D(tex, float2(uv.x + ofs.x, uv.y + ofs.y)) / lerp((ii / 1.9), 1, 0.95), 1 + (i / 7)) / 2;
			//		col.rgb += pow(FG,  (1 + (i / 7)) / 2) + (FlareOffsetCount);

					col.rgb += FG * (1 - FlareOffsetCount_);


				}


				return col / 9;
			}

			float4 Blur2(sampler2D tex, float2 uv, float2 offset, float blur_) {
				float ads = 1;
				fixed4 col = tex2D(tex, uv);

				for (int i = 1;i < FlareOffsetCount2;i++) {

					float2 ofs = float2(i * blur_ * offset);

					int ii = max(1, i);

					float3 FG = tex2D(tex, float2(uv.x + ofs.x, uv.y + ofs.y)) / lerp((ii / 1.9), 1, 0.95);

					float FlareOffsetCount_ = Remap(i, 0, FlareOffsetCount2, 0.5, 1);

					//col += pow( tex2D(tex, float2(uv.x + ofs.x, uv.y + ofs.y)) / lerp((ii / 1.9), 1, 0.95), 1 + (i / 7)) / 2;
			//		col.rgb += pow(FG,  (1 + (i / 7)) / 2) + (FlareOffsetCount);

					col.rgb += FG * (1 - FlareOffsetCount_);


				}


				return col / 15;
			}

			float4 ToneMap(float4 MainColor,float brightness,float Disaturate,float _max,float _min,float contrast,float Satur) {

				

				

				
				fixed4 output = MainColor;
			//	output = output * brightness;
				output = output * brightness;
				output = CalculateContrast(contrast,output);

				float4 disatur = dot(output, float3(0.299, 0.587, 0.114)); // Desaturate
				output = lerp(output, disatur, clamp(pow(((output.x + output.y + output.z) / 3) * Disaturate, 1.3), 0, 1));
					output.x = clamp(Remap(output.x, 0, 1, _Min, lerp(_Max, 1, 0.5)), 0, 1.5);
				output.y = clamp(Remap(output.y, 0, 1, _Min, lerp(_Max, 1, 0.5)), 0, 1.5);
					output.z = clamp(Remap(output.z, 0, 1, _Min, lerp(_Max,1,0.5)), 0, 1.5);
				
			//	output = CalculateContrast(clamp(1 - pow((output.x + output.y + output.z) / 3, 1),0,1) * 2, output);

				



					


					output = pow(output, contrast);
				
				//output = lerp(output * (1 - pow(disatur,2)), output, 1 * lerp(max,1,0.3) );

					

				//output = lerp(output, output - 0.5,  _Middle *  clamp( distance(0.8, disatur), 0, 1));

				output = lerp(clamp(output , 0, _max), output, pow(_max,4));



				output = lerp(smoothstep(output, -0.1, 0.25), output, (1 - distance(1, _max) * 2));

				
				output = lerp(dot(output, float3(0.299, 0.587, 0.114)),output, Satur);

				output = output * lerp(brightness,1,0.75);


				return output;


			}


			fixed4 frag(v2f i) : SV_Target
			{

				

				/*

					float2 PosUVR = ((i.screenPos.xy * (ScaleFlare1 - FlarePositing[0].z)) + (float2(0.5, 0.5) - ((VectorF.xy + (FlarePositing[0] / 32)) * (ScaleFlare1 - FlarePositing[0].z))));
					float2 PosUVG = ((i.screenPos.xy * (ScaleFlare2 - FlarePositing[1].z)) + (float2(0.5, 0.5) - ((VectorF.xy + (FlarePositing[1] / 32)) * (ScaleFlare2 - FlarePositing[1].z))));
					float2 PosUVB = ((i.screenPos.xy * (ScaleFlare3 - FlarePositing[2].z)) + (float2(0.5, 0.5) - ((VectorF.xy + (FlarePositing[2] / 32)) * (ScaleFlare3 - FlarePositing[2].z))));


					
					float3 FlareRender = 0;
					float2 FlareUV = 0;

					for (int ii = 0;ii < 10;ii++) {

						FlareUV = ((i.screenPos.xy * (SizeBlood - FlarePositing[ii].z)) + (float2(0.5, 0.5) - ((VectorF.xy + (FlarePositing[ii] / 32)) * (SizeBlood - FlarePositing[ii].z))));

						FlareRender += tex2D(FlareG, FlareUV).rgb;
					}


					*/

					/*
					float Depth = clamp(pow(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenPos.xy),BlurRange) * ((BlurDistance * 1500) * pow(BlurRange,2)),0,1);
				    Depth = lerp(1, Depth, FepthOfField);



					fixed4 b = tex2D(_BlurTex, i.uv);
					*/
					fixed4 col_ = 0;

					float noise;
					float noise2;

					simplexNoise3D_float(float3(i.uv.xy * 1.5, (_WorldSpaceCameraPos.x + _WorldSpaceCameraPos.y + _WorldSpaceCameraPos.z)), noise);
					simplexNoise3D_float(float3(i.uv.xy * 0.5, (_WorldSpaceCameraPos.x + _WorldSpaceCameraPos.y + _WorldSpaceCameraPos.z) * 2), noise2);

					noise = lerp(noise, 1, 0.25);
					noise2 = lerp(noise2, 1, 0.25);

					float tt = random(_WorldSpaceCameraPos.xy);
					float tt1 = random(_WorldSpaceCameraPos.xy + 2);
					float tt2 = random(_WorldSpaceCameraPos.xy + 3);
					float tt3 = random(_WorldSpaceCameraPos.xy + 4);

					float2 camI = float2(_WorldSpaceCameraPos.x, -_WorldSpaceCameraPos.y);
					float2 camI2 = float2(-_WorldSpaceCameraPos.y, _WorldSpaceCameraPos.x);

					
					float2 CorectScreen = 1;

					CorectScreen.x = Remap(ScreenSizeX, 0, ScreenSizeY, 0, 1);

					float2 UV = i.uv;

					UV.x = UV.x * CorectScreen.x;
					
					col_ += Blur(_BlurTex, i.uv, float2(-0.1 / CorectScreen.x, -0.1) ,  _BlurAmount / 22);
					col_ += Blur(_BlurTex, i.uv, float2(0.1 / CorectScreen.x, 0.1) ,  _BlurAmount/ 22);

					col_ += Blur(_BlurTex, i.uv, float2(-0.1 / CorectScreen.x, 0.1), _BlurAmount / 22);
					col_ += Blur(_BlurTex, i.uv, float2(0.1 / CorectScreen.x, -0.1), _BlurAmount / 22);

					simplexNoise3D_float(float3(i.uv.xy * 1, (_WorldSpaceCameraPos.x + _WorldSpaceCameraPos.y + _WorldSpaceCameraPos.z)), noise);
					simplexNoise3D_float(float3(i.uv.xy * 1.5, (_WorldSpaceCameraPos.x + _WorldSpaceCameraPos.y + _WorldSpaceCameraPos.z) ), noise2);
					noise = lerp(noise, 1, 0.5);
					noise2 = lerp(noise2, 1, 0.5);


					camI = float2(_WorldSpaceCameraPos.y, _WorldSpaceCameraPos.z);
					camI2 = float2(-_WorldSpaceCameraPos.z, -_WorldSpaceCameraPos.x);


					 tt = random(_WorldSpaceCameraPos.xy + 9);
					tt1 = random(_WorldSpaceCameraPos.xy + 5);
					tt2 = random(_WorldSpaceCameraPos.yx + 3);
					 tt3 = random(_WorldSpaceCameraPos.zy + 4);

					 fixed4 col_2 = 0;

					 col_2 += Blur2(_BlurTex2, i.uv, float2(-0.1 / CorectScreen.x, -0.1), _BlurAmount2 / 22);
					 col_2 += Blur2(_BlurTex2, i.uv, float2(0.1 / CorectScreen.x, 0.1), _BlurAmount2 / 22);

					 col_2 += Blur2(_BlurTex2, i.uv, float2(-0.1 / CorectScreen.x, 0.1), _BlurAmount2 / 22);
					 col_2 += Blur2(_BlurTex2, i.uv, float2(0.1 / CorectScreen.x, -0.1), _BlurAmount2 / 22);


				//	col_ += Blur(_BlurTex, i.uv, float2(-0.1, 0.1) * VectorF.zw * float2(1, 1), _BlurAmount / 22);
					//col_ += Blur(_BlurTex, i.uv, float2(0.1, -0.1) * VectorF.zw * float2(1, 1), _BlurAmount / 22);

					

					fixed4 col = tex2D(_MainTex, i.uv);

					//	col_ = Remap(col_, 0, 10, 0, 1);

					//		float3 RGBToHSV4 = RGBToHSV(col_.rgb);
					//	float3 hsvTorgb5 = HSVToRGB(float3((RGBToHSV4 + tt).x, (RGBToHSV4.y  + (tt2 * 1.7) - (col_.r + col_.g + col_.b) ), RGBToHSV4.z));

				//	col = lerp(b, col, Depth);

				//	float4 satur = 1;

					//	satur.rgb = lerp(dot(col.rgb, float3(0.299, 0.587, 0.114)), col, col_ ); // Desaturate

				//	col = satur;

				//	col_.rgb = hsvTorgb5.rgb;


					col_2 = clamp(col_2 * FlareIntensity2, 0, 0.2) * FlareIntensity2;

					col_ = clamp(col_ * FlareIntensity, 0, 0.2) * FlareIntensity;

			//		col_ = Remap(col_, 0, 0.2, 1, FlareIntensity);





					fixed4 ColAberation = 1;
					fixed4 ColAberation2 = 1;

					//ColAberation.r = tex2D(_MainTex, i.uv).r;
			//		ColAberation.g = tex2D(_MainTex, i.uv * lerp(1, 1.01, clamp(col_ * 10, 0, 0.5))).g;
				//	ColAberation.b = tex2D(_MainTex, i.uv * lerp(1, 1.015, clamp(col_ * 10, 0, 0.5))).b;



					//		ColAberation2.r = tex2D(_MainTex, i.uv * 0.995).r;
					//		ColAberation2.g = tex2D(_MainTex, i.uv * 0.99).g;
					//	ColAberation2.b = tex2D(_MainTex, i.uv * 0.985).b;



				//	ColAberation = (ColAberation + ColAberation2)/2;
				//	col = ColAberation;

				//	col = lerp(col, ColAberation, clamp(col_ * 20, 0, 1));


					


				col = col + col_ + col_2;


					//col += col_ * FlareIntensity;

			col = ToneMap(col,PostExposure,_Disaturate,_Max,_Min,Contrast, Saturation);


			


				//col = blur2;

			//col = col - 0.5;

				// just invert the colors
				//col = 1 - col;

			
			
			//tex2D(Flare, i.screenPos.xy + FlarePositing[0].xy);              + (FlarePositing[0] / 128)

			col = col * lerp(1, tex2D(Vignette, i.screenPos.xy), VignetteIntensity);
			
			//col.rgb = max(col.rgb, tex2D(Flare, PosUV).rgb);   	col.rgb = col.rgb + tex2D(Flare, PosUV).rgb;


/*
			col.rgb = max(col.rgb, tex2D(FlareR, PosUVR).rgb);
			col.rgb = max(col.rgb, tex2D(FlareG, PosUVG).rgb);
			col.rgb = max(col.rgb, tex2D(FlareB, PosUVB).rgb);
			

			for (int ii = 0;ii < 30;ii++) {

				float2 PosUVR = ((i.screenPos.xy * (ScaleFlare1 - FlarePositing[ii].z)) + (float2(0.5, 0.5) - ((VectorF.xy + (FlarePositing[ii] / 32)) * (ScaleFlare1 - FlarePositing[ii].z))));
				col.rgb = col.rgb + (tex2D(FlareR, PosUVR).rgb * FlareIntensity);
			}*/

			//	col.rgb = col.rgb + (tex2D(FlareR, PosUVR ).rgb * FlareIntensity);
			//	col.rgb = col.rgb + (tex2D(FlareG, PosUVG).rgb * FlareIntensity);
		//	col.rgb = col.rgb + (tex2D(FlareB, PosUVB).rgb * FlareIntensity);
			



			float dist = distance(i.uv,0.5);



	//		return float4(tt, tt, tt,1);

			//	col.rgb += (FlareRender /10);
		//	return float4(_WorldSpaceCameraPos,1);
		//	col.rgb = col.rgb + 0;

			// UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthNormalsTexture, UNITY_PROJ_COORD(i.screenPos)));


		//	float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
			//depth = LinearEyeDepth(depth);
		//	float3 worldspace = i.worldDirection * depth + _WorldSpaceCameraPos;

			//	 return tex2D(_BlurTex, i.uv);


		//	col = RenderRandomFlares(Flare, i.uv, SizeTest);

			return col;

			//	return float4(depth, depth, depth, 1);
			//	return float4(i.vertex.x/5,0,0,1);

			//		
			//	return	UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));

		//	return	SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenPos.xy) * 5;

			
		//	return float4(camI, camI, camI, 1);

			
			//				return float4(tex2D(_BlurTex, i.uv).a, tex2D(_BlurTex, i.uv).a, tex2D(_BlurTex, i.uv).a,1) ;
			}
			ENDCG
		}


		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

				sampler2D _MainTex;
	sampler2D _BloomTex;
	sampler2D _BloomTex2;
	fixed4 _MainTex_TexelSize;
	fixed _BlurAmount;
	float BlurDistance;
	float BlurRange;
	float4 VectorF;
	float4x4 clipToWorld;
	float Ylevel;
	float4 PixelSize;
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);


	struct appdata_ {
		fixed4 pos : POSITION;
		fixed4 uv : TEXCOORD0;
		fixed4 uv2 : TEXCOORD1;
	};

		struct v2f
		{
			float4 uv : TEXCOORD0;
			float4 uv2 : TEXCOORD1;
			float4 vertex : SV_POSITION;
			float4 screenPos : TEXCOORD2;
			float3 worldDirection : TEXCOORD3;

		};


	v2f vert(appdata_ i)
	{
		v2f o;
		o.vertex =  UnityObjectToClipPos(i.pos);
	//	o.vertex.y = (PixelSize.y - o.vertex.y);
		

		fixed2 offset = _MainTex_TexelSize.xy;
		o.uv = i.uv;
		offset = _MainTex_TexelSize.xy * _BlurAmount * 2;
		o.uv2 = i.uv2;
		float4 screenPos = ComputeScreenPos(o.vertex);
		o.screenPos = screenPos;


		float4 clip = float4(o.vertex.xy, 0.0, 1.0);
		o.worldDirection = mul(clipToWorld, clip) - _WorldSpaceCameraPos;


		UNITY_TRANSFER_DEPTH(o.depth);
		return o;
	}
	float Remap(float value, float min1, float max1, float min2, float max2) {
		return (min2 + (value - min1) * (max2 - min2) / (max1 - min1));
	}
	float CalculateLuminance(float3 colourLinear)
	{
		// magic numbers copied from wikipedia, the source of all the best magic numbers
		// https://en.wikipedia.org/wiki/Relative_luminance
		return colourLinear.x * 0.2126 +
			colourLinear.y * 0.7152 +
			colourLinear.z * 0.0722;
	}
	/*
	float4 Blur(sampler2D tex, float2 uv, float2 offset, float blur_) {
		float ads = 1;
		fixed4 col = tex2D(tex, uv);

		for (int i = 1;i < 10;i++) {

			float2 ofs = float2(i * blur_ * offset);

			int ii = max(1, i);

			float4 FG = pow(tex2D(tex, float2(uv.x + ofs.x, uv.y + ofs.y)) / lerp((ii / 1.9), 1, 0.95), 1 + (i / 7)) / 2;

			if (lerp((FG.r + FG.g + FG.b) / 3, CalculateLuminance(FG.rgb), -30) < BlurRange) {
		//		FG = 0;
			}

			col += FG;



		}


		return col / 9;
	}
	*/

	
	
	fixed4 frag(v2f i) : COLOR
	{




		

	//		fixed4 result = 0;

	/*
		result += tex2D(_MainTex, i.uv.xw);
		result += tex2D(_MainTex, i.uv.zy);
		result += tex2D(_MainTex, i.uv.zw);


		result += tex2D(_MainTex,  i.uv2.xy);
		result += tex2D(_MainTex,  i.uv2.xw);
		result += tex2D(_MainTex,  i.uv2.zy);
		result += tex2D(_MainTex,  i.uv2.zw);

		result = (result.r + result.g + result.b) / 3;
		*/

		

	fixed4 result = tex2D(_MainTex, i.uv.xy);




	/*
	fixed4 result1 = tex2D(_MainTex, i.uv.xy + float2(-0.1, -0.1) * (VectorF.x /22));
	fixed4 result2 = tex2D(_MainTex, i.uv.xy + float2(0.1, 0.1) * (VectorF.x / 22));
	fixed4 result3 = tex2D(_MainTex, i.uv.xy + float2(-0.1, 0.1) * (VectorF.x / 22));
	fixed4 result4 = tex2D(_MainTex, i.uv.xy + float2(0.1, -0.1) * (VectorF.x / 22));

	if (lerp((result1.r + result1.g + result1.b) / 3, CalculateLuminance(result1.rgb), -50) < BlurRange) {
		result1 = 0;
	}

	if (lerp((result2.r + result2.g + result2.b) / 3, CalculateLuminance(result2.rgb), -50) < BlurRange) {
		result2 = 0;
	}

	if (lerp((result3.r + result3.g + result3.b) / 3, CalculateLuminance(result3.rgb), -50) < BlurRange) {
		result3 = 0;
	}

	if (lerp((result4.r + result4.g + result4.b) / 3, CalculateLuminance(result4.rgb), -50) < BlurRange) {
		result4 = 0;
	}



		result =  ((result + result1+ result2+ result3+ result4));
		*/

	if (lerp((result.r + result.g + result.b) / 3, CalculateLuminance(result.rgb), 1) < BlurRange) {
			result = 0;
	}


	//	result = clamp(result, 0, 2);

		float disatur = 1; 

		disatur = dot(result.rgb, float3(0.299, 0.587, 0.114)); // Desaturate

		

		result.rgb = lerp(disatur, result.rgb, 0.5);

	//	result.a = tex2D(_MainTex, i.uv.xy).r;

	//	result = clamp(result, 0, 2);

	//	result = result * clamp(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenPos.xy) * 1000,0,1);



		float4 FFF = 1;

		FFF.rgb = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));

#if ( SHADER_API_MOBILE )

		FFF.rgb = 1 - UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));

#endif
		FFF = clamp(FFF * 1000, 0, 1);



		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
		depth = LinearEyeDepth(depth);
		float3 worldspace = i.worldDirection * depth + _WorldSpaceCameraPos;

		if (worldspace.y < Ylevel) {
			result = 0;
		}

		//	discard;


	//	return float4(i.worldDirection, 1);

	//	return float4(worldspace, 1);

		result = pow(result, 2);




		//return float4((i.vertex.x) /5,0,0,1);
		return result * FFF;

	}


		/*	fixed4 frag(v2f i) : COLOR
	{
		float Depth = clamp(pow(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenPos.xy),8) * 15000,0,1);
		fixed4 c = tex2D(_MainTex, i.uv);
		fixed4 b = tex2D(_BloomTex, i.uv);
		//return Depth;
		return lerp(b,c, Depth);
		}*/

		ENDCG
		}


			/*
		
		Pass //0
		{
		  ZTest Always Cull Off ZWrite Off

		  Fog { Mode off }
CGPROGRAM
#pragma vertex vertBlur
#pragma fragment fragBloom
#pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}

			Pass //1
		{
		  ZTest Always Cull Off ZWrite Off

		  Fog { Mode off }
		  CGPROGRAM
#pragma vertex vertBlur
#pragma fragment fragBlur
#pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}*/
	}

}
