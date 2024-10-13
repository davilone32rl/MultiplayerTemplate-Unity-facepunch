using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Text;
using Newtonsoft.Json.Bson;
using Netcode.Transports.Facepunch;
using System.Linq;

public class Onlinebuttons : MonoBehaviour
{
    public GameObject OnlineMenu;
    public GameObject MainMenu;
    public GameObject MenuCP;
    public GameObject SLM;
    public GameObject errNotHost;
    public Transform PlayerMenuContent;
    public GameObject PrefabPlayerMenuList;

    [SerializeField] private TMP_InputField LobbyIDInputField;
    [SerializeField] private TextMeshProUGUI LobbyID;
    [SerializeField] private TextMeshProUGUI PlayersInLobby;
    [SerializeField] private TMP_Text lobbyName;
    [SerializeField] private Slider sliderPlayer;


    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberLeft;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberLeft;
    }

    private void OnLobbyMemberLeft(Lobby lobby, Friend friend)
    {
        UpdateServerContent(lobby); 
        NotifyPlayersInLobby();
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        UpdateServerContent(lobby); 
        NotifyPlayersInLobby(); 
    }

    private async void GameLobbyJoinRequested(Lobby lobby, SteamId id)
    {
        await lobby.Join();
    }

    private void LobbyEntered(Lobby lobby)
    {
        MainMenu.SetActive(false);
        OnlineMenu.SetActive(true);
        MenuCP.SetActive(false);
        SLM.SetActive(false);
        LobbySaver.instance.currentLobby = lobby;
        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
        Debug.Log("We entered");
        UpdateServerContent(lobby);
        NotifyPlayersInLobby();
        AddPlayerToMenu(lobby);
        CheckUI();
    }

    public void AddPlayerToMenu(Lobby lobby)
    {
        foreach (var members in lobby.Members)
        {
            GameObject message = Instantiate(PrefabPlayerMenuList.gameObject);
            message.GetComponent<TextMeshProUGUI>().text = members.Name+ "'s";
        }
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);
            LobbySaver.instance.currentLobby = lobby;
            UpdateServerContent(lobby);
            NotifyPlayersInLobby();
            NetworkManager.Singleton.StartHost();
            AddPlayerToMenu(lobby);
        }
    }

    public async void HostLobby()
    {
        await SteamMatchmaking.CreateLobbyAsync(((int)sliderPlayer.value));
    }

    public async void JoinLobbyWithId()
    {
        if (string.IsNullOrWhiteSpace(LobbyIDInputField.text))
            return;

        if (!ulong.TryParse(LobbyIDInputField.text, out ulong ID))
            return;

        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

        Lobby? lobby = lobbies.FirstOrDefault(l => l.Id == ID);
        if (lobby != null)
        {
            await lobby?.Join();
        }
    }

    public void CopyIDServer()
    {
        TextEditor textEditor = new TextEditor();
        textEditor.text = LobbyID.text;
        textEditor.SelectAll();
        textEditor.Copy();
    }

    public void LeaveLobby()
    {
        LobbySaver.instance.currentLobby?.Leave();
        LobbySaver.instance.currentLobby = null;
        CheckUI();
    }

    public void ButtonStartRightNow()
    {
        if (NetworkManager.Singleton.IsHost)
            NetworkManager.Singleton.SceneManager.LoadScene("MapTest", LoadSceneMode.Single);
        
        if (!NetworkManager.Singleton.IsHost)
        {
            errNotHost.SetActive(true);
        }

    }

    public void ButtonerrNotHostAccept()
    {
        errNotHost.SetActive(false);
    }

    public void CheckUI()
    {
        if (LobbySaver.instance.currentLobby == null)
        {
            MainMenu.SetActive(true);
            OnlineMenu.SetActive(false);
        }
        else
        {
            MainMenu.SetActive(false);
            OnlineMenu.SetActive(true);
        }
    }
    public void UpdateServerContent(Lobby lobby)
    {
        LobbyID.text = lobby.Id.ToString();
        lobbyName.text = $"{lobby.Owner.Name}'s lobby";
        PlayersInLobby.text = $"{lobby.MemberCount}/{lobby.MaxMembers} players";
        Debug.Log($"Players: {lobby.MemberCount}/{lobby.MaxMembers}"); 
    }

    [ServerRpc]
    private void NotifyPlayersInLobby()
    {
        UpdatePlayerCountClientRpc(LobbySaver.instance.currentLobby?.MemberCount, LobbySaver.instance.currentLobby?.MaxMembers);
    }

    [ClientRpc]
    private void UpdatePlayerCountClientRpc(int? currentCount, int? maxCount)
    {
        PlayersInLobby.text = $"{currentCount}/{maxCount} Players";
    }
}
