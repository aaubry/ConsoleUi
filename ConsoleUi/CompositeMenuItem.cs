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
using System.Collections.Generic;
using System.Linq;

namespace ConsoleUi
{
	public class CompositeMenuItem : MenuItem
	{
		public ICollection<IMenuItem> Items { get; private set; }

		public CompositeMenuItem(string title, params IMenuItem[] items)
			: this(title, (IEnumerable<IMenuItem>)items)
		{
		}

		public CompositeMenuItem(params IMenuItem[] items)
			: this((IEnumerable<IMenuItem>)items)
		{
		}

		public CompositeMenuItem(string title, IEnumerable<IMenuItem> items)
			: base(title)
		{
			AssignItems(items);
		}

		public CompositeMenuItem(IEnumerable<IMenuItem> items)
		{
			AssignItems(items);
		}

		private void AssignItems(IEnumerable<IMenuItem> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			Items = items.ToList();
		}

		protected CompositeMenuItem(string title)
			: base(title)
		{
			Items = new List<IMenuItem>();
		}

		public override void Execute(IMenuContext context)
		{
			foreach (var item in Items)
			{
				item.Execute(context);
			}
		}
	}
}