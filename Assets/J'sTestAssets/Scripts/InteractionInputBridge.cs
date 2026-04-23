using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionInputBridge : MonoBehaviour
{
    private PlayerInput playerInput;
    
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        //Debug.Log($"[InteractionInputBridge] PlayerInput found: {(playerInput != null)}");
        //Debug.Log($"[InteractionInputBridge] Actions asset: {playerInput?.actions?.name}");
        
        if (playerInput != null)
        {
            var interactAction = playerInput.actions["Interact"];
            //Debug.Log($"[InteractionInputBridge] 'Interact' action exists: {(interactAction != null)}");
            
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
                interactAction.canceled += OnInteractCanceled;
                //Debug.Log("[InteractionInputBridge] ✓ Subscribed to Interact action");
            }
        }
    }
    
    private void OnDestroy()
    {
        var interactAction = playerInput?.actions["Interact"];
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
            interactAction.canceled -= OnInteractCanceled;
        }
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        //Debug.Log("[InteractionInputBridge] ✓✓✓ INTERACT PERFORMED!");
        
        TurnableStone[] allStones = FindObjectsByType<TurnableStone>(FindObjectsSortMode.None);
        //Debug.Log($"[InteractionInputBridge] Found {allStones.Length} stones");
        
        foreach (TurnableStone stone in allStones)
        {
            //Debug.Log($"[InteractionInputBridge] Calling stone: {stone.gameObject.name}");
            stone.OnInteractInput();
        }
    }
    
    private void OnInteractCanceled(InputAction.CallbackContext context)
    {
        //Debug.Log("[InteractionInputBridge] Interact canceled");
    }
}
