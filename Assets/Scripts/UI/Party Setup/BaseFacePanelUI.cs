using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using static Facade;

public abstract class BaseFacePanelUI : MonoBehaviour
{
    [SerializeField] protected float minDragDistance;
    protected float dragDistance;
    protected bool dragging;
    [SerializeField] protected float minHoldTime;
    protected float holdTime;

    [SerializeField] protected OptimizedCanvas canvas;
    [SerializeField] protected new Collider2D collider;
    public void SetActive(bool active)
    {
        canvas.SetActive(active);
        collider.enabled = active;
    }

    protected CharacterPanelSlot parentSlot;

    protected List<CharacterPanelSlot> hoveredSlots = new List<CharacterPanelSlot>();
    protected CharacterPanelSlot ClosestHoveredSlot
    {
        get
        {
            float closestDist = float.MaxValue;
            CharacterPanelSlot closest = null;
            foreach (var slot in hoveredSlots)
            {
                var dist = Vector3.Distance(transform.position, slot.transform.position);
                if (dist < closestDist) 
                {
                    closest = slot;
                    closestDist = dist;
                }
            }
            return closest;
        }
    }

    [SerializeField] protected Image profileGraphic;
    [SerializeField] protected Image healthBar;

    [SerializeField] protected CanvasGroup inPartyCG;

    protected bool pointerDown;
    protected bool canMove;

    protected Vector3 lastMousePos;
    protected Vector3 dragOffset;

    protected int panelIndex;
    public int PanelIndex => panelIndex;

    public virtual void InitializeWithIndex(int index)
    {
        panelIndex = index;
    }

    protected virtual void Update()
    {
        if (!pointerDown) return;

        if (parentSlot.SlotType == CharacterPanelSlot.CharacterSlotType.Character)
        {
            //if (partySetup.IsPanelInParty(this)) canMove = false;
        }

        if (canMove)
        {
            transform.position = Input.mousePosition + dragOffset;
        }
        else
        {
            dragDistance += Vector3.Distance(Input.mousePosition, lastMousePos);
            if (dragDistance >= minDragDistance)
            {
                TryStartDrag();
            }

            holdTime += Time.deltaTime;

            if (holdTime >= minHoldTime)
            {
                // Open Character Showcase
            }
            lastMousePos = Input.mousePosition;
        }
    }

    protected virtual void TryStartDrag()
    {
        dragging = true;
        canMove = true;
    }

    protected virtual IEnumerator LoadMaggot(PlayerSave.MaggotState state, string GUID)
    {
        var op = Addressables.LoadAssetAsync<CharacterObject>(GUID);
        yield return op;
        gachaSystem.TryAddLoadedMaggot(op);
        healthBar.fillAmount = state.Health / (float)op.Result.GetMaxHealth(op.Result.GetLevelFromExp(state.Exp), false);
        profileGraphic.sprite = op.Result.headshotSprite;
        SetActive(true);
    }

    public void CopyTo(BaseFacePanelUI other)
    {
        other.profileGraphic.sprite = profileGraphic.sprite;
        other.healthBar.fillAmount = healthBar.fillAmount;
        other.panelIndex = panelIndex;
    }

    public void SetParentSlot(CharacterPanelSlot slot)
    {
        transform.SetParent(slot.transform);
        parentSlot = slot;
    }

    public void PointerDown()
    {
        pointerDown = true;
        dragging = false;
        transform.SetParent(partySetup.transform);
        dragOffset = transform.position - Input.mousePosition;
        lastMousePos = Input.mousePosition;
        dragDistance = 0;
        holdTime = 0;
    }

    public virtual void OnReleaseOverHoveredSlot() { }
    public virtual void OnReleaseOverNoSlot() { }

    public virtual void OnTap() { }

    public void PointerUp()
    {
        pointerDown = false;
        canMove = false;

        if (dragging)
        {
            if (ClosestHoveredSlot)
            {
                OnReleaseOverHoveredSlot();
            }
            else
            {
                OnReleaseOverNoSlot();
            }

            transform.SetParent(parentSlot.transform);
            transform.position = parentSlot.transform.position;
            dragging = false;
        }
        else
        {
            if (holdTime < minHoldTime)
            {
                OnTap();
            }
        }

        hoveredSlots.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody.TryGetComponent(out CharacterPanelSlot slot))
        {
            hoveredSlots.Add(slot);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.attachedRigidbody.TryGetComponent(out CharacterPanelSlot slot))
        {
            hoveredSlots.Remove(slot);
        }
    }
}
