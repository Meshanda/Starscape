using System.Collections.Generic;

public class CellCave
{
    public bool Collapsed;
    public List<Possibility> Possibilities; 

    public CellCave(List<Possibility> possibilities)
    {
        Collapsed = false;
        Possibilities = possibilities;
    }
}
