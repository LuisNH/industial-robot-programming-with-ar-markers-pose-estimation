using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Trayectory : MonoBehaviour
{
    public Button VisualizarBoton;
    public Button OcultarBoton;
    public Button EnviarBoton;

    public GameObject Cordon;

    public GameObject Linea;

    public GameObject LongitudField;
    public GameObject PasoField;
    public GameObject AnchoField;
    public GameObject OffsetXField;
    public GameObject OffsetYField;
    public GameObject OffsetZField;
    public GameObject RotacionField;

    private LineRenderer lineRenderer;

    private TMP_InputField LongitudTexto;
    private TMP_InputField PasoTexto;
    private TMP_InputField AnchoTexto;
    private TMP_InputField OffsetXTexto;
    private TMP_InputField OffsetYTexto;
    private TMP_InputField OffsetZTexto;
    private TMP_InputField RotacionTexto;

    private float Longitud;
    private float Paso;
    private float Ancho;
    private float OffsetX;
    private float OffsetY;
    private float OffsetZ;
    private float Rotacion;

    private Vector3[] Puntos;
    private int NumeroDePuntos;

    void Start()
    {
        lineRenderer = Linea.GetComponent<LineRenderer>();

        Linea.SetActive(false);

        OcultarBoton.interactable = false;

        LongitudTexto = LongitudField.GetComponentInChildren<TMP_InputField>();
        PasoTexto = PasoField.GetComponentInChildren<TMP_InputField>();
        AnchoTexto = AnchoField.GetComponentInChildren<TMP_InputField>();
        OffsetXTexto = OffsetXField.GetComponentInChildren<TMP_InputField>();
        OffsetYTexto = OffsetYField.GetComponentInChildren<TMP_InputField>();
        OffsetZTexto = OffsetZField.GetComponentInChildren<TMP_InputField>();
        RotacionTexto = RotacionField.GetComponentInChildren<TMP_InputField>();

        VisualizarBoton.onClick.AddListener(VisualizarBotonClic);
        OcultarBoton.onClick.AddListener(OcultarBotonClic);
        EnviarBoton.onClick.AddListener(EnviarBotonClic);
    }

    void VisualizarBotonClic()
    {
        Parametros();

        Generar();

        Linea.SetActive(true);

        OcultarBoton.interactable = true;
    }

    void OcultarBotonClic()
    {
        Linea.SetActive(false);

        OcultarBoton.interactable = false;
    }

    void EnviarBotonClic()
    {
        Vector3 Punto;
        Vector3 PositionC;
        Vector3 RotationC;

        Parametros();
        Generar();

        var TransformCordon = Cordon.transform;
        var TransformCentro = this.transform;

        PositionC = TransformCentro.InverseTransformPoint(TransformCordon.position) * 1000;
        RotationC = (Quaternion.Inverse(TransformCordon.rotation) * TransformCentro.rotation).eulerAngles;

        var PoseAndPoints = $"Seam:\nPosition (mm) = {PositionC.x:F0}, {PositionC.z:F0}, {PositionC.y:F0}\n" + 
            $"Rotation (degrees) = {RotationC.x:F0}, {RotationC.z:F0}, {RotationC.y:F0}" +
            $"\nNumber of points: {NumeroDePuntos}";
        for (int i = 0; i < NumeroDePuntos; i++)
        {
            Punto = Puntos[i];
            Punto = new Vector3(Punto.x, Punto.z, Punto.y);
            PoseAndPoints += $"\nPoint: {Punto * 1000:F4}";
        }
        Debug.Log(PoseAndPoints);
        Linea.SetActive(true);

    }

    void Generar()
    {
        Vector3 Punto;

        if ((Longitud / Paso) > Mathf.Floor(Longitud / Paso))
        {
            NumeroDePuntos = (int)(2 * (Mathf.Floor(Longitud / Paso) + 1) + 1);
        }
        else
        {
            NumeroDePuntos = (int)(2 * (Mathf.Floor(Longitud / Paso) + 1));
        }

        Puntos = new Vector3[NumeroDePuntos];

        float x = Ancho / 2;
        float y = 0.0f;
        for (int i = 0; i < NumeroDePuntos; i++)
        {
            if (i == NumeroDePuntos - 1)
            {
                Punto = new Vector3(x, 0, Longitud);
            }
            else
            {
                Punto = new Vector3(x, 0, y);
            }
            Puntos[i] = Punto;
            if (i % 2 == 0)
            {
                x *= -1;
            }
            if ((i + 1) % 2 == 0)
            {
                y += Paso;
            }

        }


        lineRenderer.positionCount = NumeroDePuntos;

        lineRenderer.SetPositions(Puntos);

        Cordon.transform.localPosition = new Vector3(OffsetX, OffsetZ, OffsetY);
        Cordon.transform.localEulerAngles = new Vector3(0, Rotacion, 0);
    }

    void Parametros()
    {
        Longitud = float.Parse(LongitudTexto.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        Paso = float.Parse(PasoTexto.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        Ancho = float.Parse(AnchoTexto.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        OffsetX = float.Parse(OffsetXTexto.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        OffsetY = float.Parse(OffsetYTexto.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        OffsetZ = float.Parse(OffsetZTexto.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        Rotacion = float.Parse(RotacionTexto.text, CultureInfo.InvariantCulture.NumberFormat);
    }
}