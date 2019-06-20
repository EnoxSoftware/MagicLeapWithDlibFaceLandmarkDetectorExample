using DlibFaceLandmarkDetector;
using MagicLeapWithOpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnityExample;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLeapWithOpenCVForUnityExample
{
    /// <summary>
    /// Dlib MLCameraPreview Example
    /// </summary>
    [RequireComponent(typeof(MLCameraPreviewToMatHelper), typeof(ImageOptimizationHelper))]
    public class DlibMLCameraPreviewExample : MonoBehaviour
    {

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        MLCameraPreviewToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;

        /// <summary>
        /// The image optimization helper.
        /// </summary>
        ImageOptimizationHelper imageOptimizationHelper;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        /// <summary>
        /// The haarcascade_frontalface_alt_xml_filepath.
        /// </summary>
        string haarcascade_frontalface_alt_xml_filepath;

        /// <summary>
        /// The dlib shape predictor file name.
        /// </summary>
        string dlibShapePredictorFileName = "sp_human_face_68_for_mobile.dat";

        /// <summary>
        /// The dlib shape predictor file path.
        /// </summary>
        string dlibShapePredictorFilePath;


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
        /// rgbaMat
        /// </summary>
        Mat rgbaMat;

        /// <summary>
        /// detectResult
        /// </summary>
        List<UnityEngine.Rect> detectResult;

        /// <summary>
        /// pointsList
        /// </summary>
        List<List<Vector2>> pointsList;

        /// <summary>
        /// tokenSource
        /// </summary>
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        // Use this for initialization
        void Start()
        {
            fpsMonitor = GetComponent<FpsMonitor>();

            imageOptimizationHelper = gameObject.GetComponent<ImageOptimizationHelper>();
            webCamTextureToMatHelper = gameObject.GetComponent<MLCameraPreviewToMatHelper>();



            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.UnityUtils.Utils.getFilePath("haarcascade_frontalface_alt.xml");
            dlibShapePredictorFilePath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath(dlibShapePredictorFileName);
            Run();

        }

        private void Run()
        {
            if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
            {
                Debug.LogError("shape predictor file does not exist. Please copy from “DlibFaceLandmarkDetector/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
            }

            cascade = new CascadeClassifier(haarcascade_frontalface_alt_xml_filepath);
            if (cascade.empty())
            {
                Debug.LogError("cascade file is not loaded. Please copy from “OpenCVForUnity/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
            }

            faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);

            webCamTextureToMatHelper.Initialize();

            detectResult = new List<UnityEngine.Rect>();
            pointsList = new List<List<Vector2>>();
        }

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();
            Mat downscaleMat = imageOptimizationHelper.GetDownScaleMat(webCamTextureMat);

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            //            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
            //            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("deviceName", webCamTextureToMatHelper.GetDeviceName().ToString());
                fpsMonitor.Add("width", webCamTextureToMatHelper.GetWidth().ToString());
                fpsMonitor.Add("height", webCamTextureToMatHelper.GetHeight().ToString());
                fpsMonitor.Add("downscaleRaito", imageOptimizationHelper.downscaleRatio.ToString());
                fpsMonitor.Add("frameSkippingRatio", imageOptimizationHelper.frameSkippingRatio.ToString());
                fpsMonitor.Add("downscale_width", downscaleMat.width().ToString());
                fpsMonitor.Add("downscale_height", downscaleMat.height().ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());
            }


            //            float width = webCamTextureMat.width ();
            //            float height = webCamTextureMat.height ();
            //                                    
            //            float widthScale = (float)Screen.width / width;
            //            float heightScale = (float)Screen.height / height;
            //            if (widthScale < heightScale) {
            //                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            //            } else {
            //                Camera.main.orthographicSize = height / 2;
            //            }

            rgbaMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC4);
            grayMat = new Mat(downscaleMat.rows(), downscaleMat.cols(), CvType.CV_8UC1);


            Process();
        }

        /// <summary>
        /// Raises the webcam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            // Cancel Task
            tokenSource.Cancel();


            if (rgbaMat != null)
                rgbaMat.Dispose();

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
        }

        /// <summary>
        /// Raises the webcam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(MLCameraPreviewToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update()
        {

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
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("OpenCVForUnityExample");
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
        /// Process
        /// </summary>
        /// <returns></returns>
        private async void Process()
        {

            float DOWNSCALE_RATIO = 1.0f;

            while (true)
            {

                // Check TaskCancel
                if (tokenSource.Token.IsCancellationRequested)
                {
                    break;
                }


                rgbaMat = webCamTextureToMatHelper.GetMat();
                // Debug.Log ("rgbaMat.ToString() " + rgbaMat.ToString ());


                Mat downScaleRgbaMat = null;
                DOWNSCALE_RATIO = 1.0f;
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


                await Task.Run(() =>
                {

                    // set the downscale mat
                    OpenCVForUnityUtils.SetImage(faceLandmarkDetector, grayMat);

                    // detect faces on the downscale image
                    if (!enableSkipFrame || !imageOptimizationHelper.IsCurrentFrameSkipped())
                    {

                        pointsList.Clear();


                        //detect face rects
                        if (useOpenCVFaceDetector)
                        {

                            using (Mat equalizeHistMat = new Mat())
                            using (MatOfRect faces = new MatOfRect())
                            {
                                Imgproc.equalizeHist(grayMat, equalizeHistMat);

                                cascade.detectMultiScale(equalizeHistMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new Size(equalizeHistMat.cols() * 0.15, equalizeHistMat.cols() * 0.15), new Size());

                                List<OpenCVForUnity.CoreModule.Rect> opencvDetectResult = faces.toList();

                                // correct the deviation of the detection result of the face rectangle of OpenCV and Dlib.
                                detectResult.Clear();
                                foreach (var opencvRect in opencvDetectResult)
                                {
                                    detectResult.Add(new UnityEngine.Rect((float)opencvRect.x, (float)opencvRect.y + (float)(opencvRect.height * 0.1f), (float)opencvRect.width, (float)opencvRect.height));
                                }
                            }

                        }
                        else
                        {
                            // Dlib's face detection processing time increases in proportion to the image size.
                            detectResult = faceLandmarkDetector.Detect();
                        }


                        // detect face landmarks on the original image
                        foreach (var rect in detectResult)
                        {

                            //detect landmark points
                            pointsList.Add(faceLandmarkDetector.DetectLandmark(rect));

                        }

                        if (enableDownScale)
                        {
                            for (int i = 0; i < detectResult.Count; ++i)
                            {
                                var rect = detectResult[i];
                                detectResult[i] = new UnityEngine.Rect(
                                    rect.x * DOWNSCALE_RATIO,
                                    rect.y * DOWNSCALE_RATIO,
                                    rect.width * DOWNSCALE_RATIO,
                                    rect.height * DOWNSCALE_RATIO);
                            }

                            for (int i = 0; i < pointsList.Count; ++i)
                            {
                                var points = pointsList[i];
                                for (int p = 0; p < points.Count; p++)
                                {
                                    points[p] = new Vector2(points[p].x * DOWNSCALE_RATIO, points[p].y * DOWNSCALE_RATIO);
                                }
                            }
                        }
                    }


                    //Imgproc.rectangle(rgbaMat, new Point(0, 0), new Point(rgbaMat.width(), rgbaMat.height()), new Scalar(0, 0, 0, 0), -1);

                    for (int i = 0; i < pointsList.Count; i++)
                    {
                        //draw landmark points
                        OpenCVForUnityUtils.DrawFaceLandmark(rgbaMat, pointsList[i], new Scalar(0, 255, 0, 255), 2);

                    }

                    for (int i = 0; i < detectResult.Count; i++)
                    {

                        //draw face rect
                        OpenCVForUnityUtils.DrawFaceRect(rgbaMat, detectResult[i], new Scalar(255, 0, 0, 255), 2);
                    }
                });



                Utils.fastMatToTexture2D(rgbaMat, texture);


                Thread.Sleep(10);


            }

        }

    }
}
