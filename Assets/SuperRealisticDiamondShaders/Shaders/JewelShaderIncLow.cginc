// Upgrade NOTE: replaced 'UNITY_PASS_TEXCUBE(unity_SpecCube1)' with 'UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0)'


fixed Test_;

			struct appdata
			{
				fixed4 vertex : POSITION;
				fixed3 normal : NORMAL0;
				fixed2 uv : TEXCOORD0;
    fixed2 uv2 : TEXCOORD1;
    fixed2 uv3 : TEXCOORD2;
    fixed2 uv4 : TEXCOORD3;
    fixed2 uv5 : TEXCOORD4;
    fixed4 Color : COLOR;
    uint id : SV_VertexID;
    fixed4 tangent : TANGENT;

};




fixed CentreIntensity;
fixed4x4 MatrixWorldToObject;
fixed4x4 MatrixWorldToObject2;
fixed4 CentreModel;
fixed lightEstimation2;
fixed MipLevel;

samplerCUBE _Environment;
fixed4 _Environment_HDR;
fixed FixedlightEstimation;
fixed ColorIntensity;
			struct v2f 
			{
				fixed2 uv : TEXCOORD0; 
    fixed2 uv2 : TEXCOORD1;
    fixed2 uv3 : TEXCOORD2;
    fixed3 WorldBitangent : TEXCOORD3;
    fixed3 WorldNormal : TEXCOORD4;
				fixed4 vertex : SV_POSITION0; 
				fixed3 Pos : TEXCOORD5; 
    fixed3 Pos2 : TEXCOORD6;
				fixed3 Normal : NORMAL0;
    fixed4 Color : COLOR;
    uint id : TEXCOORD7;
    fixed3 worldPos : TEXCOORD8;
    fixed4 tangent : TEXCOORD9;
};
			
			// vertex shader
			v2f vert (appdata v)
			{
				v2f o;
    
    

    UNITY_INITIALIZE_OUTPUT(v2f, o);
    
    //fixed3 VertP = lerp(v.vertex, fixed3(v.uv2, v.uv3.x), Test);
   
    
    fixed3 _worldTangent = UnityObjectToWorldDir(v.tangent);
    o.tangent.xyz = _worldTangent;
    fixed3 _worldNormal = UnityObjectToWorldNormal(v.normal);
    o.WorldNormal.xyz = _worldNormal;
    fixed _vertexTangentSign = v.tangent.w * unity_WorldTransformParams.w;
    fixed3 _worldBitangent = cross(_worldNormal, _worldTangent) * _vertexTangentSign;
    o.WorldBitangent.xyz = _worldBitangent;
				
   
    
    
    fixed4 pos = v.vertex;

    pos.xyz = (pos.xyz - CentreModel.xyz);
    
    o.vertex = UnityObjectToClipPos(pos);
    
    fixed3 cameraLocalPos;
    
 //   cameraLocalPos = mul(MatrixWorldToObject, fixed4(_WorldSpaceCameraPos, 1));
    
    cameraLocalPos = mul(MatrixWorldToObject, fixed4(_WorldSpaceCameraPos, 1));
    
    
    
    
    o.Pos2 = cameraLocalPos;
    
    
    
    
   // pos = lerp(pos, mul(MatrixWorldToObject2, pos),CentreIntensity);
    
    
    o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
    o.uv2 = v.uv2;
    o.uv3 = v.uv3;
  //  o.uv4 = v.uv4;
    o.Pos = fixed4(pos.xyz, 1);
				o.Normal = v.normal;
    o.Color = v.Color;
    o.id = v.id;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}






			sampler2D _ShapeTex;
			fixed _Scale;
            fixed TotalInternalReflection;
			int _SizeX;
			int _SizeY;
			int _PlaneCount;
			int _MaxReflection;

samplerCUBE ReflectionCube;
		//	samplerCUBE _Environment;
// fixed4 _Environment_HDR;
			fixed _RefractiveIndex;

fixed _RefractiveIndex_;

			fixed _BaseReflection;




		
			#define MAX_REFLECTION (10)









fixed random(fixed2 st)
{
    fixed r = frac(sin(dot(st.xy,
					fixed2(12.9898, 78.233)))
					* 43758.5453123);
    return r * clamp(pow(distance(r, 0.6), 2.5) * 100, 0, 1);
}



fixed CalcReflectionRate(fixed3 normal, fixed3 ray, fixed baseReflection, fixed borderDot)
			{
				//fixed normalizedDot = clamp( (abs(dot(normal,ray)) - borderDot) / ( 1.0 - borderDot ), 0.0, 1.0);
    
    fixed normalizedDot = clamp((abs(dot(normal, ray)) - borderDot) / (1.0 - borderDot), 0.0, 1.0);
    
    
   // return baseReflection;
    
				return baseReflection + (1.0-baseReflection)*pow(1.0-normalizedDot, 5);
			}


fixed rgb2hsv(fixed3 c)
{
    fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
    fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));

    fixed d = q.x - min(q.w, q.y);
    fixed e = 1.0e-10;
    return abs(q.z + (q.w - q.y) / (6.0 * d + e));
}

fixed Remap(fixed value, fixed min1, fixed max1, fixed min2, fixed max2)
{
    return (min2 + (value - min1) * (max2 - min2) / (max1 - min1));
}

			fixed4 GetUnpackedPlaneByIndex(uint index)
			{
				int x_index = index % _SizeX;
				int y_index = index / _SizeX;

				fixed ustride = 1.0 / _SizeX;
				fixed vstride = 1.0 / _SizeY;

				fixed2 uv = fixed2((0.5+x_index)*ustride, (0.5+y_index)*vstride);

				fixed4 packedPlane = tex2D(_ShapeTex, uv);

#if !defined(UNITY_COLORSPACE_GAMMA)
				packedPlane.xyz = LinearToGammaSpace(packedPlane.xyz);
#endif

				fixed3 normal = packedPlane.xyz*2 - fixed3(1,1,1); // смена диапозона

				return fixed4(normal, packedPlane.w*_Scale);
			}

			
fixed CheckCollideRayWithPlane(fixed3 rayStart, fixed3 rayNormalized, fixed4 normalTriangle) // plane - normal.xyz и normal.w - distance
			{
    fixed dp = dot(rayNormalized, normalTriangle.xyz);

				if( dp < 0 )
				{
					return -1;
				}
				else
				{
        fixed distanceNormalized = normalTriangle.w - dot(rayStart.xyz, normalTriangle.xyz);

					if( distanceNormalized < 0 )
					{
						return -1;
					}

					return distanceNormalized / dp;
				}


				return -1;
			}


void CollideRayWithPlane(fixed3 Pos, fixed PassCount, fixed3 rayNormalized, fixed4 TriangleNormal, fixed startSideRelativeRefraction, out fixed reflectionRate, out fixed reflectionRate2, out fixed3 reflection, out fixed3 refraction, out fixed HorizontalElementSquared)
			{
    fixed3 rayVertical = dot(TriangleNormal.xyz, rayNormalized) * TriangleNormal.xyz;
				reflection = rayNormalized - rayVertical*2.0;
    
 
    
  //  reflection.r = pow(reflection.r, reflection.r * 2);

				fixed3 rayHorizontal = rayNormalized - rayVertical;

				fixed3 refractHorizontal = rayHorizontal * startSideRelativeRefraction ;

				fixed horizontalElementSquared = dot(refractHorizontal, refractHorizontal);
				
			/**/
    fixed borderDot = 0;

    
				if( startSideRelativeRefraction > 1.0 )
				{
					borderDot = sqrt(1.0-1.0f/(startSideRelativeRefraction*startSideRelativeRefraction));
				}
				else
				{
					borderDot = 0.0;
				} 
    
    
    
    HorizontalElementSquared = 0;
  //  HorizontalElementSquared = horizontalElementSquared;
    
    
    fixed3 _worldViewDir = UnityWorldSpaceViewDir(Pos);
    _worldViewDir = normalize(_worldViewDir);
    
    
    
    
    HorizontalElementSquared = horizontalElementSquared /3;
    if (horizontalElementSquared >= TotalInternalReflection)  
				{
        HorizontalElementSquared = 0;
        
        
					reflectionRate = 1.0;
        reflectionRate2 = 1.0;
        refraction = TriangleNormal.xyz;

					return;
				}				
			
				fixed verticalSizeSquared = 1-horizontalElementSquared;

				fixed3 refractVertical = rayVertical * sqrt( verticalSizeSquared / dot(rayVertical, rayVertical));
 //   HorizontalElementSquared = verticalSizeSquared;
    
				refraction = refractHorizontal + refractVertical;

   //  refraction = lerp(TriangleNormal.xyz, refraction, Test_);
    
    reflectionRate = CalcReflectionRate(rayNormalized, TriangleNormal.xyz, _BaseReflection * PassCount, borderDot);

    reflectionRate2 = CalcReflectionRate(rayNormalized, TriangleNormal.xyz, _BaseReflection * PassCount, borderDot);
    
  //  reflectionRate = reflectionRate * CalcReflectionRate(rayNormalized, TriangleNormal.xyz, 1 * PassCount, borderDot);
    
    


    
 //   reflectionRate = CalcReflectionRate(rayNormalized, TriangleNormal.xyz, _BaseReflection, 0);
   // reflectionRate = _BaseReflection;
    
				return;
			}

fixed3 CalcColorCoefByDistance(fixed distance,fixed4 Color)
			{

    return pow(max(Color.xyz, 0.01), distance * Color.w);
}

			fixed4 SampleEnvironment(fixed3 rayLocal)
			{
				fixed3 rayWorld = mul(unity_ObjectToWorld, fixed4(rayLocal, 0));

				rayWorld = normalize(rayWorld);

    
#if _CUBEMAPMODE_CUBEMAP 
    fixed4 tex = texCUBElod(_Environment, fixed4(rayWorld,MipLevel));
				return fixed4(DecodeHDR(tex, _Environment_HDR), 1);
    
#endif

    
    
  //  UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0);
    
    
    
    
    
    
 //   fixed4 tex = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, fixed4(rayWorld, 0)) * (1 + (1 - lightEstimation2) * 5);
    
    
#if _CUBEMAPMODE_REFLECTIONPROBE
//    fixed4 tex = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, fixed4(rayWorld, 0)) / (lerp(1, lightEstimation2, FixedlightEstimation));
    
 // fixed4 tex =   UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0,rayWorld, MipLevel) / (lerp(1, lightEstimation2, FixedlightEstimation));
    
    
    fixed4 tex =   UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0,rayWorld, MipLevel);
    
    
  //  fixed4 tex = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, fixed4(rayWorld, 0)) * (1 + ((1 - lightEstimation2) * 5 * ((1 - lightEstimation2))));
    
 //   fixed4 tex = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, fixed4(rayWorld, 0)) * 1 + lightEstimation2;
    return fixed4(DecodeHDR(tex, unity_SpecCube0_HDR), 1);
#endif
    
    
    
    
    
    
    
    
    /*
    fixed4 tex = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, fixed4(rayWorld, 0));
    return fixed4(DecodeHDR(tex, unity_SpecCube0_HDR), 1);
    */
}

			void CheckCollideRayWithAllPlanes(fixed3 rayStart, fixed3 rayDirection, out fixed4 hitPlane, out fixed hitTime)
			{
				hitTime=1000000.0;
				hitPlane=fixed4(1,0,0,1);
		//		[unroll(20)]
    for(int i=0; i<_PlaneCount; ++i)
				{
					fixed4 plane = GetUnpackedPlaneByIndex(i);
					fixed tmpTime = CheckCollideRayWithPlane(rayStart, rayDirection, plane);

        
        
        /*
                
        if (tmpTime >= -0.001 && tmpTime < hitTime)
        {
            hitTime = tmpTime;
            hitPlane = plane;
        }
        

        
        float t_ = 1;
        
        t_ = tmpTime;
        
        hitTime = lerp(hitTime, tmpTime,  t_);
        */
        
					if(tmpTime >= -0.001 && tmpTime<hitTime)
					{
					
            hitTime = tmpTime;
            hitPlane = plane;
        }
        
        
        
        
        
        
        
				}
			}

fixed4 GetColorByRay(fixed3 rayStart, fixed3 rayDirection, fixed refractiveIndex, int MaxReflection, fixed4 Color, fixed lighttransmission)
			{
				fixed3 tmpRayStart = rayStart;
				fixed3 tmpRayDirection = rayDirection;

				fixed reflectionRates[MAX_REFLECTION];
    fixed reflectionRates2[MAX_REFLECTION];
				fixed4 refractionColors[MAX_REFLECTION];
    fixed4 refractionColors2[MAX_REFLECTION];
    fixed4 refractionColors3[MAX_REFLECTION];
				fixed4 depthColors[MAX_REFLECTION];

    

    
    
    int loopCount = min(MAX_REFLECTION, _MaxReflection);
 //   if (UV.x == 3)
  //  {
   //     loopCount = 1;

   // }
				int badRay = 0;

  //  [unroll(10)]
    for( int i = 0; i<loopCount; ++i )
				{
					fixed hitTime=1000000.0;
					fixed4 hitPlane=fixed4(1,0,0,1);
					CheckCollideRayWithAllPlanes(tmpRayStart, tmpRayDirection, hitPlane, hitTime);

					if (hitTime < 0.0)
					{
						badRay = 1;
					}
										
					fixed3 rayEnd = tmpRayStart + tmpRayDirection*hitTime;
								
					fixed reflectionRate;
        fixed reflectionRate2;
					fixed3 reflectionRay;
					fixed3 refractionRay;
        fixed PlaneNull;

        fixed i_Pass = i;
        
        if (i_Pass >= 2)
        {
            i_Pass = 0;

        }
        
        if (i_Pass < 2)
        {
            i_Pass = 1;

        }
        
        
        CollideRayWithPlane(rayStart, i_Pass, tmpRayDirection, hitPlane, refractiveIndex, reflectionRate,reflectionRate2, reflectionRay, refractionRay, PlaneNull);
		
        reflectionRates[i] = reflectionRate;
        
        reflectionRates2[i] = reflectionRate2;
        

       
        
       
        
        

        
        fixed depth2 = Remap(i, 0, loopCount, 0, 1);
        
        
        depth2 = clamp(depth2, 0.0001, 1);
        
        depth2 = 1;
        
      //  PlaneNull = Remap(PlaneNull, 0, 2, 0, 1);
        
    //    PlaneNull = lerp(PlaneNull,1,0);
        
        
        fixed3 _worldViewDir = UnityWorldSpaceViewDir(rayStart.xyz);
        _worldViewDir = normalize(_worldViewDir);


        

        

        
        
        
        
        
        fixed Depth_ = depthColors[i];
        
        Depth_ = Remap(Depth_, 0.997, 0.999, 1, 0);
        
        
        refractionColors3[i] = SampleEnvironment(refractionRay);
        

        
    //    depthColors[i] = fixed4(CalcColorCoefByDistance(hitTime, Color), 1);
        
        
        Color.rgb = lerp(1, Color, ColorIntensity).rgb;
        
        depthColors[i] = fixed4(CalcColorCoefByDistance(hitTime, lerp(Color, 1, lerp(0, (refractionColors3[i].r + refractionColors3[i].g + refractionColors3[i].b) / 2, lighttransmission))), 1);
        
        
   //     refractionColors2[i].r = SampleEnvironment(refractionRay + DispersionR).r;
  //      refractionColors2[i].g = SampleEnvironment(refractionRay + DispersionG).g;
  //      refractionColors2[i].b = SampleEnvironment(refractionRay + DispersionB).b;
        
   //     refractionColors[i].b = SampleEnvironment(normalize(refractionRay) - DispersionR).b;
   //     refractionColors[i].r = SampleEnvironment(normalize(refractionRay) - DispersionG).r;
  //      refractionColors[i].g = SampleEnvironment(normalize(refractionRay) - DispersionB).g;
        
        
       
    ////    refractionColors2[i] = DispersionR * PlaneNull;
        

        
        
     //   refractionColors2[i] = PlaneNull;
        
     //   refractionColors2[i] = refractionColors2[i] * (PlaneNull);
        fixed CLR = refractionRay.x;
        
        if (CLR < 0)
        {
            CLR = CLR * -1;

        }
        
    //    refractionColors2[i] = PlaneNull;
        
     //   refractionColors2[i] = fixed4(CLR, 1, 1, 1);
        
     //   refractionColors2[i] = fixed4(refractionRay + 1, 1);
        
        
    //    refractionColors2[i] = refractionColors2[i] *  depth2;
        
        
  //      refractionColors2[i] = (refractionColors2[i] + refractionColors[i]) / 2;
        
        
        refractionColors[i] = SampleEnvironment(refractionRay);
        
        
   /*     DispersionR = DispersionR / (tmpRayDirection - Depth_) ;
        DispersionG = DispersionG / (tmpRayDirection - Depth_);
        DispersionB = DispersionB / (tmpRayDirection - Depth_) ; */
        
        
        fixed DispRandom = pow(random(hitPlane.xy),0.1);
        
     //   DispRandom = 1;
        
        fixed3 DirDisp = clamp(tmpRayStart.rgb,-1,1);
  
        
    //   refractionColors2[i] = 0;

        
        
  //      refractionColors2[i] = (refractionColors2[i] + refractionColors[i]) / 2;
        
        
   //     refractionColors[i] = SampleEnvironment(refractionRay);
        
        
        
    /*    refractionColors2[i].r = SampleEnvironment(refractionRay + (reflectionRay * Dispersion * 1.5));
        refractionColors2[i].g = SampleEnvironment(refractionRay + (reflectionRay * Dispersion * 1.7));
        refractionColors2[i].b = SampleEnvironment(refractionRay + (reflectionRay * Dispersion * 1.9));
 */
					
					

        if (i == loopCount - 1)
        {
            reflectionRates[i] = 0.0;
            reflectionRates2[i] = 0.0;
        }

        tmpRayStart = tmpRayStart + tmpRayDirection * hitTime;
        tmpRayDirection = reflectionRay;
    }
				
    fixed4 tmpReflectionColor = fixed4(0, 0, 0, 0);
				
				// reverse calc
    for (int j = loopCount - 1; j >= 0; --j)
    {
        
     
   //     tmpReflectionColor = (0.2 + tmpReflectionColor) / 1.2;
        fixed4 refractionColors_;
        
       
        /*
       
        if (j > 1)
        {
            refractionColors_ = lerp(refractionColors2[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j];
        }
        else
        {
            
            refractionColors_ = lerp(refractionColors2[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j];
            

        }
        */
      

   //     refractionColors_ = lerp(refractionColors2[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j];

       
     
            tmpReflectionColor = lerp(refractionColors3[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j];
        
				
     //   tmpReflectionColor = max(tmpReflectionColor, lerp(refractionColors2[j], tmpReflectionColor, reflectionRates2[j]) * depthColors[j]);
        
        
     //   refractionColors_ = tmpReflectionColor;
        
        
        
    //     fixed4 refractionColors_ = lerp(refractionColors[j], tmpReflectionColor, lerp(reflectionRates[j],0.5,0)) * depthColors[j];
        
        
        
        
    //    refractionColors_ = (refractionColors_ + tmpReflectionColor)/2; ======================================

     //   refractionColors_ = (reflectionRates[j] * 6);
        
        
      //  refractionColors_ = refractionColors[j];
        
       // tmpReflectionColor.r = lerp(tmpReflectionColor.r, pow(tmpReflectionColor.r, tmpReflectionColor.r * 2), tmpReflectionColor.r );
        
        
        
      
        
       // refractionColors_ = lerp(refractionColors2[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j];
        
        
      //  DispersionIntensity =  DispersionIntensity;
        
      //  DispersionIntensity = pow(refractionColors_,DispersionIntensity);
        
        
     //   refractionColors_ = lerp(refractionColors_, (lerp(refractionColors2[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j] + tmpReflectionColor) / 2, DispersionIntensity);
        
        
        
        
        
        /*
        if (j > 1)
        {
            refractionColors_ = lerp(refractionColors_, (lerp(refractionColors2[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j] + tmpReflectionColor) / 2, DispersionIntensity);
        }
        else
        {
            refractionColors_ = lerp(refractionColors_, (lerp(refractionColors3[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j] + tmpReflectionColor) / 2, DispersionIntensity);
        }
        */
        
        

     
     //    refractionColors_ = lerp(refractionColors2[j], tmpReflectionColor, reflectionRates[j] * depthColors[j]);
        
        
        
      //  refractionColors_ = tmpReflectionColor;
        
     //   refractionColors_ = fixed4(tmpRayStart, 1);
        /*
        if (Prism_ == 0)
        {
            
            refractionColors_ = lerp(refractionColors_,lerp(refractionColors2[j], tmpReflectionColor, reflectionRates[j]) * depthColors[j],DispersionIntensity);
            
        }
        */
    //    tmpReflectionColor = refractionColors_;
        /*
        if (Prism_ == 0 && tmpReflectionColor.r > 0.8 && tmpReflectionColor.g < 0.99)
        {
            fixed3 oldColor = tmpReflectionColor.rgb;
            fixed3 tmpReflectionColor2;
            
            tmpReflectionColor.r = pow(tmpReflectionColor.r, tmpRayStart.r * tmpReflectionColor.r);
            tmpReflectionColor.g = pow(tmpReflectionColor.g, tmpRayStart.g * tmpReflectionColor.g);
            tmpReflectionColor.b = pow(tmpReflectionColor.b, tmpRayStart.b * tmpReflectionColor.b);
            
            
            tmpReflectionColor2.r = pow(tmpReflectionColor.r, tmpRayStart.r * oldColor.b);
            tmpReflectionColor2.g = pow(tmpReflectionColor.g, tmpRayStart.g * oldColor.r);
            tmpReflectionColor2.b = pow(tmpReflectionColor.b, tmpRayStart.b * oldColor.g);
            
            tmpReflectionColor.rgb = pow(lerp(tmpReflectionColor.rgb, tmpReflectionColor2, oldColor.r), 10);
            
           tmpReflectionColor.rgb = lerp(oldColor, tmpReflectionColor.rgb, 5);
            
            
            /*
            tmpReflectionColor.r = pow(tmpReflectionColor.r, tmpReflectionColor.r * 2) ;
            tmpReflectionColor.g = pow(tmpReflectionColor.g, tmpReflectionColor.g * 3) ;
            tmpReflectionColor.b = pow(tmpReflectionColor.b, tmpReflectionColor.b * 4) ;
            
    }
        */
        
        
      /*  if (Prism_ < 0.97)
        {
        
        tmpReflectionColor.r = pow(tmpReflectionColor.r, tmpReflectionColor.r * 2);
            tmpReflectionColor.g = pow(tmpReflectionColor.g, tmpReflectionColor.g * 2.72);
            tmpReflectionColor.b = pow(tmpReflectionColor.b, tmpReflectionColor.b * 4.4);
        }
        */
        
        
       /* if (reflectionRates[j].r < 0.1)
        {
            tmpReflectionColor = fixed4(1, 0, 0, 1);

        }*/

        
        
    //    Prism_ = depthColors[j].b;
        
    }
				
				if (badRay > 0)
				{
					return fixed4(1, 0, 0, 1);
				}
   // return fixed4(Prism_.xxx, 1);
				return tmpReflectionColor;
			}


fixed4 CalculateContrast(fixed contrastValue, fixed4 colorTarget)
{
    fixed t = 0.5 * (1.0 - contrastValue);
    return mul(fixed4x4(contrastValue, 0, 0, t, 0, contrastValue, 0, t, 0, 0, contrastValue, t, 0, 0, 0, 1), colorTarget);
}

fixed4 ToneMap(fixed4 MainColor, fixed brightness, fixed Disaturate, fixed _max, fixed _min, fixed contrast, fixed Satur)
{

				

				

				
    fixed4 output = MainColor;
			//	output = output * brightness;
    output = output * brightness;
    output = CalculateContrast(contrast, output);

    fixed4 disatur = dot(output, fixed3(0.299, 0.587, 0.114)); // Desaturate
    output = lerp(output, disatur, clamp(pow(((output.x + output.y + output.z) / 3) * Disaturate, 1.3), 0, 1));
    output.x = clamp(Remap(output.x, 0, 1, _min, lerp(_max, 1, 0.5)), 0, 1.5);
    output.y = clamp(Remap(output.y, 0, 1, _min, lerp(_max, 1, 0.5)), 0, 1.5);
    output.z = clamp(Remap(output.z, 0, 1, _min, lerp(_max, 1, 0.5)), 0, 1.5);
				
			//	output = CalculateContrast(clamp(1 - pow((output.x + output.y + output.z) / 3, 1),0,1) * 2, output);

				



					


    output = pow(output, contrast);
				
				//output = lerp(output * (1 - pow(disatur,2)), output, 1 * lerp(max,1,0.3) );

					

				//output = lerp(output, output - 0.5,  _Middle *  clamp( distance(0.8, disatur), 0, 1));

    output = lerp(clamp(output, 0, _max), output, pow(_max, 4));



    output = lerp(smoothstep(output, -0.1, 0.25), output, (1 - distance(1, _max) * 2));

				
    output = lerp(dot(output, fixed3(0.299, 0.587, 0.114)), output, Satur);

    output = output * lerp(brightness, 1, 0.75);




    return output;


}