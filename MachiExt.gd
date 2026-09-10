extends Node2D


func _ready() -> void:
	var player = get_node("/root/Player")
	player.move_to_requested_spawn(self, "Spawn")

	var audio_player: AudioStreamPlayer = get_node("AudioStreamPlayer")
	audio_player.play()
