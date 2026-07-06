using UnityEngine;
using UnityEngine.UI;
using Discord.Sdk;

public class DiscordManager : MonoBehaviour
{
    [SerializeField] private ulong applicationId;
    [SerializeField] private RichPresence richPresence;
    [SerializeField] private FriendsList friendsList;

    private Client client;
    private string codeVerifier;

    private void Awake()
    {
        client = new Client();

        client.AddLogCallback(OnLog, LoggingSeverity.Error);
        client.SetStatusChangedCallback(OnStatusChanged);
        client.SetUserUpdatedCallback(OnUserUpdated);
    }
    private void OnDestroy()
    {
        client.Disconnect();
    }
    private void OnLog(string message, LoggingSeverity severity)
    {
        Debug.Log($"Log: {severity} - {message}");
    }
    private void OnStatusChanged(Client.Status status, Client.Error error, int errorCode)
    {
        Debug.Log($"Status changed: {status}");

        if (error != Client.Error.None)
        {
            Debug.LogError($"Error: {error}, code: {errorCode}");
        }
        if (status == Client.Status.Ready)
        {
            richPresence.UpdateRichPresence(client);
            friendsList.LoadFriends(client);
        }
    }
    public void StartOAuthFlow()
    {
        var authorizationVerifier = client.CreateAuthorizationCodeVerifier();
        codeVerifier = authorizationVerifier.Verifier();

        var args = new AuthorizationArgs();
        args.SetClientId(applicationId);
        args.SetScopes(Client.GetDefaultCommunicationScopes());
        args.SetCodeChallenge(authorizationVerifier.Challenge());
        client.Authorize(args, OnAuthorizeResult);
    }
    private void OnAuthorizeResult(ClientResult result, string code, string redirectUri)
    {
        if (!result.Successful())
        {
            Debug.Log($"Authorization result: [{result.Error()}]");
            return;
        }
        GetTokenFromCode(code, redirectUri);
    }
    private void GetTokenFromCode(string code, string redirectUri)
    {
        client.GetToken(applicationId, code, codeVerifier, redirectUri, OnGetToken);
    }
    private void OnGetToken(ClientResult result, string token, string refreshToken, AuthorizationTokenType tokenType, int expiresIn, string scope)
    {
        if (token == null || token == string.Empty)
        {
            Debug.Log("Failed to retrieve token");
        }
        else
        {
            client.UpdateToken(AuthorizationTokenType.Bearer, token, OnUpdateToken);
        }
    }
    private void OnUpdateToken(ClientResult result)
    {
        if (result.Successful())
        {
            client.Connect();
        }
        else
        {
            Debug.LogError($"Failed to update token: {result.Error()}");
        }
    }
    private void OnUserUpdated(ulong userId)
    {
        friendsList.UpdateFriends();
        friendsList.SortFriends();
    }
}
