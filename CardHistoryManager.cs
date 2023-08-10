using Cysharp.Threading.Tasks;
using SolitaireScripts;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class CardHistoryManager : MonoBehaviour
{
    private static GameObject _undoButton;
    public static CardHistory LastCardAction;
    public void Awake()
    {
        _undoButton = transform.gameObject;
        transform.GetComponent<Button>().interactable = false;
        SolitaireBehaviour.gameReset += ResetUndoButton;
    }

    public static async UniTask ResetUndoButton()
    {
        LastCardAction = null;
        _undoButton.GetComponent<Button>().interactable = false;
    }

    public static async UniTask InvokeUndo()
    {
        await LastCardAction.UndoAction();
        ResetUndoButton().Forget();

    }

    public static void RecreateSelectedCard(Card cardToSet, SetDeck selectedSetDeck)
    {
        var beforeApplyCard = Instantiate(InitializeWindow.assetBundle.LoadAsset<GameObject>("Card"), selectedSetDeck.transform).GetComponent<CardObject>();
        beforeApplyCard.card = cardToSet;
        beforeApplyCard.card.isHidden = false;
        beforeApplyCard.FrontOfCard(cardToSet);
        beforeApplyCard.IsStacked(false);
    }

    public static async UniTask CreatePastAction(GameObject hoveredObj, CardObject selectedCard, List<CardObject> stackedCards = null)
    {
        CardAction targetCard = new();
        CardAction selectedCardToTarget = new();
        CardAction behindSelectedCard = null;
        await UniTask.WaitUntil(() => {
            var hoveredCard = hoveredObj.GetComponent<CardObject>();
            targetCard.cardObj = hoveredObj;
            targetCard.parentObj = hoveredObj.transform.parent.gameObject;
            if (hoveredObj.name.Contains("Set"))
            {
                targetCard.Card = null;
            }
            else { targetCard.Card = hoveredCard.card; }          
            if (targetCard.parentObj.name.Contains("Set") && targetCard.parentObj.name != "SetGroup")
            {
                targetCard.setDeck = SolitaireBehaviour.GetSetDeck(hoveredCard);
            }
            //Debug.Log($"Selected Card: {selectedCard.card.CardSuit}, {selectedCard.card.CardNum.Num}\nIs Selected Card Stacked {(selectedCard.card.isStacked ? "Yes" : "No")}");
            selectedCardToTarget.cardObj = selectedCard.gameObject;
            selectedCardToTarget.parentObj = selectedCard.transform.parent.gameObject;
            selectedCardToTarget.Card = selectedCard.card;
            if (selectedCardToTarget.parentObj.name.Contains("Set") && selectedCardToTarget.parentObj.name != "SetGroup")
            {
                selectedCardToTarget.setDeck = SolitaireBehaviour.GetSetDeck(selectedCard);
            }
            if (stackedCards != null && selectedCardToTarget.setDeck != null)
            {
                List<Card> getStackedCards = new List<Card>();
                for (int i = 0; i < stackedCards.Count; i++)
                {
                    getStackedCards.Add(stackedCards[i].card);
                }
                selectedCardToTarget.stackedCards = getStackedCards;
            }
            else { selectedCardToTarget.stackedCards.Clear(); }
            behindSelectedCard = null;
            if (selectedCardToTarget.setDeck != null) {
                try
                {
                    behindSelectedCard = new();
                    behindSelectedCard.setDeck = SolitaireBehaviour.GetSetDeck(selectedCard);
                    behindSelectedCard.parentObj = selectedCardToTarget.parentObj;
                    if (selectedCardToTarget.stackedCards.Count != 0)
                    {
                        var childList = behindSelectedCard.setDeck.transform.GetComponentsInChildren<CardObject>();
                        for (int i = childList.Length - 1; i >= 0; i--)
                        {
                            if (childList[i].card == selectedCardToTarget.Card)
                            {
                                behindSelectedCard.cardObj = childList[i - 1].gameObject;
                                var setBehindStackedCard = childList[i - 1].card;
                                behindSelectedCard.Card = setBehindStackedCard;
                                behindSelectedCard.IsCardHidden = setBehindStackedCard.isHidden;
                                break;
                            }
                        }
                    }
                    else
                    {
                        behindSelectedCard.cardObj = behindSelectedCard.setDeck.transform.GetChild(behindSelectedCard.setDeck.transform.childCount - 2).gameObject;
                        Card setBehindCard = behindSelectedCard.setDeck.transform.GetChild(behindSelectedCard.setDeck.transform.childCount - 2).GetComponent<CardObject>().card;
                        behindSelectedCard.Card = setBehindCard;
                        behindSelectedCard.IsCardHidden = setBehindCard.isHidden;
                    }
                }
                catch { Debug.LogWarning("Could not find the behind card!"); }
            }           
            CardHistory cardHistory = new()
            {
                TargetCardAction = targetCard,
                SelectedCardToTarget = selectedCardToTarget,
                CardBehindSelectedCard = behindSelectedCard
            };
            LastCardAction = cardHistory;
            _undoButton.GetComponent<Button>().interactable = true;
            return true;
        });
    }
}
