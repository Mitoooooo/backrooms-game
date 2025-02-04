using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Spells.Fireball
{
    public class Fireball : Spell
    {
        [SerializeField]
        public GameObject fireballPrefab;

        private GameObject player;
        private Camera playerCam;

        private void Start()
        {
            player = GameObject.Find("Player");
            playerCam = player.GetComponentInChildren<Camera>();
        }

        public override void Cast()
        {
            Vector3 position = player.transform.position + player.transform.forward * 5f;
            Vector3 direction = playerCam.transform.forward;
            float spellForce = 50f;
            GameObject fireball = Instantiate(fireballPrefab, position, Quaternion.LookRotation(direction));
            fireball.GetComponent<Rigidbody>().AddForce(direction * spellForce, ForceMode.Impulse);

            /*// Attach a script to the fireball to handle collisions
            FireballCollisionHandler collisionHandler = fireball.AddComponent<FireballCollisionHandler>();
            collisionHandler.explosionPrefab = explosionPrefab;*/
        }
    } 
}
