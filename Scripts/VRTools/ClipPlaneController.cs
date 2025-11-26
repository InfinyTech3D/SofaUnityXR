using UnityEngine;
using System.Collections.Generic;

public class ClipPlaneController : MonoBehaviour
{
    [Header("Tag of objects to clip")]
    public string targetTag = "Sliceable";

    private List<Renderer> affectedObjects = new List<Renderer>();

    private static readonly int ClipPlanePos = Shader.PropertyToID("_ClipPlanePosition");
    private static readonly int ClipPlaneNormal = Shader.PropertyToID("_ClipPlaneNormal");
    private static readonly int ClippingEnabled = Shader.PropertyToID("_ClippingEnabled");

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
        Vector3 planeNormal = transform.up;   // your plane's normal direction

        foreach (Renderer r in affectedObjects)
        {
            r.material.SetVector(ClipPlanePos, planePosition);
            r.material.SetVector(ClipPlaneNormal, planeNormal);
        }
    }

    private void EnableClipping(Renderer r, bool enabled)
    {
        r.material.SetInt(ClippingEnabled, enabled ? 1 : 0);
    }
}
