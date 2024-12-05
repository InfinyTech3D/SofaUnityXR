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
    /// add all component needed for a target (designed by a button) 
    /// </summary>
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

                if (m_targetElement.GetComponent<XRGrabInteractable>() == null)
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
        }
    }

    /// <summary>
    /// listener when clicking button 
    /// </summary>
    public void OnPointerDownDelegate(PointerEventData data)
    {
        OnButtonClicked();
    }

    /// <summary>
    /// activate or not the child selected depending on the toggle state
    /// </summary>
    public void OnToggleChecked(bool value)
    {
        OnToggleCheckedImpl(value);
    }

    /// <summary>
    /// update toggle
    /// </summary>
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
        bool value = !isSelected; // Toggle selection state
        OnModelSelectedImpl(value);
    }

    /// <summary>
    /// update button color
    /// </summary>
    public void OnButtonRelease()
    {
        OnModelSelectedImpl(false);
    }

    /// <summary>
    /// update button 
    /// </summary>
    public void EmulateButtonClicked(bool value)
    {
        OnModelSelectedImpl(value);
    }

    /// <summary>
    /// return the transparency of the material of the target element (alpha)
    /// </summary>
    public float GetModelTransparency()
    {
        Material mat = m_targetElement.GetComponent<Renderer>().material;
        return mat.GetFloat("_Transparency");
    }

    /// <summary>
    /// set the transparency of the material of the target element (alpha)
    /// </summary>
    public void SetModelTransparency(float value)
    {
        Material mat = m_targetElement.GetComponent<Renderer>().material;
        mat.SetFloat("_Transparency", value);

        if (m_toggleButton != null)
        {
            m_toggleButton.isOn = value > 0.0f;
        }
    }

    public GameObject TargetElement
    {
        get => m_targetElement;
        set => m_targetElement = value;
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

    public bool IsTargetActive()
    {
        return m_targetElement != null && m_targetElement.activeSelf;
    }

    //**************************************//
    //***********  Internal API  ***********//
    //**************************************//

    /// <summary>
    /// update child state depending on the toggle 
    /// </summary>
    protected void OnToggleCheckedImpl(bool value)
    {
        if (m_targetElement != null)
        {
            m_targetElement.SetActive(value);

            Material mat = m_targetElement.GetComponent<Renderer>().material;
            mat.SetFloat("_Transparency", value ? m_transBeforeHide : 0.0f);
        }
    }

    /// <summary>
    /// update color when selecting target from the button
    /// </summary>
    protected void OnModelSelectedImpl(bool value)
    {
        isSelected = value;

        m_pushButton.image.color = isSelected ? m_pushButton.colors.pressedColor : m_pushButton.colors.normalColor + Color.white;

        ResetMaterialFromSelected(value);
    }

    /// <summary>
    /// update material of the targeted child
    /// </summary>
    public void ResetMaterialFromSelected(bool isSelected)
    {
        if (isSelected)
        {
            m_targetElement.GetComponent<Renderer>().material = m_selectedMaterial;
        }
        else
        {
            m_targetElement.GetComponent<Renderer>().material = m_defaultMaterial;
        }
    }
}
