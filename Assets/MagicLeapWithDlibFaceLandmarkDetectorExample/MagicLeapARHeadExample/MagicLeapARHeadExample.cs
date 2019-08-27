
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DlibFaceLandmarkDetector;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using MagicLeapWithDlibFaceLandmarkDetector.UnityUtils.Helper;
using UnityEngine.XR.MagicLeap;
using OpenCVForUnity.ObjdetectModule;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace MagicLeapWithDlibFaceLandmarkDetectorExample
{
    /// <summary>
    /// MagicLeap ARHead Example
    /// This example was referring to http://www.morethantechnical.com/2012/10/17/head-pose-estimation-with-opencv-opengl-revisited-w-code/
    /// and use effect asset from http://ktk-kumamoto.hatenablog.com/entry/2014/09/14/092400.
    /// </summary>
    [RequireComponent(typeof(MLCameraPreviewToMatHelper), typeof(ImageOptimizationHelper))]
    public class MagicLeapARHeadExample : MonoBehaviour
    {
        /// <summary>
        /// Determines if displays face points.
        /// </summary>
        public bool displayFacePoints;

        /// <summary>
        /// The display face points toggle.
        /// </summary>
        public Toggle displayFacePointsToggle;

        /// <summary>
        /// Determines if displays display axes
        /// </summary>
        public bool displayAxes;

        /// <summary>
        /// The display axes toggle.
        /// </summary>
        public Toggle displayAxesToggle;

        /// <summary>
        /// Determines if displays head.
        /// </summary>
        public bool displayHead;

        /// <summary>
        /// The display head toggle.
        /// </summary>
        public Toggle displayHeadToggle;

        /// <summary>
        /// Determines if displays effects.
        /// </summary>
        public bool displayEffects;

        /// <summary>
        /// The display effects toggle.
        /// </summary>
        public Toggle displayEffectsToggle;

        [Space(10)]

        /// <summary>
        /// The axes.
        /// </summary>
        public GameObject axes;

        /// <summary>
        /// The head.
        /// </summary>
        public GameObject head;

        /// <summary>
        /// The right eye.
        /// </summary>
        public GameObject rightEye;

        /// <summary>
        /// The left eye.
        /// </summary>
        public GameObject leftEye;

        /// <summary>
        /// The mouth.
        /// </summary>
        public GameObject mouth;

        /// <summary>
        /// The AR camera.
        /// </summary>
        public Camera ARCamera;

        /// <summary>
        /// The AR game object.
        /// </summary>
        public GameObject ARGameObject;

        [Space(10)]

        /// <summary>
        /// The enable low pass filter toggle.
        /// </summary>
        public Toggle enableLowPassFilterToggle;

        /// <summary>
        /// Determines if enable low pass filter.
        /// </summary>
        public bool enableLowPassFilter;

        /// <summary>
        /// The position low pass. (Value in meters)
        /// </summary>
        public float positionLowPass = 8f;
        //4

        /// <summary>
        /// The rotation low pass. (Value in degrees)
        /// </summary>
        public float rotationLowPass = 4f;
        //2

        /// <summary>
        /// The old pose data.
        /// </summary>
        PoseData oldPoseData;

        /// <summary>
        /// The mouth particle system.
        /// </summary>
        ParticleSystem[] mouthParticleSystem;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;

        /// <summary>
        /// The cameraparam matrix.
        /// </summary>
        Mat camMatrix;

        /// <summary>
        /// The distortion coeffs.
        /// </summary>
        MatOfDouble distCoeffs;

        /// <summary>
        /// The matrix that inverts the Y-axis.
        /// </summary>
        Matrix4x4 invertYM;

        /// <summary>
        /// The matrix that inverts the Z-axis.
        /// </summary>
        Matrix4x4 invertZM;

        /// <summary>
        /// The matrix that AR camera P * V.
        /// </summary>
        Matrix4x4 VP;

        /// <summary>
        /// The transformation matrix.
        /// </summary>
        Matrix4x4 transformationM = new Matrix4x4();

        /// <summary>
        /// The transformation matrix for AR.
        /// </summary>
        Matrix4x4 ARM;

        /// <summary>
        /// The 3d face object points.
        /// </summary>
        MatOfPoint3f objectPoints68;

        /// <summary>
        /// The 3d face object points.
        /// </summary>
        MatOfPoint3f objectPoints17;

        /// <summary>
        /// The 3d face object points.
        /// </summary>
        MatOfPoint3f objectPoints6;

        /// <summary>
        /// The 3d face object points.
        /// </summary>
        MatOfPoint3f objectPoints5;

        /// <summary>
        /// The image points.
        /// </summary>
        MatOfPoint2f imagePoints;

        /// <summary>
        /// The rvec.
        /// </summary>
        Mat rvec;

        /// <summary>
        /// The tvec.
        /// </summary>
        Mat tvec;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        MLCameraPreviewToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The dlib shape predictor file name.
        /// </summary>
        string dlibShapePredictorFileName = "sp_human_face_68.dat";

        /// <summary>
        /// The dlib shape predictor file path.
        /// </summary>
        string dlibShapePredictorFilePath;

#if UNITY_WEBGL && !UNITY_EDITOR
        IEnumerator getFilePath_Coroutine;
#endif

        /// <summary>
        /// The image optimization helper.
        /// </summary>
        ImageOptimizationHelper imageOptimizationHelper;

        /// <summary>
        /// Determines if enable downscale.
        /// </summary>
        public bool enableDownScale;


        /// <summary>
        /// Determines if enable skipframe.
        /// </summary>
        public bool enableSkipFrame;


        /// <summary>
        /// Determines if use OpenCV FaceDetector for face detection.
        /// </summary>
        public bool useOpenCVFaceDetector;

        /// <summary>
        /// The cascade.
        /// </summary>
        CascadeClassifier cascade;


        /// <summary>
        /// The gray mat.
        /// </summary>
        Mat grayMat;

        /// <summary>
        /// The haarcascade_frontalface_alt_xml_filepath.
        /// </summary>
        string haarcascade_frontalface_alt_xml_filepath;

        /// <summary>
        /// The detection result.
        /// </summary>
        List<UnityEngine.Rect> detectResult;       

        /// <summary>
        /// The camera offset matrix.
        /// </summary>
        Matrix4x4 cameraOffsetM = Matrix4x4.Translate(new Vector3(-0.07f, -0.005f, 0));

        readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();
        System.Object sync = new System.Object();

        bool _isThreadRunning = false;
        bool isThreadRunning
        {
            get
            {
                lock (sync)
                    return _isThreadRunning;
            }
            set
            {
                lock (sync)
                    _isThreadRunning = value;
            }
        }

        bool _isDetecting = false;
        bool isDetecting
        {
            get
            {
                lock (sync)
                    return _isDetecting;
            }
            set
            {
                lock (sync)
                    _isDetecting = value;
            }
        }

        // Use this for initialization
        void Start()
        {
            //displayFacePointsToggle.isOn = displayFacePoints;
            //displayAxesToggle.isOn = displayAxes;
            //displayHeadToggle.isOn = displayHead;
            //displayEffectsToggle.isOn = displayEffects;
            //enableLowPassFilterToggle.isOn = enableLowPassFilter;

            imageOptimizationHelper = gameObject.GetComponent<ImageOptimizationHelper>();
            webCamTextureToMatHelper = gameObject.GetComponent<MLCameraPreviewToMatHelper>();


#if UNITY_WEBGL && !UNITY_EDITOR
            getFilePath_Coroutine = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePathAsync (dlibShapePredictorFileName, (result) => {
                getFilePath_Coroutine = null;

                dlibShapePredictorFilePath = result;
                Run ();
            });
            StartCoroutine (getFilePath_Coroutine);
#else
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.UnityUtils.Utils.getFilePath("haarcascade_frontalface_alt.xml");
            dlibShapePredictorFilePath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath(dlibShapePredictorFileName);
            Run();
#endif
        }

        private void Run()
        {
            if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
            {
                Debug.LogError("shape predictor file does not exist. Please copy from “DlibFaceLandmarkDetector/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
            }

            //set 3d face object points.
            objectPoints68 = new MatOfPoint3f(
                new Point3(-34, 90, 83),//l eye (Interpupillary breadth)
                new Point3(34, 90, 83),//r eye (Interpupillary breadth)
                new Point3(0.0, 50, 117),//nose (Tip)
                new Point3(0.0, 32, 97),//nose (Subnasale)
                new Point3(-79, 90, 10),//l ear (Bitragion breadth)
                new Point3(79, 90, 10)//r ear (Bitragion breadth)
            );

            objectPoints17 = new MatOfPoint3f(
                new Point3(-34, 90, 83),//l eye (Interpupillary breadth)
                new Point3(34, 90, 83),//r eye (Interpupillary breadth)
                new Point3(0.0, 50, 117),//nose (Tip)
                new Point3(0.0, 32, 97),//nose (Subnasale)
                new Point3(-79, 90, 10),//l ear (Bitragion breadth)
                new Point3(79, 90, 10)//r ear (Bitragion breadth)
            );

            objectPoints6 = new MatOfPoint3f(
                new Point3(-34, 90, 83),//l eye (Interpupillary breadth)
                new Point3(34, 90, 83),//r eye (Interpupillary breadth)
                new Point3(0.0, 50, 117),//nose (Tip)
                new Point3(0.0, 32, 97)//nose (Subnasale)
            );

            objectPoints5 = new MatOfPoint3f(
                new Point3(-23, 90, 83),//l eye (Inner corner of the eye)
                new Point3(23, 90, 83),//r eye (Inner corner of the eye)
                new Point3(-50, 90, 80),//l eye (Tail of the eye)
                new Point3(50, 90, 80),//r eye (Tail of the eye)
                new Point3(0.0, 32, 97)//nose (Subnasale)
            );


            // adjust object points to the scale of real world space.
            AjustPointScale(objectPoints68, 0.001);
            AjustPointScale(objectPoints17, 0.001);
            AjustPointScale(objectPoints6, 0.001);
            AjustPointScale(objectPoints5, 0.001);


            imagePoints = new MatOfPoint2f();

            faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);


            cascade = new CascadeClassifier(haarcascade_frontalface_alt_xml_filepath);
            if (cascade.empty())
            {
                Debug.LogError("cascade file is not loaded. Please copy from “OpenCVForUnity/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
            }


#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
            webCamTextureToMatHelper.Initialize();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();
            Mat downscaleMat = imageOptimizationHelper.GetDownScaleMat(webCamTextureMat);

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            //Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

#if PLATFORM_LUMIN && !UNITY_EDITOR

            // get camera intrinsic calibration parameters.
            MLCVCameraIntrinsicCalibrationParameters outParameters = new MLCVCameraIntrinsicCalibrationParameters();
            MLResult result = MLCamera.GetIntrinsicCalibrationParameters(out outParameters);
            if (result.IsOk)
            {
                //Imgproc.putText(rgbaMat, "Width : " + outParameters.Width, new Point(20, 90), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                //Imgproc.putText(rgbaMat, "Height : " + outParameters.Height, new Point(20, 180), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                //Imgproc.putText(rgbaMat, "FocalLength : " + outParameters.FocalLength, new Point(20, 270), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                //Imgproc.putText(rgbaMat, "FOV : " + outParameters.FOV, new Point(20, 360), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                //Imgproc.putText(rgbaMat, "PrincipalPoint : " + outParameters.PrincipalPoint, new Point(20, 450), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                //Imgproc.putText(rgbaMat, "Distortion [0] : " + outParameters.Distortion[0], new Point(20, 540), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                //Imgproc.putText(rgbaMat, "Distortion [1] : " + outParameters.Distortion[1], new Point(20, 630), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                //Imgproc.putText(rgbaMat, "Distortion [2] : " + outParameters.Distortion[2], new Point(20, 720), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                //Imgproc.putText(rgbaMat, "Distortion [3] : " + outParameters.Distortion[3], new Point(20, 810), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                //Imgproc.putText(rgbaMat, "Distortion [4] : " + outParameters.Distortion[4], new Point(20, 900), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);

            }
            else
            {
                if (result.Code == MLResultCode.PrivilegeDenied)
                {
                    //Imgproc.putText(rgbaMat, "MLResultCode.PrivilegeDenied", new Point(20, 90), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                }
                else if (result.Code == MLResultCode.UnspecifiedFailure)
                {
                    //Imgproc.putText(rgbaMat, "MLResultCode.UnspecifiedFailure", new Point(20, 90), Imgproc.FONT_HERSHEY_SIMPLEX, 3, new Scalar(255, 0, 0, 255), 5);
                }
            }

            Matrix4x4 pm = ARUtils.CalculateProjectionMatrixFromCameraMatrixValues(outParameters.FocalLength.x, outParameters.FocalLength.y,
                outParameters.PrincipalPoint.x, outParameters.PrincipalPoint.y, outParameters.Width, outParameters.Height, 0.3f, 1000f);
            Debug.Log("CalculateProjectionMatrix: " + pm.ToString());



            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            float imageSizeScale = 1.0f;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;            
            
            //set cameraparam
            //int max_d = (int)Mathf.Max (width, height);
            double fx = outParameters.FocalLength.x;
            double fy = outParameters.FocalLength.y;
            double cx = outParameters.PrincipalPoint.x;
            double cy = outParameters.PrincipalPoint.y;
            camMatrix = new Mat (3, 3, CvType.CV_64FC1);
            camMatrix.put (0, 0, fx);
            camMatrix.put (0, 1, 0);
            camMatrix.put (0, 2, cx);
            camMatrix.put (1, 0, 0);
            camMatrix.put (1, 1, fy);
            camMatrix.put (1, 2, cy);
            camMatrix.put (2, 0, 0);
            camMatrix.put (2, 1, 0);
            camMatrix.put (2, 2, 1.0f);
            Debug.Log ("camMatrix " + camMatrix.dump ());

            distCoeffs = new MatOfDouble(outParameters.Distortion);

#else

            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            float imageSizeScale = 1.0f;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;

            //set cameraparam
            int max_d = (int)Mathf.Max(width, height);
            double fx = max_d;
            double fy = max_d;
            double cx = width / 2.0f;
            double cy = height / 2.0f;
            camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, fx);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, cx);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, fy);
            camMatrix.put(1, 2, cy);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);
            Debug.Log("camMatrix " + camMatrix.dump());

            distCoeffs = new MatOfDouble(0, 0, 0, 0);

#endif

            Debug.Log ("distCoeffs " + distCoeffs.dump ());

            // create AR camera P * V Matrix
            Matrix4x4 P = ARUtils.CalculateProjectionMatrixFromCameraMatrixValues ((float)fx, (float)fy, (float)cx, (float)cy, width, height, 0.3f, 2000f);
            Matrix4x4 V = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
            VP = P * V;
            
            //calibration camera
            Size imageSize = new Size (width * imageSizeScale, height * imageSizeScale);
            double apertureWidth = 0;
            double apertureHeight = 0;
            double[] fovx = new double[1];
            double[] fovy = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point (0, 0);
            double[] aspectratio = new double[1];
            
            Calib3d.calibrationMatrixValues (camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);
            
            Debug.Log ("imageSize " + imageSize.ToString ());
            Debug.Log ("apertureWidth " + apertureWidth);
            Debug.Log ("apertureHeight " + apertureHeight);
            Debug.Log ("fovx " + fovx [0]);
            Debug.Log ("fovy " + fovy [0]);
            Debug.Log ("focalLength " + focalLength [0]);
            Debug.Log ("principalPoint " + principalPoint.ToString ());
            Debug.Log ("aspectratio " + aspectratio [0]);
            

            // Display objects near the camera.
            ARCamera.nearClipPlane = 0.01f;


            invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
            Debug.Log("invertYM " + invertYM.ToString());

            invertZM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            Debug.Log("invertZM " + invertZM.ToString());


            axes.SetActive(false);
            head.SetActive(false);
            rightEye.SetActive(false);
            leftEye.SetActive(false);
            mouth.SetActive(false);

            mouthParticleSystem = mouth.GetComponentsInChildren<ParticleSystem>(true);


            if (enableDownScale)
            {
                grayMat = new Mat(downscaleMat.rows(), downscaleMat.cols(), CvType.CV_8UC1);
            }
            else
            {
                grayMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC1);
            }

        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            StopThread();
            lock (ExecuteOnMainThread)
            {
                ExecuteOnMainThread.Clear();
            }
            isDetecting = false;


            if (grayMat != null)
            {
                grayMat.Dispose();
                grayMat = null;
            }

            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }

            camMatrix.Dispose();
            distCoeffs.Dispose();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(MLCameraPreviewToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update()
        {

            lock (ExecuteOnMainThread)
            {
                while (ExecuteOnMainThread.Count > 0)
                {
                    ExecuteOnMainThread.Dequeue().Invoke();
                }
            }

            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                if (!enableSkipFrame || !imageOptimizationHelper.IsCurrentFrameSkipped())
                {

                    if (!isDetecting)
                    {
                        isDetecting = true;


                        Mat rgbaMat = webCamTextureToMatHelper.GetMat();

                        Mat downScaleRgbaMat = null;
                        float DOWNSCALE_RATIO = 1.0f;
                        if (enableDownScale)
                        {
                            downScaleRgbaMat = imageOptimizationHelper.GetDownScaleMat(rgbaMat);
                            DOWNSCALE_RATIO = imageOptimizationHelper.downscaleRatio;
                        }
                        else
                        {
                            downScaleRgbaMat = rgbaMat;
                            DOWNSCALE_RATIO = 1.0f;
                        }
                        Imgproc.cvtColor(downScaleRgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);


                        //OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
                        OpenCVForUnityUtils.SetImage(faceLandmarkDetector, grayMat);

                        StartThread(
                            // action
                            () =>
                            {

                                //detect face rects
                                if (useOpenCVFaceDetector)
                                {
                                    detectResult = new List<UnityEngine.Rect>();

                                    using (Mat equalizeHistMat = new Mat())
                                    using (MatOfRect faces = new MatOfRect())
                                    {
                                        Imgproc.equalizeHist(grayMat, equalizeHistMat);

                                        cascade.detectMultiScale(equalizeHistMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new Size(equalizeHistMat.cols() * 0.15, equalizeHistMat.cols() * 0.15), new Size());

                                        List<OpenCVForUnity.CoreModule.Rect> opencvDetectResult = faces.toList();

                                        // correct the deviation of the detection result of the face rectangle of OpenCV and Dlib.
                                        foreach (var opencvRect in opencvDetectResult)
                                        {
                                            detectResult.Add(new UnityEngine.Rect((float)opencvRect.x, (float)opencvRect.y + (float)(opencvRect.height * 0.1f), (float)opencvRect.width, (float)opencvRect.height));
                                        }
                                    }
                                }
                                else
                                {
                                    //detect face rects
                                    detectResult = faceLandmarkDetector.Detect();
                                }

                            },
                            // done
                            () => {

                                List<Vector2> points = null;

                                if (detectResult.Count > 0)
                                {

                                    //detect landmark points
                                    points = faceLandmarkDetector.DetectLandmark(detectResult[0]);

                                    if (enableDownScale)
                                    {

                                        for (int p = 0; p < points.Count; p++)
                                        {
                                            points[p] = new Vector2(points[p].x * DOWNSCALE_RATIO, points[p].y * DOWNSCALE_RATIO);
                                        }

                                    }

                                    //Imgproc.rectangle(rgbaMat, new Point(0, 0), new Point(rgbaMat.width(), rgbaMat.height()), new Scalar(0, 0, 0, 0), -1);

                                    if (displayFacePoints)
                                        OpenCVForUnityUtils.DrawFaceLandmark(rgbaMat, points, new Scalar(0, 255, 0, 255), 2);


                                }

                                if (detectResult.Count > 0)
                                {
                                    MatOfPoint3f objectPoints = null;
                                    bool isRightEyeOpen = false;
                                    bool isLeftEyeOpen = false;
                                    bool isMouthOpen = false;
                                    if (points.Count == 68)
                                    {

                                        objectPoints = objectPoints68;

                                        imagePoints.fromArray(
                                            new Point((points[38].x + points[41].x) / 2, (points[38].y + points[41].y) / 2),//l eye (Interpupillary breadth)
                                            new Point((points[43].x + points[46].x) / 2, (points[43].y + points[46].y) / 2),//r eye (Interpupillary breadth)
                                            new Point(points[30].x, points[30].y),//nose (Tip)
                                            new Point(points[33].x, points[33].y),//nose (Subnasale)
                                            new Point(points[0].x, points[0].y),//l ear (Bitragion breadth)
                                            new Point(points[16].x, points[16].y)//r ear (Bitragion breadth)
                                        );

                                        if (Mathf.Abs((float)(points[43].y - points[46].y)) > Mathf.Abs((float)(points[42].x - points[45].x)) / 5.0)
                                        {
                                            isRightEyeOpen = true;
                                        }

                                        if (Mathf.Abs((float)(points[38].y - points[41].y)) > Mathf.Abs((float)(points[39].x - points[36].x)) / 5.0)
                                        {
                                            isLeftEyeOpen = true;
                                        }

                                        float noseDistance = Mathf.Abs((float)(points[27].y - points[33].y));
                                        float mouseDistance = Mathf.Abs((float)(points[62].y - points[66].y));
                                        if (mouseDistance > noseDistance / 5.0)
                                        {
                                            isMouthOpen = true;
                                        }
                                        else
                                        {
                                            isMouthOpen = false;
                                        }

                                    }
                                    else if (points.Count == 17)
                                    {

                                        objectPoints = objectPoints17;

                                        imagePoints.fromArray(
                                            new Point((points[2].x + points[3].x) / 2, (points[2].y + points[3].y) / 2),//l eye (Interpupillary breadth)
                                            new Point((points[4].x + points[5].x) / 2, (points[4].y + points[5].y) / 2),//r eye (Interpupillary breadth)
                                            new Point(points[0].x, points[0].y),//nose (Tip)
                                            new Point(points[1].x, points[1].y),//nose (Subnasale)
                                            new Point(points[6].x, points[6].y),//l ear (Bitragion breadth)
                                            new Point(points[8].x, points[8].y)//r ear (Bitragion breadth)
                                        );

                                        if (Mathf.Abs((float)(points[11].y - points[12].y)) > Mathf.Abs((float)(points[4].x - points[5].x)) / 5.0)
                                        {
                                            isRightEyeOpen = true;
                                        }

                                        if (Mathf.Abs((float)(points[9].y - points[10].y)) > Mathf.Abs((float)(points[2].x - points[3].x)) / 5.0)
                                        {
                                            isLeftEyeOpen = true;
                                        }

                                        float noseDistance = Mathf.Abs((float)(points[3].y - points[1].y));
                                        float mouseDistance = Mathf.Abs((float)(points[14].y - points[16].y));
                                        if (mouseDistance > noseDistance / 2.0)
                                        {
                                            isMouthOpen = true;
                                        }
                                        else
                                        {
                                            isMouthOpen = false;
                                        }

                                    }
                                    else if (points.Count == 6)
                                    {

                                        objectPoints = objectPoints6;

                                        imagePoints.fromArray(
                                            new Point((points[2].x + points[3].x) / 2, (points[2].y + points[3].y) / 2),//l eye (Interpupillary breadth)
                                            new Point((points[4].x + points[5].x) / 2, (points[4].y + points[5].y) / 2),//r eye (Interpupillary breadth)
                                            new Point(points[0].x, points[0].y),//nose (Tip)
                                            new Point(points[1].x, points[1].y)//nose (Subnasale)
                                        );

                                    }
                                    else if (points.Count == 5)
                                    {

                                        objectPoints = objectPoints5;

                                        imagePoints.fromArray(
                                            new Point(points[3].x, points[3].y),//l eye (Inner corner of the eye)
                                            new Point(points[1].x, points[1].y),//r eye (Inner corner of the eye)
                                            new Point(points[2].x, points[2].y),//l eye (Tail of the eye)
                                            new Point(points[0].x, points[0].y),//r eye (Tail of the eye)
                                            new Point(points[4].x, points[4].y)//nose (Subnasale)
                                        );
                                    }

                                    // estimate head pose
                                    if (rvec == null || tvec == null)
                                    {
                                        rvec = new Mat(3, 1, CvType.CV_64FC1);
                                        tvec = new Mat(3, 1, CvType.CV_64FC1);
                                        Calib3d.solvePnP(objectPoints, imagePoints, camMatrix, distCoeffs, rvec, tvec);
                                    }

                                    double tvec_x = tvec.get(0, 0)[0], tvec_y = tvec.get(1, 0)[0], tvec_z = tvec.get(2, 0)[0];

                                    bool isNotInViewport = false;
                                    Vector4 pos = VP * new Vector4((float)tvec_x, (float)tvec_y, (float)tvec_z, 1.0f);
                                    if (pos.w != 0)
                                    {
                                        float x = pos.x / pos.w, y = pos.y / pos.w, z = pos.z / pos.w;
                                        if (x < -1.0f || x > 1.0f || y < -1.0f || y > 1.0f || z < -1.0f || z > 1.0f)
                                            isNotInViewport = true;
                                    }

                                    if (double.IsNaN(tvec_z) || isNotInViewport)
                                    { // if tvec is wrong data, do not use extrinsic guesses. (the estimated object is not in the camera field of view)
                                        Calib3d.solvePnP(objectPoints, imagePoints, camMatrix, distCoeffs, rvec, tvec);
                                    }
                                    else
                                    {
                                        Calib3d.solvePnP(objectPoints, imagePoints, camMatrix, distCoeffs, rvec, tvec, true, Calib3d.SOLVEPNP_ITERATIVE);
                                    }

                                    //Debug.Log (tvec.dump());

                                    if (!isNotInViewport)
                                    {

                                        if (displayHead)
                                            head.SetActive(true);
                                        if (displayAxes)
                                            axes.SetActive(true);

                                        if (displayEffects)
                                        {
                                            rightEye.SetActive(isRightEyeOpen);
                                            leftEye.SetActive(isLeftEyeOpen);

                                            if (isMouthOpen)
                                            {
                                                mouth.SetActive(true);
                                                foreach (ParticleSystem ps in mouthParticleSystem)
                                                {
                                                    var em = ps.emission;
                                                    em.enabled = true;
#if UNITY_5_5_OR_NEWER
                                                    var main = ps.main;
                                                    main.startSizeMultiplier = 20;
#else
                                                    ps.startSize = 20;
#endif
                                                }
                                            }
                                            else
                                            {
                                                foreach (ParticleSystem ps in mouthParticleSystem)
                                                {
                                                    var em = ps.emission;
                                                    em.enabled = false;
                                                }
                                            }
                                        }

                                        // Convert to unity pose data.
                                        double[] rvecArr = new double[3];
                                        rvec.get(0, 0, rvecArr);
                                        double[] tvecArr = new double[3];
                                        tvec.get(0, 0, tvecArr);
                                        PoseData poseData = ARUtils.ConvertRvecTvecToPoseData(rvecArr, tvecArr);


                                        // Changes in pos/rot below these thresholds are ignored.
                                        if (enableLowPassFilter)
                                        {
                                            ARUtils.LowpassPoseData(ref oldPoseData, ref poseData, positionLowPass, rotationLowPass);
                                        }
                                        oldPoseData = poseData;

                                        // Create transform matrix.
                                        transformationM = Matrix4x4.TRS(poseData.pos, poseData.rot, Vector3.one);
                                    }


                                    // Right-handed coordinates system (OpenCV) to left-handed one (Unity)
                                    // https://stackoverflow.com/questions/30234945/change-handedness-of-a-row-major-4x4-transformation-matrix
                                    ARM = invertYM * transformationM * invertYM;

                                    // Apply Y-axis and Z-axis refletion matrix. (Adjust the posture of the AR object)
                                    ARM = ARM * invertYM * invertZM;


                                    // Apply the cameraToWorld matrix with the Z-axis inverted.
                                    ARM = ARCamera.cameraToWorldMatrix * invertZM * cameraOffsetM * ARM;

                                    ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
                                }

                                //Imgproc.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);

                                OpenCVForUnity.UnityUtils.Utils.fastMatToTexture2D(rgbaMat, texture);


                                isDetecting = false;
                            }
                        );
                    }
                }
            }

        }

        private void StartThread(Action action, Action done)
        {
            Task.Run(() => {
                isThreadRunning = true;

                action();

                lock (ExecuteOnMainThread)
                {
                    if (ExecuteOnMainThread.Count == 0)
                    {
                        ExecuteOnMainThread.Enqueue(() => {
                            done();
                        });
                    }
                }

                isThreadRunning = false;
            });
        }

        private void StopThread()
        {
            if (!isThreadRunning)
                return;

            while (isThreadRunning)
            {
                //Wait threading stop
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            if (webCamTextureToMatHelper != null)
                webCamTextureToMatHelper.Dispose();

            if (imageOptimizationHelper != null)
                imageOptimizationHelper.Dispose();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose();

#if UNITY_WEBGL && !UNITY_EDITOR
            if (getFilePath_Coroutine != null) {
                StopCoroutine (getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose ();
            }
#endif
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("DlibFaceLandmarkDetectorExample");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick()
        {
            webCamTextureToMatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            webCamTextureToMatHelper.Pause();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {
            webCamTextureToMatHelper.Stop();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing();
        }

        /// <summary>
        /// Raises the display face points toggle value changed event.
        /// </summary>
        public void OnDisplayFacePointsToggleValueChanged()
        {
            if (displayFacePointsToggle.isOn)
            {
                displayFacePoints = true;
            }
            else
            {
                displayFacePoints = false;
            }
        }

        /// <summary>
        /// Raises the display axes toggle value changed event.
        /// </summary>
        public void OnDisplayAxesToggleValueChanged()
        {
            if (displayAxesToggle.isOn)
            {
                displayAxes = true;
            }
            else
            {
                displayAxes = false;
                axes.SetActive(false);
            }
        }

        /// <summary>
        /// Raises the display head toggle value changed event.
        /// </summary>
        public void OnDisplayHeadToggleValueChanged()
        {
            if (displayHeadToggle.isOn)
            {
                displayHead = true;
            }
            else
            {
                displayHead = false;
                head.SetActive(false);
            }
        }

        /// <summary>
        /// Raises the display effects toggle value changed event.
        /// </summary>
        public void OnDisplayEffectsToggleValueChanged()
        {
            if (displayEffectsToggle.isOn)
            {
                displayEffects = true;
            }
            else
            {
                displayEffects = false;
                rightEye.SetActive(false);
                leftEye.SetActive(false);
                mouth.SetActive(false);
            }
        }

        /// <summary>
        /// Raises the enable low pass filter toggle value changed event.
        /// </summary>
        public void OnEnableLowPassFilterToggleValueChanged()
        {
            if (enableLowPassFilterToggle.isOn)
            {
                enableLowPassFilter = true;
            }
            else
            {
                enableLowPassFilter = false;
            }
        }


        private void AjustPointScale(MatOfPoint3f p, double scale)
        {
            Point3[] arr = p.toArray();
            for (int i = 0; i < arr.Length; i++)
            {
                //arr[i] = new Point3(arr[i].x * scale, arr[i].y * scale, arr[i].z * scale);
                arr[i].x *= scale;
                arr[i].y *= scale;
                arr[i].z *= scale;
            }
            p.fromArray(arr);
        }

    }
}
