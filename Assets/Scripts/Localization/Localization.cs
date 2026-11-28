using System;
using UnityEngine;

public enum Language
{
    English,
    Spanish
}

public static class Localization
{
    private const string PrefsKey = "Language";

    private static bool isLoaded;
    private static Language current;

    public static event Action Changed;

    public static Language Current
    {
        get
        {
            if (!isLoaded)
            {
                Load();
            }

            return current;
        }
    }

    public static void Set(Language language)
    {
        if (Current == language) return;

        current = language;

        PlayerPrefs.SetInt(PrefsKey, (int)language);
        PlayerPrefs.Save();

        Changed?.Invoke();
    }

    public static string Pick(string english, string spanish)
    {
        if (Current == Language.Spanish && !string.IsNullOrEmpty(spanish))
        {
            return spanish;
        }

        return english;
    }

    public static string[] Pick(string[] english, string[] spanish)
    {
        if (Current == Language.Spanish && spanish != null && spanish.Length > 0)
        {
            return spanish;
        }

        return english;
    }

    private static void Load()
    {
        isLoaded = true;
        current = Language.English;

        if (PlayerPrefs.HasKey(PrefsKey))
        {
            current = (Language)PlayerPrefs.GetInt(PrefsKey);
        }
        else if (Application.systemLanguage == SystemLanguage.Spanish)
        {
            current = Language.Spanish;
        }
    }
}
