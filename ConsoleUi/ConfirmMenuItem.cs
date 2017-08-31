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

namespace ConsoleUi
{
    public class ConfirmMenuItem : MenuItem
	{
		private readonly string _message;

		public ConfirmMenuItem(string message = "Are you sure?")
		{
			_message = message;
		}

		public ConfirmMenuItem(string title, string message = "Are you sure?")
			: base(title)
		{
			_message = message;
		}

		public override void Execute(IMenuContext context)
		{
			if (!context.UserInterface.Confirm(_message))
				throw new CancelException();
		}
	}
}