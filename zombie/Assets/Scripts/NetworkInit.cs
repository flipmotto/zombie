using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkInit : MonoBehaviour
{
    public string ipAddress = "127.0.0.1";
    public ushort port = 7777;
    private GameObject playerPrefab;

    void Start()
    {
        // Load and register the player prefab
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player");

        NetworkManager.Singleton.AddNetworkPrefab(playerPrefab);

        // Hook up client connection callback
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            Debug.Log("Headless mode detected → starting server...");
            NetworkManager.Singleton.StartServer();
        }
        else
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(ipAddress, port);
            NetworkManager.Singleton.StartClient();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        Debug.Log($"Spawning player for clientId={clientId}");
        GameObject player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
