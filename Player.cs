using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Signal]
	public delegate void MoveStepTakenEventHandler();

	[Export] 
	public float Speed = 300f;

	private static readonly string[] FirstPhoneCallDialogue =
	{
		"Soma: \"Yo vigo you wanna come over?\"",
		"Vago: \"Nah bro i got daawat\"",
		"Soma: \"Dude pleaseee\"",
		"Soma: \"Everyone's here\"",
		"Vago: \"fuck you\"",
		"Vago: \"kill yourself\"",
		"Vago: \"you dont have sisters you dont know what its like etcetc\"",
	};

	private static readonly string[] HilalPhoneCallDialogue =
	{
		"Hilal: yaar aaja na",
		"Vago: Haan jaani bas aaya",
	};

	private static readonly string[] MachiIntSomaPhoneCallDialogue =
	{
		"Soma: \"Come amun's room bitch\"",
	};

	private const float StepDistance = 48f;
	private const double FirstCallTimeoutSeconds = 6.0;
	private const double CallRetryDelaySeconds = 2.0;
	private const double AutoAcceptDelaySeconds = 1.25;
	private const double ObjectiveDelaySeconds = 0.5;
	private const double RingtoneMixRate = 44100.0;

	private AnimatedSprite2D anim;
	private Camera2D camera;
	private AudioListener2D audioListener;
	private Panel objectivePanel;
	private Label objectiveText;
	private Panel dialoguePanel;
	private Label dialogueText;
	private Label dialogueFooter;
	private AudioStreamPlayer ringtonePlayer;
	private AudioStreamGeneratorPlayback ringtonePlayback;
	private string currentObjective = "";
	private string pendingObjective = "";
	private string requestedSpawnMarkerPath = "";
	private string incomingCaller = "";
	private string lastScenePath = "";
	private string[] activeDialogue = Array.Empty<string>();
	private string[] incomingPhoneDialogue = Array.Empty<string>();
	private Action dialogueCompleted;
	private Action incomingPhoneCompleted;
	private int dialogueIndex = -1;
	private bool isDialogueActive;
	private bool dialogueLocksMovement;
	private bool isPhoneRinging;
	private bool phoneAutoAccepts;
	private bool phoneRetriesOnDecline;
	private bool firstPhoneCallStarted;
	private bool machiIntSomaPhoneCallStarted;
	private bool externalMovementLocked;
	private bool wasXPressed;
	private bool wasFPressed;
	private bool wasHPressed;
	private float stepDistanceAccumulator;
	private double phoneRingElapsed;
	private double phoneRetryCountdown;
	private double pendingObjectiveDelay = -1.0;
	private double ringtoneSampleTime;
	private double ringtonePhase;

	public override void _Ready()
	{
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		camera = GetNode<Camera2D>("Camera2D");
		audioListener = GetNode<AudioListener2D>("AudioListener2D");
		CreateDialogueUi();
		CreateObjectiveUi();
		CreateRingtonePlayer();
		UpdateCameraEnabled();
		UpdateAudioListener();
		UpdateSceneEntryEffects();
	}

	public override void _Process(double delta)
	{
		UpdateCameraEnabled();
		UpdateAudioListener();
		UpdateSceneEntryEffects();
		UpdatePhoneCall(delta);
		UpdateDialogueInput();
		UpdateDelayedObjective(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsMovementLocked)
		{
			Velocity = Vector2.Zero;
			anim.Stop();
			MoveAndSlide();
			return;
		}

		Vector2 direction = Vector2.Zero;

		if (Input.IsActionPressed("ui_right"))
			direction.X += 1;
		if (Input.IsActionPressed("ui_left"))
			direction.X -= 1;
		if (Input.IsActionPressed("ui_down"))
			direction.Y += 1;
		if (Input.IsActionPressed("ui_up"))
			direction.Y -= 1;

		if (direction != Vector2.Zero)
		{
			Velocity = direction.Normalized() * Speed;
			UpdateAnimation(direction);
		}
		else
		{
			Velocity = Vector2.Zero;
			anim.Stop(); 
		}

		var previousPosition = GlobalPosition;
		MoveAndSlide();
		UpdateMoveSteps(direction, previousPosition);
	}

	private void UpdateAnimation(Vector2 direction)
	{
		// Reset horizontal flipping
		anim.FlipH = false;

		// Prioritize horizontal movement animations
		if (direction.X > 0)
		{
			anim.Play("walk_right");
		}
		else if (direction.X < 0)
		{
			
			// to reuse "walk_right" flipped, uncomment below:
			 anim.Play("walk_right");
			 anim.FlipH = true;
		}
		// Handle vertical movement animations
		else if (direction.Y > 0)
		{
			anim.Play("walk_down");
		}
		else if (direction.Y < 0)
		{
			anim.Play("walk_up");
		}
	}

	private void UpdateCameraEnabled()
	{
		var scenePath = GetTree().CurrentScene?.SceneFilePath;
		camera.Enabled = scenePath == "res://MachiInt.tscn" || scenePath == "res://AmunsRoom.tscn";
	}

	private void UpdateAudioListener()
	{
		if (!audioListener.IsCurrent())
		{
			audioListener.MakeCurrent();
		}
	}

	private void UpdateSceneEntryEffects()
	{
		var scenePath = GetTree().CurrentScene?.SceneFilePath ?? "";

		if (scenePath == lastScenePath)
		{
			return;
		}

		lastScenePath = scenePath;

		if (scenePath == "res://MachiExt.tscn" || scenePath == "res://MachiExit.tscn")
		{
			ClearObjective();
		}
	}

	public bool IsInteractionUiActive => isDialogueActive || isPhoneRinging;

	private bool IsMovementLocked => externalMovementLocked || isPhoneRinging || (isDialogueActive && dialogueLocksMovement);

	public void SetCutsceneState(bool isActive, bool hidePlayer = true)
	{
		externalMovementLocked = isActive;
		Visible = !(isActive && hidePlayer);

		if (isActive)
		{
			Velocity = Vector2.Zero;
			anim.Stop();
		}
	}

	public void StartDialogue(string[] lines, Action onCompleted = null, bool lockMovement = false)
	{
		if (lines == null || lines.Length == 0)
		{
			onCompleted?.Invoke();
			return;
		}

		activeDialogue = lines;
		dialogueCompleted = onCompleted;
		dialogueLocksMovement = lockMovement;
		dialogueIndex = 0;
		isDialogueActive = true;
		dialoguePanel.Visible = true;
		ShowDialogueLine();
	}

	public void TryStartFirstPhoneCall()
	{
		if (firstPhoneCallStarted)
		{
			return;
		}

		firstPhoneCallStarted = true;
		StartIncomingPhoneCall(
			"Soma",
			FirstPhoneCallDialogue,
			() => SetObjectiveAfterDelay("Find joint"),
			autoAccept: false,
			retryOnDecline: true);
	}

	public void StartHilalPhoneCall()
	{
		StartIncomingPhoneCall(
			"Hilal",
			HilalPhoneCallDialogue,
			() => SetObjectiveAfterDelay("go clifton"),
			autoAccept: true,
			retryOnDecline: false);
	}

	public void StartMachiIntSomaPhoneCall()
	{
		if (machiIntSomaPhoneCallStarted)
		{
			return;
		}

		machiIntSomaPhoneCallStarted = true;
		StartIncomingPhoneCall(
			"Soma",
			MachiIntSomaPhoneCallDialogue,
			autoAccept: true,
			retryOnDecline: false);
	}

	private void UpdateDialogueInput()
	{
		var xPressed = Input.IsKeyPressed(Key.X);

		if (isDialogueActive && xPressed && !wasXPressed)
		{
			AdvanceDialogue();
		}

		wasXPressed = xPressed;
	}

	private void ShowDialogueLine()
	{
		dialogueText.Text = activeDialogue[dialogueIndex];
		dialogueFooter.Text = "x: next";
		dialogueFooter.Visible = dialogueIndex == 0;
	}

	private void AdvanceDialogue()
	{
		dialogueIndex++;

		if (dialogueIndex >= activeDialogue.Length)
		{
			var completed = dialogueCompleted;
			activeDialogue = Array.Empty<string>();
			dialogueCompleted = null;
			dialogueIndex = -1;
			isDialogueActive = false;
			dialogueLocksMovement = false;
			dialogueText.Text = "";
			dialogueFooter.Visible = false;
			dialoguePanel.Visible = false;
			completed?.Invoke();
			return;
		}

		ShowDialogueLine();
	}

	private void StartIncomingPhoneCall(
		string caller,
		string[] dialogue,
		Action onCompleted = null,
		bool autoAccept = false,
		bool retryOnDecline = false)
	{
		incomingCaller = caller;
		incomingPhoneDialogue = dialogue;
		incomingPhoneCompleted = onCompleted;
		phoneAutoAccepts = autoAccept;
		phoneRetriesOnDecline = retryOnDecline;
		phoneRingElapsed = 0.0;
		isPhoneRinging = true;
		dialoguePanel.Visible = true;
		dialogueText.Text = $"Incoming call: {incomingCaller}";
		dialogueFooter.Text = phoneAutoAccepts ? "answering..." : "f: pick up    h: hang up";
		dialogueFooter.Visible = true;
		StartRingtone();
	}

	private void UpdatePhoneCall(double delta)
	{
		if (phoneRetryCountdown > 0.0)
		{
			phoneRetryCountdown -= delta;

			if (phoneRetryCountdown <= 0.0)
			{
				StartIncomingPhoneCall(
					incomingCaller,
					incomingPhoneDialogue,
					incomingPhoneCompleted,
					autoAccept: true,
					retryOnDecline: false);
			}
		}

		if (!isPhoneRinging)
		{
			return;
		}

		phoneRingElapsed += delta;
		UpdateRingtone();

		var fPressed = Input.IsKeyPressed(Key.F);
		var hPressed = Input.IsKeyPressed(Key.H);

		if (phoneAutoAccepts)
		{
			if (phoneRingElapsed >= AutoAcceptDelaySeconds)
			{
				AcceptPhoneCall();
			}
		}
		else if (fPressed && !wasFPressed)
		{
			AcceptPhoneCall();
		}
		else if ((hPressed && !wasHPressed) || phoneRingElapsed >= FirstCallTimeoutSeconds)
		{
			DeclineFirstPhoneCall();
		}

		wasFPressed = fPressed;
		wasHPressed = hPressed;
	}

	private void AcceptPhoneCall()
	{
		isPhoneRinging = false;
		StopRingtone();
		StartDialogue(incomingPhoneDialogue, incomingPhoneCompleted, lockMovement: true);
	}

	private void DeclineFirstPhoneCall()
	{
		isPhoneRinging = false;
		StopRingtone();
		dialogueText.Text = "";
		dialogueFooter.Visible = false;
		dialoguePanel.Visible = false;

		if (phoneRetriesOnDecline)
		{
			phoneRetryCountdown = CallRetryDelaySeconds;
		}
	}

	private void SetObjectiveAfterDelay(string objective)
	{
		pendingObjective = objective;
		pendingObjectiveDelay = ObjectiveDelaySeconds;
	}

	private void UpdateDelayedObjective(double delta)
	{
		if (pendingObjectiveDelay < 0.0)
		{
			return;
		}

		pendingObjectiveDelay -= delta;

		if (pendingObjectiveDelay <= 0.0)
		{
			SetObjective(pendingObjective);
			pendingObjective = "";
			pendingObjectiveDelay = -1.0;
		}
	}

	private void CreateDialogueUi()
	{
		var dialogueLayer = new CanvasLayer
		{
			Name = "DialogueLayer",
			Layer = 60,
		};
		AddChild(dialogueLayer);

		dialoguePanel = new Panel
		{
			Visible = false,
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			AnchorTop = 1.0f,
			AnchorBottom = 1.0f,
			OffsetLeft = -340f,
			OffsetRight = 340f,
			OffsetTop = -150f,
			OffsetBottom = -42f,
		};

		var panelStyle = new StyleBoxFlat
		{
			BgColor = Colors.Black,
		};
		dialoguePanel.AddThemeStyleboxOverride("panel", panelStyle);
		dialogueLayer.AddChild(dialoguePanel);

		dialogueText = new Label
		{
			Position = new Vector2(24, 18),
			Size = new Vector2(632, 58),
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
		};
		dialogueText.AddThemeFontSizeOverride("font_size", 24);
		dialogueText.AddThemeColorOverride("font_color", Colors.White);
		dialoguePanel.AddChild(dialogueText);

		dialogueFooter = new Label
		{
			Text = "x: next",
			Position = new Vector2(24, 78),
			Size = new Vector2(632, 24),
		};
		dialogueFooter.AddThemeFontSizeOverride("font_size", 18);
		dialogueFooter.AddThemeColorOverride("font_color", Colors.White);
		dialoguePanel.AddChild(dialogueFooter);
	}

	private void CreateRingtonePlayer()
	{
		ringtonePlayer = new AudioStreamPlayer
		{
			Name = "RingtonePlayer",
			Stream = new AudioStreamGenerator
			{
				MixRate = (float)RingtoneMixRate,
				BufferLength = 0.12f,
			},
			VolumeDb = -10f,
		};
		AddChild(ringtonePlayer);
	}

	private void StartRingtone()
	{
		ringtoneSampleTime = 0.0;
		ringtonePhase = 0.0;
		ringtonePlayer.Play();
		ringtonePlayback = ringtonePlayer.GetStreamPlayback() as AudioStreamGeneratorPlayback;
		UpdateRingtone();
	}

	private void StopRingtone()
	{
		ringtonePlayer.Stop();
		ringtonePlayback = null;
	}

	private void UpdateRingtone()
	{
		if (ringtonePlayback == null)
		{
			return;
		}

		var framesAvailable = ringtonePlayback.GetFramesAvailable();

		for (var i = 0; i < framesAvailable; i++)
		{
			var beatTime = ringtoneSampleTime % 1.15;
			var isToneOn = beatTime < 0.32 || (beatTime >= 0.45 && beatTime < 0.77);
			var sample = 0.0f;

			if (isToneOn)
			{
				var frequency = beatTime < 0.32 ? 880.0 : 660.0;
				ringtonePhase += Math.PI * 2.0 * frequency / RingtoneMixRate;
				sample = (float)Math.Sin(ringtonePhase) * 0.2f;
			}

			ringtonePlayback.PushFrame(new Vector2(sample, sample));
			ringtoneSampleTime += 1.0 / RingtoneMixRate;
		}
	}

	private void UpdateMoveSteps(Vector2 direction, Vector2 previousPosition)
	{
		if (direction == Vector2.Zero)
		{
			return;
		}

		stepDistanceAccumulator += GlobalPosition.DistanceTo(previousPosition);

		while (stepDistanceAccumulator >= StepDistance)
		{
			stepDistanceAccumulator -= StepDistance;
			EmitSignal(SignalName.MoveStepTaken);
		}
	}

	public void SetObjective(string objective)
	{
		currentObjective = objective;
		objectiveText.Text = currentObjective;
		objectivePanel.Visible = !string.IsNullOrWhiteSpace(currentObjective);
	}

	public bool HasObjective(string objective)
	{
		return string.Equals(currentObjective, objective, StringComparison.Ordinal);
	}

	public void RequestSceneSpawn(string markerPath)
	{
		requestedSpawnMarkerPath = markerPath;
	}

	public void MoveToRequestedSpawn(Node sceneRoot, string defaultMarkerPath = "")
	{
		var markerPath = string.IsNullOrWhiteSpace(requestedSpawnMarkerPath)
			? defaultMarkerPath
			: requestedSpawnMarkerPath;

		requestedSpawnMarkerPath = "";

		if (string.IsNullOrWhiteSpace(markerPath))
		{
			return;
		}

		var marker = sceneRoot.GetNodeOrNull<Marker2D>(markerPath);

		if (marker == null && !string.IsNullOrWhiteSpace(defaultMarkerPath))
		{
			marker = sceneRoot.GetNodeOrNull<Marker2D>(defaultMarkerPath);
		}

		if (marker != null)
		{
			GlobalPosition = marker.GlobalPosition;
		}
	}

	public void ClearObjective()
	{
		pendingObjective = "";
		pendingObjectiveDelay = -1.0;
		SetObjective("");
	}

	private void CreateObjectiveUi()
	{
		var objectiveLayer = new CanvasLayer
		{
			Name = "ObjectiveLayer",
			Layer = 50,
		};
		AddChild(objectiveLayer);

		objectivePanel = new Panel
		{
			Visible = false,
			AnchorLeft = 1.0f,
			AnchorRight = 1.0f,
			AnchorTop = 0.0f,
			AnchorBottom = 0.0f,
			OffsetLeft = -390f,
			OffsetRight = -24f,
			OffsetTop = 24f,
			OffsetBottom = 92f,
		};

		var panelStyle = new StyleBoxFlat
		{
			BgColor = new Color(0f, 0f, 0f, 0.68f),
		};
		objectivePanel.AddThemeStyleboxOverride("panel", panelStyle);
		objectiveLayer.AddChild(objectivePanel);

		var exclamation = new Label
		{
			Text = "!",
			Position = new Vector2(18, 4),
			Size = new Vector2(42, 58),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};
		exclamation.AddThemeFontSizeOverride("font_size", 44);
		exclamation.AddThemeColorOverride("font_color", new Color(1f, 0.93f, 0.18f, 1f));
		exclamation.AddThemeColorOverride("font_outline_color", Colors.Black);
		exclamation.AddThemeConstantOverride("outline_size", 7);
		objectivePanel.AddChild(exclamation);

		objectiveText = new Label
		{
			Position = new Vector2(66, 19),
			Size = new Vector2(276, 36),
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			VerticalAlignment = VerticalAlignment.Center,
		};
		objectiveText.AddThemeFontSizeOverride("font_size", 20);
		objectiveText.AddThemeColorOverride("font_color", Colors.White);
		objectivePanel.AddChild(objectiveText);
	}
}
