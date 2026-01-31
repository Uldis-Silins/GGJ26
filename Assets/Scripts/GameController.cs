using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public ImageController imageController;
    
    private readonly Dictionary<string, CardCharacter> m_spawnedCharacters = new();
    private readonly Dictionary<string, CardInteraction>  m_spawnedInteractions = new();

    private void OnEnable()
    {
        imageController.onCharacterSpawned += OnCharacterSpawned;
        imageController.onInteractionSpawned += OnInteractionSpwned;
    }

    private void OnInteractionSpwned(CardData.Card card, Pose pose, string imageName)
    {
        var instance = Instantiate(card.cardPrefab, pose.position, pose.rotation);
        m_spawnedInteractions.Add(imageName, instance.GetComponent<CardInteraction>());
    }

    private void OnCharacterSpawned(CardData.Card card, Pose pose, string imageName)
    {
        var instance = Instantiate(card.cardPrefab, pose.position, pose.rotation);
        m_spawnedCharacters.Add(imageName, instance.GetComponent<CardCharacter>());
    }
}