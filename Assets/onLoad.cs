using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class onLoad : MonoBehaviour
{

    public GameObject item;
   
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("asdasdadas");
        RestClient.GetArray<Game>("https://ihc-chess-server.herokuapp.com/list-games").Then(response => {
            Debug.Log(response[0].id);
            for (int i = 0; i < response.Length; i++)
            {
                GameObject newitem = (GameObject)Instantiate(item);
                var panel = GameObject.Find("ContentRooms");
                newitem.transform.parent = panel.transform.parent;
                newitem.GetComponent<RectTransform>().SetParent(panel.transform);
                newitem.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 200);
                newitem.GetComponent<RectTransform>().GetComponentInChildren<Text>().text = string.Format("Sala:{0} ----- Jugadores:({1}/{2})",i+1,response[i].players.Length,5);
                newitem.transform.Find("gameId").GetComponent<Text>().text = response[i].id;
            }

        });

        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
