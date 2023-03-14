# SNeRG Unity Viewer

https://user-images.githubusercontent.com/26555424/224686209-113c4a0f-860e-4b8c-9158-430b4ba2d2a9.mp4

This repository contains the source code for a Unity port of the web viewer for [Baking Neural Radiance Fields for Real-Time View Synthesis](https://phog.github.io/snerg/)[^1]

*Please note, that this is an unofficial port. I am not affiliated with the original authors or their institution.*

## Requirements

I recommend 32GB of RAM and a graphics card with 4GB of VRAM.

## Setup

After cloning the project you can simply use the menu *SNeRG -> Asset Downloads* to download any of the sample scenes available.
In each scene folder, there will be a convenient prefab, that you can then drag into the scene and you're good to go.

If performance is not great, you can try to reduce the "Max Step" property on the material to trade off some quality vs speed.

## Updating

Since the initial release a number of issues have been fixed in the asset generation code.
That means, that if you have already imported some scenes before, you'll have to delete these source files and regenerate them by going to *SNeRG -> Asset Downloads* again.

## Details

This project was created with Unity 2021.3 LTS using the Built-in Render Pipeline.

Deviations from the official viewer:
- Rather than doing the raymarching by drawing a full-screen quad, I opted to use a frontface-culled cube in world space instead. From a rendering perspective, there should not be too much of a difference, but it has some workflow advantages in the Unity Editor I think. It is a bit more straightforward to render the volume in Unity's scene view. It's also easier to integrate into existing projects because it allows other components to work directly with that GameObject's transform hierarchy (e.g. Physics, scripts from various XR Interaction SDKs, ...). 
- I added manual depth testing, so the volume correctly interacts with other (opaque) mesh-based objects in the scene. This usually requires a [depth pre-pass](https://docs.unity3d.com/Manual/SL-CameraDepthTexture.html) though.

## Known Issues

- Memory requirements for working with 3D textures in Unity are quite high. The way the import process works right now is that Texture3D assets are created in-Editor. The memory that Unity uses when serializing these as assets on disk is a bit too high for my liking. Granted, the size of these scenes can go up to Gigabytes. However, I've found that 16GB of RAM was often not enough to handle the import/creation of 2GB of uncompressed 3D textures due to inefficiencies. I wasn’t even able to import some of the larger scenes until upgrading to 32GB RAM.  
A possible solution to this might be to defer the 3D texture creation and import the volumes into Unity as individual 2D slices, (similar to how the official web viewer distributes the data) and only assemble the 3D textures at runtime. For development, it was quite useful to preview the volume without having to enter Play Mode though. But some scenes are currently just very difficult to handle, even when your GPU could fit the data into VRAM just fine.  
This is all assuming you have set the Asset Serialization Mode to 'Force Binary', which I have done for this project. Text serialization would otherwise be close to unusable. Unfortunately, the Asset Serialization Mode seems to be a project-wide setting, which is a bit inconvenient for use in real-world projects I imagine.  
I should have probably looked more into texture compression options, but the little information I found on compression support on various platforms for 3D textures specifically didn't instill much confidence..  
Fortunately, [MERF](https://merf42.github.io/) reduces these requirements by quite a bit.

[^1]: [Peter Hedman and Pratul P. Srinivasan and Ben Mildenhall and Jonathan T. Barron and Paul Debevec. Baking Neural Radiance Fields for Real-Time View Synthesis. ICCV, 2021](https://phog.github.io/snerg/)
