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
using Cons = System.Console;

namespace ConsoleUi.Console
{
    public sealed class ConsoleMenuRunner : IMenuRunner, IMenuUserInterface
    {
        public void Run(IMenu menu)
        {
            if (menu == null)
            {
                throw new ArgumentNullException("menu");
            }

            ExecuteItem(menu, new Context(this, 0));
        }

        private void Run(IMenu menu, int depth)
        {
            if (menu == null)
            {
                throw new ArgumentNullException("menu");
            }

            var pageNumber = 0;

            do
            {
                var nestedContext = new Context(this, depth);
                menu.Enter(nestedContext);

                _options = CreateOptions();
                var totalPages = (int)Math.Ceiling((double)menu.Items.Count / _options.Count);

                Render(menu, depth, null, pageNumber, totalPages);

                var choice = Prompt(menu, pageNumber, totalPages);
                if (choice.Item1.Key == ConsoleKey.Escape)
                {
                    break;
                }

                if (choice.Item1.KeyChar == '<')
                {
                    --pageNumber;
                    continue;
                }

                if (choice.Item1.KeyChar == '>')
                {
                    ++pageNumber;
                    continue;
                }

                Render(menu, depth, choice.Item2, pageNumber, totalPages);

                Cons.WriteLine();
                Cons.WriteLine("Running...");
                Cons.WriteLine();

                ExecuteItem(choice.Item2, nestedContext);
            }
            while (!menu.ShouldExit);
        }

        private Tuple<ConsoleKeyInfo, IMenuItem> Prompt(IMenu menu, int pageNumber, int totalPages)
        {
            if (menu == null)
            {
                throw new ArgumentNullException("menu");
            }

            Cons.WriteLine();
            Cons.Write("Pick an option or press [Esc] to exit: ");

            return Prompt((ConsoleKeyInfo key, out bool isValid) =>
            {
                var optionIndex = _options.IndexOf(char.ToUpperInvariant(key.KeyChar));
                if (optionIndex >= 0)
                {
                    var itemIndex = optionIndex + pageNumber * _options.Count;
                    if (itemIndex >= 0 && itemIndex < menu.Items.Count)
                    {
                        isValid = true;
                        return Tuple.Create(key, menu.Items[itemIndex]);
                    }
                }

                isValid = key.Key == ConsoleKey.Escape
                    || (key.KeyChar == '<' && pageNumber > 0)
                    || (key.KeyChar == '>' && pageNumber < totalPages - 1);

                return Tuple.Create(key, (IMenuItem)null);
            });
        }

        private delegate T ValidateKeyDelegate<T>(ConsoleKeyInfo key, out bool isValid);

        private T Prompt<T>(ValidateKeyDelegate<T> validateKey)
        {
            while (true)
            {
                var key = Cons.ReadKey(true);
                bool isValid;
                var result = validateKey(key, out isValid);
                if (isValid)
                {
                    Cons.WriteLine(key.KeyChar);
                    Cons.WriteLine();
                    return result;
                }
            }
        }

        private void ExecuteItem(IMenuItem choice, Context nestedContext)
        {
            if (choice == null)
            {
                throw new ArgumentNullException("choice");
            }

            try
            {
                choice.Execute(nestedContext);
            }
            catch (CancelException err)
            {
                using (Color.Set(ConsoleColor.Red))
                {
                    Cons.WriteLine(err.Message);
                }
            }
            catch (Exception err)
            {
                using (Color.Set(ConsoleColor.Red))
                {
                    Cons.WriteLine(err);
                }
            }

            if (nestedContext.ShouldPause)
            {
                Cons.WriteLine();
                Cons.Write("Press [Enter] to continue... ");

                while (true)
                {
                    var key = Cons.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Enter:
                        case ConsoleKey.Escape:
                            return;
                    }
                }
            }
        }

        private bool _yesToAll;

        private void Render(IMenu menu, int depth, IMenuItem selectedItem, int pageNumber, int totalPages)
        {
            if (menu == null)
            {
                throw new ArgumentNullException("menu");
            }

            _yesToAll = false;
            Cons.Clear();

            using (Color.Set(ConsoleColor.Cyan))
            {
                Cons.WriteLine();
                Cons.WriteLine("  {0} {1}", new string('>', depth), menu.Title);
                if (menu.Description != null)
                {
                    using (Color.Set(ConsoleColor.DarkGray))
                    {
                        foreach (var line in menu.Description.Split('\n'))
                        {
                            Cons.WriteLine("  {0}", line.TrimEnd('\r'));
                        }
                    }
                }
                Cons.WriteLine();
            }

            var maxLength = new[] { "Previous", "Next" }.Max(n => n.Length);

            if (menu.Items.Count > 0)
            {
                int index = 0;
                maxLength = Math.Max(maxLength, menu.Items.Max(i => i.Title.Length));
                foreach (var item in menu.Items.Skip(pageNumber * _options.Count).Take(_options.Count))
                {
                    var isMenu = item is IMenu;

                    Cons.Write(" ");
                    using (Color.Set(ConsoleColor.Yellow, ReferenceEquals(item, selectedItem) ? ConsoleColor.DarkGray : ConsoleColor.Black))
                    {
                        Cons.Write(" {0}", _options[index++]);
                    }

                    using (Color.Set(item.IsHighlighted ? ConsoleColor.White : ConsoleColor.Gray, ReferenceEquals(item, selectedItem) ? ConsoleColor.DarkGray : ConsoleColor.Black))
                    {
                        Cons.WriteLine("{1} {0}", item.Title.PadRight(maxLength + 1), isMenu ? "»" : " ");
                    }
                }
            }

            using (Color.Set(ConsoleColor.DarkGray))
            {
                if (pageNumber > 0)
                {
                    Cons.WriteLine("  <  {0}", "Previous".PadRight(maxLength + 1));
                }

                if (pageNumber < totalPages - 1)
                {
                    Cons.WriteLine("  >  {0}", "Next".PadRight(maxLength + 1));
                }
            }
        }

        bool IMenuUserInterface.Confirm(bool force, string message, params object[] args)
        {
            using (Color.Set(ConsoleColor.Yellow))
            {
                if (force)
                {
                    Cons.WriteLine();
                    Cons.Write("{0} (y/n) ? ", string.Format(message, args).TrimEnd(' ', '?'));

                    return Prompt((ConsoleKeyInfo key, out bool isValid) =>
                    {
                        var input = char.ToLowerInvariant(key.KeyChar);
                        isValid = "yn".Contains(input);

                        return input != 'n';
                    });
                }
                else
                {
                    if (_yesToAll)
                    {
                        return true;
                    }

                    Cons.WriteLine();
                    Cons.Write("{0} (y/n/a) ? ", string.Format(message, args).TrimEnd(' ', '?'));

                    return Prompt((ConsoleKeyInfo key, out bool isValid) =>
                    {
                        var input = char.ToLowerInvariant(key.KeyChar);
                        isValid = "yna".Contains(input);

                        _yesToAll = key.KeyChar == 'a';
                        return input != 'n';
                    });
                }
            }
        }

        bool IMenuUserInterface.Confirm(string message, params object[] args)
        {
            return ((IMenuUserInterface)this).Confirm(false, message, args);
        }

        void IMenuUserInterface.Debug(string message, params object[] args)
        {
            using (Color.Set(ConsoleColor.Gray))
            {
                Cons.WriteLine(string.Format(message, args));
            }
        }

        void IMenuUserInterface.Info(string message, params object[] args)
        {
            using (Color.Set(ConsoleColor.Green))
            {
                Cons.WriteLine(string.Format(message, args));
            }
        }

        void IMenuUserInterface.Warning(string message, params object[] args)
        {
            using (Color.Set(ConsoleColor.Magenta))
            {
                Cons.WriteLine(string.Format(message, args));
            }
        }

        void IMenuUserInterface.Error(string message, params object[] args)
        {
            using (Color.Set(ConsoleColor.Red))
            {
                Cons.WriteLine(string.Format(message, args));
            }
        }

        string IMenuUserInterface.Prompt(string message)
        {
            using (Color.Set(ConsoleColor.Yellow))
            {
                Cons.Write(message);
            }
            using (Color.Set(ConsoleColor.White))
            {
                return Cons.ReadLine();
            }
        }

        private class Context : IMenuContext
        {
            private readonly int _depth;
            private readonly ConsoleMenuRunner _menuRunner;

            public Context(ConsoleMenuRunner menuRunner, int depth)
            {
                _menuRunner = menuRunner;
                _depth = depth;
            }

            private bool? shouldPause;

            public bool ShouldPause => shouldPause ?? true;

            void IMenuContext.Run(IMenu menu)
            {
                _menuRunner.Run(menu, _depth + 1);
                if (!shouldPause.HasValue)
                {
                    shouldPause = false;
                }
            }

            void IMenuContext.SuppressPause()
            {
                shouldPause = false;
            }

            IMenuUserInterface IMenuContext.UserInterface { get { return _menuRunner; } }
        }

        private List<char> _options;

        private static List<char> CreateOptions()
        {
            var numbers = Enumerable.Range(1, 10).Select(n => (char)('0' + (n % 10)));
            var letters = Enumerable.Range(0, 26).Select(n => (char)('A' + n));

            return numbers
                .Concat(letters)
                .Take(Math.Max(3, Cons.WindowHeight - 8))
                .ToList();
        }
    }
}