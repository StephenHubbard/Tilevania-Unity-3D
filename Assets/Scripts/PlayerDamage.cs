using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
   
    private void OnTriggerEnter2D(Collider2D other)
    {
        print("attack collider");
        var enemy = other.GetComponent<EnemyMovement>();
        if (other.gameObject.tag == "Enemy")
        {
            print("hit enemy");
            enemy.DealDamage(20f);
        }
    }
}
