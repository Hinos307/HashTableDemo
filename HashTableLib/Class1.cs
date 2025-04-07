using System;
using System.Collections;
using System.Collections.Generic;
namespace HashTableLib
{
    public class HashTable<Key, Value> where Key : notnull
    {
        private const int INITIAL_CAPACITY = 11;
        public const double LOAD_FACTOR_THRESHOLD = 0.75;

        private Entry[] table;
        private int size;
        private int tombstones;
        private int capacity;

        private class Entry
        {
            public Key Key { get; }
            public Value Value { get; set; }
            public bool IsDeleted { get; set; }

            public Entry(Key key, Value value)
            {
                Key = key;
                Value = value;
                IsDeleted = false;
            }
        }

        public HashTable() : this(INITIAL_CAPACITY) { }

        public HashTable(int initialCapacity)
        {
            capacity = NextPrime(initialCapacity);
            table = new Entry[capacity];
            size = 0;
            tombstones = 0;
        }

        public void Put(Key key, Value value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (ShouldResize())
            {
                Resize();
            }

            int index = Hash1(key);
            int step = Hash2(key);
            int firstTombstoneIndex = -1;

            for (int attempt = 0; attempt < capacity; attempt++)
            {
                int currentIndex = (index + attempt * step) % capacity;
                Entry currentEntry = table[currentIndex];

                if (currentEntry is null)
                {
                    InsertEntry(key, value, ref firstTombstoneIndex, currentIndex);
                    return;
                }

                if (currentEntry.IsDeleted)
                {
                    TrackFirstTombstone(ref firstTombstoneIndex, currentIndex);
                    continue;
                }

                if (currentEntry.Key.Equals(key))
                {
                    UpdateEntryValue(currentEntry, value);
                    return;
                }
            }

            HandleFullTable(key, value, ref firstTombstoneIndex);
        }

        // Metody pomocnicze
        private bool ShouldResize()
            => (double)(size + 1) / capacity > LOAD_FACTOR_THRESHOLD;

        private void InsertEntry(Key key, Value value, ref int tombstoneIndex, int currentIndex)
        {
            if (tombstoneIndex != -1)
            {
                table[tombstoneIndex] = new Entry(key, value);
                tombstones--;
            }
            else
            {
                table[currentIndex] = new Entry(key, value);
            }
            size++;
        }

        private void TrackFirstTombstone(ref int tombstoneIndex, int currentIndex)
        {
            if (tombstoneIndex == -1)
            {
                tombstoneIndex = currentIndex;
            }
        }

        private void UpdateEntryValue(Entry entry, Value value)
        {
            entry.Value = value;
        }

        private void HandleFullTable(Key key, Value value, ref int tombstoneIndex)
        {
            if (tombstoneIndex != -1)
            {
                table[tombstoneIndex] = new Entry(key, value);
                tombstones--;
                size++;
                return;
            }

            throw new InvalidOperationException("HashTable is full. Maximum capacity reached.");
        }

        public Value? Get(Key key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int initialIndex = CalculateInitialIndex(key);
            int stepSize = CalculateStepSize(key);

            for (int attempt = 0; attempt < capacity; attempt++)
            {
                int currentIndex = GetProbedIndex(initialIndex, stepSize, attempt);
                Entry currentEntry = table[currentIndex];

                if (IsEmptyBucket(currentEntry))
                {
                    return default;
                }

                if (IsValidEntry(currentEntry, key))
                {
                    return currentEntry.Value;
                }
            }

            return default;
        }

        // Metody pomocnicze
        private int CalculateInitialIndex(Key key)
            => (key.GetHashCode() & 0x7FFFFFFF) % capacity;

        private int CalculateStepSize(Key key)
            => ((key.GetHashCode() & 0x7FFFFFFF) % (capacity - 1)) + 1;

        private int GetProbedIndex(int initialIndex, int stepSize, int attempt)
            => (initialIndex + attempt * stepSize) % capacity;

        private bool IsEmptyBucket(Entry entry)
            => entry is null;

        private bool IsValidEntry(Entry entry, Key key)
            => !entry.IsDeleted && entry.Key.Equals(key);

        public void Remove(Key key)
        {
            int index = Hash1(key);
            int step = Hash2(key);

            for (int i = 0; i < capacity; i++)
            {
                int currentIndex = (index + i * step) % capacity;
                var entry = table[currentIndex];

                if (entry == null) return;

                if (!entry.IsDeleted && entry.Key.Equals(key))
                {
                    entry.IsDeleted = true;
                    size--;
                    tombstones++;
                    return;
                }
            }
        }

        public int Size => size;
        public int Capacity => capacity;
        public double LoadFactor => (double)size / capacity;

        private int Hash1(Key key) => (key.GetHashCode() & 0x7FFFFFFF) % capacity;

        private int Hash2(Key key) => ((key.GetHashCode() & 0x7FFFFFFF) % (capacity - 1)) + 1;

        private void Resize()
        {
            int newCapacity = NextPrime(capacity * 2);
            var newTable = new Entry[newCapacity];

            for (int i = 0; i < capacity; i++)
            {
                var entry = table[i];
                if (entry == null || entry.IsDeleted) continue;

                int index = (entry.Key.GetHashCode() & 0x7FFFFFFF) % newCapacity;
                int step = ((entry.Key.GetHashCode() & 0x7FFFFFFF) % (newCapacity - 1)) + 1;

                for (int j = 0; j < newCapacity; j++)
                {
                    int currentIndex = (index + j * step) % newCapacity;
                    if (newTable[currentIndex] == null)
                    {
                        newTable[currentIndex] = entry;
                        break;
                    }
                }
            }

            table = newTable;
            capacity = newCapacity;
            tombstones = 0;
        }

        private int NextPrime(int n)
        {
            if (n <= 2) return 2;
            int candidate = n % 2 == 0 ? n + 1 : n;

            while (true)
            {
                if (IsPrime(candidate)) return candidate;
                candidate += 2;
            }
        }

        private bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2 || number == 3) return true;
            if (number % 2 == 0 || number % 3 == 0) return false;

            for (int i = 5; i * i <= number; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0) return false;
            }
            return true;
        }
    }
}
