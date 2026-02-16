using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CutoutObjectVR : MonoBehaviour
{
    [SerializeField]
    private Transform targetObject;

    [SerializeField]
    private LayerMask wallMask;

    private Camera mainCamera;

    [Header("VR Settings")]
    [SerializeField]
    private bool useVRCorrection = true;

    [SerializeField]
    [Tooltip("Distance entre les yeux en mètres (IPD). Typiquement 0.063m")]
    private float interPupillaryDistance = 0.063f;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (targetObject == null) return;

        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 offset = targetObject.position - cameraPos;

        RaycastHit[] hitObjects = Physics.RaycastAll(cameraPos, offset.normalized, offset.magnitude, wallMask);

        // Vérifier si on est en VR
        bool isInVR = XRSettings.enabled;

        for (int i = 0; i < hitObjects.Length; ++i)
        {
            Renderer hitRenderer = hitObjects[i].transform.GetComponent<Renderer>();
            if (hitRenderer == null) continue;

            Vector2 cutoutPos;

            if (isInVR && useVRCorrection)
            {
                // Calculer la position du point milieu entre les deux yeux
                // La caméra Unity en VR est généralement au centre
                Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetObject.position);

                if (viewportPos.z < 0) continue;

                // Ne PAS diviser par aspect ratio en VR
                cutoutPos = new Vector2(viewportPos.x, viewportPos.y);
            }
            else
            {
                // Mode non-VR (code original)
                Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetObject.position);

                if (viewportPos.z < 0) continue;

                cutoutPos = new Vector2(viewportPos.x, viewportPos.y);
                cutoutPos.y /= (Screen.width / Screen.height);
            }

            Material[] materials = hitRenderer.materials;

            for (int m = 0; m < materials.Length; ++m)
            {
                materials[m].SetVector("_CutoutPos", cutoutPos);
                materials[m].SetFloat("_CutoutSize", 0.1f);
                materials[m].SetFloat("_FalloffSize", 0.05f);
            }
        }
    }
}