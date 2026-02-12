using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}
public class ChessPiece : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;
    public bool isBeingManipulated;

    private Vector3 desiredPosition;
    //private Vector3 desiredScale = Vector3.one;
    
    //Problema de escala
    private Vector3 desiredScale; //Deixei assim porque ele tava forçando a escala das peças, entao dava erro na hora de iniciar (saia da escala 26 padrao do prefab pra escala de 1 na hora do Play)
    public Vector3 BaseScale { get; private set; } // Para resolver o problema de escala quando as peças forem comidas

    //Adicionado para resolver o problema de escala causado pelo Vector3.Lerp
    private void Awake()
    {
        BaseScale = transform.localScale;
        desiredScale = BaseScale;
    }

    private void Update()
    {
        if (!isBeingManipulated)
            transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));

        return r;
    }
    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
        {
            transform.localPosition = desiredPosition;
        }
    }
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }

}
