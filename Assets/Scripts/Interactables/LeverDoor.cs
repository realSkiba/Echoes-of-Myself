using UnityEngine;

public class LeverDoor : MonoBehaviour, IInteractable {
    public Animator doorAnim;   // animator on a door; has a bool "Open"
    bool open;
    public void Interact(GameObject by){
        open = !open;
        if (doorAnim) doorAnim.SetBool("Open", open);
        Debug.Log($"Lever toggled by {by.name}");
    }
}

