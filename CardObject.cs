using Cysharp.Threading.Tasks;
using UniRx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using System;
using System.Linq;

public class CardObject : MonoBehaviour
{
    public Card card = new Card(Suit.None, 1);
    private GameObject cardObject;

    private void Awake()
    {
        cardObject = transform.gameObject;

    }

    private async void Start()
    {
        string objectName = cardObject.name;
        if (objectName == "MainDeck")
        {
            await BackOfCard();
            return;
        }
        if (objectName == "CardShow")
        {
            card.isHidden = false;
            return;
        }
        if (transform.parent.name == "BaseGroup")
        {
            card.isHidden = false;
            return;
        }
        if (card.isHidden)
        {
            await BackOfCard();
        }
    }
    public async UniTask BackOfCard()
    {
        await UniTask.WaitUntil(() => { return SolitaireBehaviour.backNum != 100; });
        if (SolitaireBehaviour.backNum == 0)
        {
            cardObject.GetComponent<Image>().sprite = CardArtManager.SearchCardBack("Drawing_cardback");
        }
        else if (SolitaireBehaviour.backNum > 0 && SolitaireBehaviour.backNum <=10)
        {
            cardObject.GetComponent<Image>().sprite = CardArtManager.SearchCardBack("Cat_cardback");
        }
        else if (SolitaireBehaviour.backNum > 10 && SolitaireBehaviour.backNum <= 20)
        {
            cardObject.GetComponent<Image>().sprite = CardArtManager.SearchCardBack("Dumbo_cardback");
           
        }
        else if (SolitaireBehaviour.backNum > 20 && SolitaireBehaviour.backNum <= 40)
        {
            cardObject.GetComponent<Image>().sprite = CardArtManager.SearchCardBack("Trippy_cardback");
        }
        else { cardObject.GetComponent<Image>().sprite = CardArtManager.SearchCardBack("Default_cardback"); }
    }

    public void FrontOfCard(Card card)
    {
        if (card.CardSuit == Suit.None)
        {
            return;
        }
        cardObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        cardObject.GetComponent<Image>().sprite = CardArtManager.SearchCardFront(card);
        IsStacked(false);
    }

    public void IsStacked(bool isStacked)
    {
        BoxCollider2D boxCollider2D = transform.GetComponent<BoxCollider2D>();

        switch (isStacked)
        {
            case true:
                if (card.isHidden) { boxCollider2D.enabled = false; break; }
                boxCollider2D.enabled = true;
                boxCollider2D.size = new Vector2(86, 27);
                boxCollider2D.offset = new Vector2(0, 54);
                break;
                case false:
                if (card.isHidden) { boxCollider2D.enabled = true; }
                boxCollider2D.enabled = true;
                boxCollider2D.size = new Vector2(86, 122);
                boxCollider2D.offset = new Vector2(0, 0);
                break;
        }
        card.isStacked = isStacked;
    }


    void OnCardInteracted()
    {


    }
}
