using Godot;

public partial class Livingroom : Node2D
{
	public override void _Ready()
	{
		var player = GetNode<CharacterBody2D>("/root/Player");
		var spawn = GetNode<Marker2D>("Spawn");
		var audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		audioPlayer.Play();
		
	var sprite = GetNode<AnimatedSprite2D>("Ashfaq/AnimatedSprite2D");
		sprite.Play("overhead_press"); // Use your animation name
		
		
		player.GlobalPosition = spawn.GlobalPosition;
	}
}
