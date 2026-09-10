extends CharacterBody2D

signal move_step_taken

@export var speed: float = 300.0

const FIRST_PHONE_CALL_DIALOGUE = [
	"Soma: \"Yo vigo you wanna come over?\"",
	"Vago: \"Nah bro i got daawat\"",
	"Soma: \"Dude pleaseee\"",
	"Soma: \"Everyone's here\"",
	"Vago: \"fuck you\"",
	"Vago: \"kill yourself\"",
	"Vago: \"you dont have sisters you dont know what its like etcetc\"",
]

const HILAL_PHONE_CALL_DIALOGUE = [
	"Hilal: yaar aaja na",
	"Vago: Haan jaani bas aaya",
]

const MACHI_INT_SOMA_PHONE_CALL_DIALOGUE = [
	"Soma: \"Come amin's room bitch\"",
]

const STEP_DISTANCE: float = 48.0
const FIRST_CALL_TIMEOUT_SECONDS: float = 6.0
const CALL_RETRY_DELAY_SECONDS: float = 2.0
const AUTO_ACCEPT_DELAY_SECONDS: float = 1.25
const OBJECTIVE_DELAY_SECONDS: float = 0.5
const RINGTONE_MIX_RATE: float = 44100.0
const ZYN_GLOW_DURATION_SECONDS: float = 1.5
const ZYN_GLOW_STROBE_INTERVAL_SECONDS: float = 0.045

var anim: AnimatedSprite2D
var camera: Camera2D
var audio_listener: AudioListener2D
var objective_panel: Panel
var objective_text: Label
var dialogue_panel: Panel
var dialogue_text: Label
var dialogue_footer: Label
var ringtone_player: AudioStreamPlayer
var carried_audio_player: AudioStreamPlayer
var ringtone_playback: AudioStreamGeneratorPlayback

var current_objective: String = ""
var pending_objective: String = ""
var requested_spawn_marker_path: String = ""
var incoming_caller: String = ""
var last_scene_path: String = ""
var active_dialogue: Array = []
var incoming_phone_dialogue: Array = []
var dialogue_completed: Callable = Callable()
var incoming_phone_completed: Callable = Callable()
var dialogue_index: int = -1
var is_dialogue_active: bool = false
var dialogue_locks_movement: bool = false
var is_phone_ringing: bool = false
var phone_auto_accepts: bool = false
var phone_retries_on_decline: bool = false
var first_phone_call_started: bool = false
var machi_int_soma_phone_call_started: bool = false
var external_movement_locked: bool = false
var was_x_pressed: bool = false
var was_f_pressed: bool = false
var was_h_pressed: bool = false
var step_distance_accumulator: float = 0.0
var phone_ring_elapsed: float = 0.0
var phone_retry_countdown: float = 0.0
var pending_objective_delay: float = -1.0
var ringtone_sample_time: float = 0.0
var ringtone_phase: float = 0.0
var zyn_glow_remaining: float = 0.0
var zyn_glow_time: float = 0.0
var zyn_glow_base_modulate: Color = Color.WHITE
var is_zyn_glow_active: bool = false


func _ready() -> void:
	anim = get_node("AnimatedSprite2D")
	camera = get_node("Camera2D")
	audio_listener = get_node("AudioListener2D")
	create_dialogue_ui()
	create_objective_ui()
	create_ringtone_player()
	update_camera_enabled()
	update_audio_listener()
	update_scene_entry_effects()


func _process(delta: float) -> void:
	update_camera_enabled()
	update_audio_listener()
	update_scene_entry_effects()
	update_phone_call(delta)
	update_dialogue_input()
	update_delayed_objective(delta)
	update_zyn_glow(delta)


func _physics_process(_delta: float) -> void:
	if is_movement_locked():
		velocity = Vector2.ZERO
		anim.stop()
		move_and_slide()
		return

	var direction: Vector2 = Vector2.ZERO

	if Input.is_action_pressed("ui_right"):
		direction.x += 1.0
	if Input.is_action_pressed("ui_left"):
		direction.x -= 1.0
	if Input.is_action_pressed("ui_down"):
		direction.y += 1.0
	if Input.is_action_pressed("ui_up"):
		direction.y -= 1.0

	if direction != Vector2.ZERO:
		velocity = direction.normalized() * speed
		update_animation(direction)
	else:
		velocity = Vector2.ZERO
		anim.stop()

	var previous_position: Vector2 = global_position
	move_and_slide()
	update_move_steps(direction, previous_position)


func update_animation(direction: Vector2) -> void:
	anim.flip_h = false

	if direction.x > 0.0:
		anim.play("walk_right")
	elif direction.x < 0.0:
		anim.play("walk_right")
		anim.flip_h = true
	elif direction.y > 0.0:
		anim.play("walk_down")
	elif direction.y < 0.0:
		anim.play("walk_up")


func update_camera_enabled() -> void:
	var scene = get_tree().current_scene
	var scene_path: String = scene.scene_file_path if scene != null else ""
	camera.enabled = scene_path == "res://MachiInt.tscn" or scene_path == "res://AmunsRoom.tscn"


func update_audio_listener() -> void:
	if not audio_listener.is_current():
		audio_listener.make_current()


func update_scene_entry_effects() -> void:
	var scene = get_tree().current_scene
	var scene_path: String = scene.scene_file_path if scene != null else ""

	if scene_path == last_scene_path:
		return

	last_scene_path = scene_path

	if scene_path == "res://MachiExt.tscn" or scene_path == "res://MachiExit.tscn":
		clear_objective()


func is_interaction_ui_active() -> bool:
	return is_dialogue_active or is_phone_ringing


func is_movement_locked() -> bool:
	return external_movement_locked or is_phone_ringing or (is_dialogue_active and dialogue_locks_movement)


func set_cutscene_state(is_active: bool, hide_player: bool = true) -> void:
	external_movement_locked = is_active
	visible = not (is_active and hide_player)

	if is_active:
		velocity = Vector2.ZERO
		anim.stop()


func start_zyn_glow() -> void:
	if not is_zyn_glow_active:
		zyn_glow_base_modulate = anim.modulate

	is_zyn_glow_active = true
	zyn_glow_remaining = ZYN_GLOW_DURATION_SECONDS
	zyn_glow_time = 0.0
	update_zyn_glow_frame(true)


func start_dialogue(lines: Array, on_completed: Callable = Callable(), lock_movement: bool = false) -> void:
	if lines == null or lines.is_empty():
		if on_completed.is_valid():
			on_completed.call()
		return

	active_dialogue = lines
	dialogue_completed = on_completed
	dialogue_locks_movement = lock_movement
	dialogue_index = 0
	is_dialogue_active = true
	dialogue_panel.visible = true
	show_dialogue_line()


func try_start_first_phone_call() -> void:
	if first_phone_call_started:
		return

	first_phone_call_started = true
	start_incoming_phone_call(
		"Soma",
		FIRST_PHONE_CALL_DIALOGUE,
		Callable(self, "set_objective_after_delay").bind("Find joint"),
		false,
		true
	)


func start_hilal_phone_call() -> void:
	start_incoming_phone_call(
		"Hilal",
		HILAL_PHONE_CALL_DIALOGUE,
		Callable(self, "set_objective_after_delay").bind("go clifton"),
		true,
		false
	)


func start_machi_int_soma_phone_call() -> void:
	if machi_int_soma_phone_call_started:
		return

	machi_int_soma_phone_call_started = true
	start_incoming_phone_call(
		"Soma",
		MACHI_INT_SOMA_PHONE_CALL_DIALOGUE,
		Callable(),
		true,
		false
	)


func update_dialogue_input() -> void:
	var x_pressed: bool = Input.is_key_pressed(KEY_X)

	if is_dialogue_active and x_pressed and not was_x_pressed:
		advance_dialogue()

	was_x_pressed = x_pressed


func show_dialogue_line() -> void:
	dialogue_text.text = active_dialogue[dialogue_index]
	dialogue_footer.text = "x: next"
	dialogue_footer.visible = dialogue_index == 0


func advance_dialogue() -> void:
	dialogue_index += 1

	if dialogue_index >= active_dialogue.size():
		var completed: Callable = dialogue_completed
		active_dialogue = []
		dialogue_completed = Callable()
		dialogue_index = -1
		is_dialogue_active = false
		dialogue_locks_movement = false
		dialogue_text.text = ""
		dialogue_footer.visible = false
		dialogue_panel.visible = false

		if completed.is_valid():
			completed.call()
		return

	show_dialogue_line()


func start_incoming_phone_call(
	caller: String,
	dialogue: Array,
	on_completed: Callable = Callable(),
	auto_accept: bool = false,
	retry_on_decline: bool = false
) -> void:
	incoming_caller = caller
	incoming_phone_dialogue = dialogue
	incoming_phone_completed = on_completed
	phone_auto_accepts = auto_accept
	phone_retries_on_decline = retry_on_decline
	phone_ring_elapsed = 0.0
	is_phone_ringing = true
	dialogue_panel.visible = true
	dialogue_text.text = "Incoming call: %s" % incoming_caller
	dialogue_footer.text = "answering..." if phone_auto_accepts else "f: pick up    h: hang up"
	dialogue_footer.visible = true
	start_ringtone()


func update_phone_call(delta: float) -> void:
	if phone_retry_countdown > 0.0:
		phone_retry_countdown -= delta

		if phone_retry_countdown <= 0.0:
			start_incoming_phone_call(
				incoming_caller,
				incoming_phone_dialogue,
				incoming_phone_completed,
				true,
				false
			)

	if not is_phone_ringing:
		return

	phone_ring_elapsed += delta
	update_ringtone()

	var f_pressed: bool = Input.is_key_pressed(KEY_F)
	var h_pressed: bool = Input.is_key_pressed(KEY_H)

	if phone_auto_accepts:
		if phone_ring_elapsed >= AUTO_ACCEPT_DELAY_SECONDS:
			accept_phone_call()
	elif f_pressed and not was_f_pressed:
		accept_phone_call()
	elif (h_pressed and not was_h_pressed) or phone_ring_elapsed >= FIRST_CALL_TIMEOUT_SECONDS:
		decline_first_phone_call()

	was_f_pressed = f_pressed
	was_h_pressed = h_pressed


func accept_phone_call() -> void:
	is_phone_ringing = false
	stop_ringtone()
	start_dialogue(incoming_phone_dialogue, incoming_phone_completed, true)


func decline_first_phone_call() -> void:
	is_phone_ringing = false
	stop_ringtone()
	dialogue_text.text = ""
	dialogue_footer.visible = false
	dialogue_panel.visible = false

	if phone_retries_on_decline:
		phone_retry_countdown = CALL_RETRY_DELAY_SECONDS


func set_objective_after_delay(objective: String) -> void:
	pending_objective = objective
	pending_objective_delay = OBJECTIVE_DELAY_SECONDS


func update_delayed_objective(delta: float) -> void:
	if pending_objective_delay < 0.0:
		return

	pending_objective_delay -= delta

	if pending_objective_delay <= 0.0:
		set_objective(pending_objective)
		pending_objective = ""
		pending_objective_delay = -1.0


func update_zyn_glow(delta: float) -> void:
	if not is_zyn_glow_active:
		return

	zyn_glow_remaining -= delta
	zyn_glow_time += delta

	if zyn_glow_remaining <= 0.0:
		is_zyn_glow_active = false
		anim.modulate = zyn_glow_base_modulate
		return

	var is_green_frame: bool = int(zyn_glow_time / ZYN_GLOW_STROBE_INTERVAL_SECONDS) % 2 == 0
	update_zyn_glow_frame(is_green_frame)


func update_zyn_glow_frame(is_green_frame: bool) -> void:
	anim.modulate = Color(0.12, 1.0, 0.2, 1.0) if is_green_frame else zyn_glow_base_modulate


func create_dialogue_ui() -> void:
	var dialogue_layer: CanvasLayer = CanvasLayer.new()
	dialogue_layer.name = "DialogueLayer"
	dialogue_layer.layer = 60
	add_child(dialogue_layer)

	dialogue_panel = Panel.new()
	dialogue_panel.visible = false
	dialogue_panel.anchor_left = 0.5
	dialogue_panel.anchor_right = 0.5
	dialogue_panel.anchor_top = 1.0
	dialogue_panel.anchor_bottom = 1.0
	dialogue_panel.offset_left = -340.0
	dialogue_panel.offset_right = 340.0
	dialogue_panel.offset_top = -150.0
	dialogue_panel.offset_bottom = -42.0

	var panel_style: StyleBoxFlat = StyleBoxFlat.new()
	panel_style.bg_color = Color.BLACK
	dialogue_panel.add_theme_stylebox_override("panel", panel_style)
	dialogue_layer.add_child(dialogue_panel)

	dialogue_text = Label.new()
	dialogue_text.position = Vector2(24.0, 18.0)
	dialogue_text.size = Vector2(632.0, 58.0)
	dialogue_text.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	dialogue_text.add_theme_font_size_override("font_size", 24)
	dialogue_text.add_theme_color_override("font_color", Color.WHITE)
	dialogue_panel.add_child(dialogue_text)

	dialogue_footer = Label.new()
	dialogue_footer.text = "x: next"
	dialogue_footer.position = Vector2(24.0, 78.0)
	dialogue_footer.size = Vector2(632.0, 24.0)
	dialogue_footer.add_theme_font_size_override("font_size", 18)
	dialogue_footer.add_theme_color_override("font_color", Color.WHITE)
	dialogue_panel.add_child(dialogue_footer)


func create_ringtone_player() -> void:
	ringtone_player = AudioStreamPlayer.new()
	ringtone_player.name = "RingtonePlayer"

	var generator: AudioStreamGenerator = AudioStreamGenerator.new()
	generator.mix_rate = RINGTONE_MIX_RATE
	generator.buffer_length = 0.12
	ringtone_player.stream = generator
	ringtone_player.volume_db = -10.0
	add_child(ringtone_player)


func ensure_carried_audio_player() -> void:
	if carried_audio_player != null:
		return

	carried_audio_player = AudioStreamPlayer.new()
	carried_audio_player.name = "CarriedAudioPlayer"
	add_child(carried_audio_player)


func carry_scene_audio(source_player: AudioStreamPlayer2D) -> void:
	if source_player == null or source_player.stream == null:
		return

	ensure_carried_audio_player()
	carried_audio_player.stream = source_player.stream
	carried_audio_player.bus = source_player.bus
	carried_audio_player.volume_db = source_player.volume_db
	carried_audio_player.pitch_scale = source_player.pitch_scale
	carried_audio_player.play(source_player.get_playback_position())


func play_or_resume_scene_audio(scene_player: AudioStreamPlayer2D) -> void:
	if scene_player == null or scene_player.stream == null:
		return

	if carried_audio_player != null and carried_audio_player.playing and carried_audio_player.stream == scene_player.stream:
		scene_player.play(carried_audio_player.get_playback_position())
		carried_audio_player.stop()
		carried_audio_player.stream = null
		return

	scene_player.play()


func start_ringtone() -> void:
	ringtone_sample_time = 0.0
	ringtone_phase = 0.0
	ringtone_player.play()
	ringtone_playback = ringtone_player.get_stream_playback() as AudioStreamGeneratorPlayback
	update_ringtone()


func stop_ringtone() -> void:
	ringtone_player.stop()
	ringtone_playback = null


func update_ringtone() -> void:
	if ringtone_playback == null:
		return

	var frames_available: int = ringtone_playback.get_frames_available()

	for _i in range(frames_available):
		var beat_time: float = fmod(ringtone_sample_time, 1.15)
		var is_tone_on: bool = beat_time < 0.32 or (beat_time >= 0.45 and beat_time < 0.77)
		var sample: float = 0.0

		if is_tone_on:
			var frequency: float = 880.0 if beat_time < 0.32 else 660.0
			ringtone_phase += TAU * frequency / RINGTONE_MIX_RATE
			sample = sin(ringtone_phase) * 0.2

		ringtone_playback.push_frame(Vector2(sample, sample))
		ringtone_sample_time += 1.0 / RINGTONE_MIX_RATE


func update_move_steps(direction: Vector2, previous_position: Vector2) -> void:
	if direction == Vector2.ZERO:
		return

	step_distance_accumulator += global_position.distance_to(previous_position)

	while step_distance_accumulator >= STEP_DISTANCE:
		step_distance_accumulator -= STEP_DISTANCE
		move_step_taken.emit()


func set_objective(objective: String) -> void:
	current_objective = objective
	objective_text.text = current_objective
	objective_panel.visible = current_objective.strip_edges() != ""


func has_objective(objective: String) -> bool:
	return current_objective == objective


func request_scene_spawn(marker_path: String) -> void:
	requested_spawn_marker_path = marker_path


func move_to_requested_spawn(scene_root: Node, default_marker_path: String = "") -> void:
	var marker_path: String = default_marker_path if requested_spawn_marker_path.strip_edges() == "" else requested_spawn_marker_path
	requested_spawn_marker_path = ""

	if marker_path.strip_edges() == "":
		return

	var marker: Marker2D = scene_root.get_node_or_null(marker_path) as Marker2D

	if marker == null and default_marker_path.strip_edges() != "":
		marker = scene_root.get_node_or_null(default_marker_path) as Marker2D

	if marker != null:
		global_position = marker.global_position


func clear_objective() -> void:
	pending_objective = ""
	pending_objective_delay = -1.0
	set_objective("")


func create_objective_ui() -> void:
	var objective_layer: CanvasLayer = CanvasLayer.new()
	objective_layer.name = "ObjectiveLayer"
	objective_layer.layer = 50
	add_child(objective_layer)

	objective_panel = Panel.new()
	objective_panel.visible = false
	objective_panel.anchor_left = 1.0
	objective_panel.anchor_right = 1.0
	objective_panel.anchor_top = 0.0
	objective_panel.anchor_bottom = 0.0
	objective_panel.offset_left = -390.0
	objective_panel.offset_right = -24.0
	objective_panel.offset_top = 24.0
	objective_panel.offset_bottom = 92.0

	var panel_style: StyleBoxFlat = StyleBoxFlat.new()
	panel_style.bg_color = Color(0.0, 0.0, 0.0, 0.68)
	objective_panel.add_theme_stylebox_override("panel", panel_style)
	objective_layer.add_child(objective_panel)

	var exclamation: Label = Label.new()
	exclamation.text = "!"
	exclamation.position = Vector2(18.0, 4.0)
	exclamation.size = Vector2(42.0, 58.0)
	exclamation.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	exclamation.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	exclamation.add_theme_font_size_override("font_size", 44)
	exclamation.add_theme_color_override("font_color", Color(1.0, 0.93, 0.18, 1.0))
	exclamation.add_theme_color_override("font_outline_color", Color.BLACK)
	exclamation.add_theme_constant_override("outline_size", 7)
	objective_panel.add_child(exclamation)

	objective_text = Label.new()
	objective_text.position = Vector2(66.0, 19.0)
	objective_text.size = Vector2(276.0, 36.0)
	objective_text.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	objective_text.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	objective_text.add_theme_font_size_override("font_size", 20)
	objective_text.add_theme_color_override("font_color", Color.WHITE)
	objective_panel.add_child(objective_text)
