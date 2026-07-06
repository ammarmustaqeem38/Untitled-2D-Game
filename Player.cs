using Godot;

public partial class Player : CharacterBody2D
{
	[Export] 
	public float Speed = 400f;

	private AnimatedSprite2D anim;

	public override void _Ready()
	{
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Vector2.Zero;

		if (Input.IsActionPressed("ui_right"))
			direction.X += 1;
		if (Input.IsActionPressed("ui_left"))
			direction.X -= 1;
		if (Input.IsActionPressed("ui_down"))
			direction.Y += 1;
		if (Input.IsActionPressed("ui_up"))
			direction.Y -= 1;

		if (direction != Vector2.Zero)
		{
			Velocity = direction.Normalized() * Speed;
			UpdateAnimation(direction);
		}
		else
		{
			Velocity = Vector2.Zero;
			anim.Stop(); 
		}

		MoveAndSlide();
	}

	private void UpdateAnimation(Vector2 direction)
	{
		// Reset horizontal flipping
		anim.FlipH = false;

		// Prioritize horizontal movement animations
		if (direction.X > 0)
		{
			anim.Play("walk_right");
		}
		else if (direction.X < 0)
		{
			// If you have a separate "walk_left" animation:
			anim.Play("walk_left");
			
			// OR if you want to reuse "walk_right" flipped, uncomment below:
			// anim.Play("walk_right");
			// anim.FlipH = true;
		}
		// Handle vertical movement animations
		else if (direction.Y > 0)
		{
			anim.Play("walk_down");
		}
		else if (direction.Y < 0)
		{
			anim.Play("walk_up");
		}
	}
}
