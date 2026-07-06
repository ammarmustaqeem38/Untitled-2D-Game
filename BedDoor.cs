using Godot;

public partial class BedDoor : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}
	
		private void ChangeRoom()
	{
		GetTree().ChangeSceneToFile("res://Main.tscn");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.Name == "Player")
		{
		   CallDeferred(nameof(ChangeRoom));
		}
	}
}
