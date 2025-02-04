using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour, IDamageable
{
    public double healthPoint;

    public void Damage(double damage)
    {
        //subtract damage amount when Damage function is called
        healthPoint -= damage;
        Debug.Log("HP: " + healthPoint);

        //Check if health has fallen below zero
        if (healthPoint <= 0)
        {
            //if health has fallen below zero, deactivate it 
            gameObject.SetActive(false);
        }
    }
}
