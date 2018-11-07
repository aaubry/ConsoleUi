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
using System.Threading.Tasks;

namespace ConsoleUi
{
    public class DynamicMenu : Menu
	{
        private readonly Func<IMenuContext, IEnumerable<IMenuItem>> getItems;

        public DynamicMenu(string title, Func<IMenuContext, IEnumerable<IMenuItem>> getItems)
            : base(title)
        {
            this.getItems = getItems;
        }

        public DynamicMenu(string title, Func<IMenuContext, Task<IEnumerable<IMenuItem>>> getItems)
            : this(title, ctx => getItems(ctx).Result)
        {
        }

        public DynamicMenu(string title, bool executeIfSingleItem, Func<IMenuContext, IEnumerable<IMenuItem>> getItems)
            : base(title, executeIfSingleItem)
        {
            this.getItems = getItems;
        }

        public DynamicMenu(string title, bool executeIfSingleItem, Func<IMenuContext, Task<IEnumerable<IMenuItem>>> getItems)
            : this(title, executeIfSingleItem, ctx => getItems(ctx).Result)
        {
        }

        public override void Enter(IMenuContext context)
        {
            Items.Clear();
            foreach (var item in getItems(context))
            {
                Items.Add(item);
            }
        }
    }
}