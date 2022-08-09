using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using static WebRequestAsyncUtility;

public class SNeRGLoader {

    private static readonly string DownloadTitle = "Downloading Assets";
    private static readonly string DownloadInfo = "Downloading Assets for ";
    private static readonly string DownloadAllTitle = "Downloading All Assets";
    private static readonly string DownloadAllMessage = "You are about to download all the demo scenes from the SNeRG paper!\nDownloading/Processing might take a few minutes and take ~3.3GB of disk space.\n\nClick 'OK', if you wish to continue.";
    
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

    [MenuItem("SNeRG/Asset Downloads/Download All", false, -20)]
    public static async void DownloadAllAssets() {
        if (!EditorUtility.DisplayDialog(DownloadAllTitle, DownloadAllMessage, "OK")) {
            return;
        }

        foreach (var scene in (SNeRGScene[])Enum.GetValues(typeof(SNeRGScene))) {
            await DownloadAssets(scene);
        }
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

    [MenuItem("SNeRG/Asset Downloads/Spheres", false, 50)]
    public static void DownloadSpheresAssets() {
        ImportAssetsAsync(SNeRGScene.Spheres);
    }
    [MenuItem("SNeRG/Asset Downloads/Vase Deck", false, 50)]
    public static void DownloadVaseDeckAssets() {
        ImportAssetsAsync(SNeRGScene.VaseDeck);
    }
    [MenuItem("SNeRG/Asset Downloads/Pine Cone", false, 50)]
    public static void DownloadPineConeAssets() {
        ImportAssetsAsync(SNeRGScene.PineCone);
    }
    [MenuItem("SNeRG/Asset Downloads/Toy Car", false, 50)]
    public static void DownloadToyCarAssets() {
        ImportAssetsAsync(SNeRGScene.ToyCar);
    }

#pragma warning restore CS4014
    private static async Task DownloadAssets(SNeRGScene scene) {
        await ImportAssetsAsync(scene);
    }

    private const string BASE_URL_SYNTH = "https://storage.googleapis.com/snerg/750/lego/";
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
        return $"{GetBaseUrl(scene)}/feature{i:D3}.png";
    }

    private static string GetSceneParamsAssetPath(SNeRGScene scene) {
        string path = $"{GetBasePath(scene)}/SceneParams/{scene.String()}.asset";
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
        string path = $"{GetCacheLocation(scene)}/feature{i:D3}.png";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    //public TextAsset SceneParamsAsset;

    public Texture2D[] _rgbaArray;
    public Texture2D[] _featureArray;
    public Texture2D _atlasIndexImage;

    [SerializeField]
    //public SceneParams SceneParams;

    [SerializeField]
    private Texture3D rgbVolumeTexture;
    [SerializeField]
    private Texture3D alphaVolumeTexture;
    [SerializeField]
    private Texture3D featureVolumeTexture;
    [SerializeField]
    private Texture3D atlasIndexTexture;

    /*private int volume_width { get { return (int)SceneParams.AtlasWidth; } }
    private int volume_height { get { return (int)SceneParams.AtlasWidth; } }
    private int volume_depth { get { return (int)SceneParams.AtlasDepth; } }*/

    private static async Task ImportAssetsAsync(SNeRGScene scene) {
        string objName = scene.ToString().ToLower();

        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.1f);
        var sceneParams = await DownloadSceneParamsAsync(scene);
        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.2f);

        // downloads 3D slices to temp directory
        var atlasTask = DownloadAtlasIndexDataAsync(scene);
        var rgbVolumeTask = DownloadRGBVolumeDataAsync(scene, sceneParams);
        var featureVolumeTask = DownloadFeatureVolumeDataAsync(scene, sceneParams);

        Task.WaitAll(atlasTask, rgbVolumeTask, featureVolumeTask);

        byte[] atlasIndexData = await atlasTask;
        Texture2D[] rgbImages = await rgbVolumeTask;
        Texture2D[] featureImages = await featureVolumeTask;

        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.4f);

        Initialize(atlasIndexData, rgbImages, featureImages, sceneParams);

    }

    private static async Task<SceneParams> DownloadSceneParamsAsync(SNeRGScene scene) {
        string url = GetSceneParamsUrl(scene);
        string sceneParamsJson = await WebRequestSimpleAsync.SendWebRequestAsync(url);
        TextAsset mlpJsonTextAsset = new TextAsset(sceneParamsJson);
        AssetDatabase.CreateAsset(mlpJsonTextAsset, GetSceneParamsAssetPath(scene));

        SceneParams sceneParams = JsonConvert.DeserializeObject<SceneParams>(sceneParamsJson);
        return sceneParams;
    }

    private static async Task<byte[]> DownloadAtlasIndexDataAsync(SNeRGScene scene) {
        string path = GetAtlasIndexCachePath(scene);
        byte[] atlasIndexData;

        if (File.Exists(path)) {
            // file is already downloaded
            atlasIndexData = File.ReadAllBytes(path);
        } else {
            string url = GetAtlasIndexUrl(scene);
            atlasIndexData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
            File.WriteAllBytes(GetAtlasIndexCachePath(scene), atlasIndexData);
        }

        return atlasIndexData;
    }

    private static async Task<Texture2D[]> DownloadRGBVolumeDataAsync(SNeRGScene scene, SceneParams sceneParams) {
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

            Texture2D rgbVolumeImage = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false, linear: true);
            rgbVolumeImage.filterMode = FilterMode.Point;
            rgbVolumeImage.wrapMode = TextureWrapMode.Clamp;
            rgbVolumeImage.LoadImage(rgbVolumeData);
            rgbVolumeArray[i] = rgbVolumeImage;
        }

        return rgbVolumeArray;
    }

    private static async Task<Texture2D[]> DownloadFeatureVolumeDataAsync(SNeRGScene scene, SceneParams sceneParams) {
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

            Texture2D featureVolumeImage = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false, linear: true);
            featureVolumeImage.filterMode = FilterMode.Point;
            featureVolumeImage.wrapMode = TextureWrapMode.Clamp;
            featureVolumeImage.LoadImage(featureVolumeData);
            featureVolumeArray[i] = featureVolumeImage;
        }

        return featureVolumeArray;
    }

    private static void Initialize(byte[] atlasIndexData, Texture2D[] rgbaData, Texture2D[] featureData, SceneParams sceneParams) {
        int width   =   (int)Mathf.Ceil(sceneParams.GridWidth / (float)sceneParams.BlockSize);
        int height  =   (int)Mathf.Ceil(sceneParams.GridHeight / (float)sceneParams.BlockSize);
        int depth   =   (int)Mathf.Ceil(sceneParams.GridDepth / (float)sceneParams.BlockSize);

        Texture3D rgbVolumeTexture = new Texture3D(sceneParams.AtlasWidth, sceneParams.AtlasHeight, sceneParams.AtlasDepth, TextureFormat.RGB24, mipChain: false) {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = "RGB Volume Texture"
        };

        Texture3D alphaVolumeTexture = new Texture3D(sceneParams.AtlasWidth, sceneParams.AtlasHeight, sceneParams.AtlasDepth, TextureFormat.R8, mipChain: true) {
            filterMode = FilterMode.Trilinear,   // original code uses different min mag filters, which Unity doesn't let us do, tbd
            wrapMode = TextureWrapMode.Clamp,
            name = "Alpha Volume Texture"
        };

        Texture3D featureVolumeTexture = new Texture3D(sceneParams.AtlasWidth, sceneParams.AtlasHeight, sceneParams.AtlasDepth, TextureFormat.RGBA32, mipChain: false) {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = "Feature Volume Texture",
        };

        Texture3D atlasIndexTexture = new Texture3D(width, height, depth, TextureFormat.RGB24, mipChain: false) {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = "Atlas Index Texture"
        };
        atlasIndexTexture.SetPixelData(atlasIndexData, 0);
        atlasIndexTexture.Apply();

        loadSplitVolumeTexture(rgbaData, alphaVolumeTexture, rgbVolumeTexture, sceneParams);
        loadVolumeTexturePNG(sceneParams);
        createRayMarchMaterial(sceneParams);

        /*
        UnityEditor.AssetDatabase.CreateAsset(rgbVolumeTexture      , "Assets/" + rgbVolumeTexture.name     + ".asset");
        UnityEditor.AssetDatabase.CreateAsset(alphaVolumeTexture    , "Assets/" + alphaVolumeTexture.name   + ".asset");
        UnityEditor.AssetDatabase.CreateAsset(featureVolumeTexture  , "Assets/" + featureVolumeTexture.name + ".asset");
        UnityEditor.AssetDatabase.CreateAsset(atlasIndexTexture     , "Assets/" + atlasIndexTexture.name    + ".asset");
        */
    }

    /// Lego Scene Params:
    /// "voxel_size": 0.0031999999999999997
    /// "block_size": 30
    /// "grid_width": 750
    /// "grid_height": 750
    /// "grid_depth": 750
    /// "atlas_width": 2048
    /// "atlas_height": 2048
    /// "atlas_depth": 32
    /// "num_slices": 8
    /// "min_x": -1.2,
    /// "min_y": -1.2,
    /// "min_z": -1.2,
    /// "atlas_blocks_x": 64,
    /// "atlas_blocks_y": 64,
    /// "atlas_blocks_z": 1,
    /// "worldspace_T_opengl": [[-1.0, 0.0, 0.0, 0.0], [0.0, 0.0, 1.0, 0.0], [0.0, 1.0, 0.0, 0.0], [0.0, 0.0, 0.0, 1.0]],
    /// "ndc": false,

    /// <summary>
    /// Fills an existing 3D RGB texture and an existing 3D alpha texture.
    /// </summary>
    private static void loadSplitVolumeTexture(Texture2D[] rgbaArray, Texture3D alphaVolumeTexture, Texture3D rgbVolumeTexture, SceneParams sceneParams) {
        Debug.Assert(sceneParams.NumSlices == rgbaArray.Length, "Expected " + sceneParams.NumSlices + " RGBA slices, but found " + rgbaArray.Length);

        int volume_width  = sceneParams.AtlasWidth;
        int volume_height = sceneParams.AtlasWidth; 
        int volume_depth  = sceneParams.AtlasDepth;

        int slice_depth = 4;    // slices packed into one atlassed texture
        int num_slices = sceneParams.NumSlices;
        int numBytes = volume_width * volume_height * slice_depth;    // bytes per atlassed texture
        Debug.Log("num bytes " + numBytes); //numBytes = 16,777,216 = 2,048 x 2,048 x 4

        NativeArray<byte> rgbPixels     = rgbVolumeTexture  .GetPixelData<byte>(0);
        NativeArray<byte> alphaPixels   = alphaVolumeTexture.GetPixelData<byte>(0);

        Debug.Assert(rgbPixels  .Length == num_slices * numBytes * 3, "Mismatching RGB Texture Data. Expected: "   + num_slices * numBytes * 3 + ". Actual: + " + rgbPixels  .Length);
        Debug.Assert(alphaPixels.Length == num_slices * numBytes    , "Mismatching alpha Texture Data. Expected: " + num_slices * numBytes     + ". Actual: + " + alphaPixels.Length);

        for (int i = 0; i < num_slices; i++) {

            NativeArray<byte> _rgbaSlice = rgbaArray[i].GetRawTextureData<byte>();
            Debug.Assert(_rgbaSlice.Length == numBytes * 4, "Mismatching RGBA Texture Data. Expected: " + numBytes * 4 + ". Actual: + " + _rgbaSlice.Length);

            int baseIndexRGB = i * numBytes * 3;
            int baseIndexAlpha = i * numBytes;
            for (int j = 0; j < numBytes; j++) {
                rgbPixels   [baseIndexRGB   + (j * 3)    ] = _rgbaSlice[j * 4    ];
                rgbPixels   [baseIndexRGB   + (j * 3) + 1] = _rgbaSlice[j * 4 + 1];
                rgbPixels   [baseIndexRGB   + (j * 3) + 2] = _rgbaSlice[j * 4 + 2];
                alphaPixels [baseIndexAlpha +  j         ] = _rgbaSlice[j * 4 + 3];
            }

            /// gl.texSubImage3D(
            /// gl.TEXTURE_3D,          // target
            /// 0,                      // level (0 -> no mip maps)
            /// 0,                      // x offset
            /// y,                      // y offset
            /// z + i * slice_depth,    // z offset
            /// volume_width,           // width
            /// 1,                      // height
            /// 1,                      // depth
            /// gl.RGB,                 // format
            /// gl.UNSIGNED_BYTE,       // type
            /// rgbPixels,              // source data
            /// 3 * volume_width * (y + volume_height * z)  // optional source byte offset
            /// );


            /*re
            
            let rgbaPixels = values[0]; // lenhgth: 67,108,864
            let i = values[1];

            let rgbPixels = new Uint8Array(
                volume_width * volume_height * slice_depth * 3);
            let alphaPixels = new Uint8Array(
                volume_width * volume_height * slice_depth * 1);

            for (let j = 0; j < volume_width * volume_height * slice_depth;
                 j++) {
                rgbPixels[j * 3 + 0] = rgbaPixels[j * 4 + 0];
                rgbPixels[j * 3 + 1] = rgbaPixels[j * 4 + 1];
                rgbPixels[j * 3 + 2] = rgbaPixels[j * 4 + 2];
                alphaPixels[j] = rgbaPixels[j * 4 + 3];
            }

            // We unfortunately have to touch THREE.js internals to get access
            // to the texture handle and gl.texSubImage3D. Using dictionary
            // notation to make this code robust to minifcation.
            const rgbTextureProperties =
                gRenderer['properties'].get(texture_rgb);
            const alphaTextureProperties =
                gRenderer['properties'].get(texture_alpha);
            let gl = gRenderer.getContext();

            let oldTexture = gl.getParameter(gl.TEXTURE_BINDING_3D);
            gl.bindTexture(
                gl.TEXTURE_3D, rgbTextureProperties['__webglTexture']);
            // Upload row-by-row to work around bug with Intel + Mac OSX.
            for (let z = 0; z < slice_depth; ++z) {
                for (let y = 0; y < volume_height; ++y) {
                    gl.texSubImage3D(
                        gl.TEXTURE_3D, 0, 0, y, z + i * slice_depth,
                        volume_width, 1, 1, gl.RGB, gl.UNSIGNED_BYTE,
                        rgbPixels, 3 * volume_width * (y + volume_height * z));
                }
            }

            gl.bindTexture(
                gl.TEXTURE_3D, alphaTextureProperties['__webglTexture']);
            // Upload row-by-row to work around bug with Intel + Mac OSX.
            for (let z = 0; z < slice_depth; ++z) {
                for (let y = 0; y < volume_height; ++y) {
                    gl.texSubImage3D(
                        gl.TEXTURE_3D, 0, 0, y, z + i * slice_depth,
                        volume_width, 1, 1, gl.RED, gl.UNSIGNED_BYTE,
                        alphaPixels, volume_width * (y + volume_height * z));
                }
            }
            gl.bindTexture(gl.TEXTURE_3D, oldTexture);*/

        }

        //rgbVolumeTexture  .SetPixelData(rgbVolumeData,    0);
        //alphaVolumeTexture.SetPixelData(alphaVolumeData,  0);

        rgbVolumeTexture  .Apply(updateMipmaps: false, makeNoLongerReadable: true);
        alphaVolumeTexture.Apply(updateMipmaps: true , makeNoLongerReadable: true);
    }

    /// <summary>
    /// Fills an existing 3D RGBA texture from {url}_%03d.png.
    /// </summary>
    private static void loadVolumeTexturePNG(SceneParams sceneParams) {
        Debug.Assert(sceneParams.NumSlices == _featureArray.Length, "Expected " + sceneParams.NumSlices + " feature slices, but found " + _featureArray.Length);

        int slice_depth = 4;    // slices packed into one atlassed texture
        long num_slices = sceneParams.NumSlices;
        int numPixels = volume_width * volume_height * slice_depth;    // pixels per atlassed feature texture

        NativeArray<Color32> featurePixels = featureVolumeTexture.GetPixelData<Color32>(0);
        Debug.Assert(featurePixels.Length == num_slices * numPixels, "Mismatching RGB Texture Data. Expected: " + num_slices * numPixels + ". Actual: + " + featurePixels.Length);

        for (int i = 0; i < num_slices; i++) {

            NativeArray<Color32> _featureSlice = _featureArray[i].GetRawTextureData<Color32>();
            Debug.Assert(_featureSlice.Length == numPixels, "Mismatching feature Texture Data. Expected: " + numPixels + ". Actual: + " + _featureSlice.Length);

            NativeSlice<Color32> dst = new NativeSlice<Color32>(featurePixels, i * numPixels, numPixels);
            NativeSlice<Color32> src = new NativeSlice<Color32>(_featureSlice);

            dst.CopyFrom(src);
        }

        featureVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
    }

    private static void createRayMarchMaterial() {
        Vector3 minPosition = new Vector3(
            (float)SceneParams.MinX,
            (float)SceneParams.MinY,
            (float)SceneParams.MinZ
        );
        long        gridWidth       = SceneParams.GridWidth;
        long        gridHeight      = SceneParams.GridWidth;
        long        gridDepth       = SceneParams.GridDepth;
        long        blockSize       = SceneParams.BlockSize;
        double      voxelSize       = SceneParams.VoxelSize;
        long        atlasWidth      = SceneParams.AtlasWidth;
        long        atlasHeight     = SceneParams.AtlasHeight;
        long        atlasDepth      = SceneParams.AtlasDepth;

        double[][]  Weights_0       = SceneParams.The0_Weights;
        double[][]  Weights_1       = SceneParams.The1_Weights;
        double[][]  Weights_2       = SceneParams.The2_Weights;
        double[]    Bias_0          = SceneParams.The0_Bias;
        double[]    Bias_1          = SceneParams.The1_Bias;
        double[]    Bias_2          = SceneParams.The2_Bias;

        // First set up the network weights.

        Texture2D   weightsTexZero  = createFloatTextureFromData(Weights_0);
        Texture2D   weightsTexOne   = createFloatTextureFromData(Weights_1);
        Texture2D   weightsTexTwo   = createFloatTextureFromData(Weights_2);

        StringBuilder biasListZero  = toBiasList(Bias_0);
        StringBuilder biasListOne   = toBiasList(Bias_1);
        StringBuilder biasListTwo   = toBiasList(Bias_2);

        Debug.Log(biasListZero.Length + " " + biasListZero.Capacity);

        int channelsZero            = Weights_0 .Length;
        int channelsOne             = Bias_0    .Length;
        int channelsTwo             = Bias_1    .Length;
        int channelsThree           = Bias_2    .Length;
        int posEncScales            = 4;

        string shaderSource = RaymarchShader.Template;
        /*
        string fragmentShaderSource = rayMarchFragmentShader.replace(
            new RegExp('NUM_CHANNELS_ZERO', 'g'), channelsZero);
        fragmentShaderSource = fragmentShaderSource.replace(
            new RegExp('NUM_POSENC_SCALES', 'g'), posEncScales.toString());
        fragmentShaderSource = fragmentShaderSource.replace(
            new RegExp('NUM_CHANNELS_ONE', 'g'), channelsOne);
        fragmentShaderSource = fragmentShaderSource.replace(
            new RegExp('NUM_CHANNELS_TWO', 'g'), channelsTwo);
        fragmentShaderSource = fragmentShaderSource.replace(
            new RegExp('NUM_CHANNELS_THREE', 'g'), channelsThree);

        fragmentShaderSource = fragmentShaderSource.replace(
            new RegExp('BIAS_LIST_ZERO', 'g'), biasListZero);
        fragmentShaderSource = fragmentShaderSource.replace(
            new RegExp('BIAS_LIST_ONE', 'g'), biasListOne);
        fragmentShaderSource = fragmentShaderSource.replace(
          new RegExp('BIAS_LIST_TWO', 'g'), biasListTwo);

        // Now pass all the 3D textures as uniforms to the shader.
        let worldspace_R_opengl = new THREE.Matrix3();
        let M_dict = network_weights['worldspace_T_opengl'];
        worldspace_R_opengl['set'](
            M_dict[0][0], M_dict[0][1], M_dict[0][2],
            M_dict[1][0], M_dict[1][1], M_dict[1][2],
            M_dict[2][0], M_dict[2][1], M_dict[2][2]);
        */

        /*const material = new THREE.ShaderMaterial({
        uniforms: {
                'mapAlpha': { 'value': alphaVolumeTexture},
          'mapColor': { 'value': rgbVolumeTexture},
          'mapFeatures': { 'value': featureVolumeTexture},
          'mapIndex': { 'value': atlasIndexTexture},
          'displayMode': { 'value': gDisplayMode - 0},
          'ndc' : { 'value' : 0},
          'nearPlane' : { 'value' : 0.33},
          'blockSize': { 'value': blockSize},
          'voxelSize': { 'value': voxelSize},
          'minPosition': { 'value': minPosition},
          'weightsZero': { 'value': weightsTexZero},
          'weightsOne': { 'value': weightsTexOne},
          'weightsTwo': { 'value': weightsTexTwo},
          'world_T_clip': { 'value': new THREE.Matrix4()},
          'worldspace_R_opengl': { 'value': worldspace_R_opengl},
          'gridSize':
              { 'value': new THREE.Vector3(gridWidth, gridHeight, gridDepth)},
          'atlasSize':
              { 'value': new THREE.Vector3(atlasWidth, atlasHeight, atlasDepth)}
            },
        vertexShader:
            rayMarchVertexShader,
        fragmentShader:
            fragmentShaderSource,
        vertexColors:
            true,
      });*/

        /*material.side = THREE.DoubleSide;
        material.depthTest = false;
        return material;*/
    }

    /// <summary>
    /// Creates a float32 texture from an array of floats.
    /// </summary>
    private static Texture2D createFloatTextureFromData(double[][] weights) {
        int width = weights.Length;
        int height = weights[0].Length;
        float[] weightsData = toFloatArray(width, height, weights);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RFloat, mipChain: false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    private static float[] toFloatArray(int width, int height, double[][] data) {

        float[] result = new float[width * height];
        for (int co = 0; co < height; co++) {
            for (int ci = 0; ci < width; ci++) {
                int index = co * width + ci;
                double weight = data[ci][co];
                result[index] = (float)weight;
            }
        }
        return result;
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

public static class MNeRFSceneExtensions {

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