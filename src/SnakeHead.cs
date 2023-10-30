using Godot;
using System;

public partial class SnakeHead : Sprite2D
{
	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
		None
	};

	private const double SpeedFactor = 0.9;

	public Direction CurrentDirection { get; private set; } = Direction.Right;

	public double CurrentSpeed { get; private set; } = 0.5;

	public bool HasMoved { get; private set; }

	public double Timer { get; private set; }

	public int Score { get; private set; }

	public bool GameOver { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Game over
		if (GameOver) return;

		Timer += delta;

		Vector2 currentPosition = Position;

		Area2D parent = GetParent<Area2D>();
		Node coll = parent.FindChild("CollisionShape2D");
		((CollisionShape2D)coll).Position = Position;

		if (Timer > CurrentSpeed)
		{
			// Update the position of the snake head.
			switch (CurrentDirection)
			{
				case Direction.Up:
				{
					Position += new Vector2(0, -128);
					Texture = (Texture2D)ResourceLoader.Load("res://snake-head-up.png");
					break;
				}
				case Direction.Down:
				{
					Position += new Vector2(0, 128);
					Texture = (Texture2D)ResourceLoader.Load("res://snake-head-down.png");
					break;
				}
				case Direction.Left:
				{
					Position += new Vector2(-128, 0);
					Texture = (Texture2D)ResourceLoader.Load("res://snake-head-left.png");
					break;
				}
				case Direction.Right:
				{
					Position += new Vector2(128, 0);
					Texture = (Texture2D)ResourceLoader.Load("res://snake-head-right.png");
					break;
				}
			}

			if (!ValidateRandomLocation(GlobalPosition))
			{
				// The snake has collided with itself.
				EndGame();
			}

			// Update the positions of the snake body segments.
			foreach (Node node in parent.GetChildren())
			{
				if (node == this) continue;
				if (!(node is Sprite2D)) continue;

				Sprite2D sprite = (Sprite2D)node;

				Vector2 previousPosition = sprite.Position;
				sprite.Position = currentPosition;
				currentPosition = previousPosition;
			}

			HasMoved = true;
			Timer = 0;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (GameOver) return;

		if (!HasMoved) return;

		if (Input.IsKeyPressed(Key.Up))
		{
			if (CurrentDirection == Direction.Down)
				return;

			CurrentDirection = Direction.Up;
		}

		if (Input.IsKeyPressed(Key.Down))
		{
			if (CurrentDirection == Direction.Up)
				return;

			CurrentDirection = Direction.Down;
		}

		if (Input.IsKeyPressed(Key.Left))
		{
			if (CurrentDirection == Direction.Right)
				return;

			CurrentDirection = Direction.Left;
		}

		if (Input.IsKeyPressed(Key.Right))
		{
			if (CurrentDirection == Direction.Left)
				return;

			CurrentDirection = Direction.Right;
		}

		HasMoved = false;
	}

	/// <summary>
	/// Handle what happens when the snake touches the apple.
	/// </summary>
	/// <param name="area"></param>
	private void _on_apple_area_entered(Area2D area)
	{
		// Add one segment to the body of the snake.
		Area2D parent = GetParent<Area2D>();
		Sprite2D lastChild = parent.GetChild<Sprite2D>(parent.GetChildCount() - 1);
		Sprite2D newChild = new Sprite2D();
		newChild.Texture = lastChild.Texture;
		newChild.Position = lastChild.Position;
		parent.AddChild(newChild);

		// Randomly move the apple.
		Node2D apple = GetNode<Node2D>("/root/Node2D/Apple");
		apple.Position = GetRandomLocation(32);

		while (!ValidateRandomLocation(apple.Position))
		{
			apple.Position = GetRandomLocation(32);
		}

		CurrentSpeed *= SpeedFactor;
		Score++;

		// Update the score.
		RichTextLabel score = GetNode<RichTextLabel>("/root/Node2D/Score");
		score.Text = $"Score: {Score}";
	}

	private void _on_visible_on_screen_notifier_2d_screen_exited()
	{
		EndGame();
	}

	/// <summary>
	/// Generate a random location on the screen.
	/// </summary>
	/// <param name="offset"></param>
	/// <returns></returns>
	private Vector2 GetRandomLocation(int offset)
	{
		Random random = new Random();

		// The grid is 18 x 10 squares. Each square is 64 x 64.
		float x = 64 * random.Next(1, 18);
		float y = 64 * random.Next(1, 10);

		return new Vector2(x - offset, y - offset);
	}

	private bool ValidateRandomLocation(Vector2 vector2)
	{
		foreach (Node2D node in GetParent<Area2D>().GetChildren())
		{
			if (node == this) continue;
			if (!(node is Sprite2D)) continue;

			Sprite2D sprite = (Sprite2D)node;

			if (Math.Abs(sprite.GlobalPosition.X - vector2.X) <= 16 && Math.Abs(sprite.GlobalPosition.Y - vector2.Y) <= 16)
			{
				return false;
			}
		}

		return true;
	}

	private void EndGame()
	{
		CurrentDirection = Direction.None;
		GameOver = true;

		RichTextLabel gameOver = GetNode<RichTextLabel>("/root/Node2D/GameOver");
		gameOver.Show();
	}
}
