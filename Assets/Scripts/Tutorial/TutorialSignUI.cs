using System.Collections;
using TMPro;
using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public class TutorialSignUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text messageText;

        public static bool IsAnySignShowing { get; private set; }

        public bool IsShowing { get; private set; }

        private void Awake()
        {
            IsAnySignShowing = false;

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            IsAnySignShowing = false;
        }

        public IEnumerator ShowPages(string[] pages)
        {
            if (pages == null || pages.Length == 0)
            {
                yield break;
            }

            IsShowing = true;
            IsAnySignShowing = true;
            panel.SetActive(true);

            for (int i = 0; i < pages.Length; i++)
            {
                messageText.text = pages[i];

                yield return null;

                while (!WasClicked())
                {
                    yield return null;
                }
            }

            panel.SetActive(false);
            IsShowing = false;
            IsAnySignShowing = false;
        }

        private bool WasClicked()
        {
            return Input.GetMouseButtonDown(0) && Time.timeScale > 0f;
        }
    }
}
