using UnityEngine;

// ==============================================
// Your input actions type must be defined here.
// ==============================================
using DefaultInputActions = UnityEngine.InputSystem.DefaultInputActions;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        private DefaultInputActions defaultInputActions;

        private void CreateInputActions() => this.defaultInputActions = new DefaultInputActions();
    }
}
