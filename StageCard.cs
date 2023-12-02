using Godot;
using SonicGodot;
using System;
using System.Xml.Linq;

public partial class StageCard : Button
{
	[Export]
	public Label StageName { get; set; }
	[Export]
	public TextureRect StageThumbnail { get; set; }
	[Export]
	public StageData StageData { get; set; }
	public override void _Ready()
	{
	}
	public void SetUp()
	{
		StageName.Text = StageData.StageName;
		if (StageData.StageThumbnail != null)
		{
			StageThumbnail.Texture = StageData.StageThumbnail;
		}
	}
	private void OnPressed()
	{
		Root.StartLocalServer();
		Root.GetHostServer().RpcAll(Root.Singleton(), "Rpc_SetScene", "res://Stages/" + StageData.FolderName + "/" + StageData.StageScene, StageData);
	}

}



