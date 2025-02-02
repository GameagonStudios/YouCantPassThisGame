using Godot;
using System;
using InputSystem;
using System.Collections.ObjectModel;
using Godot.NativeInterop;

public partial class PaddleController : Area2D
{
  public float Direction;
  [Export]
  float speed = 1.0f;

  [Export]
  float speedBallMultiplier = 1.05f;
  [Export]
  ColorRect background;

  [Export]
  ColorRect rect;

  float Velocity = 0;

  /*La pala debe ser un trigger para cuando la pelota interactue con el trigger la pala debe añadir su velocidad a la de la pelota pero la pelota debe tener una velocidad maxima
  para que no se vaya de madre y que la pelota vaya siempre a su velocidad limite(coger velocidad pelota + velocidad en x de la pala en y normalizas y lo multiplicas por la velocidad limite)*/

  float Max => background.Position.X + background.Size.X - rect.Size.X;
  float Min => background.Position.X;

  public override void _Ready()
  {
    this.BodyEntered += BounceBall;
  }
  public void Move(InputActionState state)
  {
    Direction = ((float)state.strength);

  }
  public override void _PhysicsProcess(double delta)
{
    // Calculamos la velocidad de la pala
    Velocity = Direction * speed * (float)delta;
    this.Position = new Vector2(Mathf.Clamp(Velocity + Position.X, Min, Max), Position.Y);
}

public void BounceBall(Node2D node)
{
    if (node is Ball ball)
    {
      // Usamos la fórmula que ajusta la dirección y magnitud 
      ball.LinearVelocity = new Vector2(
          ball.LinearVelocity.X + Velocity,     // Sumar la velocidad de la pala en X
          -ball.LinearVelocity.Y                // Reflejar la velocidad en Y
      ).Normalized() * ball.LinearVelocity.Length() * (1 + (speedBallMultiplier - 1) * Math.Abs(Velocity));
    }
  }


}
