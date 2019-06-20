# MagicLeap With DlibFaceLandmarkDetector Example


## Environment
* MagicLeapOne Lumin OS 0.96
* Unity 2019.1.0f2 (64-bit)  
* [OpenCV for Unity](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088?aid=1011l4ehR) 2.3.5+ 
* [Dlib FaceLandmarkDetector](https://assetstore.unity.com/packages/tools/integration/dlib-facelandmark-detector-64314?aid=1011l4ehR) 1.2.8+ 


## Setup
1. Download the latest release unitypackage. [MagicLeapWithDlibFaceLandmarkDetectorExample.unitypackage](https://github.com/EnoxSoftware/MagicLeapWithDlibFaceLandmarkDetectorExample/releases)
1. Create a new project. (MagicLeapWithDlibFaceLandmarkDetectorExample)
1. Import "C:/Users/xxx/MagicLeap/tools/unity/v0.21.0/MagicLeap.unitypackage"
1. Import the OpenCVForUnity.
    * Setup the OpenCVForUnity. (Tools > OpenCV for Unity > Set Plugin Import Settings)
1. Import the DlibFaceLandmarkDetector.
    * Setup the DlibFaceLandmarkDetector. (Tools > Dlib FaceLandmarkDetector > Set Plugin Import Settings)
    * Import "Assets/DlibFaceLandmarkDetector/DlibFaceLandmarkDetectorWithOpenCVExample.unitypackage".
    * Move the "DlibFaceLandmarkDetector/StreamingAssets/" folder to the "Assets/" folder.
1. Import the MagicLeapWithDlibFaceLandmarkDetectorExample.unitypackage.
1. Add the "Assets/MagicLeapWithDlibFaceLandmarkDetectorExample/*.unity" files to the "Scenes In Build" list in the "Build Settings" window.
1. Copy "Assets/MagicLeap/Examples/Plugins/Lumin/manifest.xml" to "Assets/Plugins/Lumin/manifest.xml". https://creator.magicleap.com/learn/guides/migrate-unity-editors
1. Build and Deploy to MagicLeap.


## ScreenShot
![magicleap_dlib.jpg](magicleap_dlib.jpg) 