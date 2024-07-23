using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;

namespace SofaUnityXR
{
    class ButtonLogUI : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI m_text = null;

        protected string m_shortedLog;
        protected string m_fullLog;
        protected TextMeshProUGUI m_targetElement;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public string ShortedLog
        {
            get => m_shortedLog;
            set => m_shortedLog = value;
        }

        public string FullLog
        {
            get => m_fullLog;
            set => m_fullLog = value;
        }

        public TextMeshProUGUI Text
        {
            get => m_text;
            set => m_text = value;
        }

    }
}
