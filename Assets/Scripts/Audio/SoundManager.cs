using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager {

    public enum Sound {
        ButtonClick,
        PokeballThrow,
        PokeballHit,
        PokeballOpen,
        PokeballWiggle,
        PokemonCatch,
        ExplorationAmbiance,
        MenuMusic,
        ExplorationMusic,
        BattleMusic
    }

    private static Dictionary<Sound, float> soundTimerDictionary;

    private static GameObject oneShotGameObject;
    private static AudioSource oneShotAudioSource;

    private static GameObject musicGameObject;
    private static AudioSource musicAudioSource;

    private static GameObject ambianceGameObject;
    private static AudioSource ambianceAudioSource;

    public static void Initialize() {
        soundTimerDictionary = new Dictionary<Sound, float>();
        //soundTimerDictionary[Sound.PlayerMove] = 0f;
    }

    public static void PlaySound(Sound sound, Vector3 position) {
        if (CanPlaySound(sound)) {
            GameObject soundGO = new GameObject("Sound");
            soundGO.transform.position = position;
            AudioSource audioSource = soundGO.AddComponent<AudioSource>();
            audioSource.clip = GetAudioClip(sound);
            audioSource.maxDistance = 100;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.dopplerLevel = 0f;
            audioSource.Play();

            Object.Destroy(soundGO, audioSource.clip.length);
        }
    }

    public static void PlaySound(Sound sound) {
        if (CanPlaySound(sound)) {
            if (oneShotGameObject == null) {
                oneShotGameObject = new GameObject("Sound Object");
                oneShotAudioSource = oneShotGameObject.AddComponent<AudioSource>();
            }
            oneShotAudioSource.PlayOneShot(GetAudioClip(sound));
        }
    }

    public static void PlayAmbiance(Sound sound) {
        if (CanPlaySound(sound)) {
            if (ambianceGameObject == null) {
                ambianceGameObject = new GameObject("Ambiance Object");
                ambianceAudioSource = musicGameObject.AddComponent<AudioSource>();
                ambianceAudioSource.loop = true;
            }
            ambianceAudioSource.clip = GetAudioClip(sound);
            ambianceAudioSource.Play();
        }
    }

    public static void PlayMusic(Sound sound) {
        if (CanPlaySound(sound)) {
            if (musicGameObject == null) {
                musicGameObject = new GameObject("Music Object");
                musicAudioSource = musicGameObject.AddComponent<AudioSource>();
                musicAudioSource.loop = true;
            }
            musicAudioSource.clip = GetAudioClip(sound);
            musicAudioSource.Play();
        }
    }

    private static bool CanPlaySound(Sound sound) {
        switch (sound) {
            //case Sound.PlayerMove:
            //    if (soundTimerDictionary.ContainsKey(sound)) {
            //        float lastTimePlayed = soundTimerDictionary[sound];
            //        float playerMoveTimerMax = 0.15f;
            //        if (lastTimePlayed + playerMoveTimerMax < Time.time) {
            //            soundTimerDictionary[sound] = Time.time;
            //            return true;
            //        } else {
            //            return false;
            //        }
            //    } else {
            //        return true;
            //    }
            default:
                return true;
        }
    }

    private static AudioClip GetAudioClip(Sound sound) {
        foreach (AudioAssets.SoundAudioClip soundAudioClip in AudioAssets.i.soundAudioClipArray) {
            if (soundAudioClip.sound == sound) {
                return soundAudioClip.audioClip;
            }
        }
        return null;
    }

    public static void AddButtonSounds(this IButton button) {
        //button.OnClick += () => SoundManager.PlaySound(Sound.ButtonClick);
    }
}
