using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class ShowStatus : MonoBehaviour
{
    private int Frames = 0;

    public GameObject Centro;
    public GameObject ImageTarget;
    public GameObject ModelTarget;
    public GameObject CentroModel;

    public GameObject InfoTextoField;
    public GameObject ImageTextoField;
    public GameObject ModelTextoField;

    private bool ImageStatus;
    private bool ModelStatus;

    private TMPro.TextMeshProUGUI InfoTexto;
    private TMPro.TextMeshProUGUI ImageTexto;
    private TMPro.TextMeshProUGUI ModelTexto;


    void Start()
    {
        InfoTexto = InfoTextoField.GetComponent<TMPro.TextMeshProUGUI>();

        ImageTexto = ImageTextoField.GetComponent<TMPro.TextMeshProUGUI>();
        ImageStatus = false;

        ModelTexto = ModelTextoField.GetComponent<TMPro.TextMeshProUGUI>();
        ModelStatus = false;
    }

    void Update()
    {
        Frames++;
        if (Frames == 30)
        {
            Frames = 0;

            PoseEstimation();
        }
        if (Frames % 15 == 0)
        {
            ImageTracking();
            ModelTracking();

        }
    }

    void PoseEstimation()
    {
        var TransformImage = ImageTarget.transform;
        var TransformModel = CentroModel.transform;
        var TransformCentro = Centro.transform;
        var TransformCamera = transform;

        Vector3 PositionC;
        Vector3 RotationC;
        Vector3 PositionIT;
        Vector3 RotationIT;
        Vector3 PositionMT;
        Vector3 RotationMT;

        PositionC = TransformCentro.InverseTransformPoint(TransformCamera.position) * 1000;
        RotationC = (Quaternion.Inverse(TransformCamera.rotation) * TransformCentro.rotation).eulerAngles;

        PositionMT = TransformCentro.InverseTransformPoint(TransformModel.position) * 1000;
        RotationMT = (Quaternion.Inverse(TransformModel.rotation) * TransformCentro.rotation).eulerAngles;

        PositionIT = TransformCentro.InverseTransformPoint(TransformImage.position) * 1000;
        RotationIT = (Quaternion.Inverse(TransformImage.rotation) * TransformCentro.rotation).eulerAngles;

        var PoseEstimate = string.Format("Camera:\nPosition (mm) = {0:0}, {1:0}, {2:0}\nRotation (degrees) = " +
            "{3:0}, {4:0}, {5:0}\nMobile Marker:\nPosition (mm) = {6:0}, {7:0}, {8:0}\nRotation (degrees) = " +
            "{9:0}, {10:0}, {11:0}\nFixed Marker:\nPosition (mm) = {12:0}, {13:0}, {14:0}\nRotation (degrees) = " +
            "{15:0}, {16:0}, {17:0}",
            PositionC.x, PositionC.z, PositionC.y, RotationC.x, RotationC.z, RotationC.y,
            PositionMT.x, PositionMT.z, PositionMT.y, RotationMT.x, RotationMT.z, RotationMT.y,
            PositionIT.x, PositionIT.z, PositionIT.y, RotationIT.x, RotationIT.z, RotationIT.y);
        InfoTexto.text = PoseEstimate;
    }

    void ImageTracking()
    {
        if ((ImageTarget.GetComponent<ImageTargetBehaviour>().TargetStatus.Status == Status.TRACKED) ||
            (ImageTarget.GetComponent<ImageTargetBehaviour>().TargetStatus.Status == Status.EXTENDED_TRACKED))
        {
            if (ImageStatus == false)
            {
                ImageTexto.text = string.Format("Tracking Fixed Marker");
                ImageStatus = true;
            }
        }
        else if (ImageStatus == true)
        {
            ImageTexto.text = string.Format("");
            ImageStatus = false;
        }
    }

    void ModelTracking()
    {
        if (ModelTarget.GetComponent<ModelTargetBehaviour>().TargetStatus.Status == Status.TRACKED)
        {
            if (ModelStatus == false)
            {
                ModelTexto.text = string.Format("Tracking Mobile Marker");
                ModelStatus = true;
            }
        }
        else if (ModelStatus == true)
        {
            ModelTexto.text = string.Format("");
            ModelStatus = false;
        }
    }
}