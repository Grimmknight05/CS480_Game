using UnityEngine;
using UnityEngine.InputSystem;
using System;

// Author: Joshua Henrikson
// Modified by: GitHub Copilot / Architecture Refactor (April 2026)
public class InteractionInputBridge : MonoBehaviour
{
    // A global event that anything can listen to when the player presses 'Interact'
    public static event Action OnInteractPressed;

    private PlayerInput playerInput;
    
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null)
        {
            var interactAction = playerInput.actions["Interact"];
            
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
            }
        }
    }
    
    private void OnDestroy()
    {
        var interactAction = playerInput?.actions["Interact"];
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
        }
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Broadcast to the whole game: "The player pressed interact!"
        OnInteractPressed?.Invoke();
    }
}