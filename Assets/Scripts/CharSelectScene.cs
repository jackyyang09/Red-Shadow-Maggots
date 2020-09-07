using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSelectScene : MonoBehaviour
{
    public static CharSelectScene instance;
    public static List<CharacterObject> CharacterList;

    [SerializeField]
    List<PlayerCharacter> deck;
    public List<PlayerCharacter> Deck
    {
        get
        {
            return deck;
        }
    }
    public CharacterObject GetRandomCharacter()
    {
        return CharacterList[Random.Range(0, CharacterList.Count)];
    }

    private void Awake()
    {
        EstablishSingletonDominance();

        CharacterList = new List<CharacterObject>()
        {
            // TODO: how do I populate this list with the assets from the game folder? 
            new CharacterObject(),
        };

        deck = new List<PlayerCharacter>(FindObjectsOfType<PlayerCharacter>());
    }

    public List<PlayerCharacter> GetChosenCharacters()
    {
        // Why are the cards found in this order?
        // Who knows.
        // But I do know that 0,1,3 gets you the top 3 cards of the deck
        return new List<PlayerCharacter>() {
            deck[0],
            deck[1],
            deck[3],
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (PlayerCharacter character in deck)
        {
            character.HideCharacterUI();

            // the shuffling animation doesn't do anything
            // the deck is randomized at startup
            character.Reference = GetRandomCharacter();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // don't actually need to do this, just go to next scene and you can call GetChosenCharacters from there
        // for debug purposes
        if (Input.GetKeyDown("k"))
        {
            List<PlayerCharacter> chosenCharacters = GetChosenCharacters();
        }
    }

    // I gotta say, having this EstablishSingletonDominance function copy/pasted in several places feels bad
    // But hey, if it works it works
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
