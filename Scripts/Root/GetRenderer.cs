using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRenderer : MonoBehaviour
{
    [SerializeField] private GameObject TogglePrefab;
    [SerializeField] private GameObject m_componentScroller = null;

    protected List<SofaModelElementExplorer> m_modelElementCtrls = null;

    public List<GameObject> m_SofaMeshs = new List<GameObject>();
    public GameObject m_sofaContext;

    void Awake()
    {

        m_modelElementCtrls = new List<SofaModelElementExplorer>();

    }

    void Start()
    {
        
        // Clear the list in case there are existing elements
        m_SofaMeshs.Clear();
        
        m_sofaContext = GameObject.Find("SofaContext");
        if (m_sofaContext == null)
        {
            Debug.LogError("SofaContext is not created please add one");
            return;
        }

        // Find all GameObjects in the scene with MeshRenderer components
        MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();

        // Loop through all MeshRenderer components and add their GameObjects to the list if they are children of SofaContext
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            if (IsChildOf(meshRenderer.gameObject, m_sofaContext))
            {
                m_SofaMeshs.Add(meshRenderer.gameObject);
                
            }
        }
        
        foreach (GameObject obj in m_SofaMeshs) {
            if (obj.name == "OglModel  -  Visual") {
                Debug.Log($"Found {obj.transform.parent.name} with MeshRenderer under SofaContext.");
            }
            else { 
                Debug.Log($"Found {obj.name} with MeshRenderer under SofaContext.");
            
             }
        }
        
        if (m_sofaContext != null)
        {
            foreach (GameObject obj in m_SofaMeshs)
            {
                var btn = Instantiate(TogglePrefab).GetComponent<SofaModelElementExplorer>();
                //btn.SetModelExplorer(this);
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

    // Helper function to check if an object is a child of a specific parent
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
    // Update is called once per frame
    void Update()
    {

        
    }
}
