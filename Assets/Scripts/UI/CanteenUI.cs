using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using static Facade;
using JSAM;
using System;

public class CanteenUI : BaseGameUI
{
    [Header("Animation Properties")]
    [SerializeField] float uiTweenTime = 0.35f;
    [SerializeField] float distanceFromCamera = 0;
    [SerializeField] float canteenShakeTime = 0.25f;
    [SerializeField] int canteenShakeVibrato = 10;
    [SerializeField] float canteenScaleTime = 0.075f;
    [SerializeField] Vector2 canteenShakeScale = new Vector2(150, 175);
    [SerializeField] float canteenMaxSpeed = 10;
    [SerializeField] float canteenSmoothTime = 0.5f;

    [SerializeField] float filledAnimTime = 0.15f;
    [SerializeField] Vector2 filledDestination;
    [SerializeField] float filledScaleTime = 0.1f;
    [SerializeField] Ease filledEase = Ease.InOutExpo;
    Vector2 filledOrigin;

    [Header("Object References")]
    [SerializeField] Canvas viewportBillboardCanvas = null;
    [SerializeField] TextMeshProUGUI canteenChargeText = null;
    [SerializeField] TextMeshProUGUI canteenCountText = null;
    [SerializeField] RectTransform canteen = null;
    [SerializeField] Image canteenFill = null;
    [SerializeField] Image filledIcon = null;
    [SerializeField] GameObject[] canteenIcons = null;
    [SerializeField] Renderer canteenRenderer = null;
    Transform canteenMesh { get { return canteenRenderer.transform.parent; } }

    bool queueUpdate = false;
    void QueueUpdate() => queueUpdate = true;

    private void OnEnable()
    {
        CanteenSystem.OnStoredChargeChanged += QueueUpdate;
        CanteenSystem.OnAvailableChargeChanged += QueueUpdate;
        CanteenSystem.OnSetCharge += OnSetCharge;

        BattleSystem.OnStartPlayerTurnLate += ShowUI;
        BattleSystem.OnStartPlayerTurnLate += UpdateUI;
        UIManager.OnEnterSkillTargetMode += HideUI;
        UIManager.OnExitSkillTargetMode += ShowUI;
        UIManager.OnShowBattleUI += ShowUI;
        UIManager.OnHideBattleUI += HideUI;

        filledOrigin = filledIcon.rectTransform.anchoredPosition;
        queueUpdate = true;
        UpdateUI();
        UpdateCanteenCount();
    }

    private void OnDisable()
    {
        CanteenSystem.OnStoredChargeChanged -= QueueUpdate;
        CanteenSystem.OnAvailableChargeChanged -= QueueUpdate;
        CanteenSystem.OnSetCharge -= OnSetCharge;

        BattleSystem.OnStartPlayerTurnLate -= ShowUI;
        BattleSystem.OnStartPlayerTurnLate -= UpdateUI;
        UIManager.OnEnterSkillTargetMode -= HideUI;
        UIManager.OnExitSkillTargetMode -= ShowUI;
        UIManager.OnShowBattleUI -= ShowUI;
        UIManager.OnHideBattleUI -= HideUI;
    }

    public override void ShowUI()
    {
        optimizedCanvas.Show();
        viewportBillboardCanvas.renderMode = RenderMode.ScreenSpaceCamera;
    }

    public override void HideUI()
    {
        optimizedCanvas.Hide();
        viewportBillboardCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    Vector3 canteenVelocity;
    private void Update()
    {
        if (canteenRenderer.enabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 target = ray.origin + ray.direction * distanceFromCamera;
            canteenMesh.position = Vector3.SmoothDamp(canteenMesh.position, target, 
                ref canteenVelocity, canteenSmoothTime);
            //canteenMesh.position = ray.origin + ray.direction * distanceFromCamera;
            canteenMesh.LookAt(ray.origin);
        }
    }

    private void OnSetCharge()
    {
        UpdateCanteenCount();
    }

    public void UpdateCanteenCount(int canteenCount = -1)
    {
        if (canteenCount == -1)
        {
            canteenCount = (int)(canteenSystem.AvailableCharge / canteenSystem.ChargePerCanteen);
        }

        canteenCountText.text = canteenCount.ToString();

        for (int i = 0; i < canteenCount; i++)
        {
            canteenIcons[i].SetActive(true);
        }

        for (int i = canteenCount; i < canteenIcons.Length; i++)
        {
            canteenIcons[i].SetActive(false);
        }
    }

    void UpdateUI()
    {
        if (canteenRoutine != null)
        {
            StopCoroutine(canteenRoutine);
        }
        canteenRoutine = StartCoroutine(CanteenRoutine());
    }

    float previousFill = 0;
    float previousCharge = 0;
    Coroutine canteenRoutine;
    IEnumerator CanteenRoutine()
    {
        if (!queueUpdate) yield return null;

        float currentFill = canteenSystem.AvailableCharge % canteenSystem.ChargePerCanteen;

        float fill = currentFill / canteenSystem.ChargePerCanteen;

        if (fill != previousFill)
        {
            float chargeDiff = canteenSystem.AvailableCharge - previousCharge;
            float progress = previousCharge;
            float fillProgress = canteenFill.fillAmount * canteenSystem.ChargePerCanteen;

            while (progress < canteenSystem.AvailableCharge)
            {
                float diff = canteenSystem.AvailableCharge - progress;
                float delta = Mathf.Min(diff, canteenSystem.ChargePerCanteen - fillProgress);
                float time = Mathf.InverseLerp(0, chargeDiff, delta) * uiTweenTime;
                if (diff == chargeDiff) time = uiTweenTime;

                canteenFill.DOFillAmount(Mathf.InverseLerp(0, canteenSystem.ChargePerCanteen, fillProgress + delta), time)
                    .OnUpdate(() =>
                    {
                        canteenChargeText.text = (canteenFill.fillAmount * canteenSystem.ChargePerCanteen * 100).ToString("0.##") + "%";
                    });

                fillProgress = Mathf.Repeat(fillProgress + delta, canteenSystem.ChargePerCanteen);

                yield return new WaitForSeconds(time);

                if (canteenFill.fillAmount == 1)
                {
                    filledIcon.enabled = true;
                    filledIcon.rectTransform.anchoredPosition = filledOrigin;

                    //filledIcon.rectTransform.DOSizeDelta(Vector3.one * 200, filledScaleTime);
                    filledIcon.rectTransform.DOAnchorPos(filledDestination, filledAnimTime).SetEase(filledEase);

                    yield return new WaitForSeconds(filledAnimTime);

                    //filledIcon.rectTransform.DOSizeDelta(Vector3.one * 125, filledScaleTime);

                    //yield return new WaitForSeconds(filledScaleTime);

                    filledIcon.enabled = false;

                    canteenFill.fillAmount = 0;
                    canteenChargeText.text = "0%";
                }

                progress += delta;
                UpdateCanteenCount();
            }

            canteen.DOPunchRotation(new Vector3(0, 0, 10), canteenShakeTime, canteenShakeVibrato);
            canteen.DOSizeDelta(Vector2.one * canteenShakeScale.y, canteenScaleTime);

            yield return new WaitForSeconds(canteenShakeTime);

            canteen.DOSizeDelta(Vector2.one * canteenShakeScale.x, canteenScaleTime);
        }

        previousFill = fill;
        previousCharge = canteenSystem.AvailableCharge;
        queueUpdate = false;
        canteenRoutine = null;
    }

    public void ReleaseCanteen()
    {
        if (!canteenRenderer.enabled) return;
        RaycastHit info;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool success = false;
        if (Physics.Raycast(ray, out info))
        {
            PlayerCharacter character;
            if (info.rigidbody)
            {
                if (info.rigidbody.TryGetComponent(out character))
                {
                    canteenSystem.BorrowCritCharge(character);
                    success = true;
                    AudioManager.PlaySound(BattleSceneSounds.CanteenUse);
                }
            }
        }

        if (!success)
        {
            canteenSystem.ReleaseCritCharge();
            AudioManager.PlaySound(BattleSceneSounds.CanteenDrop);
        }
        canteenRenderer.enabled = false;
        UpdateCanteenCount();
    }

    public void ShowCanteen()
    {
        if (canteenSystem.AvailableCharge < canteenSystem.ChargePerCanteen) return;
        canteenSystem.GrabCritCharge();
        canteenRenderer.enabled = true;
        AudioManager.PlaySound(BattleSceneSounds.CanteenGrab);
        UpdateCanteenCount();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 target = ray.origin + ray.direction * distanceFromCamera;
        canteenMesh.position = ray.origin + ray.direction * distanceFromCamera;
        canteenMesh.LookAt(ray.origin);
    }

    public void PostHackUpdate()
    {
        float currentFill = canteenSystem.AvailableCharge % canteenSystem.ChargePerCanteen;
        float fill = currentFill / canteenSystem.ChargePerCanteen;

        previousFill = fill;
        previousCharge = canteenSystem.AvailableCharge;

        UpdateCanteenCount();
    }
}