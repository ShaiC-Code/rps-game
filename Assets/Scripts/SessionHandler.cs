using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SessionHandler : MonoBehaviour
{
    [SerializeField] private Text resultText;
    [SerializeField] private string lobbySceneName = "LobbySelect";

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (SessionHolder.Session != null)
        {
            Debug.Log("Subscribing to RemovedFromSession");
            SessionHolder.Session.RemovedFromSession += OnRemovedFromSession;
        }
        else
        {
            Debug.LogError("SessionHolder.Session is null in Start — events won't fire");
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        if (SessionHolder.Session != null)
            SessionHolder.Session.RemovedFromSession -= OnRemovedFromSession;
    }

    private async void OnRemovedFromSession()
    {
        Debug.Log("OnRemovedFromSession fired");
        if (NetworkManager.Singleton.IsServer) return;

        if (resultText != null)
            resultText.text = "Host left the session.";

        await Task.Delay(2000);

        SessionHolder.Session = null;
        SceneManager.LoadScene(lobbySceneName);
    }

    public async void LeaveSession()
    {
        if (SessionHolder.Session != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                try
                {
                    var hostSession = SessionHolder.Session.AsHost();
                    await hostSession.DeleteAsync();
                }
                catch (SessionException e)
                {
                    Debug.LogError($"DeleteSession failed: {e}");
                }
            }
            else
            {
                await SessionHolder.Session.LeaveAsync();
            }
        }

        SessionHolder.Session = null;
        SceneManager.LoadScene(lobbySceneName);
    }

    private async void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"OnClientDisconnected fired. clientId={clientId} ServerClientId={NetworkManager.ServerClientId} IsServer={NetworkManager.Singleton.IsServer}");

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Host disconnect detected on client — redirecting");
            if (resultText != null)
                resultText.text = "Host left the session.";

            await Task.Delay(2000);

            SessionHolder.Session = null;
            SceneManager.LoadScene(lobbySceneName);
        }
    }
}