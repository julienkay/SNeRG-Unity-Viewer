# SNeRG Unity Viewer

This repository contains the source code for a Unity port of the web viewer for [Baking Neural Radiance Fields for Real-Time View Synthesis](https://phog.github.io/snerg/)[^1]

*Please note, that this is an unofficial port. I am not affiliated with the original authors or their institution.*

## Project Status

[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)

There are a number of unresolved issues with this project.
- When zooming in, you can clearly see some visual artifacts on one axis of the 3D volume. This most likely stems from platform-specific differences between the original Three.js code and Unity. There must be either some texture coordinate conventions or coordinate space differences that I haven't been able to wrap my head around and failed to port correctly. This also causes the direction of the view-dependent effects to be incorrect.
- Memory requirements for working with 3D textures in Unity are quite high. The way the import process works right now is that Texture3D assets are created in-Editor. The memory that Unity uses when serializing these as assets on disk is a bit too high for my liking. Granted, the size of these scenes can go up to Gigabytes. However, I would have hoped that 16GB of RAM is enough to handle 2GB of uncompressed 3D textures. But Unity often crashed while running out of memory during import for me, so I'd recommend having at least 32GB RAM or more, especially for some of the larger scenes.  
A possible solution to this would be to import the volumes into Unity as individual 2D slices, (similar to how the official web viewer distributes the data) and only assemble the 3D textures at runtime. For development, it was quite useful to preview the volume without having to enter Play Mode though. But some scenes are currently just very difficult to handle, even when your GPU could fit the data into VRAM just fine.  
This is all assuming you have set the Asset Serialization Mode to 'Force Binary', which I have done for this project. Text serialization would otherwise be close to unusable. Unfortunately, the Asset Serialization Mode seems to be a project-wide setting, which is a bit inconvenient for use in real-world projects I imagine.
- Sometimes the material fails to save references to the various textures that you'll then have to reassign manually.

## Requirements

At least 32GB of RAM and a graphics card with 4GB of VRAM.

## Setup

After cloning the project you can simply use the menu *SNeRG -> Asset Downloads* to download any of the sample scenes available.
In each scene folder, there will be a convenient prefab, that you can then drag into the scene and you're good to go.

If performance is not great, you can try to reduce the "Max Step" property on the material to trade off some quality vs speed.

## Details

This project was created with Unity 2021.3 LTS using the Built-in Render Pipeline.

[^1]: [Peter Hedman and Pratul P. Srinivasan and Ben Mildenhall and Jonathan T. Barron and Paul Debevec. Baking Neural Radiance Fields for Real-Time View Synthesis. ICCV, 2021](https://phog.github.io/snerg/)
