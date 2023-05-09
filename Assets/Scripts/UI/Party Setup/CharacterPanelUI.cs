using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static Facade;

public class CharacterPanelUI : MonoBehaviour
{
    [SerializeField] float minDragDistance;
    float dragDistance;
    bool dragging;
    [SerializeField] float minHoldTime;
    float holdTime;

    [SerializeField] OptimizedCanvas canvas;
    [SerializeField] new Collider2D collider;
    public void SetActive(bool active)
    {
        canvas.SetActive(active);
        collider.enabled = active;
    }

    [SerializeField] CharacterPanelSlot parentSlot;

    [SerializeField] CharacterPanelSlot hoveredSlot;

    [SerializeField] Image profileGraphic;
    [SerializeField] Image healthBar;

    [SerializeField] CanvasGroup inPartyCG;
    public bool InParty
    {
        set
        {
            inPartyCG.alpha = value.ToInt();
        }
    }

    bool pointerDown;

    Vector3 lastMousePos;
    Vector3 dragOffset;

    int maggotIndex;
    public int MaggotIndex => maggotIndex;

    public void InitializeWithMaggot(int index)
    {
        maggotIndex = index;
        var state = PlayerData.MaggotStates[maggotIndex];
        StartCoroutine(LoadMaggot(state, state.GUID));
    }

    IEnumerator LoadMaggot(PlayerSave.MaggotState maggot, string GUID)
    {
        var op = Addressables.LoadAssetAsync<CharacterObject>(GUID);
        yield return op;
        gachaSystem.TryAddLoadedMaggot(op);
        healthBar.fillAmount = maggot.Health / (float)op.Result.GetMaxHealth(op.Result.GetLevelFromExp(maggot.Exp), false);
        profileGraphic.sprite = op.Result.headshotSprite;
    }

    public void InitializeWithEnemy(int index)
    {
        var GUID = BattleData.EnemyGUIDs[0][index];
        if (string.IsNullOrEmpty(GUID))
        {
            SetActive(false);
        }
        else
        {
            StartCoroutine(LoadEnemy(GUID));
        }
    }

    IEnumerator LoadEnemy(string GUID)
    {
        var op = Addressables.LoadAssetAsync<CharacterObject>(GUID);
        yield return op;
        gachaSystem.TryAddLoadedMaggot(op);
        healthBar.fillAmount = 1;
        profileGraphic.sprite = op.Result.headshotSprite;
        canvas.Raycaster.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pointerDown) return;

        if (dragging)
        {
            transform.position = Input.mousePosition + dragOffset;
        }
        else
        {
            dragDistance += Vector3.Distance(Input.mousePosition, lastMousePos);
            if (dragDistance >= minDragDistance) dragging = true;

            holdTime += Time.deltaTime;

            if (holdTime >= minHoldTime)
            {
                // Open Character Showcase
            }
            lastMousePos = Input.mousePosition;
        }
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

    public void PointerUp()
    {
        pointerDown = false;

        if (dragging)
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
        else
        {
            if (holdTime < minHoldTime)
            {
                partySetup.TogglePartyStatus(this);
            }
        }
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

    public void CopyTo(CharacterPanelUI other)
    {
        other.profileGraphic.sprite = profileGraphic.sprite;
        other.healthBar.fillAmount = healthBar.fillAmount;
        other.maggotIndex = maggotIndex;
    }
}