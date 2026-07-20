using Godot;

public partial class MachiInt : Node2D
{
	private static readonly Color[] TvStrobeColors =
	{
		new(1f, 0.04f, 0.03f, 0.24f),
		new(0.04f, 0.2f, 1f, 0.24f),
		new(0.05f, 1f, 0.1f, 0.24f),
		new(1f, 0.92f, 0.12f, 0.24f),
		new(1f, 1f, 1f, 0.24f),
	};

	private const double TvStrobeIntervalSeconds = 0.12;

	private Polygon2D tvLightStrobe;
	private double tvStrobeElapsed;
	private int tvStrobeColorIndex;

	public override void _Ready()
	{
		var player = GetNode<Player>("/root/Player");
		var audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		tvLightStrobe = GetNode<Polygon2D>("Lighting/TVLightStrobe");
		audioPlayer.Play();
		player.MoveToRequestedSpawn(this, "Spawn");
		player.StartMachiIntSomaPhoneCall();
		UpdateTvLightStrobe();
	}

	public override void _Process(double delta)
	{
		tvStrobeElapsed += delta;

		while (tvStrobeElapsed >= TvStrobeIntervalSeconds)
		{
			tvStrobeElapsed -= TvStrobeIntervalSeconds;
			tvStrobeColorIndex = (tvStrobeColorIndex + 1) % TvStrobeColors.Length;
			UpdateTvLightStrobe();
		}
	}

	private void UpdateTvLightStrobe()
	{
		tvLightStrobe.Color = TvStrobeColors[tvStrobeColorIndex];
	}
}
