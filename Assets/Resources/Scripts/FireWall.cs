using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWall : MonoBehaviour
{
    float despawn_interval_ = 3f;

    private void Awake()
    {
        StartCoroutine(Fade());
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
        }
    }

    private IEnumerator Fade()
    {
        float duration = UpgradeManager.Instance.GetParam("fire_duration").value;
        yield return new WaitForSeconds(duration);
        transform.Find("Fire").GetComponent<ParticleSystem>().Stop();
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(Despawn());
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(despawn_interval_);
        Destroy(gameObject);
    }
}
