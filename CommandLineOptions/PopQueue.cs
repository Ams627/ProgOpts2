﻿// Copyright (c) Adrian Sims 2020
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

namespace CommandLineOptions
{
    /// <summary>
    /// Initialises an instance of the PopQueue class. This class allows removal of an entity from the front of a queue
    /// only. The queue is initialised with an array and an offset from which to start popping.
    ///     It's not possible to add to the queue
    ///     You can only pop from the front
    ///     You can undo pops up to the point specified by the original offset
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PopQueue<T>
    {
        private readonly T[] _array;
        private int _remaining;
        private readonly int _originalCount;

        public PopQueue(T[] array, int offset = 0)
        {
            _array = array;
            _originalCount = array.Length - offset;
            _remaining = _originalCount;
        }

        /// <summary>
        /// the ORIGINAL number of items in the queue - never changes after construction
        /// </summary>
        public int OriginalCount => _originalCount;

        /// <summary>
        /// The remaining number of items in the queue:
        /// </summary>
        public int Remaining => _remaining;

        /// <summary>
        /// true if the queue is empty:
        /// </summary>
        public bool Empty => _remaining == 0;

        public (T item, int index) PopFront()
        {
            if (_remaining == 0)
            {
                throw new InvalidOperationException($"No items remaining in the PopList");
            }
            var result = (_array[_array.Length - _remaining], _array.Length - Remaining);
            _remaining--;
            return result;
        }

        /// <summary>
        /// Undo the last pop
        /// </summary>
        public void Undo()
        {
            if (_remaining == _originalCount)
            {
                throw new InvalidOperationException($"Cannot undo in poplist - the poplist is at its original fill level.");
            }
            _remaining++;
        }
    }
}