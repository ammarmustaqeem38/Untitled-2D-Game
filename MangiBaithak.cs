using Godot;

public partial class MangiBaithak : Control
{
	private Player player;
	private bool isReturning;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/Player");
		player.SetCutsceneState(isActive: true);
	}

	public override void _ExitTree()
	{
		player?.SetCutsceneState(isActive: false);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (isReturning || @event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
		{
			return;
		}

		if (keyEvent.Keycode == Key.X || keyEvent.Keycode == Key.C || keyEvent.Keycode == Key.V)
		{
			isReturning = true;
			GetTree().ChangeSceneToFile("res://AmunsRoom.tscn");
		}
	}
}
