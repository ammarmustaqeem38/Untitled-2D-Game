extends Node2D

const ZYN_INTERACTION_DISTANCE: float = 90.0

static var zyn_collected: bool = false

var player
var zyn: Sprite2D
var zyn_prompt_label: Label
var audio_player: AudioStreamPlayer2D
var was_e_pressed: bool = false


func _ready() -> void:
	player = get_node("/root/Player")
	zyn = get_node_or_null("Props/Zyn")
	audio_player = get_node("AudioStreamPlayer2D")

	var mangi_baithak_trigger: Area2D = get_node("TriggerMangiBaithak")
	mangi_baithak_trigger.body_entered.connect(_on_mangi_baithak_trigger_body_entered)

	player.play_or_resume_scene_audio(audio_player)
	player.move_to_requested_spawn(self, "Spawn")

	if zyn != null:
		zyn.visible = not zyn_collected
		create_zyn_prompt_ui()


func _process(_delta: float) -> void:
	update_zyn_interaction()


func _on_mangi_baithak_trigger_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		call_deferred("go_to_mangi_baithak")


func go_to_mangi_baithak() -> void:
	player.carry_scene_audio(audio_player)
	get_tree().change_scene_to_file("res://MangiBaithak.tscn")


func create_zyn_prompt_ui() -> void:
	zyn_prompt_label = Label.new()
	zyn_prompt_label.text = "e"
	zyn_prompt_label.position = zyn.global_position + Vector2(-8.0, -38.0)
	zyn_prompt_label.visible = false
	zyn_prompt_label.z_index = 100
	zyn_prompt_label.add_theme_font_size_override("font_size", 24)
	zyn_prompt_label.add_theme_color_override("font_color", Color.WHITE)
	zyn_prompt_label.add_theme_color_override("font_outline_color", Color.BLACK)
	zyn_prompt_label.add_theme_constant_override("outline_size", 4)
	add_child(zyn_prompt_label)


func update_zyn_interaction() -> void:
	if zyn == null or zyn_prompt_label == null or zyn_collected:
		if zyn_prompt_label != null:
			zyn_prompt_label.visible = false
		return

	var is_near_zyn: bool = player.global_position.distance_to(zyn.global_position) <= ZYN_INTERACTION_DISTANCE
	var e_pressed: bool = Input.is_key_pressed(KEY_E)

	zyn_prompt_label.visible = is_near_zyn and not player.is_interaction_ui_active()

	if is_near_zyn and not player.is_interaction_ui_active() and e_pressed and not was_e_pressed:
		zyn_collected = true
		zyn.visible = false
		zyn_prompt_label.visible = false
		player.start_zyn_glow()

	was_e_pressed = e_pressed
