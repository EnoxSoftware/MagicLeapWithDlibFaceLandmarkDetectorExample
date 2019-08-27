using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using CVVTuber;

namespace MagicLeapWithDlibFaceLandmarkDetectorExample
{
    public class MagicLeapCVVTuberExample : MonoBehaviour
    {
        /// <summary>
        /// The webcam texture mat source getter.
        /// </summary>
        public MLCameraPreviewMatSourceGetter webCamTextureMatSourceGetter;

        /// <summary>
        /// The dlib face landmark getter.
        /// </summary>
        public DlibFaceLandmarkGetter dlibFaceLandmarkGetter;

        // Use this for initialization
        void Start ()
        {
            dlibFaceLandmarkGetter.dlibShapePredictorFileName = "sp_human_face_68.dat";
            dlibFaceLandmarkGetter.dlibShapePredictorMobileFileName = "sp_human_face_68_for_mobile.dat";
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {
            SceneManager.LoadScene ("CVVTuberExample");
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick ()
        {
            webCamTextureMatSourceGetter.ChangeCamera ();
        }
    }
}