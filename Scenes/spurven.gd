extends KinematicBody2D

export var SPEED := 200.0
export var ANGULAR_S := 3.0
export var left_key := 0
export var right_key := 0
export var player_id := -1

signal died(player_id)

var velocity := Vector2.ZERO
var _dying := false

func _ready():
	$deathdetector.connect("body_entered", self, "_on_deathdetector_body_entered")

func _on_deathdetector_body_entered(body):
	if body.is_in_group("death_wall"):
		die()

func die():
	if _dying:
		return
	_dying = true
	emit_signal("died", player_id)
	queue_free()

func _physics_process(delta):
	var turn_dir := 0

	if left_key != 0 and Input.is_key_pressed(left_key):
		turn_dir -= 1
	if right_key != 0 and Input.is_key_pressed(right_key):
		turn_dir += 1

	rotation += turn_dir * ANGULAR_S * delta

	var collision = move_and_collide(velocity * delta)
	if collision:
		if collision.collider.is_in_group("death_wall"):
			die()
	velocity = Vector2.RIGHT.rotated(rotation) * SPEED
