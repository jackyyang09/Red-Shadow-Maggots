using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] CharacterCardHolder[] cardHolders = null;
    public CharacterCardHolder[] CardHolders
    {
        get { return cardHolders; }
    }

    [SerializeField] CharacterCard[] party = new CharacterCard[3];
    public CharacterCard[] Party
    {
        get
        {
            return party;
        }
    }

    public static System.Action<CharacterCardHolder> OnSelectCharacter;

    public static PartyManager instance;

    private void Awake()
    {
        EstablishSingletonDominance();
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < cardHolders.Length; i++)
        {
            var card = SaveManager.instance.Cards[SaveManager.instance.Party[i]];
            cardHolders[i].SetCharacterAndRarity(CardLoader.instance.CharacterIDToObject(card.characterID), card.rarity);
        } 
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

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
