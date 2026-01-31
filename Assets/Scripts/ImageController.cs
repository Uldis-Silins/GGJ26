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
    
    public ARTrackedImageManager trackedImageManager;
    public CardData characterCardData;
    public CardData interactionCardData;
    public CardData mapCardData;
    
    public Canvas debugCanvas;
    public TextMeshProUGUI debugTextPrefab;
    public Camera mainCamera;

    public ParticleSystem spawnParticlesPrefab;

    private readonly Dictionary<string, TextMeshProUGUI> m_spawnedDebugTexts = new();
    
    private readonly Dictionary<string, int> m_cardConfidence = new();
    
    public bool IsMapSpawned { get; private set; }

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
            if (!IsMapSpawned)
            {
                var mapCard = Array.Find(mapCardData.Cards, c => c.cardId == trackedImage.referenceImage.name);
                if (mapCard != null)
                {
                    Instantiate(mapCard.cardPrefab, trackedImage.pose.position, trackedImage.pose.rotation);
                    IsMapSpawned = true;
                }
            }

            m_cardConfidence.TryAdd(trackedImage.referenceImage.name, 0);
        }

        foreach (ARTrackedImage trackedImage in trackables.updated)
        {
            m_cardConfidence[trackedImage.referenceImage.name]++;

            if (m_cardConfidence[trackedImage.referenceImage.name] > 10)
            {
                if (SpawnCharacter(trackedImage))
                {
                    m_cardConfidence[trackedImage.referenceImage.name] = 0;
                }
                else if (SpawnInteraction(trackedImage))
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
            
            var debugText = Instantiate(debugTextPrefab, debugCanvas.transform);
            Quaternion rot = Quaternion.LookRotation((debugText.transform.position - mainCamera.transform.position).normalized, Vector3.up);
            debugText.rectTransform.SetPositionAndRotation(trackedImage.pose.position + Vector3.up * 0.2f, rot);
            debugText.text = trackedImage.referenceImage.name;
            m_spawnedDebugTexts.Add(trackedImage.referenceImage.name, debugText);
            
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
            
            var debugText = Instantiate(debugTextPrefab, debugCanvas.transform);
            Quaternion rot = Quaternion.LookRotation((debugText.transform.position - mainCamera.transform.position).normalized, Vector3.up);
            debugText.rectTransform.SetPositionAndRotation(trackedImage.pose.position + Vector3.up * 0.2f, rot);
            debugText.text = trackedImage.referenceImage.name;
            m_spawnedDebugTexts.Add(trackedImage.referenceImage.name, debugText);
            
            return true;
        }
        
        return false;
    }
}