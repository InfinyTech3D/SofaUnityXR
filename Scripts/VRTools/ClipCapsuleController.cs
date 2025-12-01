using UnityEngine;
using System.Collections.Generic;
using SofaUnityXR;


public class ClipCapsuleController : MonoBehaviour
{
    public CapsuleCollider capsule;
    public Renderer[] renderers; // all objects to clip

    void Update()
    {
        if (capsule == null) return;

        Vector3 pointA = capsule.transform.position + capsule.transform.up * capsule.height * 0.5f - capsule.transform.up * capsule.radius;
        Vector3 pointB = capsule.transform.position - capsule.transform.up * capsule.height * 0.5f + capsule.transform.up * capsule.radius;

        float radius = capsule.radius * Mathf.Max(
            capsule.transform.lossyScale.x,
            capsule.transform.lossyScale.y,
            capsule.transform.lossyScale.z);

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
