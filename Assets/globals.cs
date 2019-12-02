using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVars : MonoBehaviour
{
    public static string myGlobal = "global variable";
    public static string player_id = "";
    public static string player_name = "";
    public static string player_current_game = "";

}
public class GlobalUser : MonoBehaviour
{
    public static GlobalUser Instance { get; set;  }
    public string player_id;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}