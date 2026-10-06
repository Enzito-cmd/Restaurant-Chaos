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

        [Header("Days")]
        [SerializeField] private DaySetup[] days;

        [Header("References")]
        [SerializeField] private ClientSpawner spawner;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private PlayerController playerController;

        private int currentDayIndex = -1;
        private bool currentDayHasTutorial;

        public int CurrentDayIndex => currentDayIndex;
        public bool HasNextDay => currentDayIndex + 1 < days.Length;
        public bool IsRestaurantOpen { get; private set; }
        public bool CanOpenRestaurant { get; private set; }

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
            RequestedDayNumber = 0;

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
            playerController.transform.position = playerController.startPosition;
            playerController.transform.rotation = playerController.startRotation;
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

            UnlockStationsUpTo(index);
            ClearRestingPlates();

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

        private void ClearRestingPlates()
        {
            PlateRest[] plateRests = FindObjectsByType<PlateRest>(FindObjectsSortMode.None);

            foreach (PlateRest plateRest in plateRests)
            {
                plateRest.ClearPlate();
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
