using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageController : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public CardData characterCardData;
    public CardData mapCardData;
    
    public Canvas debugCanvas;
    public TextMeshProUGUI debugTextPrefab;
    public Camera mainCamera;
    
    private readonly Dictionary<string, CardCharacter> m_spawnedCharacters = new();
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
            if (m_spawnedCharacters.TryGetValue(trackedImage.referenceImage.name, out var character))
            {
                // character.transform.SetPositionAndRotation(
                //     trackedImage.pose.position,
                //     trackedImage.pose.rotation);
                //
                // Transform debugTransform = m_spawnedDebugTexts[trackedImage.referenceImage.name].transform;
                // debugTransform.position = trackedImage.pose.position + Vector3.up * 0.2f;
                // debugTransform.rotation = Quaternion.LookRotation((debugTransform.position - mainCamera.transform.position).normalized, Vector3.up);
            }
            else
            {
                m_cardConfidence[trackedImage.referenceImage.name]++;

                if (m_cardConfidence[trackedImage.referenceImage.name] > 10)
                {
                    if (SpawnCharacter(trackedImage))
                    {
                        m_cardConfidence[trackedImage.referenceImage.name] = 0;
                    }
                }
            }
        }

        foreach (KeyValuePair<TrackableId, ARTrackedImage> removed in trackables.removed)
        {
            //CardData.Card card = Array.Find(characterCardData.Cards, c => c.cardId == removed.Value.referenceImage.name);
            if (m_spawnedCharacters.ContainsKey(removed.Value.referenceImage.name))
            {
                Destroy(m_spawnedCharacters[removed.Value.referenceImage.name].gameObject);
                m_spawnedCharacters.Remove(removed.Value.referenceImage.name);
            }
        }
    }

    private bool SpawnCharacter(ARTrackedImage trackedImage)
    {
        CardData.Card card = Array.Find(characterCardData.Cards, c => c.cardId == trackedImage.referenceImage.name);

        if (card != null && trackedImage.trackingState == TrackingState.Tracking && GetClosestSpawnDistance(trackedImage.pose.position) >= 0.15f)
        {
            var instance = Instantiate(card.cardPrefab, trackedImage.pose.position, trackedImage.pose.rotation);
            m_spawnedCharacters.Add(trackedImage.referenceImage.name, instance.GetComponent<CardCharacter>());
            var debugText = Instantiate(debugTextPrefab, debugCanvas.transform);
            Quaternion rot = Quaternion.LookRotation((debugText.transform.position - mainCamera.transform.position).normalized, Vector3.up);
            debugText.rectTransform.SetPositionAndRotation(trackedImage.pose.position + Vector3.up * 0.2f, rot);
            debugText.text = trackedImage.referenceImage.name;
            m_spawnedDebugTexts.Add(trackedImage.referenceImage.name, debugText);
            return true;
        }
        
        return false;
    }

    private float GetClosestSpawnDistance(Vector3 location)
    {
        float closestDistance = float.PositiveInfinity;
        Transform closest = null;

        foreach (var spawnedCharacter in m_spawnedCharacters)
        {
            float currentDistance =
                Vector3.Distance(location, spawnedCharacter.Value.transform.position);
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closest = spawnedCharacter.Value.transform;
            }
        }

        return closestDistance;
    }
}