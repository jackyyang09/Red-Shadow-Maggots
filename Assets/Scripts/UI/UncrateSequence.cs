using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UncrateSequence : MonoBehaviour
{
    [SerializeField] OptimizedCanvas canvas = null;

    [SerializeField] Image characterPortrait = null;
    [SerializeField] Image dropShadow = null;
    [SerializeField] Image[] stars = null;
    [SerializeField] TMPro.TextMeshProUGUI nameText = null;
    [SerializeField] Image[] classIcons = null;
    [SerializeField] Image classEmblem = null;
    [SerializeField] Sprite[] classEmblems = null;
    [SerializeField] Image background = null;
    [SerializeField] Sprite[] backgrounds = null;
    [SerializeField] Image speedLines = null;
    [SerializeField] TMPro.TextMeshProUGUI continueText = null;

    [SerializeField] Image blackout = null;
    [SerializeField] float exitTime = 1;

    [SerializeField] float enterTime = 1;
    [SerializeField] Ease easeType = Ease.Linear;
    [SerializeField] Ease speedEase = Ease.Linear;
    [SerializeField] float revealTime = 0.75f;

    [SerializeField] CharacterObject testCharacter = null;
    [SerializeField] Rarity testRarity = Rarity.Common;

    public static UncrateSequence instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UncrateTest();
            }
        }
    }
#endif

    [ContextMenu("Uncrate Test")]
    public void UncrateTest()
    {
        if (Application.isPlaying)
        {
            UncrateCharacter(testCharacter, testRarity);
        }
    }

    public void UncrateCharacter(CharacterObject character, Rarity rarity)
    {
        StartCoroutine(PlayUncrateSequence(character, rarity));
    }

    IEnumerator PlayUncrateSequence(CharacterObject character, Rarity rarity)
    {
        canvas.Show();

        continueText.DOFade(0, 0);

        background.raycastTarget = false;
        background.color = new Color().NewUniformColor(0.5f);
        background.sprite = backgrounds[(int)rarity];

        characterPortrait.sprite = character.sprite;
        characterPortrait.color = Color.black;
        dropShadow.sprite = character.sprite;

        RectTransform portrait = ((RectTransform)characterPortrait.transform.parent);
        portrait.anchoredPosition = new Vector2(0, 0);
        portrait.localScale = new Vector3().NewUniformVector3(5);
        portrait.DOScale(1, enterTime).SetEase(easeType);

        nameText.text = string.Empty;
        nameText.color = Color.clear;

        classEmblem.enabled = false;
        classEmblem.sprite = classEmblems[(int)rarity];

        speedLines.transform.localScale = new Vector3().NewUniformVector3(0);
        speedLines.transform.DOScale(0.7f, enterTime);
        speedLines.DOFade(0, 0);
        speedLines.DOFade(1, 0.5f).SetEase(speedEase);
        speedLines.DOFade(0, 0.5f).SetDelay(0.5f).SetEase(speedEase);

        for (int i = 0; i < classIcons.Length; i++)
        {
            classIcons[i].enabled = false;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = false;
        }

        yield return new WaitForSeconds(enterTime);

        var revealEase = Ease.OutExpo;
        var revealOffset = 300;

        background.DOColor(Color.white, revealTime).SetEase(revealEase);
        portrait.DOAnchorPosX(574, revealTime).SetEase(revealEase);

        characterPortrait.DOColor(Color.white, revealTime).SetEase(revealEase);

        nameText.text = character.characterName;
        nameText.DOColor(Color.white, revealTime).SetEase(revealEase);
        RectTransform nameRect = nameText.transform as RectTransform;
        float orig = nameRect.anchoredPosition3D.x;
        nameRect.anchoredPosition = new Vector2(orig + revealOffset, nameRect.anchoredPosition.y);
        nameRect.DOAnchorPosX(orig, revealTime).SetEase(revealEase);

        RectTransform emblemREct = classEmblem.transform as RectTransform;
        classEmblem.enabled = true;
        classEmblem.DOFade(0, 0);
        classEmblem.DOFade(1, revealTime).SetEase(revealEase);
        orig = emblemREct.anchoredPosition.x;
        emblemREct.anchoredPosition = new Vector2(orig + revealOffset, emblemREct.anchoredPosition.y);
        emblemREct.DOAnchorPosX(orig, revealTime).SetEase(revealEase);

        Image theIcon = classIcons[(int)character.characterClass];
        theIcon.enabled = true;
        theIcon.DOFade(1, revealTime).SetEase(revealEase);

        speedLines.transform.DOScale(2, revealTime).SetEase(Ease.OutCubic);
        speedLines.DOFade(0, 0);
        speedLines.DOFade(1, 0.5f).SetEase(speedEase);
        speedLines.DOFade(0, 0.5f).SetDelay(revealTime / 2).SetEase(speedEase);

        yield return new WaitForSeconds(revealTime);

        for (int i = 0; i <= (int)rarity; i++)
        {
            stars[i].enabled = true;
            stars[i].transform.localScale = new Vector3().NewUniformVector3(2);
            stars[i].transform.DOScale(1, 0.5f).SetEase(revealEase);
            yield return new WaitForSeconds(0.125f);
        }

        yield return new WaitForSeconds(0.5f);

        continueText.DOFade(1, 1.5f);

        background.raycastTarget = true;

        yield return null;
    }

    public void ExitUncrateSequence()
    {
        StartCoroutine("ExitSequence");
    }

    public IEnumerator ExitSequence()
    {
        float exitHalfTime = exitTime / 2;

        blackout.DOFade(0, 0);
        blackout.color = Color.black;
        blackout.DOFade(1, exitHalfTime);

        yield return new WaitForSeconds(exitHalfTime);

        canvas.Hide();
        CharacterPreviewUI.instance.ExitCardLoadMode();

        yield return new WaitForSeconds(exitHalfTime);

        blackout.DOFade(0, exitHalfTime);

        yield return null;
    }

    void EstablishSingletonDominance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // A unique case where the Singleton exists but not in this scene
            if (instance.gameObject.scene.name == null)
            {
                instance = this;
            }
            else if (!instance.gameObject.activeInHierarchy)
            {
                instance = this;
            }
            else if (instance.gameObject.scene.name != gameObject.scene.name)
            {
                instance = this;
            }
            Destroy(gameObject);
        }
    }
}
