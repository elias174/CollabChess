using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System;
using System.Linq;
using UnityEngine.SceneManagement;


public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private Material pieceP1;
    [SerializeField]
    private Material pieceP2;
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private LayerMask clickableMask;
    [SerializeField]
    private Color pieceHighlightColor;

    [SerializeField]
    private Material highlightMoveMaterial;
    [SerializeField]
    private Material highlightEatMaterial;
    [SerializeField]
    private Material highlightCheckMaterial;

    public Text winnerText;
    public Text blackScoreText;
    public Text whiteScoreText;
    public Text gamesPlayedText;
    public GameObject winnerCanvas;
    public GameObject continueButton;
    public GameObject winnerButton;

    public const string PLAYER_WHITE = "PLAYER_WHITE";
    public const string PLAYER_BLACK = "PLAYER_BLACK";
    public const string SCORE_MAX = "SCORE_MAX";
    public const string GAME_MAX = "GAME_MAX";
    public const string GAME_CURRENT = "GAME_CURRENT";
    public const string CAMERA_VIEW = "CAMERA_VIEW";

    public delegate void SwitchedPlayer();
    public static event SwitchedPlayer SwitchedEvent;

    private GCPlayer p1 = new GCPlayer(PlayerType.P1);
    private GCPlayer p2 = new GCPlayer(PlayerType.P2);
    private GCPlayer currentPlayer;

    private GameState gameState;

    //EXPERIMENT_TIMER
    public const string PLAYER_TIMER = "PLAYER_TIMER";
    public const int DEFAULT_PLAYER_TIMER_MIN = 10; //minutes
    public Text whiteTimerText;
    public Text blackTimerText;
    public string temporal_source_position;
    public string temporal_target_position;
    public Tuple<string, string> temporal_move;
    public List<Move> all_moves;
    public bool finished_receiving_from_server = false;

    private bool whiteTurn;
    private float whiteTimer;
    public bool finished_restarted;
    private float blackTimer;
    //END of EXPERIMENT_TIMER

    private bool ready;

    public GCPlayer PlayerOponent
    {
        get
        {
            return Opponent(currentPlayer);
        }
    }

    public Material HighlightCheckMaterial
    {
        get { return highlightCheckMaterial; }
    }

    public Material HighlightEatMaterial
    {
        get { return highlightEatMaterial; }
    }

    public Color PieceHighlightColor
    {
        get { return pieceHighlightColor; }
    }

    public Material HighlightMoveMaterial
    {
        get { return highlightMoveMaterial; }
    }

    public GameState GameState
    {
        get { return gameState; }
    }

    public GCPlayer CurrentPlayer
    {
        get { return currentPlayer; }
    }

    public LayerMask CLickableMask
    {
        get { return clickableMask; }
    }

    public Camera MainCamera
    {
        get { return mainCamera; }
    }

    public GCPlayer P1
    {
        get { return p1; }
    }

    public GCPlayer P2
    {
        get { return p2; }
    }

    public bool IsReady
    {
        get { return ready; }
    }

    public Material PieceP1
    {
        get { return pieceP1; }
    }

    public Material PieceP2
    {
        get { return pieceP2; }
    }

    public Grid Grid
    {
        get { return grid; }
    }

    void Awake()
    {
        _destroyOnLoad = destroyOnLoad;
        gameState = new GameState();
        LoadScores();

        //EXPERIMENT_TIMER
        SetTimers();
    }


    //EXPERIMENT_TIMER
    void SetTimers()
    {
        float timer = PlayerPrefs.GetInt(PLAYER_TIMER, DEFAULT_PLAYER_TIMER_MIN); //get in minutes
        timer *= 60f; //convert to seconds
        whiteTimer = blackTimer = timer;
        UpdateWhiteTimer();
        UpdateBlackTimer();
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(init());
    }

    IEnumerator init()
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();

        //wait for nodes
        while (!grid.IsReady) yield return null;
        //wait for pieces
        while (!grid.ArePiecesSpawned) yield return null; //wait till pieces are spawned
        while (!p1.IsReady) yield return null; //wait till all pieces are scaled in
        while (!p2.IsReady) yield return null;
        print("Time elapsed: " + timer.ElapsedMilliseconds / 1000.0 + "s");
        timer.Stop();

        //IMPORTANT
        p1.ComputePieces();
        p2.ComputePieces();
        SwitchPlayer(); //if null current player = p1

        //all objects are now ready
        ready = true;
        yield return StartCoroutine(fill_all_moves_from_server());
        yield return StartCoroutine(simulate_moves(all_moves, false));

    }

    // Update is called once per frame
    void Update()
    {

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GameManager.Instance.GameState.Checkmate();
            GameOver(p1, GameOverType.CHECKMATE); //TODO delete
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            GameManager.Instance.GameState.Checkmate();
            GameOver(p2, GameOverType.CHECKMATE); //TODO delete
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            GameManager.Instance.GameState.Stalemate();
            GameOver(p2, GameOverType.STALEMATE);
        }
#endif

        if (!ready) return;
        if (gameState.IsGameOver) return;


        //EXPERIMENT_TIMER
        if (whiteTurn)
        {
            whiteTimer -= Time.deltaTime;
            if (whiteTimer < 0)
            {
                whiteTimer = 0;
                GameManager.Instance.GameState.OutOfTime();
                GameOver(p2, GameOverType.OUT_OF_TIME);
            }
            UpdateWhiteTimer();
        }
        else
        {
            blackTimer -= Time.deltaTime;
            if (blackTimer < 0)
            {
                blackTimer = 0;
                GameManager.Instance.GameState.OutOfTime();
                GameOver(p1, GameOverType.OUT_OF_TIME);
            }
            UpdateBlackTimer();
        }
    }

    //EXPERIMENT_TIMER
    void UpdateWhiteTimer()
    {
        whiteTimerText.text = "WHITE\n" + GetChessTimeFormat(whiteTimer);
    }

    //EXPERIMENT_TIMER
    void UpdateBlackTimer()
    {
        blackTimerText.text = "BLACK\n" + GetChessTimeFormat(blackTimer);
    }

    public void restart_board_with_official_moves()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //EXPERIMENT_TIMER
    string GetChessTimeFormat(float timeInSeconds)
    {
        int seconds = (int)(timeInSeconds % 60);
        int minutes = (int)(timeInSeconds / 60);

        int hours = (int)(minutes / 60);
        minutes = (int)(minutes % 60);
        string d2 = "D2";
        return hours.ToString(d2) + ":" + minutes.ToString(d2) + ":" + seconds.ToString(d2);
    }

    public void SimulateMove(string coord_from, string coord_to)
    {
        List<Piece> Pieces_local = GameManager.Instance.currentPlayer.Pieces;
        foreach (Piece piece in Pieces_local)
        {
            if (coord_from == piece.Node.ChessCoords)
            {
                List<Node> possible_moves = piece.PossibleMoves;
                Node from_node = piece.Node;
                foreach (Node tNode in possible_moves)
                {
                    if (coord_to == tNode.ChessCoords)
                    {
                        currentPlayer.simulate_move(from_node, tNode);
                        return;
                    }
                }
            }
        }
    }

    IEnumerator fill_all_moves_from_server()
    {
        RestClient.GetArray<Move>("https://ihc-chess-server.herokuapp.com/list-moves/" + GlobalVars.player_current_game).Then(response => {
            UnityEngine.Debug.Log("Receiving moves from server....");
            for (int i = 0; i < response.Length; i++)
            {
                all_moves.Add(response[i]);
            }
            finished_receiving_from_server = true;
        });
        yield return null;
    }

    IEnumerator simulate_moves(List<Move> moves, bool slow_motion)
    {
        while (!finished_receiving_from_server) yield return null;
        foreach (Move move in Enumerable.Reverse(moves))
        {
            PlayerType type_a = GameManager.Instance.CurrentPlayer.Type;
            GameManager.Instance.SimulateMove(move.source_position, move.target_position);
            while (type_a == GameManager.Instance.CurrentPlayer.Type) { yield return null; }
            if (slow_motion) yield return new WaitForSeconds(1);
        }
        finished_restarted = true;
        UnityEngine.Debug.Log("Recreateeed all official moves");
    }

    public void SendTemporalMoveToServer()
    {
        RestClient.Post<PossibleMove>(new RequestHelper
        {
                Uri = "https://ihc-chess-server.herokuapp.com/make_possible_move_list",
                Method = "POST",
                Timeout = 10,
                Params = new Dictionary<string, string> { { "game_id", GlobalVars.player_current_game }, { "player_id", GlobalVars.player_id } }
        }).Then(response =>
        {
            RestClient.Request(new RequestHelper
            {
                    Uri = "https://ihc-chess-server.herokuapp.com/make_possible_move",
                    Method = "POST",
                    Timeout = 10,
                    Params = new Dictionary<string, string> {{ "player_id", GlobalVars.player_id },
                                                             { "source_position", temporal_move.Item1 },
                                                             { "target_position", temporal_move.Item2 },
                                                             { "list_id", response.id.ToString() }}

            });
            temporal_move = null;
            temporal_source_position = null;
            temporal_target_position = null;
        });

    }

    public void SwitchPlayer()
    {
        if (currentPlayer != null)
        {
            currentPlayer.DisableInput();
        }

        if (currentPlayer == p2)
        {
            currentPlayer = p1;
            whiteTurn = true;
            // mainCamera.GetComponent<SwitchAngle>().SwitchCamera(PlayerType.P1);
        }
        else if (currentPlayer == p1)
        {
            currentPlayer = p2;
            whiteTurn = false;
            // mainCamera.GetComponent<SwitchAngle>().SwitchCamera(PlayerType.P2);
        }
        else
        {
            currentPlayer = p1;
            whiteTurn = true;
        }

        //IF checkmate
        if (Rules.HasNoMove())
        {
            InputManager.Instance.UnHighlightTile();
            if (currentPlayer.IsChecked)
            {
                GameManager.Instance.GameState.Checkmate();
                GameOver(PlayerOponent, GameOverType.CHECKMATE);
                print("CHECKMATE");
            }
            else
            {
                GameManager.Instance.GameState.Stalemate();
                print("STALEMATE");
                GameOver(currentPlayer, GameOverType.STALEMATE);
            }
        }

        currentPlayer.EnableInput();
        if (SwitchedEvent != null)
        {
            SwitchedEvent(); //EXPERIMENTAL
        }

        print("Turn of: " + currentPlayer.Type); //show on screen 
    }

    public GCPlayer Opponent(GCPlayer player)
    {
        if (player == null) return null;
        if (player == p1) return p2;
        else return p1;
    }

    public void GameOver(GCPlayer winner, GameOverType gameOverType)
    {
        AddGame();
        switch (gameOverType)
        {
            case GameOverType.CHECKMATE:
                if (winner == p2)
                {
                    winnerText.text = "CHECKMATE: BLACK wins";
                    AddScore(PLAYER_BLACK);
                }
                else if (winner == p1)
                {
                    winnerText.text = "CHECKMATE: WHITE wins";
                    AddScore(PLAYER_WHITE);
                }
                break;
            case GameOverType.STALEMATE:
                winnerText.text = "STALEMATE: It's a tie";
                break;
            case GameOverType.OUT_OF_TIME:
                if (winner == p1)
                {
                    winnerText.text = "OUT OF TIME: WHITE wins";
                    AddScore(PLAYER_WHITE);
                }
                else if (winner == p2)
                {
                    winnerText.text = "OUT OF TIME: BLACK wins";
                    AddScore(PLAYER_BLACK);
                }
                break;
        }
        continueButton.SetActive(true);
    }

    public void End()
    {
        continueButton.SetActive(false);
        winnerButton.SetActive(true);
    }

    public void AddGame()
    {
        int newGame = PlayerPrefs.GetInt(GAME_CURRENT, 0) + 1;
        PlayerPrefs.SetInt(GAME_CURRENT, newGame);

        int maxGame = PlayerPrefs.GetInt(GAME_MAX, 0);
        if (newGame >= maxGame)
        {
            End();
        }
    }

    public void AddScore(string playerString)
    {
        int newScore = PlayerPrefs.GetInt(playerString, 0) + 1;
        PlayerPrefs.SetInt(playerString, newScore);

        int maxScore = PlayerPrefs.GetInt(SCORE_MAX, 0);
        if (newScore >= maxScore)
        {
            End();
        }
    }

    public void LoadScores()
    {
        int white = PlayerPrefs.GetInt(PLAYER_WHITE, 0);
        int black = PlayerPrefs.GetInt(PLAYER_BLACK, 0);
        whiteScoreText.text = white.ToString();
        blackScoreText.text = black.ToString();

        int gamesPlayed = PlayerPrefs.GetInt(GAME_CURRENT, 0);
        gamesPlayedText.text = gamesPlayed.ToString();
    }

    public static void ResetScores()
    {
        PlayerPrefs.DeleteKey(PLAYER_WHITE);
        PlayerPrefs.DeleteKey(PLAYER_BLACK);
        PlayerPrefs.DeleteKey(GAME_CURRENT);
    }
}
