extends Area2D

const VAGO_ROOM_LIVING_ROOM_ENTRANCE_SPAWN: String = "LivingRoomEntrance"


func _ready() -> void:
	body_entered.connect(_on_body_entered)


func change_room() -> void:
	get_tree().change_scene_to_file("res://main.tscn")


func _on_body_entered(body: Node2D) -> void:
	var player = get_node("/root/Player")

	if body == player:
		player.request_scene_spawn(VAGO_ROOM_LIVING_ROOM_ENTRANCE_SPAWN)
		call_deferred("change_room")
