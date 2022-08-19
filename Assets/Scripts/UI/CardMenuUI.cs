using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMenuUI : MonoBehaviour
{
    [SerializeField] CharacterCardHolder cardHolder = null;
    [SerializeField] OptimizedCanvas canvas = null;

    public static CharacterCardHolder selectedHolder = null;

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}
    //
    //// Update is called once per frame
    //void Update()
    //{
    //    
    //}

    private void OnEnable()
    {
        PartyManager.OnSelectCharacter += OnNewCharacterSelected;
    }

    private void OnDisable()
    {
        PartyManager.OnSelectCharacter -= OnNewCharacterSelected;
    }

    public void EnterCardLoadMode()
    {
        //CharacterPreviewUI.instance.EnterCardLoadMode();
    }

    public void CheckInfo()
    {
        //CharacterPreviewUI.instance.BeginCharacterPreview(cardHolder.Character, cardHolder.Rarity);
        canvas.SetActive(false);
    }

    public void ExportThisCard()
    {
        CardLoader.instance.ExportCharacter(cardHolder);
    }

    void OnNewCharacterSelected(CharacterCardHolder holder)
    {
        if (holder == cardHolder)
        {
            selectedHolder = cardHolder;
            canvas.SetActive(true);
        }
        else canvas.SetActive(false);
    }
}
