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
    public abstract class MenuSkeleton : IMenu
    {
        public string Description { get; set; }
        public bool IsHighlighted { get; set; }
        public bool ExecuteIfSingleItem { get; set; }
        public bool ShouldExit { get; set; }
        public string Title { get; protected set; }

        public MenuSkeleton(string title)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
        }

        protected abstract IAsyncEnumerable<IMenuItem> Items { get; }
        protected virtual Task<bool> CanExit(IMenuContext context) => Task.FromResult(true);
        protected virtual Task Enter(IMenuContext context) => Task.CompletedTask;
        protected virtual Task Execute(IMenuContext context) => context.Run(this);

        IAsyncEnumerable<IMenuItem> IMenu.Items => Items;
        Task<bool> IMenu.CanExit(IMenuContext context) => CanExit(context);
        Task IMenu.Enter(IMenuContext context) => Enter(context);
        Task IMenuItem.Execute(IMenuContext context) => Execute(context);
    }
}