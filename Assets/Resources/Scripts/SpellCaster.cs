using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class SpellCaster : MonoBehaviour
{
    [SerializeField]
    private GameObject magic_missile_;

    [SerializeField]
    private GameObject firewall_spawner_;

    [SerializeField]
    private GameObject shatterwave_;

    [SerializeField]
    private GameObject chain_lightning_;

    [SerializeField]
    private float cast_interval_;

    private float lightning_buff_duration = 0f;

    void CastSpell(string spell_name)
    {
        switch (spell_name)
        {
            case "Hellfire":
                CastHellfire();
                break;
            case "Shatterwave":
                CastShatterwave();
                break;
            case "Lightningstorm":
                CastLightningStorm();
                break;
        }
    }

    public bool LightningBuffActive()
    {
        return lightning_buff_duration > 0f;
    }

    private void OnEnable()
    {
        SpellButton.on_spell_activation += CastSpell;
    }

    private void OnDisable()
    {
        SpellButton.on_spell_activation -= CastSpell;
    }

    private void Awake()
    {
        StartCoroutine(CastSpell());
    }

    private IEnumerator CastSpell()
    {
        yield return new WaitForSeconds(cast_interval_);
        lightning_buff_duration = Mathf.Max(lightning_buff_duration - cast_interval_, 0f);
        CastMagicMissile();
        StartCoroutine(CastSpell());
    }

    private void CastMagicMissile()
    {
        if (lightning_buff_duration > 0f)
        {
            Instantiate(chain_lightning_, transform.position, Quaternion.identity);
            return;
        }
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        GameObject nearest_zombie = null;
        float nearest_zombie_distance = float.MaxValue;
        foreach (GameObject zombie in zombies)
        {
            Enemy enemy = zombie.GetComponent<Enemy>();
            if (enemy == null || enemy.is_targeted || enemy.IsDead())
            {
                continue;
            }
            float distance = Vector2.Distance(zombie.transform.position, transform.position);
            if (distance < nearest_zombie_distance)
            {
                nearest_zombie = zombie;
                nearest_zombie_distance = distance;
            }
        }
        if (nearest_zombie != null)
        {
            GameObject missile = Instantiate(magic_missile_, transform.position, Quaternion.identity);
            missile.GetComponent<MagicMissile>().SetTarget(nearest_zombie);
            nearest_zombie.GetComponent<Enemy>().is_targeted = true;
        }
    }

    private IEnumerator RecastHellfire(int number_of_times)
    {
        yield return new WaitForSeconds(2f);
        CastHellfire(false);
        if (number_of_times > 0)
        {
            StartCoroutine(RecastHellfire(number_of_times - 1));
        }
    }

    private void CastHellfire(bool initial = true)
    {
        Vector2 position = Vector2.right * 0.01f;
        float angle = Random.Range(0, 360f);
        int number_of_spawners = 3;
        float more_chance = UpgradeManager.Instance.GetParam("chance_for_more_cracks").value / 100f;
        if (Random.Range(0f, 1f) < more_chance)
        {
            number_of_spawners += 2;
        }
        for (int i = 0; i < number_of_spawners; i++)
        {
            GameObject spell = Instantiate(firewall_spawner_, Quaternion.AngleAxis(angle, Vector3.forward) * position, Quaternion.identity);
            float speedup = UpgradeManager.Instance.GetParam("fire_first_split_speedup").value / 100f;
            spell.GetComponent<FireWallSpawner>().SpeedUpSplit(speedup);
            angle += 360f / number_of_spawners;
        }
        if (initial)
        {
            int number_of_recasts = Mathf.RoundToInt(UpgradeManager.Instance.GetParam("fire_recast_amount").value);
            if (number_of_recasts > 0)
            {
                StartCoroutine(RecastHellfire(number_of_recasts - 1));
            }
        }
    }

    private void CastShatterwave()
    {
        Instantiate(shatterwave_, transform.position, Quaternion.identity);
    }

    private void CastLightningStorm()
    {
        lightning_buff_duration = UpgradeManager.Instance.GetParam("lightning_buff_duration").value;
    }
}
