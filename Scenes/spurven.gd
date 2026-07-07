extends KinematicBody2D

export var SPEED := 200.0
export var ANGULAR_S := 3.0  # radians per second

var velocity := Vector2.ZERO

func die():
	queue_free()

func _physics_process(delta):
	# rotation input
	var turn_dir := 0
	
	if Input.is_action_pressed("ui_left"):
		turn_dir -= 1
	if Input.is_action_pressed("ui_right"):
		turn_dir += 1

	rotation += turn_dir * ANGULAR_S * delta
	
	# detect collision
	var collision = move_and_collide(velocity * delta)
	if collision:
		if collision.collider.is_in_group("death_wall"):
			die()
	# constant forward movement
	velocity = Vector2.RIGHT.rotated(rotation) * SPEED
