using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registrationPanel;

    [Header("Output Messages")]
    [SerializeField] private Text loginOutputText;
    [SerializeField] private Text registrationOutputText;

    private void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if (Instance == null)
            Instance = this;
    }

    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        registrationPanel.SetActive(false);
        ClearMessages();
    }

    public void OpenRegistrationPanel()
    {
        registrationPanel.SetActive(true);
        loginPanel.SetActive(false);
        ClearMessages();
    }

    public void ShowLoginMessage(string message, bool isError = false)
    {
        if (loginOutputText == null) return;
        loginOutputText.text = message;
        loginOutputText.color = isError ? Color.red : Color.green;
    }

    public void ShowRegistrationMessage(string message, bool isError = false)
    {
        if (registrationOutputText == null) return;
        registrationOutputText.text = message;
        registrationOutputText.color = isError ? Color.red : Color.green;
    }

    private void ClearMessages()
    {
        if (loginOutputText != null) loginOutputText.text = "";
        if (registrationOutputText != null) registrationOutputText.text = "";
    }
}