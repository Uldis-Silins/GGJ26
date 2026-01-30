using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Data/Card Data", order = 0)]
public class CardData : ScriptableObject
{
    [Serializable]
    public class Card
    {
        public string cardId = "";
        public GameObject cardPrefab = null;
    }
    
    [field: SerializeField] public Card[] Cards { get; private set; }
}