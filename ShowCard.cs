using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowCard : MonoBehaviour
{
    public static bool CanSolve = false;
    public static Card currentCard;
    public static GameObject showCardObject;
    private void Awake()
    {

        MainDeck.showCard += OnShownCard;
        showCardObject = transform.gameObject;
    }

    private void OnShownCard(Card card)
    {
        if (card == null)
        {
            currentCard = null;
            showCardObject.GetComponent<Image>().overrideSprite = CardArtManager.ApplyCardBase();
            return;
        }
        currentCard = card;
        showCardObject.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(card);
    }

    public static void ChangeCardFront(int index, bool alsoApplyCardData)
    {
        try
        {
            Card card = MainDeck.UsedCards[^index];
            if (alsoApplyCardData) 
            { 
                currentCard = card;
            }
           showCardObject.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(card);
        }
        catch
        {
            showCardObject.GetComponent<Image>().overrideSprite = CardArtManager.ApplyCardBase();
        }
    }

    public static void ChangeToSolve()
    {
        CanSolve = true;
        currentCard = null;
        showCardObject.GetComponent<Image>().overrideSprite = CardArtManager.ApplyCardSolve();
    }
}

