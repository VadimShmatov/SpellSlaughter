using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float current_transformation;

    private float GetTransformation()
    {
        float multiplier = Camera.main.orthographicSize / Screen.height;
        return (Screen.width - Screen.height) * multiplier;
    }

    void Start()
    {
        current_transformation = GetTransformation();
        Camera.main.transform.Translate(current_transformation, 0, 0);
    }

    void Update()
    {
        float new_transformation = GetTransformation();
        if (new_transformation != current_transformation)
        {
            Camera.main.transform.Translate(new_transformation - current_transformation, 0, 0);
            current_transformation = new_transformation;
        }
    }
}
