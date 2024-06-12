using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
public class TicTacTocMultiplayer : MonoBehaviourPunCallbacks
{
    public Button[] buttons; //9 botones
    public TMP_Text statusText; // Texto de control
    private int[,] board = new int[3, 3]; // 0 vacio, 1 jugador x, 2 jugador Y
    private bool isPlayerXTurn = true; //Empieza X
    private bool isLocalPlayerTurn = true; //Controla que el turno es el local
    private int movesCount = 0; //Controla el numero de movimientos para el empate

    
    // Start is called before the first frame update
    void Start()
    {
        ResetBoard(); //Inicializa el tablero
        ConnectPhoton(); //Conecta el photon
    }//Start

    private void ConnectPhoton()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    //Nos conectamos al master
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("3enRaya", new Photon.Realtime.RoomOptions { MaxPlayers = 2 },
            Photon.Realtime.TypedLobby.Default);
    }

    //Nos unimos al lobby
    public override void OnJoinedRoom()
    {
        Debug.Log("Unido a la habitación...." + PhotonNetwork.CurrentRoom.Name);
        isLocalPlayerTurn = PhotonNetwork.IsMasterClient; //El que es master es el que empieza a jugar
    }//ONJoinedRoom

    //Método que controla el click del raton. Le pasamos un indice del boton

    public void OnButtonClick(int index)
    {
        if(PhotonNetwork.IsConnected && isLocalPlayerTurn)
        {//Es mi turno por el local
            int x = index % 3;//resto
            int y = index / 3;
            //Comprobar si el boton está vacio
            if (board[x,y] == 0)
            {
                photonView.RPC("MakeMove", RpcTarget. All,x,y, isPlayerXTurn ? 1 : 2); //tiene que ejecutarse en los 2
                UpdateText();
            }
        }
    }//OnButtonClick

    [PunRPC]
    public void MakeMove(int x, int y, int player)
    {
        board[x,y] = player;
        //Actualizo el turno 
        isPlayerXTurn = !isPlayerXTurn;
        isLocalPlayerTurn = !isLocalPlayerTurn;
        movesCount++;
        int buttonIndex = y * 3 + x; // el cociente mas el resto
        buttons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>().text = player == 1 ? "X" : "0";
        //Verifico si he ganado
        if (CheckWin())
        {
            statusText.text = "Gana: " + (player == 1 ? "X" : "0");
            //Reseteamos el tablero para empezar de nuevo
            Invoke("ResetBoard", 2.5f);
        }else if (movesCount >= 9)
        {
            statusText.text = "Es un empate";
            Invoke("ResetBoard", 2.5f);
        }
    }//MakeMove

    public bool CheckWin()
    {
        //Comprobamos filas y columnas 
        for(int i = 0; i < 3; i++)
        {
            if (board[i,0]!=0 && board[i,0] == board[i,1] && board[i, 1] == board[i, 2]) //comprobamos que este vacia y despues seguimos las filas
                return true;

            if (board[0, i] != 0 && board[0, 1] == board[1, i] && board[1, i] == board[2, i])
                return true;
        }
        //Comprobamos diagonales
        if (board[0, 0] != 0 && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
            return true;

        if (board[0, 2] != 0 && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
            return true;

        return false;
    }//CheckWin

    //Resetea el tablero
    private void ResetBoard()
    {
        for (int x = 0; x < 3; x++)
        {
            for(int y = 0; y < 3; y++)
            {
                board[x, y] = 0; //Ponemos a vacio el array
            }
        }
        foreach(Button button in buttons)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
        //Inicializamos las variables del conteo y turnos
        movesCount = 0;
        isPlayerXTurn = true;
        isLocalPlayerTurn = PhotonNetwork.IsMasterClient; // Si lo soy, soy el que maneja el juego
        //Actualizamos el banner del estatus (es el texto donde sale el turno del canvas)
        UpdateText();


    }//ResetBoard

    private void UpdateText()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Está conectado.");
            statusText.text = "Conectando....";
        }
        else if (isLocalPlayerTurn)//Si es mi turno
        {
            Debug.Log("Tu turno: " + (isLocalPlayerTurn ? "X" : "0")); // si esto es true haces esto y si no haces lo otro
            statusText.text = "Tu turno: " + (isLocalPlayerTurn ? "Y" : "0"); //
        }
        else //Es el turno del oponente
        {
            Debug.Log("Turno del oponente.");
            statusText.text = "Turno del oponente";
        }
    }



}//TICTACTOCMULTIPLAYER
