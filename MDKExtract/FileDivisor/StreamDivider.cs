using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MDKExtract.FileDivisor
{
    public class StreamDivider
    {
        private readonly Stream _stream;
        private readonly SortedList<int, StreamAllocation> allocations;
        public StreamDivider(Stream stream)
        {
            _stream = stream;
            allocations = new SortedList<int, StreamAllocation>();
        }

        public int FindHighestPossibleSize(int startOffset)
        {
            var firstLarger = allocations.Select(x => x.Key).Append((int)_stream.Length).First(x => x > startOffset);
            return firstLarger - startOffset;
        }

        public StreamAllocation? DoesBelongToExisting(int offset)
        {
            var firstAtLeast = allocations.FirstOrDefault(x => x.Key >= offset);
            if (firstAtLeast.Value is null)
            {
                return null;
            }
            if (firstAtLeast.Value.Length + firstAtLeast.Key < offset)
                return firstAtLeast.Value;
            return null;
        }

        public void AllocateSection(int start, int count, string name)
        {
            if (DoesBelongToExisting(start) is not null)
                throw new ArgumentException("Already allocated");
            if (FindHighestPossibleSize(start) < count)
                throw new ArgumentException("Allocated too much");
            if (DoesBelongToExisting(start + count - 1) is not null)
                throw new ArgumentException("Redundant check failed");
            allocations.Add(start, new StreamAllocation(count, name));
        }

        public IDisposable StartAllocation(string name)
        {
            return new Allocator(name, this);
        }

        private class Allocator: IDisposable
        {
            private int _start;
            private string _name;
            private StreamDivider _allocator;
            public Allocator(string name, StreamDivider allocator)
            {
                _start = (int)allocator._stream.Position;
                _name = name;
                _allocator = allocator;
                if (_allocator.DoesBelongToExisting(_start) is not null)
                    throw new ArgumentException("This space is already occupied");
            }

            public void Dispose()
            {
                var end = (int)_allocator._stream.Position;
                _allocator.AllocateSection(_start, end - _start, _name);
            }
        }
    }
}
