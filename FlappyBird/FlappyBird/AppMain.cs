using System;
using System.Collections.Generic;

using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Input;

using Sce.PlayStation.HighLevel.GameEngine2D;
using Sce.PlayStation.HighLevel.GameEngine2D.Base;
using Sce.PlayStation.HighLevel.UI;
	
namespace FlappyBird
{
	public class AppMain
	{
		private static Sce.PlayStation.HighLevel.GameEngine2D.Scene 	gameScene;
		private static Sce.PlayStation.HighLevel.GameEngine2D.Scene		startScene;
		private static Sce.PlayStation.HighLevel.GameEngine2D.Scene		endScene;
		private static Sce.PlayStation.HighLevel.UI.Scene 				uiScene;
		private static Sce.PlayStation.HighLevel.UI.Label				scoreLabel;
		
		private static Obstacle[]	obstacles;
		private static Bird			bird;
		private static Background	background, titleBackground, endBackground;
		private static int 			score;
		private static bool			keyPressed;
		

					
		public static void Main (string[] args)
		{
			Initialize();
			
			//Game loop
			bool quitGame = false;
			while (!quitGame) 
			{
				Update ();
				
				Director.Instance.Update();
				Director.Instance.Render();
				UISystem.Render();
				
				Director.Instance.GL.Context.SwapBuffers();
				Director.Instance.PostSwap();
			}
			
			//Clean up after ourselves.
			bird.Dispose();
			foreach(Obstacle obstacle in obstacles)
				obstacle.Dispose();
			background.Dispose();
			
			Director.Terminate ();
		}

		public static void Initialize ()
		{
			//Set up director and UISystem.
			Director.Initialize ();
			UISystem.Initialize(Director.Instance.GL.Context);
			
			//Set game scene
			gameScene = new Sce.PlayStation.HighLevel.GameEngine2D.Scene();
			gameScene.Camera.SetViewFromViewport();
			gameScene.Name = "gameScene";
			
			startScene = new Sce.PlayStation.HighLevel.GameEngine2D.Scene();			
			startScene.Camera.SetViewFromViewport();
			startScene.Name = "startScene";
			
			endScene = new Sce.PlayStation.HighLevel.GameEngine2D.Scene();
			endScene.Camera.SetViewFromViewport();
			endScene.Name = "endScene";
			
			//Set the ui scene.
			uiScene = new Sce.PlayStation.HighLevel.UI.Scene();
			Panel panel  = new Panel();
			panel.Width  = Director.Instance.GL.Context.GetViewport().Width;
			panel.Height = Director.Instance.GL.Context.GetViewport().Height;
			scoreLabel = new Sce.PlayStation.HighLevel.UI.Label();
			scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
			scoreLabel.VerticalAlignment = VerticalAlignment.Top;
			scoreLabel.SetPosition(
				Director.Instance.GL.Context.GetViewport().Width/2 - scoreLabel.Width/2,
				Director.Instance.GL.Context.GetViewport().Height*0.1f - scoreLabel.Height/2);
			scoreLabel.Text = "0";
			panel.AddChildLast(scoreLabel);
			uiScene.RootWidget.AddChildLast(panel);
			UISystem.SetScene(uiScene);
			
			//Create the background.
			background = new Background(gameScene, gameScene.Name);
			titleBackground = new Background(startScene, startScene.Name);
			endBackground = new Background(endScene, endScene.Name);
			//Create the flappy douche
			bird = new Bird(gameScene);
			
			
			//Create some obstacles.
			obstacles = new Obstacle[2];
			obstacles[0] = new Obstacle(Director.Instance.GL.Context.GetViewport().Width*0.5f, gameScene);	
			obstacles[1] = new Obstacle(Director.Instance.GL.Context.GetViewport().Width, gameScene);
			
			score = 0;
			keyPressed = false;
			
			//Run the scene.
			Director.Instance.RunWithScene(startScene, true);
			
		}
		
		public static void Update()
		{
			
			
			//Determine whether the player tapped the screen
			var touches = Touch.GetData(0);
			
			if(Director.Instance.CurrentScene == startScene)
			{
				scoreLabel.Text = "Touch to start";
				

				if(touches.Count > 0 && !keyPressed)
				{
					Touch.GetData(0).Clear();
					Director.Instance.ReplaceScene(gameScene);
					keyPressed = true;
				}
				
			}
			else if(Director.Instance.CurrentScene == gameScene)
			{
					//If tapped, inform the bird.
					if(touches.Count > 0)
						bird.Tapped();
					
					//Update the bird.
					bird.Update(0.0f);
					
					if(bird.Alive)
					{
						//Move the background.
						background.Update(0.0f);
									
						//Update the obstacles.
						foreach(Obstacle obstacle in obstacles)
							obstacle.Update(0.0f);
						foreach(Obstacle obstacle in obstacles)
						{
							if(obstacle.HasCollidedWith(bird.Sprite))
							{
								bird.Alive = false;
								bird.Sprite.Color = Colors.Black;
							}
							if(obstacle.HasPassed(bird.Sprite))
							{
							   score++;

							}
						}
						scoreLabel.Text = "" + score;
					}
				if(!bird.Alive)
					Director.Instance.ReplaceScene(endScene);
			}
			else if(Director.Instance.CurrentScene == endScene)
			{
				scoreLabel.Text = "You dead";
				score = 0;
				bird.reset();
				bird.Sprite.Color = Colors.White;
				
				obstacles[0].reset(Director.Instance.GL.Context.GetViewport().Width*0.5f);
				obstacles[1].reset(Director.Instance.GL.Context.GetViewport().Width);
				
				if(touches.Count > 0 && !keyPressed)
				{
						Director.Instance.ReplaceScene(startScene);
					keyPressed = true;
				}
			}
			if(touches.Count == 0 && keyPressed)
				keyPressed = false;
			
		}

	}
}
