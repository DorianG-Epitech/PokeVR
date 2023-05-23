using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string PlayerId;
    public static GameManager instance;

    void Awake()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
            return ;
        }
        DontDestroyOnLoad(gameObject);

        SoundManager.Initialize();
        SoundManager.PlayMusic(SoundManager.Sound.MenuMusic);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        GenerateId();
    }

    private void GenerateId()
    {
        PlayerId = System.Guid.NewGuid().ToString();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name) {
            case ("Lobby"):
                SoundManager.PlayMusic(SoundManager.Sound.MenuMusic);
                break;
            case ("Map"):
                SoundManager.PlayMusic(SoundManager.Sound.ExplorationMusic);
                SoundManager.PlayAmbiance(SoundManager.Sound.ExplorationAmbiance);
                break;
            case ("Multiplayer"):
                SoundManager.PlayMusic(SoundManager.Sound.BattleMusic);
                break;
        }
    }
}
