using Godot;

public partial class TitleScreen : Control
{
	private const double FadeDurationSeconds = 0.45;

	private Player player;
	private CanvasItem enterPrompt;
	private ColorRect fadeOverlay;
	private AudioStreamPlayer titleMusic;
	private AudioStreamPlayer enterSound;
	private double promptTime;
	private double fadeTime;
	private bool isStarting;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/Player");
		player.SetCutsceneState(isActive: true);
		enterPrompt = GetNode<CanvasItem>("TitleStack/EnterPrompt");
		fadeOverlay = GetNode<ColorRect>("FadeOverlay");
		titleMusic = GetNode<AudioStreamPlayer>("TitleMusic");
		enterSound = GetNode<AudioStreamPlayer>("EnterSound");
		fadeOverlay.Color = new Color(0f, 0f, 0f, 0f);
		titleMusic.Finished += OnTitleMusicFinished;
		titleMusic.Play();
	}

	public override void _ExitTree()
	{
		if (titleMusic != null)
		{
			titleMusic.Finished -= OnTitleMusicFinished;
		}

		player?.SetCutsceneState(isActive: false);
	}

	public override void _Process(double delta)
	{
		promptTime += delta;
		var alpha = 0.18f + 0.82f * Mathf.Abs(Mathf.Sin((float)promptTime * 5.5f));
		enterPrompt.Modulate = new Color(1f, 1f, 1f, alpha);

		if (isStarting)
		{
			UpdateFade(delta);
			return;
		}

		if (!isStarting && (Input.IsKeyPressed(Key.Enter) || Input.IsKeyPressed(Key.KpEnter)))
		{
			StartGame();
		}
	}

	private void UpdateFade(double delta)
	{
		fadeTime += delta;
		var alpha = Mathf.Clamp((float)(fadeTime / FadeDurationSeconds), 0f, 1f);
		fadeOverlay.Color = new Color(0f, 0f, 0f, alpha);
	}

	private void OnTitleMusicFinished()
	{
		if (!isStarting)
		{
			titleMusic.Play();
		}
	}

	private async void StartGame()
	{
		isStarting = true;
		fadeTime = 0.0;
		fadeOverlay.Color = new Color(0f, 0f, 0f, 0f);
		titleMusic.Stop();
		enterSound.Play();
		await ToSignal(enterSound, AudioStreamPlayer.SignalName.Finished);
		GetTree().ChangeSceneToFile("res://main.tscn");
	}
}
