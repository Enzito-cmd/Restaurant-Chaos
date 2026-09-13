using UnityEngine;

namespace RestaurantChaos.Clients
{
    [System.Serializable]
    public struct DaySetup
    {
        public DayDefinition definition;
        public LockableStation[] stationsToUnlock;
    }

    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        [Header("Days")]
        [SerializeField] private DaySetup[] days;

        [Header("References")]
        [SerializeField] private ClientSpawner spawner;
        [SerializeField] private LevelManager levelManager;

        private int currentDayIndex = -1;

        public int CurrentDayIndex => currentDayIndex;
        public bool HasNextDay => currentDayIndex + 1 < days.Length;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            StartDay(0);
        }

        public void AdvanceToNextDay()
        {
            if (!HasNextDay) return;

            StartDay(currentDayIndex + 1);
        }

        private void StartDay(int index)
        {
            if (index < 0 || index >= days.Length) return;

            DaySetup setup = days[index];

            if (setup.definition == null) return;

            currentDayIndex = index;

            if (setup.stationsToUnlock != null)
            {
                foreach (LockableStation station in setup.stationsToUnlock)
                {
                    if (station != null)
                    {
                        station.SetLocked(false);
                    }
                }
            }

            if (spawner != null)
            {
                spawner.SetAvailableEntries(setup.definition.clientEntries);
                spawner.StartSpawning();
            }

            if (levelManager != null)
            {
                levelManager.StartNewDay();
            }
        }
    }
}
