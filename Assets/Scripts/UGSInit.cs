using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UGSInit : MonoBehaviour
{
    public Task InitTask { get; private set; }
    private void Awake()
    {
        InitTask = Initialize();
    }

    private async Task Initialize()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
}