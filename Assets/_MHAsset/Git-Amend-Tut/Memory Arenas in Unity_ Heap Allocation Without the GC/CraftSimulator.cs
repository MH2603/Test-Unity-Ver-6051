/// <summary>
/// Represents a node in a crafting dependency tree. Each node tracks an item that needs to be crafted,
/// how much is needed vs available, and what sub-ingredients are required to craft it.
/// 
/// This struct uses unsafe pointers for performance and works with arena allocation to avoid
/// garbage collection during crafting simulation.
/// </summary>
public unsafe struct CraftNode {
    /// <summary>The type of item this node represents</summary>
    public ItemType outputType;
    
    /// <summary>How many of this item are needed to satisfy the crafting request</summary>
    public int amountNeeded;
    
    /// <summary>How many of this item can actually be produced given current resources and recipes</summary>
    public int amountAvailable;
    
    /// <summary>
    /// Array of pointers to child nodes representing the ingredients needed to craft this item.
    /// Null if this is a base resource that can't be crafted.
    /// </summary>
    public CraftNode** subIngredients;
    
    /// <summary>Number of sub-ingredients required (length of subIngredients array)</summary>
    public int subCount;
}

/// <summary>
/// Simulates crafting operations by building a dependency tree that shows what resources are needed
/// to craft a target item and whether those resources are available.
/// 
/// This class performs deep analysis of crafting chains by:
/// 1. Looking up recipes for the target item
/// 2. Recursively analyzing each ingredient requirement
/// 3. Calculating the maximum craftable amount based on available resources
/// 4. Building a tree structure that represents the entire crafting dependency chain
/// 
/// The simulation uses arena allocation for high performance and to avoid garbage collection
/// during potentially complex crafting calculations.
/// </summary>
public unsafe class CraftSimulator {
    /// <summary>Recipe database containing all known crafting recipes</summary>
    readonly RecipeBook book;
    
    /// <summary>Current inventory state containing available base resources</summary>
    readonly Inventory inventory;

    /// <summary>
    /// Initializes a new crafting simulator with the given recipe book and inventory.
    /// </summary>
    /// <param name="book">Recipe database to use for crafting lookups</param>
    /// <param name="inventory">Current inventory state for resource availability</param>
    public CraftSimulator(RecipeBook book, Inventory inventory) {
        this.book = book;
        this.inventory = inventory;
    }

    /// <summary>
    /// Simulates crafting the specified item and builds a complete dependency tree showing
    /// what resources are required and how many can actually be produced.
    /// 
    /// The method works recursively:
    /// 1. Creates a craft node for the target item
    /// 2. If a recipe exists, analyzes each ingredient requirement recursively
    /// 3. Calculates the bottleneck (limiting factor) across all ingredients
    /// 4. If no recipe exists, treats it as a base resource from inventory
    /// 
    /// The resulting tree can be traversed to understand the complete crafting chain
    /// and identify which resources are limiting production.
    /// </summary>
    /// <param name="arena">Arena allocator to use for creating the craft tree (no GC pressure)</param>
    /// <param name="item">The type of item to craft</param>
    /// <param name="amountNeeded">How many of this item are needed</param>
    /// <returns>Pointer to the root craft node representing the complete crafting analysis</returns>
    public CraftNode* SimulateCraft(ArenaAllocator arena, ItemType item, int amountNeeded) {
        // Create the root node for this crafting operation
        CraftNode* node = arena.Alloc<CraftNode>();
        node->outputType = item;
        node->amountNeeded = amountNeeded;

        // Check if we have a recipe for crafting this item
        if (book.TryGetRecipe(item, out var recipe)) {
            // This item can be crafted - set up sub-ingredient analysis
            node->subCount = recipe.ingredients.Length;
            node->subIngredients = (CraftNode**)arena.Alloc<byte>(sizeof(CraftNode*) * node->subCount);
            
            // Track the maximum we can craft (limited by scarcest ingredient)
            int maxCraftable = int.MaxValue;

            // Recursively analyze each ingredient requirement
            for (int i = 0; i < recipe.ingredients.Length; i++) {
                var ingredient = recipe.ingredients[i];
                // Calculate total needed considering the recipe multiplier
                int requiredAmount = ingredient.count * amountNeeded;
                
                // Recursively simulate crafting this ingredient
                CraftNode* sub = SimulateCraft(arena, ingredient.type, requiredAmount);
                node->subIngredients[i] = sub;
                
                // Calculate how many complete recipes we can make with this ingredient
                int possible = sub->amountAvailable / ingredient.count;
                // Update our bottleneck if this ingredient is more limiting
                if (possible < maxCraftable) maxCraftable = possible;
            }
            
            // Our max craftable is limited by the scarcest ingredient
            node->amountAvailable = maxCraftable;
        }
        else {
            // No recipe exists - this is a base resource from inventory
            node->subCount = 0;
            node->subIngredients = null;
            // Amount available is whatever we have in inventory
            node->amountAvailable = inventory.GetCount(item);
        }
        
        return node;
    }
}