using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayerManager : MonoBehaviour
{
    public List<int> deck = new List<int>();

    public int heroHp;
    public int manaCost;
    public int defaultManaCost;

    public void Init(List<int> cardDeck)
    {
        deck = cardDeck;
        heroHp = 20;
        manaCost = 10;
        defaultManaCost = 5;
    }

    public void IncreaseManaCost()
    {
        defaultManaCost++;
        manaCost = defaultManaCost;
    }
}
