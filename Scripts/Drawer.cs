using Godot;
using System;
using System.Collections.Generic;

public class Drawer : Node2D
{
	private Vector2 position;
	private Line2D trail;
	private StaticBody2D body;
	private CollisionPolygon2D trailCollision;

	// How "straight" a run of points needs to be to get compressed.
	// Smaller = stricter (keeps more points), larger = more aggressive compression.
	private const float AngleTolerance = 0.003f;

	// How many *uncompressed* points must accumulate before we compress again.
	private const int CompressionThreshold = 500;

	// Index into trail.Points: everything before this index is already
	// finalized/compressed and won't be touched again.
	private int compressedIndex = 0;

	private void CompressTrail()
	{
		Vector2[] allPoints = trail.Points;

		int uncompressedCount = allPoints.Length - compressedIndex;
		if (uncompressedCount < 3)
			return;

		//GD.Print($"Compressing tail: {uncompressedCount} raw points (from index {compressedIndex})");

		// Simplify only the tail, starting from the last already-compressed
		// point so the new segment connects smoothly to the old one.
		List<Vector2> newSegment = new List<Vector2>();
		newSegment.Add(allPoints[compressedIndex]);

		int i = compressedIndex;
		while (i < allPoints.Length - 1)
		{
			Vector2 baseDir = (allPoints[i + 1] - allPoints[i]).Normalized();
			int j = i + 2;
			bool foundBend = false;

			for (; j < allPoints.Length; j++)
			{
				Vector2 dir = (allPoints[j] - allPoints[i]).Normalized();
				float cross = baseDir.x * dir.y - baseDir.y * dir.x;

				if (Mathf.Abs(cross) > AngleTolerance)
				{
					newSegment.Add(allPoints[j]);
					i = j;
					foundBend = true;
					break;
				}
			}

			if (!foundBend)
			{
				Vector2 last = allPoints[allPoints.Length - 1];
				if (newSegment[newSegment.Count - 1] != last)
					newSegment.Add(last);
				break;
			}
		}

		// Stitch: untouched prefix (already compressed) + newly compressed tail.
		List<Vector2> fullPoints = new List<Vector2>(compressedIndex + newSegment.Count);
		for (int k = 0; k < compressedIndex; k++)
			fullPoints.Add(allPoints[k]);
		fullPoints.AddRange(newSegment);

		trail.Points = fullPoints.ToArray();

		// Everything except the very last point is now finalized. The last
		// point stays "live" as the anchor for the next uncompressed segment,
		// since more points will keep getting appended after it.
		compressedIndex = fullPoints.Count - 1;

		//GD.Print($"Trail now has {trail.Points.Length} points, compressedIndex={compressedIndex}");
	}
	
	private void MakeCollsionPolygon(Vector2[] points, CollisionPolygon2D collisionShape)
	{
		if (points.Length < 2)
			return;

		float half = trail.Width / 2f;
		var left = new List<Vector2>();
		var right = new List<Vector2>();

		for (int k = 0; k < points.Length; k++)
		{
			// Direction at this point: average of incoming/outgoing segment
			// directions so corners get a sensible miter instead of a hard notch.
			Vector2 dir;
			if (k == 0)
			{
				dir = (points[k + 1] - points[k]).Normalized();
			}
			else if (k == points.Length - 1)
			{
				dir = (points[k] - points[k - 1]).Normalized();
			}
			else
			{
				Vector2 dirIn = (points[k] - points[k - 1]).Normalized();
				Vector2 dirOut = (points[k + 1] - points[k]).Normalized();
				dir = (dirIn + dirOut).Normalized();
				if (dir == Vector2.Zero) dir = dirIn; // 180° reversal edge case
			}

			Vector2 normal = new Vector2(-dir.y, dir.x);
			left.Add(points[k] + normal * half);
			right.Add(points[k] - normal * half);
		}

		// Build closed outline: left side forward, right side backward.
		var outline = new List<Vector2>(left.Count + right.Count);
		outline.AddRange(left);
		right.Reverse();
		outline.AddRange(right);

		collisionShape.Polygon = outline.ToArray();
		collisionShape.BuildMode = CollisionPolygon2D.BuildModeEnum.Solids;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		position = GlobalPosition;
		trail = GetNode<Line2D>("Trail");
		body = GetNode<StaticBody2D>("TrailBody");
		trailCollision = body.GetNode<CollisionPolygon2D>("TrailCollision");
		

		trail.SetAsToplevel(true);
		trail.AddPoint(position);
	}

	public override void _Process(float delta)
	{
		if (trail.GetPointCount() - compressedIndex > CompressionThreshold)
		{
			CompressTrail();
		}

		position = GlobalPosition;
		trail.AddPoint(position);
		
		MakeCollsionPolygon(trail.Points, trailCollision);
	}
}
