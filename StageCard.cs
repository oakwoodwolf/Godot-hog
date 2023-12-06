using Godot;
using SonicGodot;


public enum ButtonState
{
    Idle,
    Hovered,
    Active
}
public partial class StageCard : Button
{
	[Export]
	public Label StageName { get; set; }
    private ButtonState _state;
	[Export]
	public TextureRect StageThumbnail { get; set; }
	[Export]
	public StageData StageData { get; set; }
	public override void _Ready()
	{
	}
    public override void _Process(double delta)
    {
        if (HasFocus())
        {
            if (_state == ButtonState.Idle)
            {
                Tween tween = CreateTween();
                tween.Parallel().TweenProperty(StageName, "modulate", Colors.White, 0.15f);
                tween.Parallel().TweenProperty(this, "scale", new Vector2(1.15f, 1.15f), 0.15f).SetEase(Tween.EaseType.In);
                tween.Parallel().TweenProperty(GetNode("CardContainer"), "scale", new Vector2(1.05f, 1.05f), 0.15f).SetEase(Tween.EaseType.In);
                tween.TweenProperty(GetNode("CardContainer"), "scale", Vector2.One, 0.15f).SetEase(Tween.EaseType.Out);

            }
            _state = ButtonState.Hovered;
        } else
        {
            if (_state == ButtonState.Hovered)
            {
                Tween tween = CreateTween();
                tween.Parallel().TweenProperty(StageName, "modulate", Color.Color8(161, 139, 161), 0.15f);
                tween.Parallel().TweenProperty(GetNode("CardContainer"), "scale", Vector2.One, 0.15f).SetEase(Tween.EaseType.Out);
                tween.Parallel().TweenProperty(this, "scale", Vector2.One, 0.15f).SetEase(Tween.EaseType.Out);

            }
            _state = ButtonState.Idle;
        }
        base._Process(delta);
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



