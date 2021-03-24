using Godot;

namespace MyGame
{
    public abstract partial class Bootstrap : Control
    {
        [GodotOverride]
        private void OnUnhandledKeyInput(InputEventKey e)
        {
            if (e.IsActionPressed("ui_cancel"))
            {
                GetTree().Quit();
            }
        }
    }
}
