using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    void OnGUI()
    {
        const int w = 220, h = 30, pad = 5;
        int x = 10, y = 10;

        var nm = NetworkManager.Singleton;
        if (!nm)
        {
            GUI.Label(new Rect(x, y, 400, h), "NetworkUI: NO NetworkManager.Singleton found");
            return;
        }

        // Show current state
        string stateText = $"State: " +
                           $"IsServer={nm.IsServer} | IsClient={nm.IsClient} | IsHost={nm.IsHost}";
        GUI.Label(new Rect(x, y, 400, h), stateText);
        y += h + pad;

        // OFFLINE STATE: not server, not client -> we can choose Host/Client/Server
        if (!nm.IsServer && !nm.IsClient)
        {
            if (GUI.Button(new Rect(x, y, w, h), "Host (this instance)"))
            {
                Debug.Log("NetworkUI: StartHost() clicked");
                nm.StartHost();
            }
            y += h + pad;

            if (GUI.Button(new Rect(x, y, w, h), "Client (Join)"))
            {
                Debug.Log("NetworkUI: StartClient() clicked");
                nm.StartClient();
            }
            y += h + pad;

            if (GUI.Button(new Rect(x, y, w, h), "Server Only"))
            {
                Debug.Log("NetworkUI: StartServer() clicked");
                nm.StartServer();
            }
        }
        else
        {
            // ONLINE STATE: show a Shutdown button so we can go back to menu
            if (GUI.Button(new Rect(x, y, w, h), "Shutdown"))
            {
                Debug.Log("NetworkUI: Shutdown() clicked");
                nm.Shutdown();
            }
        }
    }
}
