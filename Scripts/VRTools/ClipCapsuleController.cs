using UnityEngine;
using System.Collections.Generic;
using SofaUnityXR;

/// <summary>
/// Class used to reveal objects through a special clipping shader.
/// This component must be used with a Sphere or Capsule collider.
///
/// Linked shaders: URP_CapsuleClip and CapsuleLitForward.hlsl  
/// Both shaders are based on the URP Lit shader and require that the project
/// is using the Universal Render Pipeline (URP) with the appropriate URP package installed.
/// </summary>
public class ClipCapsuleController : MonoBehaviour
{
    public CapsuleCollider capsule;
    //Recommanded to bo URP_CapsuleClip
    public Shader m_replacementShader;
    [Header("Use to switch between regular mode and sofa mode")]
    public bool AutoSofaObjectSetup;
    [Header("Manual Setup")]
    public Renderer[] renderers; // all objects to clip
    [Header("SofaUnity Automatic Setup")]
    //Part use only if there a GamController script that will setup the sofa object
    public GameController m_Gm;
    private bool m_launchSetup = true;
    private Vector3 _initialPosition;


    void Start()
    {
        _initialPosition = transform.position;  // Store start position of the plane
    }

    void Update()
    {
        if (capsule == null) return;

        Vector3 pointA = capsule.transform.position + capsule.transform.up * capsule.height * 0.5f - capsule.transform.up * capsule.radius;
        Vector3 pointB = capsule.transform.position - capsule.transform.up * capsule.height * 0.5f + capsule.transform.up * capsule.radius;

        float radius = capsule.radius * Mathf.Max(
            capsule.transform.lossyScale.x,
            capsule.transform.lossyScale.y,
            capsule.transform.lossyScale.z);

        // Automatic setup of the sofa objects 
        if (AutoSofaObjectSetup)
        {
            if (m_launchSetup)
            {
                //lunch this setup only once when you move the sphere fo the first time
                if (HasMovedFromStart())
                {
                    SofaSetup();
                    m_launchSetup = false;
                }
            }
            else
            {
                foreach (SofaModelElementExplorer elm in m_Gm.ModelExplorer.m_modelElementCtrls)
                {
                    Renderer rend = elm.m_targetElement.GetComponent<Renderer>();
                    foreach (Material m in rend.materials)
                    {
                        m.SetVector("_CapsulePointA", pointA);
                        m.SetVector("_CapsulePointB", pointB);
                        m.SetFloat("_CapsuleRadius", radius);
                    }
                }
               
            }
        }else//Manual setup
        {
            foreach (Renderer r in renderers)
            {
                foreach (Material m in r.materials)
                {
                    m.SetVector("_CapsulePointA", pointA);
                    m.SetVector("_CapsulePointB", pointB);
                    m.SetFloat("_CapsuleRadius", radius);
                }
            }
        }

        
    }

    private bool HasMovedFromStart()
    {

        const float threshold = 0.001f;
        return Vector3.Distance(transform.position, _initialPosition) > threshold;
    }

    private void SofaSetup()
    {
        foreach (SofaModelElementExplorer elm in m_Gm.ModelExplorer.m_modelElementCtrls)
        {
            GameObject obj = elm.m_targetElement;

            if (obj == null)
                continue;

            // Get any renderer on the object
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend == null)
                rend = obj.GetComponentInChildren<Renderer>();

            // If a renderer is found AND a shader is assigned
            if (rend != null && m_replacementShader != null)
            {
                // Apply the new shader to all materials
                foreach (var mat in rend.materials)
                {
                    mat.shader = m_replacementShader;
                    
                    // Use to render both faces
                    if (mat.HasProperty("_Cull"))
                        mat.SetFloat("_Cull", 0f);  // 0 = Off, 1 = Front, 2 = Back

                    //  "CullMode" not use on our case but you never known 
                    if (mat.HasProperty("_CullMode"))
                        mat.SetFloat("_CullMode", 0f);

                }
            }
        }
    }
}
