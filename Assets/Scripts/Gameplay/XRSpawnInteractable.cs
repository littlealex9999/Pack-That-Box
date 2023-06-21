using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRSpawnInteractable : XRBaseInteractable
{
    [SerializeField] GameObject grabbableObject;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        GameObject newObject = Instantiate(grabbableObject, transform.position, transform.rotation);
        if (GameManager.instance != null) GameManager.instance.AddObjectToClearList(newObject);

        XRGrabInteractable objectInteractable = newObject.GetComponent<XRGrabInteractable>();
        interactionManager.SelectEnter(args.interactorObject, objectInteractable);

        base.OnSelectEntered(args);
    }
}
