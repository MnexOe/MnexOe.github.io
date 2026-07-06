using Godot;
using System.Collections.Generic;
using AchtungDieSpurve.Interfaces;

public class GameData : Node
{
    public static GameData Instance { get; private set; }

    public List<IPlayerConfig> Players { get; set; } = new List<IPlayerConfig>();
    public int TargetScore { get; set; } = 10;

    public override void _Ready()
    {
        Instance = this;
    }
}
