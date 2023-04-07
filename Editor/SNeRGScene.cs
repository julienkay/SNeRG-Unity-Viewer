using System;

namespace SNeRG.Editor {

    public enum SNeRGScene {
        Custom = -1,
        Lego,
        Chair,
        Drums,
        Hotdog,
        Ship,
        Mic,
        Ficus,
        Materials,
        Spheres,
        VaseDeck,
        PineCone,
        ToyCar
    }

    public static class SNeRGSceneExtensions {
        public static string Name(this SNeRGScene scene) {
            return scene.ToString();
        }

        public static string LowerCaseName(this SNeRGScene scene) {
            return scene.ToString().ToLower();
        }

        public static SNeRGScene ToEnum(string value) {
            return (SNeRGScene)Enum.Parse(typeof(SNeRGScene), value, true);
        }

        public static bool IsSynthetic(this SNeRGScene scene) {
            switch (scene) {
                case SNeRGScene.Lego:
                case SNeRGScene.Chair:
                case SNeRGScene.Drums:
                case SNeRGScene.Hotdog:
                case SNeRGScene.Ship:
                case SNeRGScene.Mic:
                case SNeRGScene.Ficus:
                case SNeRGScene.Materials:
                    return true;
                case SNeRGScene.Spheres:
                case SNeRGScene.VaseDeck:
                case SNeRGScene.PineCone:
                case SNeRGScene.ToyCar:
                    return false;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}