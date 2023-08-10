using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardHoldStack : MonoBehaviour
{
    Card currentCard = null;

    public void SetCardFront(Card card)
    {
        currentCard = card;
        transform.GetComponent<Image>().sprite = CardArtManager.SearchCardFront(card);
    }

    public void GoodbyeCardStack()
    {
        Destroy(transform.gameObject);
    }
}
