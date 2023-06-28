using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(LineRenderer))]
public class XRInteractorLineRenderer : MonoBehaviour
{
    public XRDirectInteractor directInteractor;
    public XRRayInteractor raycastInteractor;
    public XRRayInteractor raysphereInteractor;
    float biggestDistance;

    [Space] public bool drawLineWhenNoPickup = true;

    public Gradient normalColors;
    public Gradient hoverObjectColors;

    LineRenderer lineRenderer;

    bool hoveringObjects;
    List<IXRHoverInteractable> hoverTargets;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        biggestDistance = raycastInteractor.maxRaycastDistance > raysphereInteractor.maxRaycastDistance ? raycastInteractor.maxRaycastDistance : raysphereInteractor.maxRaycastDistance;
    }

    private void Update()
    {
        if (directInteractor.hasHover || directInteractor.hasSelection) { // check if direct interactor is doing anything
            raycastInteractor.enabled = false;
            raysphereInteractor.enabled = false;

            if (!directInteractor.hasSelection) { // direct interactor is not grabbing anything
                hoveringObjects = true;
                hoverTargets = directInteractor.interactablesHovered;
            } else {
                hoveringObjects = false;
            }
        } else if (raycastInteractor.hasHover || raycastInteractor.hasSelection) { // check if raycast interactor is doing anything
            raysphereInteractor.enabled = false;

            if (!raycastInteractor.hasSelection) { // raycast interactor is not grabbing anything
                hoveringObjects = true;
                hoverTargets = raycastInteractor.interactablesHovered;
            } else {
                hoveringObjects = false;
                directInteractor.enabled = false;
            }
        } else if (raysphereInteractor.hasHover || raysphereInteractor.hasSelection) { // check if raysphere interactor is doing anything
            directInteractor.enabled = true;
            raycastInteractor.enabled = true;

            if (!raysphereInteractor.hasSelection) { // raysphere interactor is not grabbing anything
                hoveringObjects = true;
                hoverTargets = raysphereInteractor.interactablesHovered;
            } else {
                hoveringObjects = false;

                directInteractor.enabled = false;
                raycastInteractor.enabled = false;
            }
        } else { // safety net
            directInteractor.enabled = true;
            raycastInteractor.enabled = true;
            raysphereInteractor.enabled = true;

            hoveringObjects = true;
        }

        if (hoveringObjects && hoverTargets != null && hoverTargets.Count > 0) {
            if (!lineRenderer.enabled) lineRenderer.enabled = true;
            lineRenderer.colorGradient = hoverObjectColors;

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, ChooseTarget().position);
        } else if (hoveringObjects && drawLineWhenNoPickup) {
            lineRenderer.colorGradient = normalColors;

            lineRenderer.SetPosition(0, transform.position);

            // raycast to see if there are any colliders in the way. use that point if it hits, otherwise just point the line forwards
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit rayHit, biggestDistance)) {
                lineRenderer.SetPosition(1, rayHit.point);
            } else {
                lineRenderer.SetPosition(1, transform.position + transform.forward * biggestDistance);
            }
        } else if (!hoveringObjects || !drawLineWhenNoPickup && lineRenderer.enabled) {
            lineRenderer.enabled = false;
        }
    }

    Transform ChooseTarget()
    {
        if (hoverTargets.Count <= 0) return null;

        float closest = (hoverTargets[0].transform.position - transform.position).sqrMagnitude;
        Transform target = hoverTargets[0].transform;

        for (int i = 1; i < hoverTargets.Count; ++i) {
            float distance = (hoverTargets[i].transform.position - transform.position).sqrMagnitude;

            if (distance < closest) {
                closest = distance;
                target = hoverTargets[i].transform;
            }
        }

        return target;
    }
}
