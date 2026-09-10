extends Control

enum DialogueState {
	SHOWING_INTRO,
	AWAITING_CHOICE,
	SHOWING_PLAYER_RESPONSE,
	SHOWING_NPC_RESPONSE,
}

const CHOICE_PROMPT: String = "(x) normal response    (c) cunt response    (v) nothingburger"
const AMUNS_ROOM_BATHROOM_DOOR_SPAWN: String = "MangiBaithakReturnSpawn"

var player
var dialogue_text: Label
var choice_text: Label
var dialogue_state: int = DialogueState.SHOWING_INTRO
var selected_npc_response: String = ""
var is_returning: bool = false


func _ready() -> void:
	player = get_node("/root/Player")
	dialogue_text = get_node("DialoguePanel/DialogueText")
	choice_text = get_node("DialoguePanel/ChoiceText")
	player.set_cutscene_state(true)
	show_dialogue("Amin: \"Haan bc\"", "x: next")
	dialogue_state = DialogueState.SHOWING_INTRO


func _exit_tree() -> void:
	if player != null:
		player.set_cutscene_state(false)


func _unhandled_input(event: InputEvent) -> void:
	if is_returning or not (event is InputEventKey):
		return

	var key_event: InputEventKey = event as InputEventKey

	if not key_event.pressed or key_event.echo:
		return

	if dialogue_state == DialogueState.SHOWING_INTRO:
		if key_event.keycode == KEY_X:
			advance_dialogue()
	elif dialogue_state == DialogueState.AWAITING_CHOICE:
		select_choice(key_event.keycode)
	elif key_event.keycode == KEY_X:
		advance_dialogue()


func select_choice(keycode: int) -> void:
	match keycode:
		KEY_X:
			show_dialogue("Vago: \"bas yaar\"", "x: next")
			selected_npc_response = "Amin: \"chal ez\""
			dialogue_state = DialogueState.SHOWING_PLAYER_RESPONSE
		KEY_C:
			show_dialogue("Vago: \"i fucking hate you guys\"", "x: next")
			selected_npc_response = "Amin: \"gaandu\""
			dialogue_state = DialogueState.SHOWING_PLAYER_RESPONSE
		KEY_V:
			show_dialogue("Vago: \"...\"", "x: next")
			selected_npc_response = "Amin: \"chal tera baap aagaya\""
			dialogue_state = DialogueState.SHOWING_PLAYER_RESPONSE


func advance_dialogue() -> void:
	if dialogue_state == DialogueState.SHOWING_INTRO:
		show_dialogue("Amin: \"aagaya tu chars peenay\"", CHOICE_PROMPT)
		dialogue_state = DialogueState.AWAITING_CHOICE
		return

	if dialogue_state == DialogueState.SHOWING_PLAYER_RESPONSE:
		show_dialogue(selected_npc_response, "x: next")
		dialogue_state = DialogueState.SHOWING_NPC_RESPONSE
		return

	if dialogue_state == DialogueState.SHOWING_NPC_RESPONSE:
		is_returning = true
		player.request_scene_spawn(AMUNS_ROOM_BATHROOM_DOOR_SPAWN)
		get_tree().change_scene_to_file("res://AmunsRoom.tscn")


func show_dialogue(line: String, footer: String) -> void:
	dialogue_text.text = line
	choice_text.text = footer
