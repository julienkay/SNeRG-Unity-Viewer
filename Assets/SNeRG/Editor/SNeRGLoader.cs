using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using static WebRequestAsyncUtility;

public class SNeRGLoader {

    private static readonly string LoadingTitle = "Loading Assets";
    private static readonly string ProcessingTitle = "Processing Assets";
    private static readonly string DownloadInfo = "Loading Assets for ";

    [MenuItem("SNeRG/Asset Downloads/-- Synthetic Rendered Scenes --", false, -1)]
    public static void Separator0() { }
    [MenuItem("SNeRG/Asset Downloads/-- Synthetic Rendered Scenes --", true, -1)]
    public static bool Separator0Validate() {
        return false;
    }
    [MenuItem("SNeRG/Asset Downloads/-- Real Captured Scenes --", false, 49)]
    public static void Separator1() { }
    [MenuItem("SNeRG/Asset Downloads/-- Real Captured Scenes --", true, 49)]
    public static bool Separator1Validate() {
        return false;
    }

#pragma warning disable CS4014
    [MenuItem("SNeRG/Asset Downloads/Lego", false, 0)]
    public static void DownloadLegoAssets() {
        ImportAssetsAsync(SNeRGScene.Lego);
    }
    [MenuItem("SNeRG/Asset Downloads/Chair", false, 0)]
    public static void DownloadChairAssets() {
        ImportAssetsAsync(SNeRGScene.Chair);
    }
    [MenuItem("SNeRG/Asset Downloads/Drums", false, 0)]
    public static void DownloadDrumsAssets() {
        ImportAssetsAsync(SNeRGScene.Drums);
    }
    [MenuItem("SNeRG/Asset Downloads/Hotdog", false, 0)]
    public static void DownloadHotdogAssets() {
        ImportAssetsAsync(SNeRGScene.Hotdog);
    }
    [MenuItem("SNeRG/Asset Downloads/Ship", false, 0)]
    public static void DownloadShipsAssets() {
        ImportAssetsAsync(SNeRGScene.Ship);
    }
    [MenuItem("SNeRG/Asset Downloads/Mic", false, 0)]
    public static void DownloadMicAssets() {
        ImportAssetsAsync(SNeRGScene.Mic);
    }
    [MenuItem("SNeRG/Asset Downloads/Ficus", false, 0)]
    public static void DownloadFicusAssets() {
        ImportAssetsAsync(SNeRGScene.Ficus);
    }
    [MenuItem("SNeRG/Asset Downloads/Materials", false, 0)]
    public static void DownloadMaterialsAssets() {
        ImportAssetsAsync(SNeRGScene.Materials);
    }

#pragma warning restore CS4014

    private const string BASE_URL_SYNTH = "https://storage.googleapis.com/snerg/750/";
    private const string BASE_URL_REAL = "https://storage.googleapis.com/snerg/real_1000/";

    private const string BASE_FOLDER = "Assets/SNeRG Data/";
    private const string BASE_LIB_FOLDER = "Library/Cached SNeRG Data/";

    private static string GetBasePath(SNeRGScene scene) {
        return $"{BASE_FOLDER}{scene.String()}";
    }

    private static string GetCacheLocation(SNeRGScene scene) {
        return $"{BASE_LIB_FOLDER}{scene.String()}";
    }

    private static string GetBaseUrl(SNeRGScene scene) {
        if (scene.IsSynthetic()) {
            return $"{BASE_URL_SYNTH}{scene.String()}";
        } else {
            return $"{BASE_URL_REAL}{scene.String()}";
        }
    }

    private static string GetSceneParamsUrl(SNeRGScene scene) {
        return $"{GetBaseUrl(scene)}/scene_params.json";
    }
    private static string GetAtlasIndexUrl(SNeRGScene scene) {
        return $"{GetBaseUrl(scene)}/atlas_indices.png";
    }
    private static string GetRGBVolumeUrl(SNeRGScene scene, int i) {
        return $"{GetBaseUrl(scene)}/rgba_{i:D3}.png";
    }
    private static string GetFeatureVolumeUrl(SNeRGScene scene, int i) {
        return $"{GetBaseUrl(scene)}/feature_{i:D3}.png";
    }

    private static string GetSceneParamsAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/SceneParams/{scene.String()}.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetRGBTextureAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} RGB Volume Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetAlphaTextureAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Alpha Volume Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetFeatureTextureAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Feature Volume Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetAtlasTextureAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Atlas Index Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetShaderAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/Shaders/RayMarchShader_{scene}.shader";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetMaterialAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/Materials/Material_{scene}.mat";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetWeightsAssetPath(SNeRGScene scene, int i) {
        string path = $"{GetBasePath(scene)}/SceneParams/weightsTex{i}.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetPrefabAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/{scene}.prefab";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetMeshAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/Volume Mesh/{scene.String()} Volume Mesh.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetAtlasIndexCachePath(SNeRGScene scene) {
        string path = $"{GetCacheLocation(scene)}/atlas_indices.png";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetRGBVolumeCachePath(SNeRGScene scene, int i) {
        string path = $"{GetCacheLocation(scene)}/rgba_{i:D3}.png";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetFeatureVolumeCachePath(SNeRGScene scene, int i) {
        string path = $"{GetCacheLocation(scene)}/feature_{i:D3}.png";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static async Task ImportAssetsAsync(SNeRGScene scene) {
        string objName = scene.String();

        EditorUtility.DisplayProgressBar(LoadingTitle, $"{DownloadInfo}'{objName}'...", 0.1f);
        var sceneParams = await DownloadSceneParamsAsync(scene);

        EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{scene}' Raymarch Shader...", 0.2f);
        CreateRayMarchShader(scene, sceneParams);

        EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{scene}' Material...", 0.3f);
        CreateMaterial(scene, sceneParams);

        // load 3D slices from web or cacbe directory, then create 3D volume textures from that data
        EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{scene}' Atlas Index Texture...", 0.4f);
        Texture2D atlasIndexData = await LoadAtlasIndexDataAsync(scene);
        CreateAtlasIndexTexture(scene, atlasIndexData, sceneParams);

        EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{scene}' RGB Volume Texture...", 0.5f);
        Texture2D[] rgbImages = await LoadRGBVolumeDataAsync(scene, sceneParams);
        CreateRgbVolumeTexture(scene, rgbImages, sceneParams);

        EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{scene}' Feature Volume Texture...", 0.6f);
        Texture2D[] featureImages = await LoadFeatureVolumeDataAsync(scene, sceneParams);
        CreateFeatureVolumeTexture(scene, featureImages, sceneParams);

        EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{scene}' Weight Textures...", 0.7f);
        CreateWeightTextures(scene, sceneParams);

        EditorUtility.DisplayProgressBar(ProcessingTitle, $"Finishing '{scene}' assets..", 0.8f);
        VerifyMaterial(scene, sceneParams);

        CreatePrefab(scene, sceneParams);

        EditorUtility.ClearProgressBar();
    }

    private static async Task<SceneParams> DownloadSceneParamsAsync(SNeRGScene scene) {
        string url = GetSceneParamsUrl(scene);
        string sceneParamsJson = await WebRequestSimpleAsync.SendWebRequestAsync(url);
        TextAsset mlpJsonTextAsset = new TextAsset(sceneParamsJson);
        AssetDatabase.CreateAsset(mlpJsonTextAsset, GetSceneParamsAssetPath(scene));

        SceneParams sceneParams = JsonConvert.DeserializeObject<SceneParams>(sceneParamsJson);
        return sceneParams;
    }

    private static async Task<Texture2D> LoadAtlasIndexDataAsync(SNeRGScene scene) {
        string path = GetAtlasIndexCachePath(scene);
        byte[] atlasIndexData;

        if (File.Exists(path)) {
            // file is already downloaded
            atlasIndexData = File.ReadAllBytes(path);
        } else {
            string url = GetAtlasIndexUrl(scene);
            atlasIndexData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
            File.WriteAllBytes(path, atlasIndexData);
        }

        // !!! Unity's LoadImage() does NOT respect the texture format specified in the input texture!
        // It always loads this as ARGB32, no matter the format specified here.
        // Ideally we'd directly load an RGB24 texture.
        Texture2D atlasIndexImage = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true);
        atlasIndexImage.filterMode = FilterMode.Point;
        atlasIndexImage.wrapMode = TextureWrapMode.Clamp;
        atlasIndexImage.LoadImage(atlasIndexData);

        return atlasIndexImage;
    }

    private static async Task<Texture2D[]> LoadRGBVolumeDataAsync(SNeRGScene scene, SceneParams sceneParams) {
        Texture2D[] rgbVolumeArray = new Texture2D[sceneParams.NumSlices];
        for (int i = 0; i < sceneParams.NumSlices; i++) {
            string path = GetRGBVolumeCachePath(scene, i);
            byte[] rgbVolumeData;

            if (File.Exists(path)) {
                // file is already downloaded
                rgbVolumeData = File.ReadAllBytes(path);
            } else {
                string url = GetRGBVolumeUrl(scene, i);
                rgbVolumeData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
                File.WriteAllBytes(path, rgbVolumeData);
            }

            // Unity's LoadImage() always loads this as ARGB32, no matter the format specified here
            Texture2D rgbVolumeImage = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true);
            rgbVolumeImage.filterMode = FilterMode.Point;
            rgbVolumeImage.wrapMode = TextureWrapMode.Clamp;
            rgbVolumeImage.alphaIsTransparency = true;
            rgbVolumeImage.LoadImage(rgbVolumeData);
            rgbVolumeArray[i] = rgbVolumeImage;
        }

        return rgbVolumeArray;
    }

    private static async Task<Texture2D[]> LoadFeatureVolumeDataAsync(SNeRGScene scene, SceneParams sceneParams) {
        Texture2D[] featureVolumeArray = new Texture2D[sceneParams.NumSlices];

        for (int i = 0; i < sceneParams.NumSlices; i++) {
            string path = GetFeatureVolumeCachePath(scene, i);
            byte[] featureVolumeData;

            if (File.Exists(path)) {
                // file is already downloaded
                featureVolumeData = File.ReadAllBytes(path);
            } else {
                string url = GetFeatureVolumeUrl(scene, i);
                featureVolumeData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
                File.WriteAllBytes(path, featureVolumeData);
            }

            // Unity's LoadImage() always loads this as ARGB32, no matter the format specified here
            Texture2D featureVolumeImage = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true);
            featureVolumeImage.filterMode = FilterMode.Point;
            featureVolumeImage.wrapMode = TextureWrapMode.Clamp;
            featureVolumeImage.LoadImage(featureVolumeData);
            featureVolumeArray[i] = featureVolumeImage;
        }

        return featureVolumeArray;
    }

    private static void CreateRgbVolumeTexture(SNeRGScene scene, Texture2D[] rgbaData, SceneParams sceneParams) {
        string rgbAssetPath = GetRGBTextureAssetPath(scene);
        string alphaAssetPath = GetAlphaTextureAssetPath(scene);
        
        // already exists
        if (File.Exists(rgbAssetPath) && File.Exists(alphaAssetPath)) {
            return;
        }

        // initialize 3D textures
        Texture3D rgbVolumeTexture = new Texture3D(sceneParams.AtlasWidth, sceneParams.AtlasHeight, sceneParams.AtlasDepth, TextureFormat.RGB24, mipChain: false) {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = Path.GetFileNameWithoutExtension(rgbAssetPath)
        };

        Texture3D alphaVolumeTexture = new Texture3D(sceneParams.AtlasWidth, sceneParams.AtlasHeight, sceneParams.AtlasDepth, TextureFormat.R8, mipChain: true) {
            filterMode = FilterMode.Trilinear,   // original code uses different min mag filters, which Unity doesn't let us do, tbd
            wrapMode = TextureWrapMode.Clamp,
            name = Path.GetFileNameWithoutExtension(alphaAssetPath)
        };

        // load data into 3D textures
        loadSplitVolumeTexture(rgbaData, alphaVolumeTexture, rgbVolumeTexture, sceneParams);

        // destroy intermediate textures to free memory
        for (int i = rgbaData.Length - 1; i >= 0; i--) {
            UnityEngine.Object.DestroyImmediate(rgbaData[i]);
        }
        Resources.UnloadUnusedAssets();

        AssetDatabase.CreateAsset(rgbVolumeTexture, rgbAssetPath);
        AssetDatabase.CreateAsset(alphaVolumeTexture, alphaAssetPath);

        string materialAssetPath = GetMaterialAssetPath(scene);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
        material.SetTexture("mapColor", rgbVolumeTexture);
        material.SetTexture("mapAlpha", alphaVolumeTexture);

        AssetDatabase.SaveAssets();
    }

    private static void CreateFeatureVolumeTexture(SNeRGScene scene, Texture2D[] featureData, SceneParams sceneParams) {
        string featureAssetPath = GetFeatureTextureAssetPath(scene);

        // already exists
        if (File.Exists(featureAssetPath)) {
            return;
        }

        // initialize 3D texture
        Texture3D featureVolumeTexture = new Texture3D(sceneParams.AtlasWidth, sceneParams.AtlasHeight, sceneParams.AtlasDepth, TextureFormat.ARGB32, mipChain: false) {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = Path.GetFileNameWithoutExtension(featureAssetPath)
        };

        // load data into 3D textures
        LoadVolumeTexture(featureData, featureVolumeTexture, sceneParams);
        
        // destroy intermediate textures to free memory
        for (int i = featureData.Length - 1; i >= 0; i--) {
            UnityEngine.Object.DestroyImmediate(featureData[i]);
        }
        Resources.UnloadUnusedAssets();
    
        AssetDatabase.CreateAsset(featureVolumeTexture, featureAssetPath);

        string materialAssetPath = GetMaterialAssetPath(scene);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
        material.SetTexture("mapFeatures", featureVolumeTexture);
    }

    private static void CreateAtlasIndexTexture(SNeRGScene scene, Texture2D atlasIndexImage, SceneParams sceneParams) {
        int width = (int)Mathf.Ceil(sceneParams.GridWidth / (float)sceneParams.BlockSize);
        int height = (int)Mathf.Ceil(sceneParams.GridHeight / (float)sceneParams.BlockSize);
        int depth = (int)Mathf.Ceil(sceneParams.GridDepth / (float)sceneParams.BlockSize);

        string atlasAssetPath = GetAtlasTextureAssetPath(scene);

        // already exists
        if (File.Exists(atlasAssetPath)) {
            return;
        }

        // initialize 3D texture
        Texture3D atlasIndexTexture = new Texture3D(width, height, depth, TextureFormat.ARGB32, mipChain: false) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = Path.GetFileNameWithoutExtension(atlasAssetPath),
        };

        // load data into a 3D texture,

        // load data into 3D textures
        NativeArray<byte> rawAtlasIndexData = atlasIndexImage.GetRawTextureData<byte>();
        atlasIndexTexture.SetPixelData(rawAtlasIndexData, 0);

        // flip the y axis for each depth slice
        FlipY<Color32>(atlasIndexTexture);

        // reverse the slices of the 3D texture
        //FlipZ<Color32>(atlasIndexTexture);

        atlasIndexTexture.Apply();

        // destroy intermediate texture to free memory
        UnityEngine.Object.DestroyImmediate(atlasIndexImage);
        Resources.UnloadUnusedAssets();

        AssetDatabase.CreateAsset(atlasIndexTexture, atlasAssetPath);

        string materialAssetPath = GetMaterialAssetPath(scene);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
        material.SetTexture("mapIndex", atlasIndexTexture);
    }

    /// <summary>
    /// Fills two distinct 3D textures from a set of atlassed PNGs.
    /// This method flips both y and z axes of the 3D texture, because the original code assumes we're
    /// indexing from top left, but Unity loaded the PNGs starting bottom right.
    /// </summary>
    private static void loadSplitVolumeTexture(Texture2D[] rgbaArray, Texture3D alphaVolumeTexture, Texture3D rgbVolumeTexture, SceneParams sceneParams) {
        Debug.Assert(sceneParams.NumSlices == rgbaArray.Length, "Expected " + sceneParams.NumSlices + " RGBA slices, but found " + rgbaArray.Length);

        int volume_width  = sceneParams.AtlasWidth;
        int volume_height = sceneParams.AtlasHeight; 
        int volume_depth  = sceneParams.AtlasDepth;

        int slice_depth = 4;    // slices packed into one atlassed texture
        int num_slices = sceneParams.NumSlices;
        int ppAtlas = volume_width * volume_height * slice_depth;   // pixels per atlassed texture
        int ppSlice = volume_width * volume_height;                 // pixels per volume slice

        NativeArray<byte> rgbPixels     = rgbVolumeTexture  .GetPixelData<byte>(0);
        NativeArray<byte> alphaPixels   = alphaVolumeTexture.GetPixelData<byte>(0);

        Debug.Assert(rgbPixels  .Length == num_slices * ppAtlas * 3, "Mismatching RGB Texture Data. Expected: "   + num_slices * ppAtlas * 3 + ". Actual: + " + rgbPixels  .Length);
        Debug.Assert(alphaPixels.Length == num_slices * ppAtlas    , "Mismatching alpha Texture Data. Expected: " + num_slices * ppAtlas     + ". Actual: + " + alphaPixels.Length);

        for (int i = 0; i < num_slices; i++) {
            // rgba images are in ARGB format!
            NativeArray<byte> _rgbaImageFourSlices = rgbaArray[i].GetPixelData<byte>(0);
            Debug.Assert(_rgbaImageFourSlices.Length == ppAtlas * 4, "Mismatching RGBA Texture Data. Expected: " + ppAtlas * 4 + ". Actual: + " + _rgbaImageFourSlices.Length);

            for (int s_r = slice_depth - 1, s = 0; s_r >= 0; s_r--, s++) {

                int baseIndexRGB   = (i * ppAtlas + s * ppSlice) * 3;
                int baseIndexAlpha = (i * ppAtlas + s * ppSlice);
                for (int j = 0; j < ppSlice; j++) {
                    rgbPixels  [baseIndexRGB   + (j * 3)    ] = _rgbaImageFourSlices[((s_r * ppSlice + j) * 4) + 1];
                    rgbPixels  [baseIndexRGB   + (j * 3) + 1] = _rgbaImageFourSlices[((s_r * ppSlice + j) * 4) + 2];
                    rgbPixels  [baseIndexRGB   + (j * 3) + 2] = _rgbaImageFourSlices[((s_r * ppSlice + j) * 4) + 3];
                    alphaPixels[baseIndexAlpha +  j         ] = _rgbaImageFourSlices[((s_r * ppSlice + j) * 4)    ];
                }
            }
        }

        FlipY<Color24>(rgbVolumeTexture);
        FlipZ<Color24>(rgbVolumeTexture);
        FlipY<byte>(alphaVolumeTexture);
        FlipZ<byte>(alphaVolumeTexture);

        rgbVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        alphaVolumeTexture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
    }

    /// <summary>
    /// Fills an existing 3D RGBA texture from atlassed PNGs.
    /// This method flips both y and z axes of the 3D texture, because the original code assumes we're
    /// indexing from top left, but Unity loaded the PNGs starting bottom right.
    /// </summary>
    private static void LoadVolumeTexture(Texture2D[] featureImages, Texture3D featureVolumeTexture, SceneParams sceneParams) {
        Debug.Assert(sceneParams.NumSlices == featureImages.Length, "Expected " + sceneParams.NumSlices + " feature slices, but found " + featureImages.Length);

        int volume_width  = sceneParams.AtlasWidth;
        int volume_height = sceneParams.AtlasHeight;
        int volume_depth  = sceneParams.AtlasDepth;

        int slice_depth   = 4;    // slices packed into one atlassed texture
        long num_slices   = sceneParams.NumSlices;
        int ppAtlas       = volume_width * volume_height * slice_depth;     // pixels per atlassed feature texture
        int ppSlice       = volume_width * volume_height;                   // pixels per volume slice

        NativeArray<Color32> featurePixels = featureVolumeTexture.GetPixelData<Color32>(0);
        Debug.Assert(featurePixels.Length == num_slices * ppAtlas, "Mismatching RGB Texture Data. Expected: " + num_slices * ppAtlas + ". Actual: + " + featurePixels.Length);

        for (int i = 0; i < num_slices; i++) {

            NativeArray<Color32> _featureImageFourSlices = featureImages[i].GetRawTextureData<Color32>();
            Debug.Assert(_featureImageFourSlices.Length == ppAtlas, "Mismatching feature Texture Data. Expected: " + ppAtlas + ". Actual: + " + _featureImageFourSlices.Length);

            for (int s_r = slice_depth - 1, s = 0; s_r >= 0; s_r--, s++) {
                int targetIndex = (i * ppAtlas) + (s * ppSlice);
                NativeSlice<Color32> dst = new NativeSlice<Color32>(featurePixels, targetIndex, ppSlice);
                NativeSlice<Color32> src = new NativeSlice<Color32>(_featureImageFourSlices, s_r * ppSlice, ppSlice);

                dst.CopyFrom(src);
            }
        }
        FlipY<Color32>(featureVolumeTexture);
        FlipZ<Color32>(featureVolumeTexture);

        featureVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
    }

    /// <summary>
    /// Vertically flips each depth slice in the given 3D texture.
    /// </summary>
    private static void FlipY<T>(Texture3D texture) where T : struct {
        int width = texture.width;
        int height = texture.height;
        int depth = texture.depth;
        NativeArray<T> data = texture.GetPixelData<T>(0);
        for (int z = 0; z < depth; z++) {
            for (int y = 0; y < height / 2; y++) {
                for (int x = 0; x < width; x++) {
                    int flippedY = height - y - 1;
                    int source = z * (width * height) + (flippedY * width) + x;
                    int target = z * (width * height) + (y * width) + x;
                    (data[target], data[source]) = (data[source], data[target]);
                }
            }
        }
    }

    /// <summary>
    /// Flips z - Reverses the order of the depth slices of the given 3D texture
    /// </summary>
    private static void FlipZ<T>(Texture3D texture) where T : struct {
        int width = texture.width;
        int height = texture.height;
        int depth = texture.depth;

        int sliceSize = width * height;

        NativeArray<T> data = texture.GetPixelData<T>(0);
        NativeArray<T> tmp = new NativeArray<T>(sliceSize, Allocator.Temp);

        for (int z = 0; z < depth / 2; z++) {
            int slice1Index = z * sliceSize;
            int slice2Index = (depth - z - 1) * sliceSize;

            NativeSlice<T> slice1 = new NativeSlice<T>(data, slice1Index, sliceSize);
            NativeSlice<T> slice2 = new NativeSlice<T>(data, slice2Index, sliceSize);

            slice1.CopyTo(tmp);
            slice1.CopyFrom(slice2);
            slice2.CopyFrom(tmp);
        }

        tmp.Dispose();
    }

    private struct Color24 {
        public byte r;
        public byte g;
        public byte b;
    }

    private static void CreateRayMarchShader(SNeRGScene scene, SceneParams sceneParams) {
        double[][]  Weights_0      = sceneParams._0Weights;
        double[][]  Weights_1      = sceneParams._1Weights;
        double[][]  Weights_2      = sceneParams._2Weights;
        double[]    Bias_0         = sceneParams._0Bias;
        double[]    Bias_1         = sceneParams._1Bias;
        double[]    Bias_2         = sceneParams._2Bias;

        StringBuilder biasListZero = toBiasList(Bias_0);
        StringBuilder biasListOne  = toBiasList(Bias_1);
        StringBuilder biasListTwo  = toBiasList(Bias_2);

        int channelsZero            = Weights_0 .Length;
        int channelsOne             = Bias_0    .Length;
        int channelsTwo             = Bias_1    .Length;
        int channelsThree           = Bias_2    .Length;
        int posEncScales            = 4;

        string shaderSource = RaymarchShader.Template;
        shaderSource = new Regex("OBJECT_NAME"       ).Replace(shaderSource, $"{scene}");
        shaderSource = new Regex("NUM_CHANNELS_ZERO" ).Replace(shaderSource, $"{channelsZero}");
        shaderSource = new Regex("NUM_POSENC_SCALES" ).Replace(shaderSource, $"{posEncScales}");
        shaderSource = new Regex("NUM_CHANNELS_ONE"  ).Replace(shaderSource, $"{channelsOne}");
        shaderSource = new Regex("NUM_CHANNELS_TWO"  ).Replace(shaderSource, $"{channelsTwo}");
        shaderSource = new Regex("NUM_CHANNELS_THREE").Replace(shaderSource, $"{channelsThree}");
        shaderSource = new Regex("BIAS_LIST_ZERO"    ).Replace(shaderSource, $"{biasListZero}");
        shaderSource = new Regex("BIAS_LIST_ONE"     ).Replace(shaderSource, $"{biasListOne}");
        shaderSource = new Regex("BIAS_LIST_TWO"     ).Replace(shaderSource, $"{biasListTwo}");

        string shaderAssetPath = GetShaderAssetPath(scene);
        File.WriteAllText(shaderAssetPath, shaderSource);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateMaterial(SNeRGScene scene, SceneParams sceneParams) {
        string materialAssetPath = GetMaterialAssetPath(scene);
        Material material;

        if (File.Exists(GetMaterialAssetPath(scene))) {
            material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
        } else {
            string shaderAssetPath = GetShaderAssetPath(scene);
            Shader raymarchShader = AssetDatabase.LoadAssetAtPath<Shader>(shaderAssetPath);
            material = new Material(raymarchShader);
        }

        material.SetInteger("displayMode", 0);
        material.SetInteger("ndc", sceneParams.Ndc ? 1 : 0);

        material.SetVector("minPosition", new Vector4(
            (float)sceneParams.MinX,
            (float)sceneParams.MinY,
            (float)sceneParams.MinZ,
            0f)
        );
        material.SetVector("gridSize", new Vector4(
            sceneParams.GridWidth,
            sceneParams.GridHeight,
            sceneParams.GridDepth,
            0)
        );
        material.SetVector("atlasSize", new Vector4(
            sceneParams.AtlasWidth,
            sceneParams.AtlasHeight,
            sceneParams.AtlasDepth,
            0)
        );
        material.SetFloat("voxelSize", (float)sceneParams.VoxelSize);
        material.SetFloat("blockSize", (float)sceneParams.BlockSize);
        int maxStep = Mathf.CeilToInt(new Vector3(sceneParams.GridWidth, sceneParams.GridHeight, sceneParams.GridDepth).magnitude);
        material.SetInteger("maxStep", maxStep);

        // volume texture properties will be assigned when creating the 3D textures to avoid having to load them into memory here

        if (!File.Exists(GetMaterialAssetPath(scene))) {
            AssetDatabase.CreateAsset(material, materialAssetPath);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Assign volume textures again in case material got recreated.
    /// </summary>
    private static void VerifyMaterial(SNeRGScene scene, SceneParams sceneParams) {
        string materialAssetPath = GetMaterialAssetPath(scene);
        Material material        = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
                                 
        string rgbAssetPath      = GetRGBTextureAssetPath(scene);
        string alphaAssetPath    = GetAlphaTextureAssetPath(scene);
        string featureAssetPath  = GetFeatureTextureAssetPath(scene);
        string atlasAssetPath    = GetAtlasTextureAssetPath(scene);
        
        if (material.GetTexture("mapColor") == null) {
            Texture3D rgb = AssetDatabase.LoadAssetAtPath<Texture3D>(rgbAssetPath);
            material.SetTexture("mapColor", rgb);
        }
        if (material.GetTexture("mapAlpha") == null) {
            Texture3D alpha = AssetDatabase.LoadAssetAtPath<Texture3D>(alphaAssetPath);
            material.SetTexture("mapAlpha", alpha);
        }
        if (material.GetTexture("mapFeatures") == null) {
            Texture3D feature = AssetDatabase.LoadAssetAtPath<Texture3D>(featureAssetPath);
            material.SetTexture("mapFeatures", feature);
        }
        if (material.GetTexture("mapIndex") == null) {
            Texture3D atlas = AssetDatabase.LoadAssetAtPath<Texture3D>(atlasAssetPath);
            material.SetTexture("mapIndex", atlas);
        }
    }

    private static void CreateWeightTextures(SNeRGScene scene, SceneParams sceneParams) {
        Texture2D weightsTexZero = createFloatTextureFromData(sceneParams._0Weights);
        Texture2D weightsTexOne = createFloatTextureFromData(sceneParams._1Weights);
        Texture2D weightsTexTwo = createFloatTextureFromData(sceneParams._2Weights);
        AssetDatabase.CreateAsset(weightsTexZero, GetWeightsAssetPath(scene, 0));
        AssetDatabase.CreateAsset(weightsTexOne, GetWeightsAssetPath(scene, 1));
        AssetDatabase.CreateAsset(weightsTexTwo, GetWeightsAssetPath(scene, 2));

        string materialAssetPath = GetMaterialAssetPath(scene);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
        material.SetTexture("weightsZero", weightsTexZero);
        material.SetTexture("weightsOne", weightsTexOne);
        material.SetTexture("weightsTwo", weightsTexTwo);
    }

    /// <summary>
    /// Creates a float32 texture from an 2D array of weights.
    /// </summary>
    private static Texture2D createFloatTextureFromData(double[][] weights) {
        int width = weights.Length;
        int height = weights[0].Length;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RFloat, mipChain: false, linear: true);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        NativeArray<float> textureData = texture.GetRawTextureData<float>();
        FillTexture(textureData, weights);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        return texture;
    }

    private static void FillTexture(NativeArray<float> textureData, double[][] data) {
        int width = data.Length;
        int height = data[0].Length;

        for (int co = 0; co < height; co++) {
            for (int ci = 0; ci < width; ci++) {
                int index = co * width + ci;
                double weight = data[ci][co];
                textureData[index] = (float)weight;
            }
        }
    }

    private static StringBuilder toBiasList(double[] biases) {
        System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
        int width = biases.Length;
        StringBuilder biasList = new StringBuilder(width * 12);
        for (int i = 0; i < width; i++) {
            double bias = biases[i];
            biasList.Append(bias.ToString("F7", culture));
            if (i + 1 < width) {
                biasList.Append(", ");
            }
        }
        return biasList;
    }

    /// <summary>
    /// Creates a convenient prefab for the SNeRG.
    /// </summary>
    private static void CreatePrefab(SNeRGScene scene, SceneParams sceneParams) {
        GameObject prefabObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prefabObject.name = scene.ToString();
        MeshRenderer renderer = prefabObject.GetComponent<MeshRenderer>();
        string materialAssetPath = GetMaterialAssetPath(scene);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
        renderer.material = material;
        // create mesh copy and scale by volume extents to be able to raymarchin object space
        MeshFilter meshFilter = prefabObject.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = CreateMesh(meshFilter.sharedMesh, sceneParams);
        meshFilter.sharedMesh.name = $"{scene}_volume_mesh";
        AssetDatabase.CreateAsset(meshFilter.sharedMesh, GetMeshAssetPath(scene));
        prefabObject.AddComponent<EnableDepthTexture>();
        PrefabUtility.SaveAsPrefabAsset(prefabObject, GetPrefabAssetPath(scene));
        GameObject.DestroyImmediate(prefabObject);
    }

    private static Mesh CreateMesh(Mesh mesh, SceneParams sceneParams) {
        var vertices = mesh.vertices;
        for(int i = 0; i < vertices.Length; i++) {
            Vector3 v = vertices[i];
            v.Scale(new Vector3(
                Mathf.Abs((float)sceneParams.MinX),
                Mathf.Abs((float)sceneParams.MinY),
                Mathf.Abs((float)sceneParams.MinZ))
                * 2f
            );
            vertices[i] = v;
        }
        Mesh newMesh = new Mesh {
            vertices  = vertices,
            triangles = mesh.triangles,
            uv        = mesh.uv,
            normals   = mesh.normals,
            colors    = mesh.colors,
            tangents  = mesh.tangents
        };
        return newMesh;
    }
}

public enum SNeRGScene {
    Lego,
    Chair,
    Drums,
    Hotdog,
    Ship,
    Mic,
    Ficus,
    Materials,
    Spheres,
    VaseDeck,
    PineCone,
    ToyCar
}

public static class SNeRGSceneExtensions {

    public static string String(this SNeRGScene scene) {
        return scene.ToString().ToLower();
    }

    public static SNeRGScene ToEnum(string value) {
        return (SNeRGScene)Enum.Parse(typeof(SNeRGScene), value, true);
    }

    public static bool IsSynthetic(this SNeRGScene scene) {
        switch (scene) {
            case SNeRGScene.Lego:
            case SNeRGScene.Chair:
            case SNeRGScene.Drums:
            case SNeRGScene.Hotdog:
            case SNeRGScene.Ship:
            case SNeRGScene.Mic:
            case SNeRGScene.Ficus:
            case SNeRGScene.Materials:
                return true;
            case SNeRGScene.Spheres:
            case SNeRGScene.VaseDeck:
            case SNeRGScene.PineCone:
            case SNeRGScene.ToyCar:
                return false;
            default:
                throw new InvalidOperationException();
        }
    }
}