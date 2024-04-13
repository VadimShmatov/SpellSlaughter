using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FireWallSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject firewall_;

    float min_rotation = 10f;
    float max_rotation = 70f;
    float speed = 2f;
    float firewall_interval = 0.15f;
    float max_distance = 5.5f;
    float time_to_split;
    float time_to_spawn;
    Vector2 direction;

    public void SpeedUpSplit(float progress)
    {
        time_to_split *= 1f - progress;
    }

    private void Awake()
    {
        float min_split = UpgradeManager.Instance.GetParam("min_fire_split_time").value;
        float max_split = UpgradeManager.Instance.GetParam("max_fire_split_time").value;
        direction = transform.position.normalized;
        time_to_split = Random.Range(min_split, max_split);
        time_to_spawn = firewall_interval;
    }

    void RotateLeft()
    {
        direction = Quaternion.AngleAxis(Random.Range(min_rotation, max_rotation), Vector3.forward) * direction;
    }

    void RotateRight()
    {
        direction = Quaternion.AngleAxis(Random.Range(-min_rotation, -max_rotation), Vector3.forward) * direction;
    }

    private void Update()
    {
        Vector2 position = transform.position;
        transform.position = Vector2.MoveTowards(position, position + direction, speed * Time.deltaTime);
        time_to_spawn -= Time.deltaTime;


        if (position.magnitude >= max_distance)
        {
            Destroy(gameObject);
        }

        if (time_to_spawn < 0)
        {
            time_to_spawn = firewall_interval;
            Instantiate(firewall_, transform.position, Quaternion.identity);
        }

        time_to_split -= Time.deltaTime;
        if (time_to_split <= 0)
        {
            GameObject object1 = Instantiate(gameObject, transform.position, Quaternion.identity);
            object1.GetComponent<FireWallSpawner>().RotateLeft();
            GameObject object2 = Instantiate(gameObject, transform.position, Quaternion.identity);
            object2.GetComponent<FireWallSpawner>().RotateRight();
            Destroy(gameObject);
        }
    }


}
