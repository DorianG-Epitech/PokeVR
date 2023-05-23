using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PokeballSlot : MonoBehaviour
{
    public OwnedPokeball pokeballPrefab;
    public OwnedPokeball pokeball;
    public Image PokemonSprite;

    private Canvas _tooltip;
    private TextMeshProUGUI _description;
    
    void Start()
    {
        _tooltip = GetComponentInChildren<Canvas>(true);
        _description = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void AddPokeballToSlot(PokemonData pokemonData)
    {
        var position = transform.position;
        pokeball = Instantiate(pokeballPrefab, new Vector3(position.x, position.y, position.z) , Quaternion.identity);
        var transform1 = pokeball.transform;
        transform1.parent = transform;
        transform1.localScale = new Vector3(1, 1, 1);
        pokeball.pokemon = pokemonData;
    }

    private void OnTriggerEnter(Collider other)
    {
        var isHand = other.transform.name == "GrabVolumeBig";
        if (pokeball && isHand)
        {
            _tooltip.gameObject.SetActive(true);
            _description.SetText(pokeball.pokemon.data.Name);
            PokemonSprite.sprite = pokeball.pokemon.data.Sprite;
        }
    }

    private void OnTriggerExit(Collider other)
    { 
        _tooltip.gameObject.SetActive(false);
    }

    void Update()
    {
        
    }
}
