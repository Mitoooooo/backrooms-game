using Assets.Scripts.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpellCast : MonoBehaviour
{
    public Spell currentSpell;
    public UnityEvent onSpellCast = new UnityEvent();

    /*    //function to get information of currentspell
        public void GetCurrentSpell()
        {
            Fireball fireballScript = GetComponent<Fireball>();

            if (fireballScript != null)
            {
                Debug.Log("Fireball script is active.");
            }
            else
            {
                Debug.Log("Fireball script is not active or not found.");
            }
        }*/

    public void CastSpell()
    {
        //GetCurrentSpell();
        currentSpell.Cast();
        onSpellCast.Invoke();
    }
}
