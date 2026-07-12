using Godot;

public partial class MachiInt : Node2D
{
	public override void _Ready()
	{
		var player = GetNode<CharacterBody2D>("/root/Player");
		var spawn = GetNode<Marker2D>("Spawn");
		//var audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		//audioPlayer.Play();
		player.GlobalPosition = spawn.GlobalPosition;
	}
}
