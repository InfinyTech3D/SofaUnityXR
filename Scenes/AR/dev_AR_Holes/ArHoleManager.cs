using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using SofaUnity;

public class ArHoleManager : MonoBehaviour
{
    public GameObject HoleObj;
    public GameObject ActivateObj;
    public GameObject m_SofaContext;

    private bool isDisplay;
    private bool startonce;
    private Vector3 baseScale;
    private Vector3 activateStartPos;
    private Coroutine animRoutine;
    private SofaContext context;

    void Start()
    {
        baseScale = HoleObj.transform.localScale;
        activateStartPos = ActivateObj.transform.position;
        HoleObj.transform.localScale = Vector3.zero;
        isDisplay = true;
        startonce = true;
        context = m_SofaContext.GetComponent<SofaContext>();
        
    }

    void Update()
    {
        if (startonce)// only once
        {
            if (context.IsSofaUpdating)//sofa is on
            {
                if (ActivateObj.transform.position != activateStartPos )// as moved compared to the precedent frame
                {
                    AnimationHole();

                }
            }
                
        }
        

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            AnimationHole();
        }

        activateStartPos = ActivateObj.transform.position;
    }

    public void AnimationHole()
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(ScaleRoutine(isDisplay ? baseScale : Vector3.zero,
                                                  isDisplay ? Vector3.zero : baseScale));

        isDisplay = !isDisplay;
    }

    IEnumerator ScaleRoutine(Vector3 from, Vector3 to)
    {
        float t = 0f;
        float duration = 0.5f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            HoleObj.transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        HoleObj.transform.localScale = to;
    }
}
