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
using System.Threading.Tasks;

namespace ConsoleUi
{
	public sealed class ActionMenuItem : MenuItem
	{
		private readonly Action<IMenuContext> _execute;

		public ActionMenuItem(Action<IMenuContext> execute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException("execute");
			}

			_execute = execute;
		}

		public ActionMenuItem(string title, Action<IMenuContext> execute)
			: base(title)
		{
			if (execute == null)
			{
				throw new ArgumentNullException("execute");
			}

			_execute = execute;
		}

        public ActionMenuItem(Func<IMenuContext, Task> execute)
            : this(ctx => execute(ctx).Wait())
        {
        }

        public ActionMenuItem(string title, Func<IMenuContext, Task> execute)
            : this(title, ctx => execute(ctx).Wait())
        {
        }

        public override void Execute(IMenuContext context)
		{
			_execute(context);
		}
	}
}