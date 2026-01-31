using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageController : MonoBehaviour
{
    public event Action<CardData.Card, Pose, string> onCharacterSpawned;
    public event Action<CardData.Card, Pose, string> onInteractionSpawned;
    public event Action<CardData.Card, Pose, string> onEnemySpawned;
    
    public ARTrackedImageManager trackedImageManager;
    public CardData characterCardData;
    public CardData interactionCardData;
    public CardData enemyCardData;

    public ParticleSystem spawnParticlesPrefab;
    
    private readonly Dictionary<string, int> m_cardConfidence = new();
    private string m_prevTrackedImageName;
    
    public bool CharacterSpawned { get; set; }
    public bool InteractionSpawned { get; set; }
    
    private void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    private void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }
    
    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> trackables)
    {
        foreach (ARTrackedImage trackedImage in trackables.added)
        {
            m_cardConfidence.TryAdd(trackedImage.referenceImage.name, 0);
        }

        foreach (ARTrackedImage trackedImage in trackables.updated)
        {
            if (!string.IsNullOrEmpty(m_prevTrackedImageName) && m_cardConfidence.ContainsKey(m_prevTrackedImageName) &&
                trackedImage.referenceImage.name != m_prevTrackedImageName)
            {
                m_cardConfidence[m_prevTrackedImageName] = 0;
            }
            
            if(trackedImage.trackingState != TrackingState.Tracking) continue;
            
            m_cardConfidence[trackedImage.referenceImage.name]++;

            if (m_cardConfidence[trackedImage.referenceImage.name] > 0)
            {
                if (SpawnEnemy(trackedImage))
                {
                    m_cardConfidence[trackedImage.referenceImage.name] = 0;
                }
                else if (SpawnCharacter(trackedImage)&& !CharacterSpawned)
                {
                    m_cardConfidence[trackedImage.referenceImage.name] = 0;
                }
                else if (SpawnInteraction(trackedImage) && !InteractionSpawned)
                {
                    m_cardConfidence[trackedImage.referenceImage.name] = 0;
                }
            }
        }

        foreach (KeyValuePair<TrackableId, ARTrackedImage> removed in trackables.removed)
        {
            //CardData.Card card = Array.Find(characterCardData.Cards, c => c.cardId == removed.Value.referenceImage.name);
        }
    }

    private bool SpawnCharacter(ARTrackedImage trackedImage)
    {
        CardData.Card card = Array.Find(characterCardData.Cards, c => c.cardId == trackedImage.referenceImage.name);

        if (card != null && trackedImage.trackingState == TrackingState.Tracking)
        {
            onCharacterSpawned?.Invoke(card, trackedImage.pose, trackedImage.referenceImage.name);
            return true;
        }
        
        return false;
    }

    private bool SpawnInteraction(ARTrackedImage trackedImage)
    {
        CardData.Card card = Array.Find(interactionCardData.Cards, c => c.cardId == trackedImage.referenceImage.name);

        if (card != null && trackedImage.trackingState == TrackingState.Tracking)
        {
            onInteractionSpawned?.Invoke(card, trackedImage.pose, trackedImage.referenceImage.name);
            return true;
        }
        
        return false;
    }
    
    private bool SpawnEnemy(ARTrackedImage trackedImage)
    {
        CardData.Card card = Array.Find(enemyCardData.Cards, c => c.cardId == trackedImage.referenceImage.name);

        if (card != null && trackedImage.trackingState == TrackingState.Tracking)
        {
            onEnemySpawned?.Invoke(card, trackedImage.pose, trackedImage.referenceImage.name);
            return true;
        }
        
        return false;
    }
}