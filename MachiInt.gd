extends Node2D

const TV_STROBE_COLORS = [
	Color(1.0, 0.04, 0.03, 0.24),
	Color(0.04, 0.2, 1.0, 0.24),
	Color(0.05, 1.0, 0.1, 0.24),
	Color(1.0, 0.92, 0.12, 0.24),
	Color(1.0, 1.0, 1.0, 0.24),
]

const TV_STROBE_INTERVAL_SECONDS: float = 0.12

var tv_light_strobe: Polygon2D
var tv_strobe_elapsed: float = 0.0
var tv_strobe_color_index: int = 0


func _ready() -> void:
	var player = get_node("/root/Player")
	var audio_player: AudioStreamPlayer = get_node("AudioStreamPlayer")
	tv_light_strobe = get_node("Lighting/TVLightStrobe")
	audio_player.play()
	player.move_to_requested_spawn(self, "Spawn")
	player.start_machi_int_soma_phone_call()
	update_tv_light_strobe()


func _process(delta: float) -> void:
	tv_strobe_elapsed += delta

	while tv_strobe_elapsed >= TV_STROBE_INTERVAL_SECONDS:
		tv_strobe_elapsed -= TV_STROBE_INTERVAL_SECONDS
		tv_strobe_color_index = (tv_strobe_color_index + 1) % TV_STROBE_COLORS.size()
		update_tv_light_strobe()


func update_tv_light_strobe() -> void:
	tv_light_strobe.color = TV_STROBE_COLORS[tv_strobe_color_index]
