using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum GameStateType { None, Setup, Playing, GameOver }
    
    public ImageController imageController;
    
    public Transform directionalLight;

    [Header(("Debug"))] 
    public TextMeshProUGUI debugTextPrefab;
    public Canvas debugCanvas;
    public Camera debugTextCamera;

    private CardEnemy m_joker;
    private CardCharacter m_currentCharacter;
    private CardInteraction m_currentInteraction;
    
    private List<CardInteraction> m_playedInteractions = new();
    
    private readonly Dictionary<string, CardCharacter> m_spawnedCharacters = new();
    private readonly Dictionary<string, CardInteraction>  m_spawnedInteractions = new();
    
    private Dictionary<CardCharacter, TextMeshProUGUI> m_characterDebugTexts = new();
    private Dictionary<CardInteraction, TextMeshProUGUI> m_interactionDebugTexts = new();
    private TextMeshProUGUI m_enemyDebugText;

    public GameStateType CurrentGameState { get; private set; } = GameStateType.Setup;
    
    private void OnEnable()
    {
        imageController.onCharacterSpawned += OnCharacterSpawned;
        imageController.onInteractionSpawned += OnInteractionSpawned;
        imageController.onEnemySpawned += OnEnemySpawned;
    }

    private void OnDisable()
    {
        imageController.onCharacterSpawned -= OnCharacterSpawned;
        imageController.onInteractionSpawned -= OnInteractionSpawned;
        imageController.onEnemySpawned -= OnEnemySpawned;
    }

    private void Update()
    {
        foreach (var interaction in m_interactionDebugTexts)
        {
            interaction.Value.transform.position = interaction.Key.transform.position + Vector3.up * 0.05f;
            interaction.Value.transform.rotation =
                Quaternion.LookRotation(interaction.Value.transform.position - debugTextCamera.transform.position);
        }

        foreach (var character in m_characterDebugTexts)
        {
            character.Value.transform.position = character.Key.transform.position + Vector3.up * 0.2f;
            character.Value.transform.rotation = Quaternion.LookRotation(character.Value.transform.position - debugTextCamera.transform.position);
            character.Value.text = character.Key.HP.ToString();
        }

        if (m_enemyDebugText)
        {
            m_enemyDebugText.transform.rotation = Quaternion.LookRotation(m_enemyDebugText.transform.position - debugTextCamera.transform.position);
        }

        if (m_joker != null && m_currentCharacter != null)
        {
            Vector3 lookDir = m_currentCharacter.transform.position - m_joker.transform.position;
            lookDir.y = 0;
            m_joker.characterTransform.transform.rotation = Quaternion.LookRotation(lookDir.normalized);
            m_enemyDebugText.text = m_joker.health.ToString();
        }
    }

    private void OnCharacterSpawned(CardData.Card card, Pose pose, string imageName)
    {
        if (CurrentGameState == GameStateType.Playing && m_joker != null)
        {
            if ((m_currentCharacter == null || m_currentCharacter.IsDead) && !m_spawnedCharacters.ContainsKey(imageName))
            {
                var instance = Instantiate(card.cardPrefab, pose.position, pose.rotation);
                var character = instance.GetComponent<CardCharacter>();
                character.TargetEnemy = m_joker;
                
                m_currentCharacter = character;
                m_spawnedCharacters.Add(imageName, character);
                Vector3 lookDir = m_joker.transform.position - instance.transform.position;
                lookDir.y = 0;
                instance.transform.rotation = Quaternion.LookRotation(lookDir);
                
                m_currentCharacter.onKilled.AddListener(() => { 
                    m_currentCharacter = null; 
                    m_currentInteraction  = null; 
                    imageController.InteractionSpawned = false; 
                    imageController.CharacterSpawned = false; 
                });

                imageController.CharacterSpawned = true;
                
                var spawned = Instantiate(debugTextPrefab, debugCanvas.transform);
                m_characterDebugTexts.Add(m_currentCharacter, spawned);
                spawned.text = character.face + " of " + character.suit;
            }   
        }
    }

    private void OnInteractionSpawned(CardData.Card card, Pose pose, string imageName)
    {
        if (m_spawnedInteractions.ContainsKey(imageName)) return;
        
        if (m_currentInteraction == null && m_currentCharacter != null)
        {
            var instance = Instantiate(card.cardPrefab, pose.position, pose.rotation);
            var interaction = instance.GetComponent<CardInteraction>();

            if (m_playedInteractions.Contains(interaction)) return;
            
            m_currentInteraction = interaction;
            m_currentCharacter.SetDestination(interaction);
            m_currentCharacter.onInteractionReached.AddListener((i) =>
            {
                m_currentInteraction = null; 
                imageController.InteractionSpawned = false;
            });
            
            imageController.InteractionSpawned = true;
            
            m_spawnedInteractions.Add(imageName, interaction);

            var spawned = Instantiate(debugTextPrefab, debugCanvas.transform);
            m_interactionDebugTexts.Add(interaction, spawned);
            spawned.text = interaction.value + " of " + interaction.suit;
        }
    }

    private void OnEnemySpawned(CardData.Card card, Pose pose, string imageName)
    {
        if (CurrentGameState != GameStateType.Playing && m_joker == null)
        {
            var instance = Instantiate(card.cardPrefab, pose.position, pose.rotation);
            m_joker = instance.GetComponent<CardEnemy>();
            CurrentGameState = GameStateType.Playing;
            
            directionalLight.position = debugTextCamera.transform.position + Vector3.up;
            directionalLight.rotation = Quaternion.LookRotation(m_joker.transform.position - directionalLight.transform.position);

            var spawned = Instantiate(debugTextPrefab, debugCanvas.transform);
            m_enemyDebugText = spawned;
            m_enemyDebugText.transform.position = spawned.transform.position + Vector3.up * 0.2f;
            spawned.text = m_joker.health.ToString();
        }
    }
}