using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public NetworkVariable<SelectedOption> selectedOption = new NetworkVariable<SelectedOption>(
        SelectedOption.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override async void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        await PlayerManager.ReadyTask;

        if (IsServer)
            PlayerManager.Instance.RegisterPlayer(this);

        if (IsOwner)
        {
            PlayerManager.Instance.LocalPlayer = this;

            GameManager.Instance.canSelectOption.OnValueChanged += (oldVal, newVal) =>
            {
                if (newVal == true)
                    GameManager.Instance.ResetSelection();
            };
        }

        selectedOption.OnValueChanged += (oldValue, newValue) =>
        {
            if (IsOwner)
                Debug.Log($"You selected {newValue}");

            if (IsServer)
                GameManager.Instance.DetermineWinner();
        };
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            PlayerManager.Instance.UnregisterPlayer(this);

        base.OnNetworkDespawn();
    }
}