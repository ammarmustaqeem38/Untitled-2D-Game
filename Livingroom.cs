using Godot;

public partial class Livingroom : Node2D
{
	public override void _Ready()
	{
		var player = GetNode<CharacterBody2D>("/root/Player");
		var spawn = GetNode<Marker2D>("SpawnPoint");

		player.GlobalPosition = spawn.GlobalPosition;
	}
}
