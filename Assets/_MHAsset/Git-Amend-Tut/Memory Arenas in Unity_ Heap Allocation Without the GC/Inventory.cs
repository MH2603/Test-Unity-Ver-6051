using System.Collections.Generic;

/// <summary>
/// Enumeration defining the different types of items available in the game.
/// Used as keys for inventory management and recipe definitions.
/// </summary>
public enum ItemType { IronOre, IronIngot, IronSword, Wood, Stick }

/// <summary>
/// Represents an ingredient required for crafting recipes.
/// Contains the item type and the quantity needed.
/// </summary>
public struct Ingredient {
    /// <summary>The type of item required</summary>
    public ItemType type;
    /// <summary>The quantity of this item needed</summary>
    public int count;
}

/// <summary>
/// Manages a collection of crafting recipes.
/// Provides functionality to add recipes and look them up by output item type.
/// </summary>
public class RecipeBook {
    /// <summary>Dictionary storing recipes indexed by their output item type</summary>
    readonly Dictionary<ItemType, Recipe> recipes = new();
    
    /// <summary>
    /// Adds a new recipe to the recipe book.
    /// If a recipe for the same output already exists, it will be replaced.
    /// </summary>
    /// <param name="recipe">The recipe to add</param>
    public void Add(Recipe recipe) => recipes[recipe.output] = recipe;
    
    /// <summary>
    /// Attempts to retrieve a recipe for the specified item type.
    /// </summary>
    /// <param name="type">The item type to find a recipe for</param>
    /// <param name="recipe">The found recipe, or null if not found</param>
    /// <returns>True if a recipe was found, false otherwise</returns>
    public bool TryGetRecipe(ItemType type, out Recipe recipe) => recipes.TryGetValue(type, out recipe);
}

/// <summary>
/// Represents a crafting recipe that defines how to create an item.
/// Contains the output item and the list of required ingredients.
/// </summary>
public class Recipe {
    /// <summary>The item type that this recipe produces</summary>
    public ItemType output;
    /// <summary>Array of ingredients required to craft this item</summary>
    public Ingredient[] ingredients;

    /// <summary>
    /// Creates a new recipe with the specified output and ingredients.
    /// </summary>
    /// <param name="output">The item type this recipe will produce</param>
    /// <param name="ingredients">Array of ingredients needed for this recipe</param>
    public Recipe(ItemType output, Ingredient[] ingredients) {
        this.output = output;
        this.ingredients = ingredients;
    }
}

/// <summary>
/// Manages player inventory by tracking the quantity of each item type.
/// Provides methods to add items and check current stock levels.
/// </summary>
public class Inventory {
    /// <summary>Dictionary storing item quantities indexed by item type</summary>
    readonly Dictionary<ItemType, int> stock = new();

    /// <summary>
    /// Adds a specified quantity of an item to the inventory.
    /// If the item doesn't exist in inventory, it will be initialized to 0 before adding.
    /// </summary>
    /// <param name="type">The type of item to add</param>
    /// <param name="count">The quantity to add to the inventory</param>
    public void Add(ItemType type, int count) {
        stock.TryAdd(type, 0);  // Initialize to 0 if item doesn't exist
        stock[type] += count;   // Add the specified count
    }
    
    /// <summary>
    /// Gets the current quantity of a specific item type in the inventory.
    /// </summary>
    /// <param name="type">The item type to check</param>
    /// <returns>The current quantity of the item, or 0 if the item is not in inventory</returns>
    public int GetCount(ItemType type) => stock.GetValueOrDefault(type, 0);
}