using Godot;

public partial class MachiExt : Node2D
{
	public override void _Ready()
	{
		var player = GetNode<Player>("/root/Player");

		player.MoveToRequestedSpawn(this, "Spawn");
		
		var audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		audioPlayer.Play();
	}
}
