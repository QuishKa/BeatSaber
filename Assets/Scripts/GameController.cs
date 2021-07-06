using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum ButtonType { start, chooseTrack, editTrack, options, exit }
    public ButtonType buttonType;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ButtonPressed(Button.ButtonType buttonType)
    {
        switch (buttonType)
        {
            case Button.ButtonType.start:
                break;
            case Button.ButtonType.chooseTrack:
                break;
            case Button.ButtonType.editTrack:
                break;
            case Button.ButtonType.options:
                break;
            case Button.ButtonType.exit:
                Application.Quit();
                break;
        }
    }
}
