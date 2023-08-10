using Cysharp.Threading.Tasks;
using SolitaireScripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SetDeck : MonoBehaviour
{
    public List<Card> Cards = new List<Card>();
    public bool isTarget;
    public bool ListInitialized;
    public bool NoHiddenCardsInSet = false;
    private bool isFirstCardActive;

    private void Awake()
    {
        SolitaireBehaviour.gameStart += OnGameStart;
        transform.gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }
    private async UniTask OnGameStart()
    {
        NoHiddenCardsInSet = false;
        switch (transform.gameObject.name)
        {
            case "SetOne":
                Cards.AddRange(SolitaireBehaviour.SetOneList);
                await UniTask.WaitUntil(() => { return Cards.Count >= 1; });
                SolitaireBehaviour.SetOneList.Clear();
                break;
            case "SetTwo":
                Cards.AddRange(SolitaireBehaviour.SetTwoList);
                await UniTask.WaitUntil(() => { return Cards.Count >= 2; });
                SolitaireBehaviour.SetTwoList.Clear();
                break;
            case "SetThree":
                Cards.AddRange(SolitaireBehaviour.SetThreeList);
                await UniTask.WaitUntil(() => { return Cards.Count >= 3; });
                SolitaireBehaviour.SetThreeList.Clear();
                break;
            case "SetFour":
                Cards.AddRange(SolitaireBehaviour.SetFourList);
                await UniTask.WaitUntil(() => { return Cards.Count >= 4; });
                SolitaireBehaviour.SetFourList.Clear();
                break;
            case "SetFive":
                Cards.AddRange(SolitaireBehaviour.SetFiveList);
                await UniTask.WaitUntil(() => { return Cards.Count >= 5; });
                SolitaireBehaviour.SetFiveList.Clear();
                break;
            case "SetSix":
                Cards.AddRange(SolitaireBehaviour.SetSixList);
                await UniTask.WaitUntil(() => { return Cards.Count >= 6; });
                SolitaireBehaviour.SetSixList.Clear();
                break;
            case "SetSeven":
                Cards.AddRange(SolitaireBehaviour.SetSevenList);
                await UniTask.WaitUntil(() => { return Cards.Count >= 7; });
                SolitaireBehaviour.SetSevenList.Clear();
                break;
            default:
                return;
        }   
        ListInitialized = true;
        for (int i = 0; i < Cards.Count; i++)
        {
            var cardObject = Instantiate(InitializeWindow.assetBundle.LoadAsset<GameObject>("Card"), transform).GetComponent<CardObject>();
            cardObject.card = Cards[i];
            cardObject.IsStacked(true);
            cardObject.card.isHidden = true;
            if (i == Cards.Count - 1)
            {
                cardObject.card.isHidden = false;
                cardObject.FrontOfCard(Cards[i]);
                cardObject.IsStacked(false);
                CheckActiveCard();
            }
        }
        if (transform.gameObject.name == "SetSeven" && transform.childCount == 7)
        {
            SolitaireBehaviour.PreventAction.SetActive(false);
        }
    }

    public CardObject GetActiveCard()
    {
        if (!isTarget)
        {
            return null;
        }
        CardObject obj = null;
        var childList = transform.gameObject.GetComponentsInChildren<CardObject>(true);
        childList[childList.Length -1].IsStacked(false);
        for (int i = 0; i < childList.Length; i++)
        {
            if (i == childList.Length-1)
            {
                obj = childList[i];          
                // Debug.Log($"{childList[i].gameObject.name} stacked: {childList[i].card.isStacked}");
                break;

            }
            childList[i].IsStacked(true);
            childList[i].GetComponent<BoxCollider2D>().enabled = false;
            // Debug.Log($"{childList[i].gameObject.name} stacked: {childList[i].card.isStacked}");
        }
        return obj;
    }

    public CardObject CheckActiveCard()
    {
        CardObject obj = null;
        var childList = transform.gameObject.GetComponentsInChildren<CardObject>(true);
        bool existHiddenCards = false;
        for (int i = childList.Length-1; i >= 0; i--)
        {
            if (!childList[i].gameObject.activeInHierarchy && i == 0)
            {
                Destroy(childList[i].gameObject);
                transform.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                //Debug.Log($"{transform.gameObject.name} is now Active.");
                isFirstCardActive = false;
                NoHiddenCardsInSet = true;
                return null;
            }
            if (!childList[i].gameObject.activeInHierarchy)
            {              
                Destroy(childList[i].gameObject);
                continue;
            }                       
                if (!isFirstCardActive && childList[i].gameObject.activeInHierarchy)
                {
                    obj = childList[i];
                    obj.IsStacked(false);
                    if (!obj.gameObject.GetComponent<Image>().enabled)
                    {
                         obj.gameObject.GetComponent<Image>().enabled = true;
                    }
                    if (obj.gameObject.GetComponent<Image>().color != new Color(1, 1, 1, 1))
                    {
                         obj.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    }
                   // Debug.Log($"{obj.name} is now Active.");
                    isFirstCardActive = true;
                    if (childList[i].card.isHidden)
                    {
                         existHiddenCards = true;
                    }
                    SetInnerVerticalPadding(i);

                    continue;
                }
                else 
                { 
                    if (childList[i].card.isHidden)
                    {
                         existHiddenCards = true;
                    }
                    childList[i].IsStacked(true);
                    SetReducedCollider(childList.Length, childList[i]);
                }
        }
        if (childList.Length == 0)
        {
            transform.gameObject.GetComponent<BoxCollider2D>().enabled = true;
            //Debug.Log($"{transform.gameObject.name} is now Active.");
            isFirstCardActive = false;
            NoHiddenCardsInSet = true;
            return null;
        }
        if (!existHiddenCards)
        {
            NoHiddenCardsInSet = true;
            SolitaireBehaviour.CheckForSolve();
        }
        //Debug.Log($"Child count of {transform.gameObject.name}: {transform.childCount}");
        isFirstCardActive = false;
        transform.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        return obj;
    }
    public List<CardObject> GetCardStack(CardObject obj)
    {
        var childList = transform.gameObject.GetComponentsInChildren<CardObject>(true);
        List<CardObject> stackedCards = new List<CardObject>();
        for (int i = childList.Length - 1; i >= 0; i--)
        {
            if (childList[i].gameObject.activeInHierarchy)
            {                             
                    if (childList[i].card == obj.card)
                    {
                    stackedCards.Add(childList[i]);
                    stackedCards.Reverse();
                    //Debug.Log("Has returned with stack.");
                    return stackedCards;
                    }
                    if (!childList[i].card.isHidden)
                    {
                        stackedCards.Add(childList[i]);
                    }                                           
            }
        }
        //Debug.Log("Has returned null.");
        return null;
    }

    public void SetCardStack(List<CardObject> stackedCards)
    {

            for (int i = 0; i< stackedCards.Count; i++)
            {
                var newStackedCard = Instantiate(InitializeWindow.assetBundle.LoadAsset<GameObject>("Card"), transform).GetComponent<CardObject>();
                newStackedCard.card = stackedCards[i].card;
                newStackedCard.FrontOfCard(stackedCards[i].card);
                Cards.Add(newStackedCard.card);

            }
        
    }

    public void SetCardStack(List<Card> stackedCards)
    {

        for (int i = 0; i < stackedCards.Count; i++)
        {
            var newStackedCard = Instantiate(InitializeWindow.assetBundle.LoadAsset<GameObject>("Card"), transform).GetComponent<CardObject>();
            newStackedCard.card = stackedCards[i];
            newStackedCard.FrontOfCard(stackedCards[i]);
            Cards.Add(newStackedCard.card);

        }

    }

    public void RemoveSourceStack(List<Card> stackedCards)
    {
        var childList = transform.gameObject.GetComponentsInChildren<CardObject>(true);
        for (int i = 0; i < childList.Length; i++)
        {           
           if (stackedCards.Contains(childList[i].card))
           {
                Cards.Remove(childList[i].card);
                childList[i].gameObject.SetActive(false);
           }

        }
    }
    public void RemoveSourceStack(List<CardObject> stackedCards)
    {
        for (int i = 0; i < stackedCards.Count; i++)
        {
            Cards.Remove(stackedCards[i].card);
            stackedCards[i].gameObject.SetActive(false);

        }
    }
    public void ResetSourceStack(List<CardObject> stackedCards)
    {
        if (stackedCards == null) { return; }
        for (int i = 0; i < stackedCards.Count; i++)
        {
            stackedCards[i].gameObject.GetComponent<Image>().enabled = true;
            stackedCards[i].gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            stackedCards[i].FrontOfCard(stackedCards[i].card);
            //Debug.Log("I am trying!");
        }
    }

    private void SetInnerVerticalPadding(int count)
    {
        var vGroup = transform.GetComponent<VerticalLayoutGroup>();
        vGroup.spacing = -105;
        if (count <= 6)
        {
            return;
        }
        vGroup.spacing -= (count - 6) * 3;
    }

    private void SetReducedCollider(int totalCount, CardObject obj)
    {
        BoxCollider2D boxCollider2D = obj.GetComponent<BoxCollider2D>();
        Vector2 newSize;
        Vector2 newOffset;

        if (!obj.card.isStacked)
        {
            return;
        }
        if (totalCount <= 7){  return; }
        if (totalCount >= 15) {
            newSize = new(86, 3);
            newOffset = new(0, 70);
            boxCollider2D.size = newSize;
            boxCollider2D.offset = newOffset;
            return; }
        newSize = new(86,27 - ((totalCount - 7)*3));
        newOffset = new(0, 54 + ((totalCount - 7) * 2));
        boxCollider2D.size = newSize;
        boxCollider2D.offset = newOffset;
    }
}
