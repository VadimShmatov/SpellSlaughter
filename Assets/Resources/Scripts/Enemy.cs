using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private GameObject shatter_animation_;

    [SerializeField]
    private GameObject icicle_;

    private Animator animator;
    private Vector2 target = Vector2.zero;
    private float speed = 0.6f;
    private float despawn_interval = 1.0f;
    private float shatter_animation_despawn_interval = 0.5f;
    private float shatter_icicle_interval = 0.15f;
    private bool is_dead = false;
    private float freeze_duration = 0f;
    private float autodeath_timer = 0f;

    public bool is_targeted = false;

    public delegate void OnEnemyDeath(Enemy enemy);
    public static OnEnemyDeath on_enemy_death;

    public bool IsDead()
    {
        return is_dead;
    }

    public void Die()
    {
        if (is_dead) return;
        on_enemy_death?.Invoke(GetComponent<Enemy>());
        is_dead = true;
        animator.SetTrigger("isDead");
        GetComponent<Collider2D>().enabled = false;
        if (freeze_duration == 0f)
        {
            StartCoroutine(DespawnEnemy(despawn_interval));
        }
        else
        {
            Shatter();
        }
    }

    private void Shatter()
    {
        GameObject shatter_animation = Instantiate(shatter_animation_, transform.position, Quaternion.identity);
        StartCoroutine(SpawnIcicles());
        StartCoroutine(DespawnAnimation(shatter_animation, shatter_animation_despawn_interval));
        StartCoroutine(DespawnEnemy(despawn_interval));
    }

    public void Freeze(float duration)
    {
        if (freeze_duration == 0f && duration > 0f)
        {
            animator.SetBool("isFrozen", true);
        }
        if (duration > freeze_duration)
        {
            freeze_duration = duration;
        }
        float autoshatter_chance = UpgradeManager.Instance.GetParam("autoshatter_chance").value / 100f;
        for (int i = 0; i < Mathf.CeilToInt(freeze_duration); i++)
        {
            if (Random.Range(0f, 1f) < autoshatter_chance * Mathf.Min(1f, freeze_duration - i))
            {
                autodeath_timer = i + Random.Range(0f, Mathf.Min(1f, freeze_duration - i));
            }
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (is_dead)
        {
            return;
        }
        if (autodeath_timer > 0f)
        {
            autodeath_timer -= Time.deltaTime;
            if (autodeath_timer <= 0f)
            {
                Die();
                return;
            }
        }
        if (freeze_duration > 0f)
        {
            freeze_duration -= Time.deltaTime;
            if (freeze_duration <= 0f)
            {
                freeze_duration = 0f;
                animator.SetBool("isFrozen", false);
            }
            return;
        }
        Vector2 position = transform.position;
        Vector2 velocity = (target - position).normalized * speed;
        transform.position = Vector2.MoveTowards(position, target, speed * Time.deltaTime);
        animator.SetFloat("velocityX", velocity.x);
        animator.SetFloat("velocityY", velocity.y);
    }

    private IEnumerator SpawnIcicles()
    {
        yield return new WaitForSeconds(shatter_icicle_interval);
        GetComponent<Renderer>().enabled = false;
        int icicles_amount = Mathf.RoundToInt(UpgradeManager.Instance.GetParam("icicles_amount").value);
        for (int i = 0; i < icicles_amount; i++)
        {
            GameObject icicle = Instantiate(icicle_, transform.position, Quaternion.identity);
            icicle.GetComponent<MagicMissile>().SetDirection(Random.Range(-180, 180));
        }
    }

    private IEnumerator DespawnEnemy(float despawn_interval)
    {
        yield return new WaitForSeconds(despawn_interval);
        Destroy(gameObject);
    }

    private IEnumerator DespawnAnimation(GameObject animation, float despawn_interval)
    {
        yield return new WaitForSeconds(despawn_interval);
        Destroy(animation);
    }
}
