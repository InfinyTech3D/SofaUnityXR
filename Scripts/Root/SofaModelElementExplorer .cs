using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SofaModelElementExplorer : MonoBehaviour
{
    //**************************************//
    //************  Parameters  ************//
    //**************************************//

    /// Pointer to the Component Model gameobject related to this button
    [SerializeField] private GameObject m_targetElement;
    [SerializeField] private Toggle m_toggleButton;
    [SerializeField] private Button m_pushButton;

    private SofaModelExplorer m_modelExplorer = null;

    /// Bool to store the information if this model/button is selected
    protected bool isSelected = false;
    [SerializeField] protected Material m_selectedMaterial = null;
    protected Material m_defaultMaterial;
    protected float m_transBeforeHide = 1.0f;


    //**************************************//
    //************  Public API  ************//
    //**************************************//

    /// Method call at scene creation, will init the button
    void Awake()
    {
        isSelected = false;
        if (!m_selectedMaterial)
            Debug.LogError("Selected material has not been assigned");
    }

    /// <summary>
    /// add all component neded for a target (designed by a button) 
    /// </summary>
    // Start is called before the first frame update
    void Start()
    {
        if (m_targetElement != null)
        {
            transform.GetComponentInChildren<TextMeshProUGUI>().text = m_targetElement.name;

            m_defaultMaterial = m_targetElement.GetComponent<Renderer>().material;
            
            if (m_targetElement.GetComponent<BoxCollider>() == null)
                m_targetElement.AddComponent<BoxCollider>();

            if (m_targetElement.GetComponent<Rigidbody>() == null)
            {
                m_targetElement.AddComponent<Rigidbody>();
                m_targetElement.GetComponent<Rigidbody>().useGravity = false;
                

                if(m_targetElement.GetComponent<XRGrabInteractable>() == null)
                {
                    m_targetElement.AddComponent<XRGrabInteractable>();

                    m_targetElement.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask("Default");
                    m_targetElement.GetComponent<XRGrabInteractable>().throwOnDetach = false;
                    m_targetElement.GetComponent<XRGrabInteractable>().useDynamicAttach = true;
                }
                m_targetElement.GetComponent<Rigidbody>().isKinematic = true;
            }

            m_targetElement.layer = LayerMask.NameToLayer("Grabbable");


            EventTrigger trigger = m_targetElement.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = m_targetElement.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
                trigger.triggers.Add(entry);
            }

            DetermineGrabbableElement(false);
        }
    }

    /// <summary>
    /// listner when clickin button 
    /// </summary>
    /// <param name="data"></param>
    public void OnPointerDownDelegate(PointerEventData data)
    {
        OnButtonClicked();
    }

    /// <summary>
    /// activate or not the child selected depending on the toogle state
    /// </summary>
    /// <param name="value"></param>
    public void OnToggleChecked(bool value)
    {
        OnToggleCheckedImpl(value);
    }

    /// <summary>
    /// update togge
    /// </summary>
    /// <param name="value"></param>
    public void EmulateToggleChecked(bool value)
    {
        if (m_toggleButton.isOn == value)
            return;

        if (m_toggleButton != null)
            m_toggleButton.SetIsOnWithoutNotify(value);

        OnToggleCheckedImpl(value);
    }


    /// <summary>
    /// assign a child to a button 
    /// </summary>
    public void OnButtonClicked()
    {
        bool value = true;
        if (isSelected) // already selected, will unselect
            value = false;

        if (value)
            m_modelExplorer.UpdateElementSelected(this);
        else
            m_modelExplorer.UpdateElementSelected(null);

        OnModelSelectedImpl(value);
    }


    /// <summary>
    /// update bitton color
    /// </summary>
    public void OnButtonRelease()
    {
        OnModelSelectedImpl(false);
    }

    /// <summary>
    /// update button 
    /// </summary>
    /// <param name="value"></param>
    public void EmulateButtonClicked(bool value)
    {
        OnModelSelectedImpl(value);
    }

    /// <summary>
    /// return the transparancy of the materal of at the child (alpha)
    /// </summary>
    /// <returns></returns>
    public float GetModelTransparency()
    {
        Material mat = m_targetElement.GetComponent<Renderer>().material;
        float alpha = mat.GetFloat("_Transparency");
        return alpha;
    }

    /// <summary>
    /// set the transparancy of the materal of at the child (alpha)
    /// </summary>
    /// <param name="value"></param>
    public void SetModelTransparency(float value)
    {
        Material mat = m_targetElement.GetComponent<Renderer>().material;
        mat.SetFloat("_Transparency", value);

        if (m_toggleButton != null)
        {
            if (value == 0.0f)
                m_toggleButton.isOn = false;
            else
                m_toggleButton.isOn = true;
        }
    }

    public GameObject TargetElement
    {
        get => m_targetElement;
        set => m_targetElement = value;
    }

    public void SetModelExplorer(SofaModelExplorer modelExplorer)
    {
        m_modelExplorer = modelExplorer;
    }

    /// <summary>
    /// change the color of a button and model selected
    /// </summary>
    public void OnModelSelected()
    {
        OnModelSelectedImpl(true);
    }


    /// <summary>
    /// undo the changes of color of a button and model selected
    /// </summary>
    public void OnModelUnselected()
    {
        OnModelSelectedImpl(false);
    }


    public bool IstargetActive()
    {
        if (m_targetElement != null)
            return m_targetElement.activeSelf;

        return false;
    }

    //**************************************//
    //***********  Internal API  ***********//
    //**************************************//

    /// <summary>
    /// update child state depending on the toggle 
    /// </summary>
    /// <param name="value"></param>
    protected void OnToggleCheckedImpl(bool value)
    {
        if (m_targetElement != null)
            m_targetElement.SetActive(m_toggleButton.isOn);

        if (m_targetElement.activeSelf)
        {
            Material mat = m_targetElement.GetComponent<Renderer>().material;
            mat.SetFloat("_Transparency", m_transBeforeHide);
            m_modelExplorer.OnUpdateSliderNoNotif();
        }
        else
        {
            Material mat = m_targetElement.GetComponent<Renderer>().material;
            m_transBeforeHide = mat.GetFloat("_Transparency");
            mat.SetFloat("_Transparency", 0.0f);
            m_modelExplorer.OnUpdateSliderNoNotif();
        }

    }

    /// <summary>
    /// change the interaction layer mask og a grabble go to block or not the possibility to grab it;
    /// parent or child. neither both 
    /// </summary>
    /// <param name="value"></param>
    private void DetermineGrabbableElement(bool value)
    {
        if (value)
        {
            m_targetElement.transform.parent.gameObject.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask("Default");
            m_targetElement.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask("Direct Interaction");
        }
        else
        {
            m_targetElement.transform.parent.gameObject.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask("Direct Interaction");
            m_targetElement.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask("Default");
        }
    }

    /// <summary>
    /// update color when selecting child from the button
    /// </summary>
    /// <param name="value"></param>
    protected void OnModelSelectedImpl(bool value)
    {
        isSelected = value;

        //need to a standard image color for normal color to have same result as other button 
        m_pushButton.image.color = isSelected ? m_pushButton.colors.pressedColor : m_pushButton.colors.normalColor + Color.white;  
        
        ResetMaterialFromSelected(value);
        DetermineGrabbableElement(value);
    }

    /// <summary>
    /// update material of the targeted child
    /// </summary>
    /// <param name="isSelected"></param>
    public void ResetMaterialFromSelected(bool isSelected)
    {
        string target = m_targetElement.name;
        if (isSelected)
        {
            float transparency = m_modelExplorer.GetTransparencyByName(target);
            m_targetElement.GetComponent<Renderer>().material = m_selectedMaterial;
            m_modelExplorer.SetTransparencyByName(transparency, target);
        }
        else
        {
            float transparency = m_modelExplorer.GetTransparencyByName(target);
            m_targetElement.GetComponent<Renderer>().material = m_defaultMaterial;
            m_modelExplorer.SetTransparencyByName(transparency, target);
        }

    }

}
