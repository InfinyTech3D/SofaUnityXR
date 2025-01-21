using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using SofaUnityXR;

namespace SofaUnityXR
{
    public class SofaModelElementExplorer: MonoBehaviour
    {
        //**************************************//
        //************  Parameters  ************//
        //**************************************//

        /// Pointer to the Component Model gameobject related to this button
        [SerializeField] public GameObject m_targetElement;
        [SerializeField] public GameObject m_SofaContextObj;
        [SerializeField] private Toggle m_toggleButton;
        [SerializeField] private Button m_pushButton;

        public Vector3 m_simuPosition;
        public Vector3 m_plannifPosition;
        public Vector3 m_simuScale;
        public Vector3 m_plannifScale;
        public Quaternion m_simuRotation;
        public Quaternion m_plannifRotation;

        [SerializeField] private SofaModelExplorer m_modelExplorer = null;

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
                m_targetElement.layer = LayerMask.NameToLayer("Grabbable");

                m_simuPosition = m_targetElement.transform.position;
                m_plannifPosition = m_targetElement.transform.position;
                m_simuScale= m_targetElement.transform.localScale;
                m_plannifScale = m_targetElement.transform.localScale;
                m_simuRotation = m_targetElement.transform.rotation;
                m_plannifRotation = m_targetElement.transform.rotation;

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

        public void SetButton()
        {
            if(m_pushButton != null)
            {
                m_pushButton.GetComponent<Button>().onClick.AddListener(OnButtonClicked);
            }
            else
            {
                Debug.LogError("Can't find pushButton on the prefab");
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
            if (mat == null) { Debug.LogError("can't find material"); }
            return mat.GetFloat("_Transparency");
        }

        /// <summary>
        /// set the transparency of the material of the target element (alpha)
        /// </summary>
        public void SetModelTransparency(float value)
        {
            Material mat = m_targetElement.GetComponent<Renderer>().material;
            if(mat == null) { Debug.LogError("can't find material"); }
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
                m_SofaContextObj.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask("Default");
                m_targetElement.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask(InteractionLayerMask.LayerToName(2));//our object
            }
            else
            {
                m_SofaContextObj.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask(InteractionLayerMask.LayerToName(2));
                m_targetElement.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask("Default");
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
        public void SetModelExplorer(SofaModelExplorer modelExplorer)
        {
            m_modelExplorer = modelExplorer;
        }
    }
}