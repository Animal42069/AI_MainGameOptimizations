using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_MainGameOptimizations
{
    public static class GameLayers
    { 
        public enum LayerStrategy
        {
            None,
            SingleLayer,
            MultiLayer
        }

        public enum Layer
        {
            DefaultLayer = 0, 
            WaterLayer = 4,
            MediumObjectLayer = 6,
            SmallObjectLayer = 7,
            LargeObjectLayer = 9,
            CharaLayer = 10,
            MapLayer = 11
        }

        public enum LayerMask
        {
            None = 0,
            DefaultLayer = (1 << Layer.DefaultLayer),
            LargeObjectLayer = (1 << Layer.LargeObjectLayer),
            WaterLayer = (1 << Layer.WaterLayer),
            MediumObjectLayer = (1 << Layer.MediumObjectLayer),
            SmallObjectLayer = (1 << Layer.SmallObjectLayer),
            CharaLayer = (1 << Layer.CharaLayer),
            MapLayer = (1 << Layer.MapLayer)
        }
    }
}
