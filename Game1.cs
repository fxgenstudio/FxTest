#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

#endregion

//based on https://github.com/keijiro/RippleEffect

namespace FxTest
{


	class Droplet
	{
		//Random rnd;
		Vector2 position;

		float time;

		public Droplet()
		{
			//rnd = new Random(1234);
			time = 1000;
		}

		public void Reset()
		{
			//position = new Vector2((float)rnd.NextDouble(), (float)rnd.NextDouble());
			position = new Vector2(0.5f, 0.5f);
			time = 0;
		}

		public void Update(float _dt)
		{
			time += _dt;
		}

		public Vector3 MakeShaderParameter(float aspect)
		{
			return new Vector3(position.X * aspect, position.Y, time);
		}

	}


	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager m_graphics;
		SpriteBatch spriteBatch;

		Effect distortEffect;
		EffectTechnique distortTechnique;

		RenderTarget2D sceneMap;
		RenderTarget2D distortionMap;

		Texture2D backgroundTexture;
		Texture2D gradTexture;

		Vector2 spritePos;

		Droplet[] droplets;
		public float waveSpeed = 1.25f;
		public float reflectionStrength = 1.7f;
		public Color reflectionColor = Color.Gray;
		public float refractionStrength = 1.5f;
		public float dropInterval = 0.5f;

		float timer;

		int dropCount;


		public Game1()
		  : base()
		{
			m_graphics = new GraphicsDeviceManager(this);

			m_graphics.IsFullScreen = false;
			m_graphics.PreferredBackBufferWidth = 800;
			m_graphics.PreferredBackBufferHeight = 600;
			this.IsMouseVisible = true;


			Content.RootDirectory = "Content";


			droplets = new Droplet[3];

			droplets[0] = new Droplet();

			droplets[1] = new Droplet();

			droplets[2] = new Droplet();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{

			//Init OpEngine
			string strpath = @"Assets";

			spriteBatch = new SpriteBatch(m_graphics.GraphicsDevice);
	



			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// TODO: use this.Content to load your game content here
			distortEffect = Content.Load<Effect>("Distorter_Ripple");
			distortTechnique = distortEffect.Techniques["Technique1"];

			backgroundTexture = Content.Load<Texture2D>("background");

			//displacementMap = Content.Load<Texture2D>("displacementmap");

			// look up the resolution and format of our main backbuffer
			PresentationParameters pp = GraphicsDevice.PresentationParameters;
			int width = pp.BackBufferWidth;
			int height = pp.BackBufferHeight;
			SurfaceFormat format = pp.BackBufferFormat;
			DepthFormat depthFormat = pp.DepthStencilFormat;

			// create textures for reading back the backbuffer contents
			sceneMap = new RenderTarget2D(GraphicsDevice, width, height, false, format, depthFormat);
			//distortionMap = new RenderTarget2D(GraphicsDevice, width, height, false, format, depthFormat);


			// Build Displacement texture


			Curve waveform = new Curve();

			waveform.Keys.Add(new CurveKey(0.00f, 0.50f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.05f, 1.00f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.15f, 0.10f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.25f, 0.80f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.35f, 0.30f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.45f, 0.60f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.55f, 0.40f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.65f, 0.55f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.75f, 0.46f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.85f, 0.52f, 0, 0));
			waveform.Keys.Add(new CurveKey(0.99f, 0.50f, 0, 0));


			gradTexture = new Texture2D(GraphicsDevice, 256, 1, false, SurfaceFormat.Color);

			Color[] datas = new Color[256];

			for (var i = 0; i < gradTexture.Width; i++)
			{
				var x = 1.0f / gradTexture.Width * i;
				var a = waveform.Evaluate(x);
				datas[i] = new Color(a, a, a, a);
			}

			gradTexture.SetData<Color>(datas);

			//var stream = File.Create("c:\\temp\\file.png");
			//gradTexture.SaveAsPng(stream, 2048,1);



		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			spriteBatch = null;
			distortEffect = null;
			distortTechnique = null;
			gradTexture = null;

			if (sceneMap != null)
			{
				sceneMap.Dispose();
				sceneMap = null;
			}
			if (distortionMap != null)
			{
				distortionMap.Dispose();
				distortionMap = null;
			}
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			//if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			//  Exit();

			var ms = Mouse.GetState();
			
			spritePos = ms.Position.ToVector2();


			if (dropInterval > 0)

			{

				timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

				while (timer > dropInterval)

				{

					droplets[dropCount++ % droplets.Length].Reset();

					timer -= dropInterval;

				}

			}

			foreach (var d in droplets)
				d.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{

			

			var gd1 = GraphicsDevice;

			

			Viewport viewport = GraphicsDevice.Viewport;

			float aspect = viewport.AspectRatio;

			Matrix projection;
			Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, -1, out projection);
			Matrix _matrix = Matrix.Identity;//matrix use in spriteBatch.Draw

			Matrix.Multiply(ref _matrix, ref projection, out projection);

			this.distortEffect.Parameters["MatrixTransform"].SetValue(projection);

			this.distortEffect.Parameters["GradTexture"].SetValue(this.gradTexture);

			this.distortEffect.Parameters["_Reflection"].SetValue(reflectionColor.ToVector4() );

			this.distortEffect.Parameters["_Params1"].SetValue(new Vector4(aspect, 1, 1 / waveSpeed, 0));    // [ aspect, 1, scale, 0 ]
			this.distortEffect.Parameters["_Params2"].SetValue(new Vector4(1, 1 / aspect, refractionStrength, reflectionStrength));    // [ 1, 1/aspect, refraction, reflection ]

			this.distortEffect.Parameters["_Drop1"].SetValue(droplets[0].MakeShaderParameter(aspect));


			//Draw background for test
			GraphicsDevice.SetRenderTarget(sceneMap);


			GraphicsDevice.Clear(Color.CornflowerBlue);
			DrawFullscreenQuad(backgroundTexture, viewport.Width, viewport.Height, null);


			//Drawing sprites effect
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, DepthStencilState.None, null, distortEffect);

            //spriteBatch.Draw(sceneMap, new Rectangle((int)spritePos.X - 128, (int)spritePos.Y - 128 , 256, 256), Color.White);

            Color c = Color.White;

            Vector2 scaleFactor = new Vector2(0.5f, 0.5f);
            Vector2 origin = new Vector2(sceneMap.Width / 2 , sceneMap.Height / 2 );
            Vector2 pos = new Vector2((int)spritePos.X , (int)spritePos.Y );

            spriteBatch.Draw(sceneMap,
                        pos,
                        null,
                        c,
                        0f,
                        origin,
                        scaleFactor, //m_particles[i].lifeTime / (float)m_nEndLifeTime,
                        SpriteEffects.None,
                        0
                     );



            var d = droplets[0].MakeShaderParameter(aspect);
			d.Z += 0.1f;
			this.distortEffect.Parameters["_Drop1"].SetValue(d);

			spriteBatch.Draw(sceneMap, new Rectangle(100, 100, 256, 256), Color.White);

			spriteBatch.End();



			GraphicsDevice.SetRenderTarget(null);
			DrawFullscreenQuad(sceneMap, viewport.Width, viewport.Height, null);


			base.Draw(gameTime);

		}

		/// <summary>
		/// Helper for drawing a texture into the current rendertarget,
		/// using a custom shader to apply postprocessing effects.
		/// </summary>
		void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect)
		{

			spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
			spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
			spriteBatch.End();
		}

	}
}
