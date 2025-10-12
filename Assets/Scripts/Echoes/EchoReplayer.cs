using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class EchoReplayer : MonoBehaviour {
    public float replaySpeed = 1f;
    public float lookLerp = 12f;
    public float yOffset = 0f;
    public Renderer[] visuals;                       // assign for tint
    public Color color = new Color(0.7f,0.9f,1f,0.6f);

    public struct Keyframe { public float time; public Vector3 pos; public Quaternion rot; }
    public struct PlayEvent { public float time; public int interactableId; }

    List<Keyframe> path; List<PlayEvent> events;
    float t; int iSample, iEvent; CharacterController cc;

    public void Play(List<Keyframe> path, List<PlayEvent> evs){
        this.path = path; this.events = evs;
        t = 0f; iSample = 0; iEvent = 0;
        foreach (var r in visuals) if (r) { var m = r.material; m.color = color; }
        transform.position = path[0].pos + Vector3.up*yOffset;
        transform.rotation = path[0].rot;
    }

    void Awake(){ cc = GetComponent<CharacterController>(); if (!cc) cc = gameObject.AddComponent<CharacterController>(); }

    void Update(){
        if (path == null || path.Count < 2) { Destroy(gameObject); return; }
        t += Time.deltaTime * replaySpeed;

        // play events
        while (iEvent < events.Count && events[iEvent].time <= t){
            if (InteractableRegistry.TryGet(events[iEvent].interactableId, out var iid)){
                var interact = iid.GetComponent<IInteractable>(); interact?.Interact(gameObject);
            }
            iEvent++;
        }

        // follow path
        while (iSample < path.Count-2 && path[iSample+1].time < t) iSample++;
        var a = path[iSample]; var b = path[Mathf.Min(iSample+1, path.Count-1)];
        float u = Mathf.InverseLerp(a.time, b.time, t);
        Vector3 target = Vector3.Lerp(a.pos, b.pos, u) + Vector3.up*yOffset;
        Vector3 move = target - transform.position;
        Vector3 horiz = new Vector3(move.x,0,move.z);
        if (horiz.sqrMagnitude > 0.0001f){
            var dir = horiz.normalized;
            transform.forward = Vector3.Slerp(transform.forward, dir, lookLerp*Time.deltaTime);
        }
        cc.Move(move);

        if (iSample >= path.Count-2 && u >= 1f) Destroy(gameObject);
    }
}

