using UnityEngine;
using UnityEngine.UI;
using SofaUnity;

public class GameController : MonoBehaviour
{
    // Public attributes
    public GameObject m_hideSimu;
    public GameObject m_hidePlannif;
    public Toggle toggleSimu; 
    public GameObject m_SofaPlayer_Panel;

    private SofaPlayer m_SofaPlayer;
    private bool firstpass;


    // Start is called before the first frame update
    void Start()
    {
        m_SofaPlayer = m_SofaPlayer_Panel.GetComponent<SofaPlayer>();
        firstpass = true;
        // Ensure the Toggle is assigned
        if (toggleSimu != null)
        {
            toggleSimu.onValueChanged.AddListener(OnToggleSimuChanged);
            m_hideSimu.SetActive(true);
            m_hidePlannif.SetActive(false);
            


        }
        else
        {
            Debug.LogError("Please assign a Toggle to the GameController.");
        }

        m_SofaPlayer.stopSofaSimulation();
    }

    void Update()
    {
        //TODO: Pierre FONDA 05/12/2024
        //cut the annimation on the first frame (pretty ugly the following way)
        if (firstpass)
        {
            m_SofaPlayer.stopSofaSimulation();
            firstpass = false;
        }
    }

    /// <summary>
    /// Method to switch Sofa Simulation mode and And Plannification/Manipulation mode
    /// </summary>
    /// <param name="isOn"></param>
    private void OnToggleSimuChanged(bool isOn)
    {
        if (m_hideSimu == null || m_hidePlannif == null)
        {
            Debug.LogError("Please assign both m_hideSimu and m_hidePlannif in the Inspector.");
            return;
        }

        // Simulation mode
        if (isOn)
        {
            m_hideSimu.SetActive(false);
            m_hidePlannif.SetActive(true);
            m_SofaPlayer.startSofaSimulation();
        }
        else // Plannification & Manipulation mode
        {
            m_hideSimu.SetActive(true);
            m_hidePlannif.SetActive(false);
            m_SofaPlayer.stopSofaSimulation();

        }
    }
}
