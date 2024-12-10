using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using Dummiesman;
using System.Threading;

/// <summary>
/// Class to create and execute OBJLoader in a dedicated thread
/// Final step to create GameOBject and Material from the OBJLoader and the builder need to be 
/// called from main thread.
/// Callback could be setup to launch the creation of each new objloader from Main thread. Right now
/// creation of all OBJ is created when @sa workDone is set to true.
/// </summary>
public class ThreadLoader
{
    /// List of OBJLoader created in the thread @sa DoWork
    private List<OBJLoader> m_loaders = new List<OBJLoader>();
    
    /// Bool used to warn the fact that all obj have been loaded into builder
    private bool m_allLoaded = true;
    
    // List of obj file path to load
    private string[] m_modelParts;


    /// Default constructor taking the list of file to load
    public ThreadLoader(string modelPath)
    {
        m_modelParts = Directory.GetFiles(modelPath, "*.obj");
        Array.Sort(m_modelParts); // This needed to ensure the order of models
        //Debug.Log("found path: " + fullPath);
        //Debug.Log("nbr models: " + m_modelParts.Length);
    }

    /// Main method to launch the thread to load obj files
    public void DoWork()
    {
        // unactive loading on main thread
        m_allLoaded = true;
        // Fill vector of loader with empty objects, to avoid resize during execution on main thread
        foreach (string filePath in m_modelParts)
        {
            OBJLoader loader = new OBJLoader();
            m_loaders.Add(loader);
        }

        // active mesh creation on main thread
        m_allLoaded = false;

        // Load mesh from file into memory
        int cpt = 0;
        foreach (string filePath in m_modelParts)
        {
            OBJLoader loader = m_loaders[cpt];
            loader.LoadInBuilder(filePath);

            // warn mesh has been loaded and can be created (need to be done in main thread)
            loader.status = LoaderStatus.Loaded;            
            cpt++;
        }
    }

    public List<OBJLoader> Loaders
    {
        get => m_loaders;
    }

    public bool AllLoaded
    {
        get => m_allLoaded;
        set => m_allLoaded = value;
    }
}

public struct ModelData
{
    private GameObject m_modelCreated;
    private Vector3 m_initPosition;
    private Vector3 m_initScale;
    private Vector3 m_center;

    public GameObject ModelCreated
    {
        get => m_modelCreated;
        set => m_modelCreated = value;
    }

    public Vector3 InitPosition
    {
        get => m_initPosition;
        set => m_initPosition = value;
    }

    public Vector3 InitScale
    {
        get => m_initScale;
        set => m_initScale = value;
    }

    public Vector3 Center
    {
        get => m_center;
        set => m_center = value;
    }
}

public class ModelController : MonoBehaviour
{
    protected Vector3 oldPos;

    protected bool startedRot = false;

    protected bool startedTouch = false;

    protected float ScaleSaved = 1f;

    protected bool emitter = false;

    protected List<GameObject> m_models = null;
    protected GameObject m_targetModel = null;

    private ThreadLoader m_loader = null;
    private Thread m_loaderThread = null;

    private List<string> m_foldersPaths = new List<string>();
    private GameObject m_modelParent;
    private bool m_newGameObject;
    private bool m_nextFrame;
    private List<ModelData> m_modelDatas = new List<ModelData>();
    ModelData m_modelData = new ModelData();
    private bool m_modelLoad;
   
    void Awake()
    {
        m_models = new List<GameObject>();
    }




    /// <summary>
    /// Create parent of model and lauch thread to instantiate gameobject from obj file
    /// </summary>
    /// <param name="modelPath"></param>
    /// <param name="modelName"></param>
    public void CreateModelSelected(string modelPath, string modelName)
    {
        m_loader = new ThreadLoader(modelPath);
        m_loaderThread = new Thread(new ThreadStart(m_loader.DoWork));
        m_loaderThread.Start();
        m_modelParent = new GameObject();
        m_modelParent.transform.parent = transform;
        m_modelParent.name = modelName;
        MakeModelInteractable(m_modelParent);
        m_newGameObject = true;
        m_nextFrame = false;
        m_modelData.ModelCreated = m_modelParent;
        m_modelLoad = false;
        //StartCoroutine(LoadModelCoroutine());
    }

    /// <summary>
    /// make a given gameObject, here model parent, interactable ;  
    /// need a rigidbody, box collider and XRGrabµµInteractable component
    /// </summary>
    /// <param name="model"></param>
    public void MakeModelInteractable(GameObject model)
    {
        if (model.GetComponent<BoxCollider>() == null)
        {
            model.AddComponent<BoxCollider>();
            model.GetComponent<BoxCollider>().size = new Vector3(3f, 3f, 3f);
        }

        if (model.GetComponent<Rigidbody>() == null)
        {
            model.AddComponent<Rigidbody>();
            model.GetComponent<Rigidbody>().useGravity = false;

            if (model.GetComponent<XRGrabInteractable>() == null)
            {
                model.AddComponent<XRGrabInteractable>();
                model.GetComponent<XRBaseInteractable>().interactionLayers = InteractionLayerMask.GetMask("Direct Interaction");
                model.GetComponent<XRGrabInteractable>().throwOnDetach = false;
                model.GetComponent<XRGrabInteractable>().useDynamicAttach = true;
            }
            model.GetComponent<Rigidbody>().isKinematic = true;
        }

        model.layer = LayerMask.NameToLayer("Grabbable");
    }

    /// <summary>
    /// metod used with update to check statu of thread avancement and create each element in model
    /// </summary>
    public void LoadModel()
    {
        int counter = 0;
        bool workToDo = false;
        foreach (OBJLoader loader in m_loader.Loaders)
        {
            if (loader.status == LoaderStatus.Loaded)
            {
                GameObject objModel = loader.CreateObjectFromBuilder();
                objModel.transform.parent = m_modelParent.transform;
                objModel.transform.localPosition = Vector3.zero;
                objModel.transform.localRotation = Quaternion.identity;
                m_models.Add(objModel);
                loader.status = LoaderStatus.Created;
                counter++;
                return;
            }
            else if (loader.status == LoaderStatus.Working)
            {
                workToDo = true;
            }
        }

        if (!workToDo)
        {
            m_loader.AllLoaded = true;
        }
    }

    /// <summary>
    /// check ellement with mesh filter (each one will be a child element of our model)
    /// </summary>
    public void GetChildrenWithMeshFilter()
    {
        List<GameObject> list = new List<GameObject>();

        foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            list.Add(meshFilter.gameObject);
        }
        for (int i = 0; i < list.Count; i++)
        {
            list[i].transform.parent = m_modelParent.transform;
        }
    }

    /// <summary>
    /// need keep only the children with mesh filter so we delete the others
    /// </summary>
    public void DeleteChildrenWithoutMeshFilter()
    {
        for (int i = 0; i < m_modelParent.transform.childCount; i++)
        {
            MeshFilter meshFilter = m_modelParent.transform.GetChild(i).gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Destroy(m_modelParent.transform.GetChild(i).gameObject);

            }
        }
    }

    //private IEnumerator LoadModelCoroutine()
    //{
    //    while (m_loader != null && !m_loader.allLoaded)
    //    {
    //        yield return null;
    //        Debug.Log(m_loader);
    //        print(m_loader.allLoaded);
    //        LoadModel();
    //    } 
    //    //while(m_loader != null && m_loader.allLoaded && m_newGameObject)
    //    //{
    //    //    if (!m_nextFrame)
    //    //    {
    //    //        GetChildrenWithMeshFilter();
    //    //        DeleteChildrenWithoutMeshFilter();
    //    //        m_nextFrame = true;
    //    //    }
    //    //    yield return null;
    //    //    if (m_nextFrame)
    //    //    {
    //    //        m_targetModel = m_modelParent;

    //    //        RootNode.Instance.m_modelExplorer.ResetMaterial();
    //    //        RootNode.Instance.m_modelExplorer.ResetTransparancy();
    //    //        RootNode.Instance.m_modelExplorer.PopulateModelElements();

    //    //        m_nextFrame = false;
    //    //        m_newGameObject = false;
    //    //    }       
    //    //}
    //}


    /// <summary>
    /// update the box collider of th model to adapt for each child
    /// </summary>
    /// <param name="model"></param>
    public void UpdateBoxCollider(GameObject model)
    {
        var parentBoxCol = model.GetComponent<BoxCollider>();
        if (parentBoxCol == null)
            parentBoxCol = model.AddComponent<BoxCollider>();

        Bounds bounds = new Bounds(model.transform.position, Vector3.zero);

        bounds.min = new Vector3(99999f, 99999f, 99999f);
        bounds.max = new Vector3(-99999f, -99999f, -99999f);
        Vector3 tempBoundsMinVec = new Vector3(99999f, 99999f, 99999f);
        Vector3 tempBoundsMaxVec = new Vector3(-99999f, -99999f, -99999f);

        for (int i = 0; i < model.transform.childCount; i++)
        {
            Collider childCol = model.transform.GetChild(i).gameObject.GetComponent<Collider>();

            if (childCol != null)
            {
                // Get bounds of child and compute 8 corners of the box in global frame
                Vector3[] corners = new Vector3[8];
                corners[0] = new Vector3(childCol.bounds.min.x, childCol.bounds.min.y, childCol.bounds.min.z);
                corners[1] = new Vector3(childCol.bounds.min.x, childCol.bounds.max.y, childCol.bounds.min.z);
                corners[2] = new Vector3(childCol.bounds.max.x, childCol.bounds.max.y, childCol.bounds.min.z);
                corners[3] = new Vector3(childCol.bounds.max.x, childCol.bounds.min.y, childCol.bounds.min.z);

                corners[4] = new Vector3(childCol.bounds.min.x, childCol.bounds.min.y, childCol.bounds.max.z);
                corners[5] = new Vector3(childCol.bounds.min.x, childCol.bounds.max.y, childCol.bounds.max.z);
                corners[6] = new Vector3(childCol.bounds.max.x, childCol.bounds.max.y, childCol.bounds.max.z);
                corners[7] = new Vector3(childCol.bounds.max.x, childCol.bounds.min.y, childCol.bounds.max.z);

                // convert 8 corners of the child box in the parent local frame
                for (int j = 0; j < 8; ++j)
                    corners[j] = parentBoxCol.transform.InverseTransformPoint(corners[j]);


                // compute min and max coordinates of the 8 corners in the parent local frame
                for (int j = 0; j < 8; ++j)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        if (corners[j][k] < tempBoundsMinVec[k])
                            tempBoundsMinVec[k] = corners[j][k];

                        if (corners[j][k] > tempBoundsMaxVec[k])
                            tempBoundsMaxVec[k] = corners[j][k];
                    }
                }
            }
        }

        bounds.min = tempBoundsMinVec;
        bounds.max = tempBoundsMaxVec;
        bounds.size = bounds.max - bounds.min;

        parentBoxCol.size = tempBoundsMaxVec - tempBoundsMinVec;
        parentBoxCol.center = (tempBoundsMaxVec + tempBoundsMinVec) * 0.5f;
    }

    /// <summary>
    /// positinate the model a the good position (0, 1.5, 0 position for now) an good scale
    /// </summary>
    /// <param name="target"></param>
    public void InitiateModelPosition(GameObject target)
    {
        ResetPosition(target);
        MoveChildrenToTargetOrigin(target, GetCenter(target));
        InitModelPosition();
        InitModelScale(target);

        m_modelDatas.Add(m_modelData);
    }

    /// <summary>
    /// up the model created and save the position (will be used for each other rest of position)
    /// </summary>
    public void InitModelPosition()
    {
        m_modelParent.transform.position = Vector3.zero;
        m_modelParent.transform.position += new Vector3(0f, 1.5f, 0f);

        m_modelData.InitPosition = m_modelParent.transform.position;
    }

    /// <summary>
    /// set the scale of the model create (the model init is too big) (by dividing the max of the size)and save it 
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public float InitModelScale(GameObject target)
    {
        Vector3 dimentions = target.GetComponent<BoxCollider>().size;
        float max = dimentions.x;
        if (dimentions.y > max)
            max = dimentions.y;
        if (dimentions.z > max)
            max = dimentions.z;

        float invert = 1 / max;

        target.transform.localScale = new Vector3(invert, invert, invert);

        m_modelData.InitScale = target.transform.localScale;

        return max;
    }

    /// <summary>
    /// get the center of the model created (the center of the collider in not the same that the transform of the model)
    /// this will be use to move children
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public Vector3 GetCenter(GameObject target)
    {
        m_modelData.Center = target.GetComponent<BoxCollider>().center;
        return m_modelData.Center;
    }

    /// <summary>
    /// place the model and his children at the origin of the world
    /// </summary>
    /// <param name="target"></param>
    public void ResetPosition(GameObject target)
    {
        target.transform.position = Vector3.zero;
        target.transform.rotation = Quaternion.identity;
        target.transform.localScale = Vector3.one;
        for (int i = 0; i < target.transform.childCount; i++)
        {
            target.transform.GetChild(i).localPosition = Vector3.zero;
            target.transform.GetChild(i).localRotation = Quaternion.identity;
            target.transform.GetChild(i).localScale = Vector3.one;
        }
    }

    /// <summary>
    /// use the center to positionate the children at the transform point of the parent 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="center"></param>
    private void MoveChildrenToTargetOrigin(GameObject target, Vector3 center)
    {
        for (int i = 0; i < target.transform.childCount; i++)
        {
            target.transform.GetChild(i).localPosition -= center;
            target.transform.GetChild(i).localRotation = Quaternion.identity;
            target.transform.GetChild(i).localScale = Vector3.one;
        }
    }

    /// <summary>
    /// reset the transform of the children 
    /// </summary>
    /// <param name="target"></param>
    private void ResetModelChildren(GameObject target)
    {
        for (int i = 0; i < target.transform.childCount; i++)
        {
            target.transform.GetChild(i).localPosition = Vector3.zero;
            target.transform.GetChild(i).localRotation = Quaternion.identity;
            target.transform.GetChild(i).localScale = Vector3.one;
        }
    }

    /// <summary>
    /// set the good position and scale of the model when clicking on the button 
    /// </summary>
    /// <param name="target"></param>
    public void ResetModel(GameObject target)
    {
        
        for (int i = 0; i <  m_modelDatas.Count; i++)
        {
            if(m_modelDatas[i].ModelCreated.name == target.name)
            {
                ResetPosition(target);
                target.transform.position = m_modelDatas[i].InitPosition;
                target.transform.rotation = Quaternion.identity;
                target.transform.localScale = m_modelDatas[i].InitScale;
                ResetModelChildren(target);
                MoveChildrenToTargetOrigin(target, m_modelDatas[i].Center);
            }
        }

    }



    public ThreadLoader GetThreadLoader()
    {
        return m_loader;
    }

    public bool NewGameObject
    {
        get => m_newGameObject;
        set => m_newGameObject = value;
    }

    public bool ModelLoaded
    {
        get => m_modelLoad;
        set => m_modelLoad = value;
    }

    public bool NextFrame
    {
        get => m_nextFrame;
        set => m_nextFrame = value;
    }

    /// <summary>
    /// return targeted model 
    /// </summary>
    /// <returns></returns>
    public GameObject GetTargetModel()
    {
        return m_targetModel;
    }


    /// <summary>
    /// set target model 
    /// </summary>
    /// <param name="target"></param>
    public void SetTargetModel(GameObject target)
    {
        m_targetModel = target;
    }

    public GameObject GetModelParent()
    {
        return m_modelParent;
    }


    public Vector3 GetMinBounds(GameObject target)
    {
        return target.GetComponent<Collider>().bounds.min;
    }

    public Vector3 GetMaxBounds(GameObject target)
    {
        return target.GetComponent<Collider>().bounds.max;
    }

    /// <summary>
    /// return the list of the path of each folder
    /// </summary>
    /// <returns></returns>
    public List<string> GetFolderPath()
    {
        return m_foldersPaths;   
    }

    public void AddFolderPath(string path)
    {
        m_foldersPaths.Add(path);
    }

    /// <summary>
    /// return list of gameobject model created
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetModelCreated()
    {
        List<GameObject> temp = new List<GameObject>();
        for(int i = 0; i < m_modelDatas.Count; i++)
        {
            temp.Add(m_modelDatas[i].ModelCreated);
        }
        return temp;
    }

    
}
