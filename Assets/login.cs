using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class login : MonoBehaviour
{
    
    public void authenticate()
    {
        Debug.Log("authenticating");
        RestClient.Post<Player>(new RequestHelper
        {
            Uri = "https://ihc-chess-server.herokuapp.com/authenticate",
            Method = "POST",
            Timeout = 10,
            Params = new Dictionary<string, string> { { "username", GameObject.Find("usertext").GetComponent<Text>().text },
                                                  { "password", GameObject.Find("passwordtext").GetComponent<Text>().text} },

        }).Then(response =>
        {
            GlobalVars.player_id = response.id.ToString();
            //GlobalUser.Instance.player_id = response.id.ToString();
            SceneManager.LoadScene("lobby");
            
        });

    }
}
