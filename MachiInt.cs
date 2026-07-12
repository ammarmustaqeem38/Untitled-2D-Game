using Godot;

public partial class MachiInt : Node2D
{
	private static readonly Color[] TvStrobeColors =
	{
		new(1.0f, 0.92f, 0.12f, 1.0f),
		new(0.12f, 1.0f, 0.25f, 1.0f),
		new(1.0f, 0.12f, 0.08f, 1.0f),
		new(0.26f, 0.62f, 1.0f, 1.0f),
	};

	private Polygon2D tvLightStrobe;
	private double strobeTime;

	public override void _Ready()
	{
		var player = GetNode<CharacterBody2D>("/root/Player");
		var spawn = GetNode<Marker2D>("Spawn");
		//var audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		//audioPlayer.Play();
		player.GlobalPosition = spawn.GlobalPosition;
		tvLightStrobe = GetNodeOrNull<Polygon2D>("Lighting/TVLightStrobe");
	}

	public override void _Process(double delta)
	{
		if (tvLightStrobe == null)
		{
			return;
		}

		strobeTime += delta;
		var alpha = 0.16f + 0.12f * (1.0f + Mathf.Sin((float)strobeTime * 9.0f)) * 0.5f;
		var color = TvStrobeColors[(int)(strobeTime / 0.22) % TvStrobeColors.Length];
		tvLightStrobe.Color = new Color(color.R, color.G, color.B, alpha);
	}
}
