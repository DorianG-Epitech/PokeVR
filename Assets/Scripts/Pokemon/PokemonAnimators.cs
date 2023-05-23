using System;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

public class PokemonAnimators : MonoBehaviour
{
    #if UNITY_EDITOR

    [Header("Setup")]
    public RuntimeAnimatorController pokemonAnimator;
    public string PokemonModelsPath = "Assets/Prefabs/Pokemons/Models";
    public string PokemonAnimationPath = "/Files/Animations/";

    [Space(10)]
    public AnimationOverride[] Animations;

    [Serializable]
    public struct AnimationOverride
    {
        public string Original;
        public string Override;
    }

    public void CreateAnimatorControllers()
    {
        var folders = AssetDatabase.GetSubFolders(PokemonModelsPath);
        foreach (var folder in folders)
        {
            CreateAnimatorController(folder);
        }
    }

    private void CreateAnimatorController(string pokemonPath)
    {
        Debug.Log(pokemonPath);

        string[] pathSplit = pokemonPath.Split('/');
        string pokemonName = pathSplit[pathSplit.Length - 1];

        AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(pokemonAnimator);

        foreach (var anim in Animations)
        {
            string[] results = AssetDatabase.FindAssets(anim.Override, new string[] {$"{pokemonPath}{PokemonAnimationPath}"});
            if (results.Length != 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(results[0]);
                if (path != null && path != "")
                {
                    AnimationClip clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));
                    animatorOverrideController[anim.Original] = clip;
                }
            } else
            {
                Debug.LogError("Missing animation for : " + pokemonName);
            }
        }
        
        AssetDatabase.CreateAsset(animatorOverrideController, $"{pokemonPath}{PokemonAnimationPath}{pokemonName}{"Animator.overrideController"}");

        GameObject pokemonPrefab = AssetDatabase.LoadAssetAtPath($"{PokemonModelsPath}{"/"}{pokemonName}{"/"}{pokemonName}{".prefab"}", typeof(GameObject)) as GameObject;
        pokemonPrefab.GetComponentInChildren<Animator>().runtimeAnimatorController = animatorOverrideController;
        SetLayerRecursively(pokemonPrefab, LayerMask.NameToLayer("Pokemon"));
        AssetDatabase.SaveAssets();

        Debug.Log("Override animator created.");
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
    
        foreach (var child in obj.transform.GetComponentsInChildren<Transform>())
        {
            if (child != obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
    #endif
}