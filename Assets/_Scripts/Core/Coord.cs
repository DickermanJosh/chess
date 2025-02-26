using UnityEngine;

namespace Core
{
    public struct Coord
    {
        public readonly int file;
        public readonly int rank;

        public Coord(int file, int rank)
        {
            this.file = file;
            this.rank = rank;
        }

        public readonly int GetIndex()
        {
            return file * 8 + rank;
        }

        override public readonly string ToString()
        {
           return $"{GetFileAsChar(file)}{rank + 1}";
        }

        public readonly Vector2 ToVector2()
        {
            return new Vector2(file, rank);
        }

        private readonly char GetFileAsChar(int file)
        {
            switch (file)
            {
                case 0: return 'A';
                case 1: return 'B';
                case 2: return 'C';
                case 3: return 'D';
                case 4: return 'E';
                case 5: return 'F';
                case 6: return 'G';
                case 7: return 'H';
            }

            return 'Z'; // return invalid file if not found
        }
    }
}
