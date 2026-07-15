using Godot;

public partial class AmunsRoom : Node2D
{
	public override void _Ready()
	{
		var player = GetNode<CharacterBody2D>("/root/Player");
		var spawn = GetNode<Marker2D>("Spawn");
		var audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		var mangiBaithakTrigger = GetNode<Area2D>("TriggerMangiBaithak");
		mangiBaithakTrigger.BodyEntered += OnMangiBaithakTriggerBodyEntered;
		audioPlayer.Play();
		player.GlobalPosition = spawn.GlobalPosition;
	}

	private void OnMangiBaithakTriggerBodyEntered(Node2D body)
	{
		if (body.Name == "Player")
		{
			CallDeferred(nameof(GoToMangiBaithak));
		}
	}

	private void GoToMangiBaithak()
	{
		GetTree().ChangeSceneToFile("res://MangiBaithak.tscn");
	}
}
