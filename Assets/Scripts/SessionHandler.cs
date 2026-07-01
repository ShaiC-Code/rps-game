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

    private bool _isLeavingIntentionally = false;

    private async void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        await SessionHolder.SessionReady;

        Debug.Log("Subscribing to RemovedFromSession");
        SessionHolder.Session.RemovedFromSession += OnRemovedFromSession;
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
        _isLeavingIntentionally = true;

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
        Debug.Log($"OnClientDisconnected fired. clientId={clientId} IsServer={NetworkManager.Singleton.IsServer}");

        if (!NetworkManager.Singleton.IsServer && !_isLeavingIntentionally)
        {
            Debug.Log("Host disconnect detected on client — redirecting");
            if (resultText != null)
                resultText.text = "Host left the session.";

            await Task.Delay(3000);

            SessionHolder.Session = null;
            SceneManager.LoadScene(lobbySceneName);
        }
    }
}