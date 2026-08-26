using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level 1");
    }
}