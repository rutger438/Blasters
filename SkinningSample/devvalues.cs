using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace blastersNS
{
    class devvalues
    {
        SpriteFont font;
        String X, Y, Z;
        Blasters game;

        public devvalues(ContentManager content)
        {
            game = new Blasters();
            font = content.Load<SpriteFont>(@"SpriteFont1");
        }
        public void Update(GameTime gameTime, String X, String Y, String Z)
        {
            this.X = "";
            this.X += X;
            this.Y += Y;
            this.Z += Z;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font, X, new Vector2(0, 0), Color.Black);
            spriteBatch.End();
        }
    }
}
