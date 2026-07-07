using Godot;

public partial class GoToMachi : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}
	
		private void ChangeRoom()
	{
		GetTree().ChangeSceneToFile("res://BalconyIdle.tscn");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.Name == "Player")
		{
		   CallDeferred(nameof(ChangeRoom));
		}
	}
}
