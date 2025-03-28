using UnityEngine;
using UnityEngine.UI;
using SofaUnity;
using UnityEngine.XR.Interaction.Toolkit;
using SofaUnityXR;
using System.Collections;

namespace SofaUnityXR
{
    public class GameController : MonoBehaviour
    {
        // Public attributes
        public GameObject m_hideSimu;
        public GameObject m_hidePlannif;
        public Toggle toggleSimu;
        public GameObject m_SofaPlayer_Panel;
        public GameObject m_SofaContext;
        [SerializeField] private SofaTwoHandedGrabMoveProvider m_STHGMP;
        
        [SerializeField] private Vector3 m_SofaContextSimuPosition;
        [SerializeField] private Vector3 m_SofaContextPlannifPosition;
        [SerializeField] private Vector3 m_SofaContextSimuScale;
        [SerializeField] private Vector3 m_SofaContextPlannifScale;
        [SerializeField] private Quaternion m_SofaContextSimuRotation;
        [SerializeField] private Quaternion m_SofaContextPlannifRotation;

        private SofaPlayer m_SofaPlayer;
        [SerializeField] private SofaModelExplorer m_modelExplorer;
        private bool firstpass;

        /// <summary>
        /// To know if the app is simulation or plannification mode
        /// </summary>
        public bool SimuIsOn;

        /// <summary>
        /// To know when the corroutine that manage the animation is finish
        /// </summary>
        public bool AnimeIsOver;

        /// <summary>
        /// number of frame of switch mode animation, if too small you will have imprecision with the final position 
        /// </summary>
        private int numSteps = 20;

        /// <summary>
        /// Numstep*Timestep=minimum duration of the animation in second (approximation)
        /// </summary>
        private float Timestep = 0.01f;


        // Start is called before the first frame update
        void Start()
        {

            m_SofaPlayer = m_SofaPlayer_Panel.GetComponent<SofaPlayer>();
            firstpass = true;
            AnimeIsOver=false;

            if (m_modelExplorer == null)
            {
                Debug.LogError("can't find public SofaModelExplorer, please use the inspector to specify it");
            }

            if (toggleSimu != null)
            {
                toggleSimu.onValueChanged.AddListener(OnToggleSimuChanged);
                m_hideSimu.SetActive(true);
                m_hidePlannif.SetActive(false);
            }
            else
            {
                Debug.LogError("Please assign a Toggle to the GameController.");
            }

            m_SofaContext = GameObject.Find("SofaContext");
            if (m_SofaContext== null)
            {
                Debug.LogError("Please add a SofaContext from SofaUnity");
            }
            else
            {
                m_SofaContextSimuPosition = m_SofaContext.transform.position;
                m_SofaContextPlannifPosition = m_SofaContext.transform.position;
                m_SofaContextSimuScale = m_SofaContext.transform.localScale;
                m_SofaContextPlannifScale = m_SofaContext.transform.localScale;
                m_SofaContextSimuRotation = m_SofaContext.transform.rotation;
                m_SofaContextPlannifRotation = m_SofaContext.transform.rotation;
            }

            m_SofaPlayer.stopSofaSimulation();
            SetupSofaObject();
        }

        void Update()
        {
            //TODO: Pierre FONDA 05/12/2024
            //cut the annimation on the first frame (pretty ugly the following way)
            if (firstpass)
            {
                m_SofaPlayer.stopSofaSimulation();
                firstpass = false;
            }
        }

        /// <summary>
        /// Method to switch Sofa Simulation mode and And Plannification/Manipulation mode
        /// </summary>
        /// <param name="isOn"></param>
        private void OnToggleSimuChanged(bool isOn)
        {
            SimuIsOn = isOn;
            if (m_hideSimu == null || m_hidePlannif == null)
            {
                Debug.LogError("Please assign both m_hideSimu and m_hidePlannif in the Inspector.");
                return;
            }

            // Simulation mode
            if (SimuIsOn)
            {

                m_hideSimu.SetActive(false);
                m_hidePlannif.SetActive(true);
                m_STHGMP.enableScaling = false;
                m_STHGMP.enableRotation = false;
                m_SofaContext.GetComponent<XRGrabInteractable>().enabled = false;
                foreach (SofaModelElementExplorer elm in m_modelExplorer.m_modelElementCtrls)
                {
                    var obj = elm.m_targetElement;
                    elm.m_plannifPosition = obj.transform.position;
                    elm.m_plannifRotation = obj.transform.rotation;
                    elm.m_plannifScale = obj.transform.localScale;
                    //obj.transform.position = elm.m_simuPosition;//without animation
                    StartCoroutine(SmoothTransitionPosition(elm.m_plannifPosition, elm.m_simuPosition, obj));
                    StartCoroutine(SmoothTransitionScale(elm.m_plannifScale, elm.m_simuScale, obj));
                    StartCoroutine(SmoothTransitionQuaternion(elm.m_plannifRotation, elm.m_simuRotation, obj));
                    //obj.transform.rotation = elm.m_simuRotation;//without animation
                    obj.GetComponent<XRGrabInteractable>().interactionLayers = InteractionLayerMask.GetMask(InteractionLayerMask.LayerToName(0));


                }
                m_SofaContextPlannifPosition = m_SofaContext.transform.position;
                m_SofaContextPlannifRotation = m_SofaContext.transform.rotation;
                m_SofaContextPlannifScale = m_SofaContext.transform.localScale;
                StartCoroutine(SmoothTransitionQuaternion(m_SofaContextPlannifRotation, m_SofaContextSimuRotation, m_SofaContext));
                StartCoroutine(SmoothTransitionPosition(m_SofaContextPlannifPosition, m_SofaContextSimuPosition, m_SofaContext));
                StartCoroutine(SmoothTransitionScale(m_SofaContextPlannifScale, m_SofaContextSimuScale, m_SofaContext));

                m_SofaPlayer.startSofaSimulation();

            }
            else // Plannification & Manipulation mode
            {
                AnimeIsOver=false;
                m_STHGMP.enableScaling = true;
                m_STHGMP.enableRotation = true;
                m_hideSimu.SetActive(true);
                m_hidePlannif.SetActive(false);
                m_SofaContext.GetComponent<XRGrabInteractable>().enabled = true;
                foreach (SofaModelElementExplorer elm in m_modelExplorer.m_modelElementCtrls)
                {
                    var obj = elm.m_targetElement;
                    StartCoroutine(SmoothTransitionPosition(elm.m_simuPosition,elm.m_plannifPosition, obj));
                    StartCoroutine(SmoothTransitionQuaternion(elm.m_simuRotation, elm.m_plannifRotation, obj));
                    StartCoroutine(SmoothTransitionScale(elm.m_simuScale, elm.m_plannifScale, obj));
                    if (elm.GetIsSelected())
                    {
                        obj.GetComponent<XRGrabInteractable>().interactionLayers = InteractionLayerMask.GetMask(InteractionLayerMask.LayerToName(2));
                    }
                }
                StartCoroutine(SmoothTransitionQuaternion(m_SofaContextSimuRotation, m_SofaContextPlannifRotation,  m_SofaContext));
                StartCoroutine(SmoothTransitionPosition(m_SofaContextSimuPosition, m_SofaContextPlannifPosition, m_SofaContext));
                StartCoroutine(SmoothTransitionScale(m_SofaContextSimuScale, m_SofaContextPlannifScale, m_SofaContext));


                m_SofaPlayer.stopSofaSimulation();

            }
        }
        /// <summary>
        /// fonction to setup sofa visuals objects (give them manipulation script,boxCollider...)
        /// </summary>
        public void SetupSofaObject()
        {

            if (m_modelExplorer.m_SofaMeshs == null || m_modelExplorer.m_SofaMeshs.Count == 0)
            {
                Debug.LogWarning("No Sofa Meshs detected");
                return;
            }

            foreach (SofaModelElementExplorer elm in m_modelExplorer.m_modelElementCtrls)
            {
                AddXRGrab(elm.m_targetElement);
                elm.m_SofaContextObj = m_SofaContext;
            }
            
            SetParentCollider(m_SofaContext);
            AddXRGrab(m_SofaContext);

        }

        /// <summary>
        /// Add elements to grab the object in VR
        /// </summary>
        /// <param name="obj"></param>
        private void AddXRGrab(GameObject obj)
        {
            
            // Add Rigidbody if not already present
            if (obj.GetComponent<Rigidbody>() == null)
            {
                obj.AddComponent<Rigidbody>();
                obj.GetComponent<Rigidbody>().isKinematic = true;
                obj.GetComponent<Rigidbody>().useGravity = false;
                obj.GetComponent<Rigidbody>().excludeLayers= InteractionLayerMask.GetMask("UI");
            }

            // Add BoxCollider if not already present
            if (obj.GetComponent<BoxCollider>() == null)
            {
                obj.AddComponent<BoxCollider>();
            }

            // Add XrInterctible if not already present
            if (obj.GetComponent<XRGrabInteractable>() == null)
            {
                obj.AddComponent<XRGrabInteractable>();
                obj.GetComponent<XRGrabInteractable>().throwOnDetach = false;
                obj.GetComponent<XRGrabInteractable>().useDynamicAttach = true;
                //InteractionLayerMask.LayerToName(2) is "mixed"
                obj.GetComponent<XRGrabInteractable>().interactionLayers = InteractionLayerMask.GetMask(InteractionLayerMask.LayerToName(2));

            }
        }
        /// <summary>
        /// Coroutine to animate the movement of an object between two points
        /// </summary>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="obj">The object to move</param>
        /// <returns>nothing</returns>
        private IEnumerator SmoothTransitionPosition(Vector3 startPos, Vector3 endPos, GameObject obj)
        {
            //Debug.Log("I'm " + obj.transform.parent.gameObject.name + " I must go from " + startPos +" to " + endPos);
            for (int i = 0; i <= numSteps; i++)
            {
                // Interpolation
                obj.transform.position = Vector3.Lerp(startPos, endPos, (i / (float)numSteps)); 
                yield return new WaitForSeconds(Timestep);
            }
            obj.transform.position = endPos;
            AnimeIsOver = true;
        }
        /// <summary>
        /// Coroutine to animate the scale modifiaction
        /// </summary>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="obj">The object to move</param>
        /// <returns>nothing</returns>
        private IEnumerator SmoothTransitionScale(Vector3 startScale, Vector3 endScale, GameObject obj)
        {
            
            for (int i = 0; i <= numSteps; i++)
            {
                obj.transform.localScale = Vector3.Lerp(startScale, endScale, (i / (float)numSteps));
                yield return new WaitForSeconds(Timestep);
            }
            obj.transform.localScale = endScale;
        }
        /// <summary>
        /// Coroutine to animate the rotation of an object between two rotation
        /// </summary>
        /// <param name="startRot">Start position</param>
        /// <param name="endRot">End position</param>
        /// <param name="obj">The object to move</param>
        /// <returns>nothing</returns>
        private IEnumerator SmoothTransitionQuaternion(Quaternion startRot, Quaternion endRot, GameObject obj)
        {
            
            for (int i = 0; i <= numSteps; i++)
            {
                float t = i / (float)numSteps; // Fraction du progrès
                                               // Interpolation sphérique
                Quaternion currentRot = Quaternion.Slerp(startRot, endRot, t);
                obj.transform.rotation = currentRot;
                yield return new WaitForSeconds(Timestep); // Attente entre chaque étape
            }
            obj.transform.rotation= endRot;
        }

        /// <summary>
        ///Sets a BoxCollider on the given parent GameObject to encompass all its children,
        /// including those with colliders or renderers in the entire hierarchy.
        /// </summary>
        /// <param name="parentObj"></param>
        public void SetParentCollider(GameObject parentObj)
        {
            if (parentObj == null)
            {
                Debug.LogError("Parent object is null!");
                return;
            }

            // Initialize a Bounds object starting with the parent's position
            Bounds bounds = new Bounds(parentObj.transform.position, Vector3.zero);
            bool hasValidChild = false;

            
            foreach (BoxCollider childCollider in parentObj.GetComponentsInChildren<BoxCollider>())
            {
                Vector3 tmpVec = bounds.size;
                //searching for max
                if (tmpVec.x < childCollider.size.x)
                    tmpVec.x = childCollider.size.x;
                if (tmpVec.y < childCollider.size.y)
                    tmpVec.y = childCollider.size.y;
                if (tmpVec.z < childCollider.size.z)
                    tmpVec.z = childCollider.size.z;
                bounds.size = tmpVec;
                
                hasValidChild = true;
            }

            if (!hasValidChild)
            {
                Debug.LogWarning("No children with colliders or renderers found in the hierarchy.");
                return;
            }

            
            BoxCollider parentCollider = parentObj.GetComponent<BoxCollider>();
            if (parentCollider == null)
            {
                parentCollider = parentObj.AddComponent<BoxCollider>();
            }

           
            parentCollider.center = parentObj.transform.InverseTransformPoint(bounds.center);
            parentCollider.size = bounds.size;
            
        }

    }
}
