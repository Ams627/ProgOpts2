using System;

namespace ProgOpts2
{
    public class PopList<T>
    {
        private readonly T[] _array;
        private int _remaining;
        private readonly int _count;

        public PopList(T[] array, int offset = 0)
        {
            _array = array;
            _count = array.Length - offset;
            _remaining = _count;
        }

        public int Count => _count;
        public int Remaining => _remaining;
        public bool Empty => _remaining == 0;

        public (T item, int index) Current => (_array[_array.Length - _remaining], _array.Length - Remaining);

        public (T item, int index) PopFront()
        {
            if (_remaining == 0)
            {
                throw new InvalidOperationException($"No items remaining in the PopList");
            }
            var result = Current;
            _remaining--;
            return result;
        }

        public void Undo()
        {
            if (_remaining == _count)
            {
                throw new InvalidOperationException($"Cannot undo in poplist - the poplist is at its original fill level.");
            }
            _remaining++;
        }
    }
}