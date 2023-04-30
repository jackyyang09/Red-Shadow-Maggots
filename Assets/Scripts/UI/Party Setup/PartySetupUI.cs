using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class PartySetupUI : BasicSingleton<PartySetupUI>
{
    [SerializeField] OptimizedCanvas canvas;

    [SerializeField] CharacterPanelSlot[] partySlots;
    [SerializeField] CharacterPanelSlot[] characterSlots;

    [SerializeField] GameObject panelPrefab;

    [ContextMenu(nameof(Initialize))]
    public void Initialize()
    {
        var indicesToIgnore = new List<int>();

        for (int i = 0; i < PlayerData.Party.Length; i++)
        {
            if (PlayerData.Party[i] == -1) continue;
            //var newPanel = Instantiate(panelPrefab, partySlots[i].transform).GetComponent<CharacterPanelUI>();
            //newPanel
            //partySlots[i].InitializeWithOccupant(newPanel);
            indicesToIgnore.Add(PlayerData.Party[i]);
        }

        int slotsOccupied = 0;
        for (int i = 0; i < PlayerData.MaggotStates.Count; i++)
        {
            if (indicesToIgnore.Contains(i)) continue;
            var newPanel = Instantiate(panelPrefab, characterSlots[slotsOccupied].transform).GetComponent<CharacterPanelUI>();
            characterSlots[slotsOccupied].InitializeWithOccupant(newPanel);
            slotsOccupied++;
        }

        canvas.Show();
    }
}
