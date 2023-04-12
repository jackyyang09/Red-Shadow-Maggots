using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardCanvasProjection : BaseCanvasProjection
{
    public void OnPointerClick() => OnCardClicked?.Invoke(this);
    public System.Action<CardCanvasProjection> OnCardClicked;
    public void OnPointerExit() => OnCardExited?.Invoke(this);
    public System.Action<CardCanvasProjection> OnCardExited;
    public void OnPointerEnter() => OnCardHovered?.Invoke(this);
    public System.Action<CardCanvasProjection> OnCardHovered;

    CharacterCardHolder cardHolder;
    public CharacterCardHolder CardHolder => cardHolder;

    static int CardsCreated = 0;
    readonly Vector2 START_POS = new Vector2(0, 2000);
    readonly Vector2 PADDING = new Vector2(50, 50);

    protected override void Start()
    {
        base.Start();

        worldProjector.transform.position = START_POS;
        worldProjector.transform.position = new Vector3
            (CardsCreated % 10 * PADDING.x,
            worldProjector.transform.position.y + CardsCreated / 10 * PADDING.y, 0);

        cardHolder = worldProjector.GetComponent<CharacterCardHolder>();

        CardsCreated++;
    }

    private void OnDestroy()
    {
        CardsCreated--;
    }

    public void SetActive(bool b) => rawImage.enabled = b;
    public void Show() => rawImage.enabled = true;
    public void Hide() => rawImage.enabled = false;
}
