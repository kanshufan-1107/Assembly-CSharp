using System;

namespace Hearthstone.UI.Scripting;

[Flags]
public enum ScriptFeatureFlags
{
	Conditionals = 1,
	Identifiers = 2,
	Keywords = 4,
	Arithmetic = 8,
	Relational = 0x10,
	Constants = 0x20,
	Events = 0x40,
	Tuples = 0x80,
	Methods = 0x100,
	All = 0x1FF
}
