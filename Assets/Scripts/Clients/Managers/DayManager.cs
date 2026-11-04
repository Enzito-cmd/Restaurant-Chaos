using System.Collections;
using RestaurantChaos.Tutorial;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    [System.Serializable]
    public struct DaySetup
    {
        public DayDefinition definition;
        public LockableStation[] stationsToUnlock;
        public TutorialSequence tutorial;
    }

    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        public static int RequestedDayNumber = 0;
        public static int RequestedTutorialNumber = 0;

        [Header("Days")]
        [SerializeField] private DaySetup[] days;

        [Header("References")]
        [SerializeField] private ClientSpawner spawner;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private ScreenFade fade;

        private int currentDayIndex = -1;
        private bool currentDayHasTutorial;
        private TutorialSequence currentTutorial;
        private PlayerHoldSystem playerHoldSystem;
        private PlayerInteraction playerInteraction;

        public int CurrentDayIndex => currentDayIndex;
        public bool HasNextDay => currentDayIndex + 1 < days.Length;
        public bool IsRestaurantOpen { get; private set; }
        public bool CanOpenRestaurant { get; private set; }
        public bool IsTutorialDay => currentDayHasTutorial;

        public bool CanUseStations
        {
            get
            {
                if (IsRestaurantOpen)
                {
                    return true;
                }

                return currentDayHasTutorial;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            OrderBoard.ClearAll();
            ClientRegistry.ClearAll();
        }

        private void Start()
        {
            int requestedDay = RequestedDayNumber;
            int requestedTutorial = RequestedTutorialNumber;

            RequestedDayNumber = 0;
            RequestedTutorialNumber = 0;

            if (requestedTutorial > 0)
            {
                StartDay(ResolveTutorialIndex(requestedTutorial));
                return;
            }

            StartDay(ResolveStartIndex(requestedDay));
        }

        public void AdvanceToNextDay()
        {
            if (!HasNextDay) return;

            SoundManager.Instance?.PlaySound(SoundType.ButtonClick);

            StartNextDay();
        }

        public void StartNextDay()
        {
            if (!HasNextDay) return;

            StartDay(currentDayIndex + 1);
        }

        public IEnumerator FadeToNextDay()
        {
            if (fade == null)
            {
                StartNextDay();
                yield break;
            }

            SetPlayerLocked(true);

            yield return fade.Out();

            StartNextDay();

            yield return fade.In();

            SetPlayerLocked(false);
        }

        public bool IsInteractionAllowed(IInteractable interactable)
        {
            return currentTutorial == null || currentTutorial.AllowsInteraction(interactable);
        }

        public void SetPlayerLocked(bool locked)
        {
            if (playerController != null)
            {
                playerController.SetMovement(!locked);
            }

            if (playerInteraction == null)
            {
                playerInteraction = FindFirstObjectByType<PlayerInteraction>();
            }

            if (playerInteraction != null)
            {
                playerInteraction.SetLocked(locked);
            }
        }

        public void SetOpeningAllowed(bool allowed)
        {
            CanOpenRestaurant = allowed;
        }

        public void OpenRestaurant()
        {
            if (!CanOpenRestaurant) return;
            if (IsRestaurantOpen) return;

            IsRestaurantOpen = true;
            SoundManager.Instance?.PlaySound(SoundType.DoorOpen);

            if (spawner != null)
            {
                spawner.StartSpawning();
            }
        }

        private int ResolveTutorialIndex(int tutorialNumber)
        {
            int tutorialsFound = 0;

            for (int i = 0; i < days.Length; i++)
            {
                if (days[i].tutorial == null)
                {
                    continue;
                }

                tutorialsFound++;

                if (tutorialsFound == tutorialNumber)
                {
                    return i;
                }
            }

            return 0;
        }

        private int ResolveStartIndex(int dayNumber)
        {
            if (dayNumber <= 0)
            {
                return 0;
            }

            int realDaysFound = 0;

            for (int i = 0; i < days.Length; i++)
            {
                if (days[i].tutorial != null)
                {
                    continue;
                }

                realDaysFound++;

                if (realDaysFound == dayNumber)
                {
                    return i;
                }
            }

            return 0;
        }

        private void StartDay(int index)
        {
            if (index < 0 || index >= days.Length) return;

            DaySetup setup = days[index];

            if (setup.definition == null) return;

            currentDayIndex = index;
            currentTutorial = setup.tutorial;

            ResetPlayerCameraAndMoney();
            UnlockStationsUpTo(index);
            ClearRestingPlates();
            ClearPlayerHands();

            bool hasTutorial = setup.tutorial != null;

            currentDayHasTutorial = hasTutorial;
            IsRestaurantOpen = false;
            CanOpenRestaurant = !hasTutorial;

            if (spawner != null)
            {
                spawner.PrepareDay(setup.definition);
            }

            if (levelManager != null)
            {
                levelManager.StartNewDay(!hasTutorial);
            }

            if (hasTutorial)
            {
                setup.tutorial.Begin();
            }
        }

        private void ResetPlayerCameraAndMoney()
        {
            if (playerController != null)
            {
                playerController.ResetToStart();
            }

            if (CameraLookLock.Instance != null)
            {
                CameraLookLock.Instance.ResetCamera();
            }

            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.ResetMoney();
            }
        }

        private void ClearRestingPlates()
        {
            PlateRest[] plateRests = FindObjectsByType<PlateRest>(FindObjectsSortMode.None);

            foreach (PlateRest plateRest in plateRests)
            {
                plateRest.ClearPlate();
            }
        }

        private void ClearPlayerHands()
        {
            if (playerHoldSystem == null)
            {
                playerHoldSystem = FindFirstObjectByType<PlayerHoldSystem>();
            }

            if (playerHoldSystem != null)
            {
                playerHoldSystem.ClearHeldItem();
            }

            ExtinguisherProp[] extinguishers = FindObjectsByType<ExtinguisherProp>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (ExtinguisherProp extinguisher in extinguishers)
            {
                extinguisher.ResetProp();
            }
        }

        private void UnlockStationsUpTo(int index)
        {
            for (int i = 0; i <= index; i++)
            {
                LockableStation[] stations = days[i].stationsToUnlock;

                if (stations == null)
                {
                    continue;
                }

                foreach (LockableStation station in stations)
                {
                    if (station != null)
                    {
                        station.SetLocked(false);
                    }
                }
            }
        }
    }
}
