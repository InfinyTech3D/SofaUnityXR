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

        /// <summary>
        /// Pointer to the Component Model gameobject related to this button
        /// </summary>
        [SerializeField] public GameObject m_targetElement;
        [SerializeField] public GameObject m_SofaContextObj;
        [SerializeField] private Toggle m_toggleButton;
        [SerializeField] private Button m_pushButton;

        /// <summary>
        /// Propreties used to switch beetween plannification and simulation modes 
        /// </summary> 
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
        [SerializeField] protected Material m_TransMatMaterial = null;
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
                Debug.LogWarning("Selected material has not been assigned");
        }

        /// <summary>
        /// add all component needed for a target (designed by a button) 
        /// </summary>
        void Start()
        {
            if (m_targetElement != null)
            {
               
                transform.GetComponentInChildren<TextMeshProUGUI>().text = this.name;

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
                if (m_modelExplorer.m_useURP)
                {
                    m_selectedMaterial = new Material(Shader.Find("CustomURPTransparancy"));
                    m_selectedMaterial.CopyPropertiesFromMaterial(m_defaultMaterial);
                    //m_selectedMaterial.SetColor("_BaseMap", m_defaultMaterial.GetColor("_BaseMap"));
                    //m_selectedMaterial.SetColor("_BaseColor", (m_selectedMaterial.GetColor("_BaseColor") * 0.3f + (Color.yellow)*0.7f));
                    /*
                     TO DO: atempt to set and get "Surface Type" to switch between Opaque and Transparent:
                     m_selectedMaterial.SetFloat("_Surface", 1.0f);
                    m_selectedMaterial.SetFloat("_Blend", 2.0f);
                    var transcolor = new Color(m_selectedMaterial.GetColor("_BaseColor").r, m_selectedMaterial.GetColor("_BaseColor").g, m_selectedMaterial.GetColor("_BaseColor").b, 0.5f);
                    m_selectedMaterial.SetColor("_BaseColor", transcolor);
                    m_selectedMaterial.SetOverrideTag("RenderType", "Transparent");
                    m_selectedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    m_selectedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    m_selectedMaterial.SetInt("_ZWrite", 0);
                    m_selectedMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m_selectedMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    m_selectedMaterial.SetShaderPassEnabled("ShadowCaster", false);*/
                    //SwitchToTrans(m_selectedMaterial);
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
            if (m_modelExplorer.m_useURP)
            {
                
                return mat.GetFloat("_Alpha");
                
            }
            else
            {
                return mat.GetFloat("_Transparency");
            }
            
        }

        /// <summary>
        /// set the transparency of the material of the target element (alpha)
        /// </summary>
        public void SetModelTransparency(float value)
        {
            Material mat = m_targetElement.GetComponent<Renderer>().material;
            if(mat == null) { Debug.LogError("can't find material"); }
            
            if (m_modelExplorer.m_useURP)
            {
                if(value < 1f)
                {
                    //SwitchToTrans(m_defaultMaterial, value);
                    //SwitchToTrans(m_selectedMaterial,value);       
                    //mat.SetFloat("_Alpha", value);
                    //m_selectedMaterial.SetFloat("_Alpha", value);
                    m_targetElement.GetComponent<Renderer>().material = m_TransMatMaterial;
                }
                else
                {
                    m_targetElement.GetComponent<Renderer>().material = m_defaultMaterial;
                }
            }
            else
            {
                mat.SetFloat("_Transparency", value);
            }

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

        /// <summary>
        /// used to switch to transparency while using URP, Actually not used needs fix
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="value"></param>
        public void SwitchToTrans (Material mat,float value)
        {
            Debug.Log(value);
            //Debug.Log(m_selectedMaterial.GetFloat("_Surface")); //(m_selectedMaterial.GetColor("_BaseColor") * 0.3f + (Color.yellow)*0.7f));
            mat.SetFloat("_Surface", 1.0f);
            mat.SetFloat("_Blend", 2.0f);
            var transcolor = new Color(mat.GetColor("_BaseColor").r, mat.GetColor("_BaseColor").g, m_selectedMaterial.GetColor("_BaseColor").b,value);
            mat.SetColor("_BaseColor", transcolor);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.SetShaderPassEnabled("ShadowCaster", false);
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
            Debug.Log("Detergrabbableelem"+value);

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
                if(m_targetElement.GetComponent<Renderer>().material != m_TransMatMaterial)
                {
                    m_targetElement.GetComponent<Renderer>().material = m_defaultMaterial;
                }
                m_modelExplorer.SetTransparencyByName(transparency, target);
            }

        }
        public void SetModelExplorer(SofaModelExplorer modelExplorer)
        {
            m_modelExplorer = modelExplorer;
        }
        
        public bool GetIsSelected() {  return isSelected; }
    }
}