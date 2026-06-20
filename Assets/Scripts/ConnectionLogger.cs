using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ConnectionLogger : MonoBehaviour
{
    public NetworkVariable<int> ConnectedClients = new NetworkVariable<int>();
    public Text ConnectedClientsText;
    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += id =>
        {
            Debug.Log($"Client: {id}");
            ConnectedClients.Value++;
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += id =>
        {
            Debug.Log($"Client: {id}");
            ConnectedClients.Value--;
        };

        ConnectedClients.OnValueChanged += (oldValue, newValue) =>
        {
            ConnectedClientsText.text = $"Connected Clients: {newValue}";
        };
    }
}
