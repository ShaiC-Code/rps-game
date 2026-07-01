using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private static TaskCompletionSource<bool> _readyTcs = new TaskCompletionSource<bool>();
    public static Task ReadyTask => _readyTcs.Task;

    public Player LocalPlayer { get; set; }

    public NetworkVariable<NetworkBehaviourReference> player1Ref = new();
    public NetworkVariable<NetworkBehaviourReference> player2Ref = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        _readyTcs.TrySetResult(true);
    }

    public override void OnNetworkDespawn()
    {
        _readyTcs = new TaskCompletionSource<bool>();
    }

    public void RegisterPlayer(Player player)
    {
        if (!player1Ref.Value.TryGet(out Player _))
        {
            player1Ref.Value = player;
            Debug.Log($"Registered as player1: {player.OwnerClientId}");
        }
        else if (!player2Ref.Value.TryGet(out Player _))
        {
            player2Ref.Value = player;
            Debug.Log($"Registered as player2: {player.OwnerClientId}");
        }
        else
        {
            Debug.Log("Both slots already filled, could not register.");
        }
    }

    public void UnregisterPlayer(Player player)
    {
        if (player1Ref.Value.TryGet(out Player p1) && p1 == player)
            player1Ref.Value = default;
        else if (player2Ref.Value.TryGet(out Player p2) && p2 == player)
            player2Ref.Value = default;
    }

    public bool TryGetPlayer1(out Player player) => player1Ref.Value.TryGet(out player);
    public bool TryGetPlayer2(out Player player) => player2Ref.Value.TryGet(out player);
}