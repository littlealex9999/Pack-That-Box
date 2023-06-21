using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(XRRayInteractor))]
public class XRInteractorLineRenderer : MonoBehaviour
{
    LineRenderer lineRenderer;
    XRRayInteractor rayInteractor;

    List<Transform> targets = new List<Transform>();

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        rayInteractor = GetComponent<XRRayInteractor>();
    }

    private void Update()
    {
        if (targets.Count > 0) {
            if (!lineRenderer.enabled) lineRenderer.enabled = true;

            lineRenderer.SetPosition(0, rayInteractor.transform.position);
            lineRenderer.SetPosition(1, ChooseTarget().position);
        } else if (lineRenderer.enabled) {
            lineRenderer.enabled = false;
        }
    }

    public void SetTarget(HoverEnterEventArgs args)
    {
        targets.Add(args.interactableObject.transform);
    }

    public void SetTarget(HoverExitEventArgs args)
    {
        targets.Remove(args.interactableObject.transform);
    }

    Transform ChooseTarget()
    {
        if (targets.Count <= 0) return null;

        float closest = (targets[0].position - rayInteractor.rayOriginTransform.position).sqrMagnitude;
        Transform target = targets[0];

        for (int i = 1; i < targets.Count; ++i) {
            float distance = (targets[i].position - rayInteractor.rayOriginTransform.position).sqrMagnitude;

            if (distance < closest) {
                closest = distance;
                target = targets[i];
            }
        }

        return target;
    }
}
