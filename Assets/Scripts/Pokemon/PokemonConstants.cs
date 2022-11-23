using System;

public class PokemonConstants
{
    public enum Types
    {
        unknown,
        normal,
        fighting,
        flying,
        poison,
        ground,
        rock,
        bug,
        ghost,
        steel,
        fire,
        water,
        grass,
        electric,
        psychic,
        ice,
        dragon,
        dark,
        fairy,
        shadow
    }

    [Serializable]
    public class Stat
    {
        public string name = "";
        public int value = 0;
        public int effort = 0;
    }
}
