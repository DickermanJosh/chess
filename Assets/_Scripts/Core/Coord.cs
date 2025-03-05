using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public struct Coord
    {
        public readonly int file;
        public readonly int rank;

        private static readonly Dictionary<int, char> FileToCharMap = new Dictionary<int, char>
        {
            { 0, 'a' },
            { 1, 'b' },
            { 2, 'c' },
            { 3, 'd' },
            { 4, 'e' },
            { 5, 'f' },
            { 6, 'g' },
            { 7, 'h' }
        };

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
            if (FileToCharMap.TryGetValue(file, out var ch))
            {
                return ch;
            }
            
            Debug.LogError("Invalid file index in Coord struct.");
            return 'z';
        }
    }
}
