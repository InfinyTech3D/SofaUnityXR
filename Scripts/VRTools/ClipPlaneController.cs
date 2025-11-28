using UnityEngine;
using System.Collections.Generic;
using SofaUnityXR;

/// <summary>
/// This is meant to be used on a plane, you can then move that plane to cut object
/// Cutted Objects must have 
/// - the tag "targetTag" set on the editor 
/// _ A material with the custom shader URPLit_Clippable for URP pipline or SliceableClip_URP for base render pipline
/// </summary>
public class ClipPlaneController : MonoBehaviour
{
    [Header("Tag of objects to clip")]
    public string targetTag = "Sliceable";

    private List<Renderer> affectedObjects = new List<Renderer>();

    private static readonly int ClipPlanePos = Shader.PropertyToID("_ClipPlanePosition");
    private static readonly int ClipPlaneNormal = Shader.PropertyToID("_ClipPlaneNormal");
    private static readonly int ClippingEnabled = Shader.PropertyToID("_ClippingEnabled");

    [Header("SofaUnityXR only")]
    //Part use only if there a GamController script that will setup the sofa object for a cut 
    public bool AutoSofaObjectSetup;

    public GameController m_Gm;
    private bool m_launchSetup=true;
    private Vector3 _initialPosition;
    public Shader m_replacementShader;


    void Start()
    {
        _initialPosition = transform.position;  // Store start position of the plane
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;

        Renderer r = other.GetComponent<Renderer>();
        if (r != null && !affectedObjects.Contains(r))
        {
            affectedObjects.Add(r);
            EnableClipping(r, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;

        Renderer r = other.GetComponent<Renderer>();
        if (r != null && affectedObjects.Contains(r))
        {
            EnableClipping(r, false);
            affectedObjects.Remove(r);
        }
    }

    private void Update()
    {
        if (affectedObjects.Count == 0) return;

        Vector3 planePosition = transform.position;
        Vector3 planeNormal = transform.up;   //normal direction

        foreach (Renderer r in affectedObjects)
        {
            r.material.SetVector(ClipPlanePos, planePosition);
            r.material.SetVector(ClipPlaneNormal, planeNormal);
        }

        if (m_launchSetup)
        {
            //lunch this setup only once when you move the plane fo the first time
            if(AutoSofaObjectSetup && HasMovedFromStart())
            {
                SofaSetup();
                m_launchSetup = false;
            }
        }


    }

    private void EnableClipping(Renderer r, bool enabled)
    {
        r.material.SetInt(ClippingEnabled, enabled ? 1 : 0);
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
                }
            }
        }
    }
}
