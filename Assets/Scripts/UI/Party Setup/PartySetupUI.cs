using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class PartySetupUI : BasicSingleton<PartySetupUI>
{
    [SerializeField] OptimizedCanvas canvas;

    [SerializeField] CharacterPanelSlot[] partySlots;
    [SerializeField] CharacterPanelSlot[] characterSlots;

    [SerializeField] CharacterPanelUI[] partyPanels;
    List<CharacterPanelUI> characterPanels = new();
    List<CharacterPanelUI> panelsInParty = new() { null, null, null };

    [SerializeField] GameObject panelPrefab;

    [ContextMenu(nameof(Initialize))]
    public void Initialize()
    {
        var party = new List<int>();

        for (int i = 0; i < PlayerData.Party.Length; i++)
        {
            if (PlayerData.Party[i] == -1)
            {
                partyPanels[i].SetActive(false);
                continue;
            }
            else
            {
                partyPanels[i].InitializeWithMaggot(PlayerData.Party[i]);
                partySlots[i].InitializeWithOccupant(partyPanels[i]);
                party.Add(PlayerData.Party[i]);
            }
        }

        int slotsOccupied = 0;
        for (int i = 0; i < PlayerData.MaggotStates.Count; i++)
        {
            bool inParty = party.Contains(i);
            var newPanel = Instantiate(panelPrefab, characterSlots[slotsOccupied].transform).GetComponent<CharacterPanelUI>();
            newPanel.InitializeWithMaggot(i);
            newPanel.InParty = inParty;
            characterPanels.Add(newPanel);
            characterSlots[slotsOccupied].InitializeWithOccupant(newPanel);
            slotsOccupied++;
            if (inParty)
            {
                panelsInParty[party.IndexOf(i)] = newPanel;
            }
        }

        canvas.Show();
    }

    public void TogglePartyStatus(CharacterPanelUI panel)
    {
        if (panelsInParty.Contains(panel))
        {
            int i = panelsInParty.IndexOf(panel);
            partyPanels[i].SetActive(false);
            panel.InParty = false;
            panelsInParty[i] = null;
        }
        else
        {
            int i = panelsInParty.IndexOf(null);
            if (i == -1) return;
            partyPanels[i].SetActive(true);
            panel.CopyTo(partyPanels[i]);
            panel.InParty = true;
            panelsInParty[i] = panel;
        }
    }

    public void StartBattle()
    {
        for (int i = 0; i < partyPanels.Length; i++)
        {
            if (!panelsInParty[i]) continue;
            PlayerData.Party[i] = partyPanels[i].MaggotIndex;
        }
        mapSceneManager.MoveToBattleScene();
    }
}