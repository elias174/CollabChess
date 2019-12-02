using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToCollaborativeLobby()
    {
        SceneManager.LoadScene("lobby");
    }

    public void GoToLogin()
    {
        SceneManager.LoadScene("login");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("mainmenu");
    }
    public void Exit()
    {
        Application.Quit();
    }
}
