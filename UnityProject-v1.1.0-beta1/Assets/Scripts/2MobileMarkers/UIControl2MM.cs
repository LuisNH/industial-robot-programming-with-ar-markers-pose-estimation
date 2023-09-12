using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControl2MM : MonoBehaviour
{
    public Toggle PoseToggle;
    public Toggle ParamToggle;

    public GameObject Pose;
    public GameObject Param;

    void Start()
    {
        ParamToggle.isOn = false;
        Param.GetComponent<CanvasGroup>().alpha = 0;
        Param.GetComponent<CanvasGroup>().interactable = false;

        PoseToggle.onValueChanged.AddListener(delegate { PoseToggleClic(PoseToggle); } );
        ParamToggle.onValueChanged.AddListener(delegate { ParamToggleClic(ParamToggle); } );
    }

    void PoseToggleClic(Toggle change)
    {
        ChangeUIPose(Pose);
    }

    void ParamToggleClic(Toggle change)
    {
        ChangeUIParam(Param);
    }

    void ChangeUIPose(GameObject Group)
    {
        if (PoseToggle.isOn)
        {
            Group.GetComponent<CanvasGroup>().alpha = 1;
            Group.GetComponent<CanvasGroup>().interactable = true;
        }
        else
        {
            Group.GetComponent<CanvasGroup>().alpha = 0;
            Group.GetComponent<CanvasGroup>().interactable = false;
        }
    }

    void ChangeUIParam(GameObject Group)
    {
        if (ParamToggle.isOn)
        {
            Group.GetComponent<CanvasGroup>().alpha = 1;
            Group.GetComponent<CanvasGroup>().interactable = true;
        }
        else
        {
            Group.GetComponent<CanvasGroup>().alpha = 0;
            Group.GetComponent<CanvasGroup>().interactable = false;
        }
    }
}