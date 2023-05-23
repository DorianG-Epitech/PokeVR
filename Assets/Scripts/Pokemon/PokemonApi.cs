using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using Newtonsoft.Json.Linq;

public class PokemonApi : MonoBehaviour
{
    #if UNITY_EDITOR
    // Endpoints
    public string ApiUrl = "https://pokeapi.co/api/v2/";
    public string PokemonEndpoint = "pokemon/";
    public string PokemonSpeciesEndpoint = "pokemon-species/";
    public string MoveEndpoint = "move/";

    // Ids
    public int MaxPokemonId = 151;
    public int MaxMoveId = 826;

    // Save folders
    public string MovesFolder = "Assets/Prefabs/Pokemons/Moves/";
    public string PokemonsFolder = "Assets/Prefabs/Pokemons/Data/";
    public string ModelsFolder = "Assets/Prefabs/Pokemons/Models/";
    public string SpritesFolder = "Assets/Textures/Pokemons/";

    public string[] FlyingPokemon;

    public void StartLoadPokemonsData()
    {
        StartCoroutine(LoadPokemonsData());
    }

    public void StartLoadMoveData()
    {
        StartCoroutine(LoadMovesData());
    }

    private IEnumerator LoadPokemonsData()
    {
        Debug.Log("Loading pokemons data...");

        for (int i = 1; i <= MaxPokemonId; i++)
        {
            StartCoroutine(GetRequest(ApiUrl + PokemonEndpoint + i,
                (string pokemon) => {
                    StartCoroutine(GetRequest(ApiUrl + PokemonSpeciesEndpoint + i,
                        (string pokemonSpecies) => {
                            string[] res = { pokemon, pokemonSpecies };
                            ParsePokemon(res);
                        }
                    ));
                }
            ));
            yield return new WaitForSeconds(1);
        }

        Debug.Log("Pokemons data loaded.");
    }

    private IEnumerator LoadMovesData()
    {
        Debug.Log("Loading moves data...");

        for (int i = 1; i <= MaxMoveId; i++)
        {
            StartCoroutine(GetRequest(ApiUrl + MoveEndpoint + i, ParseMove));
            yield return new WaitForSeconds(1);
        }

        Debug.Log("Moves data loaded.");
    }

    private void ParsePokemon(string[] pokemonJson)
    {
        dynamic pokemonData = JObject.Parse(pokemonJson[0].Replace("\\n", ""));
        PokemonDataSO pokemon = ScriptableObject.CreateInstance<PokemonDataSO>();
        
        var prefabName = CapitalizeFirstLetter((string)pokemonData.name);

        // Pokemon Id
        pokemon.Id = pokemonData.id;

        // Model
        string[] modelPath = AssetDatabase.FindAssets(prefabName + " t:prefab", new[] { ModelsFolder });
        if (modelPath.Length != 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(modelPath[0]);
            if (path != null && path != "")
            {
                UnityEngine.GameObject model = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)) as GameObject;
                pokemon.Model = model;
            }
        }
        else
            return;

        // Sprite
        string[] spritePath = AssetDatabase.FindAssets(prefabName + " t:texture2D", new[] { SpritesFolder });
        if (spritePath.Length != 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(spritePath[0]);
            if (path != null && path != "")
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
                pokemon.Sprite = sprite;
            }
        }
        else
            return;

        // Types
        List<PokemonConstants.Types> types = new List<PokemonConstants.Types>();
        foreach (JObject element in pokemonData.types)
        {
            types.Add(GetEnumValue<PokemonConstants.Types>((string)element["type"]["name"]));
        }
        pokemon.Types = types.ToArray();

        // Stats
        List<PokemonConstants.Stat> stats = new List<PokemonConstants.Stat>();
        foreach (JObject element in pokemonData.stats)
        {
            PokemonConstants.Stat stat = new PokemonConstants.Stat();
            stat.name = element["stat"]["name"].ToString();
            stat.value = (int)element["base_stat"];
            stat.effort = (int)element["effort"];
            stats.Add(stat);
        }
        pokemon.Stats = stats.ToArray();

        // Moves
        List<MoveDataSO> moves = new List<MoveDataSO>();
        foreach (JObject element in pokemonData.moves)
        {
            string[] results = AssetDatabase.FindAssets(element["move"]["name"].ToString());
            if (results.Length != 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(results[0]);
                if (path != null && path != "")
                {
                    MoveDataSO move = (MoveDataSO)AssetDatabase.LoadAssetAtPath(path, typeof(MoveDataSO));
                    if (move) moves.Add(move);
                }
            }
        }
        pokemon.Moves = moves.ToArray();

        dynamic pokemonSpeciesData = JObject.Parse(pokemonJson[1].Replace("\\n", ""));
        
        // Capture Rate
        pokemon.CaptureRate = pokemonSpeciesData.capture_rate;

        // Name
        JArray names = pokemonSpeciesData.names;
        pokemon.Name = names
            .Children<JObject>()
            .FirstOrDefault(obj => obj["language"]["name"].ToString() == "fr")["name"]
            .ToString();

        // IsFlying
        if (FlyingPokemon.Contains(prefabName)) pokemon.IsFlying = true;

        // Save the new scriptable object
        AssetDatabase.CreateAsset(pokemon, $"{PokemonsFolder}{prefabName}{".asset"}");
        AssetDatabase.SaveAssets();

        Debug.Log($"{"Create new pokemon : "}{prefabName}");
    }

    private void ParseMove(string moveJson)
    {
        dynamic moveData = JObject.Parse(moveJson.Replace("\\n", ""));
        MoveDataSO move = ScriptableObject.CreateInstance<MoveDataSO>();

        JArray names = moveData.names;
        var prefabName = names
            .Children<JObject>()
            .FirstOrDefault(obj => obj["language"]["name"].ToString() == "en")["name"]
            .ToString();

        // Move
        move.Id           = moveData.id;
        move.Accuracy     = GetIntValue(moveData.accuracy);
        move.EffectChance = GetIntValue(moveData.effect_chance);
        move.PP           = GetIntValue(moveData.pp);
        move.Priority     = GetIntValue(moveData.priority);
        move.Power        = GetIntValue(moveData.power);
        move.DamageClass  = GetEnumValue<MoveDataSO.MoveDamageClass>((string)moveData.damage_class.name);
        move.Target       = GetEnumValue<MoveDataSO.MoveTarget>(ToCamelCase((string)moveData.target.name));
        move.Type         = GetEnumValue<PokemonConstants.Types>(ToCamelCase((string)moveData.type.name));

        move.Name = names
            .Children<JObject>()
            .FirstOrDefault(obj => obj["language"]["name"].ToString() == "fr")["name"]
            .ToString();

        JArray flavorTextEntries = moveData.flavor_text_entries;
        move.Description = flavorTextEntries
            .Children<JObject>()
            .FirstOrDefault(obj => obj["language"]["name"].ToString() == "fr")["flavor_text"]
            .ToString();

        // Metadata
        move.Ailment       = GetEnumValue<MoveDataSO.MoveAilment>(ToCamelCase((string)moveData.meta.ailment.name));
        move.Category      = GetEnumValue<MoveDataSO.MoveCategory>(ToCamelCase((string)moveData.meta.category.name));
        move.MinHits       = GetIntValue(moveData.meta.min_hits);
        move.MaxHits       = GetIntValue(moveData.meta.max_hits);
        move.MinTurns      = GetIntValue(moveData.meta.min_turns);
        move.MaxTurns      = GetIntValue(moveData.meta.max_turns);
        move.Drain         = GetIntValue(moveData.meta.drain);
        move.Healing       = GetIntValue(moveData.meta.healing);
        move.CritRate      = GetIntValue(moveData.meta.crit_rate);
        move.AilmentChance = GetIntValue(moveData.meta.ailment_chance);
        move.FlinchChance  = GetIntValue(moveData.meta.flinch_chance);
        move.StatChance    = GetIntValue(moveData.meta.stat_chance);

        // StatChange
        List<PokemonConstants.Stat> StatChanges = new List<PokemonConstants.Stat>();
        foreach (JObject element in moveData.stat_changes)
        {
            PokemonConstants.Stat statToChange = new PokemonConstants.Stat();
            statToChange.name  = element["stat"]["name"].ToString();
            statToChange.value = (int)element["change"];
            StatChanges.Add(statToChange);
        }
        move.StatChanges = StatChanges.ToArray();

        // Save the new scriptable object
        AssetDatabase.CreateAsset(move, $"{MovesFolder}{moveData.name}{".asset"}");
        AssetDatabase.SaveAssets();

        Debug.Log($"{"Create new move : "}{prefabName}");
    }

    private IEnumerator GetRequest(string uri, Action<String> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] routes = uri.Split('/');
            int route = routes.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(routes[route] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(routes[route] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    callback(webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    private int GetIntValue(dynamic data)
    {
        return data != null ? data : -1;
    }

    private string CapitalizeFirstLetter(string text)
    {
        if (text.Length == 0)
            return "";
        else if (text.Length == 1)
            return char.ToUpper(text[0]).ToString();
        else
            return char.ToUpper(text[0]) + text.Substring(1);
    }

    private T GetEnumValue<T>(string name) where T : struct, IConvertible
    {
        return Enum.TryParse(name, true, out T val) ? val : default;
    }

    private string ToCamelCase(string str)
    {
        var words = str.Split(new[] { "-", " " }, StringSplitOptions.RemoveEmptyEntries);
        var leadWord = words[0].ToLower();
        var tailWords = words.Skip(1)
            .Select(word => char.ToUpper(word[0]) + word.Substring(1))
            .ToArray();
        return $"{leadWord}{string.Join(string.Empty, tailWords)}";
    }
    #endif
}
