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
using Cons = System.Console;

namespace ConsoleUi.Console
{
    public static class Color
    {
        public static IDisposable Set(ConsoleColor foreground, ConsoleColor background = ConsoleColor.Black)
        {
            return new ConsoleColorContext(foreground, background);
        }

        private class ConsoleColorContext : IDisposable
        {
            private readonly ConsoleColor _originalForeground;
            private readonly ConsoleColor _originalBackground;

            public ConsoleColorContext(ConsoleColor foreground, ConsoleColor background)
            {
                _originalForeground = Cons.ForegroundColor;
                Cons.ForegroundColor = foreground;

                _originalBackground = Cons.BackgroundColor;
                Cons.BackgroundColor = background;
            }

            void IDisposable.Dispose()
            {
                Cons.ForegroundColor = _originalForeground;
                Cons.BackgroundColor = _originalBackground;
            }
        }
    }
}
