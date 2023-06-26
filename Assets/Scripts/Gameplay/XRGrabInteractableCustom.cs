using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabInteractableCustom : XRGrabInteractable
{
    [SerializeField] bool moveGrabTransform = true;
    Collider[] objectColliders;

    private void Start()
    {
        if (attachTransform == null) {
            attachTransform = Instantiate(new GameObject(), transform).transform;
        }

        objectColliders = GetComponents<Collider>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (moveGrabTransform && objectColliders != null && objectColliders.Length > 0) {
            Transform grabberTransform = args.interactorObject.transform;
            Vector3 closestPointOnTarget = objectColliders[0].ClosestPoint(grabberTransform.position);

            float distance = (closestPointOnTarget - grabberTransform.position).sqrMagnitude;

            for (int i = 1; i < objectColliders.Length; ++i) {
                Vector3 closestPoint = objectColliders[i].ClosestPoint(grabberTransform.position);
                float tempDistance = (closestPoint - grabberTransform.position).sqrMagnitude;

                if (tempDistance > distance) {
                    closestPointOnTarget = closestPoint;
                    distance = tempDistance;
                }
            }

            attachTransform.position = closestPointOnTarget;
            attachTransform.rotation = grabberTransform.rotation;
        }

        base.OnSelectEntered(args);
    }
}
