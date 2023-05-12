using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Facade;

public class CharacterPanelUI : BaseFacePanelUI
{
    public override void InitializeWithIndex(int index)
    {
        base.InitializeWithIndex(index);

        if (index > -1)
        {
            var state = PlayerData.MaggotStates[panelIndex];
            StartCoroutine(LoadMaggot(state, state.GUID));
        }

        if (panelIndex == -1) SetActive(false);
    }

    private void OnEnable()
    {
        partySetup.OnPartyStateChanged += UpdateCharacterPanel;
    }

    private void OnDisable()
    {
        partySetup.OnPartyStateChanged -= UpdateCharacterPanel;
    }

    protected override void TryStartDrag()
    {
        if (PlayerData.Party.ToList().Contains(PanelIndex)) return;
        base.TryStartDrag();
    }

    public override void OnReleaseOverHoveredSlot()
    {
        if (ClosestHoveredSlot.SlotType != CharacterPanelSlot.CharacterSlotType.Party) return;
        partySetup.SetMaggotAtPartySlot(panelIndex, ClosestHoveredSlot);
    }

    public override void OnTap()
    {
        partySetup.TogglePartyStatusForMaggotAtIndex(PanelIndex);
    }

    protected void UpdateCharacterPanel()
    {
        var partyList = PlayerData.Party.ToList();
        inPartyCG.alpha = partyList.Contains(PanelIndex).ToInt();
    }
}