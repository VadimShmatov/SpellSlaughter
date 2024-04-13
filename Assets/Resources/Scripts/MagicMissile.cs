using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissile : MonoBehaviour
{
    [SerializeField]
    private float speed;

    private float max_magnitude = 10.0f;
    private Vector2 direction;

    public void SetTarget(GameObject target)
    {
        Vector2 current_position = transform.position;
        Vector2 target_position = target.transform.position;
        direction = (target_position - current_position).normalized;
        transform.rotation = Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.right, direction), Vector3.forward);
    }

    public void SetDirection(float angle)
    {
        direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
        transform.rotation = Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.right, direction), Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Zombie")
        {
            Enemy zombie = collision.gameObject.GetComponent<Enemy>();
            if (zombie != null)
            {
                zombie.Die();
            }
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        Vector2 current_position = transform.position;
        transform.position = Vector2.MoveTowards(current_position, current_position + direction, speed * Time.deltaTime);
        if (transform.position.magnitude > max_magnitude)
        {
            Destroy(gameObject);
        }
    }
}
