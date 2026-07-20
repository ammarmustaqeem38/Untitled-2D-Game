using Godot;

public partial class AmunsRoom : Node2D
{
	private const float ZynInteractionDistance = 90f;

	private static bool zynCollected;

	private Player player;
	private Sprite2D zyn;
	private Label zynPromptLabel;
	private bool wasEPressed;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/Player");
		zyn = GetNodeOrNull<Sprite2D>("Props/Zyn");
		var audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		var mangiBaithakTrigger = GetNode<Area2D>("TriggerMangiBaithak");
		mangiBaithakTrigger.BodyEntered += OnMangiBaithakTriggerBodyEntered;
		audioPlayer.Play();
		player.MoveToRequestedSpawn(this, "Spawn");

		if (zyn != null)
		{
			zyn.Visible = !zynCollected;
			CreateZynPromptUi();
		}
	}

	public override void _Process(double delta)
	{
		UpdateZynInteraction();
	}

	private void OnMangiBaithakTriggerBodyEntered(Node2D body)
	{
		if (body.Name == "Player")
		{
			CallDeferred(nameof(GoToMangiBaithak));
		}
	}

	private void GoToMangiBaithak()
	{
		GetTree().ChangeSceneToFile("res://MangiBaithak.tscn");
	}

	private void CreateZynPromptUi()
	{
		zynPromptLabel = new Label
		{
			Text = "e",
			Position = zyn.GlobalPosition + new Vector2(-8, -38),
			Visible = false,
			ZIndex = 100,
		};
		zynPromptLabel.AddThemeFontSizeOverride("font_size", 24);
		zynPromptLabel.AddThemeColorOverride("font_color", Colors.White);
		zynPromptLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
		zynPromptLabel.AddThemeConstantOverride("outline_size", 4);
		AddChild(zynPromptLabel);
	}

	private void UpdateZynInteraction()
	{
		if (zyn == null || zynPromptLabel == null || zynCollected)
		{
			if (zynPromptLabel != null)
			{
				zynPromptLabel.Visible = false;
			}

			return;
		}

		var isNearZyn = player.GlobalPosition.DistanceTo(zyn.GlobalPosition) <= ZynInteractionDistance;
		var ePressed = Input.IsKeyPressed(Key.E);

		zynPromptLabel.Visible = isNearZyn && !player.IsInteractionUiActive;

		if (isNearZyn && !player.IsInteractionUiActive && ePressed && !wasEPressed)
		{
			zynCollected = true;
			zyn.Visible = false;
			zynPromptLabel.Visible = false;
		}

		wasEPressed = ePressed;
	}
}
