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
	public interface IMenuUserInterface
	{
		bool Confirm(string message, params object[] args);
		bool Confirm(bool force, string message, params object[] args);

		void Debug(string message, params object[] args);
		void Info(string message, params object[] args);
		void Warning(string message, params object[] args);
		void Error(string message, params object[] args);

        string Prompt(string message);
	}
}