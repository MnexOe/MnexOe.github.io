extends Node2D

export var thiccness := 10
export var length := 1024
export var height := 600

const WALL_COLOR = Color(0.329412, 0.0705882, 0.0705882, 1)

func _ready():
	_apply_width()

func _draw():
	draw_rect(Rect2(0, 0, length, thiccness), WALL_COLOR)
	draw_rect(Rect2(0, height - thiccness, length, thiccness), WALL_COLOR)
	draw_rect(Rect2(0, 0, thiccness, height), WALL_COLOR)
	draw_rect(Rect2(length - thiccness, 0, thiccness, height), WALL_COLOR)

func _apply_width():
	for child in get_children():
		var rect_size := Vector2.ZERO
		var rect_pos := Vector2.ZERO
		if child.name == "top":
			rect_size = Vector2(length, thiccness)
			rect_pos = Vector2(0, 0)
		elif child.name == "bot":
			rect_size = Vector2(length, thiccness)
			rect_pos = Vector2(0, height - thiccness)
		elif child.name == "left":
			rect_size = Vector2(thiccness, height)
			rect_pos = Vector2(0, 0)
		else: # right
			rect_size = Vector2(thiccness, height)
			rect_pos = Vector2(length - thiccness, 0)
		for grandchild in child.get_children():
			if grandchild is ColorRect:
				grandchild.visible = false
			elif grandchild is CollisionShape2D and grandchild.shape is RectangleShape2D:
				grandchild.shape.extents = rect_size / 2.0
				grandchild.position = rect_pos + rect_size / 2.0
