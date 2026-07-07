extends Node2D

export var thiccness := 10
export var length := 1024
export var height := 600

func _ready():
	_apply_width()

func _apply_width():
	for child in get_children():
		for grandchild in child.get_children():
			if grandchild is ColorRect:
				var rect_size := Vector2(length, thiccness) if child.name == "top" or child.name == "bot" else Vector2(thiccness, height)
				grandchild.rect_size = rect_size
				for sibling in child.get_children():
					if sibling is CollisionShape2D and sibling.shape is RectangleShape2D:
						sibling.shape.extents = grandchild.rect_size / 2.0
