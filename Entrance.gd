extends Area2D

const AMUNS_ROOM_SCENE_PATH: String = "res://AmunsRoom.tscn"
const MACHI_INT_AMUN_RETURN_SPAWN: String = "EntranceMarkers/AmunReturnSpawn"


func _ready() -> void:
	body_entered.connect(_on_body_entered)


func change_room() -> void:
	get_tree().change_scene_to_file("res://MachiInt.tscn")


func _on_body_entered(body: Node2D) -> void:
	var player = get_node("/root/Player")

	if body == player:
		var scene = get_tree().current_scene

		if scene != null and scene.scene_file_path == AMUNS_ROOM_SCENE_PATH:
			player.request_scene_spawn(MACHI_INT_AMUN_RETURN_SPAWN)

		call_deferred("change_room")
