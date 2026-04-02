using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SofaUnity;
using SofaUnityAPI;
using System;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace SofaUnityXR
{
    /// <summary>
    /// Base class inherite from MonoBehavior that design allow to create a set of sphere collision models
    /// This class is a work in progress. 
    /// It allows from a Unity GameObject geometry to generate a set of sphere that approximate the object.
    /// The spheres are mapped into collision models in Sofa
    /// </summary>
    [ExecuteInEditMode]
    public class SofaSphereCollisionHand : SofaBaseObject
    {
        /////////////////////////////////////////////////
        /////   SofaSphereCollisionObject members   /////
        /////////////////////////////////////////////////

        /// Collision sphere radius
        [SerializeField] protected float m_radius = 1.0f;

        [SerializeField]
        private List<GameObject> m_capsuleColliderList = new List<GameObject>();
        private List<Vector3> m_pointsList = new List<Vector3>();

        private SofaSphereCollision m_sofaSphereCollision = new SofaSphereCollision();
        
        public SofaMesh m_sofaMesh = null;
        public string m_sofaMeshName = ""; // to automatically find it TODO
        public SofaCollisionModel m_sphereModel = null;


        /// Parameter bool to store information if vec3 or rigid are parsed.
        private bool m_ready = false;

        /////////////////////////////////////////////////
        /////  SofaSphereCollisionObject public API /////
        /////////////////////////////////////////////////

        /// <summary>
        /// Reference to SofaSphereCollision : commun part of  SofaSphereCollisionHand and SofaSphereCollisionObject
        /// </summary>
        [SerializeField]
        public SofaSphereCollision SofaSphereCollision
        {
            get => m_sofaSphereCollision;
            set => m_sofaSphereCollision = value;
        }

        /// <summary>
        /// Reference of collider list 
        /// </summary>
        public List<GameObject> CapsuleColliderList
        {
            get => m_capsuleColliderList;
            set => m_capsuleColliderList = value;
        }


        void OnDestroy()
        {
            if (m_sofaSphereCollision == null)
                return;

            m_sofaSphereCollision.ReleaseSofaSphereCollisionObject();
            m_ready = false;
        }


        //////////////////////////////////////////////////
        /////  SofaSphereCollisionObject public API  /////
        //////////////////////////////////////////////////

        // Use this for initialization
        void Start()
        {
            // Clear the capsule collider list on start to avoid duplicate 
            m_capsuleColliderList.Clear();

            // Looking for Capsule collider in children
            CapsuleCollider[] colliders = gameObject.GetComponentsInChildren<CapsuleCollider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                m_capsuleColliderList.Add(colliders[i].gameObject);
            }

            Init_impl();

        }


        // Update is called once per frame
        void Update()
        {
            if (!m_isCreated)
                return;

            // first update capsule spheres world position
            UpdatePoints();

            // equivalent from SofaMeshController::UpdateToSofa
            m_sofaSphereCollision.UpdateLoop();

        }


        /// Method to draw debug information like the vertex being grabed
        void OnDrawGizmosSelected()
        {
            //for now let's assume that all sphere has the same size...
            m_sofaSphereCollision.DrawSphereGizmos();
        }


        //////////////////////////////////////////////////
        ///// SofaSphereCollisionObject internal API /////
        //////////////////////////////////////////////////

        /// Method called by @sa CreateObject method to really create the MechanicalObject and the sphere collision model on SOFA side
        protected override void Create_impl()
        {
            //m_sofaSphereCollision.CreateImpl(m_uniqueNameId, m_parentName, m_sofaContext, transform, enabled, m_isCreated);
            //Debug.LogWarning("$$$$$$$$$$$SofaSphereCollisionHand: Create_impl");
            //SofaLog("####### SofaSphereCollisionObject::Create_impl: " + UniqueNameId);

            // TODO: we remove this for the moment as it doesn't work anymore in SofaVerseAPI. We will use existing SofaComponents
            //if (m_sofaSphereCollision.Impl == null)
            //{
            //    m_sofaSphereCollision.Impl = new SofaCustomMeshAPI(m_sofaContext.GetSimuContext(), m_parentName, m_uniqueNameId);

            //    if (m_sofaSphereCollision.Impl == null || !m_sofaSphereCollision.Impl.m_isCreated)
            //    {
            //        SofaLog("SofaSphereCollisionObject:: Object creation failed: " + m_uniqueNameId, 2);
            //        this.enabled = false;
            //        return;
            //    }
            //    else
            //    {
            //        m_isCreated = true;
            //        foreach (Transform child in this.transform)
            //        {
            //            SofaMesh _mesh = child.gameObject.GetComponent<SofaMesh>();
            //            SofaCollisionModel _col = child.gameObject.GetComponent<SofaCollisionModel>();
            //            if (_mesh)
            //            {
            //                m_sofaSphereCollision.Impl.SetMeshNameID(_mesh.UniqueNameId);
            //            }
            //            else if (_col)
            //            {
            //                m_sofaSphereCollision.Impl.SetCollisionNameID(_col.UniqueNameId);
            //            }
            //        }
            //    }
            //}
            //else
            //    SofaLog("SofaSphereCollisionObject::Create_impl, SofaCustomMeshAPI already created: " + UniqueNameId, 1);



        }

        /// Method called by @sa Reconnect() method from SofaContext when scene is resctructed/reloaded.
        protected override void Reconnect_impl()
        {
            // nothing different.
            Create_impl();
        }

        /// <summary>
        /// Update list of Sphere position depending on the capsule collider  
        /// </summary>
        private void UpdatePoints()
        {
            if (m_capsuleColliderList.Count*2 != m_sofaSphereCollision.NbrSpheres)
                return;

            int j = 0;
            for (int i = 0; i < m_capsuleColliderList.Count; i++)
            {
                var col = m_capsuleColliderList[i].GetComponent<CapsuleCollider>();

                var direction = new Vector3 { [col.direction] = 1 };
                var offset = col.height / 2 - col.radius;

                var localPoint0 = col.center - direction * offset;
                var localPoint1 = col.center + direction * offset;

                var point0 = m_capsuleColliderList[i].transform.TransformPoint(localPoint0);
                var point1 = m_capsuleColliderList[i].transform.TransformPoint(localPoint1);

                // work in world coordinates for the moment, we will see if we need to change that
                m_sofaSphereCollision.Centers[j] = point0;// transform.InverseTransformPoint(point0);
                j++;
                m_sofaSphereCollision.Centers[j] = point1;// transform.InverseTransformPoint(point1);
                j++;
            }
        }

        /// <summary>
        /// Define the sphere position for the first iteration ; 
        /// The list is empty on start.
        /// </summary>
        /// <returns></returns>
        private Vector3[] DefinePoints()
        {
            for (int i = 0; i < m_capsuleColliderList.Count; i++)
            {
                var col = m_capsuleColliderList[i].GetComponent<CapsuleCollider>();

                var direction = new Vector3 { [col.direction] = 1 };
                var offset = col.height / 2 - col.radius;

                var localPoint0 = col.center - direction * offset;
                var localPoint1 = col.center + direction * offset;

                var point0 = m_capsuleColliderList[i].transform.TransformPoint(localPoint0);
                var point1 = m_capsuleColliderList[i].transform.TransformPoint(localPoint1);

                m_pointsList.Add(point0);
                m_pointsList.Add(point1);
            }

            Vector3[] pointListWorld = m_pointsList.ToArray();

            return pointListWorld;
        }

        /// Method called by @sa Awake() method. As post process method after creation.
        protected override void Init_impl()
        {
            //Debug.LogWarning("$$$$$$$$$$$SofaSphereCollisionHand: m_sofaContext: " + m_sofaContext.name);
            //Mesh m_mesh = this.GetComponent<MeshFilter>().sharedMesh;

            //if (m_mesh == null) // look for a mesh in the current gameObject
            //{
            //    Debug.LogError("SofaSphereCollisionObject::AwakePostProcess Error No valid Meshfilter found in current gameObject.");
            //    return;
            //}

            if (m_sofaMeshName.Length > 0)
            {
                SofaMesh[] meshes = GameObject.FindObjectsByType<SofaMesh>(FindObjectsSortMode.None);
                Debug.Log("Nbr Mesh: " + meshes.Length);
                foreach (SofaMesh mesh in meshes)
                {
                    if (mesh.UniqueNameId.Contains(m_sofaMeshName))
                        m_sofaMesh = mesh;
                }
            }

            if (m_sofaMesh == null)
            {
                Debug.LogError("m_sofaMesh is not set.");
                m_ready = false;
                return;
            }

            if (m_sphereModel == null)
            {
                Debug.LogError("m_sphereModel is not set.");
                m_ready = false;
                return;
            }

            // Link to existing Mesh and CollisionModel in Sofa scene
            m_sofaSphereCollision.LinkSofaSphereCollisionObject(m_sofaMesh, m_sphereModel);

            // First time define the list of center and will check if SOFA buffer is correctly allocated
            m_sofaSphereCollision.CreateSphereCenters(DefinePoints()); // store spheres center in world coordinates
            m_isCreated = true;
            m_ready = true;
        }

    }
}
