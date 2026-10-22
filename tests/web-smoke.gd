extends SceneTree

var failures: int = 0
var player


func _initialize() -> void:
	_run.call_deferred()


func check(condition: bool, message: String) -> void:
	if not condition:
		failures += 1
		push_error(message)


func visit(scene: String) -> void:
	check(change_scene_to_file("res://" + scene + ".tscn") == OK, "Cannot load " + scene)
	await process_frame
	await process_frame
	check(current_scene != null, "Scene was not instantiated: " + scene)


func finish_dialogue() -> void:
	for _line in range(30):
		if not player.is_dialogue_active:
			return
		player.advance_dialogue()
	check(false, "Dialogue did not finish")


func _run() -> void:
	await process_frame
	player = root.get_node_or_null("Player")
	check(player != null, "Player autoload failed on a clean import")
	if player == null:
		quit(1)
		return

	await visit("TitleScreen")
	check(not player.visible, "Title screen should hide the player")
	check(current_scene.get_node("TitleMusic").playing, "Title music did not start")

	await visit("main")
	check(player.visible, "Leaving the title screen should restore the player")
	var start: Vector2 = player.global_position
	Input.action_press("ui_right")
	for _frame in range(5):
		await physics_frame
	Input.action_release("ui_right")
	check(player.global_position.x > start.x, "Player movement failed")

	player.try_start_first_phone_call()
	check(player.is_phone_ringing, "First call did not ring")
	check(player.ringtone_playback != null, "Procedural ringtone playback failed")
	player.decline_first_phone_call()
	check(player.phone_retry_countdown > 0.0, "Declined call did not schedule a retry")
	player.update_phone_call(2.01)
	# A large time step can also auto-answer the retried call.
	if player.is_phone_ringing:
		player.accept_phone_call()
	check(player.is_dialogue_active, "Retried call did not start dialogue")
	finish_dialogue()
	player.update_delayed_objective(1.0)
	check(player.current_objective == "Find joint", "First call objective was lost")
	player.start_hilal_phone_call()
	player.accept_phone_call()
	finish_dialogue()
	player.update_delayed_objective(1.0)
	check(player.current_objective == "go clifton", "Hilal call objective was lost")

	await visit("BalconyIdle")
	check(current_scene.get_node("AudioStreamPlayer").playing, "Balcony audio did not start")
	await visit("Livingroom")
	check(current_scene.get_node("AudioStreamPlayer2D").playing, "Living room audio did not start")
	current_scene.handle_ashfaq_interaction()
	check(player.is_dialogue_active, "Living room interaction failed")
	finish_dialogue()
	await visit("MachiExt")
	check(player.current_objective.is_empty(), "Machi exterior should clear the objective")
	await visit("MachiInt")
	check(player.camera.enabled, "Machi interior camera was not enabled")
	if player.is_phone_ringing:
		player.accept_phone_call()
	finish_dialogue()
	await visit("AmunsRoom")
	check(player.camera.enabled, "Amun's room camera was not enabled")
	var music = current_scene.get_node("AudioStreamPlayer2D")
	check(music.playing, "Amun's room music did not start")
	player.start_zyn_glow()
	check(player.is_zyn_glow_active, "Zyn glow did not start")
	player.carry_scene_audio(music)
	check(player.carried_audio_player.playing, "Music was not carried across scenes")

	for key in [KEY_X, KEY_C, KEY_V]:
		await visit("MangiBaithak")
		check(not player.visible, "Dialogue scene should hide the player")
		current_scene.advance_dialogue()
		current_scene.select_choice(key)
		check(not current_scene.selected_npc_response.is_empty(), "Dialogue choice failed")
		current_scene.advance_dialogue()
		current_scene.advance_dialogue()
		await process_frame
		await process_frame
		check(current_scene.scene_file_path == "res://AmunsRoom.tscn", "Dialogue choice failed to return to Amun's room")
		check(player.visible, "Returning from dialogue should restore the player")
		check(current_scene.get_node("AudioStreamPlayer2D").playing, "Room music did not resume")

	print("Game smoke checks: ", "PASS" if failures == 0 else "FAIL (%d)" % failures)
	# Let the audio mixer finish pending playback before ending a short headless run.
	for node in root.find_children("*", "", true, false):
		if node is AudioStreamPlayer or node is AudioStreamPlayer2D:
			node.stop()
			node.stream = null
	await create_timer(0.2).timeout
	quit.call_deferred(0 if failures == 0 else 1)
