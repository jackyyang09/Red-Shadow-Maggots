using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lazlo;

[CreateAssetMenu(menuName = nameof(GlobalIndex), fileName = nameof(GlobalIndex))]
public sealed class GlobalIndex : BaseIndex
{
	// The static instance is what allows us to get the index from anywhere in the code:
	private static GlobalIndex _instance;
	public static GlobalIndex Instance => GetOrLoad(ref _instance);

	// Set up your references below! 
	// You only need to assign references once with this pattern.
	public Sprite[] classIcons = null;
	public Sprite GetClassIcon(CharacterClass characterClass) => classIcons[(int)characterClass];
}