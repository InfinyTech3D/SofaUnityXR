using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimator : MonoBehaviour
{
    [SerializeField] private InputActionProperty m_triggerAnimationAction;
    [SerializeField] private InputActionProperty m_gripAnimationAction;
    [SerializeField] private Animator m_handAnimator;

    private float m_triggerValue;
    private float m_gripValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_triggerValue = m_triggerAnimationAction.action.ReadValue<float>();
        m_handAnimator.SetFloat("Trigger", m_triggerValue);

        m_gripValue = m_gripAnimationAction.action.ReadValue<float>();
        m_handAnimator.SetFloat("Grip", m_gripValue);
    }
}

