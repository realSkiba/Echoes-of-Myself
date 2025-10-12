using UnityEngine;

public class Interactor : MonoBehaviour {
    [Header("Setup")]
    public Camera cam;                 // assign Main Camera in Inspector
    public KeyCode key = KeyCode.E;
    public float useRange = 3f;
    public LayerMask interactMask = ~0; // start with Everything while testing

    IInteractable hovered; InteractableID hoveredID;
    void Awake(){
        if (!cam) cam = Camera.main;
        if (!cam) Debug.LogError("Interactor: No camera assigned and no Camera.main found.");
    }

    void Update(){
        hovered = null; hoveredID = null;

        if (cam){
            // draw the ray so you can SEE it in Scene view
            Debug.DrawRay(cam.transform.position, cam.transform.forward * useRange, Color.cyan);

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, useRange, interactMask, QueryTriggerInteraction.Ignore)){
                // log what we hit
                // Debug.Log($"Hit {hit.collider.name}", hit.collider);

                hovered = hit.collider.GetComponentInParent<IInteractable>();
                hoveredID = hit.collider.GetComponentInParent<InteractableID>();
            }
        }

        // show a simple prompt in Console for now
        // if (hovered != null) Debug.Log("Press E to interact");

        if (hovered != null && Input.GetKeyDown(key)){
            hovered.Interact(gameObject);
            var rec = GetComponent<EchoRecorder>();
            if (hoveredID && rec) rec.RecordInteract(hoveredID.id);
        }
    }
}
