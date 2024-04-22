using Cysharp.Threading.Tasks;
using SolitaireScripts;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IDragHandler
{
    private static ReactiveProperty<Card> cardObservable = new ReactiveProperty<Card>();
    public static Card card = null;
    public static List<CardObject> selectedCardStack = null;
    public static CardObject hoveredObject = null;
    public static CardObject selectedObject = null;
    public static BaseDeck hoveredBaseDeck = null;
    public static BaseDeck selectedBaseDeck = null;
    public static GameObject FirstHeldCardObject;
    private GameObject _cardPrefab;
    private RectTransform _rectTransform;
    private static BoxCollider2D _collider;


    private bool isMainDeck;
    private bool isShowCard;
    private bool isUndoButton;


    private float zPosition;
    private void Awake()
    {
        selectedBaseDeck = null;
        selectedCardStack = null;
        selectedObject = null;
        hoveredBaseDeck = null;
        hoveredObject = null;
        zPosition = transform.position.z - 1;
        ; FirstHeldCardObject = transform.GetChild(0).gameObject;
        _cardPrefab = InitializeWindow.assetBundle.LoadAsset<GameObject>("Card");
        _rectTransform = transform.GetComponent<RectTransform>();
        _collider = transform.GetComponent<BoxCollider2D>();
        FirstHeldCardObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        ToggleCollider(false);
    }

    void LateUpdate()
    {
        Vector3 cameraPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cameraPos.z = zPosition;
        _rectTransform.position = cameraPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isShowCard && !ShowCard.CanSolve)
        {
            card = MainDeck.UsedCards[^1];
            ShowCard.ChangeCardFront(2, false);
            FirstHeldCardObject.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(card);
        }
        if (card != null)
        {
            FirstHeldCardObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);

            if (selectedBaseDeck != null && selectedBaseDeck.BaseCards.Count != 0 && card == selectedBaseDeck.BaseCards[^1])
            {
                selectedBaseDeck.ChangeCardFront(2);
                FirstHeldCardObject.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(card);
            }
        }
        if (selectedObject != null && selectedObject.card.isStacked && !selectedObject.card.isHidden)
        {
            if (selectedCardStack == null) { return; }
            CardHoldStack[] cardHoldStack = transform.GetComponentsInChildren<CardHoldStack>();
            for (int i = 0; i < selectedCardStack.Count; i++)
            {
                // Debug.Log($"Card in Stack: {selectedCardStack[i].card.CardSuit} {selectedCardStack[i].card.CardNum.Num}");
                selectedCardStack[i].gameObject.GetComponent<Image>().enabled = false;
                if (i == 0)
                {
                    FirstHeldCardObject.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(card);
                }
                else
                {
                    cardHoldStack[i].GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    cardHoldStack[i].SetCardFront(selectedCardStack[i].card);
                }


            }
            return;
        }
        if (selectedObject != null)
        {
            selectedObject.gameObject.GetComponent<Image>().enabled = false;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        BaseDeck selectedBase = hoveredBaseDeck;
        CardObject selectedObj = hoveredObject;
        if (cardObservable.Value != null)
        {
            card = cardObservable.Value;
            if (isMainDeck || isShowCard)
            {
                return;
            }
            if (selectedObj != null && selectedBase == null)
            {
                selectedObject = selectedObj;

                if (selectedObject.card.isStacked)
                {
                    selectedCardStack = selectedObject.transform.parent.GetComponent<SetDeck>().GetCardStack(selectedObject);
                    //Debug.Log(selectedCardStack.Count);
                    for (int i = 0; i < selectedCardStack.Count; i++)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        var cardStack = Instantiate(FirstHeldCardObject, transform);
                        cardStack.SetActive(true);
                    }
                    return;
                }
            }
            else if (selectedBase != null)
            {
                selectedBaseDeck = selectedBase;
            }
            FirstHeldCardObject.GetComponent<Image>().overrideSprite = CardArtManager.SearchCardFront(card);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (card != null)
        {
            return;
        }
        if (isUndoButton && CardHistoryManager.LastCardAction != null && !ShowCard.CanSolve)
        {
            // await CardHistoryManager.InvokeUndo();
            return;
        }
        if (isMainDeck)
        {
            MainDeck.OnClicked();
            // Debug.Log("Main Deck Count: " + MainDeck.Cards.Count.ToString() + ", Shown Card Count: " + MainDeck.UsedCards.Count.ToString());
            // await CardHistoryManager.CreatePastAction(ShowCard.showCardObject, ShowCard.showCardObject.GetComponent<CardObject>()); 
            return;
        }
        if (isShowCard)
        {
            if (ShowCard.CanSolve && !SolitaireBehaviour.PlayerWon)
            {
                SolitaireBehaviour.SolveForWin();
            }
            return;
        }
        if (card == null && hoveredObject != null && hoveredObject.gameObject.name != "CardShow" && hoveredObject.card.isHidden && !hoveredObject.card.isStacked)
        {
            hoveredObject.card.isHidden = false;
            hoveredObject.IsStacked(false);
            cardObservable.Value = hoveredObject.card;
            hoveredObject.FrontOfCard(hoveredObject.card);
            hoveredObject.transform.parent.GetComponent<SetDeck>().CheckActiveCard();

        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (card != null && MainDeck.UsedCards.Count != 0 && card == MainDeck.UsedCards[^1] || (hoveredObject != null && (hoveredObject.gameObject.name == "CardShow" || hoveredObject.gameObject.name == "MainDeck" || hoveredObject.card.isStacked)))
        {
            ShowCard.ChangeCardFront(1, false);
        }
        if (card != null && selectedBaseDeck != null && selectedBaseDeck.BaseCards.Count != 0 && card == selectedBaseDeck.BaseCards[^1] || (selectedBaseDeck != null && hoveredObject != null && (hoveredObject.gameObject.name == "CardShow" || hoveredObject.gameObject.name == "MainDeck" || hoveredObject.card.isStacked)) || (selectedBaseDeck != null && hoveredObject == null))
        {
            selectedBaseDeck.ChangeCardFront(1);
        }
        selectedBaseDeck = null;
        card = null;
        FirstHeldCardObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        if (selectedObject == null) { return; }
        if (selectedCardStack != null && selectedObject.card.isStacked && !selectedObject.card.isHidden || (hoveredObject != null && (selectedObject.transform.parent == hoveredObject.transform.parent || hoveredObject.gameObject.name == "CardShow" || hoveredObject.gameObject.name == "MainDeck" || hoveredObject.card.isStacked)) || (selectedCardStack != null && hoveredObject == null))
        {
            selectedObject.transform.parent.GetComponent<SetDeck>().ResetSourceStack(selectedCardStack);
        }
        if (hoveredObject == null || selectedObject == hoveredObject || (hoveredObject != null && (hoveredObject.gameObject.name == "CardShow" || hoveredObject.gameObject.name == "MainDeck" || hoveredObject.card.isStacked)))
        {
            selectedObject.gameObject.GetComponent<Image>().enabled = true;

        }
        if (transform.childCount > 1)
        {
            RemoveCardHoldStack();
        }
        selectedObject = null;
        selectedCardStack = null;

    }

    private void RemoveCardHoldStack()
    {
        CardHoldStack[] cardStack = transform.GetComponentsInChildren<CardHoldStack>();
        for (int i = 0; i < cardStack.Length; i++)
        {
            if (i == 0)
            {
                cardStack[i].GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
            else
            {
                cardStack[i].GoodbyeCardStack();
            }
        }
    }


    async void OnTriggerEnter2D(Collider2D collider2D)
    {
        isUndoButton = false;
        isMainDeck = false;
        isShowCard = false;
        hoveredObject = null;
        cardObservable.Value = null;
        GameObject gameObject = collider2D.gameObject;
        //Debug.Log(gameObject.name);    
        if (gameObject.name == "UndoButton")
        {
            isUndoButton = true;
            return;
        }
        if (gameObject.name == "MainDeck")
        {
            isMainDeck = true;
            return;
        }
        if (card == null && gameObject.name == "CardShow")
        {
            isShowCard = true;
        }
        if (gameObject.transform.parent.name == "BaseGroup")
        {
            List<Card> baseCards = gameObject.GetComponent<BaseDeck>().BaseCards;
            if (card == null && baseCards.Count != 0)
            {
                cardObservable.Value = baseCards[^1];
                hoveredBaseDeck = gameObject.GetComponent<BaseDeck>();
            }
            else
            {
                await ApplyToBaseDeck(gameObject);
                return;
            }
        }
        if (card != null && gameObject.name.Contains("Set"))
        {
            await ApplyFirstCardToSet(gameObject);
            return;
        }
        bool getCard = gameObject.TryGetComponent<CardObject>(out var cardObject);
        if (!getCard)
        {
            return;
        }
        hoveredObject = cardObject;
        if (!cardObject.transform.parent.TryGetComponent<SetDeck>(out _))
        {
            return;
        }
        CardObject hoveredCard = cardObject.transform.parent.GetComponent<SetDeck>().CheckActiveCard();
        if (!getCard) { return; }
        if (!cardObject.card.isHidden)
        {
            if (card == null)
            {
                cardObservable.Value = cardObject.card;
                return;
            }
            if (!cardObject.card.isStacked)
            {
                SetDeck targetSet = cardObject.transform.parent.GetComponent<SetDeck>();
                if (selectedBaseDeck != null && selectedBaseDeck.BaseCards.Contains(card))
                {
                    await ApplyFromBaseDeck(hoveredCard, cardObject, targetSet);
                    return;
                }
                if (MainDeck.UsedCards.Contains(card))
                {
                    await ApplyShownCard(hoveredCard, cardObject, targetSet);
                    return;
                }
                if (selectedCardStack != null)
                {
                    await ApplyCardStack(hoveredCard, cardObject, targetSet);
                    return;

                }
                await ApplySelectedCard(hoveredCard, cardObject, targetSet);
                return;

            }

        }
    }


    private void ApplyNewCard(SetDeck targetSet, Card applyCardFromHold)
    {
        var newCard = Instantiate(_cardPrefab, targetSet.transform).GetComponent<CardObject>();
        newCard.card = applyCardFromHold;
        newCard.IsStacked(false);
        newCard.FrontOfCard(applyCardFromHold);
    }
    private async UniTask ApplyToBaseDeck(GameObject obj)
    {
        BaseDeck targetBase = obj.transform.GetComponent<BaseDeck>();
        targetBase.IsTarget = true;
        await UniTask.WaitUntil(() => { return Input.GetMouseButtonUp(0); });
        var applyCardFromHold = card;
        if (!targetBase.AddBaseCard(applyCardFromHold))
        {
            if (MainDeck.UsedCards.Contains(applyCardFromHold))
            {
                ShowCard.ChangeCardFront(1, true);
            }
            else if (selectedObject != null)
            {
                selectedObject.gameObject.GetComponent<Image>().enabled = true;
            }
            return;
        }
        if (selectedCardStack != null) { return; }
        if (MainDeck.UsedCards.Contains(applyCardFromHold))
        {
            // await CardHistoryManager.CreatePastAction(targetBase.gameObject, ShowCard.showCardObject.GetComponent<CardObject>());
            MainDeck.UsedCards.Remove(applyCardFromHold);
            ShowCard.ChangeCardFront(1, true);
        }
        else if (selectedObject != null)
        {

            var selectedSet = selectedObject.transform.parent.GetComponent<SetDeck>();
            selectedObject.gameObject.SetActive(false);
            selectedSet.Cards.Remove(applyCardFromHold);
            // await CardHistoryManager.CreatePastAction(targetBase.gameObject, selectedObject);
            selectedSet.CheckActiveCard();
        }

        SolitaireBehaviour.CheckForSolve();
    }

    private async UniTask ApplyFromBaseDeck(CardObject hoveredCard, CardObject cardObject, SetDeck targetSet)
    {
        if (selectedBaseDeck == null) { return; }

        targetSet.isTarget = true;
        await UniTask.WaitUntil(() => { return Input.GetMouseButtonUp(0); });
        if (!targetSet.isTarget) { return; }
        if (targetSet.Cards.Contains(card))
        {
            return;
        }
        CardObject setCard = targetSet.GetActiveCard();
        if (setCard == null || hoveredCard.card.isStacked || (hoveredCard.gameObject.name == "CardShow" || hoveredCard.gameObject.name == "MainDeck") || !(IsSuitValid(hoveredCard.card) && IsNumValid(hoveredCard.card)))
        {
            selectedBaseDeck.ChangeCardFront(1);
            targetSet.CheckActiveCard();
            return;
        }
        var applyCardFromHold = card;
        var hoveredObj = hoveredCard.gameObject;
        var selectedBase = selectedBaseDeck;
        // await CardHistoryManager.CreatePastAction(hoveredObj, selectedBaseDeck.gameObject.GetComponent<CardObject>());
        selectedBase.RemoveBaseCard(applyCardFromHold);
        if (targetSet.Cards.Contains(applyCardFromHold))
        {
            return;
        }
        ApplyNewCard(targetSet, applyCardFromHold);
        targetSet.Cards.Add(applyCardFromHold);
        targetSet.CheckActiveCard();
    }

    private async UniTask ApplyShownCard(CardObject hoveredCard, CardObject cardObject, SetDeck targetSet)
    {
        targetSet.isTarget = true;
        await UniTask.WaitUntil(() => { return Input.GetMouseButtonUp(0); });
        CardObject setCard = targetSet.GetActiveCard();
        if (!targetSet.isTarget) { return; }
        if (targetSet.Cards.Contains(card))
        {
            return;
        }
        var applyCardFromHold = card;
        var hoveredObj = hoveredCard.gameObject;
        if (setCard == null || hoveredCard.card.isStacked || !(IsSuitValid(hoveredCard.card) && IsNumValid(hoveredCard.card)))
        {
            // Debug.Log($"Hovered Card: {hoveredCard.card.CardSuit} & {hoveredCard.card.CardNum.Num} \n Set Card: {card.CardSuit} & {card.CardNum.Num}");
            ShowCard.ChangeCardFront(1, true);
            targetSet.CheckActiveCard();
            return;
        }
        // await CardHistoryManager.CreatePastAction(hoveredObj, ShowCard.showCardObject.GetComponent<CardObject>());
        if (targetSet.Cards.Contains(applyCardFromHold))
        {
            return;
        }
        MainDeck.UsedCards.Remove(applyCardFromHold);
        ShowCard.ChangeCardFront(1, true);
        ApplyNewCard(targetSet, applyCardFromHold);
        targetSet.Cards.Add(applyCardFromHold);
        targetSet.CheckActiveCard();
    }
    private async UniTask ApplySelectedCard(CardObject hoveredCard, CardObject cardObject, SetDeck targetSet)
    {
        if (selectedObject == null || selectedObject.card.isStacked) { return; }
        targetSet.isTarget = true;
        await UniTask.WaitUntil(() => { return Input.GetMouseButtonUp(0); });
        if (!targetSet.isTarget) { return; }
        if (targetSet.Cards.Contains(card))
        {
            return;
        }
        CardObject setCard = targetSet.GetActiveCard();
        GameObject selectedCard = selectedObject.gameObject;
        var selectedSet = SolitaireBehaviour.GetSetDeck(selectedObject);
        var hoveredObj = hoveredCard.gameObject;
        if (setCard == null || hoveredCard.card.isStacked || !(IsSuitValid(hoveredCard.card) && IsNumValid(hoveredCard.card)))
        {
            // Debug.Log($"Hovered Card: {hoveredCard.card.CardSuit} & {hoveredCard.card.CardNum.Num} \n Set Card: {card.CardSuit} & {card.CardNum.Num}");
            selectedObject.gameObject.GetComponent<Image>().enabled = true;
            targetSet.CheckActiveCard();
            return;
        }
        var applyCardFromHold = card;
        // await CardHistoryManager.CreatePastAction(hoveredObj, selectedObject);
        if (targetSet.Cards.Contains(applyCardFromHold))
        {
            return;
        }
        selectedCard.SetActive(false);
        selectedSet.Cards.Remove(applyCardFromHold);
        ApplyNewCard(targetSet, applyCardFromHold);
        targetSet.Cards.Add(applyCardFromHold);
        targetSet.CheckActiveCard();
        selectedSet.CheckActiveCard();
    }

    private async UniTask ApplyCardStack(CardObject hoveredCard, CardObject cardObject, SetDeck targetSet)
    {
        if (selectedObject == null && (selectedObject.card.isHidden || !selectedObject.card.isStacked))
        {
            return;
        }
        targetSet.isTarget = true;
        await UniTask.WaitUntil(() => { return Input.GetMouseButtonUp(0); });
        if (!targetSet.isTarget) { return; }
        if (targetSet.Cards.Contains(card))
        {
            return;
        }
        CardObject setCard = targetSet.GetActiveCard();
        var selectedSet = SolitaireBehaviour.GetSetDeck(selectedObject);
        var hoveredObj = hoveredCard.gameObject;
        if (hoveredCard.transform.parent.name == "BaseGroup" || hoveredCard.card.isStacked || !(IsSuitValid(hoveredCard.card) && IsNumValid(hoveredCard.card)))
        {
            // Debug.Log($"Hovered Card: {hoveredCard.card.CardSuit} & {hoveredCard.card.CardNum.Num} \n Set Card: {card.CardSuit} & {card.CardNum.Num}");
            selectedObject.transform.parent.GetComponent<SetDeck>().ResetSourceStack(selectedCardStack);
            selectedObject.transform.parent.GetComponent<SetDeck>().CheckActiveCard();
            targetSet.CheckActiveCard();
            return;
        }

        var applyingCardStack = selectedCardStack;
        var applyCardFromHold = card;
        // await CardHistoryManager.CreatePastAction(hoveredObj, selectedObject, selectedCardStack);
        if (targetSet.Cards.Contains(applyCardFromHold))
        {

            return;
        }
        hoveredCard.card.isStacked = true;
        selectedSet.RemoveSourceStack(applyingCardStack);
        selectedSet.CheckActiveCard();
        targetSet.SetCardStack(applyingCardStack);
        targetSet.CheckActiveCard();

    }

    private async UniTask ApplyFirstCardToSet(GameObject obj)
    {
        SetDeck targetSet = obj.transform.GetComponent<SetDeck>();
        targetSet.isTarget = true;
        await UniTask.WaitUntil(() => { return Input.GetMouseButtonUp(0); });
        if (!targetSet.isTarget) { return; }
        var applyCardFromHold = card;
        if (targetSet.Cards.Contains(applyCardFromHold))
        {
            return;
        }
        if (applyCardFromHold.CardNum.Num != 13)
        {
            if (MainDeck.UsedCards.Contains(applyCardFromHold))
            {
                selectedBaseDeck.ChangeCardFront(1);
            }
            else if (selectedBaseDeck != null && selectedBaseDeck.BaseCards.Count != 0 && card == selectedBaseDeck.BaseCards[^1])
            {
                ShowCard.ChangeCardFront(1, true);
            }
            else if (selectedCardStack != null)
            {
                selectedObject.transform.parent.GetComponent<SetDeck>().ResetSourceStack(selectedCardStack);
                selectedObject.transform.parent.GetComponent<SetDeck>().CheckActiveCard();
                targetSet.CheckActiveCard();
            }
            else if (selectedObject != null)
            {
                selectedObject.gameObject.GetComponent<Image>().enabled = true;
            }

            return;
        }
        if (selectedCardStack != null)
        {
            var applyingCardStack = selectedCardStack;
            var selectedStackSet = SolitaireBehaviour.GetSetDeck(selectedObject);
            // await CardHistoryManager.CreatePastAction(obj, selectedObject, selectedCardStack);
            if (targetSet.Cards.Contains(applyCardFromHold))
            {
                return;
            }
            selectedStackSet.RemoveSourceStack(applyingCardStack);
            selectedStackSet.CheckActiveCard();
            targetSet.SetCardStack(applyingCardStack);
            targetSet.CheckActiveCard();
            return;
        }
        else if (selectedObject != null)
        {
            var selectedSet = SolitaireBehaviour.GetSetDeck(selectedObject);
            selectedObject.gameObject.SetActive(false);
            // await CardHistoryManager.CreatePastAction(obj, selectedObject);
            selectedSet.CheckActiveCard();
        }
        else if (MainDeck.UsedCards.Contains(applyCardFromHold))
        {
            // await CardHistoryManager.CreatePastAction(obj, ShowCard.showCardObject.GetComponent<CardObject>());
            MainDeck.UsedCards.Remove(applyCardFromHold);
            ShowCard.ChangeCardFront(1, true);
        }
        else if (selectedBaseDeck != null && selectedBaseDeck.BaseCards.Count != 0 && card == selectedBaseDeck.BaseCards[^1])
        {
            // await CardHistoryManager.CreatePastAction(obj, selectedBaseDeck.gameObject.GetComponent<CardObject>());
            selectedBaseDeck.RemoveBaseCard(applyCardFromHold);
            selectedBaseDeck.ChangeCardFront(1);
        }
        if (targetSet.Cards.Contains(applyCardFromHold))
        {
            return;
        }
        ApplyNewCard(targetSet, applyCardFromHold);
        targetSet.Cards.Add(applyCardFromHold);
        targetSet.CheckActiveCard();
    }
    void OnTriggerExit2D(Collider2D collider2D)
    {
        isUndoButton = false;
        isMainDeck = false;
        isShowCard = false;
        hoveredBaseDeck = null;
        if (collider2D.gameObject.TryGetComponent<BaseDeck>(out _))
        {
            collider2D.gameObject.GetComponent<BaseDeck>().IsTarget = false;
        }
        if (collider2D.gameObject.TryGetComponent<SetDeck>(out _))
        {
            collider2D.gameObject.GetComponent<SetDeck>().isTarget = false;
        }
        if (collider2D.gameObject.transform.parent.TryGetComponent<SetDeck>(out _))
        {
            collider2D.gameObject.transform.parent.GetComponent<SetDeck>().isTarget = false;
        }
        //if (transform.GetComponent<Rigidbody2D>().IsTouchingLayers()) { return; }
        //Debug.Log("Exited collider: " + collider2D.name);
        hoveredObject = null;
        cardObservable.Value = null;
    }

    bool IsSuitValid(Card cardHover)
    {
        if (cardHover.CardSuit == Suit.Spade || cardHover.CardSuit == Suit.Club)
        {
            return card.CardSuit == Suit.Heart || card.CardSuit == Suit.Diamond;
        }
        if (cardHover.CardSuit == Suit.Heart || cardHover.CardSuit == Suit.Diamond)
        {
            return card.CardSuit == Suit.Spade || card.CardSuit == Suit.Club;
        }
        return false;
    }

    bool IsNumValid(Card cardHover)
    {
        return cardHover.CardNum.Num == card.CardNum.Num + 1;
    }

    public static void ToggleCollider(bool isEnabled)
    {
        _collider.enabled = isEnabled;
    }
}
