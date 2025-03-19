using System;
using System.Linq.Expressions;

public abstract class BTB_RumbleDome_MissionEntity : GenericDungeonMissionEntity
{
	public static class MemberInfoGetting
	{
		public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
		{
			return ((MemberExpression)memberExpression.Body).Member.Name;
		}
	}
}
