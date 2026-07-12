using Godot;

public partial class Livingroom : Node2D
{
	private static readonly string[] AshfaqDialogue =
	{
		"Vago: \"Dad?\"",
		"Ashfaq: \"...\"",
		"Ashfaq: \"Can't you see I'm busy?\"",
		"Ashfaq: \"You're a fucking disgrace\"",
		"Ashfaq: \"Get out of my sight\"",
	};

	private const float AshfaqInteractionDistance = 190f;

	private CharacterBody2D player;
	private Node2D ashfaq;
	private Label promptLabel;
	private Panel dialoguePanel;
	private Label dialogueText;
	private int dialogueIndex = -1;
	private bool isDialogueActive;
	private bool wasEPressed;
	private bool wasXPressed;

	public override void _Ready()
	{
		player = GetNode<CharacterBody2D>("/root/Player");
		var spawn = GetNode<Marker2D>("Spawn");
		var audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		audioPlayer.Play();
		
		var sprite = GetNode<AnimatedSprite2D>("Ashfaq/AnimatedSprite2D");
		sprite.Play("overhead_press"); // Use your animation name
		ashfaq = sprite;
		
		player.GlobalPosition = spawn.GlobalPosition;
		CreateDialogueUi();
	}

	public override void _Process(double delta)
	{
		var isNearAshfaq = player.GlobalPosition.DistanceTo(ashfaq.GlobalPosition) <= AshfaqInteractionDistance;
		var ePressed = Input.IsKeyPressed(Key.E);
		var xPressed = Input.IsKeyPressed(Key.X);

		promptLabel.Visible = isNearAshfaq && !isDialogueActive;

		if (isNearAshfaq && !isDialogueActive && ePressed && !wasEPressed)
		{
			StartDialogue();
		}
		else if (isDialogueActive && xPressed && !wasXPressed)
		{
			AdvanceDialogue();
		}

		wasEPressed = ePressed;
		wasXPressed = xPressed;
	}

	private void CreateDialogueUi()
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

		var dialogueLayer = new CanvasLayer
		{
			Name = "DialogueLayer",
		};
		AddChild(dialogueLayer);

		dialoguePanel = new Panel
		{
			Visible = false,
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			AnchorTop = 1.0f,
			AnchorBottom = 1.0f,
			OffsetLeft = -340f,
			OffsetRight = 340f,
			OffsetTop = -150f,
			OffsetBottom = -42f,
		};

		var panelStyle = new StyleBoxFlat
		{
			BgColor = Colors.Black,
		};
		dialoguePanel.AddThemeStyleboxOverride("panel", panelStyle);
		dialogueLayer.AddChild(dialoguePanel);

		dialogueText = new Label
		{
			Position = new Vector2(24, 18),
			Size = new Vector2(632, 58),
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
		};
		dialogueText.AddThemeFontSizeOverride("font_size", 24);
		dialogueText.AddThemeColorOverride("font_color", Colors.White);
		dialoguePanel.AddChild(dialogueText);

		var nextLabel = new Label
		{
			Text = "x: next",
			Position = new Vector2(24, 78),
			Size = new Vector2(632, 24),
		};
		nextLabel.AddThemeFontSizeOverride("font_size", 18);
		nextLabel.AddThemeColorOverride("font_color", Colors.White);
		dialoguePanel.AddChild(nextLabel);
	}

	private void StartDialogue()
	{
		isDialogueActive = true;
		dialogueIndex = 0;
		dialoguePanel.Visible = true;
		dialogueText.Text = AshfaqDialogue[dialogueIndex];
		promptLabel.Visible = false;
	}

	private void AdvanceDialogue()
	{
		dialogueIndex++;

		if (dialogueIndex >= AshfaqDialogue.Length)
		{
			isDialogueActive = false;
			dialogueIndex = -1;
			dialoguePanel.Visible = false;
			dialogueText.Text = "";
			return;
		}

		dialogueText.Text = AshfaqDialogue[dialogueIndex];
	}
}
