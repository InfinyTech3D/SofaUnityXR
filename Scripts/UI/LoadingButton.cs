using UnityEngine;
using UnityEngine.UI;

namespace SofaUnityXR
{
    public class LoadingButton : MonoBehaviour
    {
        [Header("State")]
        public bool m_validate;

        [Header("Timing")]
        public float timeToValid = 2f;

        [Header("References")]
        public Slider loadingBar;

        private float currentTime;
        private bool isColliding;

        void Start()
        {
            m_validate = false;
            currentTime = 0f;

            if (loadingBar != null)
            {
                loadingBar.minValue = 0f;
                loadingBar.maxValue = timeToValid;
                loadingBar.value = 0f;
            }

        
        }

        void Update()
        {
            if (isColliding && !m_validate)
            {
                currentTime += Time.deltaTime;
                currentTime = Mathf.Clamp(currentTime, 0f, timeToValid);

                if (loadingBar != null)
                    loadingBar.value = currentTime;

                if (currentTime >= timeToValid)
                {
                    m_validate = true;
                    OnValidated();
                }
            } 
        
        }

    
        private void OnTriggerEnter(Collider other)
        {
            if (other != null)
            {
                isColliding = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null)
            {
                if(!m_validate)
                    ResetButton();
            }
        }

        void ResetButton()
        {
            isColliding = false;
            currentTime = 0f;

            if (loadingBar != null)
                loadingBar.value = 0f;
        }

        void OnValidated()
        {
            Debug.Log("Button validated!");
            // Optional: disable further interaction
        
        }
    }

}