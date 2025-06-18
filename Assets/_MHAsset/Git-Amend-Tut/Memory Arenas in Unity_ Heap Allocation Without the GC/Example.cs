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
        //Git_Amend_Example();
        DemonstrateReuseFeature();
    }


    void Git_Amend_Example()
    {
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
    
    void DemonstrateReuseFeature() {
        arena = new ArenaAllocator(1024);
    
        // ========== PHASE 1: Create CraftNodes ==========
        CraftNode* node1 = arena.Alloc<CraftNode>();
        node1->outputType = ItemType.IronSword;
        node1->amountNeeded = 5;
    
        CraftNode* node2 = arena.Alloc<CraftNode>();
        node2->outputType = ItemType.IronIngot;
        node2->amountNeeded = 10;
    
        Debug.Log($"Before Reset - Node1: {node1->outputType}, Amount: {node1->amountNeeded}");
        Debug.Log($"Before Reset - Node2: {node2->outputType}, Amount: {node2->amountNeeded}");
        Debug.Log($"Arena offset: {arena.offset}"); // Will show ~128 bytes used
    
        // ========== PHASE 2: Reset Arena ==========
        arena.Reset(); // ONLY resets offset to 0
    
        Debug.Log($"After Reset - Arena offset: {arena.offset}"); // Shows 0
    
        // ========== PHASE 3: Data Still Exists! ==========
        // Old pointers still point to same memory addresses
        Debug.Log($"After Reset - Node1 STILL: {node1->outputType}, Amount: {node1->amountNeeded}");
        Debug.Log($"After Reset - Node2 STILL: {node2->outputType}, Amount: {node2->amountNeeded}");
        // Data is still there! Just the offset was reset!
    
        // ========== PHASE 4: New Allocation Overwrites ==========
        CraftNode* newNode = arena.Alloc<CraftNode>();
        newNode->outputType = ItemType.Stick;
        newNode->amountNeeded = 99;
    
        // Now node1's memory has been overwritten (same memory address as newNode)
        Debug.Log($"After New Alloc - Node1 NOW: {node1->outputType}, Amount: {node1->amountNeeded}");
        // Shows: Stick, 99 (because newNode overwrote node1's memory!)
    
        Debug.Log($"NewNode address: {(long)newNode}");
        Debug.Log($"Node1 address: {(long)node1}");
        // These addresses are THE SAME!
    }
    
    /// <summary>
    /// Cleanup: Dispose of the arena to free unmanaged memory when the GameObject is destroyed.
    /// This prevents memory leaks by properly releasing the arena's underlying buffer.
    /// </summary>
    void OnDestroy() => arena.Dispose();
}