using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XR_Debug : MonoBehaviour
{
    [SerializeField] private int m_maxStack = 50;
    [SerializeField] private int m_maxLength = 100;

    [SerializeField] private Button_Log m_logButton = null;
    [SerializeField] private Button_LogError m_logErrorButton = null;
    [SerializeField] private Button_LogWarning m_logWarningButton = null;

    [SerializeField] private Transform m_Origin = null;
    [SerializeField] private TextMeshProUGUI m_fullText = null;
    [SerializeField] private TextMeshProUGUI m_toggleText = null;
    [SerializeField] private Toggle m_toggle = null;
    [SerializeField] private GameObject m_leftPanel = null;
    [SerializeField] private GameObject m_rightPanel = null;

    [SerializeField] private TextMeshProUGUI m_logFilterText = null;
    [SerializeField] private TextMeshProUGUI m_errorFilterText = null;
    [SerializeField] private TextMeshProUGUI m_warningFilterText = null;

    private List<Base_Log> m_logsList = new List<Base_Log>();
    private List<Button_Log> m_logList = new List<Button_Log>();
    private List<Button_LogError> m_logErrorList = new List<Button_LogError>();
    private List<Button_LogWarning> m_logWarningList = new List<Button_LogWarning>();

    private bool m_logFilterState = false;
    private bool m_errorFilterState = false;
    private bool m_warningFilterState = false;

    private Base_Log m_logButtonClicked = null;

    private void Awake()
    {
        m_logFilterText.text = "0";
        m_errorFilterText.text = "0";
        m_warningFilterText.text = "0";
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void Start()
    {
        OnToogleValueChange();
        m_logFilterState = false;
        m_errorFilterState = false;
        m_warningFilterState = false;
        m_fullText.text = "";
    }

    public void UpdateColorLogButton(bool value)
    {
        Button btn = m_logButtonClicked.gameObject.GetComponent<Button>();
        btn.image.color = value ? btn.colors.pressedColor : btn.colors.normalColor + Color.white;
    }


    private void OnButtonClicked(Base_Log baseLog, string txt)
    {
        if(m_logButtonClicked) 
            UpdateColorLogButton(false);
        m_logButtonClicked = baseLog;
            UpdateColorLogButton(true);
        m_fullText.text = txt;
    }

    private void ComptuteLogsInstance(Base_Log baseLog, string log)
    {
        baseLog.gameObject.GetComponent<Button>().onClick.AddListener(() => OnButtonClicked(baseLog, log));

        baseLog.FullLog = log;

        if (log.Length > m_maxLength)
            baseLog.ShortedLog = log.Substring(0, m_maxLength);
        else
            baseLog.ShortedLog = log.Substring(0, log.Length);

        baseLog.Text.text = baseLog.ShortedLog;
        m_logsList.Add(baseLog);
    }


    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (m_logsList.Count > m_maxStack)
            return;

        string newString = "[" + type + "] : " + logString;

        if (type == LogType.Log)
        {
            var logInstance = Instantiate(m_logButton, m_Origin);
            ComptuteLogsInstance(logInstance, newString);
            m_logList.Add(logInstance);
            m_logFilterText.text = m_logList.Count.ToString();
        }
        else if (type == LogType.Error)
        {
            var errorInstance = Instantiate(m_logErrorButton, m_Origin);
            ComptuteLogsInstance(errorInstance, newString);
            m_logErrorList.Add(errorInstance);
            m_errorFilterText.text = m_logErrorList.Count.ToString();
        }
        else if (type == LogType.Warning)
        {
            var warningInstance = Instantiate(m_logWarningButton, m_Origin);
            ComptuteLogsInstance(warningInstance, newString);
            m_logWarningList.Add(warningInstance);
            m_warningFilterText.text = m_logWarningList.Count.ToString();
        }
    }


    protected void UpdadeColorFilterButton(Button btn, bool value)
    {
        if (btn.gameObject.name == "LogFilterButton")
            m_logFilterState = value;
        else if (btn.gameObject.name == "LogErrorFilterButton")
            m_errorFilterState = value;
        else if (btn.gameObject.name == "LogWarningFilterButton")
            m_warningFilterState = value;

        //need to add standard image color for normal color to have original color 
        btn.image.color = value ? btn.colors.pressedColor : btn.colors.normalColor + Color.white;
    }

    private void ResetLogButtonClicked()
    {
        if (m_logButtonClicked)
            UpdateColorLogButton(false);
        m_logButtonClicked = null;

        m_fullText.text = "";
    }

    public void OnLogFilterClicked()
    {
        bool value = true;
        if (m_logFilterState)
            value = false;
        for (int i = 0; i < m_logList.Count; i++)
        {
            m_logList[i].gameObject.SetActive(!m_logList[i].gameObject.activeSelf);
        }
        UpdadeColorFilterButton(m_logFilterText.transform.parent.GetComponent<Button>(), value);
        ResetLogButtonClicked();
    }

    public void OnLogErrorFilterClicked()
    {
        bool value = true;
        if (m_errorFilterState)
            value = false;
        for (int i = 0; i < m_logErrorList.Count; i++)
        {
            m_logErrorList[i].gameObject.SetActive(!m_logErrorList[i].gameObject.activeSelf);
        }
        UpdadeColorFilterButton(m_errorFilterText.transform.parent.GetComponent<Button>(), value);
        ResetLogButtonClicked();
    }

    public void OnLogWarningFilterClicked()
    {
        bool value = true;
        if (m_warningFilterState)
            value = false;
        for (int i = 0; i < m_logWarningList.Count; i++)
        {
            m_logWarningList[i].gameObject.SetActive(!m_logWarningList[i].gameObject.activeSelf);
        }
        UpdadeColorFilterButton(m_warningFilterText.transform.parent.GetComponent<Button>(), value);
        ResetLogButtonClicked();
    }

    public void OnToogleValueChange()
    {
        if(m_toggle.isOn)
        {
            m_toggleText.text = "Hide";
            m_leftPanel.SetActive(true);
            m_rightPanel.SetActive(true);
        }
        else
        {
            m_toggleText.text = "Show";
            m_leftPanel.SetActive(false);
            m_rightPanel.SetActive(false);
        }
    }


}
