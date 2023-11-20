using Godot;
using System;

public partial class Menu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void _on_play_pressed()
	{
		GetNode<Control>("PlayMenu").Visible = true;

	}
	private void _on_options_pressed()
	{
		GetNode<Control>("OptionsMenu").Visible = true;

	}
	private void _on_quit_pressed()
	{
		GetTree().Quit();
	}
}









