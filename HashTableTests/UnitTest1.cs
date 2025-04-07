using System;
using System.Collections.Generic;
using Xunit;
using HashTableLib;

namespace HashTableTests
{
    public class HashTableTests
    {
        // Testy konstruktora
        [Fact]
        public void Constructor_InitializesWithDefaultPrimeCapacity()
        {
            var ht = new HashTable<int, int>();
            Assert.True(IsPrime(ht.Capacity));
            Assert.Equal(11, ht.Capacity);
        }

        [Fact]
        public void Constructor_WithCustomCapacity_AdjustsToNextPrime()
        {
            var ht = new HashTable<int, int>(15);
            Assert.True(IsPrime(ht.Capacity));
            Assert.Equal(17, ht.Capacity);
        }

        // Testy metody Put
        [Fact]
        public void Put_NewKey_IncreasesSize()
        {
            var ht = new HashTable<string, int>();
            ht.Put("a", 1);
            ht.Put("b", 2);
            Assert.Equal(2, ht.Size);
        }

        [Fact]
        public void Put_ExistingKey_UpdatesValue()
        {
            var ht = new HashTable<string, int>();
            ht.Put("a", 1);
            ht.Put("a", 2);
            Assert.Equal(2, ht.Get("a"));
        }

        [Fact]
        public void Put_TriggersResize_WhenLoadFactorExceedsThreshold()
        {
            var ht = new HashTable<int, string>();
            int initialCapacity = ht.Capacity;

            // Oblicz próg używając publicznej stałej
            int elementsToAdd = (int)(initialCapacity * HashTable<int, string>.LOAD_FACTOR_THRESHOLD) - ht.Size + 1;

            for (int i = 0; i < elementsToAdd; i++)
            {
                ht.Put(i, i.ToString());
            }

            Assert.True(ht.Capacity > initialCapacity);
        }

        // Testy metody Get
        [Fact]
        public void Get_NonExistentKey_ReturnsDefault()
        {
            var ht = new HashTable<int, string>();
            Assert.Null(ht.Get(999));
        }

        [Fact]
        public void Get_AfterResize_ReturnsAllValues()
        {
            var ht = new HashTable<int, string>();
            var testData = new Dictionary<int, string>();
            var rnd = new Random();

            for (int i = 0; i < 100; i++)
            {
                int key = rnd.Next();
                testData[key] = key.ToString();
                ht.Put(key, key.ToString());
            }

            foreach (var kvp in testData)
            {
                Assert.Equal(kvp.Value, ht.Get(kvp.Key));
            }
        }

        // Testy metody Remove
        [Fact]
        public void Remove_ExistingKey_MarksTombstone()
        {
            var ht = new HashTable<int, string>();
            ht.Put(1, "one");
            ht.Remove(1);
            Assert.Equal(0, ht.Size);
            Assert.Null(ht.Get(1));
        }

        [Fact]
        public void Remove_NonExistentKey_DoesNothing()
        {
            var ht = new HashTable<int, string>();
            ht.Put(1, "one");
            ht.Remove(2);
            Assert.Equal(1, ht.Size);
        }

        // Testy współczynnika wypełnienia
        [Fact]
        public void LoadFactor_CalculatesCorrectly()
        {
            var ht = new HashTable<int, string>();
            for (int i = 0; i < 5; i++)
            {
                ht.Put(i, i.ToString());
            }
            Assert.Equal((double)5 / ht.Capacity, ht.LoadFactor);
        }

        // Testy obsługi kolizji
        [Fact]
        public void Collisions_HandledWithDoubleHashing()
        {
            var ht = new HashTable<CollidingKey, int>();
            var key1 = new CollidingKey(0, 1);
            var key2 = new CollidingKey(0, 2);

            ht.Put(key1, 100);
            ht.Put(key2, 200);

            Assert.Equal(100, ht.Get(key1));
            Assert.Equal(200, ht.Get(key2));
        }

        // Testy obsługi dużych zestawów danych
        [Fact]
        public void StressTest_With10000Elements()
        {
            var ht = new HashTable<int, int>();
            for (int i = 0; i < 10000; i++)
            {
                ht.Put(i, i);
            }

            for (int i = 0; i < 10000; i++)
            {
                Assert.Equal(i, ht.Get(i));
            }
        }

        // Testy specjalnych przypadków
        [Fact]
        public void Put_AfterMultipleDeletes_ReusesTombstones()
        {
            var ht = new HashTable<int, string>();
            int capacity = ht.Capacity;

            // Add and remove elements
            for (int i = 0; i < capacity; i++)
            {
                ht.Put(i, i.ToString());
                ht.Remove(i);
            }

            // Add new elements
            for (int i = 0; i < capacity; i++)
            {
                ht.Put(i + 100, (i + 100).ToString());
            }

            Assert.Equal(capacity, ht.Size);
        }

        [Fact]
        public void Put_ValidatesParameters()
        {
            var ht = new HashTable<string, int>();

            // Test dla null key
            var exception = Assert.Throws<ArgumentNullException>(() => ht.Put(null, 10));
            Assert.Equal("key", exception.ParamName);

            // Test dla poprawnych wartości
            ht.Put("valid", 20);
            Assert.Equal(20, ht.Get("valid"));  // Dodaj asercję potwierdzającą działanie
        }

        // Metody pomocnicze
        private bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            for (int i = 3; i * i <= number; i += 2)
            {
                if (number % i == 0) return false;
            }
            return true;
        }

        private class CollidingKey
        {
            public int HashCode { get; }
            public int Step { get; }

            public CollidingKey(int hashCode, int step)
            {
                HashCode = hashCode;
                Step = step;
            }

            public override int GetHashCode() => HashCode;
        }
    }
}