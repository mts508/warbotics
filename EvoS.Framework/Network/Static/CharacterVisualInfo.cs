using System;
using EvoS.Framework.Assets;
using EvoS.Framework.Assets.Serialized;

namespace EvoS.Framework.Network.Static
{
    [Serializable]
    [EvosMessage(390)]
    public struct CharacterVisualInfo : ISerializedItem
    {
        public CharacterVisualInfo(int skin, int pattern, int color)
        {
            skinIndex = skin;
            patternIndex = pattern;
            colorIndex = color;
        }

        public override string ToString()
        {
            return $"{nameof(CharacterVisualInfo)}(" +
                   $"{nameof(skinIndex)}: {skinIndex}, " +
                   $"{nameof(patternIndex)}: {patternIndex}, " +
                   $"{nameof(colorIndex)}: {colorIndex}" +
                   ")";
        }

        public void DeserializeAsset(AssetFile assetFile, StreamReader stream)
        {
            skinIndex = stream.ReadInt32();
            patternIndex = stream.ReadInt32();
            colorIndex = stream.ReadInt32();
        }

        public override bool Equals(object obj)
        {
            if (obj is CharacterVisualInfo)
            {
                CharacterVisualInfo characterVisualInfo = (CharacterVisualInfo) obj;
                return skinIndex == characterVisualInfo.skinIndex &&
                       patternIndex == characterVisualInfo.patternIndex &&
                       colorIndex == characterVisualInfo.colorIndex;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void ResetToDefault()
        {
            skinIndex = 0;
            patternIndex = 0;
            colorIndex = 0;
        }

        public bool IsDefaultSelection()
        {
            return skinIndex == 0 && patternIndex == 0 && colorIndex == 0;
        }

        public int skinIndex;
        public int patternIndex;
        public int colorIndex;
        
    }
}
