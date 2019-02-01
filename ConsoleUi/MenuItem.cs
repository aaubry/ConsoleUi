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

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleUi
{
	public abstract class MenuItem : IMenuItem
	{
		public string Title { get; private set; }
		public bool IsHighlighted { get; set; }

        public MenuItem()
		{
			Title = Regex.Replace(DescriptionHelper.Get(GetType()), " menu item$", "");
		}

		public MenuItem(string title)
		{
			if (string.IsNullOrEmpty(title))
			{
				throw new ArgumentNullException("title");
			}

			Title = title;
		}

		public abstract Task Execute(IMenuContext context);
	}
}