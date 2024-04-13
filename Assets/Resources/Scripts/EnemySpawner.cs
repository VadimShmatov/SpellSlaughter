using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject enemy_;

    [SerializeField]
    private float radius_;

    [SerializeField]
    private float max_spawn_interval_;

    [SerializeField]
    private float min_spawn_interval_;

    [SerializeField]
    private float spawn_interval_factor_;
    private void Awake()
    {
        StartCoroutine(SpawnEnemy(max_spawn_interval_));
    }

    private IEnumerator SpawnEnemy(float spawn_interval)
    {
        yield return new WaitForSeconds(spawn_interval);
        float angle = Random.Range(0f, 2f * Mathf.PI);
        Instantiate(enemy_, new Vector3(Mathf.Sin(angle) * radius_, Mathf.Cos(angle) * radius_, 0), Quaternion.identity);
        StartCoroutine(SpawnEnemy(Mathf.Max(spawn_interval * spawn_interval_factor_, min_spawn_interval_)));
    }
}
