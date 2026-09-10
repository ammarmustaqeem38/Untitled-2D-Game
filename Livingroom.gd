extends Node2D

const ASHFAQ_DIALOGUE = [
	"Vago: \"Dad?\"",
	"Ashfaq: \"...\"",
	"Ashfaq: \"You're a fucking disgrace\"",
	"Ashfaq: \"Get out of my sight\"",
]

const ASHFAQ_REPEAT_DIALOGUE = [
	"Ashfaq: \"I thought I told you to leave me alone\"",
]

const ASHFAQ_INTERACTION_DISTANCE: float = 190.0
const ASHFAQ_STROBE_DURATION: float = 1.2
const ASHFAQ_STROBE_INTERVAL: float = 0.045

static var ashfaq_interaction_count: int = 0

var player
var ashfaq: AnimatedSprite2D
var prompt_label: Label
var was_e_pressed: bool = false
var ashfaq_strobe_remaining: float = 0.0
var ashfaq_strobe_time: float = 0.0


func _ready() -> void:
	player = get_node("/root/Player")
	var audio_player: AudioStreamPlayer2D = get_node("AudioStreamPlayer2D")
	audio_player.play()

	ashfaq = get_node("Ashfaq/AnimatedSprite2D")
	ashfaq.play("overhead_press")

	player.move_to_requested_spawn(self, "Spawn")
	create_prompt_ui()


func _process(delta: float) -> void:
	var is_near_ashfaq: bool = player.global_position.distance_to(ashfaq.global_position) <= ASHFAQ_INTERACTION_DISTANCE
	var e_pressed: bool = Input.is_key_pressed(KEY_E)

	prompt_label.visible = is_near_ashfaq and not player.is_interaction_ui_active()

	if is_near_ashfaq and not player.is_interaction_ui_active() and e_pressed and not was_e_pressed:
		handle_ashfaq_interaction()

	update_ashfaq_strobe(delta)
	was_e_pressed = e_pressed


func create_prompt_ui() -> void:
	prompt_label = Label.new()
	prompt_label.text = "e"
	prompt_label.position = ashfaq.global_position + Vector2(-8.0, -130.0)
	prompt_label.visible = false
	prompt_label.z_index = 100
	prompt_label.add_theme_font_size_override("font_size", 24)
	prompt_label.add_theme_color_override("font_color", Color.WHITE)
	prompt_label.add_theme_color_override("font_outline_color", Color.BLACK)
	prompt_label.add_theme_constant_override("outline_size", 4)
	add_child(prompt_label)


func handle_ashfaq_interaction() -> void:
	prompt_label.visible = false

	if ashfaq_interaction_count == 0:
		ashfaq_interaction_count += 1
		player.start_dialogue(ASHFAQ_DIALOGUE)
		return

	if ashfaq_interaction_count == 1:
		ashfaq_interaction_count += 1
		player.start_dialogue(ASHFAQ_REPEAT_DIALOGUE)
		return

	ashfaq_interaction_count += 1
	start_ashfaq_strobe()


func start_ashfaq_strobe() -> void:
	ashfaq_strobe_remaining = ASHFAQ_STROBE_DURATION
	ashfaq_strobe_time = 0.0


func update_ashfaq_strobe(delta: float) -> void:
	if ashfaq_strobe_remaining <= 0.0:
		return

	ashfaq_strobe_remaining -= delta
	ashfaq_strobe_time += delta

	var is_red_frame: bool = int(ashfaq_strobe_time / ASHFAQ_STROBE_INTERVAL) % 2 == 0
	ashfaq.modulate = Color.RED if is_red_frame else Color.WHITE

	if ashfaq_strobe_remaining <= 0.0:
		ashfaq.modulate = Color.WHITE
