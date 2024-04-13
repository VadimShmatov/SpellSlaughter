using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatterwaveSpell : MonoBehaviour
{
    private float duration = 2f;
    private float speed = 6f;
    private float max_radius = 5f;
    private bool expand = true;

    private void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        if (!expand)
        {
            return;
        }
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider.radius > max_radius)
        {
            expand = false;
            collider.enabled = false;
            return;
        }
        collider.radius += Time.deltaTime * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Zombie")
        {
            Enemy zombie = collision.gameObject.GetComponent<Enemy>();
            if (zombie != null)
            {
                float min_duration = UpgradeManager.Instance.GetParam("min_freeze_duration").value;
                float max_duration = UpgradeManager.Instance.GetParam("max_freeze_duration").value;
                zombie.Freeze(Random.Range(min_duration, max_duration));
            }
        }
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
