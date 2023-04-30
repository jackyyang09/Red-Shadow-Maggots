using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPanelSlot : MonoBehaviour
{
    CharacterPanelUI characterPanel;
    public CharacterPanelUI CharacterPanel => characterPanel;

    public bool Occupied => CharacterPanel;

    public void InitializeWithOccupant(CharacterPanelUI occupant)
    {
        OccupySlot(occupant);
        occupant.transform.SetParent(transform);
        occupant.SetParentSlot(this);
    }

    public void OccupySlot(CharacterPanelUI occupant)
    {
        characterPanel = occupant;
    }

    public void Unoccupy(CharacterPanelUI formerOccupant)
    {
        if (formerOccupant == CharacterPanel) characterPanel = null;
    }
}
