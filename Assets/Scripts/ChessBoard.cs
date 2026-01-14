using System;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public enum SpecialMoves
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}


public class ChessBoard : MonoBehaviour
{
    //masterials and objects
    [Header("materials")]
    [SerializeField] private Material TileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float YOffset = 0.2f;
    [SerializeField] private Vector3 boardCentre = Vector3.zero;
    [SerializeField] private float CapturedSize = 0.3f;
    [SerializeField] private float CapturedSpacing = 0.3f;
    [SerializeField] private float DragOffset = 1.5f;
    [SerializeField] private GameObject WinningScreen;
    [SerializeField] private GameObject PromotionMenu;
    [SerializeField] private GameObject RematchIndicator;
    [SerializeField] private Button RematchButton;
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private AudioClip CheckSound;
    [SerializeField] private AudioClip MoveSound;
    [SerializeField] private AudioClip QueenTaken;
    



    [Header("Prefabs")]
    [SerializeField] private GameObject[] Prefabs;
    [SerializeField] private Material[] TeamMaterial;
    

    //logic
    private ChessPiece[,] Pieces;
    private ChessPiece PendingPromotionPawn;
    private List<Vector2Int> AvailableMoves = new List<Vector2Int>();
    private List<Vector2Int[]> MoveList = new List<Vector2Int[]>();
    private SpecialMoves specialMoves;
    private List<ChessPiece> CapturedWhites = new List<ChessPiece>();
    private List<ChessPiece> CapturedBlacks = new List<ChessPiece>();
    private ChessPiece CurrentlyDragging;
    private const int TileCount_X = 8;
    private const int TileCount_Y = 8;
    private GameObject[,] Tiles;
    private Camera CurrentCamera;
    private Vector2Int CurrentHover;
    private Vector3 Bounds;
    private bool IsWhiteTurn;
    private ChessPiece PendingPromotion = null;

    //network
    private int PlayerCount = -1;
    private int CurrentTeam = -1;
    private bool LocalGame;
    private bool[] PlayerRematch = new bool[2];



    private void Start()
    {
        AudioSource = GetComponent<AudioSource>();
        //sets white team to go first
        IsWhiteTurn = true;

        GenerateAllTiles(1, TileCount_X, TileCount_Y);

        SpawnAllPieces();
        PositionAllPieces();

        RegisterEvents();
    }

    private void Update()
    {
        Client.instance?.Update();

        // ff the pawn promotion menu is open, block all board input
        if (PromotionMenu != null && PromotionMenu.activeSelf)
            return;

        // makes sure we always have a reference to the active camera
        if (!CurrentCamera)
        {
            CurrentCamera = Camera.main;
            return;
        }

        // Raycast from the mouse position into the world
        RaycastHit info;
        Ray ray = CurrentCamera.ScreenPointToRay(Input.mousePosition);
        // Check if the mouse is over a board-related tile
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "HighLight")))
        { 
            if (CurrentlyDragging != null && Input.GetMouseButtonDown(1))
            {
                //right click cancels drag
                CancelDrag();
                return;
            }

            if (CurrentlyDragging != null &&  ((CurrentTeam == 0 && !IsWhiteTurn) || (CurrentTeam == 1 && IsWhiteTurn)))
            {
                //cancels drag if not players turn
            CancelDrag();
            return;
            }

            // Convert the hit object into board coordinates
            Vector2Int HitPosition = LookUpTileIndex(info.transform.gameObject);

            //if we are hovering over a tile after not hovering over a tile
            if (CurrentHover == -Vector2Int.one)
            {
                CurrentHover = HitPosition;

                Tiles[HitPosition.x, HitPosition.y].layer = LayerMask.NameToLayer("Hover");
               
            }

            //if the mouse moved to a different tile
            if (CurrentHover != HitPosition)
            {
                Tiles[CurrentHover.x, CurrentHover.y].layer = (ContainsValidMove(ref AvailableMoves, CurrentHover)) ? LayerMask.NameToLayer("HighLight") : LayerMask.NameToLayer("Tile");
                
                //sets the new hover tile
                CurrentHover = HitPosition;
                Tiles[CurrentHover.x, CurrentHover.y].layer = LayerMask.NameToLayer("Hover");
                
            }

            //picks up piece if left mouse clicked
            if (Input.GetMouseButtonDown(0))
            {
                if (Pieces[HitPosition.x, HitPosition.y] != null)
                {
                    //checks team and whos turn it is
                    if ((Pieces[HitPosition.x, HitPosition.y].Team == 0 && IsWhiteTurn && CurrentTeam == 0) || (Pieces[HitPosition.x, HitPosition.y].Team == 1 && !IsWhiteTurn && CurrentTeam == 1))
                    {
                        CurrentlyDragging = Pieces[HitPosition.x, HitPosition.y];
                        //get a list of where the player can go and hightligts tiles as well
                        AvailableMoves = CurrentlyDragging.GetAvailableMoves(ref Pieces, TileCount_X, TileCount_Y);

                        //get a list of special moves
                        specialMoves = CurrentlyDragging.GetSpecialMoves(ref Pieces, ref MoveList, ref AvailableMoves);

                        PreventCheck();

                        HighlightTiles();
                    }
                }
            }

            // moves chesspiece if its a legal move
            if (CurrentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int PreviousPosition = new Vector2Int(CurrentlyDragging.CurrentX, CurrentlyDragging.CurrentY);
                if (ContainsValidMove(ref AvailableMoves, new Vector2Int(HitPosition.x, HitPosition.y)))
                {
                    MoveTo(PreviousPosition.x, PreviousPosition.y, HitPosition.x, HitPosition.y);

                    //net implementation
                    NetMakeMove MM = new NetMakeMove();
                    MM.originalX = PreviousPosition.x;
                    MM.originalY = PreviousPosition.y;
                    MM.DestinationX = HitPosition.x;
                    MM.DestinationY = HitPosition.y;
                    MM.TeamID = CurrentTeam;
                    if (Client.instance != null && Client.instance.Connection.IsCreated)
                    {
                        Client.instance.SendToServer(MM);
                    }

                    ChessPiece movedPiece = Pieces[HitPosition.x, HitPosition.y];
                    if (movedPiece.PieceType == ChessPieceType.Pawn &&
                        (HitPosition.y == 0 || HitPosition.y == 7)) // last rank
                    {
                        PendingPromotion = movedPiece; // save for later
                        PromotionMenu.SetActive(true); // block input locally
                    }
                    else
                    {
                        CurrentlyDragging = null; // normal move
                        RemoveHighlightTiles();
                    }

                    CurrentlyDragging = null;
                    RemoveHighlightTiles();
                }
                else
                {
                    //snap piece back to original tile if not legal move
                    CurrentlyDragging.SetPosition(GetTileCentre(PreviousPosition.x, PreviousPosition.y), true);

                }
            }

            else
            {
                // If we are NOT hitting a tile

                // Only reset hover, DO NOT cancel drag
                if (CurrentHover != -Vector2Int.one)
                {
                    Tiles[CurrentHover.x, CurrentHover.y].layer = (ContainsValidMove(ref AvailableMoves, CurrentHover)) ? LayerMask.NameToLayer("HighLight") : LayerMask.NameToLayer("Tile");
                    CurrentHover = -Vector2Int.one;
                }

                // Only cancel dragging if mouse is released, NOT when clicking down
                if (CurrentlyDragging && Input.GetMouseButtonUp(0))
                {
                    CurrentlyDragging.SetPosition(GetTileCentre(CurrentlyDragging.CurrentX, CurrentlyDragging.CurrentY), true);
                    CurrentlyDragging = null;
                    RemoveHighlightTiles();
                }

            }

            //follow mouse position if piece is currently selected
            if (CurrentlyDragging)
            {
                Plane HorizontalPlane = new Plane(Vector3.up, Vector3.up * YOffset);
                float Distance = 0.0f;

                //moves piece smoothly
                if (HorizontalPlane.Raycast(ray, out Distance))
                    CurrentlyDragging.SetPosition(ray.GetPoint(Distance) + Vector3.up * DragOffset, true);
            }

        }
    }






    private void GenerateAllTiles(float TileSize, int TileCountX, int TileCountY)
    {
        YOffset += transform.position.y;
        Bounds = new Vector3((TileCountX / 2) * TileSize, 0, (TileCountY / 2) * TileSize) + boardCentre;

        Tiles = new GameObject[TileCountX, TileCountY];
        for (int x = 0; x < TileCountX; x++)
        {
            for (int y = 0; y < TileCountY; y++)
            {
                Tiles[x, y] = GenerateSingleTile(TileSize, x, y);
            }
        }
    }


    private GameObject GenerateSingleTile(float TileSize, int x, int y)
    {
        GameObject TileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        TileObject.transform.parent = transform;

        Mesh mesh = new Mesh();

        //ataches mesh to tiles
        TileObject.AddComponent<MeshFilter>().mesh = mesh;
        TileObject.AddComponent<MeshRenderer>().material = TileMaterial;



        //defines corner vertices of the tile
        Vector3[] Vertices = new Vector3[4];

        //the four corners from bottom left to top right
        Vertices[0] = new Vector3(x * TileSize, YOffset, y * TileSize) - Bounds;
        Vertices[1] = new Vector3(x * TileSize, YOffset, (y + 1) * TileSize) - Bounds;
        Vertices[2] = new Vector3((x + 1) * TileSize, YOffset, y * TileSize) - Bounds;
        Vertices[3] = new Vector3((x + 1) * TileSize, YOffset, (y + 1) * TileSize) - Bounds;

        //defines the two triangles in the quad
        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };


        // assigns geometry data to mesh
        mesh.vertices = Vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        TileObject.layer = LayerMask.NameToLayer("Tile");
        TileObject.AddComponent<BoxCollider>();



        return TileObject;
    }

    private void SpawnAllPieces()
    {
        //spawns all pieces at the start of the game
        Pieces = new ChessPiece[TileCount_X, TileCount_Y];

        int WhiteTeam = 0, BlackTeam = 1;

        //White team

        Pieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, WhiteTeam);
        Pieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, WhiteTeam);
        Pieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, WhiteTeam);
        Pieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, WhiteTeam);
        Pieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, WhiteTeam);
        Pieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, WhiteTeam);
        Pieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, WhiteTeam);
        Pieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, WhiteTeam);
        for (int i = 0; i < TileCount_X; i++)
            Pieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, WhiteTeam);

        //black team
        Pieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, BlackTeam);
        Pieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, BlackTeam);
        Pieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, BlackTeam);
        Pieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, BlackTeam);
        Pieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, BlackTeam);
        Pieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, BlackTeam);
        Pieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, BlackTeam);
        Pieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, BlackTeam);
        for (int i = 0; i < TileCount_X; i++)
            Pieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, BlackTeam);
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType Type, int Team)
    {
        ChessPiece CP = Instantiate(Prefabs[(int)Type - 1], transform).GetComponent<ChessPiece>();


        //assigns the piece type pawn or rook for example
        CP.PieceType = Type; 
        //assigns team so black or white
        CP.Team = Team;
        CP.GetComponent<MeshRenderer>().material = TeamMaterial[Team];

        return CP;
    }

    private void PositionAllPieces()
    {
        //lloops through every tile and assign chesspiece if it matches with coordinates
        for (int x = 0; x < TileCount_X; x++)
            for (int y = 0; y < TileCount_Y; y++)
                if (Pieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        //if no piece exists on tile do nothing
        if (Pieces[x, y] == null) return;
        
        //checks valid moves
        Pieces[x, y].CurrentX = x;
        Pieces[x, y].CurrentY = y;
        //set piece to middle of the tile
        Pieces[x, y].SetPosition(GetTileCentre(x, y), force);
    }

    private Vector3 GetTileCentre(int x, int y)
    {
        //returns the world position at the centre of the tiles
        return new Vector3(x * tileSize, YOffset, y * tileSize) - Bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    private void MoveTo(int OriginalX, int OriginalY, int x, int y)
    {
        //play sound if piece moves
        if (AudioSource != null && MoveSound != null)
        {
            AudioSource.PlayOneShot(MoveSound);  // Play the sound when a move is made
            
        }
    
        


    ChessPiece CP = Pieces[OriginalX, OriginalY];
        //if a piece exist in the tile being moved to then capture the piece currently occupying the square
        if (Pieces[x, y] != null)  
        {
            ChessPiece OtherCP = Pieces[x, y];

            //play special sound if queen gets captured
            if (OtherCP.PieceType == ChessPieceType.Queen)
            {
                if (OtherCP.Team == 0)
                {
                    
                    AudioSource.PlayOneShot(QueenTaken);
                }
                else
                {
                    AudioSource.PlayOneShot(QueenTaken);
                }
            }
        }

                //stores previous position
                Vector2Int PreviosPosition = new Vector2Int(OriginalX, OriginalY);

        if (Pieces[x, y] != null)
        {
            //is there another chesspiece in the new position
            //otherCP stands for other chesspiece
            ChessPiece OtherCP = Pieces[x, y];

            //if friendly team
            if (CP.Team == OtherCP.Team)
            {
                return;
            }

            //if enemy team
            if (OtherCP.Team == 0)
            {
                if (OtherCP.PieceType == ChessPieceType.King)
                    CheckMate(1);


                CapturedWhites.Add(OtherCP);
                OtherCP.SetScale(Vector3.one * CapturedSize, true);
                // places the pieces off the board with 7 being the width so you multiply it my 8 then get the bounds then it gets the centre of the square amd finally gets the direction of teh captured pieces ie up.
                OtherCP.SetPosition(new Vector3(8 * tileSize, YOffset, -1 * tileSize) - Bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * CapturedSpacing) * CapturedWhites.Count, true);

            }
            else
            {
                if (OtherCP.PieceType == ChessPieceType.King)
                    CheckMate(0);

                CapturedBlacks.Add(OtherCP);
                OtherCP.SetScale(Vector3.one * CapturedSize, true);
                OtherCP.SetPosition(new Vector3(-1 * tileSize, YOffset, 8 * tileSize) - Bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.back * CapturedSpacing) * CapturedBlacks.Count, true);
            }

            
        }
        //updates board array
        Pieces[x, y] = CP;
        Pieces[PreviosPosition.x, PreviosPosition.y] = null;
        //updates piece position
        PositionSinglePiece(x, y);

        IsWhiteTurn = !IsWhiteTurn;

        if (LocalGame)
            CurrentTeam = (CurrentTeam == 0) ? 1 : 0;

        MoveList.Add(new Vector2Int[] { PreviosPosition, new Vector2Int(x, y) });

        ProcessSpecialMoves();

        if(CheckForCheckmate())
            CheckMate(CP.Team);

        return;
    }

    private void CancelDrag()
    {
        //cancels drag and immediately snaps piece back to original position
        CurrentlyDragging.SetPosition(
            GetTileCentre(CurrentlyDragging.CurrentX, CurrentlyDragging.CurrentY),
            true
        );

        CurrentlyDragging = null;
        RemoveHighlightTiles();

        if (CurrentHover != -Vector2Int.one)
        {
            Tiles[CurrentHover.x, CurrentHover.y].layer = LayerMask.NameToLayer("Tile");
            CurrentHover = -Vector2Int.one;
        }
    }

    private void CheckMate(int Team)
    {
        //displays winning screen if checkmate detected
        DisplayWinningScreen(Team);
        
    }

    private void DisplayWinningScreen(int WinningTeam)
    {
        WinningScreen.SetActive(true);
        WinningScreen.transform.GetChild(WinningTeam).gameObject.SetActive(true);

    }

    public void OnRematchbutton()
    {
        
        if (LocalGame)
        {
            NetRematch LRM = new NetRematch();
            LRM.TeamID = CurrentTeam;
            LRM.WantRematch = 1;
            Client.instance.SendToServer(LRM);
            
        }
        else
        {
            NetRematch NRM = new NetRematch();
            NRM.TeamID = CurrentTeam;
            NRM.WantRematch = 1;
            Client.instance.SendToServer(NRM);
            Debug.Log("Rematch");
        }
    }

    public void GameReset()
    {
        //clears board and destroys current pieces and spawns new ones
        RematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
        RematchIndicator.transform.GetChild(1).gameObject.SetActive(false);

        WinningScreen.transform.GetChild(0).gameObject.SetActive(false);
        WinningScreen.transform.GetChild(1).gameObject.SetActive(false);
        WinningScreen.SetActive(false);

        CurrentlyDragging = null;
        AvailableMoves.Clear();
        MoveList.Clear();
        PlayerRematch[0] = PlayerRematch[1] = false;

        for (int x = 0; x < TileCount_X; x++)
        {
            for (int y = 0; y < TileCount_Y; y++)
            {
                if (Pieces[x, y] != null)
                    Destroy(Pieces[x, y].gameObject);

                Pieces[x, y] = null;
            }
        }

        for (int i = 0; i < CapturedWhites.Count; i++)
            Destroy(CapturedWhites[i].gameObject);

        for (int i = 0; i < CapturedBlacks.Count; i++)
            Destroy(CapturedBlacks[i].gameObject);

        CapturedWhites.Clear();
        CapturedBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        IsWhiteTurn = true;




    }

    public void OnMenuButton()
    {
        NetRematch RM = new NetRematch();
        RM.TeamID = CurrentTeam;
        RM.WantRematch = 1;
        Client.instance.SendToServer(RM);

        GameReset();
        GameUI.instance.OnLeaveFromGameMenu();

        Invoke("ShutdownRelay", 1.0f);

        PlayerCount = -1;
        CurrentTeam = -1;

    }
    public void OnPromotionSelected(ChessPieceType selectedType)
    {
        if (PendingPromotion == null) return;

        NetPromotion promoMsg = new NetPromotion();
        promoMsg.TeamID = CurrentTeam;
        promoMsg.NewType = selectedType;
        promoMsg.Position = new Vector2Int(PendingPromotion.CurrentX, PendingPromotion.CurrentY);

        if (Client.instance != null && Client.instance.Connection.IsCreated)
            Client.instance.SendToServer(promoMsg);

        PendingPromotion.PieceType = selectedType;
        PromotionMenu.SetActive(false);
        PendingPromotion = null;
    }


    private void ProcessSpecialMoves()
    {
        if (specialMoves == SpecialMoves.EnPassant)
        {
            var NewMove = MoveList[MoveList.Count - 1];
            ChessPiece MyPawn = Pieces[NewMove[1].x, NewMove[1].y];
            var TargetPawnPosition = MoveList[MoveList.Count - 2];
            ChessPiece EnemyPawn = Pieces[TargetPawnPosition[1].x, TargetPawnPosition[1].y];

            if (MyPawn.CurrentX == EnemyPawn.CurrentX)
            {
                if (MyPawn.CurrentY == EnemyPawn.CurrentY - 1 || MyPawn.CurrentY == EnemyPawn.CurrentY + 1)
                {
                    if (EnemyPawn.Team == 0)
                    {

                        CapturedWhites.Add(EnemyPawn);
                        EnemyPawn.SetScale(Vector3.one * CapturedSize, true);
                        // places the pieces off the board with 7 being the width so you multiply it my 8 then get the bounds then it gets the centre of the square amd finally gets the direction of teh captured pieces ie up.
                        EnemyPawn.SetPosition(new Vector3(8 * tileSize, YOffset, -1 * tileSize) - Bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * CapturedSpacing) * CapturedWhites.Count, true);
                    }
                    else
                    {
                        CapturedBlacks.Add(EnemyPawn);
                        EnemyPawn.SetScale(Vector3.one * CapturedSize, true);
                        EnemyPawn.SetPosition(new Vector3(-1 * tileSize, YOffset, 8 * tileSize) - Bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.back * CapturedSpacing) * CapturedBlacks.Count, true);

                    }
                    Pieces[EnemyPawn.CurrentX, EnemyPawn.CurrentY] = null;
                }
            }
        }

        if (specialMoves == SpecialMoves.Promotion)
        {
            Vector2Int[] LastMove = MoveList[MoveList.Count - 1];
            ChessPiece TargetPawn = Pieces[LastMove[1].x, LastMove[1].y];

            if (TargetPawn.PieceType == ChessPieceType.Pawn)
            {
                // Check if pawn reached promotion row
                bool promotionRowReached = (TargetPawn.Team == 0 && LastMove[1].y == 7) ||
                                           (TargetPawn.Team == 1 && LastMove[1].y == 0);

                if (promotionRowReached)
                {
                    // Store pawn reference to replace later
                    PendingPromotionPawn = TargetPawn;

                    // Show the promotion menu UI
                    PromotionMenu.SetActive(true);

                   
                }
            }
        }


        if (specialMoves == SpecialMoves.Castling)
        {
            var LastMove = MoveList[MoveList.Count - 1];

            // left rook 
            if (LastMove[1].x == 2)
            {
                if (LastMove[1].y == 0)//white side
                {
                    ChessPiece Rook = Pieces[0, 0];
                    Pieces[3, 0] = Rook;
                    PositionSinglePiece(3, 0);
                    Pieces[0, 0] = null;
                }
                else if (LastMove[1].y == 7)//black side
                {
                    ChessPiece Rook = Pieces[0, 7];
                    Pieces[3, 7] = Rook;
                    PositionSinglePiece(3, 7);
                    Pieces[0, 7] = null;
                }
            }

            //right rook
            else if (LastMove[1].x == 6)
            {

                if (LastMove[1].y == 0)//white side
                {
                    ChessPiece Rook = Pieces[7, 0];
                    Pieces[5, 0] = Rook;
                    PositionSinglePiece(5, 0);
                    Pieces[7, 0] = null;
                }
                else if (LastMove[1].y == 7)//black side
                {
                    ChessPiece Rook = Pieces[7, 7];
                    Pieces[5, 7] = Rook;
                    PositionSinglePiece(5, 7);
                    Pieces[7, 7] = null;
                }

            }
        }
    }

    public void PromotePieceByID(int pieceID)
    {
        RemoveHighlightTiles();
        if (PendingPromotionPawn == null) return;

        int x = PendingPromotionPawn.CurrentX;
        int y = PendingPromotionPawn.CurrentY;

        // converts int to enum
        ChessPieceType type = (ChessPieceType)pieceID;

        // spawns new piece
        ChessPiece NewPiece = SpawnSinglePiece(type, PendingPromotionPawn.Team);
        NewPiece.transform.position = PendingPromotionPawn.transform.position;

        // Assign to array 
        Pieces[x, y] = NewPiece;

        // positions it properly
        PositionSinglePiece(x, y);

        // destroys old pawn
        Destroy(PendingPromotionPawn.gameObject);
        PendingPromotionPawn = null;

        // hides promotion menu
        PromotionMenu.SetActive(false);
    }


    private void PreventCheck()
    {
        ChessPiece TargetKing = null;
        for(int x = 0; x < TileCount_X; x++)
            for(int y = 0; y < TileCount_Y; y++)
                if (Pieces[x, y] != null)
                if (Pieces[x,y].PieceType == ChessPieceType.King)
                    if (Pieces[x,y].Team == CurrentlyDragging.Team)
                        TargetKing = Pieces[x,y];

            //getting the available moves and deleting them if they put the palyer in check
           SimulateMoveForSinglePiece(CurrentlyDragging, ref AvailableMoves, TargetKing);
        
    }

    private void SimulateMoveForSinglePiece(ChessPiece CP,ref List<Vector2Int> Moves, ChessPiece TargetKing)
    {
        // save the current values to reset them after the function gets called
        int ActualX = CP.CurrentX;
        int ActualY = CP.CurrentY;
        List<Vector2Int> MovesToRemove = new List<Vector2Int>();

        // go through all the moves and find if any will put you in check

        for(int i = 0; i < Moves.Count; i++)
        {
            int SimX = Moves[i].x;
            int SimY = Moves[i].y;

            Vector2Int KingPositionThisMove = new Vector2Int(TargetKing.CurrentX, TargetKing.CurrentY);
            //was the king moved
            if(CP.PieceType == ChessPieceType.King)
                KingPositionThisMove = new Vector2Int(SimX, SimY);
            //copy the [,] not the reference
            ChessPiece[,] Simulate = new ChessPiece[TileCount_X, TileCount_Y];
            List<ChessPiece> AttackingPieces = new List<ChessPiece>();

            for(int x = 0; x < TileCount_X; x++)
            {
                for (int y = 0; y < TileCount_Y; y++)
                {
                    if (Pieces[x, y] != null)
                    {
                        Simulate[x, y] = Pieces[x, y];
                        if (Simulate[x, y].Team != CP.Team)
                            AttackingPieces.Add(Simulate[x, y]);
                    }
                }

            }

            //simulate move
            Simulate[ActualX, ActualY] = null;
            CP.CurrentX = SimX;
            CP.CurrentY = SimY;
            Simulate[SimX, SimY] = CP;

            //were one of the pieces get captured during the simulation
            var CapturedPieces = AttackingPieces.Find( c => c.CurrentX == SimX && c.CurrentY == SimY);
            if (CapturedPieces != null)
                AttackingPieces.Remove(CapturedPieces);

            List<Vector2Int> SimMove = new List<Vector2Int>();
            for(int a = 0; a < AttackingPieces.Count; a++)
            {
                var PieceMoves = AttackingPieces[a].GetAvailableMoves(ref Simulate, TileCount_X, TileCount_Y);
                for(int b = 0; b < PieceMoves.Count; b++)
                    SimMove.Add(PieceMoves[b]);
                
            }

            // if the king is in check remove the move
            if(ContainsValidMove(ref SimMove, KingPositionThisMove))
            {
                MovesToRemove.Add(Moves[i]);
            }

            //restore the actual chesspiece data
            CP.CurrentX = ActualX; 
            CP.CurrentY = ActualY;

        }





        // removes illegal moves from move list
        for(int i = 0; i < MovesToRemove.Count; i++)
        {
            Moves.Remove(MovesToRemove[i]);
        }
    }

    private bool CheckForCheckmate()
    {
        if (MoveList.Count == 0)
            return false; // No moves yet

        var LastMove = MoveList[MoveList.Count - 1];
        ChessPiece movedPiece = Pieces[LastMove[1].x, LastMove[1].y];

        if (movedPiece == null)
        {
            Debug.LogWarning("Last move points to a null piece. Skipping checkmate check.");
            return false;
        }

        int TargetTeam = (movedPiece.Team == 0) ? 1 : 0;

        List<ChessPiece> AttackingPiece = new List<ChessPiece>();
        List<ChessPiece> DefendingPiece = new List<ChessPiece>();
        ChessPiece TargetKing = null;

        for (int x = 0; x < TileCount_X; x++)
        {
            for (int y = 0; y < TileCount_Y; y++)
            {
                ChessPiece piece = Pieces[x, y];
                if (piece == null) continue;

                if (piece.Team == TargetTeam)
                {
                    DefendingPiece.Add(piece);
                    if (piece.PieceType == ChessPieceType.King)
                        TargetKing = piece;
                }
                else
                {
                    AttackingPiece.Add(piece);
                }
            }
        }

        if (TargetKing == null)
        {
            Debug.LogWarning("No king found for team " + TargetTeam);
            return false;
        }

        //is the king attacked right now
        List<Vector2Int> CurremtAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < AttackingPiece.Count; i++)
        {
            var PieceMoves = AttackingPiece[i].GetAvailableMoves(ref Pieces, TileCount_X, TileCount_Y);
            CurremtAvailableMoves.AddRange(PieceMoves);
        }

        //is the king in check right now
        if (ContainsValidMove(ref CurremtAvailableMoves, new Vector2Int(TargetKing.CurrentX, TargetKing.CurrentY)))
        {
            // if the king is in check can the player move
            for (int i = 0; i < DefendingPiece.Count; i++)
            {
                List<Vector2Int> DefendingMoves = DefendingPiece[i].GetAvailableMoves(ref Pieces, TileCount_X, TileCount_Y);
                SimulateMoveForSinglePiece(DefendingPiece[i], ref DefendingMoves, TargetKing);

                if (DefendingMoves.Count != 0)
                    return false;
            }
            return true; // checkmate exit
        }

        return false;
    }

    private void HighlightTiles()
    {
        for (int i = 0; i < AvailableMoves.Count; i++)
        {
            Tiles[AvailableMoves[i].x, AvailableMoves[i].y].layer = LayerMask.NameToLayer("HighLight");
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < AvailableMoves.Count; i++)
            Tiles[AvailableMoves[i].x, AvailableMoves[i].y].layer = LayerMask.NameToLayer("Tile");


        AvailableMoves.Clear();
    }

    private bool ContainsValidMove(ref List<Vector2Int> Moves, Vector2 Position)
    {
        for (int i = 0; i < Moves.Count; i++)
            if (Moves[i].x == Position.x && Moves[i].y == Position.y)
                return true;

        return false;
    }

    private Vector2Int LookUpTileIndex(GameObject HitInfo)
    {
        for (int x = 0; x < TileCount_X; x++)

            for (int y = 0; y < TileCount_Y; y++)

                if (Tiles[x, y] == HitInfo)
                    return new Vector2Int(x, y);

        return -Vector2Int.one; // -1, -1
    }

    #region

    private void RegisterEvents()
    {
        NetUtility.S_Welcome += OnWelcomeServer;
        NetUtility.S_Make_Move += OnMakeMoveServer;
        NetUtility.S_Rematch += OnRematchServer;

        NetUtility.C_Welcome += OnWelcomeClient;

        NetUtility.C_Start_Game += OnStartGameClient;
        NetUtility.C_Make_Move += OnMakeMoveClient;
        NetUtility.C_Rematch += OnRematchClient;

        GameUI.instance.SetLocalGame += SetLocalGame;
    }

    

    private void UnRegisterEvents()
    {
        NetUtility.S_Welcome -= OnWelcomeServer;
        NetUtility.S_Make_Move -= OnMakeMoveServer;
        NetUtility.S_Rematch -= OnRematchServer;

        NetUtility.C_Welcome -= OnWelcomeClient;

        NetUtility.C_Start_Game -= OnStartGameClient;
        NetUtility.C_Make_Move -= OnMakeMoveClient;
        NetUtility.C_Rematch -= OnRematchClient;

        GameUI.instance.SetLocalGame -= SetLocalGame;
    }

    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        //client has connected, assign a team and send the message back to them
        NetWelcome NW = msg as NetWelcome;

        //assign a team
        NW.AssignedTeam = ++PlayerCount;

        //return back to the client
        Server.instance.SendToClient(cnn, NW);

        //if full start the game
        if (PlayerCount == 1)
            Server.instance.Brodcast(new NetStartGame());
        
    }

    private void OnWelcomeClient(NetMessage msg)
    {
        //receive the connection message 
        NetWelcome NW = msg as NetWelcome;
        CurrentTeam = NW.AssignedTeam;

        Debug.Log($" My assigned team is {NW.AssignedTeam}");

        

        if (LocalGame && CurrentTeam == 0)
        {
            Server.instance.Brodcast(new NetStartGame());
        }
    }

    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        //receive and broadcast move back
        NetMakeMove MM = msg as NetMakeMove;

        Server.instance.Brodcast(msg);
        
    }

    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove MM = msg as NetMakeMove;

        Debug.Log($"MM : {MM.TeamID} : {MM.originalX} {MM.originalY} -> {MM.DestinationX} {MM.DestinationY}");

        if(MM.TeamID != CurrentTeam)
        {
            ChessPiece Target = Pieces[MM.originalX, MM.originalY];
            AvailableMoves = Target.GetAvailableMoves(ref Pieces, TileCount_X, TileCount_Y);
            specialMoves = Target.GetSpecialMoves(ref Pieces, ref MoveList, ref AvailableMoves);

            MoveTo(MM.originalX, MM.originalY, MM.DestinationX, MM.DestinationY);
        }
    }

    private void OnStartGameClient(NetMessage msg)
    {
        GameUI.instance.ChangeCamera(CurrentTeam == 0 ? CameraAngle.WhiteTeam : CameraAngle.BlackTeam);
    }

    private void OnRematchServer(NetMessage msg, NetworkConnection cnn)
    {
        Server.instance.Brodcast(msg);
    }

    private void OnRematchClient(NetMessage msg)
    {
        NetRematch RM = msg as NetRematch;

        PlayerRematch[RM.TeamID] = RM.WantRematch == 1;

        if(RM.TeamID != CurrentTeam)
                RematchIndicator.transform.GetChild((RM.WantRematch == 1) ? 0 : 1).gameObject.SetActive(true);
        if(RM.WantRematch != 1)
        {
            RematchButton.SetEnabled(false);
        }

        if (PlayerRematch[0] && PlayerRematch[1])
            GameReset();
        
    }

    private void ShutdownRelay()
    {
        Client.instance.Shutdown();
        Server.instance.Shutdown();
    }

    private void SetLocalGame(bool LG)
    {
        PlayerCount = -1;
        CurrentTeam = -1;
        LocalGame = LG;
    }

    #endregion
}


