using System;
using UnityEngine;

namespace LegendarySkins;

[CreateAssetMenu(fileName = "High Resolution Bake Pack", menuName = "ScriptableObjects/Legendary Heros/High Resolution Bake Pack")]
public class LegendaryHighResBaker : ScriptableObject
{
	[Flags]
	public enum BakeOptions
	{
		CardBack = 1,
		HeroFrame = 2,
		CollectionManager = 4,
		HeroTray = 8
	}

	public CardDef CardDefinition;

	public CardBack CardBack;

	public BakeOptions Options = (BakeOptions)(-1);

	public int Resolution = 8192;
}
