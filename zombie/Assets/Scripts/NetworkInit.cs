using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkInit : MonoBehaviour
{
    private string ipAddress = "127.0.0.1";
    private ushort port = 7777;
    private GameObject playerPrefab;

    void Start()
    {
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
        NetworkManager.Singleton.AddNetworkPrefab(playerPrefab);

        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            Debug.Log($"Starting server on {ipAddress}:{port}");
            NetworkManager.Singleton.StartServer();
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("Starting host");
            NetworkManager.Singleton.StartHost();
            SpawnPlayer(0);
#else
            Debug.Log("Starting client");
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(ipAddress, port);
            NetworkManager.Singleton.StartClient();
#endif
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected with clientId={clientId}");

        if (!NetworkManager.Singleton.IsServer)
            return;
        
        SpawnPlayer(clientId);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"clientId={clientId} disconnected");
    }

    private void SpawnPlayer(ulong clientId)
    {
        Debug.Log($"Spawning player for clientId={clientId}");

        Quaternion spawnRot = Quaternion.identity;
        Vector3 spawnPos = new Vector3(0, 1, 0);

        GameObject player = Instantiate(playerPrefab, spawnPos, spawnRot);
        var netObj = player.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);
        netObj.name = clientId.ToString();

        var playersParent = GameObject.Find("Players").GetComponent<NetworkObject>();
        netObj.TrySetParent(playersParent, true);
    }
}
