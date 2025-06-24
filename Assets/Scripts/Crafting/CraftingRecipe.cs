using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Результат крафта")]
    [Tooltip("Предмет, который создаётся по этому рецепту")]
    public InventoryItem resultItem;

    [Tooltip("Сколько единиц результат даст в инвентарь")]
    public int resultAmount = 1;

    [Header("Ингредиенты")]
    [Tooltip("Список необходимых предметов и их кол-ва")]
    public List<Ingredient> ingredients = new List<Ingredient>();

    [System.Serializable]
    public class Ingredient
    {
        [Tooltip("Какой предмет нужен")]
        public InventoryItem item;

        [Tooltip("Сколько штук нужно")]
        public int amount = 1;
    }
}
