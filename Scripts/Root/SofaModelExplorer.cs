using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.IO;

public class SofaModelExplorer : MonoBehaviour
{
    //**************************************//
    //************  Parameters  ************//
    //**************************************//
    [SerializeField] private TMP_Dropdown m_modelSelector = null;
    [SerializeField] private GameObject m_componentScroller = null;
    [SerializeField] private GameObject TogglePrefab;

    protected List<string> m_DropOptions = null;
    protected List<SofaModelElementExplorer> m_modelElementCtrls = null;
    protected SofaModelElementExplorer m_targetElement = null;

    [SerializeField] private Slider m_sliderSelected;
    [SerializeField] private Slider m_sliderOthers;

    [SerializeField] private ModelController m_modelController = null;

    private List<GameObject> m_listOfModelCreated = new List<GameObject>();
    private bool m_elementExist = false;

    private string m_dataPath;

    /// <summary>
    /// easiest mean to know if we create/load a model without modify the API
    /// </summary>
    private bool m_modelCreationLaunched = false;
    private bool m_modelEnabled = false;

    private string m_modelName;

    /// <summary>
    /// Initialize modelSelector (dropdown)
    /// </summary>
    void Awake()
    {
        if (m_modelSelector == null)
        {
            Debug.LogError("ModelExplorer Dropdown is null");
            return;
        }

        m_modelSelector.enabled = false;
        if (m_DropOptions == null) // first time
        {
            m_modelSelector.enabled = true;
            m_DropOptions = new List<string>();
            m_DropOptions.Add("None");
        }
        m_modelSelector.ClearOptions();
        m_modelSelector.AddOptions(m_DropOptions);

        m_modelSelector.onValueChanged.AddListener(delegate
        {
            OnDropdownValueChanged(m_modelSelector);
        });

        m_modelElementCtrls = new List<SofaModelElementExplorer>();

        m_sliderSelected.SetValueWithoutNotify(1.0f);
        m_sliderOthers.SetValueWithoutNotify(1.0f);

    }

    private void Start()
    {
        if (m_modelController)
        {
            string modelsPath = string.Empty;
            // Assign the path depending we are the unity editor or not to use models
#if UNITY_EDITOR
        m_dataPath = Application.dataPath + "/Resources/Models/";
#else
            m_dataPath = Application.dataPath + "/StreamingAssets/Resources/Models/";
#endif

            modelsPath = m_dataPath;
            SetupModelExplorer(modelsPath);
        }
        else
            Debug.LogError("No modelController assigned !!!");
    }

    /// <summary>
    /// for each folder add an item to the model selector
    /// </summary>
    /// <param name="modelsPath"></param>
    public void SetupModelExplorer(string modelsPath)
    {
        string[] folders = Directory.GetDirectories(modelsPath);

        for (int i = 0; i < folders.Length; i++)
        {
            m_modelController.AddFolderPath(folders[i]);
            folders[i] = folders[i].Replace(modelsPath, "");

            AddModel(folders[i]);
        }
    }


    /// <summary>
    /// Reference a folder in the model selecor
    /// </summary>
    /// <param name="modelName"></param>
    public void AddModel(string modelName)
    {
        if (m_modelSelector == null)
        {
            Debug.LogError("ModelExplorer Dropdown is null");
            return;
        }

        if (m_DropOptions == null) // first time
        {
            m_modelSelector.enabled = true;
            m_DropOptions = new List<string>();
            m_DropOptions.Add("None");
        }
        // add new model
        m_DropOptions.Add(modelName);

        // update dropdown
        int value = m_modelSelector.value;
        m_modelSelector.ClearOptions();
        m_modelSelector.AddOptions(m_DropOptions);
        m_modelSelector.value = value;
    }


    /// <summary>
    /// Create or select a model depending on the model selected
    /// </summary>
    /// <param name="change"></param>
    public void OnDropdownValueChanged(TMP_Dropdown change)
    {
        string modelName = m_modelSelector.options[change.value].text;
        GameObject current = m_modelController.GetTargetModel();
        if (current && modelName == current.name) // nothing to do
            return;

        m_listOfModelCreated = m_modelController.GetModelCreated();

        if (m_listOfModelCreated.Count > 0 && m_listOfModelCreated != null)
        {
            for (int i = 0; i < m_listOfModelCreated.Count; i++)
            {
                if (m_listOfModelCreated[i].activeSelf)
                {
                    m_listOfModelCreated[i].SetActive(false);
                }
            }
        }

        m_modelName = modelName;
        if (modelName != "None")
        {
            for (int i = 0; i < m_listOfModelCreated.Count; i++)
            {
                if (m_listOfModelCreated[i].name.Equals(modelName))
                {
                    m_modelEnabled = true;
                    m_elementExist = true;
                    m_listOfModelCreated[i].SetActive(true);

                    m_modelController.SetTargetModel(m_listOfModelCreated[i]);


                    m_modelController.ResetModel(m_listOfModelCreated[i]);
                    ResetMaterial();
                    ResetTransparancy();


                    PopulateModelElements();
                    return;
                }
                else
                    m_elementExist = false;
            }

            if (!m_elementExist)
            {
                string modelPath = "";
                List<string> listOfPath = m_modelController.GetFolderPath();
                for (int i = 0; i < listOfPath.Count; i++)
                {
                    string tempModelName = listOfPath[i].Replace(m_dataPath, "");
                    if (tempModelName.Equals(modelName))
                        modelPath = listOfPath[i];
                }
                m_modelController.CreateModelSelected(modelPath, modelName);
                m_modelCreationLaunched = true;
            }
        }
        else
        {
            m_modelController.SetTargetModel(null);
            if (m_componentScroller == null)
            {
                Debug.LogError("component Scroller not assigned ");
                return;
            }
            ResetMaterial();
            ResetTransparancy();

            foreach (var item in m_componentScroller.transform.OfType<Transform>().ToList())
            {
                Destroy(item.gameObject);
            }
            m_modelElementCtrls.Clear();
        }

    }



    /// <summary>
    /// For a model look for each child ant create a button in the content of ui
    /// </summary>
    public void PopulateModelElements()
    {
        if (m_componentScroller == null)
            return;
        // Destroy old data
        foreach (var item in m_componentScroller.transform.OfType<Transform>().ToList())
        {
            Destroy(item.gameObject);
        }
        if (m_modelElementCtrls != null)
            m_modelElementCtrls.Clear();
        else
            m_modelElementCtrls = new List<SofaModelElementExplorer>();
        m_targetElement = null;

        GameObject target = m_modelController.GetTargetModel();
        if (target == null)
            return;

        foreach (var item in target.transform.OfType<Transform>().ToList())
        {
            int i = 0;
            //TODO: BUG for button intanciation to fix later 
            /// <summary>
            /// name: M Coquelin
            /// date: 15/02/23
            /// description: 
            /// Appear when choosing a model, grabbing it, release it, change modeel, come back to the other model: one more button created called "[Direct Interactor] Dynamic Attach"
            /// Grab a model parent give an attach point gameObject (as child of the model) called "[Direct Interactor] Dynamic Attach".
            /// Seem that this object is not child when releasing the model(not present in the scene hierachy).
            /// But at the result when the children are checked to insantiate buttons, a new button is here.
            /// </summary>
            if (item.gameObject.name != "[Direct Interactor] Dynamic Attach")
            {
                var btn = Instantiate(TogglePrefab).GetComponent<SofaModelElementExplorer>();
                btn.SetModelExplorer(this);
                btn.transform.SetParent(m_componentScroller.transform);
                btn.transform.localScale = TogglePrefab.transform.localScale;
                btn.transform.localPosition = Vector3.zero;
                btn.transform.localRotation = Quaternion.identity;
                btn.TargetElement = item.gameObject;
                btn.name = item.gameObject.name;
                m_modelElementCtrls.Add(btn);
                //m_modelElementCtrls[i].SetModelExplorer(this);
                //print("number element in the list = " + m_modelElementCtrls.Count);
                //print("name of the last element = " + m_modelElementCtrls[m_modelElementCtrls.Count - 1]);
            }
            //var btn = Instantiate(TogglePrefab).GetComponent<ModelElementController>();
            //btn.transform.SetParent(m_componentScroller.transform);
            //btn.transform.localScale = TogglePrefab.transform.localScale;
            //btn.transform.localPosition = Vector3.zero;
            //btn.transform.localRotation = Quaternion.identity;
            //btn.TargetElement = item.gameObject;
            //btn.name = item.gameObject.name;
            //m_modelElementCtrls.Add(btn);
            i++;
        }
    }

    void Update()
    {
        if (m_modelController.GetThreadLoader() != null && !m_modelController.GetThreadLoader().AllLoaded)
        {
            m_modelController.LoadModel();
        }
        if (m_modelController.GetThreadLoader() != null && m_modelController.GetThreadLoader().AllLoaded && !m_modelController.NewGameObject)
        {
            if (m_modelController.GetTargetModel())
                m_modelController.UpdateBoxCollider(m_modelController.GetTargetModel());
            if (!m_modelController.NextFrame)
            {
                m_modelController.InitiateModelPosition(m_modelController.GetTargetModel());
                m_modelController.ModelLoaded = true;
                m_modelController.NextFrame = true;
            }
        }

        if (m_modelController.GetThreadLoader() != null && m_modelController.GetThreadLoader().AllLoaded && m_modelController.NewGameObject)
        {
            m_modelController.GetChildrenWithMeshFilter();
            m_modelController.DeleteChildrenWithoutMeshFilter();
            if (!m_modelController.NextFrame)
                m_modelController.NextFrame = true;
            else
            {
                m_modelController.SetTargetModel(m_modelController.GetModelParent());

                ResetMaterial();
                ResetTransparancy();
                PopulateModelElements();

                m_modelController.NextFrame = false;
                m_modelController.NewGameObject = false;
            }

        }

    }

    /// <summary>
    /// select the targeted child of a the model when clicking on a button
    /// </summary>
    /// <param name="elem"></param>
    public void UpdateElementSelected(SofaModelElementExplorer elem)
    {
        if (m_targetElement) // release previous button
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


    /// <summary>
    /// Restet position of the model on button "reset 3d model" clicked
    /// </summary>
    public void OnResetPositionClicked()
    {
        GameObject targetModel = m_modelController.GetTargetModel();
        m_modelController.ResetModel(targetModel);
    }

    /// <summary>
    /// Reset material color to release selected (color) when changing model ;
    /// be at the same sate to the new buton when selcting this model the next time
    /// </summary>
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

    /// <summary>
    /// reset transparency
    /// </summary>
    public void ResetTransparancy()
    {
        if (m_modelElementCtrls.Count > 0)
        {
            m_sliderSelected.SetValueWithoutNotify(1.0f);
            m_sliderOthers.SetValueWithoutNotify(1.0f);
            foreach (var item in m_modelElementCtrls)
            {
                item.SetModelTransparency(1f);
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
    }

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
    /// Helper function to check if an object is a child of a specific parent
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Quit the app
    /// </summary>
    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }


    public ModelController ModelController
    {
        set => m_modelController = value;
        get => m_modelController;
    }

    public SofaModelElementExplorer TargetElement
    {
        get => m_targetElement;
    }

    public bool ModelCreationLaunched
    {
        get => m_modelCreationLaunched;
        set => m_modelCreationLaunched = value;
    }

    public bool ModelEnable
    {
        get => m_modelEnabled;
        set => m_modelEnabled = value;
    }

    public string ModelName
    {
        get => m_modelName;
    }
}