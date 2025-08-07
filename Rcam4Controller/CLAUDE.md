# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Rcam4** is a real-time VFX project that utilizes iPhone LiDAR sensors and Unity VFX Graph. It consists of two Unity projects:

1. **Rcam4Controller** (this project) - iOS app that captures camera and depth data from iPhone LiDAR
2. **Rcam4Visualizer** (sibling project) - Desktop app (PC/Mac) that receives and renders VFX

The Controller captures the scene using iPhone's camera and depth sensors, then transmits the data via NDI (Network Device Interface) to the Visualizer for real-time VFX rendering.

## System Requirements

- Unity 6000.0.40f1 or later
- Controller: iOS device with LiDAR sensor (iPhone 15 Pro, etc.)
- Visualizer: Windows PC or Mac
- NDI network connection between devices

## Project Architecture

### Core Components

- **FrameEncoder**: Captures AR camera and depth data, encodes for NDI transmission
- **InputLinker**: Links UI controls to input system using VJUITK
- **StatusView**: Displays connection and performance status
- **InputHandle**: Manages input state for transmission to Visualizer
- **Metadata**: Serializable camera pose and input data structure

### Key Dependencies

The project uses Unity Package Manager with scoped registries from Keijiro's npm registry:

- `jp.keijiro.klak.ndi` (2.1.4) - NDI network transmission
- `jp.keijiro.metatex` (1.0.4) - Texture processing utilities  
- `jp.keijiro.vjuitk` (1.1.1) - Virtual joystick UI toolkit
- `jp.keijiro.rcam4.common` (local) - Shared components between Controller/Visualizer
- `com.unity.xr.arkit` (6.0.5) - ARKit integration for LiDAR
- `com.unity.render-pipelines.universal` (17.0.4) - URP rendering
- `com.unity.inputsystem` (1.13.1) - Input handling

### Project Structure

```
Assets/
├── Scripts/           # C# scripts for AR capture and UI
├── Shaders/          # Custom shaders for encoding/monitoring
├── UI/               # UI Toolkit files (.uxml, .uss, themes)
├── URP/              # Universal Render Pipeline settings
└── XR/               # ARKit and XR Simulation settings
```

### Data Flow

1. ARFoundation captures camera color and depth textures
2. FrameEncoder encodes data with custom shader and packages with Metadata
3. NDI Sender transmits encoded frames over network
4. InputLinker captures UI input state and includes in Metadata
5. Visualizer receives and decodes frames for VFX rendering

## Common Development Tasks

### Building for iOS
Use Unity's standard iOS build process. The project is configured for iOS with ARKit support and includes NDI native plugins.

### Testing
- Use XR Simulation in Unity Editor for development without physical device
- Test NDI transmission using NDI tools or the companion Visualizer project

### Shared Components
The `Rcam4Common` folder contains shared code between Controller and Visualizer projects. Changes here affect both projects.

## Network Architecture

Uses NDI (Network Device Interface) for real-time video transmission:
- Encoded color/depth frames transmitted as video stream
- Camera metadata transmitted as XML ancillary data
- Requires network connection between iOS device and desktop machine