using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageController : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public CardData cardData;

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
            CardData.Card card = Array.Find(cardData.Cards, c => c.cardId == trackedImage.referenceImage.name);
        }

        foreach (ARTrackedImage trackedImage in trackables.updated)
        {
            CardData.Card card = Array.Find(cardData.Cards, c => c.cardId == trackedImage.referenceImage.name);
        }

        foreach (KeyValuePair<TrackableId, ARTrackedImage> removed in trackables.removed)
        {
            CardData.Card card = Array.Find(cardData.Cards, c => c.cardId == removed.Value.referenceImage.name);
        }
    }
}