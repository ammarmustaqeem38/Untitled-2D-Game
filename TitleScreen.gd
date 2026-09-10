extends Control

const FADE_DURATION_SECONDS: float = 0.45

var player
var enter_prompt: CanvasItem
var fade_overlay: ColorRect
var title_music: AudioStreamPlayer
var enter_sound: AudioStreamPlayer
var prompt_time: float = 0.0
var fade_time: float = 0.0
var is_starting: bool = false


func _ready() -> void:
	player = get_node("/root/Player")
	player.set_cutscene_state(true)
	enter_prompt = get_node("TitleStack/EnterPrompt")
	fade_overlay = get_node("FadeOverlay")
	title_music = get_node("TitleMusic")
	enter_sound = get_node("EnterSound")
	fade_overlay.color = Color(0.0, 0.0, 0.0, 0.0)
	title_music.finished.connect(_on_title_music_finished)
	title_music.play()


func _exit_tree() -> void:
	if title_music != null and title_music.finished.is_connected(_on_title_music_finished):
		title_music.finished.disconnect(_on_title_music_finished)

	if player != null:
		player.set_cutscene_state(false)


func _process(delta: float) -> void:
	prompt_time += delta
	var alpha: float = float(0.18 + 0.82 * abs(sin(prompt_time * 5.5)))
	enter_prompt.modulate = Color(1.0, 1.0, 1.0, alpha)

	if is_starting:
		update_fade(delta)
		return

	if Input.is_key_pressed(KEY_ENTER) or Input.is_key_pressed(KEY_KP_ENTER):
		start_game()


func update_fade(delta: float) -> void:
	fade_time += delta
	var alpha: float = float(clamp(fade_time / FADE_DURATION_SECONDS, 0.0, 1.0))
	fade_overlay.color = Color(0.0, 0.0, 0.0, alpha)


func _on_title_music_finished() -> void:
	if not is_starting:
		title_music.play()


func start_game() -> void:
	is_starting = true
	fade_time = 0.0
	fade_overlay.color = Color(0.0, 0.0, 0.0, 0.0)
	title_music.stop()
	enter_sound.play()
	await enter_sound.finished
	get_tree().change_scene_to_file("res://main.tscn")
