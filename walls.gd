extends Node2D

export var thiccness := 10
export var length := 1024
export var height := 600

func _ready():
	_apply_width()

func _apply_width():
	for child in get_children():
		var rect_size := Vector2.ZERO; var rect_pos := Vector2.ZERO
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
				grandchild.rect_size = rect_size
				grandchild.rect_position = rect_pos
			elif grandchild is CollisionShape2D and grandchild.shape is RectangleShape2D:
				grandchild.shape.extents = rect_size / 2.0
				grandchild.position = rect_pos + rect_size / 2.0
#				for sibling in child.get_children():
#					if sibling is CollisionShape2D and sibling.shape is RectangleShape2D:
#						sibling.shape.extents = grandchild.rect_size / 2.0
