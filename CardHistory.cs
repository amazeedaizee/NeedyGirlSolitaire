using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAction
{
    public GameObject cardObj;
    public GameObject parentObj;
    public SetDeck setDeck;
    public Card Card;
    public bool IsCardHidden;    
    public List<Card> stackedCards = new List<Card>();

}
public class CardHistory
{
    public CardAction TargetCardAction;
    public CardAction SelectedCardToTarget;
    public CardAction CardBehindSelectedCard = null;

    public async UniTask UndoAction()
    {
        Debug.Log($"Is Target Card SetDeck Null: {(TargetCardAction.setDeck != null ? "No" : "Yes")}\n" +
            $"Is Selected Card SetDeck Null: {(SelectedCardToTarget.setDeck != null ? "No" : "Yes")}"
);
        if (TargetCardAction == null || SelectedCardToTarget == null) { return; }
        if (TargetCardAction.cardObj.name == "CardShow" && SelectedCardToTarget.cardObj.name == "CardShow")
        {
            UndoShowCard();
        }
        if (TargetCardAction.setDeck != null)
        {
            var targetSetDeck = TargetCardAction.setDeck;
            var targetSetCard = targetSetDeck.transform.GetChild(targetSetDeck.transform.childCount - 1).GetComponent<CardObject>();
            if (targetSetCard != null && targetSetDeck.Cards.Contains(SelectedCardToTarget.Card))
            {
                if (SelectedCardToTarget.setDeck != null && SelectedCardToTarget.stackedCards.Count != 0)
                {
                    await UndoAppliedCardStack(targetSetDeck);
                    return;
                }
                targetSetDeck.Cards.Remove(SelectedCardToTarget.Card);
                targetSetCard.gameObject.SetActive(false);
                targetSetDeck.CheckActiveCard();

                if (SelectedCardToTarget.setDeck != null)
                {
                    await UndoAppliedSelectedCard();
                    return;
                }
                if (SelectedCardToTarget.cardObj.name.Contains("Base"))
                {
                    UndoAppliedFromBaseDeck();
                    return;
                }
                
                if (SelectedCardToTarget.cardObj.name == "CardShow")
                {
                    UndoAppliedShownCard();
                    return;
                }
            }
        }
        if (TargetCardAction.cardObj.name.Contains("Set"))
        {
            var targetSetDeck = TargetCardAction.cardObj.GetComponent<SetDeck>();
            var firstCard = TargetCardAction.cardObj.transform.GetChild(0).GetComponent<CardObject>();
            TargetCardAction.cardObj.GetComponent<BoxCollider2D>().enabled = true;
            if (targetSetDeck.Cards.Contains(SelectedCardToTarget.Card))
            {
                Debug.Log($"Stacked Cards: {SelectedCardToTarget.stackedCards.Count}");
                if (SelectedCardToTarget.setDeck != null && SelectedCardToTarget.stackedCards.Count != 0)
                {
                    await UndoAppliedCardStack(targetSetDeck);
                    targetSetDeck.Cards.Clear();
                    return;
                }
                firstCard.gameObject.SetActive(false);
                targetSetDeck.Cards.Clear();
                targetSetDeck.CheckActiveCard();
                if (SelectedCardToTarget.setDeck != null)
                {
                    await UndoAppliedSelectedCard();
                    return;
                }
                if (SelectedCardToTarget.cardObj.name.Contains("Base"))
                {
                    UndoAppliedFromBaseDeck();
                    return;
                }
                if (SelectedCardToTarget.cardObj.name == "CardShow")
                {
                    UndoAppliedShownCard();
                    return;
                }
            }
        }
        if (TargetCardAction.cardObj.name.Contains("Base"))
        {
            var targetBaseDeck = TargetCardAction.cardObj.GetComponent<BaseDeck>();
            if (targetBaseDeck.BaseCards.Contains(SelectedCardToTarget.Card))
            {
                targetBaseDeck.RemoveBaseCard(SelectedCardToTarget.Card);
                targetBaseDeck.ChangeCardFront(1);
                if (SelectedCardToTarget.setDeck != null)
                {
                    await UndoAppliedSelectedCard();
                    return;
                }
                if (SelectedCardToTarget.cardObj.name == "CardShow")
                {
                    UndoAppliedShownCard();
                    return;
                }
            }
        }
    }

    private async UniTask UndoAppliedCardStack(SetDeck targetSetDeck)
    {
        if (CardBehindSelectedCard != null && CardBehindSelectedCard.IsCardHidden)
        {
            var behindSelected = CardBehindSelectedCard.cardObj.GetComponent<CardObject>();
            behindSelected.card.isHidden = true;
            CardBehindSelectedCard.setDeck.CheckActiveCard();
            await behindSelected.BackOfCard();
        }
        targetSetDeck.RemoveSourceStack(SelectedCardToTarget.stackedCards);
        targetSetDeck.CheckActiveCard();
        SelectedCardToTarget.setDeck.SetCardStack(SelectedCardToTarget.stackedCards);
        SelectedCardToTarget.setDeck.CheckActiveCard();
    }

    private async UniTask UndoAppliedSelectedCard()
    {
        CardHistoryManager.RecreateSelectedCard(SelectedCardToTarget.Card, SelectedCardToTarget.setDeck);
        SelectedCardToTarget.setDeck.Cards.Add(TargetCardAction.Card);
        SelectedCardToTarget.setDeck.CheckActiveCard();
        if (CardBehindSelectedCard != null && CardBehindSelectedCard.IsCardHidden)
        {
            var behindSelected = CardBehindSelectedCard.cardObj.GetComponent<CardObject>();
            behindSelected.card.isHidden = true;
            CardBehindSelectedCard.setDeck.CheckActiveCard();
            await behindSelected.BackOfCard();
        }
    }

    private void UndoAppliedFromBaseDeck()
    {
        var beforeApplyFromBase = SelectedCardToTarget.cardObj.GetComponent<BaseDeck>();
        beforeApplyFromBase.BaseCards.Add(SelectedCardToTarget.Card);
        beforeApplyFromBase.ChangeCardFront(1);
    }

    private void UndoAppliedShownCard()
    {
        MainDeck.UsedCards.Add(SelectedCardToTarget.Card);
        ShowCard.ChangeCardFront(1, true);
    }
    private void UndoShowCard()
    {
        Card shownCard = MainDeck.UsedCards[^1];
        MainDeck.Cards.Add(shownCard);
        MainDeck.UsedCards.Remove(shownCard);
        ShowCard.ChangeCardFront(1, true);
    }
}

