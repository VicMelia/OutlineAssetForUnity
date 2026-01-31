# Handy Outlines - Customizable Highlight Effects

## Overview

The Handy Outlines - Customizable Highlight Effects Package is a comprehensive post-processing tool for Unity (URP) designed to render high-quality outlines tailored to your project's artistic style. It offers a wide range of edge detection algorithms, visual styles, and animation effects.

This tool utilizes a custom Render Feature with multiple passes that allow for real-time updates and extensive customization. Whether you're aiming for a cartoonish look or a more realistic effect, this package provides the flexibility and performance needed to enhance your game's visuals.

## Features

### 1. Advanced Filter Algorithms
[cite_start]Choose from various edge detection calculation modes to fit your performance and aesthetic needs [cite: 12-17]:
* [cite_start]**Sobel** (Default) [cite: 21]
* Roberts Cross
* Prewitt
* Scharr
* Laplacian
* Difference of Gaussians (DoG)

### 2. Detection Modes
* **Depth Only:** Calculates outlines based solely on object depth. [cite_start]Configurable thickness, opacity, and reduction (threshold)[cite: 26, 35].
* **Depth + Normals:** Adds interior lines based on surface normals. [cite_start]Includes specific settings for normal thickness and opacity[cite: 36, 40].
    * [cite_start]*Note:* Normal line thickness is clamped to the depth line thickness to prevent visual artifacts[cite: 58].

### 3. Visual Styles & Effects
* **Double Outline:** Switch between Simple (single line) and Double mode. [cite_start]Double mode adds a secondary, thicker line with its own color[cite: 60, 69].
* [cite_start]**Light Color Blending:** Blend the scene's Directional Light color into the outline to integrate it with the environment[cite: 72, 77].
* **Bloom:** Makes the outlines glow using the Global Volume settings. [cite_start]Includes Simple and Intermittent modes[cite: 104, 114].
* [cite_start]**Custom Textures:** Apply and scale textures onto the outline lines for unique artistic effects[cite: 117, 125].

### 4. Noise & Animation
[cite_start]Apply distortion to create a hand-drawn or shaky look[cite: 80]:
* [cite_start]**Presets:** Waves (Low scale) and Pencil (High scale)[cite: 94, 98].
* [cite_start]**Custom:** Define your own noise scale[cite: 100].
* [cite_start]**Animation:** Animate the distortion on X, Y, or both axes with adjustable intensity[cite: 101, 102].

### 5. Layer Exclusion
[cite_start]Define which Unity Layers should be ignored by the outline effect using a Layer Mask[cite: 131, 133].

## Architecture

The tool consists of three main components:

1.  **Outline Feature:** A generic Render Feature that handles the blit operation. It executes 4 passes:
    * *Depth Prepass* (Generates depth map excluding masked layers).
    * *Normal Prepass*.
    * *Outline Pass* (Applies the outline material).
    * [cite_start]*Depth Rewritten* (Maintains pipeline compatibility) [cite: 137-145].
2.  **Handy Outlines Editor Window:** An Editor window where you can setup the tool and customize the outline settings. [cite_start]It updates material properties (like color, thickness, and filters) in real-time[cite: 151, 152].

## Usage

1.  Click on **Tools > Handy Outlines** in the Unity editor to open the Handy Outlines Editor Window.
2.  Click on **Setup tool**.
3.  Configure the settings in the inspector and you're ready to go.

For more detailed instructions, please refer to the [DOCUMENTATION](Documentation/com.viktor_mjs.handy-outlines-customizable-highlight-effects.pdf).

## Known Issues

* **Layer Exclusion in Scene View:** Objects excluded via layers may appear to visually clip through others in the Scene View. [cite_start]This is a visual bug limited to the editor; it works correctly in the Game View[cite: 135, 163].
* **Bloom Detection:** Occasionally, the Bloom integration may not auto-detect the Global Volume in downloaded projects. [cite_start]Verify your Volume settings manually[cite: 116, 164].

## Examples

Here are some examples of what you can create with the Handy Outlines Package:

- Animated hand-drawn outline effect using noise distortion.
- Double outline with different colors and thicknesses.
- Outlines blended with scene lighting for a cohesive look.

## License

This project is licensed under the standard Unity Asset Store EULA. See the [LICENSE](LICENSE) file for details.

## Contact

For any questions or inquiries, please contact [vikoradevv@gmail.com](mailto:vikoradevv@gmail.com).
