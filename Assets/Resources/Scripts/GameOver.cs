using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    private Vector2 target;
    void Awake()
    {
        target = Vector2.zero;
        gameOverPanel = GameObject.Find("GameOverPanel");
        gameOverPanel.SetActive(false);
    }
    private void Update()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject zombie in zombies)
        {
            if (Vector2.Distance(zombie.transform.position, target) <= 1.0f)
            {
                gameOverPanel.SetActive(true);
                Time.timeScale = 0.0f;
            }
        }

    }
    public void ToMainMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("MainMenu");
    }
}
