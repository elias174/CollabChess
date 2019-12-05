using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Proyecto26;
using UnityEditor;
using System;
using UnityEngine.UI;
using System.Net.Http;

[Serializable]
public class Move
{
    public string game_id;
    public int id;
    public int player_id;
    public string source_position;
    public string target_position;
}


[Serializable]
public class PossibleMove
{
    public int id;
    public int player_id;
    public Move[] moves;
}
[Serializable]
public class Player
{
    public int id;
    public string name;
    public string game_id;

}

[Serializable]
public class Game
{
    public string id;
    public Move[] moves;
    public Player[] players;
}


public class LobbyBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject itemRoom;
    public Text tex_id;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(" update in lobby -> player_id: " + GlobalVars.player_id + " game_id: " + GlobalVars.player_current_game);
    }

    public void enterGame(Text gameId)
    {
        Debug.Log(gameId.text);
        Debug.Log(GlobalVars.player_name);
        RestClient.Request(new RequestHelper
        {
            Uri = "https://ihc-chess-server.herokuapp.com/enter_game",
            Method = "POST",
            Timeout = 10,
            Params = new Dictionary<string, string> { {"game_id",gameId.text }, {"player_id",GlobalVars.player_id} }

        }).Then( response => 
        {
            GlobalVars.player_current_game = gameId.text;
            SceneManager.LoadScene("Main");
        });
    }

    public void exitGame()
    {
        RestClient.Request(new RequestHelper
        {
            Uri = "https://ihc-chess-server.herokuapp.com/exit_game",
            Method = "POST",
            Timeout = 10,
            Params = new Dictionary<string, string> { { "player_id", GlobalVars.player_id } }

        }).Then(response =>
        {
            GlobalVars.player_current_game = "";
            SceneManager.LoadScene("lobby");
        });
    }

    public void create_game()
    {
        RestClient.Get<Game>("https://ihc-chess-server.herokuapp.com/create-game").Then(response => {
            tex_id.text = response.id;
            }
        );

    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("mainmenu");
    }
}
