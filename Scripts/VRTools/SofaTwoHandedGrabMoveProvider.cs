using UnityEngine;
using UnityEngine.UI;
using SofaUnityXR;
using UnityEngine.XR.Interaction.Toolkit;

namespace SofaUnityXR
{ 
    public class SofaTwoHandedGrabMoveProvider : ConstrainedMoveProvider
    {
        public GameObject m_camera = null;
        public GameObject m_controllerA = null;
        public GameObject m_controllerB = null;

        private Transform m_objectToMove = null;
        private Vector3 m_leftControllerInitPosition;
        private Vector3 m_rightControllerInitPosition;
        private Vector3 m_scaleInitObject;
        private Vector3 restControllerA ;
        private Vector3 restControllerB ;
        private LineRenderer LineBetween;// ray between two hands
        [SerializeField] private float m_scaleResize = 1.0f;
        [SerializeField] private SofaModelExplorer m_modelExplorer;
        //[SerializeField] private Slider m_slider = null;


        public Transform objectToMove
        {
            get => m_objectToMove;
            set => m_objectToMove = value;
        }

        [SerializeField]
        [Tooltip("The left hand grab move instance which will be used as one half of two-handed locomotion.")]
        GrabMoveProvider m_LeftGrabMoveProvider;
        /// <summary>
        /// The left hand grab move instance which will be used as one half of two-handed locomotion.
        /// </summary>
        public GrabMoveProvider leftGrabMoveProvider
        {
            get => m_LeftGrabMoveProvider;
            set => m_LeftGrabMoveProvider = value;
        }

        [SerializeField]
        [Tooltip("The right hand grab move instance which will be used as one half of two-handed locomotion.")]
        GrabMoveProvider m_RightGrabMoveProvider;
        /// <summary>
        /// The right hand grab move instance which will be used as one half of two-handed locomotion.
        /// </summary>
        public GrabMoveProvider rightGrabMoveProvider
        {
            get => m_RightGrabMoveProvider;
            set => m_RightGrabMoveProvider = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to override the settings for individual handed providers with this provider's settings on initialization.")]
        bool m_OverrideSharedSettingsOnInit = true;
        /// <summary>
        /// Controls whether to override the settings for individual handed providers with this provider's settings on initialization.
        /// </summary>
        public bool overrideSharedSettingsOnInit
        {
            get => m_OverrideSharedSettingsOnInit;
            set => m_OverrideSharedSettingsOnInit = value;
        }

        [SerializeField]
        [Tooltip("The ratio of actual movement distance to controller movement distance.")]
        float m_MoveFactor = 1f;
        /// <summary>
        /// The ratio of actual movement distance to controller movement distance.
        /// </summary>
        public float moveFactor
        {
            get => m_MoveFactor;
            set => m_MoveFactor = value;
        }

        [SerializeField]
        [Tooltip("Controls whether translation requires both grab move inputs to be active.")]
        bool m_RequireTwoHandsForTranslation;
        /// <summary>
        /// Controls whether translation requires both grab move inputs to be active.
        /// </summary>
        public bool requireTwoHandsForTranslation
        {
            get => m_RequireTwoHandsForTranslation;
            set => m_RequireTwoHandsForTranslation = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to enable yaw rotation of the user.")]
        bool m_EnableRotation = true;
        /// <summary>
        /// Controls whether to enable yaw rotation of the user.
        /// </summary>
        public bool enableRotation
        {
            get => m_EnableRotation;
            set => m_EnableRotation = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to enable uniform scaling of the user.")]
        bool m_EnableScaling;
        /// <summary>
        /// Controls whether to enable uniform scaling of the user.
        /// </summary>
        public bool enableScaling
        {
            get => m_EnableScaling;
            set => m_EnableScaling = value;
        }

        [SerializeField]
        [Tooltip("The minimum user scale allowed.")]
        float m_MinimumScale = 0.2f;
        /// <summary>
        /// The minimum user scale allowed.
        /// </summary>
        public float minimumScale
        {
            get => m_MinimumScale;
            set => m_MinimumScale = value;
        }

        [SerializeField]
        [Tooltip("The maximum user scale allowed.")]
        float m_MaximumScale = 100f;
        /// <summary>
        /// The maximum user scale allowed.
        /// </summary>
        public float maximumScale
        {
            get => m_MaximumScale;
            set => m_MaximumScale = value;
        }

        bool m_IsMoving;

        Vector3 m_PreviousMidpointBetweenControllers;

        float m_InitialOriginYaw;
        Vector3 m_InitialLeftToRightDirection;
        Vector3 m_InitialLeftToRightOrthogonal;

        float m_InitialOriginScale;
        float m_InitialDistanceBetweenHands;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {

            LineBetween= m_controllerA.GetComponent<LineRenderer>();
            if (m_LeftGrabMoveProvider == null || m_RightGrabMoveProvider == null)
            {
                Debug.LogError("Left or Right Grab Move Provider is not set or has been destroyed.", this);
                enabled = false;
                return;
            }

            if (m_RequireTwoHandsForTranslation)
            {
                m_LeftGrabMoveProvider.canMove = false;
                m_RightGrabMoveProvider.canMove = false;
            }

            if (m_OverrideSharedSettingsOnInit)
            {
                m_LeftGrabMoveProvider.system = system;
                m_LeftGrabMoveProvider.enableFreeXMovement = enableFreeXMovement;
                m_LeftGrabMoveProvider.enableFreeYMovement = enableFreeYMovement;
                m_LeftGrabMoveProvider.enableFreeZMovement = enableFreeZMovement;
                m_LeftGrabMoveProvider.useGravity = useGravity;
                m_LeftGrabMoveProvider.gravityMode = gravityMode;
                m_LeftGrabMoveProvider.moveFactor = m_MoveFactor;
                m_RightGrabMoveProvider.system = system;
                m_RightGrabMoveProvider.enableFreeXMovement = enableFreeXMovement;
                m_RightGrabMoveProvider.enableFreeYMovement = enableFreeYMovement;
                m_RightGrabMoveProvider.enableFreeZMovement = enableFreeZMovement;
                m_RightGrabMoveProvider.useGravity = useGravity;
                m_RightGrabMoveProvider.gravityMode = gravityMode;
                m_RightGrabMoveProvider.moveFactor = m_MoveFactor;
            }

            beginLocomotion += OnBeginLocomotion;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            if (m_LeftGrabMoveProvider != null)
                m_LeftGrabMoveProvider.canMove = true;
            if (m_RightGrabMoveProvider != null)
                m_RightGrabMoveProvider.canMove = true;

            beginLocomotion -= OnBeginLocomotion;
        }

        

        /// <inheritdoc/>
        protected override Vector3 ComputeDesiredMove(out bool attemptingMove)
        {
            
            attemptingMove = false;
            var wasMoving = m_IsMoving;
            var xrOrigin = system.xrOrigin;
            m_IsMoving = m_LeftGrabMoveProvider.IsGrabbing() && m_RightGrabMoveProvider.IsGrabbing() && xrOrigin != null;//not grab but trigger button now 
            if (LineBetween != null)
            {
                // Mettre à jour les positions des extrémités du LineRenderer
                LineBetween.positionCount = 2; // La ligne a 2 points
                LineBetween.SetPosition(0, m_controllerA.transform.position); // Point de départ (objet 1)
                LineBetween.SetPosition(1, m_controllerB.transform.position); // Point de fin (objet 2)
            }

            if (!m_IsMoving)
            {
                LineBetween.enabled = false;
                // Enable one-handed movement
                if (!m_RequireTwoHandsForTranslation)
                {
                    m_LeftGrabMoveProvider.canMove = true;
                    m_RightGrabMoveProvider.canMove = true;
                }
                restControllerA = m_controllerA.transform.position;
                restControllerB = m_controllerB.transform.position;
                return Vector3.zero;
            }

            LineBetween.enabled = true;
            RotateModel();
            // Prevent individual grab locomotion since we perform our own translation
            m_LeftGrabMoveProvider.canMove = false;
            m_RightGrabMoveProvider.canMove = false;

            var originTransform = xrOrigin.transform;
            var leftHandLocalPosition = m_LeftGrabMoveProvider.controllerTransform.localPosition;



            var rightHandLocalPosition = m_RightGrabMoveProvider.controllerTransform.localPosition;
            var midpointLocalPosition = (leftHandLocalPosition + rightHandLocalPosition) * 0.5f;
            if (m_modelExplorer.m_targetElement == null)
            {
                m_objectToMove = m_modelExplorer.m_sofaContext.transform;
            }
            else
            {
                m_objectToMove = m_modelExplorer.m_targetElement.m_targetElement.transform;
            }

            if (!wasMoving && m_IsMoving && m_objectToMove != null ) // Cannot simply check locomotionPhase because it might always be in moving state, due to gravity application mode
            {
                m_leftControllerInitPosition = m_LeftGrabMoveProvider.controllerTransform.position;
                m_rightControllerInitPosition = m_RightGrabMoveProvider.controllerTransform.position;
                m_scaleInitObject = m_objectToMove.transform.localScale;

                //When pressing both grip button the first time 
                //restControllerA = m_controllerA.transform.position;
                //restControllerB = m_controllerB.transform.position;

                m_InitialOriginYaw = originTransform.eulerAngles.y;
                m_InitialLeftToRightDirection = rightHandLocalPosition - leftHandLocalPosition;
                m_InitialLeftToRightDirection.y = 0f; // Only use yaw rotation
                m_InitialLeftToRightOrthogonal = Quaternion.AngleAxis(90f, Vector3.down) * m_InitialLeftToRightDirection;

                m_InitialOriginScale = originTransform.localScale.x;
                m_InitialDistanceBetweenHands = Vector3.Distance(leftHandLocalPosition, rightHandLocalPosition);

                // Do not move the first frame of grab
                m_PreviousMidpointBetweenControllers = midpointLocalPosition;
                

                return Vector3.zero;
            }
            
            attemptingMove = true;
 
            var move = originTransform.TransformVector(m_PreviousMidpointBetweenControllers - midpointLocalPosition) * m_MoveFactor;
            m_PreviousMidpointBetweenControllers = midpointLocalPosition;
            //return move;
            
            return Vector3.zero;
        }

        /// <summary>
        /// Allow to rotate the model selected 
        /// </summary>
        Quaternion customRot;
        private void RotateModel()
        {

            
            if (m_modelExplorer.m_targetElement == null)
            {
                m_objectToMove = m_modelExplorer.m_sofaContext.transform;
               
            }
            else
            {
                m_objectToMove = m_modelExplorer.m_targetElement.m_targetElement.transform;
            }

            Vector3 oldAB = restControllerB - restControllerA;
            Vector3 newAB = m_controllerB.transform.position - m_controllerA.transform.position;
            var restControllerUp = m_controllerA.transform.up + m_controllerB.transform.up;
            Vector3 oldZ = restControllerUp.normalized;
            Vector3 newZ = m_controllerA.transform.up + m_controllerB.transform.up;
            newZ.Normalize();

            Quaternion tmpRot = Quaternion.FromToRotation(oldAB, newAB);
            tmpRot = tmpRot * Quaternion.FromToRotation(oldZ, newZ);

            float val = Vector3.Dot(m_camera.transform.forward,  m_objectToMove.transform.forward);
            if (val < 0)
            {
                float xR = -tmpRot.eulerAngles.x;
                float yR = tmpRot.eulerAngles.y;
                float zR = -tmpRot.eulerAngles.z;
                tmpRot.eulerAngles = new Vector3(xR, yR, zR);
            }

            /* rotation
            if (m_fixMode)
                customRot = tmpRot;
            else
                customRot = customRot * tmpRot;*/

             m_objectToMove.transform.eulerAngles =  m_objectToMove.transform.eulerAngles + tmpRot.eulerAngles;//customRot.eulerAngles;
             restControllerA = m_controllerA.transform.position;
             restControllerB = m_controllerB.transform.position;
        }


        void OnBeginLocomotion(LocomotionSystem otherSystem)
        {
            
            //var leftHandLocalPosition = m_LeftGrabMoveProvider.controllerTransform.localPosition;
            //var rightHandLocalPosition = m_RightGrabMoveProvider.controllerTransform.localPosition;
            var leftHandNewPosition = m_LeftGrabMoveProvider.controllerTransform.position;
            var rightHandNewPosition = m_RightGrabMoveProvider.controllerTransform.position;

            //if (m_EnableRotation)
            //{
            //    var leftToRightDirection = rightHandLocalPosition - leftHandLocalPosition;
            //    leftToRightDirection.y = 0f; // Only use yaw rotation
            //    var yawSign = Mathf.Sign(Vector3.Dot(m_InitialLeftToRightOrthogonal, leftToRightDirection));
            //    var targetYaw = m_InitialOriginYaw + Vector3.Angle(m_InitialLeftToRightDirection, leftToRightDirection) * yawSign;
            //    m_objectToMove.rotation = Quaternion.AngleAxis(targetYaw, Vector3.up);
            //}
            if (m_modelExplorer.m_targetElement == null)
            {
                m_objectToMove = m_modelExplorer.m_sofaContext.transform;
            }   
            else
            {
                 m_objectToMove = m_modelExplorer.m_targetElement.m_targetElement.transform;
            }
                
            if (m_EnableScaling && m_objectToMove != null)
            {
                Vector3 diffLeftHand = leftHandNewPosition - m_leftControllerInitPosition;
                Vector3 diffRghtHand = rightHandNewPosition - m_rightControllerInitPosition;

                float normLeft = diffLeftHand.magnitude;
                float normRight = diffRghtHand.magnitude;

                if (normLeft < 0.01f && normRight < 0.01f)
                    return;

                //print("normLeft = " + normLeft);
                //print("normRight = " + normRight);

                Vector3 oldLeftRight = m_rightControllerInitPosition - m_leftControllerInitPosition;



                Vector3 newLeftRight = rightHandNewPosition - leftHandNewPosition;
                float oldNormLeftRight = oldLeftRight.magnitude;
                float newNormLeftRight = newLeftRight.magnitude;

                ////translate center
                //if (normLeft > 0.01f)
                //    m_objectToMove.position += diffLeftHand;

                //scale
                float ratio = 1.0f;
                if (oldNormLeftRight > 0.1f)
                    ratio = newNormLeftRight / oldNormLeftRight;
                if (ratio < 0.001f)
                    return;
                m_objectToMove.transform.localScale = m_scaleInitObject * ratio * m_scaleResize;
                //print("ratio = " + ratio);
                //print("objectScale = " + m_objectToMove.localScale);
                //print("ObjectPosition = " + m_objectToMove.position);

                ////rotation
                //Quaternion rot = Quaternion.FromToRotation(oldLeftRight, newLeftRight);
                //Vector3 rotationWithModulo;

                //rotationWithModulo.x = rot.eulerAngles.x % 360f;
                //rotationWithModulo.y = rot.eulerAngles.y % 360f;
                //rotationWithModulo.z = rot.eulerAngles.z % 360f;

                ////print("objectPositionBefore = " + m_objectToMove.localEulerAngles);
                ////print("rotationValue = " + rot.eulerAngles);
                ////m_objectToMove.localEulerAngles = m_objectToMove.localEulerAngles + rot.eulerAngles;
                ////print("objectPositionAfter = " + m_objectToMove.localEulerAngles);

                //print("objectPositionBefore = " + m_objectToMove.localEulerAngles);
                //print("rotationValue = " + rotationWithModulo);
                //m_objectToMove.localEulerAngles = m_objectToMove.localEulerAngles + rotationWithModulo;
                //print("objectPositionAfter = " + m_objectToMove.localEulerAngles);

                ////update init position 
                //if (normLeft > 0.01f)
                //    m_leftControllerInitPosition = leftHandNewPosition;
                //if (normRight > 0.01f)
                //    m_rightControllerInitPosition = rightHandNewPosition;



                //var distanceBetweenHands = Vector3.Distance(leftHandLocalPosition, rightHandLocalPosition);
                //var targetScale = distanceBetweenHands != 0f
                //    ? /*m_InitialOriginScale*/m_objectToMove.localScale.x * (m_InitialDistanceBetweenHands / distanceBetweenHands)
                //    : m_objectToMove.localScale.x;

                //targetScale = Mathf.Clamp(targetScale, m_MinimumScale, m_MaximumScale);
                //m_objectToMove.localScale = Vector3.one * targetScale;

            }
        }
    }
}
