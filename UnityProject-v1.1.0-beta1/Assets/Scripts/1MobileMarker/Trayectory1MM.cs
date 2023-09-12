using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vuforia;

public class Trayectory1MM : MonoBehaviour
{
    InputJSON1MM data;

    [SerializeField] string IP; // local host
    [SerializeField] int rxPort; // port to receive data from Python on
    [SerializeField] int txPort; // port to send data to Python on

    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread

    public Button GenerateButton;
    public Button SendButton;

    public Toggle TrajectoryToggle;

    public GameObject FixedMarker;
    public GameObject MobileMarker;

    public GameObject Seam;
    public GameObject CenterRobotBase;

    public GameObject Line;

    private LineRenderer lineRenderer;

    public TMP_InputField LengthField;
    public TMP_InputField StepField;
    public TMP_InputField WidthField;
    public TMP_InputField OffsetXField;
    public TMP_InputField OffsetYField;
    public TMP_InputField OffsetZField;
    public TMP_InputField RotationField;

    private float Length;
    private float Step;
    private float Width;
    private float OffsetX;
    private float OffsetY;
    private float OffsetZ;
    private float Rotation;

    private Vector3[] Points;
    private int NumberOfPoints;

    private bool fieldsStatus = false;

    private bool trajectoryDataRecieved = false;
    private bool trajectoryFunctionStatus = false;

    void Start()
    {
        GenerateButton.interactable = false;
        SendButton.interactable = false;

        lineRenderer = Line.GetComponent<LineRenderer>();

        Line.SetActive(false);

        TrajectoryToggle.interactable = false;
        TrajectoryToggle.isOn = true;

        GenerateButton.onClick.AddListener(GenerateButtonClic);
        SendButton.onClick.AddListener(SendButtonClic);

        TrajectoryToggle.onValueChanged.AddListener(delegate { TrajectoryToggleClic(TrajectoryToggle); } );
    }

    void Update()
    {
        string LengthText = LengthField.text;
        string StepText = StepField.text;
        string WidthText = WidthField.text;
        string OffsetXText = OffsetXField.text;
        string OffsetYText = OffsetYField.text;
        string OffsetZText = OffsetZField.text;
        string RotationText = RotationField.text;

        if (!string.IsNullOrWhiteSpace(LengthText)
            && !string.IsNullOrWhiteSpace(StepText)
            && !string.IsNullOrWhiteSpace(WidthText)
            && !string.IsNullOrWhiteSpace(OffsetXText)
            && !string.IsNullOrWhiteSpace(OffsetYText)
            && !string.IsNullOrWhiteSpace(OffsetZText)
            && !string.IsNullOrWhiteSpace(RotationText))
        {
            fieldsStatus = true;
        }
        else
        {
            fieldsStatus = false;
        }

        if ((FixedMarker.GetComponent<ImageTargetBehaviour>().TargetStatus.Status == Status.TRACKED ||
            FixedMarker.GetComponent<ImageTargetBehaviour>().TargetStatus.Status == Status.EXTENDED_TRACKED)
            && (MobileMarker.GetComponent<ModelTargetBehaviour>().TargetStatus.Status == Status.TRACKED ||
            MobileMarker.GetComponent<ModelTargetBehaviour>().TargetStatus.Status == Status.EXTENDED_TRACKED)
            && fieldsStatus)
        {
            if (GenerateButton.interactable == false)
            {
                GenerateButton.interactable = true;
            }
        }
        else
        {
            if (GenerateButton.interactable == true)
            {
                GenerateButton.interactable = false;
            }
        }
    }

    IEnumerator SendDataCoroutine(string body)
    {
        SendData(body);

        yield return new WaitForSeconds(4f);

        if (trajectoryDataRecieved)
        {
            if (trajectoryFunctionStatus)
            {
                SendButton.GetComponentInChildren<TMP_Text>().text = "Created";
            }
            else
            {
                SendButton.GetComponentInChildren<TMP_Text>().text = "Failed";
            }
        }
        else
        {
            SendButton.GetComponentInChildren<TMP_Text>().text = "Not sent";
        }

        trajectoryDataRecieved = false;
        trajectoryFunctionStatus = false;

        yield return new WaitForSeconds(2f);
        SendButton.GetComponentInChildren<TMP_Text>().text = "Send to PC";
        SendButton.interactable = true;
    }

    public void SendData(string message) // Use to send data to Python
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    void Awake()
    {
        int IP1 = PlayerPrefs.GetInt("IP1");
        int IP2 = PlayerPrefs.GetInt("IP2");
        int IP3 = PlayerPrefs.GetInt("IP3");
        int IP4 = PlayerPrefs.GetInt("IP4");

        rxPort = PlayerPrefs.GetInt("rxPort");
        txPort = PlayerPrefs.GetInt("txPort");

        IP = $"{IP1}.{IP2}.{IP3}.{IP4}";

        // Create remote endpoint (to Matlab) 
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);

        // Create local client
        client = new UdpClient(rxPort);

        // local endpoint define (where messages are received)
        // Create a new thread for reception of incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Initialize (seen in comments window)
        print("UDP Comms Initialised");
    }

    // Receive data, update packets received
    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                print(">> " + text);
                ProcessInput(text);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    void GenerateButtonClic()
    {

        if (!SendButton.interactable)
        {
            SendButton.interactable = true;
        }

        Parameters();

        Generate();

        Line.SetActive(true);

        TrajectoryToggle.interactable = true;
    }

    void TrajectoryToggleClic(Toggle change)
    {
        if (TrajectoryToggle.isOn)
        {
            Line.SetActive(true);
        }
        else
        {
            Line.SetActive(false);
        }
        
    }

    void SendButtonClic()
    {
        Vector3 Point;
        Vector3 PositionC;
        Vector3 RotationC;

        Parameters();
        Generate();

        var TransformSeam = Seam.transform;
        var TransformCenter = CenterRobotBase.transform;

        PositionC = TransformCenter.InverseTransformPoint(TransformSeam.position) * 1000;
        RotationC = (Quaternion.Inverse(TransformSeam.rotation) * TransformCenter.rotation).eulerAngles;

        string body = $"{{\"function\": \"trajectory\",\"pose\": [{PositionC.x}, {PositionC.z}, {PositionC.y}, " + 
            $"{RotationC.x}, {RotationC.z}, {RotationC.y}], " +
            $"\"points\": [";
        for (int i = 0; i < NumberOfPoints - 1; i++)
        {
            Point = Points[i];
            Point = new Vector3(Point.x, Point.z, Point.y);
            body += $"{Point * 1000}, ";
        }
        Point = Points[NumberOfPoints - 1];
        Point = new Vector3(Point.x, Point.z, Point.y);
        body += $"{Point * 1000}]}}";

        Line.SetActive(true);

        SendButton.interactable = false;
        SendButton.GetComponentInChildren<TMP_Text>().text = "Sending data";
        StartCoroutine(SendDataCoroutine(body));
    }

    void Generate()
    {
        Vector3 Point;

        if ((Length / Step) > Mathf.Floor(Length / Step))
        {
            NumberOfPoints = (int)(2 * (Mathf.Floor(Length / Step) + 1) + 1);
        }
        else
        {
            NumberOfPoints = (int)(2 * (Mathf.Floor(Length / Step) + 1));
        }

        Points = new Vector3[NumberOfPoints];

        float x = Width / 2;
        float y = 0.0f;
        for (int i = 0; i < NumberOfPoints; i++)
        {
            if (i == NumberOfPoints - 1)
            {
                Point = new Vector3(x, 0, Length);
            }
            else
            {
                Point = new Vector3(x, 0, y);
            }
            Points[i] = Point;
            if (i % 2 == 0)
            {
                x *= -1;
            }
            if ((i + 1) % 2 == 0)
            {
                y += Step;
            }

        }

        lineRenderer.positionCount = NumberOfPoints;

        lineRenderer.SetPositions(Points);

        Seam.transform.localPosition = new Vector3(OffsetX, OffsetZ, OffsetY);
        Seam.transform.localEulerAngles = new Vector3(0, Rotation, 0);
    }

    void Parameters()
    {
        Length = float.Parse(LengthField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        Step = float.Parse(StepField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        Width = float.Parse(WidthField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        OffsetX = float.Parse(OffsetXField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        OffsetY = float.Parse(OffsetYField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        OffsetZ = float.Parse(OffsetZField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        Rotation = float.Parse(RotationField.text, CultureInfo.InvariantCulture.NumberFormat);
    }

    private void ProcessInput(string jsonString)
    {
        data = JsonUtility.FromJson<InputJSON1MM>(jsonString);

        if (data.function == "trajectory")
        {
            trajectoryDataRecieved = true;
            if (data.status)
            {
                trajectoryFunctionStatus = true;
            }
        }
    }

    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }
}

[System.Serializable]
public class InputJSON1MM
{
    public string function;
    public bool status;
    public Valid1MM valid;
    public List<float> posture;

    public InputJSON1MM(string _function, bool _status, Valid1MM _valid, List<float> _posture)
    {
        function = _function;
        status = _status;
        valid = _valid;
        posture = _posture;
    }
}

[System.Serializable]
public class Valid1MM
{
    // Checks that init or final poses are close
    // enough and with approximately the same orientation
    public bool init_dist;
    public bool init_rot;
    public bool final_dist;
    public bool final_rot;
    // Distance between the two markers
    public bool distance;
    // Checks if parallel
    public bool parallel;
    // Checks for coplanarity
    public bool coplanar;

    public Valid1MM(bool _init_dist, bool _init_rot, bool _final_dist, bool _final_rot, bool _distance, bool _parallel, bool _coplanar)
    {
        init_dist = _init_dist;
        init_rot = _init_rot;
        final_dist = _final_dist;
        final_rot = _final_rot;
        distance = _distance;
        parallel = _parallel;
        coplanar = _coplanar;
    }
}
