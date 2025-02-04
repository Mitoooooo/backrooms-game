using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class LoadingScene : MonoBehaviour
    {
        public Animator endTransition;

        public IEnumerator LoadLevel(int sceneIndex)
        {
            endTransition.SetTrigger("End");

            yield return new WaitForSeconds(3f);

            // Load Menu scene
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
