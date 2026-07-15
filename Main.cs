using Godot;

public partial class Main : Node2D
{
	private static readonly string[] JointGrailDialogue =
	{
		"Vago:\"goddammit, out again\"",
	};

	private const float JointGrailInteractionDistance = 115f;

	private Player player;
	private Node2D jointGrail;
	private Label jointPromptLabel;
	private int moveStepCount;
	private bool phoneCallTriggered;
	private bool jointGrailInteracted;
	private bool wasEPressed;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/Player");
		jointGrail = GetNode<Node2D>("JointGrail");
		player.MoveStepTaken += OnPlayerMoveStepTaken;
		CreateJointPromptUi();
	}

	public override void _Process(double delta)
	{
		UpdateJointGrailInteraction();
	}

	public override void _ExitTree()
	{
		if (player != null)
		{
			player.MoveStepTaken -= OnPlayerMoveStepTaken;
		}
	}

	private void OnPlayerMoveStepTaken()
	{
		if (phoneCallTriggered)
		{
			return;
		}

		moveStepCount++;

		if (moveStepCount < 3)
		{
			return;
		}

		phoneCallTriggered = true;
		player.TryStartFirstPhoneCall();
	}

	private void CreateJointPromptUi()
	{
		jointPromptLabel = new Label
		{
			Text = "e",
			Position = jointGrail.Position + new Vector2(-8, -52),
			Visible = false,
			ZIndex = 100,
		};
		jointPromptLabel.AddThemeFontSizeOverride("font_size", 24);
		jointPromptLabel.AddThemeColorOverride("font_color", Colors.White);
		jointPromptLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
		jointPromptLabel.AddThemeConstantOverride("outline_size", 4);
		AddChild(jointPromptLabel);
	}

	private void UpdateJointGrailInteraction()
	{
		var isNearJoint = player.GlobalPosition.DistanceTo(jointGrail.GlobalPosition) <= JointGrailInteractionDistance;
		var ePressed = Input.IsKeyPressed(Key.E);

		jointPromptLabel.Visible = isNearJoint && !jointGrailInteracted && !player.IsInteractionUiActive;

		if (isNearJoint && !jointGrailInteracted && !player.IsInteractionUiActive && ePressed && !wasEPressed)
		{
			jointGrailInteracted = true;
			jointPromptLabel.Visible = false;
			player.ClearObjective();
			player.StartDialogue(JointGrailDialogue, player.StartHilalPhoneCall);
		}

		wasEPressed = ePressed;
	}
}
