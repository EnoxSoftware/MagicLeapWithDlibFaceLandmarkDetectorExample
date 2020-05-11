# MagicLeap With DlibFaceLandmarkDetector Example


## Environment
* MagicLeapOne Lumin OS 0.98.10
* Lumin SDK 0.24.1
* Unity 2019.3.10f1 (64-bit)  
* [OpenCV for Unity](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088?aid=1011l4ehR) 2.3.9+ 
* [Dlib FaceLandmarkDetector](https://assetstore.unity.com/packages/tools/integration/dlib-facelandmark-detector-64314?aid=1011l4ehR) 1.3.0+ 


## Setup
1. Download the latest release unitypackage. [MagicLeapWithDlibFaceLandmarkDetectorExample.unitypackage](https://github.com/EnoxSoftware/MagicLeapWithDlibFaceLandmarkDetectorExample/releases)
1. Create a new project. (MagicLeapWithDlibFaceLandmarkDetectorExample) and [setup UnityProject](https://developer.magicleap.com/learn/guides/get-started-developing-in-unity).
1. Import the OpenCVForUnity.
    * Setup the OpenCVForUnity. (Tools > OpenCV for Unity > Set Plugin Import Settings)
1. Import the DlibFaceLandmarkDetector.
    * Setup the DlibFaceLandmarkDetector. (Tools > Dlib FaceLandmarkDetector > Set Plugin Import Settings)
    * Move the "DlibFaceLandmarkDetector/StreamingAssets/" folder to the "Assets/" folder.
1. Import the MagicLeapWithDlibFaceLandmarkDetectorExample.unitypackage.
   ![setup.PNG](setup.PNG)
1. Add the "Assets/MagicLeapWithDlibFaceLandmarkDetectorExample/*.unity" files to the "Scenes In Build" list in the "Build Settings" window.
1. Check CameraCapture and ComputerVision checkbox in Publishing Settings.
   ![manifest_settings.PNG](manifest_settings.PNG)
1. Build and Deploy to MagicLeap.


## ScreenShot
![magicleap_dlib.jpg](magicleap_dlib.jpg) 

