using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.IO;
using SofaUnityXR;
using SofaUnity;

namespace SofaUnityXR
{
    public class SofaModelExplorer : MonoBehaviour
    {
        //**************************************//
        //************  Parameters  ************//
        //**************************************//

        [SerializeField] private GameObject m_componentScroller = null;

        /// <summary>
        /// prefabs used to create element on the UI panels
        /// </summary>
        [SerializeField] private GameObject TogglePrefab;

        public List<SofaModelElementExplorer> m_modelElementCtrls = null;

        /// <summary>
        /// current selected element 
        /// </summary>
        public SofaModelElementExplorer m_targetElement = null;

        [SerializeField] private Slider m_sliderSelected;
        [SerializeField] private Slider m_sliderOthers;

        public List<GameObject> m_SofaMeshs = new List<GameObject>();
        public GameObject m_sofaContext;

        /// <summary>
        /// to know if the app is using Universal render pipline 
        /// </summary>
        public bool m_useURP = false;


        void Awake()
        {
            m_modelElementCtrls = new List<SofaModelElementExplorer>();

            m_sliderSelected.SetValueWithoutNotify(1.0f);
            m_sliderOthers.SetValueWithoutNotify(1.0f);

            m_sofaContext = GameObject.Find("SofaContext");
            m_useURP = PiplineIsURP();
            FindRenderer();
        }

        private void Start()
        {
            

            if (m_sofaContext != null)
            {
                foreach (GameObject obj in m_SofaMeshs)
                {
                    //Set the good shader
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        if (m_useURP)
                        {
                            // Using URP Render Pipeline
                            Shader newShader = Shader.Find("CustomURPTransparancy");
                            if (newShader != null)
                            {
                                renderer.material.shader = newShader;
                                //TO DO: atempt to set and get "Surface Type" to switch between Opaque and Transparent
                                //Debug.Log(Shader.PropertyToID("Surface Type"));//1823
                                //Debug.Log(newShader.GetPropertyType(Shader.PropertyToID("Surface Type")));
                                //renderer.material.SetFloat("_Surface", 0f);
                                //Debug.Log(renderer.material.//shader.surface);
                            }
                            else
                            {
                                Debug.LogError("Shader CustomURPTransparancy not found!");
                            }
                        }
                        else
                        {
                            //GraphicsSettings.currentRenderPipeline == null or something else
                            //Using Built-in Render Pipeline 
                                    
                            Shader newShader = Shader.Find("Custom/Diffuse_Stipple_Transparency");
                            if (newShader != null)
                            {
                                renderer.material.shader = newShader;
                            }
                            else
                            {
                                Debug.LogError("Shader 'Custom/Diffuse_Stipple_Transparency' not found!");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Renderer or Material is missing on a sofavisualmodels");
                    }
                    //set ui btn 
                    var btn = Instantiate(TogglePrefab).GetComponent<SofaModelElementExplorer>();
                    btn.SetModelExplorer(this);
                    btn.transform.SetParent(m_componentScroller.transform);
                    btn.transform.localScale = TogglePrefab.transform.localScale;
                    btn.transform.localPosition = Vector3.zero;
                    btn.transform.localRotation = Quaternion.identity;
                    btn.TargetElement = obj;
                    btn.name = GetSofaName(obj);
                    btn.SetButton();
                    m_modelElementCtrls.Add(btn);
                }
            }
        }



        void Update()
        {
           

        }

        /// <summary>
        /// select the targeted child of a the model when clicking on a button
        /// </summary>
        /// <param name="elem"></param>
        public void UpdateElementSelected(SofaModelElementExplorer elem)
        {
            if (m_targetElement)
                m_targetElement.OnButtonRelease();

            m_targetElement = elem;

            if (m_sliderSelected != null && m_targetElement != null)
                m_sliderSelected.value = m_targetElement.GetModelTransparency();
        }


        /// <summary>
        /// set the slider value for a given child when clickin on a button 
        /// </summary>
        /// <param name="elementName"></param>
        /// <param name="value"></param>
        public void EmulateElementSelected(string elementName, bool value)
        {
            if (m_targetElement && m_targetElement.name == elementName) // nothing to do
                return;

            foreach (var item in m_modelElementCtrls)
            {
                if (item.name == elementName)
                {
                    m_targetElement = item;
                    m_targetElement.EmulateButtonClicked(value);

                    if (m_sliderSelected != null)
                        m_sliderSelected.value = m_targetElement.GetModelTransparency();
                }
                else
                    item.EmulateButtonClicked(false);
            }
        }

        /// <summary>
        /// update the state of the toggle
        /// </summary>
        /// <param name="elementName"></param>
        /// <param name="value"></param>
        public void EmulateElementToggle(string elementName, bool value)
        {
            foreach (var item in m_modelElementCtrls)
            {
                if (item.name == elementName)
                {
                    item.EmulateToggleChecked(value);
                    break;
                }
            }
        }

        private bool PiplineIsURP()
        {

            // Check the currently active Render Pipeline
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                //Debug.Log("Using Built-in Render Pipeline");
                return false;
            }
            else
            {
                RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
                string pipelineName = pipeline.GetType().ToString();

                if (pipelineName.Contains("UniversalRender"))
                {
                    //Debug.Log("Using Universal Render Pipeline (URP)");
                    return true;
                }
                else
                {
                    Debug.LogWarning("Using a Custom Render Pipeline: " + pipelineName+ ". Unknown pipeline prevent from using some fonctionalities");
                    return false;
                }
            }
            
        }

        private string GetSofaName(GameObject obj)
        {
            var sofaNode = obj.GetComponent<SofaVisualModel>();
            if (sofaNode != null)
            {
                string displayName=sofaNode.UniqueNameId;
                int atIndex = displayName.IndexOf('@');
                if (atIndex <= 0)
                {
                    return ("Unknown Name");
                }
                displayName = displayName.Substring(0, atIndex);
                //check if there is a visu in the name
                int visuIndex = displayName.IndexOf("visu");
                if (visuIndex <= 0)
                {
                    return displayName;
                }
                else
                {
                    return  displayName.Substring(0, visuIndex-1);// -1 beacause underscore 
                }
               
              
            }
            else
            {
                return ("Unknown Name");
            }
        }
        /// Method called from user interaction click on slide, real work is done in @sa OnSliderChangedImpl
        public void OnSliderChanged(Slider ctrl)
        {

            if (m_targetElement)
                OnSliderChangedImpl(ctrl);
        }


        public void OnSliderChanged(Slider ctrl, float value)
        {
            if (m_targetElement == null)
                return;

            ctrl.value = value;
            OnSliderChangedImpl(ctrl);
        }

        /// <summary>
        /// update the slider value 
        /// </summary>
        /// <param name="sliderName"></param>
        /// <param name="value"></param>
        public void EmulateSliderChanged(string sliderName, float value)
        {
            if (value > 1) // cast problem from oculus
            {
                EmulateSliderChanged(sliderName, value * 0.1f);
                return;
            }

            if (m_sliderSelected.name == sliderName)
            {
                m_sliderSelected.value = value;
                OnSliderChangedImpl(m_sliderSelected);
            }
            else if (m_sliderOthers.name == sliderName)
            {
                m_sliderOthers.value = value;
                OnSliderChangedImpl(m_sliderOthers);
            }
        }


        /// <summary>
        /// hide model on button "hide (x)" clicked
        /// </summary>
        public void OnHideClicked()
        {
            if (m_targetElement)
            {
                bool value = !m_targetElement.IstargetActive();
                OnHideClickedImpl(value);
            }
        }

        /// <summary>
        /// enable/disable helps
        /// </summary>
        /// <param name="help"></param>
        public void OnHelpClicked(GameObject help)
        {
            help.SetActive(!help.activeSelf);
        }

        /// <summary>
        /// unselect element by calling it
        /// </summary>
        public void OnUnselectClicked()
        {
            if (m_targetElement)
            {
                UpdateElementSelected(null);
            }

        }



        /// <summary>
        /// show all model on button "show all"  clicked
        /// </summary>
        public void OnShowAllClicked()
        {
            OnShowAllClickedImpl();
        }


        public void OnResetPositionClicked()
        {
            //TODO 
            //need to connect with gameController 
        }

        public void ResetMaterial()
        {
            if (m_modelElementCtrls.Count > 0)
            {
                foreach (var item in m_modelElementCtrls)
                {
                    if (item != m_targetElement)
                        continue;
                    item.ResetMaterialFromSelected(false);
                }
            }
        }

        public void ResetTransparancy()
        {
            if (m_modelElementCtrls.Count > 0)
            {
                m_sliderSelected.SetValueWithoutNotify(1.0f);
                m_sliderOthers.SetValueWithoutNotify(1.0f);
                foreach (var item in m_modelElementCtrls)
                {
                    item.SetModelTransparency(1f);
                }
            }
        }

        /// <summary>
        /// get the actual transparency of one item by name when changing material
        /// </summary>
        /// <returns></returns>
        public float GetTransparencyByName(string name)
        {
            float transparency = -1f;
            if (m_modelElementCtrls.Count > 0)
            {
                foreach (var item in m_modelElementCtrls)
                {
                    if (item.name == name)
                        return item.GetModelTransparency();
                }
            }
            return transparency;
        }

        /// <summary>
        /// set transparancy to one item by name when changing material
        /// </summary>
        /// <param name="transparencyList"></param>
        public void SetTransparencyByName(float transparency, string name)
        {
            if (m_modelElementCtrls.Count > 0)
            {
                foreach (var item in m_modelElementCtrls)
                {
                    if (item.name == name)
                    {
                        item.SetModelTransparency(transparency);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Isolate model 
        /// </summary>
        protected bool isolate = true;
        public void OnIsolateClicked()
        {
            if (m_targetElement)
            {
                OnIsolateClickedImpl(isolate);
                isolate = !isolate;
            }
        }


        /// <summary>
        /// update slider
        /// </summary>
        public void OnUpdateSliderNoNotif()
        {
            if (m_targetElement != null)
            {
                m_sliderSelected.SetValueWithoutNotify(m_targetElement.GetModelTransparency());
            }

            foreach (var item in m_modelElementCtrls)
            {
                if (item == m_targetElement)
                    continue;

                m_sliderOthers.SetValueWithoutNotify(item.GetModelTransparency());
                break;
            }
        }
        /*
        /// <summary>
        /// update slider
        /// </summary>
        /// <param name="status"></param>
        public void ParseComponentStatus(ViewerStatus status)
        {
            foreach (var item in m_modelElementCtrls)
            {
                status.m_elementTransparency[item.name] = item.GetModelTransparency();
                status.m_elementActivated[item.name] = item.IstargetActive();
            }

            if (m_targetElement)
            {
                m_sliderSelected.SetValueWithoutNotify(status.m_elementTransparency[m_targetElement.name]);
            }
            m_sliderOthers.SetValueWithoutNotify(status.m_sliderOthers);
        }

        public void SetComponentStatus(ViewerStatus status)
        {
            foreach (var item in m_modelElementCtrls)
            {
                item.SetModelTransparency(status.m_elementTransparency[item.name]);
                item.EmulateToggleChecked(status.m_elementActivated[item.name]);
            }
            status.m_sliderOthers = m_sliderOthers.value;
        }*/

        //**************************************//
        //***********  Internal API  ***********//
        //**************************************//

        /// Internal Method to do the work when a slider is changed
        protected void OnSliderChangedImpl(Slider ctrl)
        {
            if (ctrl == m_sliderSelected)
            {
                if (m_targetElement)
                {
                    m_targetElement.SetModelTransparency(ctrl.value);
                }
            }
            else if (ctrl == m_sliderOthers)
            {
                foreach (var item in m_modelElementCtrls)
                {
                    if (item == m_targetElement)
                        continue;

                    item.SetModelTransparency(ctrl.value);
                }
            }
        }


        /// Internal Method to Hide selected
        public void OnHideClickedImpl(bool value)
        {
            m_targetElement.EmulateToggleChecked(value);
        }

        /// <summary>
        /// show all models
        /// </summary>
        public void OnShowAllClickedImpl()
        {
            foreach (var item in m_modelElementCtrls)
            {
                item.EmulateToggleChecked(true);
            }
        }

        /// <summary>
        /// reset the transparancy and the slider
        /// </summary>
        public void OnResetClickedImpl()
        {
            OnSliderChanged(m_sliderSelected, 1.0f);
            OnSliderChanged(m_sliderOthers, 1.0f);

        }

        /// <summary>
        /// isolate a model
        /// </summary>
        /// <param name="isolate"></param>
        public void OnIsolateClickedImpl(bool isolate)
        {
            foreach (var item in m_modelElementCtrls)
            {
                if (item == m_targetElement)
                    continue;

                item.EmulateToggleChecked(!isolate);
            }
        }

        /// <summary>
        /// Quit the app
        /// </summary>
        public void OnQuitButtonClicked()
        {
            Application.Quit();
        }

        public SofaModelElementExplorer TargetElement
        {
            get => m_targetElement;
        }
       
        //adds for sofa version//


        private void FindRenderer()
        {
            m_SofaMeshs.Clear();

            if (m_sofaContext == null)
            {
                Debug.LogError("SofaContext is not created please add one");
                return;
            }


            MeshRenderer[] meshRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.InstanceID);

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                //check if it'a a child of sofa context and if the mesh renderer is active
                if (IsChildOf(meshRenderer.gameObject, m_sofaContext) && (meshRenderer.enabled==true))
                {
                    m_SofaMeshs.Add(meshRenderer.gameObject);
                }
            }
        }

        private bool IsChildOf(GameObject obj, GameObject parent)
        {
            Transform current = obj.transform;
            while (current != null)
            {
                if (current.gameObject == parent)
                    return true;
                current = current.parent;
            }
            return false;
        }
    }
}