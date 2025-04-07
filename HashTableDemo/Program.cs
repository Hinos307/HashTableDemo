using HashTableLib;
using System;

namespace HashTableDemo
{
    class Program
    {
        static void Main()
        {
            // SCENARIUSZ: System zarządzania inwentarzem w sklepie elektronicznym
            // ---------------------------------------------------------------
            // Demonstracja wykorzystania tablicy haszującej do śledzenia:
            // - Dostępności produktów
            // - Aktualizacji stanów magazynowych
            // - Wyszukiwania produktów po ID
            // - Obsługi wycofanych produktów

            var inventory = new HashTable<string, Product>();

            // 1. Dodawanie nowych produktów do systemu
            Console.WriteLine("=== ETAP 1: Inicjalizacja inwentarza ===");
            AddProduct(inventory, "LPT-1452", "Laptop Gaming", 15);
            AddProduct(inventory, "MON-9987", "Monitor 4K 32\"", 8);
            AddProduct(inventory, "KEY-3341", "Mechaniczna klawiatura", 23);

            PrintInventory(inventory);
            Console.WriteLine();

            // 2. Aktualizacja stanu magazynowego
            Console.WriteLine("=== ETAP 2: Aktualizacja stanów ===");
            UpdateStock(inventory, "LPT-1452", -3); // Sprzedaż 3 sztuk
            UpdateStock(inventory, "KEY-3341", 10); // Dostawa 10 sztuk
            UpdateStock(inventory, "PHN-0001", 5);  // Próba aktualizacji nieistniejącego produktu

            PrintInventory(inventory);
            Console.WriteLine();

            // 3. Wycofanie produktu z oferty
            Console.WriteLine("=== ETAP 3: Wycofanie produktu ===");
            RemoveProduct(inventory, "MON-9987");
            CheckProduct(inventory, "MON-9987"); // Sprawdzenie usuniętego produktu
            Console.WriteLine();

            // 4. Próba dodania zduplikowanego produktu
            Console.WriteLine("=== ETAP 4: Duplikacja produktu ===");
            AddProduct(inventory, "LPT-1452", "Laptop Premium", 10);
            CheckProduct(inventory, "LPT-1452");
            Console.WriteLine();

            // 5. Statystyki systemu
            Console.WriteLine("=== STATYSTYKI SYSTEMU ===");
            Console.WriteLine($"Łączna liczba produktów: {inventory.Size}");
            Console.WriteLine($"Rozmiar tablicy haszującej: {inventory.Capacity}");
            Console.WriteLine($"Współczynnik wypełnienia: {inventory.LoadFactor:P}");
        }

        // Metody pomocnicze
        static void AddProduct(HashTable<string, Product> inventory, string id, string name, int stock)
        {
            try
            {
                inventory.Put(id, new Product(name, stock));
                Console.WriteLine($"Dodano: {id} - {name} (Ilość: {stock})");
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("BŁĄD: ID produktu nie może być puste!");
            }
        }

        static void UpdateStock(HashTable<string, Product> inventory, string id, int quantityDelta)
        {
            var product = inventory.Get(id);
            if (product != null)
            {
                product.Stock += quantityDelta;
                if (product.Stock < 0) product.Stock = 0;
                Console.WriteLine($"Zaktualizowano {id}: Nowa ilość {product.Stock}");
            }
            else
            {
                Console.WriteLine($"BŁĄD: Produkt {id} nie istnieje w systemie!");
            }
        }

        static void RemoveProduct(HashTable<string, Product> inventory, string id)
        {
            inventory.Remove(id);
            Console.WriteLine($"Wycofano produkt: {id}");
        }

        static void CheckProduct(HashTable<string, Product> inventory, string id)
        {
            var product = inventory.Get(id);
            Console.WriteLine(product != null
                ? $"Stan produktu {id}: {product.Name} (Dostępnych: {product.Stock})"
                : $"Produkt {id} nie istnieje lub został wycofany");
        }

        static void PrintInventory(HashTable<string, Product> inventory)
        {
            Console.WriteLine("\n=== BIEŻĄCY STAN MAGAZYNU ===");
            Console.WriteLine($"Zarejestrowane produkty: {inventory.Size}");
            Console.WriteLine($"Pojemność systemu: {inventory.Capacity}");
            Console.WriteLine($"Współczynnik wypełnienia: {inventory.LoadFactor:P}\n");
        }
    }

    // Klasa pomocnicza reprezentująca produkt
    class Product
    {
        public string Name { get; set; }
        public int Stock { get; set; }

        public Product(string name, int stock)
        {
            Name = name;
            Stock = stock;
        }
    }
}