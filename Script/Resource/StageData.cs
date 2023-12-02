using Godot;
using System;

[GlobalClass]
public partial class StageData : Resource
{
	[ExportCategory("Essentials")]
	[Export]
	public string FolderName { get; set; }
    [Export]
    public string StageName { get; set; }
    [Export]
	public string StageScene { get; set; }
    [Export]
    public Texture2D StageThumbnail { get; set; }
    [Export]
    public Texture2D StageBackground { get; set; }
}
