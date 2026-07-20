using Godot;

public partial class MangiBaithak : Control
{
	private enum DialogueState
	{
		ShowingIntro,
		AwaitingChoice,
		ShowingPlayerResponse,
		ShowingNpcResponse,
	}

	private const string ChoicePrompt = "(x) normal response    (c) cunt response    (v) nothingburger";
	private const string AmunsRoomBathroomDoorSpawn = "MangiBaithakReturnSpawn";

	private Player player;
	private Label dialogueText;
	private Label choiceText;
	private DialogueState dialogueState;
	private string selectedNpcResponse = "";
	private bool isReturning;

	public override void _Ready()
	{
		player = GetNode<Player>("/root/Player");
		dialogueText = GetNode<Label>("DialoguePanel/DialogueText");
		choiceText = GetNode<Label>("DialoguePanel/ChoiceText");
		player.SetCutsceneState(isActive: true);
		ShowDialogue("Amin: \"Haan bc\"", "x: next");
		dialogueState = DialogueState.ShowingIntro;
	}

	public override void _ExitTree()
	{
		player?.SetCutsceneState(isActive: false);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (isReturning || @event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
		{
			return;
		}

		if (dialogueState == DialogueState.ShowingIntro)
		{
			if (keyEvent.Keycode == Key.X)
			{
				AdvanceDialogue();
			}
		}
		else if (dialogueState == DialogueState.AwaitingChoice)
		{
			SelectChoice(keyEvent.Keycode);
		}
		else if (keyEvent.Keycode == Key.X)
		{
			AdvanceDialogue();
		}
	}

	private void SelectChoice(Key keycode)
	{
		switch (keycode)
		{
			case Key.X:
				ShowDialogue("Vago: \"bas yaar\"", "x: next");
				selectedNpcResponse = "Amin: \"chal ez\"";
				dialogueState = DialogueState.ShowingPlayerResponse;
				break;
			case Key.C:
				ShowDialogue("Vago: \"i fucking hate you guys\"", "x: next");
				selectedNpcResponse = "Amin: \"gaandu\"";
				dialogueState = DialogueState.ShowingPlayerResponse;
				break;
			case Key.V:
				ShowDialogue("Vago: \"...\"", "x: next");
				selectedNpcResponse = "Amin: \"chal tera baap aagaya\"";
				dialogueState = DialogueState.ShowingPlayerResponse;
				break;
		}
	}

	private void AdvanceDialogue()
	{
		if (dialogueState == DialogueState.ShowingIntro)
		{
			ShowDialogue("Amin: \"aagaya tu chars peenay\"", ChoicePrompt);
			dialogueState = DialogueState.AwaitingChoice;
			return;
		}

		if (dialogueState == DialogueState.ShowingPlayerResponse)
		{
			ShowDialogue(selectedNpcResponse, "x: next");
			dialogueState = DialogueState.ShowingNpcResponse;
			return;
		}

		if (dialogueState == DialogueState.ShowingNpcResponse)
		{
			isReturning = true;
			player.RequestSceneSpawn(AmunsRoomBathroomDoorSpawn);
			GetTree().ChangeSceneToFile("res://AmunsRoom.tscn");
		}
	}

	private void ShowDialogue(string line, string footer)
	{
		dialogueText.Text = line;
		choiceText.Text = footer;
	}
}
