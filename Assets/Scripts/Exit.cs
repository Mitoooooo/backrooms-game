using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace Assets.Scripts
{
    public class Exit : MonoBehaviour, IInteractable
    {
        private GameObject interactOverlay;
        public string textInput;
        private List<GameObject> listGameObjects = new List<GameObject>();

        [Header("Text Settings")]
        [SerializeField] [TextArea] private string[] message;
        [SerializeField] private float textSpeed = 0.03f;

        [Header("UI Element")]
        private TMP_Text messageText;
        private int currentDisplayingText = 0;

        private GameObject panel;

        private GameObject gameManager;
        private GameObject entity;
        private LoadingScene loadingScene;

        private void Awake()
        {
            panel = GameObject.Find("ExitPanel");
            messageText = panel.GetComponentInChildren<TMP_Text>();

            interactOverlay = GameObject.Find("InteractText");

            panel.SetActive(false);

            // Find GameManager dynamically by tag, name, or type
            gameManager = GameObject.Find("GameManager");
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found in the scene!");
            }

            loadingScene = gameManager.GetComponentInChildren<LoadingScene>();

            listGameObjects.Add(gameManager);

            // Find Entity dynamically by tag, name, or type
            entity = GameObject.FindWithTag("Entity");
            if (entity == null)
            {
                Debug.LogError("Entity not found in the scene!");
            }

            listGameObjects.Add(entity);
        }

        public string GetText()
        {
            return textInput;
        }

        public void Interact()
        {
            DisableGameObjects();

            ShowMessageScreen();
        }

        void DisableGameObjects()
        {
            foreach (GameObject gameObject in listGameObjects) 
            { 
                gameObject.SetActive(false);
            }
        }

        void ShowMessageScreen()
        {
            ActivatePanel();

            StartCoroutine(ScrollingText());           
        }

        IEnumerator ScrollingText()
        {
            for(int i = 0; i < message[currentDisplayingText].Length + 1; i++)
            {
                messageText.text = message[currentDisplayingText].Substring(0, i);
                yield return new WaitForSeconds(textSpeed);
            }

            yield return new WaitForSeconds(5f);

            for (int i = 0; i < message[currentDisplayingText + 1].Length + 1; i++)
            {
                messageText.text = message[1].Substring(0, i);
                yield return new WaitForSeconds(textSpeed);
            }

            yield return new WaitForSeconds(5f);

            StartCoroutine(loadingScene.LoadLevel(0));
        }

        void ActivatePanel()
        {
            panel.SetActive(true);
        }
    }
}
