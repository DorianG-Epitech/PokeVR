using UnityEngine;

public class PokemonAction : MonoBehaviour
{
    private PokemonEntity _pkmn;
    private PokemonEntity _target;
    
    public void OnAttack(string[] values)
    {
        if (values != null)
        {
            string name = values[0];
            Debug.Log(name);
            _pkmn = GameObject.Find(name).GetComponent<PokemonEntity>();
        }
        if (_pkmn != null)
            _pkmn.Attack(null);
        else
            print("Did not understand");
        _pkmn = null;
    }
    
    public void OnAttackTarget(string[] values)
    {
        Debug.Log(values[0]);
        Debug.Log(values[1]);
        Debug.Log(values[2]);
        if (values[0] != null)
            _pkmn = GameObject.Find(values[0]).GetComponent<PokemonEntity>();
        if (values[2] != null)
            _target = GameObject.Find(values[2]).GetComponent<PokemonEntity>();
        if (_pkmn != null && _target != null)
        {
            _pkmn.Attack(_target);
        }
        else
            print("Did not understand");
        _pkmn = null;
    }
}