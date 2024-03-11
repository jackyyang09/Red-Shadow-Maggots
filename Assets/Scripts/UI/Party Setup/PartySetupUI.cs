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
        for (int i = 0; i < PlayerData.MaggotStates.Count && i < characterSlots.Length; i++)
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
        var node = mapSceneManager.NextFightNode;

        battleStateManager.ResetData();

        BattleData.Canteens = PlayerData.Canteens;
        BattleData.PlayerStates = new List<BattleState.PlayerState>();
        for (int i = 0; i < PlayerData.Party.Length; i++)
        {
            if (PlayerData.Party[i] == -1) continue;
            var newState = new BattleState.PlayerState();
            newState.Index = PlayerData.Party[i];
            if (PlayerData.MaggotStates[newState.Index] != null)
            {
                newState.Health = PlayerData.MaggotStates[newState.Index].Health;
            }
            // Cooldown skills by 1 turn because this is what the player would expect to happen
            newState.Cooldowns[0] = Mathf.Max(0, PlayerData.MaggotStates[newState.Index].SkillCoodowns[0] - 1);
            newState.Cooldowns[1] = Mathf.Max(0, PlayerData.MaggotStates[newState.Index].SkillCoodowns[1] - 1);
            BattleData.PlayerStates.Add(newState);
        }

        BattleData.RoomLevel = PlayerData.NodesTravelled - PlayerData.BattlesFought;
        BattleData.UseSpecialCam = new bool[node.waves.Count];

        for (int i = 0; i < node.waves.Count; i++)
        {
            BattleData.UseSpecialCam[i] = node.waves[i].UseSpecialCam;

            BattleData.EnemyGUIDs.Add(new List<string>());
            for (int j = 0; j < node.waves[i].Enemies.Length; j++)
            {
                string guid = "";
                if (node.waves[i].Enemies[j] != null)
                {
                    guid = node.waves[i].Enemies[j].AssetGUID;
                }
                BattleData.EnemyGUIDs[i].Add(guid);
            }
        }
        battleStateManager.SaveData();

        mapSceneManager.MoveToBattleScene();
    }
}