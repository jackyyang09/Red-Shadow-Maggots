using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Facade;

public class CharacterPanelUI : MonoBehaviour
{
    [SerializeField] CharacterPanelSlot parentSlot;

    [SerializeField] CharacterPanelSlot hoveredSlot;

    [SerializeField] Image healthBar;

    bool dragging;

    Vector3 dragOffset;

    // Update is called once per frame
    void Update()
    {
        if (!dragging) return;
        transform.position = Input.mousePosition + dragOffset;
    }

    public void InitializeWithMaggot(PlayerSave.MaggotState maggot)
    {

    }

    public void PointerDown()
    {
        dragging = true;
        transform.SetParent(partySetup.transform);
        dragOffset = transform.position - Input.mousePosition;
    }

    public void PointerUp()
    {
        if (hoveredSlot)
        {
            parentSlot.Unoccupy(this);
            parentSlot = hoveredSlot;
            parentSlot.OccupySlot(this);
        }

        transform.SetParent(parentSlot.transform);
        transform.position = parentSlot.transform.position;
        dragging = false;
    }

    public void SetParentSlot(CharacterPanelSlot slot)
    {
        parentSlot = slot;
        hoveredSlot = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody.TryGetComponent(out CharacterPanelSlot slot))
        {
            if (slot.Occupied) return;
            hoveredSlot = slot;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.attachedRigidbody.TryGetComponent(out CharacterPanelSlot slot))
        {
            if (hoveredSlot == slot) hoveredSlot = null;
        }
    }
}