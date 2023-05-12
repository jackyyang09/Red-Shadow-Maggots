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

    // Update is called once per frame
    void Update()
    {
        if (!pointerDown) return;

        if (canMove)
        {
            transform.position = Input.mousePosition + dragOffset;
        }
        else
        {
            dragDistance += Vector3.Distance(Input.mousePosition, lastMousePos);
            if (dragDistance >= minDragDistance)
            {
                dragging = true;
                canMove = true;
            }

            holdTime += Time.deltaTime;

            if (holdTime >= minHoldTime)
            {
                // Open Character Showcase
            }
            lastMousePos = Input.mousePosition;
        }
    }

    public override void OnReleaseOverHoveredSlot()
    {
        partySetup.SetMaggotAtPartySlot(panelIndex, hoveredSlot);
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