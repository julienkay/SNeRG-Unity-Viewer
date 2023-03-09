public static class RaymarchShader {
    public const string Template = @"Shader ""SNeRG/RayMarchShader_OBJECT_NAME"" {
    Properties {
        mapAlpha(""Alpha Map"", 3D) = """" {}
        mapColor(""Color Map"", 3D) = """" {}
        mapFeatures(""Feature Map"", 3D) = """" {}
        mapIndex(""Index Map"", 3D) = """" {}

        weightsZero (""Weights Zero"", 2D) = ""white"" {}
        weightsOne (""Weights One"", 2D) = ""white"" {}
        weightsTwo (""Weights Two"", 2D) = ""white"" {}

        displayMode(""Display Mode"", Integer) = 0
        ndc(""NDC"", Integer) = 0

	    minPosition (""Min Position"", Vector) = (0, 0, 0, 0)
        gridSize (""Grid Size"", Vector) = (0, 0, 0, 0)
        atlasSize (""Atlas Size"", Vector) = (0, 0, 0, 0)
	    voxelSize (""Voxel Size"", Float) = 0.0
	    blockSize (""Block Size"", Float) = 0.0

        maxStep (""Max Step"", Integer) = 0.0
    }
    SubShader {
        Cull Front
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include ""UnityCG.cginc""

            int displayMode;
            int ndc;

            float4 minPosition;
            float4 gridSize;
            float4 atlasSize;
            float voxelSize;
            float blockSize;
            int maxStep;

            UNITY_DECLARE_TEX3D(mapAlpha);
            UNITY_DECLARE_TEX3D(mapColor);
            UNITY_DECLARE_TEX3D(mapFeatures);
            UNITY_DECLARE_TEX3D(mapIndex);

            UNITY_DECLARE_TEX2D(weightsZero);
            UNITY_DECLARE_TEX2D(weightsOne);
            UNITY_DECLARE_TEX2D(weightsTwo);

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 origin : TEXCOORD1;
                float3 direction : TEXCOORD2;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.origin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
                o.direction = mul(unity_WorldToObject, -WorldSpaceViewDir(v.vertex));

                // assumes we're rendering on a unit cube
                o.origin *= abs(minPosition) * 2;

                return o;
            }

            half indexToPosEnc (half3 dir, int index) {
                half coordinate =
                    (index % 3 == 0) ? dir.x : (
                        (index % 3 == 1) ? dir.y : dir.z);
                if (index < 3) {
                    return coordinate;
                }
                int scaleExponent = ((index - 3) % (3 * 4)) / 3;
                coordinate *= pow(2.0, float(scaleExponent));
                if ((index - 3) >= 3 * 4) {
                    const float kHalfPi = 1.57079632679489661923;
                    coordinate += kHalfPi;
                }
                return sin(coordinate);
            }
            
            half3 evaluateNetwork(fixed3 color, fixed4 features, fixed3 viewdir) {
                half intermediate_one[NUM_CHANNELS_ONE] = { BIAS_LIST_ZERO };
                int i = 0;
                int j = 0;

                for (j = 0; j < NUM_CHANNELS_ZERO; ++j) {
                    half input_value = 0.0;
                    if (j < 27) {
                        input_value = indexToPosEnc(viewdir, j);
                    }
                    else if (j < 30) {
                        input_value =
                            (j % 3 == 0) ? color.r : (
                                (j % 3 == 1) ? color.g : color.b);
                    }
                    else {
                        input_value =
                            (j == 30) ? features.r : (
                                (j == 31) ? features.g : (
                                    (j == 32) ? features.b : features.a));
                    }
                    if (abs(input_value) < 0.1 / 255.0) {
                        continue;
                    }
                    for (int i = 0; i < NUM_CHANNELS_ONE; ++i) {
                        intermediate_one[i] += input_value * weightsZero.Load(int3(j, i, 0)).x;
                    }
                }

                half intermediate_two[NUM_CHANNELS_TWO] = { BIAS_LIST_ONE };

                for (j = 0; j < NUM_CHANNELS_ONE; ++j) {
                    if (intermediate_one[j] <= 0.0) {
                        continue;
                    }
                    for (i = 0; i < NUM_CHANNELS_TWO; ++i) {
                        intermediate_two[i] += intermediate_one[j] * weightsOne.Load(int3(j, i, 0)).x;
                    }
                }

                half result[NUM_CHANNELS_THREE] = { BIAS_LIST_TWO };

                for (j = 0; j < NUM_CHANNELS_TWO; ++j) {
                    if (intermediate_two[j] <= 0.0) {
                        continue;
                    }
                    for (i = 0; i < NUM_CHANNELS_THREE; ++i) {
                        result[i] += intermediate_two[j] * weightsTwo.Load(int3(j, i, 0)).x;
                    }
                }
                for (i = 0; i < NUM_CHANNELS_THREE; ++i) {
                    result[i] = 1.0 / (1.0 + exp(-result[i]));
                }

                return half3(result[0], result[1], result[2]);
            }

            half3 convertOriginToNDC(float3 origin, float3 direction) {
                // We store the NDC scenes flipped, so flip back.
                origin.z *= -1.0;
                direction.z *= -1.0;

                const float near = 1.0;
                float t = -(near + origin.z) / direction.z;
                origin = origin * t + direction;

                // Hardcoded, worked out using approximate iPhone FOV of 67.3 degrees
                // and an image width of 1006 px.
                const float focal = 755.644;
                const float W = 1006.0;
                const float H = 756.0;
                float o0 = 1.0 / (W / (2.0 * focal)) * origin.x / origin.z;
                float o1 = -1.0 / (H / (2.0 * focal)) * origin.y / origin.z;
                float o2 = 1.0 + 2.0 * near / origin.z;

                origin = float3(o0, o1, o2);
                origin.z *= -1.0;
                return origin;
            }

            half3 convertDirectionToNDC(float3 origin, float3 direction) {
                // We store the NDC scenes flipped, so flip back.
                origin.z *= -1.0;
                direction.z *= -1.0;

                const float near = 1.0;
                float t = -(near + origin.z) / direction.z;
                origin = origin * t + direction;

                // Hardcoded, worked out using approximate iPhone FOV of 67.3 degrees
                // and an image width of 1006 px.
                const float focal = 755.6440;
                const float W = 1006.0;
                const float H = 756.0;

                float d0 = 1.0 / (W / (2.0 * focal)) *
                    (direction.x / direction.z - origin.x / origin.z);
                float d1 = -1.0 / (H / (2.0 * focal)) *
                    (direction.y / direction.z - origin.y / origin.z);
                float d2 = -2.0 * near / origin.z;

                direction = normalize(float3(d0, d1, d2));
                direction.z *= -1.0;
                return direction;
            }

            // Compute the atlas block index for a point in the scene using pancake
            // 3D atlas packing.
            half3 pancakeBlockIndex(half3 posGrid, float blockSize, int3 iBlockGridBlocks) {
                int3 iBlockIndex = int3(floor(posGrid / blockSize));
                int3 iAtlasBlocks = atlasSize.xyz / (blockSize + 2.0);
                int linearIndex = iBlockIndex.x + iBlockGridBlocks.x *
                    (iBlockIndex.z + iBlockGridBlocks.z * iBlockIndex.y);

                half3 atlasBlockIndex = half3(
                    float(linearIndex % iAtlasBlocks.x),
                    float((linearIndex / iAtlasBlocks.x) % iAtlasBlocks.y),
                    float(linearIndex / (iAtlasBlocks.x * iAtlasBlocks.y)));

                // If we exceed the size of the atlas, indicate an empty voxel block.
                if (atlasBlockIndex.z >= float(iAtlasBlocks.z)) {
                    atlasBlockIndex = half3(-1.0, -1.0, -1.0);
                }

                return atlasBlockIndex;
            }
            
            half2 rayAabbIntersection(half3 aabbMin, half3 aabbMax, half3 origin, half3 invDirection) {
                half3 t1 = (aabbMin - origin) * invDirection;
                half3 t2 = (aabbMax - origin) * invDirection;
                half3 tMin = min(t1, t2);
                half3 tMax = max(t1, t2);
                return half2(max(tMin.x, max(tMin.y, tMin.z)), min(tMax.x, min(tMax.y, tMax.z)));
            }

            fixed4 frag (v2f i) : SV_Target {
                // Runs the full model with view dependence.
                const int DISPLAY_NORMAL = 0;
                // Disables the view-dependence network.
                const int DISPLAY_DIFFUSE = 1;
                // Only shows the latent features.
                const int DISPLAY_FEATURES = 2;
                // Only shows the view dependent component.
                const int DISPLAY_VIEW_DEPENDENT = 3;
                // Only shows the coarse block grid.
                const int DISPLAY_COARSE_GRID = 4;
                // Only shows the 3D texture atlas.
                const int DISPLAY_3D_ATLAS = 5;

                // Set up the ray parameters in object space..
                float nearPlane = _ProjectionParams.y;
                half3 origin = i.origin;
                half3 directionWorld = normalize(i.direction);
                if (ndc != 0) {
                    nearPlane = 0.0;
                    origin = convertOriginToNDC(i.origin, normalize(i.direction));
                    directionWorld = convertDirectionToNDC(i.origin, normalize(i.direction));
                }

                // Now transform them to the voxel grid coordinate system.
                half3 originGrid = (origin - minPosition.xyz) / voxelSize;
                half3 directionGrid = directionWorld;
                half3 invDirectionGrid = 1.0 / directionGrid;

                int3 iGridSize = int3(round(gridSize.xyz));
                int iBlockSize = int(round(blockSize));
                int3 iBlockGridBlocks = (iGridSize + iBlockSize - 1) / iBlockSize;
                int3 iBlockGridSize = iBlockGridBlocks * iBlockSize;
                half3 blockGridSize = half3(iBlockGridSize);
                half2 tMinMax = rayAabbIntersection(half3(0.0, 0.0, 0.0), gridSize.xyz, originGrid, invDirectionGrid);

                // Skip any rays that miss the scene bounding box.
                if (tMinMax.x > tMinMax.y) {
                  return fixed4(1.0, 1.0, 1.0, 1.0);
                }

                float t = max(nearPlane / voxelSize, tMinMax.x) + 0.5;
                half3 posGrid = originGrid + directionGrid * t;

                half3 blockMin = floor(posGrid / blockSize) * blockSize;
                half3 blockMax = blockMin + blockSize;
                half2 tBlockMinMax = rayAabbIntersection(
                      blockMin, blockMax, originGrid, invDirectionGrid);
                half3 atlasBlockIndex;

                if (displayMode == DISPLAY_3D_ATLAS) {
                  atlasBlockIndex = pancakeBlockIndex(posGrid, blockSize, iBlockGridBlocks);
                } else {
                  atlasBlockIndex = 255.0 * UNITY_SAMPLE_TEX3D(mapIndex, (blockMin + blockMax) / (2.0 * blockGridSize)).xyz;
                }

                half visibility = 1.0;
                half3 color = half3(0.0, 0.0, 0.0);
                half4 features = half4(0.0, 0.0, 0.0, 0.0);
                int step = 0;

                [loop]
                while (step < maxStep && t < tMinMax.y && visibility > 1.0 / 255.0) {
                    // Skip empty macroblocks.
                    if (atlasBlockIndex.x > 254.0) {
                        t = 0.5 + tBlockMinMax.y;
                    } else { // Otherwise step through them and fetch RGBA and Features.
                        half3 posAtlas = clamp(posGrid - blockMin, 0.0, blockSize);

                        posAtlas += atlasBlockIndex * (blockSize + 2.0);
                        posAtlas += 1.0; // Account for the one voxel padding in the atlas.

                        if (displayMode == DISPLAY_COARSE_GRID) {
                            color = atlasBlockIndex * (blockSize + 2.0) / atlasSize.xyz;
                            features.rgb = atlasBlockIndex * (blockSize + 2.0) / atlasSize.xyz;
                            features.a = 1.0;
                            visibility = 0.0;
                            continue;
                        }

                        // Do a conservative fetch for alpha!=0 at a lower resolution,
                        // and skip any voxels which are empty. First, this saves bandwidth
                        // since we only fetch one byte instead of 8 (trilinear) and most
                        // fetches hit cache due to low res. Second, this is conservative,
                        // and accounts for any possible alpha mass that the high resolution
                        // trilinear would find.
                        const int skipMipLevel = 2;
                        const float miniBlockSize = float(1 << skipMipLevel);

                        // Only fetch one byte at first, to conserve memory bandwidth in
                        // empty space.
                        float atlasAlpha = mapAlpha.Load(int4(int3(posAtlas / miniBlockSize), skipMipLevel)).x;

                        if (atlasAlpha > 0.0) {
                            // OK, we hit something, do a proper trilinear fetch at high res.
                            half3 atlasUvw = posAtlas / atlasSize.xyz;
                            atlasAlpha = UNITY_SAMPLE_TEX3D_LOD(mapAlpha, atlasUvw, 0.0).x;

                            // Only worth fetching the content if high res alpha is non-zero.
                            if (atlasAlpha > 0.5 / 255.0) {
                            half4 atlasRgba = half4(0.0, 0.0, 0.0, atlasAlpha);
                            atlasRgba.rgb = UNITY_SAMPLE_TEX3D(mapColor, atlasUvw).rgb;
                            if (displayMode != DISPLAY_DIFFUSE) {
                                half4 atlasFeatures = UNITY_SAMPLE_TEX3D(mapFeatures, atlasUvw);
                                features += visibility * atlasFeatures;
                            }
                            color += visibility * atlasRgba.rgb;
                            visibility *= 1.0 - atlasRgba.a;
                            }
                        }
                        t += 1.0;
                    }

                    posGrid = originGrid + directionGrid * t;
                    if (t > tBlockMinMax.y) {
                        blockMin = floor(posGrid / blockSize) * blockSize;
                        blockMax = blockMin + blockSize;
                        tBlockMinMax = rayAabbIntersection(blockMin, blockMax, originGrid, invDirectionGrid);

                        if (displayMode == DISPLAY_3D_ATLAS) {
                            atlasBlockIndex = pancakeBlockIndex(posGrid, blockSize, iBlockGridBlocks);
                        } else {
                            atlasBlockIndex = 255.0 * UNITY_SAMPLE_TEX3D(mapIndex, (blockMin + blockMax) / (2.0 * blockGridSize)).xyz;
                        }
                    }
                    step++;
                }

                if (displayMode == DISPLAY_VIEW_DEPENDENT) {
                  color = half3(0.0, 0.0, 0.0) * visibility;
                } else if (displayMode == DISPLAY_FEATURES) {
                  color = features.rgb;
                }

                // For forward-facing scenes, we partially unpremultiply alpha to fill
                // tiny holes in the rendering.
                half alpha = 1.0 - visibility;
                if (ndc != 0 && alpha > 0.0) {
                    half filledAlpha = min(1.0, alpha * 1.5);
                    color *= filledAlpha / alpha;
                    alpha = filledAlpha;
                    visibility = 1.0 - filledAlpha;
                }

                // convert from Unity's right handed coordinate system
                // to OpenGL coordinates used in the MLP evaluation
                i.direction.xz = -i.direction.xz;
                i.direction.yz = i.direction.zy;

                // Compute the final color, to save compute only compute view-depdence
                // for rays that intersected something in the scene.
                color = half3(1.0, 1.0, 1.0) * visibility + color;
                const float kVisibilityThreshold = 254.0 / 255.0;
                if (visibility <= kVisibilityThreshold &&
                    (displayMode == DISPLAY_NORMAL ||
                     displayMode == DISPLAY_VIEW_DEPENDENT)) {
                  color += evaluateNetwork(color, features, normalize(i.direction));
                }

                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}";
}
