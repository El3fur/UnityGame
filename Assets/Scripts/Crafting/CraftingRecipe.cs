using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("��������� ������")]
    [Tooltip("�������, ������� �������� �� ����� �������")]
    public InventoryItem resultItem;

    [Tooltip("������� ������ ��������� ���� � ���������")]
    public int resultAmount = 1;

    [Header("�����������")]
    [Tooltip("������ ����������� ��������� � �� ���-��")]
    public List<Ingredient> ingredients = new List<Ingredient>();

    [System.Serializable]
    public class Ingredient
    {
        [Tooltip("����� ������� �����")]
        public InventoryItem item;

        [Tooltip("������� ���� �����")]
        public int amount = 1;
    }
}
