using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Facade;

public class PartySetupUI : BasicSingleton<PartySetupUI>
{
    [SerializeField] OptimizedCanvas canvas;

    [SerializeField] CharacterPanelSlot[] partySlots;
    [SerializeField] CharacterPanelSlot[] characterSlots;
    [SerializeField] CharacterPanelSlot[] enemySlots;

    [SerializeField] PartyPanelUI[] partyPanels;
    [SerializeField] EnemyPanelUI[] enemyPanels;

    List<CharacterPanelUI> characterPanels = new();

    [SerializeField] GameObject panelPrefab;

    public System.Action OnPartyStateChanged;

    [ContextMenu(nameof(Initialize))]
    public void Initialize()
    {
        for (int i = 0; i < PlayerData.Party.Length; i++)
        {
            partySlots[i].InitializeWithOccupant(partyPanels[i], i);
        }

        int slotsOccupied = 0;
        for (int i = 0; i < PlayerData.MaggotStates.Count; i++)
        {
            var newPanel = Instantiate(panelPrefab, characterSlots[slotsOccupied].transform).GetComponent<CharacterPanelUI>();
            characterPanels.Add(newPanel);
            characterSlots[slotsOccupied].InitializeWithOccupant(newPanel, i);
            slotsOccupied++;
        }

        for (int i = 0; i < enemyPanels.Length; i++)
        {
            enemySlots[i].InitializeWithOccupant(enemyPanels[i], i);
        }

        OnPartyStateChanged?.Invoke();

        canvas.Show();
    }

    public void TogglePartyStatusForMaggotAtIndex(int index)
    {
        var partyList = PlayerData.Party.ToList();
        var i = partyList.IndexOf(index);
        if (i > -1)
        {
            PlayerData.Party[i] = -1;
        }
        else
        {
            PlayerData.Party[partyList.IndexOf(-1)] = index;
        }
        OnPartyStateChanged?.Invoke();
    }

    public void SetMaggotAtPartySlot(int maggot, CharacterPanelSlot slot)
    {
        var partyIndex = slot.CharacterPanel.PanelIndex;
        PlayerData.Party[partyIndex] = maggot;
        OnPartyStateChanged?.Invoke();
    }

    public void SwapMaggotAtPartySlot(CharacterPanelSlot start, CharacterPanelSlot dest)
    {
        var startI = start.CharacterPanel.PanelIndex;
        var destI = dest.CharacterPanel.PanelIndex;
        var temp = PlayerData.Party[destI];
        PlayerData.Party[destI] = PlayerData.Party[startI];
        PlayerData.Party[startI] = temp;
        OnPartyStateChanged?.Invoke();
    }

    public void StartBattle()
    {
        mapSceneManager.MoveToBattleScene();
    }
}