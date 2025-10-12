using UnityEngine;
using System.Collections.Generic;

public class InteractableID : MonoBehaviour {
    public int id;
    void OnEnable(){ InteractableRegistry.Register(this); }
    void OnDisable(){ InteractableRegistry.Unregister(this); }
}

public static class InteractableRegistry {
    static readonly Dictionary<int, InteractableID> map = new();
    public static void Register(InteractableID x){ if (x) map[x.id] = x; }
    public static void Unregister(InteractableID x){ if (x && map.TryGetValue(x.id, out var cur) && cur == x) map.Remove(x.id); }
    public static bool TryGet(int id, out InteractableID x) => map.TryGetValue(id, out x);
}

