using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    public void HostGame() => NetworkManager.Singleton.StartHost();
    public void JoinGame() => NetworkManager.Singleton.StartClient();
    public void ServerGame() => NetworkManager.Singleton.StartServer();
}