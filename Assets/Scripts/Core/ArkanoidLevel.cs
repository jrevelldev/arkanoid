using UnityEngine;

namespace Arkanoid.Core
{
    [CreateAssetMenu(fileName = "New Level", menuName = "Arkanoid/Level")]
    public class ArkanoidLevel : ScriptableObject
    {
        public string levelName = "New Level";
        public int rows = 8;
        public int columns = 12;

        [TextArea(10, 16)]
        public string asciiLayout = "............\n............\n............\n............\n............\n............\n............\n............";

        public string[] GetLayoutLines()
        {
            if (string.IsNullOrEmpty(asciiLayout)) return new string[0];
            return asciiLayout.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
