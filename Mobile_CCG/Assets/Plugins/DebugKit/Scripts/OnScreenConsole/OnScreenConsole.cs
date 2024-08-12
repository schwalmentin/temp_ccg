using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnScreenConsole : MonoBehaviour
{
    Queue<GameObject> logQueue;

    [Header("References")]
    [SerializeField] private GameObject textMessage;
    [SerializeField] private Transform content;
    [Space]
    [SerializeField] private Button buttonToggleNormalMessages;
    [SerializeField] private Button buttonToggleWarnings;
    [SerializeField] private Button buttonToggleAsserts;
    [SerializeField] private Button buttonToggleErrors;
    [SerializeField] private Button buttontoggleExceptions;

    [Header ("Startup Settings")]
    [SerializeField] private uint logSize = 100;
    [SerializeField] private float spacingBetweenLogs = 15;
    [Space]
    [SerializeField] private bool logNormalMessages = true;
    [SerializeField] private bool logWarnings = true;
    [SerializeField] private bool logAsserts = true;
    [SerializeField] private bool logErrors = true;
    [SerializeField] private bool logExceptions = true;

    [Header("Log Normal Display Settings")]
    [SerializeField] private Color normalColor = Color.black;
    [SerializeField] private float normalFontSize = 16;
    [SerializeField] private FontStyles normalStyle = FontStyles.Normal;
    [SerializeField] private bool normalIncludeStackTrace = false;

    [Header("Log Warning Display Settings")]
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private float warningFontSize = 16;
    [SerializeField] private FontStyles warningStyle = FontStyles.Normal;
    [SerializeField] private bool warningIncludeStackTrace = false;

    [Header("Log Assert Display Settings")]
    [SerializeField] private Color assertColor = Color.yellow;
    [SerializeField] private float assertFontSize = 16;
    [SerializeField] private FontStyles assertStyle = FontStyles.Normal;
    [SerializeField] private bool assertIncludeStackTrace = false;

    [Header("Log Error Display Settings")]
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private float errorFontSize = 20;
    [SerializeField] private FontStyles errorStyle = FontStyles.Bold;
    [SerializeField] private bool errorIncludeStackTrace = true;

    [Header("Log Exception Display Settings")]
    [SerializeField] private Color exceptionColor = Color.red;
    [SerializeField] private float exceptionFontSize = 20;
    [SerializeField] private FontStyles exceptionStyle = FontStyles.Bold;
    [SerializeField] private bool exceptionIncludeStackTrace = true;

    [Header("Cosmetic Enabled/Disabled Buttons Settings")]
    [SerializeField] private Color enabledButton_NormalColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color enabledButton_PressedColor = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private Color enabledButton_SelectedColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color enabledButton_HighlightedColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color enabledButton_DisabledColor = new Color(0.8f, 0.5f, 0.5f);
    [Space]
    [SerializeField] private Color disabledButton_NormalColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color disabledButton_PressedColor = new Color(0.1f, 0.1f, 0.1f);
    [SerializeField] private Color disabledButton_SelectedColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color disabledButton_HighlightedColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color disabledButton_DisabledColor = new Color(0.8f, 0.5f, 0.5f);

    private void Awake()
    {
        logQueue = new Queue<GameObject>();

        content.GetComponent<VerticalLayoutGroup>().spacing = spacingBetweenLogs;

        for (int i = 0; i < logSize; i++)
        {
            GameObject newLog = Instantiate(textMessage, content);
            newLog.SetActive(false);
            logQueue.Enqueue(newLog);
        }

        buttonToggleNormalMessages.onClick.AddListener(ToggleNormalMessages);
        buttonToggleWarnings.onClick.AddListener(ToggleWarningMessages);
        buttonToggleAsserts.onClick.AddListener(ToggleAssertMessages);
        buttonToggleErrors.onClick.AddListener(ToggleErrorMessages);
        buttontoggleExceptions.onClick.AddListener(ToggleExceptionMessages);

        logNormalMessages = !logNormalMessages;
        ToggleNormalMessages();
        logWarnings = !logWarnings;
        ToggleWarningMessages();
        logAsserts = !logAsserts;
        ToggleAssertMessages();
        logErrors = !logErrors;
        ToggleErrorMessages();
        logExceptions = !logExceptions;
        ToggleExceptionMessages();
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Log:
                if (!logNormalMessages) { return; }
                break;
            case LogType.Warning:
                if (!logWarnings) { return; }
                break;
            case LogType.Assert:
                if (!logAsserts) { return; }
                break;
            case LogType.Error:
                if (!logErrors) { return; }
                break;
            case LogType.Exception:
                if (!logExceptions) { return; }
                break;
            default:
                break;
        }

        GameObject newLog = logQueue.Dequeue();
        TMP_Text logContent = newLog.GetComponentInChildren<TMP_Text>();
        newLog.transform.SetParent(null);
        newLog.transform.SetParent(content);
        logContent.text = logString;

        switch (type)
        {
            case LogType.Log:
                if (normalIncludeStackTrace) { logContent.text += " " + stackTrace; }
                logContent.color = normalColor;
                logContent.fontSize = normalFontSize;
                logContent.fontStyle = normalStyle;
                break;

            case LogType.Warning:
                if (warningIncludeStackTrace) { logContent.text += " " + stackTrace; }
                logContent.color = warningColor;
                logContent.fontSize = warningFontSize;
                logContent.fontStyle = warningStyle;
                break;
            
            case LogType.Assert:
                if (assertIncludeStackTrace) { logContent.text += " " + stackTrace; }
                logContent.color = assertColor;
                logContent.fontSize = assertFontSize;
                logContent.fontStyle = assertStyle;
                break;
            
            case LogType.Error:
                if (errorIncludeStackTrace) { logContent.text += " " + stackTrace; }
                logContent.color = errorColor;
                logContent.fontSize = errorFontSize;
                logContent.fontStyle = errorStyle;
                break;

            case LogType.Exception:
                if (exceptionIncludeStackTrace) { logContent.text += " " + stackTrace; }
                logContent.color = exceptionColor;
                logContent.fontSize = exceptionFontSize;
                logContent.fontStyle = exceptionStyle;
                break;
            
            default:
                logContent.text = "!Undefined Log Type (using Type:Error as a fallback)! The following message was logged: " + logString + "\n" + stackTrace;
                logContent.color = errorColor;
                logContent.fontSize = errorFontSize;
                logContent.fontStyle = errorStyle;
                break;
        }

        newLog.SetActive(true);
        logQueue.Enqueue(newLog);
    }

    private void Toggle(ref bool logMessagesOfType, ref Color baseColor, ref Button buttonToApplyTo)
    {
        if (logMessagesOfType)
        {
            logMessagesOfType = false;

            ColorBlock colors = new ColorBlock();
            colors.normalColor = disabledButton_NormalColor;
            colors.pressedColor = disabledButton_PressedColor;
            colors.highlightedColor = disabledButton_HighlightedColor;
            colors.selectedColor = disabledButton_SelectedColor;
            colors.disabledColor = disabledButton_DisabledColor;
            colors.colorMultiplier = 1;

            buttonToApplyTo.colors = colors;
            buttonToApplyTo.GetComponentInChildren<TMP_Text>().color = baseColor / 2;
        }
        else
        {
            logMessagesOfType = true;

            ColorBlock colors = new ColorBlock();
            colors.normalColor = enabledButton_NormalColor;
            colors.pressedColor = enabledButton_PressedColor;
            colors.highlightedColor = enabledButton_HighlightedColor;
            colors.selectedColor = enabledButton_SelectedColor;
            colors.disabledColor = enabledButton_DisabledColor;
            colors.colorMultiplier = 1;

            buttonToApplyTo.colors = colors;
            buttonToApplyTo.GetComponentInChildren<TMP_Text>().color = baseColor;
        }
    }

    private void ToggleNormalMessages()
    {
        Toggle(ref logNormalMessages, ref normalColor, ref buttonToggleNormalMessages);
    }

    private void ToggleWarningMessages()
    {
        Toggle(ref logWarnings, ref warningColor, ref buttonToggleWarnings);
    }

    private void ToggleAssertMessages()
    {
        Toggle(ref logAsserts, ref assertColor, ref buttonToggleAsserts);
    }

    private void ToggleErrorMessages()
    {
        Toggle(ref logErrors, ref errorColor, ref buttonToggleErrors);
    }

    private void ToggleExceptionMessages()
    {
        Toggle(ref logExceptions, ref exceptionColor, ref buttontoggleExceptions);
    }
}
