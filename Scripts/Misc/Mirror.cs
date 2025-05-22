using Godot;
using System;

[Tool]
public partial class Mirror : Node3D
{
	[Export]
	public Camera3D camera;

	[Export]
	public SubViewport viewport;

	[Export(PropertyHint.Range, "0,1")]
	public float viewportResolutionFactor { get => _viewportResolutionFactor; set
		{
			_viewportResolutionFactor = value;
			FitViewport();
	} }

	float _viewportResolutionFactor;

	public override void _EnterTree()
	{
		base._EnterTree();

		GetWindow().SizeChanged += FitViewport;
	}

	public override void _Ready()
	{
		base._Ready();
		
		FitViewport();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		UpdateCamera();
	}

	public void UpdateCamera()
	{
		Camera3D currentCamera = GetViewport().GetCamera3D();

		camera.Fov = currentCamera.Fov;
		if (camera.KeepAspect != currentCamera.KeepAspect) camera.KeepAspect = currentCamera.KeepAspect;
		camera.GlobalTransform = GlobalTransform * Transform3D.FlipZ * GlobalTransform.AffineInverse() * currentCamera.GlobalTransform;

		camera.Basis = camera.Basis * Basis.FlipX;
	}

	public void FitViewport()
	{
		if(viewport != null) viewport.Size = (Vector2I)((Vector2)GetWindow().Size * viewportResolutionFactor).Floor();
	}
}
