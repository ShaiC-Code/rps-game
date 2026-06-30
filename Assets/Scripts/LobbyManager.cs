using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform sessionListContainer; // Scroll View > Viewport > Content
    [SerializeField] private GameObject sessionButtonPrefab;
    [SerializeField] private InputField sessionNameInput;
    [SerializeField] private string gameSceneName = "GameScene";

    private async void Start()
    {
        await NetworkManager.Singleton.GetComponent<UGSInit>().InitTask;
        await RefreshSessionList();
    }

    public async void OnCreateClicked()
    {
        string name = string.IsNullOrWhiteSpace(sessionNameInput.text)
            ? "Room " + Random.Range(1000, 9999)
            : sessionNameInput.text;

        await CreateSession(name);
    }

    public async void OnRefreshClicked()
    {
        await RefreshSessionList();
    }

    private async Task CreateSession(string sessionName)
    {
        var options = new SessionOptions
        {
            Name = sessionName,
            MaxPlayers = 2,
            IsPrivate = false
        }.WithRelayNetwork();

        try
        {
            ISession session = await MultiplayerService.Instance.CreateSessionAsync(options);
            Debug.Log($"Session created: {session.Id}  Code: {session.Code}");
            SessionHolder.Session = session;
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
        catch (SessionException e)
        {
            Debug.LogError($"CreateSession failed: {e.Message}");
        }
    }

    private async Task JoinSession(string sessionId)
    {
        try
        {
            ISession session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
            Debug.Log($"Joined session: {session.Id}");
            SessionHolder.Session = session;
        }
        catch (SessionException e)
        {
            Debug.LogError($"JoinSession failed: {e.Message}");
        }
    }

    private async Task RefreshSessionList()
    {
        foreach (Transform child in sessionListContainer)
            Destroy(child.gameObject);

        QuerySessionsResults results;
        try
        {
            results = await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());
        }
        catch (SessionException e)
        {
            Debug.LogError($"QuerySessions failed: {e.Message}");
            return;
        }

        foreach (ISessionInfo info in results.Sessions)
        {
            int currentPlayers = info.MaxPlayers - info.AvailableSlots;

            GameObject go = Instantiate(sessionButtonPrefab, sessionListContainer);
            go.GetComponentInChildren<Text>().text =
                $"{info.Name}  ({currentPlayers}/{info.MaxPlayers})";

            string capturedId = info.Id;
            go.GetComponent<Button>().onClick.AddListener(async () =>
            {
                await JoinSession(capturedId);
            });
        }
    }
}