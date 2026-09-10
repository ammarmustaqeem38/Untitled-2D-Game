extends Node2D

const JOINT_GRAIL_DIALOGUE = [
	"Vago:\"goddammit, out again\"",
]

const JOINT_GRAIL_INTERACTION_DISTANCE: float = 115.0

var player
var joint_grail: Node2D
var joint_prompt_label: Label
var move_step_count: int = 0
var phone_call_triggered: bool = false
var joint_grail_interacted: bool = false
var was_e_pressed: bool = false


func _ready() -> void:
	player = get_node("/root/Player")
	player.move_to_requested_spawn(self)
	joint_grail = get_node("JointGrail")
	player.move_step_taken.connect(_on_player_move_step_taken)
	create_joint_prompt_ui()


func _process(_delta: float) -> void:
	update_joint_grail_interaction()


func _exit_tree() -> void:
	if player != null and player.move_step_taken.is_connected(_on_player_move_step_taken):
		player.move_step_taken.disconnect(_on_player_move_step_taken)


func _on_player_move_step_taken() -> void:
	if phone_call_triggered:
		return

	move_step_count += 1

	if move_step_count < 3:
		return

	phone_call_triggered = true
	player.try_start_first_phone_call()


func create_joint_prompt_ui() -> void:
	joint_prompt_label = Label.new()
	joint_prompt_label.text = "e"
	joint_prompt_label.position = joint_grail.position + Vector2(-8.0, -52.0)
	joint_prompt_label.visible = false
	joint_prompt_label.z_index = 100
	joint_prompt_label.add_theme_font_size_override("font_size", 24)
	joint_prompt_label.add_theme_color_override("font_color", Color.WHITE)
	joint_prompt_label.add_theme_color_override("font_outline_color", Color.BLACK)
	joint_prompt_label.add_theme_constant_override("outline_size", 4)
	add_child(joint_prompt_label)


func update_joint_grail_interaction() -> void:
	var is_near_joint: bool = player.global_position.distance_to(joint_grail.global_position) <= JOINT_GRAIL_INTERACTION_DISTANCE
	var e_pressed: bool = Input.is_key_pressed(KEY_E)

	joint_prompt_label.visible = is_near_joint and not joint_grail_interacted and not player.is_interaction_ui_active()

	if is_near_joint and not joint_grail_interacted and not player.is_interaction_ui_active() and e_pressed and not was_e_pressed:
		joint_grail_interacted = true
		joint_prompt_label.visible = false
		player.clear_objective()
		player.start_dialogue(JOINT_GRAIL_DIALOGUE, Callable(player, "start_hilal_phone_call"))

	was_e_pressed = e_pressed
