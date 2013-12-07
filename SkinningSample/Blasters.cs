using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SkinnedModel;

namespace blastersNS
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Blasters : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model myModel;
        Model worldModel, block;
        float jumpSpeed = 1;
        float aspectRatio;
        public String X, Y, Z;
        SpriteFont font;
        Rectangle playerCollition2d, box1;
        BoundingBox boundingBoxChar, BoundingBoxBlock;
        AnimationPlayer animationPlayer;
        // Set the position of the model in world space, and set the rotation.
        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;
        // Set the velocity of the model, applied each frame to the model's position.
        Vector3 modelVelocity = Vector3.Zero;

        // Set the position of the camera in world space, for our view matrix.
        Vector3 cameraPosition = new Vector3(0.0f, 300.0f, 600.0f);

        public Blasters()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }
        protected override void LoadContent()
        {
            modelPosition.Z = 120;
            font = Content.Load<SpriteFont>(@"SpriteFont1");
            modelPosition.X = 60;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            myModel = Content.Load<Model>(@"dude");
            block = Content.Load<Model>(@"block");
            worldModel = Content.Load<Model>(@"plane");
            playerCollition2d = new Rectangle(0, 0, 20, 20);
            box1 = new Rectangle(0, 0, 50, 50);
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            // Look up our custom skinning information.
            SkinningData skinningData = myModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip1 = skinningData.AnimationClips["Take 001"];

            animationPlayer.StartClip(clip1);
        }
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            X = "X:";
            X += modelPosition.X;
            Y = "Y:";
            Y += modelPosition.Y;
            Z = "Z:";
            Z += modelPosition.Z;
            boundingBoxChar = new BoundingBox(new Vector3(modelPosition.X - 10, modelPosition.Y, modelPosition.Z - 10), new Vector3(modelPosition.X + 10, modelPosition.Y + 50, modelPosition.Z + 10));
            BoundingBoxBlock = new BoundingBox(new Vector3(- 110, -10, - 110), new Vector3(110, 50, 110));
            if (boundingBoxChar.Intersects(BoundingBoxBlock))
            {
                if (modelPosition.Z <= -109)
                {
                    modelVelocity.Z -= 3;
                }
                if (modelPosition.Z >= 109)
                {
                    modelVelocity.Z += 3;
                }
                if (modelPosition.X <= -109)
                {
                    modelVelocity.X -= 3;
                }
                if (modelPosition.X >= 109)
                {
                    modelVelocity.X += 3;
                }
            }
            /*for(int i = 0; i < block.Meshes.Count; i++)
            {
                BoundingSphere c1BoundingSphere = block.Meshes[i].BoundingSphere;
                c1BoundingSphere.Center += modelPosition;
                for (int j = 0; j < myModel.Meshes.Count; j++)
                {
                    BoundingSphere c2BoundingSphere = myModel.Meshes[j].BoundingSphere;
                    c2BoundingSphere.Center += modelPosition;
                    if (!c1BoundingSphere.Intersects(c2BoundingSphere))
                    {
                        modelPosition.Z += 20;
                        break;
                    }
                }
            }*/

            // Get some input.
            UpdateInput(gameTime);

            // Add velocity to the current position.
            modelPosition += modelVelocity;

            // Bleed off velocity over time.
            modelVelocity *= 0.0f;
            
            base.Update(gameTime);
        }
        
        protected void UpdateInput(GameTime gameTime)
        {
            // Get the game pad state.
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);
            if (currentState.IsConnected)
            {
                // Rotate the model using the left thumbstick, and scale it down
                modelRotation -= currentState.ThumbSticks.Left.X * 0.10f;

                // Create some velocity if the right trigger is down.
                Vector3 modelVelocityAdd = Vector3.Zero;

                // Find out what direction we should be thrusting, 
                // using rotation.
                modelVelocityAdd.X = -(float)Math.Sin(modelRotation);
                modelVelocityAdd.Z = -(float)Math.Cos(modelRotation);

                // Now scale our direction by how hard the trigger is down.
                modelVelocityAdd *= -(currentState.ThumbSticks.Left.Y*2);

                if (currentState.Buttons.A == ButtonState.Pressed)
                {
                    if (modelPosition.Y <= 10)
                    {
                        jumpSpeed = 3;
                    }
                    Jump();    
                }
                if (currentState.Buttons.A == ButtonState.Released)
                {
                    if (modelPosition.Y > 10)
                    {
                        jumpSpeed = -3;
                        Jump();
                    }
                }

                // Set Model velocity to 0 if analog stick is released so the character doesn't float around
                if (currentState.ThumbSticks.Left.Y <= 0.3)
                {
                    modelVelocity = Vector3.Zero;
                }
                if (currentState.ThumbSticks.Left.Y >= 0.4)
                {
                    animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
                }

                // Finally, add this vector to our velocity.
                modelVelocity -= modelVelocityAdd;


                // In case you get lost, press A to warp back to the center.
                if (currentState.Buttons.B == ButtonState.Pressed)
                {
                    modelPosition = Vector3.Zero;
                    modelVelocity = Vector3.Zero;
                    modelRotation = 0.0f;
                }
            }
        }
        public void Jump()
        {
            if (modelPosition.Y >= 60)
            {
                jumpSpeed = -3;
            }
            if (modelPosition.Y >= 0)
            {
                modelPosition.Y += jumpSpeed;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            Matrix[] bones = animationPlayer.GetSkinTransforms();
            myModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] *
                        Matrix.CreateRotationY(modelRotation)
                        * Matrix.CreateTranslation(modelPosition);
                    effect.View = Matrix.CreateLookAt(cameraPosition,
                        Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio,
                        1.0f, 10000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            foreach (ModelMesh mesh in worldModel.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.View = Matrix.CreateLookAt(cameraPosition,
                        Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio,
                        1.0f, 10000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            foreach (ModelMesh mesh in block.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.View = Matrix.CreateLookAt(cameraPosition,
                        Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio,
                        1.0f, 10000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            spriteBatch.Begin();
            spriteBatch.DrawString(font, X+"  "+Y+"  "+Z, new Vector2(0, 0), Color.Black);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (Blasters game = new Blasters())
            {
                game.Run();
            }
        }
    }

    #endregion
}
