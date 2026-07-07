using Godot;
using System;

public class Drawer : Node2D
{
	private Vector2 position;
	private Line2D trail;

	private void CompressTrail(){
//		Vector2[] allPoints = trail.Points;
//
//		List<int> startIndices = new();
//		List<int> endIndices = new();
//
//		for (int i = 0; i <= allPoints.Length; i++){
//			for (int j = allPoints.Length; j >= i; j--){
//				double delta_x = allPoints[i][0] - allPoints[j][0];
//				double delta_y = allPoints[i][1] - allPoints[j][1];
//				double slope = delta_x / delta_y;
//
//				GD.Print(slope);
//			}
//		}
		return;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		position = GlobalPosition;
		trail = GetNode<Line2D>("Trail");
		
		trail.SetAsToplevel(true);
		trail.AddPoint(position);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{	
		if (trail.GetPointCount() > 500) {
			CompressTrail();
		}
		position = GlobalPosition;
		trail.AddPoint(position);
	}
}
