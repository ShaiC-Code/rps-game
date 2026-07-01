using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public enum SelectedOption
{
    None,
    Rock,
    Paper,
    Scissors
}

public enum ResultState
{
    None,
    Tie,
    Player1Wins,
    Player2Wins
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Image rockImage;
    [SerializeField] private Image paperImage;
    [SerializeField] private Image scissorsImage;

    [SerializeField] private Text resultText;

    [SerializeField] private Button rockButton;
    [SerializeField] private Button paperButton;
    [SerializeField] private Button scissorsButton;

    [SerializeField] private float resultDisplayDuration = 3f;

    [HideInInspector]
    public NetworkVariable<ResultState> resultState = new NetworkVariable<ResultState>(
        ResultState.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [HideInInspector]
    public NetworkVariable<ulong> winnerClientId = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [HideInInspector]
    public NetworkVariable<bool> canSelectOption = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

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
        canSelectOption.OnValueChanged += (oldVal, newVal) =>
        {
            rockButton.interactable = newVal;
            paperButton.interactable = newVal;
            scissorsButton.interactable = newVal;
        };

        resultState.OnValueChanged += (oldVal, newVal) => UpdateResultText(newVal);
        winnerClientId.OnValueChanged += (oldVal, newVal) => UpdateResultText(resultState.Value);
    }

    private void UpdateResultText(ResultState state)
    {
        if (resultText == null) return;

        switch (state)
        {
            case ResultState.None:
                resultText.text = "Selected:";
                break;

            case ResultState.Tie:
                resultText.text = "It's a tie!";
                break;

            case ResultState.Player1Wins:
            case ResultState.Player2Wins:
                Player localPlayer = PlayerManager.Instance.LocalPlayer;
                if (localPlayer == null) return;

                bool iWon = winnerClientId.Value == localPlayer.OwnerClientId;
                resultText.text = iWon ? "You win!" : "You lose!";
                break;
        }
    }

    private void SelectOption(SelectedOption option)
    {
        Player localPlayer = PlayerManager.Instance.LocalPlayer;
        if (localPlayer == null) return;

        localPlayer.selectedOption.Value = option;
    }

    private void UpdateSelectionImages(Image selectedImage)
    {
        Image[] images = { rockImage, paperImage, scissorsImage };
        foreach (Image img in images)
            img.gameObject.SetActive(img == selectedImage);
    }

    private void Select(SelectedOption option, Image image)
    {
        SelectOption(option);
        UpdateSelectionImages(image);
    }

    public void SelectRock() => Select(SelectedOption.Rock, rockImage);
    public void SelectPaper() => Select(SelectedOption.Paper, paperImage);
    public void SelectScissors() => Select(SelectedOption.Scissors, scissorsImage);
    public void ResetSelection() => Select(SelectedOption.None, null);

    public async void DetermineWinner()
    {
        if (!IsServer) return;

        while (!PlayerManager.Instance.TryGetPlayer1(out _) || !PlayerManager.Instance.TryGetPlayer2(out _))
        {
            await Task.Yield();
        }

        if (!PlayerManager.Instance.TryGetPlayer1(out Player player1) ||
            !PlayerManager.Instance.TryGetPlayer2(out Player player2))
            return;

        SelectedOption option1 = player1.selectedOption.Value;
        SelectedOption option2 = player2.selectedOption.Value;

        Debug.Log($"Player1 selected: {option1}, Player2 selected: {option2}");

        if (option1 == SelectedOption.None || option2 == SelectedOption.None)
            return;

        if (option1 == option2)
        {
            resultState.Value = ResultState.Tie;
            winnerClientId.Value = ulong.MaxValue;
        }
        else if ((option1 == SelectedOption.Rock && option2 == SelectedOption.Scissors) ||
                 (option1 == SelectedOption.Paper && option2 == SelectedOption.Rock) ||
                 (option1 == SelectedOption.Scissors && option2 == SelectedOption.Paper))
        {
            resultState.Value = ResultState.Player1Wins;
            winnerClientId.Value = player1.OwnerClientId;
        }
        else
        {
            resultState.Value = ResultState.Player2Wins;
            winnerClientId.Value = player2.OwnerClientId;
        }

        Debug.Log($"Winner determined: {resultState.Value}, Winner ClientId: {winnerClientId.Value}");

        canSelectOption.Value = false;
        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resultDisplayDuration);

        resultState.Value = ResultState.None;
        winnerClientId.Value = ulong.MaxValue;
        canSelectOption.Value = true;
    }
}