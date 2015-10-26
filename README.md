# FusionFramework
## [DEPRECATED]

Fusion Framework is free and open-source (MIT License) set of managed libraries devloped game and graphics application development based on Microsoft .NET Framework 4.0.

The overall architecture of Fusion Framework is very close (but incompatible) to 
Microsoft XNA and Monogame. It based on  http://sharpdx.org and provides the set of easy to use classes to 
gain access to powerful features of DirectX 11 and to use and extend content processing pipeline

Now framework is on early stage of development but already was used 
in several projects. If you have any ideas, suggestions, improvements 
and features requests fill free to contact us. 

**Note:** the breaking changes are possible.

The Fusion Framework offers the following features:
Graphics:
- Direct3D 11.
- Vertex, Hull, Domain, Geometry, Vertex, Pixel and Compute Shaders.
- 2D, 3D and Cube textures.
- 2D and Cube render targets and depth stencil buffers.
- Ubershaders.
- Constant, Vertex and Index buffers.
- Instancing.
- Stream output.
- R/W Structured buffers.
- Scene graph with materials, animations and skinning support.

Stereo Rendering Support:
- Built in stereo rendering pipeline support.
- NVIDIA 3D Vision.
- Dual-Head Stereo (for paired projectors with polarized or Infitec® glasses).
- Interlaced Stereo (for TVs with passive glasses).
- Oculus Rift.
			
Audio:
- 2D sounds.
- 3D sounds.

Input:
- Keyboard.
- Mouse.
- Xbox 360 game pad.
- Oculus Rift head tracking.

Content Pipeline:
- Dependency and change tracking.
- Hot building, rebuilding and reloading.
- NVidia Texture Tools based texture converter.
- Texture atlas generator.
- FBX importer with materials, animation and skinning.
- Abstract asset types for game object description.
- Bitmap fonts.
- Ubershader compiler with pre-generated combinations.

Fusion Framework depends on the following third-party libraries and tools:
- SharpDX.
- BEPUphysics.
- Lidgren Network.
- NVidia Texture Tools.
- Autodesk FBX SDK.
- AngelCode's Bitmap Font Generator.

Issues and Limitations:
- Only x64 architecture is supported.
- Only Feature level 11 is supported.
- INTEL GPUs may not work.
- Very rare crashes on exit.
- Partial multithreading support.

Future works:
- Lower feature levels.
- Support for mobile devices (primarily Windows Phone).
- Standalone content pipeline processing.
- Own implementation of DirectX wrapper.
- FMOD Audio support.
- Wrappers for BEPUphysics and Lidgren network.
- Better multithreading support.
