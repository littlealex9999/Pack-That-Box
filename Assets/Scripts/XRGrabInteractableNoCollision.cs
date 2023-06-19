using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabInteractableNoCollision : XRGrabInteractable
{
    LayerMask originalLayerMask;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        originalLayerMask = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Grabbed");
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        gameObject.layer = originalLayerMask;
    }
}
