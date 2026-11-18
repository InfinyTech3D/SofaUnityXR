
using UnityEngine;
using SofaUnityXR;

public class ClipCapsuleController : MonoBehaviour
{
    [Header("Capsule")]
    public CapsuleCollider capsule;

    [Header("Clipping Settings")]
    [Range(0.05f, 2f)]
    public float clipSizeMultiplier = 1f;

    [Header("Shader")]
    public Shader m_replacementShader;

    [Header("Use to switch between regular mode and sofa mode")]
    public bool AutoSofaObjectSetup;

    [Header("Manual Setup")]
    public Renderer[] renderers;

    [Header("SofaUnity Automatic Setup")]
    public GameController m_Gm;

    private bool m_launchSetup = true;
    private Vector3 _initialPosition;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int PointAId =
        Shader.PropertyToID("_CapsulePointA");

    private static readonly int PointBId =
        Shader.PropertyToID("_CapsulePointB");

    private static readonly int RadiusId =
        Shader.PropertyToID("_CapsuleRadius");

    private void Start()
    {
        if (capsule == null)
            capsule = GetComponent<CapsuleCollider>();

        _initialPosition = transform.position;
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (capsule == null)
            return;

        if (AutoSofaObjectSetup && m_launchSetup)
        {
            if (!HasMovedFromStart())
                return;

            if (!SofaSetup())
                return;

            m_launchSetup = false;
        }

        Transform t = capsule.transform;
        Vector3 scale = t.lossyScale;

        float sx = Mathf.Abs(scale.x);
        float sy = Mathf.Abs(scale.y);
        float sz = Mathf.Abs(scale.z);

        Vector3 localAxis;
        float axisScale;
        float radialScale;

        switch (capsule.direction)
        {
            case 0: // X axis
                localAxis = Vector3.right;
                axisScale = sx;
                radialScale = Mathf.Max(sy, sz);
                break;

            case 2: // Z axis
                localAxis = Vector3.forward;
                axisScale = sz;
                radialScale = Mathf.Max(sx, sy);
                break;

            default: // Y axis
                localAxis = Vector3.up;
                axisScale = sy;
                radialScale = Mathf.Max(sx, sz);
                break;
        }

        Vector3 center = t.TransformPoint(capsule.center);
        Vector3 axis = t.TransformDirection(localAxis).normalized;

        float worldRadius = capsule.radius * radialScale;
        float worldHeight = capsule.height * axisScale;

        float halfSegment = Mathf.Max(
            0f,
            worldHeight * 0.5f - worldRadius
        );

        float multiplier = Mathf.Max(
            0.001f,
            clipSizeMultiplier
        );

        float radius = worldRadius * multiplier;
        halfSegment *= multiplier;

        Vector3 pointA = center + axis * halfSegment;
        Vector3 pointB = center - axis * halfSegment;

        if (AutoSofaObjectSetup)
        {
            if (m_Gm == null || m_Gm.ModelExplorer == null)
                return;

            foreach (SofaModelElementExplorer elm
                in m_Gm.ModelExplorer.m_modelElementCtrls)
            {
                if (elm == null || elm.m_targetElement == null)
                    continue;

                Renderer[] targetRenderers =
                    elm.m_targetElement
                        .GetComponentsInChildren<Renderer>(true);

                foreach (Renderer r in targetRenderers)
                    ApplyClipping(r, pointA, pointB, radius);
            }
        }
        else
        {
            if (renderers == null)
                return;

            foreach (Renderer r in renderers)
                ApplyClipping(r, pointA, pointB, radius);
        }
    }

    private void ApplyClipping(
        Renderer r,
        Vector3 pointA,
        Vector3 pointB,
        float radius)
    {
        if (r == null)
            return;

        r.GetPropertyBlock(propertyBlock);

        propertyBlock.SetVector(PointAId, pointA);
        propertyBlock.SetVector(PointBId, pointB);
        propertyBlock.SetFloat(RadiusId, radius);

        r.SetPropertyBlock(propertyBlock);
    }

    private bool HasMovedFromStart()
    {
        const float threshold = 0.001f;

        return Vector3.Distance(
            transform.position,
            _initialPosition
        ) > threshold;
    }

    private bool SofaSetup()
    {
        if (m_Gm == null ||
            m_Gm.ModelExplorer == null ||
            m_replacementShader == null)
            return false;

        foreach (SofaModelElementExplorer elm
            in m_Gm.ModelExplorer.m_modelElementCtrls)
        {
            if (elm == null || elm.m_targetElement == null)
                continue;

            Renderer[] targetRenderers =
                elm.m_targetElement
                    .GetComponentsInChildren<Renderer>(true);

            foreach (Renderer rend in targetRenderers)
            {
                foreach (Material mat in rend.materials)
                {
                    mat.shader = m_replacementShader;

                    if (mat.HasProperty("_Cull"))
                        mat.SetFloat("_Cull", 0f);

                    if (mat.HasProperty("_CullMode"))
                        mat.SetFloat("_CullMode", 0f);
                }
            }
        }

        return true;
    }
}
