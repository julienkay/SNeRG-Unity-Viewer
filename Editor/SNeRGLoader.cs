using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using static SNeRG.Editor.WebRequestAsyncUtility;

namespace SNeRG.Editor {

    public class SNeRGLoader {

        private static readonly string LoadingTitle = "Loading Assets";
        private static readonly string ProcessingTitle = "Processing Assets";
        private static readonly string DownloadInfo = "Loading Assets for ";
        private static readonly string FolderTitle = "Select folder with SNeRG source files";
        private static readonly string FolderExistsTitle = "Folder already exists";
        private static readonly string FolderExistsMsg = "A folder for this asset already exists in the Unity project. Overwrite?";
        private static readonly string OK = "OK";
        private static readonly string ImportErrorTitle = "Error importing SNeRG assets";

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

        [MenuItem("SNeRG/Import from disk", false, 0)]
        public async static void ImportAssetsFromDisk() {
            // select folder with custom data
            string path = EditorUtility.OpenFolderPanel(FolderTitle, "", "");
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) {
                return;
            }

            // ask whether to overwrite existing folder
            string objName = new DirectoryInfo(path).Name;
            if (Directory.Exists($"{BASE_FOLDER}{objName}")) {
                if (!EditorUtility.DisplayDialog(FolderExistsTitle, FolderExistsMsg, OK)) {
                    return;
                }
            }

            await ImportCustomScene(path);
        }

        [MenuItem("SNeRG/Asset Downloads/Lego", false, 0)]
        public async static void DownloadLegoAssets() {
            await ImportDemoSceneAsync(SNeRGScene.Lego);
        }
        [MenuItem("SNeRG/Asset Downloads/Chair", false, 0)]
        public async static void DownloadChairAssets() {
            await ImportDemoSceneAsync(SNeRGScene.Chair);
        }
        [MenuItem("SNeRG/Asset Downloads/Drums", false, 0)]
        public async static void DownloadDrumsAssets() {
            await ImportDemoSceneAsync(SNeRGScene.Drums);
        }
        [MenuItem("SNeRG/Asset Downloads/Hotdog", false, 0)]
        public async static void DownloadHotdogAssets() {
            await ImportDemoSceneAsync(SNeRGScene.Hotdog);
        }
        [MenuItem("SNeRG/Asset Downloads/Ship", false, 0)]
        public async static void DownloadShipsAssets() {
            await ImportDemoSceneAsync(SNeRGScene.Ship);
        }
        [MenuItem("SNeRG/Asset Downloads/Mic", false, 0)]
        public async static void DownloadMicAssets() {
            await ImportDemoSceneAsync(SNeRGScene.Mic);
        }
        [MenuItem("SNeRG/Asset Downloads/Ficus", false, 0)]
        public async static void DownloadFicusAssets() {
            await ImportDemoSceneAsync(SNeRGScene.Ficus);
        }
        [MenuItem("SNeRG/Asset Downloads/Materials", false, 0)]
        public async static void DownloadMaterialsAssets() {
            await ImportDemoSceneAsync(SNeRGScene.Materials);
        }

        [MenuItem("SNeRG/Asset Downloads/Spheres", false, 50)]
        public async static void DownloadSpheresAssets() {
            await ImportDemoSceneAsync(SNeRGScene.Spheres);
        }
        [MenuItem("SNeRG/Asset Downloads/Vase Deck", false, 50)]
        public async static void DownloadVaseDeckAssets() {
            await ImportDemoSceneAsync(SNeRGScene.VaseDeck);
        }
        [MenuItem("SNeRG/Asset Downloads/Pine Cone", false, 50)]
        public async static void DownloadPineConeAssets() {
            await ImportDemoSceneAsync(SNeRGScene.PineCone);
        }
        [MenuItem("SNeRG/Asset Downloads/Toy Car", false, 50)]
        public async static void DownloadToyCarAssets() {
            await ImportDemoSceneAsync(SNeRGScene.ToyCar);
        }

        private const string BASE_URL_SYNTH = "https://storage.googleapis.com/snerg/750/";
        private const string BASE_URL_REAL = "https://storage.googleapis.com/snerg/real_1000/";

        private static readonly string BASE_FOLDER = Path.Combine("Assets", "SNeRG Data");
        private static readonly string BASE_LIB_FOLDER = Path.Combine("Library", "Cached SNeRG Data");

        private static ImportContext _context;

        private static string GetBasePath(string sceneName) {
            return Path.Combine(BASE_FOLDER, sceneName);
        }
        private static string GetCacheLocation(string sceneName) {
            return Path.Combine(BASE_LIB_FOLDER, sceneName);
        }
        private static string GetBaseUrl(SNeRGScene scene) {
            if (scene.IsSynthetic()) {
                return $"{BASE_URL_SYNTH}{scene.LowerCaseName()}";
            } else {
                return $"{BASE_URL_REAL}{scene.LowerCaseName()}";
            }
        }

        private static string GetSceneParamsUrl() {
            return $"{GetBaseUrl(_context.Scene)}/scene_params.json";
        }
        private static string GetAtlasIndexUrl() {
            return $"{GetBaseUrl(_context.Scene)}/atlas_indices.png";
        }
        private static string GetRGBVolumeUrl(int i) {
            return $"{GetBaseUrl(_context.Scene)}/rgba_{i:D3}.png";
        }
        private static string GetFeatureVolumeUrl(int i) {
            return $"{GetBaseUrl(_context.Scene)}/feature_{i:D3}.png";
        }

        private static string GetSceneParamsAssetPath() {
            return GetAssetPath("SceneParams", $"{_context.SceneName}.asset");
        }
        private static string GetRGBTextureAssetPath() {
            return GetAssetPath("Textures", $"{_context.SceneName} RGB Volume Texture.asset");
        }
        private static string GetAlphaTextureAssetPath() {
            return GetAssetPath("Textures", $"{_context.SceneName} Alpha Volume Texture.asset");
        }
        private static string GetFeatureTextureAssetPath() {
            return GetAssetPath("Textures", $"{_context.SceneName} Feature Volume Texture.asset");
        }
        private static string GetAtlasTextureAssetPath() {
            return GetAssetPath("Textures", $"{_context.SceneName} Atlas Index Texture.asset");
        }
        private static string GetShaderAssetPath() {
            return GetAssetPath("Shaders", $"RayMarchShader_{_context.SceneName}.shader");
        }
        private static string GetMaterialAssetPath() {
            return GetAssetPath("Materials", $"Material_{_context.SceneName}.mat");
        }
        private static string GetWeightsAssetPath(int i) {
            return GetAssetPath("SceneParams", $"weightsTex{i}.asset");
        }
        private static string GetPrefabAssetPath() {
            return GetAssetPath("", $"{_context.SceneName}.prefab");
        }
        private static string GetMeshAssetPath() {
            return GetAssetPath("Volume Mesh", $"{_context.SceneName} Volume Mesh.asset");
        }
        /// <summary>
        /// This returns a path in the asset directory to store the specific asset into.
        /// </summary>
        private static string GetAssetPath(string subFolder, string assetName) {
            string path = Path.Combine(GetBasePath(_context.SceneName), subFolder, assetName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }

        private static string GetAtlasIndexCachePath() {
            return GetCachePath("atlas_indices.png");
        }
        private static string GetRGBVolumeCachePath(int i) {
            return GetCachePath($"rgba_{i:D3}.png");
        }
        private static string GetFeatureVolumeCachePath(int i) {
            return GetCachePath($"feature_{i:D3}.png");
        }
        /// <summary>
        /// This is either the location where demo scenes are first downloaded to,
        /// or the path to the source files for custom scene imports.
        /// </summary>
        private static string GetCachePath(string assetName) {
            string path;
            if (_context.CustomScene) {
                path = Path.Combine(_context.CustomScenePath, assetName);
            } else {
                path = Path.Combine(GetCacheLocation(_context.SceneName), assetName);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            return path;
        }

        /// <summary>
        /// Creates Unity assets for the given SNeRG assets on disk.
        /// </summary>
        /// <param name="path">The path to the folder with the SNeRG assets (PNGs & mlp.json)</param>
        private static async Task ImportCustomScene(string path) {
            _context = new ImportContext() {
                CustomScene = true,
                CustomScenePath = path,
                Scene = SNeRGScene.Custom
            };
            string objName = new DirectoryInfo(path).Name;

            SceneParams sceneParams = CopySceneParamsFromPath(path);
            if (sceneParams == null) {
                return;
            }
            if (!ValidatePNGs(path, sceneParams)) {
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            await ProcessAssets(objName);
        }

        /// <summary>
        /// Downloads the source files for the given SNeRG demo scene
        /// into the <see cref="BASE_LIB_FOLDER"/> folder. Then in the
        /// Unity project, creates the Unity assets necessary to display it.
        /// </summary>
        private static async Task ImportDemoSceneAsync(SNeRGScene scene) {
            _context = new ImportContext() {
                CustomScene = false,
                Scene = scene
            };

            EditorUtility.DisplayProgressBar(LoadingTitle, $"{DownloadInfo}'{scene.Name()}'...", 0.1f);
            await DownloadSceneParamsAsync();

            await ProcessAssets(scene.Name());
        }

        /// <summary>
        /// Set specific import settings on OBJs/PNGs.
        /// Creates Weight Textures, Materials and Shader from MLP data.
        /// Creates a convenient prefab for the SNeRG object.
        /// </summary>
        private static async Task ProcessAssets(string sceneName) {
            var sceneParams = GetSceneParams();

            EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{sceneName}' Raymarch Shader...", 0.2f);
            CreateRayMarchShader(sceneParams);

            EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{sceneName}' Material...", 0.3f);
            CreateMaterial(sceneParams);

            // load 3D slices from web or cacbe directory, then create 3D volume textures from that data
            EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{sceneName}' Atlas Index Texture...", 0.4f);
            Texture2D atlasIndexData = await LoadAtlasIndexDataAsync();
            CreateAtlasIndexTexture(atlasIndexData, sceneParams);

            EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{sceneName}' RGB Volume Texture...", 0.5f);
            Texture2D[] rgbImages = await LoadRGBVolumeDataAsync(sceneParams);
            CreateRgbVolumeTexture(rgbImages, sceneParams);

            EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{sceneName}' Feature Volume Texture...", 0.6f);
            Texture2D[] featureImages = await LoadFeatureVolumeDataAsync(sceneParams);
            CreateFeatureVolumeTexture(featureImages, sceneParams);

            EditorUtility.DisplayProgressBar(ProcessingTitle, $"Creating '{sceneName}' Weight Textures...", 0.7f);
            CreateWeightTextures(sceneParams);

            EditorUtility.DisplayProgressBar(ProcessingTitle, $"Finishing '{sceneName}' assets..", 0.8f);
            VerifyMaterial(sceneParams);

            CreatePrefab(sceneParams);

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Looks for a scene_params.json at <paramref name="path"/> and imports it.
        /// </summary>
        private static SceneParams CopySceneParamsFromPath(string path) {
            string[] sceneParamsPaths = Directory.GetFiles(path, "scene_params.json", SearchOption.AllDirectories);
            if (sceneParamsPaths.Length > 1) {
                EditorUtility.DisplayDialog(ImportErrorTitle, "Multiple scene_params.json files found", OK);
                return null;
            }
            if (sceneParamsPaths.Length <= 0) {
                EditorUtility.DisplayDialog(ImportErrorTitle, "No scene_params.json files found", OK);
                return null;
            }

            string sceneParamsJson = File.ReadAllText(sceneParamsPaths[0]);
            TextAsset sceneParamsTextAsset = new TextAsset(sceneParamsJson);
            AssetDatabase.CreateAsset(sceneParamsTextAsset, GetSceneParamsAssetPath());
            SceneParams sceneParams = JsonConvert.DeserializeObject<SceneParams>(sceneParamsJson);
            return sceneParams;
        }

        private static async Task DownloadSceneParamsAsync() {
            string url = GetSceneParamsUrl();
            string sceneParamsJson = await WebRequestSimpleAsync.SendWebRequestAsync(url);
            TextAsset sceneParamsTextAsset = new TextAsset(sceneParamsJson);
            AssetDatabase.CreateAsset(sceneParamsTextAsset, GetSceneParamsAssetPath());
        }

        private static SceneParams GetSceneParams() {
            string mlpJson = AssetDatabase.LoadAssetAtPath<TextAsset>(GetSceneParamsAssetPath()).text;
            return JsonConvert.DeserializeObject<SceneParams>(mlpJson);
        }

        /// <summary>
        /// Checks if all necessary textures for a given SNeRG scene are present in the given folder.
        /// </summary>
        private static bool ValidatePNGs(string path, SceneParams mlp) {
            string objName = new DirectoryInfo(path).Name;
            int numSlicePNGs = mlp.NumSlices;

            string[] pngPaths;
            pngPaths = Directory.GetFiles(path, "feature_*.png", SearchOption.TopDirectoryOnly);
            if (pngPaths.Length != numSlicePNGs) {
                EditorUtility.DisplayDialog(ImportErrorTitle, $"Invalid number of feature textures found. Expected: {numSlicePNGs}. Actual: {pngPaths.Length}", OK);
                return false;
            }
            pngPaths = Directory.GetFiles(path, "rgba_*.png", SearchOption.TopDirectoryOnly);
            if (pngPaths.Length != numSlicePNGs) {
                EditorUtility.DisplayDialog(ImportErrorTitle, $"Invalid number of feature textures found. Expected: {numSlicePNGs}. Actual: {pngPaths.Length}", OK);
                return false;
            }

            string atlasPath = Path.Combine(path, "atlas_indices.png");
            if (!File.Exists(atlasPath)) {
                EditorUtility.DisplayDialog(ImportErrorTitle, $"Could not find atlas indices texture.", OK);
                return false;
            }
            return true;
        }

        private static async Task<Texture2D> LoadAtlasIndexDataAsync() {
            string path = GetAtlasIndexCachePath();
            byte[] atlasIndexData;

            if (File.Exists(path)) {
                // file is already downloaded
                atlasIndexData = File.ReadAllBytes(path);
            } else {
                string url = GetAtlasIndexUrl();
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

        private static async Task<Texture2D[]> LoadRGBVolumeDataAsync(SceneParams sceneParams) {
            Texture2D[] rgbVolumeArray = new Texture2D[sceneParams.NumSlices];
            for (int i = 0; i < sceneParams.NumSlices; i++) {
                string path = GetRGBVolumeCachePath(i);
                byte[] rgbVolumeData;

                if (File.Exists(path)) {
                    // file is already downloaded
                    rgbVolumeData = File.ReadAllBytes(path);
                } else {
                    string url = GetRGBVolumeUrl(i);
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

        private static async Task<Texture2D[]> LoadFeatureVolumeDataAsync(SceneParams sceneParams) {
            Texture2D[] featureVolumeArray = new Texture2D[sceneParams.NumSlices];

            for (int i = 0; i < sceneParams.NumSlices; i++) {
                string path = GetFeatureVolumeCachePath(i);
                byte[] featureVolumeData;

                if (File.Exists(path)) {
                    // file is already downloaded
                    featureVolumeData = File.ReadAllBytes(path);
                } else {
                    string url = GetFeatureVolumeUrl(i);
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

        private static void CreateRgbVolumeTexture(Texture2D[] rgbaData, SceneParams sceneParams) {
            string rgbAssetPath = GetRGBTextureAssetPath();
            string alphaAssetPath = GetAlphaTextureAssetPath();

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

            string materialAssetPath = GetMaterialAssetPath();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
            material.SetTexture("mapColor", rgbVolumeTexture);
            material.SetTexture("mapAlpha", alphaVolumeTexture);

            AssetDatabase.SaveAssets();
        }

        private static void CreateFeatureVolumeTexture(Texture2D[] featureData, SceneParams sceneParams) {
            string featureAssetPath = GetFeatureTextureAssetPath();

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

            string materialAssetPath = GetMaterialAssetPath();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
            material.SetTexture("mapFeatures", featureVolumeTexture);
        }

        private static void CreateAtlasIndexTexture(Texture2D atlasIndexImage, SceneParams sceneParams) {
            int width = (int)Mathf.Ceil(sceneParams.GridWidth / (float)sceneParams.BlockSize);
            int height = (int)Mathf.Ceil(sceneParams.GridHeight / (float)sceneParams.BlockSize);
            int depth = (int)Mathf.Ceil(sceneParams.GridDepth / (float)sceneParams.BlockSize);

            string atlasAssetPath = GetAtlasTextureAssetPath();

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

            string materialAssetPath = GetMaterialAssetPath();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
            material.SetTexture("mapIndex", atlasIndexTexture);
        }

        /// <summary>
        /// Fills two distinct 3D textures from a set of atlased PNGs.
        /// This method flips both y and z axes of the 3D texture, because the original code assumes we're
        /// indexing from top left, but Unity loaded the PNGs starting bottom right.
        /// </summary>
        private static void loadSplitVolumeTexture(Texture2D[] rgbaArray, Texture3D alphaVolumeTexture, Texture3D rgbVolumeTexture, SceneParams sceneParams) {
            Debug.Assert(sceneParams.NumSlices == rgbaArray.Length, "Expected " + sceneParams.NumSlices + " RGBA slices, but found " + rgbaArray.Length);

            int volumeWidth = sceneParams.AtlasWidth;
            int volumeHeight = sceneParams.AtlasHeight;
            int volumeDepth = sceneParams.AtlasDepth;

            int sliceDepth = 4;                                       // slices packed into one atlased texture
            int numSlices = sceneParams.NumSlices;                   // number of slice atlases
            int ppAtlas = volumeWidth * volumeHeight * sliceDepth; // pixels per atlased texture
            int ppSlice = volumeWidth * volumeHeight;              // pixels per volume slice

            NativeArray<byte> rgbPixels = rgbVolumeTexture.GetPixelData<byte>(0);
            NativeArray<byte> alphaPixels = alphaVolumeTexture.GetPixelData<byte>(0);

            Debug.Assert(rgbPixels.Length == numSlices * ppAtlas * 3, "Mismatching RGB Texture Data. Expected: " + numSlices * ppAtlas * 3 + ". Actual: + " + rgbPixels.Length);
            Debug.Assert(alphaPixels.Length == numSlices * ppAtlas, "Mismatching alpha Texture Data. Expected: " + numSlices * ppAtlas + ". Actual: + " + alphaPixels.Length);

            for (int i = 0; i < numSlices; i++) {
                // rgba images are in ARGB format!
                NativeArray<byte> rgbaImageFourSlices = rgbaArray[i].GetPixelData<byte>(0);
                Debug.Assert(rgbaImageFourSlices.Length == ppAtlas * 4, "Mismatching RGBA Texture Data. Expected: " + ppAtlas * 4 + ". Actual: + " + rgbaImageFourSlices.Length);

                for (int s_r = sliceDepth - 1, s = 0; s_r >= 0; s_r--, s++) {

                    int baseIndexRGB = (i * ppAtlas + s * ppSlice) * 3;
                    int baseIndexAlpha = (i * ppAtlas + s * ppSlice);
                    for (int j = 0; j < ppSlice; j++) {
                        rgbPixels[baseIndexRGB + (j * 3)] = rgbaImageFourSlices[((s_r * ppSlice + j) * 4) + 1];
                        rgbPixels[baseIndexRGB + (j * 3) + 1] = rgbaImageFourSlices[((s_r * ppSlice + j) * 4) + 2];
                        rgbPixels[baseIndexRGB + (j * 3) + 2] = rgbaImageFourSlices[((s_r * ppSlice + j) * 4) + 3];
                        alphaPixels[baseIndexAlpha + j] = rgbaImageFourSlices[((s_r * ppSlice + j) * 4)];
                    }
                }
            }

            FlipY<Color24>(rgbVolumeTexture);
            FlipZ<Color24>(rgbVolumeTexture, sceneParams.AtlasBlocksZ);
            FlipY<byte>(alphaVolumeTexture);
            FlipZ<byte>(alphaVolumeTexture, sceneParams.AtlasBlocksZ);

            rgbVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            alphaVolumeTexture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
        }

        /// <summary>
        /// Fills an existing 3D RGBA texture from atlased PNGs.
        /// This method flips both y and z axes of the 3D texture, because the original code assumes we're
        /// indexing from top left, but Unity loaded the PNGs starting bottom right.
        /// </summary>
        private static void LoadVolumeTexture(Texture2D[] featureImages, Texture3D featureVolumeTexture, SceneParams sceneParams) {
            Debug.Assert(sceneParams.NumSlices == featureImages.Length, "Expected " + sceneParams.NumSlices + " feature slices, but found " + featureImages.Length);

            int volumeWidth = sceneParams.AtlasWidth;
            int volumeHeight = sceneParams.AtlasHeight;
            int volumeDepth = sceneParams.AtlasDepth;

            int sliceDepth = 4;                                       // slices packed into one atlased texture
            long numSlices = sceneParams.NumSlices;                   // number of slice atlases
            int ppAtlas = volumeWidth * volumeHeight * sliceDepth; // pixels per atlased feature texture
            int ppSlice = volumeWidth * volumeHeight;              // pixels per volume slice

            NativeArray<Color32> featurePixels = featureVolumeTexture.GetPixelData<Color32>(0);
            Debug.Assert(featurePixels.Length == numSlices * ppAtlas, "Mismatching RGB Texture Data. Expected: " + numSlices * ppAtlas + ". Actual: + " + featurePixels.Length);

            for (int i = 0; i < numSlices; i++) {

                NativeArray<Color32> _featureImageFourSlices = featureImages[i].GetRawTextureData<Color32>();
                Debug.Assert(_featureImageFourSlices.Length == ppAtlas, "Mismatching feature Texture Data. Expected: " + ppAtlas + ". Actual: + " + _featureImageFourSlices.Length);

                for (int s_r = sliceDepth - 1, s = 0; s_r >= 0; s_r--, s++) {
                    int targetIndex = (i * ppAtlas) + (s * ppSlice);
                    NativeSlice<Color32> dst = new NativeSlice<Color32>(featurePixels, targetIndex, ppSlice);
                    NativeSlice<Color32> src = new NativeSlice<Color32>(_featureImageFourSlices, s_r * ppSlice, ppSlice);

                    dst.CopyFrom(src);
                }
            }
            FlipY<Color32>(featureVolumeTexture);
            FlipZ<Color32>(featureVolumeTexture, sceneParams.AtlasBlocksZ);

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
        /// Flips z - Reverses the order of the depth slices of the given 3D texture.
        /// Atlases might be divided in macro blocks that are treated individually here.
        /// I.e. depth slices are only reversed within a block.
        /// </summary>
        private static void FlipZ<T>(Texture3D texture, int atlasBlocksZ) where T : struct {
            int width = texture.width;
            int height = texture.height;
            int depth = texture.depth;
            int stride = depth / atlasBlocksZ;
            int blockSize = width * height * stride;
            int sliceSize = width * height;

            NativeArray<T> data = texture.GetPixelData<T>(0);
            NativeArray<T> tmp = new NativeArray<T>(sliceSize, Allocator.Temp);

            for (int z = 0; z < atlasBlocksZ; z++) {
                for (int s = 0; s < stride / 2; s++) {
                    int atlasBlock = z * blockSize;
                    int slice1Index = atlasBlock + s * sliceSize;
                    int slice2Index = atlasBlock + ((stride - s - 1) * sliceSize);

                    NativeSlice<T> slice1 = new NativeSlice<T>(data, slice1Index, sliceSize);
                    NativeSlice<T> slice2 = new NativeSlice<T>(data, slice2Index, sliceSize);

                    slice1.CopyTo(tmp);
                    slice1.CopyFrom(slice2);
                    slice2.CopyFrom(tmp);
                }
            }

            tmp.Dispose();
        }

        private struct Color24 {
            public byte r;
            public byte g;
            public byte b;
        }

        private static void CreateRayMarchShader(SceneParams sceneParams) {
            double[][] Weights_0 = sceneParams._0Weights;
            double[][] Weights_1 = sceneParams._1Weights;
            double[][] Weights_2 = sceneParams._2Weights;
            double[] Bias_0 = sceneParams._0Bias;
            double[] Bias_1 = sceneParams._1Bias;
            double[] Bias_2 = sceneParams._2Bias;

            StringBuilder biasListZero = toBiasList(Bias_0);
            StringBuilder biasListOne = toBiasList(Bias_1);
            StringBuilder biasListTwo = toBiasList(Bias_2);

            int channelsZero = Weights_0.Length;
            int channelsOne = Bias_0.Length;
            int channelsTwo = Bias_1.Length;
            int channelsThree = Bias_2.Length;
            int posEncScales = 4;

            string shaderSource = RaymarchShader.Template;
            shaderSource = new Regex("OBJECT_NAME").Replace(shaderSource, $"{_context.SceneNameUpperCase}");
            shaderSource = new Regex("NUM_CHANNELS_ZERO").Replace(shaderSource, $"{channelsZero}");
            shaderSource = new Regex("NUM_POSENC_SCALES").Replace(shaderSource, $"{posEncScales}");
            shaderSource = new Regex("NUM_CHANNELS_ONE").Replace(shaderSource, $"{channelsOne}");
            shaderSource = new Regex("NUM_CHANNELS_TWO").Replace(shaderSource, $"{channelsTwo}");
            shaderSource = new Regex("NUM_CHANNELS_THREE").Replace(shaderSource, $"{channelsThree}");
            shaderSource = new Regex("BIAS_LIST_ZERO").Replace(shaderSource, $"{biasListZero}");
            shaderSource = new Regex("BIAS_LIST_ONE").Replace(shaderSource, $"{biasListOne}");
            shaderSource = new Regex("BIAS_LIST_TWO").Replace(shaderSource, $"{biasListTwo}");

            string shaderAssetPath = GetShaderAssetPath();
            File.WriteAllText(shaderAssetPath, shaderSource);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateMaterial(SceneParams sceneParams) {
            string materialAssetPath = GetMaterialAssetPath();
            Material material;

            if (File.Exists(GetMaterialAssetPath())) {
                material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
            } else {
                string shaderAssetPath = GetShaderAssetPath();
                Shader raymarchShader = AssetDatabase.LoadAssetAtPath<Shader>(shaderAssetPath);
                material = new Material(raymarchShader);
            }

            material.SetInteger("displayMode", 0);
            material.SetInteger("ndc", sceneParams.Ndc ? 1 : 0);

            material.SetVector("minPosition", new Vector4(
                (float)sceneParams.MinX,
                (float)sceneParams.MinY,
                (float)sceneParams.MinZ
                )
            );
            material.SetVector("gridSize", new Vector4(
                sceneParams.GridWidth,
                sceneParams.GridHeight,
                sceneParams.GridDepth
                )
            );
            material.SetVector("atlasSize", new Vector4(
                sceneParams.AtlasWidth,
                sceneParams.AtlasHeight,
                sceneParams.AtlasDepth
                )
            );
            material.SetFloat("voxelSize", (float)sceneParams.VoxelSize);
            material.SetFloat("blockSize", (float)sceneParams.BlockSize);
            int maxStep = Mathf.CeilToInt(new Vector3(sceneParams.GridWidth, sceneParams.GridHeight, sceneParams.GridDepth).magnitude);
            material.SetInteger("maxStep", maxStep);

            // volume texture properties will be assigned when creating the 3D textures to avoid having to load them into memory here

            if (!File.Exists(GetMaterialAssetPath())) {
                AssetDatabase.CreateAsset(material, materialAssetPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Assign volume textures again in case material got recreated.
        /// </summary>
        private static void VerifyMaterial(SceneParams sceneParams) {
            string materialAssetPath = GetMaterialAssetPath();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);

            string rgbAssetPath = GetRGBTextureAssetPath();
            string alphaAssetPath = GetAlphaTextureAssetPath();
            string featureAssetPath = GetFeatureTextureAssetPath();
            string atlasAssetPath = GetAtlasTextureAssetPath();

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

        private static void CreateWeightTextures(SceneParams sceneParams) {
            Texture2D weightsTexZero = createFloatTextureFromData(sceneParams._0Weights);
            Texture2D weightsTexOne = createFloatTextureFromData(sceneParams._1Weights);
            Texture2D weightsTexTwo = createFloatTextureFromData(sceneParams._2Weights);
            AssetDatabase.CreateAsset(weightsTexZero, GetWeightsAssetPath(0));
            AssetDatabase.CreateAsset(weightsTexOne, GetWeightsAssetPath(1));
            AssetDatabase.CreateAsset(weightsTexTwo, GetWeightsAssetPath(2));

            string materialAssetPath = GetMaterialAssetPath();
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
        private static void CreatePrefab(SceneParams sceneParams) {
            GameObject prefabObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefabObject.name = _context.SceneName;
            MeshRenderer renderer = prefabObject.GetComponent<MeshRenderer>();
            string materialAssetPath = GetMaterialAssetPath();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
            renderer.material = material;
            MeshFilter meshFilter = prefabObject.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateMesh(meshFilter.sharedMesh, sceneParams);
            meshFilter.sharedMesh.name = $"{_context.SceneName}_volume_mesh";
            AssetDatabase.CreateAsset(meshFilter.sharedMesh, GetMeshAssetPath());
            prefabObject.AddComponent<EnableDepthTexture>();
            PrefabUtility.SaveAsPrefabAsset(prefabObject, GetPrefabAssetPath());
            GameObject.DestroyImmediate(prefabObject);
        }

        /// <summary>
        /// Creates a copy of the default cube mesh
        /// and scales by volume extents to be able to raymarch in object space.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="sceneParams"></param>
        /// <returns></returns>
        private static Mesh CreateMesh(Mesh mesh, SceneParams sceneParams) {
            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
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
                vertices = vertices,
                triangles = mesh.triangles,
                uv = mesh.uv,
                normals = mesh.normals,
                colors = mesh.colors,
                tangents = mesh.tangents
            };
            return newMesh;
        }
    }
}