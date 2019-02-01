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

using ConsoleUi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cons = System.Console;

namespace ConsoleUi.Console
{
    public class ConsoleMenuRunner : IMenuRunner, IMenuUserInterface
    {
        public Task Run(IMenu menu)
        {
            return Run(menu, new[] { menu });
        }

        protected virtual async Task Run(IMenu menu, IEnumerable<IMenu> path)
        {
            if (menu == null)
            {
                throw new ArgumentNullException("menu");
            }

            var pageNumber = 0;

            await menu.Enter(new Context(this, path));

            using (var pages = new PaginatedAsyncSequence<IMenuItem>(menu.Items))
            {
                do
                {
                    options = CreateOptions();

                    var progressCancellation = new CancellationTokenSource();
                    var loaderCancellation = new CancellationTokenSource();
                    var progressTask = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(100, progressCancellation.Token);
                            if (!progressCancellation.Token.IsCancellationRequested)
                            {
                                using (var progress = ((IMenuUserInterface)this).StartProgress("Loading... Press [Esc] to cancel"))
                                {
                                    progress.SetIndeterminate();
                                    var result = await ((IMenuUserInterface)this).Select(null, r => (r.Type == PromptType.Cancel, r.Type), progressCancellation.Token);
                                    if (result == PromptType.Cancel)
                                    {
                                        loaderCancellation.Cancel();
                                    }
                                }
                            }
                        }
                        catch (TaskCanceledException) { }
                    });

                    var page = await pages.GetPage(pageNumber, options.Count, loaderCancellation.Token);
                    progressCancellation.Cancel();
                    await progressTask;

                    if (menu.ExecuteIfSingleItem && page.IsFirstPage && page.Count == 1)
                    {
                        await ExecuteItem(page[0], new Context(this, path));
                        break;
                    }

                    Render(menu, page, null, path);

                    var (choice, item) = await Prompt(page);

                    if (choice == MenuChoice.Cancel)
                    {
                        if (await menu.CanExit(new Context(this, path)))
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (choice == MenuChoice.PreviousPage)
                    {
                        --pageNumber;
                        continue;
                    }

                    if (choice == MenuChoice.NextPage)
                    {
                        ++pageNumber;
                        continue;
                    }

                    if (choice == MenuChoice.Item)
                    {
                        Render(menu, page, item, path);

                        Cons.WriteLine();
                        Cons.WriteLine("Running...");
                        Cons.WriteLine();

                        await ExecuteItem(item, new Context(this, path));
                    }
                }
                while (!menu.ShouldExit);
            }
        }

        public enum MenuChoice
        {
            None,
            Cancel,
            Item,
            PreviousPage,
            NextPage,
        };

        private Task<(MenuChoice choice, IMenuItem item)> Prompt(Page<IMenuItem> page)
        {
            Cons.WriteLine();

            return ((IMenuUserInterface)this).Select<(MenuChoice choice, IMenuItem item)>("Pick an option or press [Esc] to exit: ", selection =>
            {
                switch (selection.Type)
                {
                    case PromptType.Cancel:
                        return (true, (MenuChoice.Cancel, null));

                    case PromptType.Character when selection.SelectedCharacter == '<':
                        return (!page.IsFirstPage, (MenuChoice.PreviousPage, null));

                    case PromptType.Character when selection.SelectedCharacter == '>':
                        return (!page.IsLastPage, (MenuChoice.NextPage, null));

                    case PromptType.Character:
                        var optionIndex = options.IndexOf(char.ToUpperInvariant(selection.SelectedCharacter));
                        if (optionIndex >= 0 && optionIndex < page.Count)
                        {
                            return (true, (MenuChoice.Item, page[optionIndex]));
                        }
                        break;
                }

                return (false, (MenuChoice.None, null));
            });
        }

        private async Task ExecuteItem(IMenuItem choice, Context nestedContext)
        {
            if (choice == null)
            {
                throw new ArgumentNullException("choice");
            }

            try
            {
                await choice.Execute(nestedContext);
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

        protected virtual void Render(IMenu menu, Page<IMenuItem> page, IMenuItem selectedItem, IEnumerable<IMenu> path)
        {
            _yesToAll = false;
            Cons.Clear();

            using (Color.Set(ConsoleColor.Cyan))
            {
                Cons.WriteLine();

                var titleBar = "  " + string.Join(" > ", path.Select(m => m.Title));
                var maxTitleBarLength = Cons.BufferWidth - 4;
                if (titleBar.Length > maxTitleBarLength)
                {
                    titleBar = "  ... " + titleBar.Substring(titleBar.Length - maxTitleBarLength - 4);
                }

                Cons.WriteLine(titleBar);

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

            if (page.Count > 0)
            {
                int index = 0;

                maxLength = Math.Max(maxLength, page.Max(i => i.Title.Length));
                foreach (var item in page)
                {
                    var isMenu = item is IMenu;

                    Cons.Write(" ");
                    using (Color.Set(ConsoleColor.Yellow, ReferenceEquals(item, selectedItem) ? ConsoleColor.DarkGray : ConsoleColor.Black))
                    {
                        Cons.Write(" {0}", options[index++]);
                    }

                    using (Color.Set(item.IsHighlighted ? ConsoleColor.White : ConsoleColor.Gray, ReferenceEquals(item, selectedItem) ? ConsoleColor.DarkGray : ConsoleColor.Black))
                    {
                        Cons.Write(isMenu ? "» " : "  ");
                        var finalLength = Cons.CursorLeft + maxLength + 1;
                        RenderMenuItemTitle(item);
                        if (finalLength > Cons.CursorLeft)
                        {
                            Cons.Write(new string(' ', finalLength - Cons.CursorLeft));
                        }

                        Cons.WriteLine();
                    }
                }
            }

            using (Color.Set(ConsoleColor.DarkGray))
            {
                if (!page.IsFirstPage)
                {
                    Cons.WriteLine("  <  {0}", "Previous".PadRight(maxLength + 1));
                }

                if (!page.IsLastPage)
                {
                    Cons.WriteLine("  >  {0}", "Next".PadRight(maxLength + 1));
                }
            }
        }

        protected virtual void RenderMenuItemTitle(IMenuItem item)
        {
            Cons.Write(item.Title);
        }

        async Task<bool> IMenuUserInterface.Confirm(bool force, string message, params object[] args)
        {
            using (Color.Set(ConsoleColor.Yellow))
            {
                if (force)
                {
                    Cons.WriteLine();
                    var formattedMessage = string.Format("{0} (y/n) ? ", string.Format(message, args).TrimEnd(' ', '?'));

                    var result = await ((IMenuUserInterface)this).Select(formattedMessage, choice =>
                    {
                        var lowerChoice = char.ToLowerInvariant(choice.SelectedCharacter);
                        return ("yn".Contains(lowerChoice), lowerChoice);
                    });

                    return result != 'n';
                }
                else
                {
                    if (_yesToAll)
                    {
                        return true;
                    }

                    Cons.WriteLine();
                    var formattedMessage = string.Format("{0} (y/n/a) ? ", string.Format(message, args).TrimEnd(' ', '?'));

                    var result = await ((IMenuUserInterface)this).Select(formattedMessage, choice =>
                    {
                        var lowerChoice = char.ToLowerInvariant(choice.SelectedCharacter);
                        return ("yna".Contains(lowerChoice), lowerChoice);
                    });

                    _yesToAll = result == 'a';
                    return result != 'n';
                }
            }
        }

        Task<bool> IMenuUserInterface.Confirm(string message, params object[] args)
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

        Task<T> IMenuUserInterface.Select<T>(string message, Func<SelectionResult, (bool isValid, T selection)> choiceValidator, CancellationToken cancellationToken)
        {
            if (message != null)
            {
                Cons.Write(message);
            }
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (Cons.KeyAvailable)
                    {
                        var key = Cons.ReadKey(true);

                        SelectionResult choice;
                        switch (key.Key)
                        {
                            case ConsoleKey.Escape:
                                choice = new SelectionResult(PromptType.Cancel);
                                break;

                            case ConsoleKey.Enter:
                                choice = new SelectionResult(PromptType.Accept);
                                break;

                            default:
                                choice = new SelectionResult(key.KeyChar);
                                break;
                        }

                        var (isValid, selection) = choiceValidator(choice);
                        if (isValid)
                        {
                            if (choice.Type == PromptType.Character && message != null)
                            {
                                Cons.WriteLine(choice.SelectedCharacter);
                                Cons.WriteLine();
                            }
                            return selection;
                        }
                    }
                    else
                    {
                        try
                        {
                            await Task.Delay(100, cancellationToken);
                        }
                        catch (TaskCanceledException) { }
                    }
                }

                return default;
            });
        }

        IProgressBar IMenuUserInterface.StartProgress(string status)
        {
            var progress = new ProgressBar(status);
            progress.SetProgress(0);
            return progress;
        }

        public sealed class ProgressBar : IProgressBar
        {
            private string status;
            private CancellationTokenSource animationCancellation;
            private Task animationTask;

            public ProgressBar(string status)
            {
                this.status = status;
                Cons.CursorVisible = false;
            }

            public void Clear(string status)
            {
                this.status = status;
                Clear();
            }

            public void Clear()
            {
                StopAnimation();
                Render(status, 0, 0);
            }

            public void SetProgress(int progressPercentage, string status)
            {
                this.status = status;
                SetProgress(progressPercentage);
            }

            public void SetProgress(int progressPercentage)
            {
                StopAnimation();
                Render($"{status} {progressPercentage} %", 0, progressPercentage);
            }

            private void StopAnimation()
            {
                if (animationCancellation != null)
                {
                    animationCancellation.Cancel();
                    animationTask.Wait();

                    animationCancellation = null;
                    animationTask = null;
                }
            }

            private async Task AnimateIndeterminate(CancellationToken cancellationToken)
            {
                int indeterminateCycle = 0;
                int indeterminateIncrement = 1;

                while (!cancellationToken.IsCancellationRequested)
                {
                    Render(status, indeterminateCycle, indeterminateCycle + 10);
                    indeterminateCycle += indeterminateIncrement;

                    if (indeterminateCycle == 0 || indeterminateCycle == 90)
                    {
                        indeterminateIncrement = -indeterminateIncrement;
                    }

                    await Task.Delay(10);
                }
            }

            public void SetIndeterminate(string status)
            {
                this.status = status;
                SetIndeterminate();
            }

            public void SetIndeterminate()
            {
                if (animationCancellation == null)
                {
                    animationCancellation = new CancellationTokenSource();
                    animationTask = Task.Run(() => AnimateIndeterminate(animationCancellation.Token));
                }
            }

            private void Render(string label, int startHighlightPercentage, int endHighlightPercentage)
            {
                Cons.CursorLeft = 1;

                var text = ("   " + label).PadRight(Cons.BufferWidth - 2);
                var leftSplitIndex = (int)Math.Round(startHighlightPercentage * text.Length / 100.0);
                var rightSplitIndex = (int)Math.Round(endHighlightPercentage * text.Length / 100.0);

                using (Color.Set(ConsoleColor.Black, ConsoleColor.Gray))
                {
                    Cons.Write(text.Substring(0, leftSplitIndex));
                }

                using (Color.Set(ConsoleColor.Black, ConsoleColor.White))
                {
                    Cons.Write(text.Substring(leftSplitIndex, rightSplitIndex - leftSplitIndex));
                }

                using (Color.Set(ConsoleColor.Black, ConsoleColor.Gray))
                {
                    Cons.Write(text.Substring(rightSplitIndex));
                }
            }

            public void Dispose()
            {
                StopAnimation();
                Cons.CursorVisible = true;
                Cons.WriteLine();
            }
        }

        private class Context : IMenuContext
        {
            private readonly ConsoleMenuRunner menuRunner;
            private readonly IEnumerable<IMenu> menus;

            public Context(ConsoleMenuRunner menuRunner, IEnumerable<IMenu> menus)
            {
                this.menuRunner = menuRunner;
                this.menus = menus;
            }

            private bool? shouldPause;

            public bool ShouldPause => shouldPause ?? true;

            async Task IMenuContext.Run(IMenu menu)
            {
                await menuRunner.Run(menu, menus.Concat(new[] { menu }));
                if (!shouldPause.HasValue)
                {
                    shouldPause = false;
                }
            }

            void IMenuContext.SuppressPause()
            {
                shouldPause = false;
            }

            IMenuUserInterface IMenuContext.UserInterface { get { return menuRunner; } }
        }

        private List<char> options;

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