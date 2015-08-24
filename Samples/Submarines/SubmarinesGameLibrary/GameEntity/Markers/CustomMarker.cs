using Fusion;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.Field;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity.Markers
{
    public class CustomMarker : Marker
    {
        private CustomMarker(Texture2D texture, Cell cell)
            : base(texture, cell)
        { }

        static private GraphicsDevice _graphicsDevice;
        static internal GraphicsDevice GraphicsDevice { set { _graphicsDevice = value; } }

        static private Dictionary<String, Texture2D> textures = new Dictionary<String, Texture2D>();

        static public void registerNewMarker(String name, String path)
        {
            try
            {
                textures.Add(name, new Texture2D(_graphicsDevice, File.Open(path, FileMode.Open), false));
            }
            catch (Exception ex)
            {
                Log.Message("Something wrong with register " + name + ": " + ex.ToString());
            }
        }

        static public CustomMarker getMarker(String name, Cell cell)
        {
            Texture2D texture;
            if (textures.TryGetValue(name, out texture))
                return new CustomMarker(texture, cell);
            else
                return null;
        }

        static internal void Dispose()
        {
            foreach (Texture2D texture in textures.Values)
                texture.Dispose();
        }
    }
}
