using Godot;
using System;
using InputSystem;
using System.Collections.ObjectModel;
using Godot.NativeInterop;

public partial class Pala : AnimatableBody2D
{
	float Direction;
	[Export]
	float speed = 1.0f;

  [Export]
  ColorRect background;

  [Export] 
  ColorRect player;


/*La pala debe ser un trigger para cuando la pelota interactue con el trigger la pala debe añadir su velocidad a la de la pelota pero la pelota debe tener una velocidad maxima
para que no se vaya de madre y que la pelota vaya siempre a su velocidad limite(coger velocidad pelota + velocidad en x de la pala en y normalizas y lo multiplicas por la velocidad limite)*/

float maxRight;
float maxLeft;
  public void Move(InputActionState state)
  {
    Direction = ((float)state.strength);
    maxRight = background.Position.X + background.Size.X - player.Size.X - 0.01f;
    maxLeft = background.Position.X;

  }
  public override void _PhysicsProcess(double delta)
  {


    if (Direction == 1 && this.Position.X <= maxRight || Direction == -1 && this.Position.X >= maxLeft) 
    {
      this.Position = new Vector2(Direction * speed * (float)delta, 0) + Position;
    }



  }
}
