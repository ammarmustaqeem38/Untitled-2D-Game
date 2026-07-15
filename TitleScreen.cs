using Godot;

public partial class TitleScreen : Control
{
	private Player player;
	private CanvasItem enterPrompt;
	private double promptTime;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/Player");
		player.SetCutsceneState(isActive: true);
		enterPrompt = GetNode<CanvasItem>("TitleStack/EnterPrompt");
	}

	public override void _ExitTree()
	{
		player?.SetCutsceneState(isActive: false);
	}

	public override void _Process(double delta)
	{
		promptTime += delta;
		var alpha = 0.18f + 0.82f * Mathf.Abs(Mathf.Sin((float)promptTime * 5.5f));
		enterPrompt.Modulate = new Color(1f, 1f, 1f, alpha);

		if (Input.IsKeyPressed(Key.Enter) || Input.IsKeyPressed(Key.KpEnter))
		{
			GetTree().ChangeSceneToFile("res://main.tscn");
		}
	}
}
