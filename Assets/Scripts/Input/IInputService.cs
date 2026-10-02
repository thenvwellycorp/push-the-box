using System;
using UnityEngine;

namespace PushTheBox.InputSystem
{
    /// <summary>
    /// Contract for game input providers (Keyboard, Touch/Swipe, Virtual D-pad).
    /// Decouples player and level control from specific input hardware.
    /// </summary>
    public interface IInputService
    {
        event Action<Vector2Int> OnMoveInput;
        event Action OnUndoInput;
        event Action OnRestartInput;
    }
}
