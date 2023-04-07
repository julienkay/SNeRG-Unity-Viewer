using Newtonsoft.Json;

namespace SNeRG.Editor {
    public partial class SceneParams {
        [JsonProperty("voxel_size")]
        public double VoxelSize { get; set; }

        [JsonProperty("block_size")]
        public int BlockSize { get; set; }

        [JsonProperty("grid_width")]
        public int GridWidth { get; set; }

        [JsonProperty("grid_height")]
        public int GridHeight { get; set; }

        [JsonProperty("grid_depth")]
        public int GridDepth { get; set; }

        [JsonProperty("atlas_width")]
        public int AtlasWidth { get; set; }

        [JsonProperty("atlas_height")]
        public int AtlasHeight { get; set; }

        [JsonProperty("atlas_depth")]
        public int AtlasDepth { get; set; }

        [JsonProperty("num_slices")]
        public int NumSlices { get; set; }

        [JsonProperty("min_x")]
        public double MinX { get; set; }

        [JsonProperty("min_y")]
        public double MinY { get; set; }

        [JsonProperty("min_z")]
        public double MinZ { get; set; }

        [JsonProperty("atlas_blocks_x")]
        public int AtlasBlocksX { get; set; }

        [JsonProperty("atlas_blocks_y")]
        public int AtlasBlocksY { get; set; }

        [JsonProperty("atlas_blocks_z")]
        public int AtlasBlocksZ { get; set; }

        [JsonProperty("worldspace_T_opengl")]
        public float[][] WorldspaceTOpengl { get; set; }

        [JsonProperty("ndc")]
        public bool Ndc { get; set; }

        [JsonProperty("0_weights")]
        public double[][] _0Weights { get; set; }

        [JsonProperty("1_weights")]
        public double[][] _1Weights { get; set; }

        [JsonProperty("2_weights")]
        public double[][] _2Weights { get; set; }

        [JsonProperty("0_bias")]
        public double[] _0Bias { get; set; }

        [JsonProperty("1_bias")]
        public double[] _1Bias { get; set; }

        [JsonProperty("2_bias")]
        public double[] _2Bias { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
    }
}