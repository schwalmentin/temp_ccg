
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

        public string PlayerId { get; private set; }

    #endregion

    #region Unity Methods

        protected override async void Awake()
        {
            base.Awake();

            await this.LoginAnonymously();
            await CustomSceneManager.Instance.SwitchSceneAsync("Lobby", false);
        }  

    #endregion

    #region Authentication Methods

        /// <summary>
        /// Inizialize unity services and login the player anonymously
        /// </summary>
        private async Task LoginAnonymously()
        {
            await this.InitializeUnityServices();

            this.SetupEvents();

            await this.UnityLogin();
        }

        /// <summary>
        /// Initialize the unity services using the Parralel Sync package.
        /// </summary>
        private async Task InitializeUnityServices()
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
                this.PlayerId = AuthenticationService.Instance.PlayerId;
                Debug.Log("Sign in anonymously suceeded!");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

    #endregion
}
