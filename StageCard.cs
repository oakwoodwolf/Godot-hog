using Godot;
using SonicGodot;
using System;
using System.Xml.Linq;

public partial class StageCard : Button
{
	[Export]
	public StageData StageData { get; set; }

	private void OnPressed()
	{
		Root.StartLocalServer();
		Root.GetHostServer().RpcAll(Root.Singleton(), "Rpc_SetScene", "res://Stages/" + StageData.StageName + "/" + StageData.StageScene);
	}

}



