using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button : MonoBehaviour, IPointerDownHandler
{
    public enum ButtonType { start, chooseTrack, editTrack, options, exit }
    [SerializeField] private ButtonType buttonType;
    private GameController gameController;

    void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }
    // let GameController process the event
    public void OnPointerDown(PointerEventData eventData)
    {
        gameController.ButtonPressed(buttonType);
    }
}
