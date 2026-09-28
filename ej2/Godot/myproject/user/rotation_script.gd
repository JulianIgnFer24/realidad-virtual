extends Node3D

@export var rotation_speed: float = 2.0
var horizontal_rotation: float = 0.0
var vertical_rotation: float = 0.0
var animating: bool = true

func _process(delta: float) -> void:
	if not animating:
		return
	
	var input_horizontal = 0.0
	var input_vertical = 0.0
	
	if Input.is_key_pressed(KEY_LEFT):
		input_horizontal -= 1.0
	if Input.is_key_pressed(KEY_RIGHT):
		input_horizontal += 1.0
	if Input.is_key_pressed(KEY_UP):
		input_vertical -= 1.0
	if Input.is_key_pressed(KEY_DOWN):
		input_vertical += 1.0
	
	horizontal_rotation += input_horizontal * rotation_speed * delta
	vertical_rotation += input_vertical * rotation_speed * delta
	
	vertical_rotation = clamp(vertical_rotation, -PI/2 + 0.1, PI/2 - 0.1)
	
	rotation.y = horizontal_rotation
	rotation.x = vertical_rotation

func _input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed:
		match event.keycode:
			KEY_D:
				animating = false
			KEY_R:
				animating = true
				rotation = Vector3(0, 0, 0)
				horizontal_rotation = 0.0
				vertical_rotation = 0.0