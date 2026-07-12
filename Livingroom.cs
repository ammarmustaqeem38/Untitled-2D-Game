using Godot;

public partial class Livingroom : Node2D
{
	private static readonly string[] AshfaqDialogue =
	{
		"Vago: \"Dad?\"",
		"Ashfaq: \"...\"",
		"Ashfaq: \"You're a fucking disgrace\"",
		"Ashfaq: \"Get out of my sight\"",
	};

	private const float AshfaqInteractionDistance = 190f;

	private Player player;
	private Node2D ashfaq;
	private Label promptLabel;
	private bool wasEPressed;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/Player");
		var spawn = GetNode<Marker2D>("Spawn");
		var audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		audioPlayer.Play();
		
		var sprite = GetNode<AnimatedSprite2D>("Ashfaq/AnimatedSprite2D");
		sprite.Play("overhead_press"); // Use your animation name
		ashfaq = sprite;
		
		player.GlobalPosition = spawn.GlobalPosition;
		CreatePromptUi();
	}

	public override void _Process(double delta)
	{
		var isNearAshfaq = player.GlobalPosition.DistanceTo(ashfaq.GlobalPosition) <= AshfaqInteractionDistance;
		var ePressed = Input.IsKeyPressed(Key.E);

		promptLabel.Visible = isNearAshfaq && !player.IsInteractionUiActive;

		if (isNearAshfaq && !player.IsInteractionUiActive && ePressed && !wasEPressed)
		{
			StartDialogue();
		}

		wasEPressed = ePressed;
	}

	private void CreatePromptUi()
	{
		promptLabel = new Label
		{
			Text = "e",
			Position = ashfaq.GlobalPosition + new Vector2(-8, -130),
			Visible = false,
			ZIndex = 100,
		};
		promptLabel.AddThemeFontSizeOverride("font_size", 24);
		promptLabel.AddThemeColorOverride("font_color", Colors.White);
		promptLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
		promptLabel.AddThemeConstantOverride("outline_size", 4);
		AddChild(promptLabel);
	}

	private void StartDialogue()
	{
		promptLabel.Visible = false;
		player.StartDialogue(AshfaqDialogue, () => player.SetObjective("Get out of his sight"));
	}
}
