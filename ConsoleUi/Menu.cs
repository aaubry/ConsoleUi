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
    public class Menu : MenuSkeleton
    {
        private readonly IAsyncEnumerable<IMenuItem> items;

        protected override IAsyncEnumerable<IMenuItem> Items => items;

        public Menu(string title, params IMenuItem[] items)
			: this(title, items.ToAsyncEnumerable())
		{
		}

        public Menu(string title, IEnumerable<IMenuItem> items)
            : this(title, items.ToAsyncEnumerable())
        {
        }

        public Menu(string title, IAsyncEnumerable<IMenuItem> items)
            : base(title)
		{
			this.items = items ?? throw new ArgumentNullException("items");
		}
	}
}