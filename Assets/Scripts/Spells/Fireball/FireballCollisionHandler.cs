using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Spells.Fireball
{
    public class FireballCollisionHandler : MonoBehaviour
    {
        public GameObject explosionPrefab;

        private void OnCollisionEnter(Collision collision)
        {
            // Instantiate the explosion prefab at the collision point
            GameObject explosion = Instantiate(explosionPrefab, collision.contacts[0].point, Quaternion.identity);

            // Deactivate the fireball's collider and rigidbody and mesh renderer
            gameObject.GetComponent<Collider>().enabled = false;
            gameObject.GetComponent<Rigidbody>().Sleep();
            gameObject.GetComponent<MeshRenderer>().enabled = false;

            // Destroy the explosion
            Helper.StartWaitingForSeconds(this, 2f, () =>
            {           
                Destroy(explosion);
                Destroy(gameObject);
            });
        }
    }
}
