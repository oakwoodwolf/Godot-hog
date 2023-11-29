using Godot;
using System;

[GlobalClass]
public partial class StageData : Resource
{
	[ExportCategory("Essentials")]
	[Export]
	public string StageName { get; set; }
	[Export]
	public string StageScene { get; set; }
}
