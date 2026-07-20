using Godot;

public partial class GoToMachi : Area2D
{
	private const string RequiredObjective = "go clifton";

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}
	
	private void ChangeRoom()
	{
		GetTree().ChangeSceneToFile("res://MachiExt.tscn");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player && player.HasObjective(RequiredObjective))
		{
			CallDeferred(nameof(ChangeRoom));
		}
	}
}
