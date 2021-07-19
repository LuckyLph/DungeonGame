using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndMenuController : MonoBehaviour
{
    private MainController mainController;
    [SerializeField] private Text menuTitle;
    [SerializeField] private Text starsText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text livesText;

    private void Awake()
    {
        mainController = GameObject.FindWithTag("MainController").GetComponent<MainController>();
    }

    private void Start()
    {
        if (mainController.IsGameWon)
        {
            menuTitle.text = "Victory !";
            menuTitle.color = Color.green;
        }
        else
        {
            menuTitle.text = "Game Over !";
        }

        starsText.text = "Stars collected : " + mainController.PlayerStars.ToString();
        timerText.text = "Your time : " + ConvertGameTimerToString(mainController.GameTimer);

        if (mainController.PlayerLives == 0)
        {
            livesText.text = "You died !";
        }
        else if (mainController.PlayerLives == 1)
        {
            livesText.text = "1 life left";
        }
        else
        {
            livesText.text = mainController.PlayerLives.ToString() + " lives left";
        }
    }

    private string ConvertGameTimerToString(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
