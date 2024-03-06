# FFT-Ocean
This project demonstrates two implementations of the sum of sines algorithm for real-time, 3D ocean shading: a custom wave dispersion function based on wavenumber, and a wave dispersion function based on the Phillips wave energy spectrum and positive cosine squared directional spread. The project is set up to run either implementation as independent shaders on separate ocean meshes.

This project is presented in fulfillment of my College of Wooster Senior Independent Study: A Performance Analysis of the Sum of Sines and Fast Fourier Transform Algorithms for Real-Time Ocean Rendering.

## Code Structure
### `FastFourierTransform.compute`
This compute shader implements the kernel functions required to calculate the inverse and regular Fast Fourier Transforms. No modifications were made to this class.

### `InitialSpectrum.compute`
This compute shader implements the kernel functions required to calculate the initial wave energy spectrum for the Fast Fourier Transform. No modifications were made to this class.

### `TimeDependentSpectrum.compute`
This compute shader implements the kernel functions required to calculate the amplitude at a given vertex and time. No modifications were made to this class.

### `WavesTextureMerger.compute`
This compute shader implements the kernel functions required to combine the ocean heightmap textures and foam textures. No modifications were made to this class.

### `FastFourierTransform.cs`
This class controls the invocation of kernel functions for the computation of the inverse and regular Fast Fourier Transform. No modifications were made to this class.

### `FastFourierTransform.cs`
This class provides a method to print all available ProfilerRecorders to the console upon startup. This is necessary to determine what data the Unity project is able to collect while profiling. This class can be added to any Unity project without dependencies. This class is used verbatim from the Unity 2022.3 documentation.

### `OceanGeometry.cs`
This class controls the instantiation of ocean tile objects in Unity. This class is modified from the original to remove mesh tiling and level-of-detail cascades. Code that is removed from the original is left in comments.

**Significant Parameters:**
* vertexDensity: Controls the dimension of the ocean mesh.

### `Profiler.cs`
This class samples average , minimum , and maximum frame rate (FPS), draw time (ms), and memory usage (MB) for any Godot project . Performance data is sampled every 10 frames (see ‘sample_rate ‘) and averages are calculated every 500 frames (see ‘print_rate ‘). The latest performance data and number of elapsed frames are displayed in a GUI overlay of the Godot project.

This class can be added without dependencies to any Godot project

### `SimpleCameraController.cs`
This class manages user-driven camera movement in the ocean scene. No modifications were made to this class.

### `WavesCascade.cs`
This class controls the invocation of kernel functions required to initialize the wave energy spectrum, twiddle factors, and compute the Inverse Fast Fourier Transform. No modifications were made to this class.

### `WavesGenerator.cs`
This class manages the calculation of ocean heightmap textures for three LOD cascades. Results from `WavesCascade.cs` are synced. No modifications were made to this class.

**Significant Parameters:**
* size: Controls the number of waves sampled for the FFT.

### `WavesSettings.cs`
This class sets up some of the parameters required for the calculation of the wave energy spectrum and Fast Fourier Transform. No modifications were made to this class.

### `Ocean2.shader`
This HLSL shader applies the heightmap textures produces by the Fast Fourier Transform compute shader to the ocean mesh and applies a simple diffuse lighting model as the fragment shader. The original version of this program included Jacobbian foam generation in the fragment shader.


## Code References
* Original FFT project: https://github.com/gasgiant/FFT-Ocean
* Original ProfilerRecorder project: https://docs.unity3d.com/ScriptReference/Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderDescription.html
* Basic ProfilerRecorder example: https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerRecorder.html
