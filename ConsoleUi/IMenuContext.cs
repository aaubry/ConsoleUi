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
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUi
{
    public interface IMenuContext
    {
        Task Run(IMenu menu);
        IMenuUserInterface UserInterface { get; }

        /// <summary>
        /// Suppresses the pause after executing a menu item.
        /// </summary>
        void SuppressPause();

        /// <summary>
        /// Indicates that the current menu has become invalid as a result of executing the current item, and needs to be reloaded.
        /// </summary>
        void InvalidateMenu();
    }

    public static class MenuContextExtensions
    {
        public static Task<T> RunUntilCancelled<T>(this IMenuContext context, Func<CancellationToken, Task<T>> backgroundTaskFactory)
        {
            return context.UserInterface.RunUntilCancelled(backgroundTaskFactory);
        }

        public static async Task<T> RunUntilCancelled<T>(this IMenuUserInterface userInterface, Func<CancellationToken, Task<T>> backgroundTaskFactory)
        {
            var cancellation = new CancellationTokenSource();

            var backgroundTask = Task.Run(async () =>
            {
                try
                {
                    return await backgroundTaskFactory(cancellation.Token);
                }
                catch (TaskCanceledException)
                {
                    return default;
                }
            });

            var cancellationTask = userInterface.Select(null, result => (result.Type == PromptType.Cancel, result), cancellation.Token);

            await Task.WhenAny(backgroundTask, cancellationTask);
            if (!cancellation.IsCancellationRequested)
            {
                cancellation.Cancel();
            }

            await Task.WhenAll(backgroundTask, cancellationTask);
            return backgroundTask.Result;
        }

        public static Task RunUntilCancelled(this IMenuContext context, Func<CancellationToken, Task> backgroundTaskFactory)
        {
            return context.RunUntilCancelled(async ct =>
            {
                await backgroundTaskFactory(ct);
                return true;
            });
        }
    }
}