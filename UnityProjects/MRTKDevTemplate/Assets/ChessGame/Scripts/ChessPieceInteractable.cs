using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ChessPieceInteractable : MonoBehaviour
{
    private ChessPiece chessPiece;
    private Chessboard chessboard;
    private ObjectManipulator objectManipulator;

    private void Start()
    {
        chessPiece = GetComponent<ChessPiece>();
        chessboard = GetComponentInParent<Chessboard>();
        objectManipulator = GetComponent<ObjectManipulator>();

        if (objectManipulator != null)
        {
            objectManipulator.selectEntered.AddListener(OnGrabbed);
            objectManipulator.selectExited.AddListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        chessPiece.isBeingManipulated = true;
        chessboard.OnPieceGrabbed(chessPiece);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        chessPiece.isBeingManipulated = false;
        chessboard.OnPieceReleased(chessPiece);
    }

    private void OnDestroy()
    {
        if (objectManipulator != null)
        {
            objectManipulator.selectEntered.RemoveListener(OnGrabbed);
            objectManipulator.selectExited.RemoveListener(OnReleased);
        }
    }
}
