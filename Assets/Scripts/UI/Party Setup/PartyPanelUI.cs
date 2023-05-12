using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Facade;

public class PartyPanelUI : BaseFacePanelUI
{
    private void OnEnable()
    {
        partySetup.OnPartyStateChanged += UpdatePartyPanel;
    }

    private void OnDisable()
    {
        partySetup.OnPartyStateChanged -= UpdatePartyPanel;
    }

    public override void InitializeWithIndex(int index)
    {
        base.InitializeWithIndex(index);

        if (PlayerData.Party[panelIndex] == -1) return;
        var state = PlayerData.MaggotStates[PlayerData.Party[panelIndex]];
        StartCoroutine(LoadMaggot(state, state.GUID));

        inPartyCG.alpha = 1;
        SetActive(true);
    }

    protected void UpdatePartyPanel()
    {
        if (PlayerData.Party[panelIndex] == -1)
        {
            SetActive(false);
            return;
        }

        var state = PlayerData.MaggotStates[PlayerData.Party[panelIndex]];
        StartCoroutine(LoadMaggot(state, state.GUID));
    }

    public override void OnTap()
    {
        partySetup.TogglePartyStatusForMaggotAtIndex(PanelIndex);
    }

    public override void OnReleaseOverNoSlot()
    {
        if (PlayerData.Party[panelIndex] != -1)
        {
            partySetup.TogglePartyStatusForMaggotAtIndex(PlayerData.Party[panelIndex]);
        }
    }
}