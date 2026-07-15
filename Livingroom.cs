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

	private static readonly string[] AshfaqRepeatDialogue =
	{
		"Ashfaq: \"I thought I told you to leave me alone\"",
	};

	private const float AshfaqInteractionDistance = 190f;
	private const double AshfaqStrobeDuration = 1.2;
	private const double AshfaqStrobeInterval = 0.045;

	private static int ashfaqInteractionCount;

	private Player player;
	private AnimatedSprite2D ashfaq;
	private Label promptLabel;
	private bool wasEPressed;
	private double ashfaqStrobeRemaining;
	private double ashfaqStrobeTime;

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
			HandleAshfaqInteraction();
		}

		UpdateAshfaqStrobe(delta);
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

	private void HandleAshfaqInteraction()
	{
		promptLabel.Visible = false;

		if (ashfaqInteractionCount == 0)
		{
			ashfaqInteractionCount++;
			player.StartDialogue(AshfaqDialogue);
			return;
		}

		if (ashfaqInteractionCount == 1)
		{
			ashfaqInteractionCount++;
			player.StartDialogue(AshfaqRepeatDialogue);
			return;
		}

		ashfaqInteractionCount++;
		StartAshfaqStrobe();
	}

	private void StartAshfaqStrobe()
	{
		ashfaqStrobeRemaining = AshfaqStrobeDuration;
		ashfaqStrobeTime = 0.0;
	}

	private void UpdateAshfaqStrobe(double delta)
	{
		if (ashfaqStrobeRemaining <= 0.0)
		{
			return;
		}

		ashfaqStrobeRemaining -= delta;
		ashfaqStrobeTime += delta;

		var isRedFrame = ((int)(ashfaqStrobeTime / AshfaqStrobeInterval) % 2) == 0;
		ashfaq.Modulate = isRedFrame ? Colors.Red : Colors.White;

		if (ashfaqStrobeRemaining <= 0.0)
		{
			ashfaq.Modulate = Colors.White;
		}
	}
}
