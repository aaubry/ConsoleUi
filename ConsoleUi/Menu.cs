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
	public class Menu : IMenu
	{
		public string Title { get; private set; }
		public string Description { get; set; }
        public bool IsHighlighted { get; set; }
        public bool ShouldExit { get; set; }
        public IList<IMenuItem> Items { get; private set; }
		private bool _executeIfSingleItem;

		public Menu(string title, params IMenuItem[] items)
			: this(title, true, items)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		/// <param name="executeIfSingleItem">
		/// If true and the menu contains a single item, that item will be 
		/// executed instead of rendering the menu.
		/// </param>
		/// <param name="items"></param>
		public Menu(string title, bool executeIfSingleItem, params IMenuItem[] items)
			: this(title, (IEnumerable<IMenuItem>)items)
		{
			_executeIfSingleItem = executeIfSingleItem;
		}

		public Menu(string title, IEnumerable<IMenuItem> items)
		{
			if (string.IsNullOrEmpty(title))
			{
				throw new ArgumentNullException("title");
			}

			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			Title = title;
			Items = items.ToList();
		}

        public virtual void Enter(IMenuContext context) { }

		public virtual void Execute(IMenuContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			if (Items.Count != 1 || !_executeIfSingleItem)
			{
				context.Run(this);
			}
			else
			{
				Items[0].Execute(context);
			}
		}
	}
}