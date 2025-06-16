using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// Example demonstration of using ArenaAllocator for memory-efficient crafting simulation.
/// 
/// This example showcases how to use arena allocation to manage memory for a crafting system
/// without triggering garbage collection. The arena allocator provides fast, linear memory 
/// allocation that's perfect for temporary data structures like crafting simulation trees.
/// 
/// Key benefits demonstrated:
/// - Zero GC allocations for crafting simulation data
/// - Fast memory allocation using pointer arithmetic
/// - Simple bulk deallocation via arena reset
/// - Excellent memory locality for performance
/// 
/// The example simulates crafting an Iron Sword which requires Iron Ingots and Sticks,
/// where Iron Ingots themselves need to be crafted from Iron Ore.
/// </summary>
public unsafe class Example : MonoBehaviour {
    /// <summary>
    /// Contains all available crafting recipes. This defines what items can be crafted
    /// and what ingredients they require.
    /// </summary>
    RecipeBook book;
    
    /// <summary>
    /// Player's current inventory containing available materials for crafting.
    /// </summary>
    Inventory inventory;
    
    /// <summary>
    /// Memory arena allocator used for temporary crafting simulation data.
    /// Pre-allocated to hold CraftNode structures without GC pressure.
    /// </summary>
    ArenaAllocator arena;
    
    /// <summary>
    /// Handles the logic for simulating crafting operations and determining
    /// what can be crafted with available resources.
    /// </summary>
    CraftSimulator simulator;

    void Start() {
        // Initialize all systems
        book = new RecipeBook();
        inventory = new Inventory();
        
        // Create arena sized for 10 CraftNode structures
        // CraftNodes are used to build the crafting dependency tree
        arena = new ArenaAllocator(UnsafeUtility.SizeOf<CraftNode>() * 10);
        simulator = new CraftSimulator(book, inventory);
        
        // Set up crafting recipes
        // Recipe 1: Iron Ingot requires 2 Iron Ore
        book.Add(new Recipe(ItemType.IronIngot, new[] {
            new Ingredient { type = ItemType.IronOre, count = 2 }
        }));
        
        // Recipe 2: Iron Sword requires 3 Iron Ingots + 1 Stick
        book.Add(new Recipe(ItemType.IronSword, new[] {
            new Ingredient { type = ItemType.IronIngot, count = 3 },
            new Ingredient { type = ItemType.Stick, count = 1 }
        }));
        
        // Set up initial inventory with raw materials
        inventory.Add(ItemType.IronOre, 10);  // Enough for 5 Iron Ingots (10/2)
        inventory.Add(ItemType.Stick, 3);     // More than enough for Iron Sword
        
        // Simulate crafting 1 Iron Sword using arena allocation
        // This creates a tree structure showing crafting dependencies and availability
        CraftNode* root = simulator.SimulateCraft(arena, ItemType.IronSword, 1);
        
        // Display main crafting result
        Debug.Log($"Can craft {root->outputType}: {root->amountAvailable}/{root->amountNeeded}");

        // Display sub-ingredient requirements and availability
        for (int i = 0; i < root->subCount; i++) {
            CraftNode* sub = root->subIngredients[i];
            Debug.Log($" -> {sub->outputType}: {sub->amountAvailable}/{sub->amountNeeded}");
        }
        
        // Reset arena for potential reuse (makes all allocated memory available again)
        // This is much faster than individual deallocations and produces zero GC
        arena.Reset();
    }
    
    /// <summary>
    /// Cleanup: Dispose of the arena to free unmanaged memory when the GameObject is destroyed.
    /// This prevents memory leaks by properly releasing the arena's underlying buffer.
    /// </summary>
    void OnDestroy() => arena.Dispose();
}