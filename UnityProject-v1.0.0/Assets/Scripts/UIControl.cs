using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class UIControl : MonoBehaviour
{
    public Button PoseBoton;
    public Button ParamBoton;

    public Button EnviarBoton;

    public GameObject ImageTarget;
    public GameObject ModelTarget;


    public GameObject Pose;
    public GameObject Param;

    private TMPro.TextMeshProUGUI PoseTexto;
    private TMPro.TextMeshProUGUI ParamTexto;

    void Start()
    {
        EnviarBoton.interactable = false;

        PoseTexto = PoseBoton.gameObject.transform.GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        ParamTexto = ParamBoton.gameObject.transform.GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        PoseTexto.text = "Hide pose estimation";
        ParamTexto.text = "Show parametrization of trayectories";

        Param.SetActive(false);

        PoseBoton.onClick.AddListener(PoseBotonClic);
        ParamBoton.onClick.AddListener(ParamBotonClic);
    }

    void Update()
    {
        if ((ImageTarget.GetComponent<ImageTargetBehaviour>().TargetStatus.Status == Status.TRACKED ||
            ImageTarget.GetComponent<ImageTargetBehaviour>().TargetStatus.Status == Status.EXTENDED_TRACKED)
            && ModelTarget.GetComponent<ModelTargetBehaviour>().TargetStatus.Status == Status.TRACKED)
        {
            if (EnviarBoton.interactable == false)
            {
                EnviarBoton.interactable = true;
            }
        }
        else
        {
            if (EnviarBoton.interactable == true)
            {
                EnviarBoton.interactable = false;
            }
        }
    }

    void PoseBotonClic()
    {
        ChangeUIPose(Pose, PoseTexto);
    }

    void ParamBotonClic()
    {
        ChangeUIParam(Param, ParamTexto);
    }

    void ChangeUIPose(GameObject Group, TMPro.TextMeshProUGUI Texto)
    {
        if (Texto.text[0] == 'H')
        {
            Group.GetComponent<CanvasGroup>().alpha = 0;
            Group.GetComponent<CanvasGroup>().interactable = false;
            Texto.text = "Show pose estimation";
        }
        else
        {
            Group.GetComponent<CanvasGroup>().alpha = 1;
            Group.GetComponent<CanvasGroup>().interactable = true;
            Texto.text = "Hide pose estimation";
        }
    }

    void ChangeUIParam(GameObject Group, TMPro.TextMeshProUGUI Texto)
    {
        if (Texto.text[0] == 'H')
        {
            Group.SetActive(false);
            Texto.text = "Show parametrization of trayectories";
        }
        else
        {
            Group.SetActive(true);
            Texto.text = "Hide parametrization of trayectories";
        }
    }

}