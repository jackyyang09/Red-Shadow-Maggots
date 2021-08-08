using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MarketingControls : MonoBehaviour
{
    [SerializeField] KeyCode hideAllButton = KeyCode.Backslash;

    [Header("Character Showcase")]

    [SerializeField] KeyCode characterShowcaseButton = KeyCode.LeftBracket;

    [SerializeField] GameObject characterModel = null;
    [SerializeField] GameObject characterShowcaseObjects = null;

    [SerializeField] Animator rigAnim = null;

    [SerializeField] float tweenTime = 0.5f;

    [SerializeField] RectTransform characterGraphic = null;
    [SerializeField] Vector2 characterGraphicMoveRange = new Vector2();

    [SerializeField] RectTransform characterInfo = null;
    [SerializeField] Vector2 characterInfoMoveRange = new Vector2();

    //[SerializeField] ParticleSystem textParticles = null;
    //[SerializeField] Vector2 textParticleMoveRange = new Vector2();

    [SerializeField] RectMask2D mask = null;
    [SerializeField] Vector2 maskRange = new Vector2();

    [SerializeField] float continuedMovementSpeed = 5;

    [SerializeField] float continuedMoveTime = 5;

    [Header("End Card")]

    [SerializeField] KeyCode endCardButton = KeyCode.RightBracket;

    [SerializeField] GameObject title = null;

    [SerializeField] GameObject characterSplashes = null;

    [SerializeField] GameObject socialMediaItems = null;

    // Start is called before the first frame update
    void Start()
    {
        rigAnim.Play("Idle");
    }

    private void Update()
    {
        if (Input.GetKeyDown(characterShowcaseButton))
        {
            HideEndCard();
            StopAllCoroutines();
            StartCoroutine(DisplayCharacter());
        }
        else if (Input.GetKey(endCardButton))
        {
            HideCharacterShowCase();
            StopAllCoroutines();
            ShowEndCard();
        }
        else if (Input.GetKey(hideAllButton))
        {
            HideCharacterShowCase();
            HideEndCard();
        }
    }

    Coroutine characterDisplayRoutine = null;

    [ContextMenu("Show End Card")]
    public void ShowEndCard()
    {
        title.SetActive(true);
        characterSplashes.SetActive(true);
        socialMediaItems.SetActive(true);
    }

    [ContextMenu("Hide End Card")]
    public void HideEndCard()
    {
        title.SetActive(false);
        characterSplashes.SetActive(false);
        socialMediaItems.SetActive(false);
    }

    [ContextMenu("Hide Character Showcase")]
    public void HideCharacterShowCase()
    {
        characterModel.SetActive(false);
        characterShowcaseObjects.SetActive(false);
        characterInfo.gameObject.SetActive(false);
    }

    IEnumerator DisplayCharacter()
    {
        characterModel.SetActive(true);
        characterShowcaseObjects.SetActive(true);
        characterInfo.gameObject.SetActive(true);

        //textParticles.Play();

        mask.padding = new Vector4(0, 0, maskRange.x, 0);
        DOTween.To(() => mask.padding.z, x => mask.padding = new Vector4(0, 0, x, 0), maskRange.y, tweenTime).SetEase(Ease.Linear);

        //textParticles.transform.DOMoveX(textParticleMoveRange.x, 0);
        //textParticles.transform.DOMoveX(textParticleMoveRange.y, tweenTime).SetEase(Ease.Linear);

        characterGraphic.DOAnchorPosX(characterGraphicMoveRange.x, 0);
        characterGraphic.DOAnchorPosX(characterGraphicMoveRange.y, tweenTime);

        characterInfo.DOAnchorPosX(characterInfoMoveRange.x, 0);
        characterInfo.DOAnchorPosX(characterInfoMoveRange.y, tweenTime);

        yield return new WaitForSeconds(tweenTime);

        //textParticles.Stop();

        float timer = 0;
        while (timer < continuedMoveTime)
        {
            characterGraphic.anchoredPosition += new Vector2(continuedMovementSpeed * Time.deltaTime, 0);
            characterInfo.anchoredPosition += new Vector2(-continuedMovementSpeed * Time.deltaTime, 0);
            timer += Time.deltaTime;
            yield return null;
        }

        characterGraphic.DOAnchorPosX(-characterGraphicMoveRange.x, tweenTime);
        characterInfo.DOAnchorPosX(-characterInfoMoveRange.x, tweenTime);

        yield return null;
    }
}
