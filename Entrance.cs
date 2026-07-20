using Godot;

public partial class Entrance : Area2D
{
	private const string AmunsRoomScenePath = "res://AmunsRoom.tscn";
	private const string MachiIntAmunReturnSpawn = "EntranceMarkers/AmunReturnSpawn";

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}
	
	private void ChangeRoom()
	{
		GetTree().ChangeSceneToFile("res://MachiInt.tscn");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			if (GetTree().CurrentScene?.SceneFilePath == AmunsRoomScenePath)
			{
				player.RequestSceneSpawn(MachiIntAmunReturnSpawn);
			}

			CallDeferred(nameof(ChangeRoom));
		}
	}
}
