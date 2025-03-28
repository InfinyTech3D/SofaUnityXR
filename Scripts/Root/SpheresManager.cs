using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace SofaUnityXR
{
    public class SpheresManager : MonoBehaviour
    {
        /// <summary>
        /// at cration left/right instance (1 instance by hand ) follow left/right hand 
        /// </summary>
        [SerializeField] private Transform m_leftHandAnchor = null;
        [SerializeField] private Transform m_rightHandAnchor = null;

        /// <summary>
        /// reference to the sphere prefab
        /// </summary>
        [SerializeField] private GameObject m_sphere = null;

        /// <summary>
        /// reference to spher parent (this) to recreate the architecure :
        /// 1 parent for many transform (transformpoint) and 1 transphormPoint => one corresponding model 
        /// (separate sphere when changing model) 
        /// </summary>
        [SerializeField] private Transform m_sphereParent = null;

        /// <summary>
        /// slider to chanche the size of the instance created 
        /// </summary>
        [SerializeField] private Slider m_sizeSphereSlider = null;

        /// <summary>
        /// reference to the interactor to check in the sphere crossed the nearest
        /// </summary>
        [SerializeField] private Transform m_leftRayInteractor = null;
        [SerializeField] private Transform m_rightRayInteractor = null;

        /// <summary>
        /// reference to model explorer used when selecting a model
        /// </summary>
        [SerializeField] private SofaModelExplorer m_modelExplorer = null;

        /// <summary>
        /// controller inputs
        /// </summary>

        /// <summary>
        /// primary button (A and X for touch controlleer ) to delete
        /// </summary>
        [Header("Inputs")]
        [SerializeField] private InputActionReference m_leftDeleteButton = null;
        [SerializeField] private InputActionReference m_rightDeleteButton = null;

        /// <summary>
        /// secondary button (B and Y for touch controller) to create 
        /// </summary>
        [SerializeField] private InputActionReference m_sphereCreatorLeft = null;
        [SerializeField] private InputActionReference m_sphereCreatorRight = null;

        /// <summary>
        /// trigger button (index finger) to select 
        /// </summary>
        [SerializeField] private InputActionReference m_triggerLeft = null;
        [SerializeField] private InputActionReference m_triggerRight = null;

        /// <summary>
        /// when pressing the button to create sphere instance the pher is in half transparency
        /// at releade the transparancy is set to max (no transparent) an a color is atribute (white by default)
        /// </summary>
        private Color m_instancesColor = Color.yellow;
        private Color m_colorHalfTransparency = new Color(1f, 1f, 1f, 0.5f);

        /// <summary>
        /// referance to the hand to follow when creating sphere
        /// </summary>
        private GameObject m_instanceLeftHand = null;
        private GameObject m_instanceRightHand = null;

        /// <summary>
        /// save the instances to place it in the worl and change the visual
        /// </summary>
        private bool m_leftInstanceCreation = false;
        private bool m_rightInstanceCreation = false;

        /// <summary>
        /// size of the sphere attribut corresponding to the slider value 
        /// (we use a slider on the user interface to change sphere size)
        /// </summary>
        private float m_newSizeSphere;

        /// <summary>
        /// to know if we are in sphere mode or not 
        /// in order to have a transform point for sphere side following transform model 
        /// we need do have access to this transform when returning in model mode (and moving it => the sphere must follow the model)
        /// instead disable sphereManager we create sphere mode value to notify some changements 
        /// </summary>
        private bool m_sphereMode = true;//false;
        
        /// <summary>
        /// transform that is followed by spheres 
        /// one transformPoint by model 
        /// the transformPoint follow the transform of the model 
        /// switch of transformPoint when changing model 
        /// desactivate/activate transformPoint when changing model 
        /// 
        /// all of that to separate spheres when switch model 
        /// </summary>
        private Transform m_transformPoint = null;

        /// <summary>
        /// save the targeted model we want to follow 
        /// </summary>
        private GameObject m_targetModel = null;

        /// <summary>
        /// boolean value to define the transform to follow when entering to spheremode
        /// suppose that at this time the model wil be loaded and has a transform to follow
        /// </summary>
        private bool m_firstTimeNewModel = false;

        /// <summary>
        /// since in some case the model is not created...yet we check the name 
        /// by default set to none in case we select no model 
        /// use to desactivate/activate transformPoint when switch model 
        /// </summary>
        private string m_oldModelName = "None";

        /// <summary>
        /// need to save the sithe of the model loaded/created since it's no set to one and change for each model
        /// </summary>
        private Vector3 m_initModelSize = Vector3.one;

        /// <summary>
        /// reference to the selected sphere 
        /// </summary>
        private GameObject m_selectedSphere = null;

        /// <summary>
        /// lists to reference the spher crossed by ray interactors
        /// </summary>
        public List<GameObject> m_interactableSpheresList = new List<GameObject>();

        

        public List<GameObject> m_sphereList = new List<GameObject>();
        [SerializeField] private List<Vector3> m_SpheresPos = new List<Vector3>();
        public GameController m_gamecontroller;

        /// <summary>
        /// need to be check before the start for the mapping to function
        /// </summary>
        public bool m_mappingOn;
        /* mapping feature is private ask us for more informations
       [SerializeField] private SphereMapping m_sphereMapping;
        */

       private void OnEnable()
       {

           m_sphereCreatorLeft.action.performed += CreateInstanceLeft;
           m_sphereCreatorLeft.action.canceled += PlaceInstanceLeft;
           m_sphereCreatorRight.action.performed += CreateInstanceRight;
           m_sphereCreatorRight.action.canceled += PlaceInstanceRight;

           m_leftDeleteButton.action.performed += DestroyLastSphere;
           m_rightDeleteButton.action.performed += DestroyLastSphere;

           m_triggerLeft.action.performed += PerformLeftSelection;
           m_triggerRight.action.performed += PerformRightSelection;

           SphereListenerManager.OnHoverEnterLeft += AddItemInLeftRayList;
           SphereListenerManager.OnHoverExitLeft += DeleteItemInLeftRayList;
           SphereListenerManager.OnHoverEnterRight += AddItemInRightRayList;
           SphereListenerManager.OnHoverExitRight += DeleteItemInRightRayList;
       }

       private void OnDisable()
       {
           m_sphereCreatorLeft.action.performed -= CreateInstanceLeft;
           m_sphereCreatorLeft.action.canceled -= PlaceInstanceLeft;
           m_sphereCreatorRight.action.performed -= CreateInstanceRight;
           m_sphereCreatorRight.action.canceled -= PlaceInstanceRight;

           m_leftDeleteButton.action.performed -= DestroySphere;
           m_rightDeleteButton.action.performed -= DestroySphere;

           m_triggerLeft.action.performed -= PerformLeftSelection;
           m_triggerRight.action.performed -= PerformRightSelection;

           SphereListenerManager.OnHoverEnterLeft -= AddItemInLeftRayList;
           SphereListenerManager.OnHoverExitLeft -= DeleteItemInLeftRayList;
           SphereListenerManager.OnHoverEnterRight -= AddItemInRightRayList;
           SphereListenerManager.OnHoverExitRight -= DeleteItemInRightRayList;
       }

       private void Start()
       {
           m_sizeSphereSlider.value = m_sphere.transform.localScale.x;
           m_newSizeSphere = m_sizeSphereSlider.value*0.5f;
           if (m_gamecontroller != null) {
               Debug.LogWarning("no gamecontroller attach to the spheremanager ");
           }
           /* mapping feature is private ask us for more informations
          if (m_mappingOn)
          {
              m_sphereMapping = this.gameObject.GetComponent<SphereMapping>();
              if (m_sphereMapping == null)
              {
                  Debug.LogError("Mapping is On on your sphere manager but you need to put a SphereMapping script on the the same object to make it function ");

              }
          }*/
    }

    //*************************SphereInstances*************************

    /// <summary>
    /// Generate Random color
    /// </summary>
    /// <returns></returns>
    public void OnButtonRandomColorClicked()
       {
           m_instancesColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
       }

       /// <summary>
       /// Change the size of the sphere created 
       /// this is to change sphere size in heand
       /// </summary>
       public void OnSizeSliderValueChange()
       {
           m_newSizeSphere = m_sizeSphereSlider.value *0.5f;
           if (m_leftInstanceCreation)
               m_instanceLeftHand.transform.localScale = new Vector3(m_newSizeSphere, m_newSizeSphere, m_newSizeSphere);

           if (m_rightInstanceCreation)
               m_instanceRightHand.transform.localScale = new Vector3(m_newSizeSphere, m_newSizeSphere, m_newSizeSphere);
       }

       /// <summary>
       /// create Instance following left hand 
       /// </summary>
       /// <param name="obj"></param>
       private void CreateInstanceLeft(InputAction.CallbackContext obj)
       {
           if (m_sphereMode)
           {
               m_instanceLeftHand = Instantiate(m_sphere, m_leftHandAnchor);
               m_leftInstanceCreation = true;
               m_instanceLeftHand.transform.localScale = new Vector3(m_newSizeSphere, m_newSizeSphere, m_newSizeSphere);
               m_instanceLeftHand.GetComponent<Renderer>().material.color = m_colorHalfTransparency;
               AddItemInSphereList(m_instanceLeftHand);//add instance in sphere list

           }
       }

       /// <summary>
       /// Place left instance to a good transform point 
       /// </summary>
       /// <param name="obj"></param>
       private void PlaceInstanceLeft(InputAction.CallbackContext obj)
       {
           if (m_sphereMode)
           {
               m_leftInstanceCreation = false;
               Vector3 position = m_instanceLeftHand.transform.position;
               //m_instanceLeftHand.transform.parent = m_transformPoint;
               SetSofaParent(m_instanceLeftHand);
               m_instanceLeftHand.transform.position = position;
               m_instanceLeftHand.GetComponent<SphereListenerManager>().DefaultColor = m_instancesColor;
               m_instanceLeftHand.GetComponent<Renderer>().material.color = m_instancesColor;
           }
       }

       /// <summary>
       /// create Instance following right hand 
       /// </summary>
       /// <param name="obj"></param>
       private void CreateInstanceRight(InputAction.CallbackContext obj)
       {
           if (m_sphereMode)
           {
               m_instanceRightHand = Instantiate(m_sphere, m_rightHandAnchor);
               m_rightInstanceCreation = true;
               m_instanceRightHand.transform.localScale = new Vector3(m_newSizeSphere, m_newSizeSphere, m_newSizeSphere);
               m_instanceRightHand.GetComponent<Renderer>().material.color = m_colorHalfTransparency;
               AddItemInSphereList(m_instanceRightHand);//add instance in sphere list

           }
       }

       /// <summary>
       /// Place left instance to a precedently defined transform point 
       /// </summary>
       /// <param name="obj"></param>
       private void PlaceInstanceRight(InputAction.CallbackContext obj)
       {
           if (m_sphereMode)
           {
               m_rightInstanceCreation = false;
               Vector3 position = m_instanceRightHand.transform.position;
               //m_instanceRightHand.transform.parent = m_transformPoint;
               SetSofaParent(m_instanceRightHand);
               m_instanceRightHand.transform.position = position;
               m_instanceRightHand.GetComponent<SphereListenerManager>().DefaultColor = m_instancesColor;
               m_instanceRightHand.GetComponent<Renderer>().material.color = m_instancesColor;
           }
       }

       /// <summary>
       /// add one item in sphere list
       /// </summary>
       /// <param name="item"></param>
       private void AddItemInSphereList(GameObject item)
       {
           m_sphereList.Add(item);
       }

       /// <summary>
       /// delete one item in sphere list
       /// </summary>
       /// <param name="item"></param>
       private void DeleteItemInSphereList(GameObject item)
       {
           m_sphereList.Remove(item);
       }

       /// <summary>
       /// get all existants sphere in list
       /// </summary>
       private void PopulateSphereList()
       {
           for (int i = 0; i < m_transformPoint.childCount; i++)
           {
               m_sphereList.Add(m_transformPoint.GetChild(i).gameObject);
           }
       }

       /// <summary>
       /// empty list spherelist
       /// </summary>
       private void EmptyList()
       {
           m_sphereList.Clear();
       }

       /// <summary>
       /// Define the tansform point to place instance (this transform will follow the model)
       /// </summary>
       /// <param name="target"></param>
       public void DefineTransformPoint(GameObject target)
       {
           for (int i = 0; i < m_sphereParent.childCount; i++)
           {
               Transform childTransform = m_sphereParent.GetChild(i);
               if (childTransform.gameObject.name == target.name + "Spheres")
               {
                   m_transformPoint = childTransform;
               }
           }
       }

       /// <summary>
       /// Enable/ Disable transform point depending on the model chosen 
       /// </summary>
       /// <param name="targetName"></param>
       /// <param name="state"></param>
       private void SetTransformPointState(string targetName, bool state)
       {
           for (int i = 0; i < m_sphereParent.childCount; i++)
           {
               Transform childTransform = m_sphereParent.GetChild(i);
               if (childTransform.gameObject.name == targetName + "Spheres")
               {
                   childTransform.gameObject.SetActive(state);
               }
           }
       }

       public void SetSofaParent(GameObject obj)
       {

           if (m_modelExplorer.m_targetElement != null)
           {
               obj.transform.parent = m_modelExplorer.m_targetElement.m_targetElement.transform;
           }
           else
           {
               obj.transform.parent = m_modelExplorer.m_sofaContext.transform;
           }
       }

       private void Update()
       {
           //*************************Follow Transform*************************
           //the spheres will follow the model only in the model mode and if a model is loaded (cannot create a sphere when no model selelected : "None")
           if (m_targetModel && !m_sphereMode)
           {
               m_transformPoint.position = m_targetModel.transform.position;
               m_transformPoint.rotation = m_targetModel.transform.rotation;
               m_transformPoint.localScale = m_targetModel.transform.localScale / m_initModelSize.x; //any coordinate could fit
           }

           int lst_length = m_sphereList.Count;

           /* mapping feature is private ask us for more informations
            * 
           if ((lst_length != 0)&& m_mappingOn)
           {
               if (m_gamecontroller.SimuIsOn)
               {
                  if (!m_gamecontroller.AnimeIsOver)//to call it only once when animation is over
                   {
                       m_sphereMapping.getUnitySpherePosition();
                   }

                   m_sphereMapping.m_followSimu = true;

               }
               else
               {
                   m_sphereMapping.m_followSimu = false;
                   m_sphereMapping.SetSphereList(m_sphereList);
               }

           }*/


        }


        //*************************Sphere Destroy*************************

        /// <summary>
        /// Sedtroy sphere selected
        /// </summary>
        /// <param name="obj"></param>
        private void DestroySphere(InputAction.CallbackContext obj)
        {
            if (m_selectedSphere)
            {
                DeleteItemInSphereList(m_selectedSphere);// remove the destroyed instance of sphere list
                Destroy(m_selectedSphere);
            }
        }

        private void DestroyLastSphere(InputAction.CallbackContext obj)
        {
            if (m_sphereList == null || m_sphereList.Count == 0)
            {
                Debug.Log(" Nothing to delete.");
                return;
            }

            // Get the last element
            GameObject lastSphere = m_sphereList[m_sphereList.Count - 1];

            // Remove it from the list
            m_sphereList.RemoveAt(m_sphereList.Count - 1);

            // Destroy the GameObject
            Destroy(lastSphere);
        }
            //*************************Sphere Selection*************************

            /// <summary>
            /// add item to the list corresponding gameObject crossed by left ray interactor 
            /// </summary>
            /// <param name="target"></param>
            public void AddItemInLeftRayList(GameObject target)
        {
           
            m_interactableSpheresList.Add(target);
        }

        /// <summary>
        /// add item to the list corresponding gameObject crossed by right ray interactor 
        /// </summary>
        /// <param name="target"></param>
        public void AddItemInRightRayList(GameObject target)
        {
            m_interactableSpheresList.Add(target);
        }

        /// <summary>
        /// delete item to the list corresponding gameObject crossed by left ray interactor 
        /// </summary>
        /// <param name="target"></param>
        public void DeleteItemInLeftRayList(GameObject target)
        {
            
            m_interactableSpheresList.Remove(target);
        }

        /// <summary>
        /// delete item to the list corresponding gameObject crossed by right ray interactor 
        /// </summary>
        /// <param name="target"></param>
        public void DeleteItemInRightRayList(GameObject target)
        {
            m_interactableSpheresList.Remove(target);
        }

        /// <summary>
        /// Return the nearest gameobject of given list if existing
        /// </summary>
        /// <param name="rayInteractorTransform"></param>
        /// <param name="interactableList"></param>
        /// <returns></returns>
        private GameObject GetNearestTarget(Transform rayInteractorTransform, List<GameObject> interactableList)
        {
            GameObject near = null;
            float smallestdistance = 99999.0f;
            for (int i = 0; i < interactableList.Count; i++)
            {
                float tempsDistance = Vector3.Distance(rayInteractorTransform.position, interactableList[i].transform.position);
                if (tempsDistance < smallestdistance)
                {
                    smallestdistance = tempsDistance;
                    near = interactableList[i];
                }
            }
            return near;
        }

        /// <summary>
        /// Select a sphere pointed by a ray interactor (the nearest of the ray interactor)
        /// </summary>
        /// <param name="target"></param>
        private void PerformSelection(GameObject target)
        {
            target.GetComponent<SphereListenerManager>().IsSelected = !target.GetComponent<SphereListenerManager>().IsSelected;
            if (target.GetComponent<SphereListenerManager>().IsSelected)
            {
                if (m_selectedSphere)
                {
                    m_selectedSphere.GetComponent<Renderer>().material.color = m_selectedSphere.GetComponent<SphereListenerManager>().DefaultColor;
                    m_selectedSphere.GetComponent<SphereListenerManager>().IsSelected = false;
                }

                m_selectedSphere = target;
                m_selectedSphere.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                target.GetComponent<Renderer>().material.color = target.GetComponent<SphereListenerManager>().DefaultColor;
                m_selectedSphere = null;
            }
        }

        /// <summary>
        /// launch the selection for left trigger if pointing on a sphere 
        /// </summary>
        /// <param name="obj"></param>
        private void PerformLeftSelection(InputAction.CallbackContext obj)
        {
            GameObject nearTarget = GetNearestTarget(m_leftRayInteractor, m_interactableSpheresList);
            if (nearTarget != null)
                PerformSelection(nearTarget);
           
        }

        /// <summary>
        /// launch the selection for right trigger if pointing on a sphere 
        /// </summary>
        /// <param name="obj"></param>
        private void PerformRightSelection(InputAction.CallbackContext obj)
        {
            GameObject nearTarget = GetNearestTarget(m_rightRayInteractor, m_interactableSpheresList);
            if (nearTarget != null)
            {
                Debug.Log("toto");
                PerformSelection(nearTarget);
            }
                
        }


        //*************************getter/setter*************************

        public bool SphereMode
        {
            get => m_sphereMode;
            set => m_sphereMode = value;
        }

        public GameObject TargetModel
        {
            set => m_targetModel = value;
        }

        public Vector3 TransformPointPosition
        {
            set => m_transformPoint.position = value;
        }

        public Quaternion TransformPointRotation
        {
            set => m_transformPoint.rotation = value;
        }

        public bool FirstTimeNewModel
        {
            get => m_firstTimeNewModel;
            set => m_firstTimeNewModel = value;
        }

        public Vector3 InitModelSize
        {
            set => m_initModelSize = value;
        }

        public GameObject SelectedSphere
        {
            get => m_selectedSphere;
            set => m_selectedSphere = value;
        }

        public List<GameObject> SphereList
        {
            get => m_sphereList;
        }
    }



}//sofaunityXR namespace
