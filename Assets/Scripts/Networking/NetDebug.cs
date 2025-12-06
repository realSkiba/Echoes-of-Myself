using UnityEngine;
using Unity.Netcode;

public class NetDebug : MonoBehaviour
{
    void Start()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("NetDebug: No NetworkManager.Singleton found!");
            return;
        }

        nm.OnClientConnectedCallback += (id) =>
        {
            Debug.Log($"NetDebug: Client connected: {id}");
        };

        nm.OnClientDisconnectCallback += (id) =>
        {
            Debug.Log($"NetDebug: Client disconnected: {id}");
        };

        nm.OnServerStarted += () =>
        {
            Debug.Log("NetDebug: Server started");
        };
    }
}

