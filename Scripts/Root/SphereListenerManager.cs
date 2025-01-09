using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// used to define the sphere comportment when hovering for the selection
/// </summary>
public class SphereListenerManager : MonoBehaviour
{
    // referent to method to add sphere instance to the list whent crossed by ray 
    public static event Action<GameObject> OnHoverEnterLeft; 
    public static event Action<GameObject> OnHoverEnterRight;

    // referent to method to delete sphere instance to the list whent crossed by ray 
    public static event Action<GameObject> OnHoverExitLeft; 
    public static event Action<GameObject> OnHoverExitRight;

    // state of a given sphere (selected when press trigger button on hover sphere)
    private bool m_isSelected;

    // save the color in case chang it in the sphere mode 
    private Color m_defaultColor;

    private void Start()
    {
        // default color if no random colr generated (in sphere manager) used when release sphere 
        m_defaultColor = Color.white;

        //when created sphere are not selected
        m_isSelected = false;
    }

    /// <summary>
    /// when begin hovering sphere with left ray interactor add to list gameObject crossed by ray interactor
    /// </summary>
    /// <param name="arg0"></param>
    public void OnHoverEnterLeftTrigger(HoverEnterEventArgs arg0)
    {
        if (arg0.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            if (controllerInteractor.gameObject.tag == "LeftRayInteractor")
            {
                OnHoverEnterLeft?.Invoke(gameObject);
            }
        }
    }

    /// <summary>
    /// when begin hovering sphere with right ray interactor add to list gameObject crossed by ray interactor
    /// </summary>
    /// <param name="arg0"></param>
    public void OnHoverEnterRightTrigger(HoverEnterEventArgs arg0)
    {
        if (arg0.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            if (controllerInteractor.gameObject.tag == "RightRayInteractor")
            {
                OnHoverEnterRight?.Invoke(gameObject);
            }
        }
    }

    /// <summary>
    /// when ending hovering sphere with left ray interactor delete to list gameObject crossed by ray interactor
    /// </summary>
    /// <param name="arg0"></param>
    public void OnHoverExitLeftTrigger(HoverExitEventArgs arg0)
    {
        if (arg0.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            if (controllerInteractor.gameObject.tag == "LeftRayInteractor")
            {
                OnHoverExitLeft?.Invoke(gameObject);
            }
        }
    }

    /// <summary>
    /// when ending hovering sphere with right ray interactor delete to list gameObject crossed by ray interactor
    /// </summary>
    /// <param name="arg0"></param>
    public void OnHoverExitRightTrigger(HoverExitEventArgs arg0)
    {
        if (arg0.interactorObject is XRBaseControllerInteractor controllerInteractor )
        {
            if (controllerInteractor.gameObject.tag == "RightRayInteractor")
            {
                OnHoverExitRight?.Invoke(gameObject);
            }
        }
    }

    /// <summary>
    /// To change the state of selected sphere 
    /// </summary>
    public bool IsSelected
    {
        get => m_isSelected;
        set => m_isSelected = value;
    }

    /// <summary>
    /// To reuse the init color when releasing selected
    /// </summary>
    public Color DefaultColor
    {
        get => m_defaultColor;
        set => m_defaultColor = value;
    }
}
