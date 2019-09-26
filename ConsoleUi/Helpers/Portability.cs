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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD2_0
namespace System
{
    public interface IAsyncDisposable
    {
        //
        // Summary:
        //     Performs application-defined tasks associated with freeing, releasing, or resetting
        //     unmanaged resources asynchronously.
        //
        // Returns:
        //     A task that represents the asynchronous dispose operation.
        ValueTask DisposeAsync();
    }
}

namespace ConsoleUi
{
    internal static class EnumerableExtensions
    {
        public static IAsyncEnumerator<T> GetAsyncEnumerator<T>(this IAsyncEnumerable<T> enumerable) => enumerable.GetEnumerator();

        public static ValueTask<bool> MoveNextAsync<T>(this IAsyncEnumerator<T> enumerator) => new ValueTask<bool>(enumerator.MoveNext(CancellationToken.None));

        public static ValueTask DisposeAsync<T>(this IAsyncEnumerator<T> enumerator)
        {
            enumerator.Dispose();
            return default;
        }
    }
}
#else
namespace ConsoleUi
{
    internal static class EnumerableExtensions
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> sequence)
        {
            foreach (var item in sequence)
            {
                yield return item;
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
#endif