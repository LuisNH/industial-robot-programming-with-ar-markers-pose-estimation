using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Calibration : MonoBehaviour
{
    public Button SkipButton;
    public Button ContinueButton;

    public TMP_InputField FMXField;
    public TMP_InputField FMYField;
    public TMP_InputField FMZField;
    public TMP_InputField FMAField;
    public TMP_InputField FMBField;
    public TMP_InputField FMCField;
    public TMP_InputField FMDField;

    public TMP_InputField MMXField;
    public TMP_InputField MMYField;
    public TMP_InputField MMZField;
    public TMP_InputField MMAField;
    public TMP_InputField MMBField;
    public TMP_InputField MMCField;
    public TMP_InputField MMDField;

    public TMP_InputField IP1Field;
    public TMP_InputField IP2Field;
    public TMP_InputField IP3Field;
    public TMP_InputField IP4Field;
    public TMP_InputField rxPortField;
    public TMP_InputField txPortField;

    public TMP_InputField SamplesField;

    public TMPro.TextMeshProUGUI SamplesText;

    public TMP_Dropdown MethodDropdown;

    void Start()
    {
        SamplesField.text = "3";

        SamplesText.enabled = false;
        SamplesField.gameObject.SetActive(false);

        IP1Field.text = "192";
        IP2Field.text = "168";
        IP3Field.text = "0";
        IP4Field.text = "1";
        rxPortField.text = "8080";
        txPortField.text = "8081";

        ContinueButton.interactable = false;

        MethodDropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(MethodDropdown); });

        SkipButton.onClick.AddListener(SkipButtonClic);
        ContinueButton.onClick.AddListener(ContinueButtonClic);
    }

    void Update()
    {
        SkipButtonInteractable();
        ContinueButtonInteractable();
    }

    void SkipButtonInteractable()
    {
        if (!string.IsNullOrWhiteSpace(rxPortField.text)
                && !string.IsNullOrWhiteSpace(txPortField.text)
                && !string.IsNullOrWhiteSpace(IP1Field.text)
                && !string.IsNullOrWhiteSpace(IP2Field.text)
                && !string.IsNullOrWhiteSpace(IP3Field.text)
                && !string.IsNullOrWhiteSpace(IP4Field.text)
            )
        {
            if (SamplesField.gameObject.activeSelf)
            {
                if (!string.IsNullOrWhiteSpace(SamplesField.text))
                {
                    if (!SkipButton.interactable)
                    { SkipButton.interactable = true; }
                }
                else
                {
                    if (SkipButton.interactable)
                    { SkipButton.interactable = false; }

                }
            }
            else
            {
                if (!SkipButton.interactable)
                { SkipButton.interactable = true; }

            }
        }
        else
        {
            if (SkipButton.interactable)
            { SkipButton.interactable = false; }
        }
    }

    void ContinueButtonInteractable()
    {

        if (!string.IsNullOrWhiteSpace(FMXField.text)
                && !string.IsNullOrWhiteSpace(FMYField.text)
                && !string.IsNullOrWhiteSpace(FMZField.text)
                && !string.IsNullOrWhiteSpace(FMAField.text)
                && !string.IsNullOrWhiteSpace(FMBField.text)
                && !string.IsNullOrWhiteSpace(FMCField.text)
                && !string.IsNullOrWhiteSpace(FMDField.text)
                && !string.IsNullOrWhiteSpace(MMXField.text)
                && !string.IsNullOrWhiteSpace(MMYField.text)
                && !string.IsNullOrWhiteSpace(MMZField.text)
                && !string.IsNullOrWhiteSpace(MMAField.text)
                && !string.IsNullOrWhiteSpace(MMBField.text)
                && !string.IsNullOrWhiteSpace(MMCField.text)
                && !string.IsNullOrWhiteSpace(MMDField.text)
                && !string.IsNullOrWhiteSpace(rxPortField.text)
                && !string.IsNullOrWhiteSpace(txPortField.text)
                && !string.IsNullOrWhiteSpace(IP1Field.text)
                && !string.IsNullOrWhiteSpace(IP2Field.text)
                && !string.IsNullOrWhiteSpace(IP3Field.text)
                && !string.IsNullOrWhiteSpace(IP4Field.text)
            )
        {
            if (SamplesField.gameObject.activeSelf)
            {
                if (!string.IsNullOrWhiteSpace(SamplesField.text))
                {
                    if (!ContinueButton.interactable)
                    { ContinueButton.interactable = true; }
                }
                else
                {
                    if (ContinueButton.interactable)
                    { ContinueButton.interactable = false; }

                }
            }
            else
            {
                if (!ContinueButton.interactable)
                { ContinueButton.interactable = true; }

            }
        }
        else
        {
            if (ContinueButton.interactable)
            { ContinueButton.interactable = false; }
        }
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        if (change.value == 0)
        {
            SamplesText.enabled = false;
            SamplesField.gameObject.SetActive(false);
        }
        else
        {
            SamplesText.enabled = true;
            SamplesField.gameObject.SetActive(true);
        }
    }

    void SaveDefaultCalibration()
    {
        int IP1 = int.Parse(IP1Field.text);
        int IP2 = int.Parse(IP2Field.text);
        int IP3 = int.Parse(IP3Field.text);
        int IP4 = int.Parse(IP4Field.text);
        int rxPort = int.Parse(rxPortField.text);
        int txPort = int.Parse(txPortField.text);

        PlayerPrefs.SetFloat("FMX", -0.3050634f);
        PlayerPrefs.SetFloat("FMY", 0.024944f);
        PlayerPrefs.SetFloat("FMZ", 0.0007f);
        PlayerPrefs.SetFloat("FMA", 0.154822573f);
        PlayerPrefs.SetFloat("FMB", 0f);
        PlayerPrefs.SetFloat("FMC", 0.987942338f);
        PlayerPrefs.SetFloat("FMD", 0f);

        PlayerPrefs.SetFloat("MMX", -0.006366664f);
        PlayerPrefs.SetFloat("MMY", -0.0595391f);
        PlayerPrefs.SetFloat("MMZ", -0.00001015978f);
        PlayerPrefs.SetFloat("MMA", 0.995820403f);
        PlayerPrefs.SetFloat("MMB", 0.00538534299f);
        PlayerPrefs.SetFloat("MMC", 0.0911586955f);
        PlayerPrefs.SetFloat("MMD", 0.00168016029f);

        PlayerPrefs.SetInt("IP1", IP1);
        PlayerPrefs.SetInt("IP2", IP2);
        PlayerPrefs.SetInt("IP3", IP3);
        PlayerPrefs.SetInt("IP4", IP4);
        PlayerPrefs.SetInt("rxPort", rxPort);
        PlayerPrefs.SetInt("txPort", txPort);
    }

    void SaveCalibration()
    {
        float FMX = float.Parse(FMXField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        float FMY = float.Parse(FMYField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        float FMZ = float.Parse(FMZField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        float FMA = float.Parse(FMAField.text, CultureInfo.InvariantCulture.NumberFormat);
        float FMB = float.Parse(FMBField.text, CultureInfo.InvariantCulture.NumberFormat);
        float FMC = float.Parse(FMCField.text, CultureInfo.InvariantCulture.NumberFormat);
        float FMD = float.Parse(FMDField.text, CultureInfo.InvariantCulture.NumberFormat);
        float MMX = float.Parse(MMXField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        float MMY = float.Parse(MMYField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        float MMZ = float.Parse(MMZField.text, CultureInfo.InvariantCulture.NumberFormat) / 1000;
        float MMA = float.Parse(MMAField.text, CultureInfo.InvariantCulture.NumberFormat);
        float MMB = float.Parse(MMBField.text, CultureInfo.InvariantCulture.NumberFormat);
        float MMC = float.Parse(MMCField.text, CultureInfo.InvariantCulture.NumberFormat);
        float MMD = float.Parse(MMDField.text, CultureInfo.InvariantCulture.NumberFormat);
        int IP1 = int.Parse(IP1Field.text);
        int IP2 = int.Parse(IP2Field.text);
        int IP3 = int.Parse(IP3Field.text);
        int IP4 = int.Parse(IP4Field.text);
        int rxPort = int.Parse(rxPortField.text);
        int txPort = int.Parse(txPortField.text);

        PlayerPrefs.SetFloat("FMX", FMX);
        PlayerPrefs.SetFloat("FMY", FMY);
        PlayerPrefs.SetFloat("FMZ", FMZ);
        PlayerPrefs.SetFloat("FMA", FMA);
        PlayerPrefs.SetFloat("FMB", FMB);
        PlayerPrefs.SetFloat("FMC", FMC);
        PlayerPrefs.SetFloat("FMD", FMD);

        PlayerPrefs.SetFloat("MMX", MMX);
        PlayerPrefs.SetFloat("MMY", MMY);
        PlayerPrefs.SetFloat("MMZ", MMZ);
        PlayerPrefs.SetFloat("MMA", MMA);
        PlayerPrefs.SetFloat("MMB", MMB);
        PlayerPrefs.SetFloat("MMC", MMC);
        PlayerPrefs.SetFloat("MMD", MMD);

        PlayerPrefs.SetInt("IP1", IP1);
        PlayerPrefs.SetInt("IP2", IP2);
        PlayerPrefs.SetInt("IP3", IP3);
        PlayerPrefs.SetInt("IP4", IP4);
        PlayerPrefs.SetInt("rxPort", rxPort);
        PlayerPrefs.SetInt("txPort", txPort);
    }

    void SkipButtonClic()
    {

        SaveDefaultCalibration();

        if (MethodDropdown.value == 0)
        {
            SceneManager.LoadScene("PoseEstimation1MobileMarker", LoadSceneMode.Single);
        }
        else
        {
            int Samples = int.Parse(SamplesField.text);
            PlayerPrefs.SetInt("Samples", Samples);

            SceneManager.LoadScene("PoseEstimation2MobileMarkers", LoadSceneMode.Single);
        }

    }

    void ContinueButtonClic()
    {

        SaveCalibration();

        if (MethodDropdown.value == 0)
        {
            SceneManager.LoadScene("PoseEstimation1MobileMarker", LoadSceneMode.Single);
        }
        else
        {
            int Samples = int.Parse(SamplesField.text);
            PlayerPrefs.SetInt("Samples", Samples);

            SceneManager.LoadScene("PoseEstimation2MobileMarkers", LoadSceneMode.Single);
        }
    }

}
