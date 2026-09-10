extends Area2D


func _ready() -> void:
	body_entered.connect(_on_body_entered)


func change_room() -> void:
	get_tree().change_scene_to_file("res://Livingroom.tscn")


func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		call_deferred("change_room")
