using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.IO;
using SofaUnityXR;

namespace SofaUnityXR
{
    public class SofaModelExplorer : MonoBehaviour
    {
        //**************************************//
        //************  Parameters  ************//
        //**************************************//
        [SerializeField] private GameObject m_componentScroller = null;
        [SerializeField] private GameObject TogglePrefab;

        protected List<SofaModelElementExplorer> m_modelElementCtrls = null;
        protected SofaModelElementExplorer m_targetElement = null;

        [SerializeField] private Slider m_sliderSelected;
        [SerializeField] private Slider m_sliderOthers;

        private List<GameObject> m_listOfModelCreated = new List<GameObject>();
        private bool m_elementExist = false;

        private string m_dataPath;

        //sofa version add
        public List<GameObject> m_SofaMeshs = new List<GameObject>();
        public GameObject m_sofaContext;

        /// <summary>
        /// easiest mean to know if we create/load a model without modify the API
        /// </summary>
        private bool m_modelCreationLaunched = false;
        private bool m_modelEnabled = false;

        private string m_modelName;

        void Awake()
        {
            m_modelElementCtrls = new List<SofaModelElementExplorer>();

            m_sliderSelected.SetValueWithoutNotify(1.0f);
            m_sliderOthers.SetValueWithoutNotify(1.0f);
        }

        private void Start()
        {
            m_sofaContext = GameObject.Find("SofaContext");

            FindRenderer();

            if (m_sofaContext != null)
            {
                foreach (GameObject obj in m_SofaMeshs)
                {
                    var btn = Instantiate(TogglePrefab).GetComponent<SofaModelElementExplorer>();
                    btn.transform.SetParent(m_componentScroller.transform);
                    btn.transform.localScale = TogglePrefab.transform.localScale;
                    btn.transform.localPosition = Vector3.zero;
                    btn.transform.localRotation = Quaternion.identity;
                    btn.TargetElement = obj;
                    btn.name = obj.name;
                    m_modelElementCtrls.Add(btn);
                }
            }
        }

        public void PopulateModelElements()
        {
            if (m_componentScroller == null)
                return;

            foreach (var item in m_componentScroller.transform.OfType<Transform>().ToList())
            {
                Destroy(item.gameObject);
            }

            if (m_modelElementCtrls != null)
                m_modelElementCtrls.Clear();
            else
                m_modelElementCtrls = new List<SofaModelElementExplorer>();

            m_targetElement = null;
        }

        void Update()
        {
            // Any update logic can go here
        }

        public void UpdateElementSelected(SofaModelElementExplorer elem)
        {
            if (m_targetElement)
                m_targetElement.OnButtonRelease();

            m_targetElement = elem;

            if (m_sliderSelected != null && m_targetElement != null)
                m_sliderSelected.value = m_targetElement.GetModelTransparency();
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

        private void FindRenderer()
        {
            m_SofaMeshs.Clear();

            if (m_sofaContext == null)
            {
                Debug.LogError("SofaContext is not created please add one");
                return;
            }

            MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (IsChildOf(meshRenderer.gameObject, m_sofaContext))
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