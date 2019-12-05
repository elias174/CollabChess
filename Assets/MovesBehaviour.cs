using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;


public class MovesBehaviour : MonoBehaviour
{
    public GameObject item;
    public GameObject itemPossible;
    public GameObject prefabButton;
    public GameObject vote_button;
    public string source;
    public List<Tuple<string, string>> moves = new List<Tuple<string, string>>();
    public List<Move> official_moves = new List<Move>();

    public void makeMove()
    {
        Debug.Log("Making moves from the 'Moves' list");
        if ( moves.ToArray().Length >= 0)
        {
            RestClient.Post<PossibleMove>(new RequestHelper
            {
                Uri = "https://ihc-chess-server.herokuapp.com/make_possible_move_list",
                Method = "POST",
                Timeout = 10,
                Params = new Dictionary<string, string> { { "game_id", GlobalVars.player_current_game }, { "player_id", GlobalVars.player_id }}
            }
            ).Then( response =>
            {
                for (int i = 0; i < moves.ToArray().Length; i++)
                {
                    RestClient.Request(new RequestHelper
                    {
                        Uri = "https://ihc-chess-server.herokuapp.com/make_possible_move",
                        Method = "POST",
                        Timeout = 10,
                        Params = new Dictionary<string, string> {{ "player_id", GlobalVars.player_id },
                                                                { "source_position", moves.ToArray()[i].Item1 },
                                                                { "target_position", moves.ToArray()[i].Item2 },
                                                                { "list_id", response.id.ToString() }}
                    });
                }
            }
            
            
            );
            
            
        }
        
    }

    public void selectPossibleMoves(Text moves)
    {
        vote_button.transform.Find("ListIdToVote").GetComponent<Text>().text = moves.text;
    }

    public void Vote(Text ListToVote)
    {
        Debug.Log("Voting for list of moves -->" + ListToVote.text);
        if (ListToVote.text != "null")
        {
            RestClient.Request(new RequestHelper
            {
                Uri = "https://ihc-chess-server.herokuapp.com/vote",
                Method = "POST",
                Timeout = 10,
                Params = new Dictionary<string, string> { { "list_id", ListToVote.text }, { "game_id", GlobalVars.player_current_game } }

            });
        }
        else
        {
            Debug.Log("Debe seleccionar una jugada para votar por ella");
        }
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void addMove()
    {
        moves.Add(Tuple.Create<string, string>(GameObject.Find("sourcetext").GetComponent<Text>().text, GameObject.Find("targettext").GetComponent<Text>().text));
        for(int i=0; i < moves.ToArray().Length; i++)
        {
            Debug.Log(moves.ToArray()[i]);
        }
    }

    public void restart_board_with_official_moves()
    {
        official_moves.Reverse();
        StartCoroutine(simulate_moves(official_moves, false));
        official_moves.Reverse();
    }

    public void DoMoves(List<Move> moves, bool slow_motion)
    {
        // GameManager.Instance.restart_board_with_official_moves();
        StartCoroutine(make_simulation(moves, slow_motion));

    }

    IEnumerator make_simulation(List<Move> moves, bool slow_motion)
    {
        // yield return StartCoroutine(need_restart(moves, slow_motion));
        yield return StartCoroutine(simulate_moves(moves, slow_motion));
    }

    IEnumerator need_restart(List<Move> moves, bool slow_motion)
    {
        yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
    IEnumerator simulate_moves(List<Move> moves, bool slow_motion)
    {
        foreach (Move move in Enumerable.Reverse(moves))
        {
            Debug.Log(move.source_position);
            PlayerType type_a = GameManager.Instance.CurrentPlayer.Type;
            GameManager.Instance.SimulateMove(move.source_position, move.target_position);
            while (type_a == GameManager.Instance.CurrentPlayer.Type) { yield return null; }
            if(slow_motion) yield return new WaitForSeconds(1);
        }
        yield return new WaitForSeconds(5);
        yield return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Currently on Game Scene");
        RestClient.GetArray<Move>("https://ihc-chess-server.herokuapp.com/list-moves/"+GlobalVars.player_current_game).Then(response => {
            for (int i = 0; i < response.Length; i++)
            {
                GameObject newitem = (GameObject)Instantiate(item);
                var panel = GameObject.Find("ContentMoves");
                newitem.transform.parent = panel.transform.parent;
                newitem.GetComponent<RectTransform>().SetParent(panel.transform);
                newitem.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 200);
                newitem.GetComponent<RectTransform>().GetComponentInChildren<Text>().text = string.Format("{0} // {1}", response[i].source_position, response[i].target_position);

            }
        });

        RestClient.GetArray<PossibleMove>("https://ihc-chess-server.herokuapp.com/list-possible-moves/" + GlobalVars.player_current_game).Then(response => {
            var panel = GameObject.Find("PossibleMoves");
            for (int i = 0; i < response.Length; i++)
            {
                GameObject btn_vote = (GameObject)Instantiate(vote_button);
                btn_vote.transform.parent = panel.transform.parent;
                btn_vote.GetComponent<RectTransform>().SetParent(panel.transform);
                btn_vote.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0,200);
                btn_vote.transform.Find("ListIdToVote").GetComponent<Text>().text = response[i].id.ToString();

                GameObject newitem = (GameObject)Instantiate(itemPossible);
                newitem.transform.parent = panel.transform.parent;
                newitem.GetComponent<RectTransform>().SetParent(panel.transform);
                newitem.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 200);
                newitem.GetComponent<RectTransform>().GetComponentInChildren<Text>().text = string.Format("Propuesta {0}", response[i].id);
                Button tempButton = newitem.GetComponent<Button>();
                List<Move> moves_received = new List<Move>(new Move[] {});
                foreach (Move move in response[i].moves)
                {
                    moves_received.Add(move);
                }
                tempButton.onClick.AddListener(() => DoMoves(moves_received, true));
                newitem.transform.Find("possiblemovesId").GetComponent<Text>().text = response[i].id.ToString();

            }


        });
        InvokeRepeating("update_moves", 2f, 2f);


    }
    // Update is called once per frame

    void update_moves()
    {
        // Debug.Log(" service to update moves -> player_id: " + GlobalVars.player_id + " game_id: " + GlobalVars.player_current_game);
        RestClient.GetArray<Move>("https://ihc-chess-server.herokuapp.com/list-moves/"+GlobalVars.player_current_game).Then(response => {
            var panel1 = GameObject.Find("ContentMoves");
            if (response.Length > panel1.transform.childCount)
            {
                int last_index = response.Length - 1;
                GameObject newitem = (GameObject)Instantiate(item);
                var panel = GameObject.Find("ContentMoves");
                newitem.transform.parent = panel.transform.parent;
                newitem.GetComponent<RectTransform>().SetParent(panel.transform);
                newitem.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 200);
                newitem.GetComponent<RectTransform>().GetComponentInChildren<Text>().text = string.Format(
                    "{0} // {1}", response[last_index].source_position, response[last_index].target_position);
            }


        });

        RestClient.GetArray<PossibleMove>("https://ihc-chess-server.herokuapp.com/list-possible-moves/" + GlobalVars.player_current_game).Then(response => {
            var panel1 = GameObject.Find("PossibleMoves");
            if (response.Length > panel1.transform.childCount)
            {
                int last_index = response.Length - 1;

                GameObject btn_vote = (GameObject)Instantiate(vote_button);
                btn_vote.transform.parent = panel1.transform.parent;
                btn_vote.GetComponent<RectTransform>().SetParent(panel1.transform);
                btn_vote.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 200);
                btn_vote.transform.Find("ListIdToVote").GetComponent<Text>().text = response[last_index].id.ToString();

                GameObject newitem = (GameObject)Instantiate(itemPossible);
                newitem.transform.parent = panel1.transform.parent;
                newitem.GetComponent<RectTransform>().SetParent(panel1.transform);
                newitem.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 200);
                newitem.GetComponent<RectTransform>().GetComponentInChildren<Text>().text = string.Format("Propuesta {0}", response[last_index].id);
                Button tempButton = newitem.GetComponent<Button>();
                List<Move> moves_received = new List<Move>(new Move[] { });
                foreach (Move move in response[last_index].moves)
                {
                    moves_received.Add(move);
                }
                tempButton.onClick.AddListener(() => DoMoves(moves_received, true));
                newitem.transform.Find("possiblemovesId").GetComponent<Text>().text = response[last_index].id.ToString();

            }
        });
    }
    void Update()
    {
        /*
        Debug.Log("on update");
        RestClient.GetArray<Move>("https://ihc-chess-server.herokuapp.com/list-moves/5dbf88ddb67efa00144cbf3d").Then(response => {
            var panel1 = GameObject.Find("ContentMoves");
            if( response.Length > panel1.transform.childCount)
            {
                for (int i = panel1.transform.childCount - 1; i > 0; i--)
                {
                    Object.Destroy(panel1.transform.GetChild(i).gameObject);
                }

                for (int i = 0; i < response.Length; i++)
                {
                    GameObject newitem = (GameObject)Instantiate(item);
                    var panel = GameObject.Find("ContentMoves");
                    newitem.transform.parent = panel.transform.parent;
                    newitem.GetComponent<RectTransform>().SetParent(panel.transform);
                    newitem.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 200);
                    newitem.GetComponent<RectTransform>().GetComponentInChildren<Text>().text = string.Format("{0} // {1}", response[i].source_position, response[i].target_position);
                }
            }
            

        });
        */




    }

 
}
