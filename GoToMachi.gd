extends Area2D

const REQUIRED_OBJECTIVE: String = "go clifton"
const MACHI_INT_SCENE_PATH: String = "res://MachiInt.tscn"
const MACHI_EXT_MACHI_INT_ENTRANCE_SPAWN: String = "MachiIntEntrance"


func _ready() -> void:
	body_entered.connect(_on_body_entered)


func change_room() -> void:
	get_tree().change_scene_to_file("res://MachiExt.tscn")


func _on_body_entered(body: Node2D) -> void:
	var player = get_node("/root/Player")

	if body == player and player.has_objective(REQUIRED_OBJECTIVE):
		var scene = get_tree().current_scene

		if scene != null and scene.scene_file_path == MACHI_INT_SCENE_PATH:
			player.request_scene_spawn(MACHI_EXT_MACHI_INT_ENTRANCE_SPAWN)

		call_deferred("change_room")
