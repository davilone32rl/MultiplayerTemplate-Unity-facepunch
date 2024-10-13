using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtonsAndErrors : MonoBehaviour
{

    public Slider pl_Slider;
    public TextMeshProUGUI PlayersText;
    // GameObjects
    public GameObject ErrorMenu;
    public GameObject CpMenu;
    public GameObject SLM;
    // Bools
    public bool MainScreenCP = false;
    public bool ServerListMenu = false;
    void Start()
    {
        if (!Steamworks.SteamClient.IsValid)
        {
            ErrorMenu.SetActive(true);
        }
    }

    void Update()
    {
        PlayersText.text = $"{pl_Slider.value.ToString()}/6 Players";
    }

    public void OnPressButtonCP()
    {
        if (!MainScreenCP)
        {
            if (ServerListMenu)
            {
                CpMenu.SetActive(true);
                SLM.SetActive(false);
                MainScreenCP = true;
                ServerListMenu = false;

            }
            else
            {
                CpMenu.SetActive(true);
                MainScreenCP = true;
            }
        }
        else
        {
            CpMenu.SetActive(false);
            MainScreenCP = false;
        }
    }

    public void OnPressButonSLM()
    {
        if(!ServerListMenu)
        {
            if (MainScreenCP)
            {
                CpMenu.SetActive(false);
                SLM.SetActive(true);
                ServerListMenu = true;
                MainScreenCP = false;
            }
            else
            {
                SLM.SetActive(true);
                ServerListMenu = true;
            }
        }
        else
        {
            SLM.SetActive(false);
            ServerListMenu = false;
        }
    }

    public void OnPressButtonExit()
    {
        Application.Quit();
        Steamworks.SteamClient.Shutdown(); ;
    }
}
