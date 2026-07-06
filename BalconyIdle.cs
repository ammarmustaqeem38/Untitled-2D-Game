using Godot;

public partial class BalconyIdle : Node2D
{
	public override void _Ready()
	{
		var player = GetNode<CharacterBody2D>("/root/Player");
		var spawn = GetNode<Marker2D>("Spawn");

		player.GlobalPosition = spawn.GlobalPosition;
		
		var audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		audioPlayer.Play();
	}
}
