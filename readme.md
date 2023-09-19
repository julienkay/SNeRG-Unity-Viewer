# SNeRG Unity Viewer

https://user-images.githubusercontent.com/26555424/224686209-113c4a0f-860e-4b8c-9158-430b4ba2d2a9.mp4

This repository contains the source code for a Unity port of the web viewer for [Baking Neural Radiance Fields for Real-Time View Synthesis](https://phog.github.io/snerg/)[^1]

*Please note, that this is an unofficial port. I am not affiliated with the original authors or their institution.*

## Requirements

I recommend 32GB of RAM and a graphics card with 4GB of VRAM.

## Usage

### Installation

Go to the [releases section](https://github.com/julienkay/SNeRG-Unity-Viewer/releases/latest), download the Unity Package, and import it into any Unity project. This is a 'Hybrid Package' that will install into your project as a local package.

##### Alternatives

<details>
  <summary> UPM Package via OpenUPM </summary>
  
  In `Edit -> Project Settings -> Package Manager`, add a new scoped registry:

    Name: Doji
    URL: https://package.openupm.com
    Scope(s): com.doji
 
  In the Package Manager install 'com.doji.snerg' either by name or via `Package Manager -> My Registries`
</details>

<details>
  <summary> UPM Package via Git URL </summary>
  
  In `Package Manager -> Add package from git URL...` paste `https://github.com/julienkay/SNeRG-Unity-Viewer.git` [as described here](https://docs.unity3d.com/Manual/upm-ui-giturl)
</details>

### Importing sample scenes

After succesful installation, you can use the menu *SNeRG -> Asset Downloads* to download any of the sample scenes available.
In each scene folder there will be a convenient prefab, that you can then drag into the scene and you're good to go.

### Updating

Since the initial release a number of issues have been fixed in the asset generation code.
That means, that if you have already imported some scenes before, you'll have to delete these source files and regenerate them by going to *SNeRG -> Asset Downloads* again.

### Importing self-trained scenes

If you have successfully trained your own SNeRG scenes using the [official code release](https://github.com/google-research/google-research/tree/master/snerg) and want to render them in Unity, you can use the menu *SNeRG -> Import from disk*.
This lets you choose a folder that must contain the output files of your training process.

More specifically, the following assets are required:
- a file named 'scene_params.json'
- a PNG file for the 3D atlas texture: atlas_indices.png
- several PNG files for the 3D feature texture: feature_XXX.png - feature_YYY.png
- several PNG files for the 3D RGB and alpha texture: rgba_XXX.png - rgba_YYY.png

## Details

This project was created with Unity 2021.3 LTS using the Built-in Render Pipeline.

Deviations from the official viewer:
- Rather than doing the raymarching by drawing a full-screen quad, I opted to use a frontface-culled cube in world space instead. From a rendering perspective, there should not be too much of a difference, but it has some workflow advantages in the Unity Editor I think. It is a bit more straightforward to render the volume in Unity's scene view. It's also easier to integrate into existing projects because it allows other components to work directly with that GameObject's transform hierarchy (e.g. Physics, scripts from various XR Interaction SDKs, ...). 
- I added manual depth testing, so the volume correctly interacts with other (opaque) mesh-based objects in the scene. This usually requires a [depth pre-pass](https://docs.unity3d.com/Manual/SL-CameraDepthTexture.html) though.

## Known Issues

- Memory requirements for working with 3D textures in Unity are quite high. The way the import process works right now is that Texture3D assets are created in-Editor. The memory that Unity uses when serializing these as assets on disk is a bit too high for my liking. Granted, the size of these scenes can go up to Gigabytes. However, I've found that 16GB of RAM was often not enough to handle the import/creation of 2GB of uncompressed 3D textures due to inefficiencies. I wasn’t even able to import some of the larger scenes until upgrading to 32GB RAM.

- Unity straight-up errors out when trying to create 3D textures with sizes of 2GB or larger. Some of the feature volumes like the ficus scene (2048x2048x128) hit that limit and can not be imported.

### Possible Workarounds

- One possible solution to handling the data in-Editor might be to defer the 3D texture creation and import the volumes into Unity as individual 2D slices, (similar to how the official web viewer distributes the data) and only assemble the 3D textures at runtime. For development, it was quite useful to preview the volume without having to enter Play Mode though. But some scenes are currently just very difficult to handle, even when your GPU could fit the data into VRAM just fine.

- I should have probably looked more into texture compression options for 3D textures 

- I've set the Asset Serialization Mode to 'Force Binary' with some success of reducing memory consumption during import. Unfortunately, the Asset Serialization Mode is a project-wide setting, which is a bit inconvenient, because in real-world projects text serialization is more commonly used I suppose.

For future improvements, [MERF](https://merf42.github.io/) looks promising in terms of reducing memory usage.

## References

Other projects exploring NeRFs and related techniques in Unity:
- [MobileNeRF (Exploiting the Polygon Rasterization Pipeline for Efficient Neural Field Rendering on Mobile Architectures)](https://github.com/julienkay/MobileNeRF-Unity-Viewer)
- [MERF (Memory Efficient Radiance Fields)](https://github.com/julienkay/MERF-Unity-Viewer)

[^1]: [Peter Hedman and Pratul P. Srinivasan and Ben Mildenhall and Jonathan T. Barron and Paul Debevec. Baking Neural Radiance Fields for Real-Time View Synthesis. ICCV, 2021](https://phog.github.io/snerg/)
