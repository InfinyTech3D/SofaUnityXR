using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class XR_FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_fpsText = null;
    [SerializeField] private int m_refreshRate = 10;

    private int m_frameCounter;
    private float m_totalTime;

    // Start is called before the first frame update
    void Start()
    {
        m_frameCounter = 0;
        m_totalTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_frameCounter == m_refreshRate)
        {
            float averageFps = (1.0f / (m_totalTime / m_refreshRate));
            m_fpsText.text = averageFps.ToString("F1") + " FPS";
            m_frameCounter = 0;
            m_totalTime = 0;
        }
        else
        {
            m_totalTime += Time.deltaTime;
            m_frameCounter++;
        }
    }
}
