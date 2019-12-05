using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Moves : MonoBehaviour
{
    // Start is called before the first frame update
    public void ReceiveMoves()
    {
        string[] input_text = GameObject.Find("TextTestInput").GetComponent<Text>().text.Split(',');
        string from = input_text.GetValue(0).ToString();
        string to = input_text.GetValue(1).ToString();


        GameManager.Instance.SimulateMove(from, to);

    }
    public void UndoTemporalMove()
    {
        GameManager.Instance.restart_board_with_official_moves();
    }

    public void SendTemporalMove()
    {
        GameManager.Instance.SendTemporalMoveToServer();
    }

}
