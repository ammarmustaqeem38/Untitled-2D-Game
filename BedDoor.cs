using Godot;

public partial class BedDoor : Area2D
{
	private const string VagoRoomLivingRoomEntranceSpawn = "LivingRoomEntrance";

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
		if (body is Player player)
		{
			player.RequestSceneSpawn(VagoRoomLivingRoomEntranceSpawn);
			CallDeferred(nameof(ChangeRoom));
		}
	}
}
