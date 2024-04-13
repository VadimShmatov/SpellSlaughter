using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ChainLightning : MonoBehaviour
{
    [SerializeField]
    private GameObject chain_lightning_animation_;

    private bool initial_hit = true;
    private float period = 0.1f;
    private float duration = 0.5f;
    private int chain_number;

    private void Awake()
    {
        StartCoroutine(Chain());
        int min_jumps = Mathf.RoundToInt(UpgradeManager.Instance.GetParam("min_lightning_jumps").value);
        int max_jumps = Mathf.RoundToInt(UpgradeManager.Instance.GetParam("max_lightning_jumps").value);
        chain_number = Random.Range(min_jumps, max_jumps + 1);
    }

    public void SetChains(int number)
    {
        chain_number = number;
        initial_hit = false;
    }

    private IEnumerator Chain()
    {
        yield return new WaitForSeconds(period);
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        GameObject nearest_zombie = null;
        float nearest_zombie_distance = float.MaxValue;
        float range = UpgradeManager.Instance.GetParam("lightning_jump_radius").value;
        foreach (GameObject zombie in zombies)
        {
            Enemy enemy = zombie.GetComponent<Enemy>();
            float distance = Vector2.Distance(zombie.transform.position, transform.position);
            if (!enemy.IsDead() && distance < nearest_zombie_distance && (initial_hit || distance < range))
            {
                nearest_zombie = zombie;
                nearest_zombie_distance = distance;
            }
        }
        if (nearest_zombie != null && chain_number > 0)
        {
            nearest_zombie.GetComponent<Enemy>().Die();
            Vector2 current_position = transform.position;
            Vector2 target_position = nearest_zombie.transform.position;
            float distance = Vector2.Distance(current_position, target_position);
            Vector2 direction = (target_position - current_position).normalized;
            float rotation = Vector2.SignedAngle(Vector2.right, direction);
            GameObject animation = Instantiate(chain_lightning_animation_, transform.position, Quaternion.AngleAxis(90 + rotation, Vector3.forward));
            SpriteRenderer renderer = animation.GetComponent<SpriteRenderer>();
            renderer.size = new Vector2(renderer.size.x, distance);
            animation.transform.Translate(0, -renderer.size.y / 2, 0);
            Destroy(animation, duration);
            transform.position = nearest_zombie.transform.position;
            initial_hit = false;
            chain_number--;
            float fork_chance = UpgradeManager.Instance.GetParam("lightning_fork_chance").value / 100f;
            if (Random.Range(0f, 1f) <  fork_chance)
            {
                Instantiate(gameObject).GetComponent<ChainLightning>().SetChains(chain_number);
            }
            StartCoroutine(Chain());
        }
        else
        {
            Destroy(gameObject, duration);
        }

    }
}
