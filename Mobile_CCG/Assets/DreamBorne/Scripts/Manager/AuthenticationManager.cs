
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System.Threading.Tasks;
using System;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class AuthenticationManager : Singleton<AuthenticationManager>
{
    #region Variables

    private string playerId;

    public string PlayerId { get { return this.playerId; } }

    #endregion

    #region Unity Methods

    protected override void Awake()
    {
        base.Awake();

        this.LoginAnonymously();
        CustomSceneManager.Instance.SwitchSceneAsync("Lobby");
    }

    #endregion

    #region Authentication Methods

    /// <summary>
    /// Inizialize unity services and login the player anonymously
    /// </summary>
    public async void LoginAnonymously()
    {
        await this.InizializeUnityServices();

        this.SetupEvents();

        await this.UnityLogin();
    }

    /// <summary>
    /// Inizaialize the unity services using the Parralel Sync package.
    /// </summary>
    private async Task InizializeUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var options = new InitializationOptions();

            #if UNITY_EDITOR
            // PARRALEL SYNC - It's used to differentiate the clients, otherwise lobby will count them as the same
            options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
            #endif

            await UnityServices.InitializeAsync(options);
        }
    }

    /// <summary>
    /// Set event listeners to receive updates of the status of the players authentication.
    /// </summary>
    private void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    /// <summary>
    /// Login the player anonymously via the authentication services.
    /// </summary>
    private async Task UnityLogin()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            this.playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log("Sign in anonymously suceeded!");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    #endregion
}
