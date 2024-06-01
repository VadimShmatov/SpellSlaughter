using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutocastManager : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }

        public void EnterSettings()
    {
        Time.timeScale = 0f;
        gameObject.SetActive(true);
    }

    public void ExitSettings()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
