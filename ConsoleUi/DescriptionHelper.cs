// This file is part of ConsoleUi.
// 
// ConsoleUi is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ConsoleUi is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ConsoleUi.  If not, see <http://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ConsoleUi
{
	public static class DescriptionHelper
	{
		public static string Get(MemberInfo target)
		{
			var description = (DescriptionAttribute)target.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();

			return description != null
				? description.Description
				: SplitCamelCaseWords(Regex.Replace(target.Name, "`.*", ""));
		}

		public static string SplitCamelCaseWords(string name)
		{
			return Regex.Replace(name, @"(?<!^)[A-Z]", m => " " + m.Value.ToLowerInvariant());
		}
	}
}