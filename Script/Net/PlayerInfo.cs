using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerInfo : Node
{
    public string PlayerName;
    public int Id = 0;
    public override void _Ready() => Name = Id.ToString();
}
