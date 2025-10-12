using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class EchoRecorder : MonoBehaviour {
    public float recordSeconds = 10f;
    public float sampleRateHz = 10f;

    public KeyCode spawnKey = KeyCode.Q;
    public EchoReplayer echoPrefab;
    public Transform echoSpawnParent;
    public LayerMask echoLayer;            // set to your "Echo" layer (optional)

    float sampleTimer;
    readonly List<Sample> samples = new();
    readonly List<InteractEvent> eventsAll = new();

    public struct Sample { public float t; public Vector3 p; public Quaternion r; }
    public struct InteractEvent { public float t; public int id; }

    void Update(){
        // record pose at fixed rate
        sampleTimer += Time.deltaTime;
        float interval = 1f / Mathf.Max(1f, sampleRateHz);
        while (sampleTimer >= interval){
            sampleTimer -= interval;
            samples.Add(new Sample{ t = Time.time, p = transform.position, r = transform.rotation });
        }

        // trim old data
        float cutoff = Time.time - recordSeconds;
        while (samples.Count > 0 && samples[0].t < cutoff) samples.RemoveAt(0);
        while (eventsAll.Count > 0 && eventsAll[0].t < cutoff) eventsAll.RemoveAt(0);

        // spawn echo
        if (Input.GetKeyDown(spawnKey) && echoPrefab && samples.Count >= 2){
            SpawnEcho();
        }
    }

    public void RecordInteract(int interactableId){
        eventsAll.Add(new InteractEvent{ t = Time.time, id = interactableId });
    }

    void SpawnEcho(){
        float t0 = samples[0].t;
        var path = new List<EchoReplayer.Keyframe>(samples.Count);
        foreach (var s in samples) path.Add(new EchoReplayer.Keyframe{ time = s.t - t0, pos = s.p, rot = s.r });

        var evs = new List<EchoReplayer.PlayEvent>(eventsAll.Count);
        foreach (var e in eventsAll) evs.Add(new EchoReplayer.PlayEvent{ time = e.t - t0, interactableId = e.id });

        var echo = Instantiate(echoPrefab, path[0].pos, path[0].rot, echoSpawnParent);
        if (echoLayer.value != 0) echo.gameObject.layer = LayerMaskToLayer(echoLayer);
        echo.Play(path, evs);
    }

    int LayerMaskToLayer(LayerMask m){
        int v = m.value; if (v == 0) return gameObject.layer;
        for (int i=0;i<32;i++) if ((v & (1<<i))!=0) return i;
        return gameObject.layer;
    }
}

