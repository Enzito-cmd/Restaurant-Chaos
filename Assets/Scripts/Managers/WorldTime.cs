using UnityEngine;

public static class WorldTime
{
    private static bool isMenuPaused;
    private static bool isTutorialFrozen;

    public static bool IsMenuPaused => isMenuPaused;

    public static void SetMenuPaused(bool paused)
    {
        isMenuPaused = paused;
        Apply();
    }

    public static void SetTutorialFrozen(bool frozen)
    {
        isTutorialFrozen = frozen;
        Apply();
    }

    public static void Reset()
    {
        isMenuPaused = false;
        isTutorialFrozen = false;
        Apply();
    }

    private static void Apply()
    {
        if (isMenuPaused || isTutorialFrozen)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
