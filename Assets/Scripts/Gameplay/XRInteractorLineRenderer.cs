using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(XRRayInteractor))]
public class XRInteractorLineRenderer : MonoBehaviour
{
    LineRenderer lineRenderer;
    XRRayInteractor rayInteractor;

    Transform target;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        rayInteractor = GetComponent<XRRayInteractor>();
    }

    private void Update()
    {
        if (target != null) {
            if (!lineRenderer.enabled) lineRenderer.enabled = true;

            lineRenderer.SetPosition(0, rayInteractor.transform.position);
            lineRenderer.SetPosition(1, target.position);
        } else if (lineRenderer.enabled) {
            lineRenderer.enabled = false;
        }
    }

    public void SetTarget(HoverEnterEventArgs args)
    {
        target = args.interactableObject.transform;
    }

    public void SetTarget(HoverExitEventArgs args)
    {
        target = null;
    }
}
