using System;
using System.Collections.Generic;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header ("Art related")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.5f; //tem que ser 50% do tamanho da escala 
    [SerializeField] private float deathSpacing = 0.5f; //conferir se o tamanho ta bom ou se ta muito perto
    [SerializeField] private float capturedYOffset = 0.8f; // subir a altura das peças quando capturadas
    //[SerializeField] private float dragOffset = 1.0f;
    [SerializeField] private GameObject victoryScreen;


    [Header ("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    // LOGIC
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Vector3 bounds;
    private bool isWhiteTurn;

    private void Awake()
    {
        isWhiteTurn = true;
        GenerateAllTiles(tileSize, 8, 8);
        SpawnAllPieces();
        PositionAllPieces();
        UpdateInteractableStates();
    }
    // MRTK Hand Interaction - Piece grabbed
    public void OnPieceGrabbed(ChessPiece piece)
    {
        if (!IsCorrectTurn(piece))
            return;

        currentlyDragging = piece;
        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
        HighlightTiles();
    }

    // MRTK Hand Interaction - Piece released
    public void OnPieceReleased(ChessPiece piece)
    {
        if (currentlyDragging == null)
            return;

        Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
        Vector2Int targetTile = WorldPositionToTile(piece.transform.position);

        bool validMove = false;
        if (targetTile.x >= 0 && targetTile.x < TILE_COUNT_X && targetTile.y >= 0 && targetTile.y < TILE_COUNT_Y)
            validMove = MoveTo(currentlyDragging, targetTile.x, targetTile.y);

        if (!validMove)
            currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));

        currentlyDragging = null;
        RemoveHighlightTiles();
    }

    public bool IsCorrectTurn(ChessPiece piece)
    {
        return (piece.team == 0 && isWhiteTurn) || (piece.team == 1 && !isWhiteTurn);
    }

    private Vector2Int WorldPositionToTile(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        localPos += bounds - new Vector3(tileSize / 2, 0, tileSize / 2);
        int x = Mathf.RoundToInt(localPos.x / tileSize);
        int y = Mathf.RoundToInt(localPos.z / tileSize);
        return new Vector2Int(x, y);
    }

    private void UpdateInteractableStates()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    var om = chessPieces[x, y].GetComponent<ObjectManipulator>();
                    if (om != null)
                        om.enabled = IsCorrectTurn(chessPieces[x, y]);
                }
            }
        }
    }


    //Board Generation
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        bounds = new Vector3 ((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x,y] = GenerateSingleTile(tileSize, x, y); 
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - bounds;

        int[] tris = new int[] {0, 1, 2, 1, 3, 2};

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer =  LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();


        return tileObject;
    }

    //Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0, blackTeam = 1;

        //White team
        chessPieces[0,0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1,0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2,0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3,0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4,0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5,0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6,0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7,0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i,1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        

        //Black team
        chessPieces[0,7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1,7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2,7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3,7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4,7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5,7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6,7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7,7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i,6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate (prefabs[(int) type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;
        if (team == 1)
            cp.transform.rotation = Quaternion.Euler(-90, 0, -180); //-90 é o padrao, ja vem assim, -180 é pra arrumar a direçao das peças pretas
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        // Add MRTK hand interaction components
        if (cp.GetComponent<BoxCollider>() == null)
            cp.gameObject.AddComponent<BoxCollider>();

        var om = cp.gameObject.AddComponent<ObjectManipulator>();
        om.AllowedManipulations = TransformFlags.Move;

        // Remove auto-added BoundsControl and its entire visual container
        var boundsControl = cp.GetComponent<BoundsControl>();
        if (boundsControl != null)
            DestroyImmediate(boundsControl);
        for (int i = cp.transform.childCount - 1; i >= 0; i--)
        {
            var child = cp.transform.GetChild(i);
            if (child.name.Contains("BoundingBox"))
                DestroyImmediate(child.gameObject);
        }

        // Enlarge collider for easier hand grabbing
        var collider = cp.GetComponent<BoxCollider>();
        if (collider != null)
            collider.size *= 2f;

        cp.gameObject.AddComponent<ChessPieceInteractable>();

        return cp;
    }

    //Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if(chessPieces[x,y] != null)
                    PositionSinglePiece(x,y,true);
    }
    private void PositionSinglePiece (int x, int y, bool force = false)
    {
        chessPieces[x,y].currentX = x;
        chessPieces[x,y].currentY = y;
        chessPieces[x,y].SetPosition(GetTileCenter(x,y), force); 
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3 (x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
    
    //Highlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        }
        availableMoves.Clear();
    }
    
    //Checkmate
    private void CheckMate(int team)
    {
        DisplayVictory(team);
    }
    private void DisplayVictory(int winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);

    }
    public void OnResetButton()
    {
        //UI
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        //Fields reset
        currentlyDragging = null;
        availableMoves = new List<Vector2Int>();
        
        //Clean up
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(chessPieces[x,y] != null)
                    Destroy(chessPieces[x, y].gameObject);

                chessPieces[x,y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);
        for (int i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);

        deadWhites.Clear();
        deadBlacks.Clear();

        //Pieces positioning
        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
        UpdateInteractableStates();
    }
    public void OnExitButton()
    {
        Application.Quit();
    }

    //Operations
    private Vector2Int LookupTileIndex (GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
        
        return -Vector2Int.one; //Invalid
    }
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if(!ContainsValidMove(ref availableMoves, new Vector2(x,y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        //Is there another piece on the target position?
        if (chessPieces [x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];
            if (cp.team == ocp.team)
                return false;

            //If it is the enemy team
            // Disable ObjectManipulator on captured piece so it can't be grabbed from sideline
            var capturedOM = ocp.GetComponent<ObjectManipulator>();
            if (capturedOM != null)
                capturedOM.enabled = false;

            if (ocp.team == 0)
            {
                if (ocp.type == ChessPieceType.King)
                    CheckMate(1);

                deadWhites.Add(ocp);
                ocp.SetScale(ocp.BaseScale * deathSize);
                ocp.SetPosition(
                    new Vector3(8 * tileSize, capturedYOffset, -1 * tileSize) // at the margin right of the board
                    - bounds + new Vector3(tileSize / 2, 0, tileSize /2) // center of the square
                    + (Vector3.forward * deathSpacing) * deadWhites.Count, true); // direction in which it goes
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                    CheckMate(0);
                deadBlacks.Add(ocp);
                ocp.SetScale(ocp.BaseScale * deathSize);
                ocp.SetPosition(
                    new Vector3(-1 * tileSize, capturedYOffset, 8 * tileSize) // at the margin right of the board
                    - bounds + new Vector3(tileSize / 2, 0, tileSize /2) // center of the square
                    + (Vector3.back * deathSpacing) * deadBlacks.Count, true); // direction in which it goes
            }
            
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x,y);
        isWhiteTurn = !isWhiteTurn;
        UpdateInteractableStates();
        return true;
    }
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if(moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
    }
}


