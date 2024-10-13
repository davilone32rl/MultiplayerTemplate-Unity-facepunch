using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField MessageInputField;
    [SerializeField]
    private TextMeshProUGUI MessageTemplate;
    [SerializeField]
    private GameObject MessagesContainer;

    private void Start()
    {
        MessageTemplate.text = "";
    }

    private void OnEnable()
    {
        SteamMatchmaking.OnChatMessage += OnChatSend;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnChatMessage -= OnChatSend;
    }

    private void OnChatSend(Lobby lobby, Friend friend, string msg)
    {
        string[] forbiddenWords = { "Nazi", "nigger" };
        string urlPattern = @"(http[s]?:\/\/|www\.)[^\s]+";
        

        foreach (string word in forbiddenWords)
        {
            if (msg.Contains(word, StringComparison.OrdinalIgnoreCase)) 
            {
                msg = msg.Replace(word, new string('*', word.Length), StringComparison.OrdinalIgnoreCase);
            }
        }

        msg = Regex.Replace(msg, urlPattern, match => new string('*', match.Value.Length));
        AddMessageToBox(friend.Name + ": " + msg);
    }

    private void AddMessageToBox(string msg)
    {
        GameObject message = Instantiate(MessageTemplate.gameObject, MessagesContainer.transform);
        message.GetComponent<TextMeshProUGUI>().text = msg;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleChatBox();
        }
    }

    private void ToggleChatBox()
    {
        if (MessageInputField.gameObject.activeSelf)
        {
            if (!String.IsNullOrEmpty(MessageInputField.text))
            {
                LobbySaver.instance.currentLobby?.SendChatString(MessageInputField.text);
                MessageInputField.text = "";
            }

            MessageInputField.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            MessageInputField.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(MessageInputField.gameObject);
        }
    }
}
