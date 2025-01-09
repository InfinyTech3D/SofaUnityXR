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
        public Vector3 m_SofaContextSimuPosition;
        public Vector3 m_SofaContextPlannifPosition;
        public Quaternion m_SofaContextSimuRotation;
        public Quaternion m_SofaContextPlannifRotation;

        private SofaPlayer m_SofaPlayer;
        private SofaModelExplorer m_modelExplorer;
        private bool firstpass;


        // Start is called before the first frame update
        void Start()
        {
            m_SofaPlayer = m_SofaPlayer_Panel.GetComponent<SofaPlayer>();
            m_modelExplorer = this.GetComponent<SofaModelExplorer>();
            firstpass = true;

            if (m_modelExplorer == null)
            {
                Debug.LogError("can't find SofaModelExplorer");
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
            if (m_hideSimu == null || m_hidePlannif == null)
            {
                Debug.LogError("Please assign both m_hideSimu and m_hidePlannif in the Inspector.");
                return;
            }

            // Simulation mode
            if (isOn)
            {

                m_hideSimu.SetActive(false);
                m_hidePlannif.SetActive(true);
                foreach (SofaModelElementExplorer elm in m_modelExplorer.m_modelElementCtrls)
                {
                    var obj = elm.m_targetElement;
                    elm.m_plannifPosition = obj.transform.position;
                    elm.m_plannifRotation = obj.transform.rotation;
                    //obj.transform.position = elm.m_simuPosition;//without animation
                    StartCoroutine(SmoothTransitionVec3(elm.m_plannifPosition, elm.m_simuPosition, obj));
                    StartCoroutine(SmoothTransitionQuaternion(elm.m_plannifRotation, elm.m_simuRotation, obj));
                    //obj.transform.rotation = elm.m_simuRotation;//without animation
                    obj.GetComponent<XRGrabInteractable>().enabled = false;

                }
                m_SofaContextPlannifPosition = m_SofaContext.transform.position;
                m_SofaContextPlannifRotation = m_SofaContext.transform.rotation;
                StartCoroutine(SmoothTransitionQuaternion(m_SofaContextPlannifRotation, m_SofaContextSimuRotation, m_SofaContext));
                StartCoroutine(SmoothTransitionVec3(m_SofaContextPlannifPosition, m_SofaContextSimuPosition, m_SofaContext));

                m_SofaPlayer.startSofaSimulation();

            }
            else // Plannification & Manipulation mode
            {
                m_hideSimu.SetActive(true);
                m_hidePlannif.SetActive(false);
                foreach (SofaModelElementExplorer elm in m_modelExplorer.m_modelElementCtrls)
                {
                    var obj = elm.m_targetElement;
                    StartCoroutine(SmoothTransitionVec3(elm.m_simuPosition,elm.m_plannifPosition, obj));
                    StartCoroutine(SmoothTransitionQuaternion(elm.m_simuRotation, elm.m_plannifRotation, obj));
                    obj.GetComponent<XRGrabInteractable>().enabled = true;

                }
                StartCoroutine(SmoothTransitionQuaternion(m_SofaContextSimuRotation, m_SofaContextPlannifRotation,  m_SofaContext));
                StartCoroutine(SmoothTransitionVec3(m_SofaContextSimuPosition, m_SofaContextPlannifPosition, m_SofaContext));


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
            AddXRGrab(m_SofaContext);
            SetParentCollider(m_SofaContext);


        }

        /// <summary>
        /// Add all the element to grab the object in VR
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
        private IEnumerator SmoothTransitionVec3(Vector3 startPos, Vector3 endPos, GameObject obj)
        { 
            int numSteps = 20;
            for (int i = 0; i <= numSteps; i++)
            {
                float t = i / (float)numSteps;
                // Interpolation
                Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
                obj.transform.position = currentPos;
                yield return new WaitForSeconds(0.01f);
            }
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
            int numSteps = 20; // Nombre d'étapes dans la transition
            for (int i = 0; i <= numSteps; i++)
            {
                float t = i / (float)numSteps; // Fraction du progrès
                                               // Interpolation sphérique
                Quaternion currentRot = Quaternion.Slerp(startRot, endRot, t);
                obj.transform.rotation = currentRot;
                yield return new WaitForSeconds(0.01f); // Attente entre chaque étape
            }
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

            // Recursively search all children for colliders or renderers
            foreach (Collider childCollider in parentObj.GetComponentsInChildren<Collider>())
            {
                bounds.Encapsulate(childCollider.bounds);
                hasValidChild = true;
            }

            foreach (Renderer childRenderer in parentObj.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(childRenderer.bounds);
                hasValidChild = true;
            }

            if (!hasValidChild)
            {
                Debug.LogWarning("No children with colliders or renderers found in the hierarchy.");
                return;
            }

            // Add or update BoxCollider to match calculated bounds
            BoxCollider parentCollider = parentObj.GetComponent<BoxCollider>();
            if (parentCollider == null)
            {
                parentCollider = parentObj.AddComponent<BoxCollider>();
            }

            // Adjust the BoxCollider to match the bounds
            parentCollider.center = parentObj.transform.InverseTransformPoint(bounds.center);
            parentCollider.size = bounds.size;

        }

    }
}
