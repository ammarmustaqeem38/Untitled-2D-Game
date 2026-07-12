using Godot;

public partial class Main : Node2D
{
	private Player player;
	private int moveStepCount;
	private bool phoneCallTriggered;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/Player");
		player.MoveStepTaken += OnPlayerMoveStepTaken;
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
}
