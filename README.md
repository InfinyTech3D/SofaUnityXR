# SofaUnityXR Asset

[![Documentation](https://img.shields.io/badge/doc-on_website-green.svg)](https://infinytech3d.com/sapapi-unity3d/)
[![Support](https://img.shields.io/badge/support-on_GitHub_Discussions-blue.svg)](https://github.com/InfinyTech3D/SofaUnity/discussions/)
[![Discord](https://img.shields.io/badge/chat-on_Discord-darkred.svg)](https://discord.gg/G63t3a8Ra6)
[![Contact](https://img.shields.io/badge/contact-on_website-orange.svg)](https://infinytech3d.com/contact/)
[![Support us](https://img.shields.io/badge/support_us-on_Github_Sponsor-purple.svg)](https://github.com/sponsors/InfinyTech3D)


## Description
SofaUnityXR extends the [SofaUnity](https://github.com/InfinyTech3D/SofaUnity) asset by adding script for VR and XR device support to interact with SOFA simulations embedded in Unity3D.
It allows users to interact with SOFA's advanced physical simulation capabilities in real-time through VR controllers and headsets using Unity’s XR system, enabling immersive medical, robotic, or scientific training applications.

This package need [SofaUnity](https://github.com/InfinyTech3D/SofaUnity) Asset to work. It only provide the bridge to use SOFA in VR through Unity. No complexe surgical or robotic scenario are provided.
<p align="center">
	<img src="./Doc/img/LiverInteraction_03.jpg" style="width:80%;"/>
</p>


### Compatibility:
* Tested on Unity version > 2022.3.x (Tested with LTS 2022.3 and 2023.3)
* SOFA version > 24.06 with SofaVerseAPI
* Tested on Windows platforms only

### Installation guide
Full installation process available [here](https://infinytech3d.com/getting-started/).


## Dependencies
+ Requires SofaUnity asset installed.
+ XR Interaction Toolkit (tested with v4.4.0)
    + Starter Asset
+ OpenXR Plugin (tested with v1.10.0)
+ Input System (tested with v1.7.0)

## Examples
Two examples are provided in the Scenes folder
- Demo-01_SimpleLiver.unity: Provides a simple integration of SOFA deformable liver simulation in VR, allowing to play/restart simulation and show different models.
- Demo-02_LiverInteraction.unity: Demonstrate how to interact between VR controllers (with SOFA sphere collisions) and deformable liver.

Here are a some results of the basic integration:
|<img align="center" height="250" src="./Doc/img/LiverIntegration_01.jpg">|<img align="center" height="250" src="./Doc/img/LiverIntegration_02.jpg">|
|--|--|--|
| Simple Liver simulation | FEM and sphere collision display |

## License
This main Unity asset is under Standard Unity Asset Store EULA
Other license formats can be provided for commercial use. For more information check [InfinyTech3D license page](https://infinytech3d.com/licenses/).