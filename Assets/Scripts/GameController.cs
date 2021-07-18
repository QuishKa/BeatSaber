using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ButtonPressed(string pressedButton)
    {
        switch (pressedButton)
        {
            case "start":
                break;
            case "edit":
                SceneManager.LoadScene(1);
                break;
            case "options":
                break;
            case "exit":
                Application.Quit();
                break;
        }
    }
}
