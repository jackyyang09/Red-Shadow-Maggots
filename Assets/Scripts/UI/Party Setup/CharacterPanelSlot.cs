using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPanelSlot : MonoBehaviour
{
    public enum CharacterSlotType
    {
        Party,
        Character,
        Enemy
    }

    public CharacterSlotType SlotType;

    BaseFacePanelUI characterPanel;
    public BaseFacePanelUI CharacterPanel => characterPanel;

    public bool Occupied => CharacterPanel;

    public void InitializeWithOccupant(BaseFacePanelUI occupant, int index)
    {
        OccupySlot(occupant);
        occupant.SetParentSlot(this);
        occupant.InitializeWithIndex(index);
    }

    public void OccupySlot(BaseFacePanelUI occupant)
    {
        characterPanel = occupant;
    }

    public void LeaveSlot(BaseFacePanelUI formerOccupant)
    {
        if (formerOccupant == CharacterPanel) characterPanel = null;
    }
}
