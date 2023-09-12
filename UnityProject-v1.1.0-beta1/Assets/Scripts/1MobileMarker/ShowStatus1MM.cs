using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class ShowStatus1MM : MonoBehaviour
{
    private int Frames = 0;

    public GameObject CenterRobotBase;
    public GameObject FixedMarker;
    public GameObject MobileMarker;
    public GameObject CenterMobileMarker;

    public GameObject InfoTextField;
    public GameObject FixedMarkerTextField;
    public GameObject MobileMarkerTextField;

    private bool FixedMarkerStatus;
    private bool MobileMarkerStatus;

    private TMPro.TextMeshProUGUI InfoText;
    private TMPro.TextMeshProUGUI FixedMarkerText;
    private TMPro.TextMeshProUGUI MobileMarkerText;

    void Start()
    {
        float FMX = PlayerPrefs.GetFloat("FMX");
        float FMY = PlayerPrefs.GetFloat("FMY");
        float FMZ = PlayerPrefs.GetFloat("FMZ");
        float FMA = PlayerPrefs.GetFloat("FMA");
        float FMB = PlayerPrefs.GetFloat("FMB");
        float FMC = PlayerPrefs.GetFloat("FMC");
        float FMD = PlayerPrefs.GetFloat("FMD");

        float MMX = PlayerPrefs.GetFloat("MMX");
        float MMY = PlayerPrefs.GetFloat("MMY");
        float MMZ = PlayerPrefs.GetFloat("MMZ");
        float MMA = PlayerPrefs.GetFloat("MMA");
        float MMB = PlayerPrefs.GetFloat("MMB");
        float MMC = PlayerPrefs.GetFloat("MMC");
        float MMD = PlayerPrefs.GetFloat("MMD");

        Quaternion CenterRobotBaseQuaternion = new Quaternion(FMB, FMC, FMD, FMA);

        CenterRobotBase.transform.localPosition = new Vector3(FMX, FMZ, FMY);
        CenterRobotBase.transform.localRotation = CenterRobotBaseQuaternion.normalized;

        Quaternion CenterMobileMarkerQuaternion = new Quaternion(MMB, MMC, MMD, MMA);

        CenterMobileMarker.transform.localPosition = new Vector3(MMX, MMZ, MMY);
        CenterMobileMarker.transform.localRotation = CenterMobileMarkerQuaternion.normalized;

        InfoText = InfoTextField.GetComponent<TMPro.TextMeshProUGUI>();

        FixedMarkerText = FixedMarkerTextField.GetComponent<TMPro.TextMeshProUGUI>();
        FixedMarkerStatus = false;

        MobileMarkerText = MobileMarkerTextField.GetComponent<TMPro.TextMeshProUGUI>();
        MobileMarkerStatus = false;
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
            FixedMarkerTracking();
            MobileMarkerTracking();

        }
    }

    void PoseEstimation()
    {
        var TransformFixedMarker = FixedMarker.transform;
        var TransformMobileMarker = CenterMobileMarker.transform;
        var TransformCenterRobotBase = CenterRobotBase.transform;
        var TransformCamera = transform;

        Vector3 PositionC;
        Vector3 RotationC;
        Vector3 PositionMM;
        Vector3 RotationMM;
        Vector3 PositionFM;
        Vector3 RotationFM;

        PositionC = TransformCenterRobotBase.InverseTransformPoint(TransformCamera.position) * 1000;
        RotationC = (Quaternion.Inverse(TransformCamera.rotation) * TransformCenterRobotBase.rotation).eulerAngles;

        PositionMM = TransformCenterRobotBase.InverseTransformPoint(TransformMobileMarker.position) * 1000;
        RotationMM = (Quaternion.Inverse(TransformMobileMarker.rotation) * TransformCenterRobotBase.rotation).eulerAngles;

        PositionFM = TransformCenterRobotBase.InverseTransformPoint(TransformFixedMarker.position) * 1000;
        RotationFM = (Quaternion.Inverse(TransformFixedMarker.rotation) * TransformCenterRobotBase.rotation).eulerAngles;

        string PoseEstimate = string.Format("Camera:\nPosition (mm) = {0:0}, {1:0}, {2:0}\nRotation (degrees) = " +
            "{3:0}, {4:0}, {5:0}\nMobile Marker:\nPosition (mm) = {6:0}, {7:0}, {8:0}\nRotation (degrees) = " +
            "{9:0}, {10:0}, {11:0}\nFixed Marker:\nPosition (mm) = {12:0}, {13:0}, {14:0}\nRotation (degrees) = " +
            "{15:0}, {16:0}, {17:0}",
            PositionC.x, PositionC.z, PositionC.y, RotationC.x, RotationC.z, RotationC.y,
            PositionMM.x, PositionMM.z, PositionMM.y, RotationMM.x, RotationMM.z, RotationMM.y,
            PositionFM.x, PositionFM.z, PositionFM.y, RotationFM.x, RotationFM.z, RotationFM.y);
        InfoText.text = PoseEstimate;
    }

    void FixedMarkerTracking()
    {
        if ((FixedMarker.GetComponent<ImageTargetBehaviour>().TargetStatus.Status == Status.TRACKED) ||
            (FixedMarker.GetComponent<ImageTargetBehaviour>().TargetStatus.Status == Status.EXTENDED_TRACKED))
        {
            if (FixedMarkerStatus == false)
            {
                FixedMarkerText.text = string.Format("Tracking Fixed Marker");
                FixedMarkerStatus = true;
            }
        }
        else if (FixedMarkerStatus == true)
        {
            FixedMarkerText.text = string.Format("");
            FixedMarkerStatus = false;
        }
    }

    void MobileMarkerTracking()
    {
        if (MobileMarker.GetComponent<ModelTargetBehaviour>().TargetStatus.Status == Status.TRACKED ||
            MobileMarker.GetComponent<ModelTargetBehaviour>().TargetStatus.Status == Status.EXTENDED_TRACKED)
        {
            if (MobileMarkerStatus == false)
            {
                MobileMarkerText.text = string.Format("Tracking Mobile Marker");
                MobileMarkerStatus = true;
            }
        }
        else if (MobileMarkerStatus == true)
        {
            MobileMarkerText.text = string.Format("");
            MobileMarkerStatus = false;
        }
    }
}