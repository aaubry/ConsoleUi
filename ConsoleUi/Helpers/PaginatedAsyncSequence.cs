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
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUi.Helpers
{
    public class PaginatedAsyncSequence<T> : IAsyncDisposable
    {
        private readonly List<T> bufferedItems = new List<T>();
        private readonly IAsyncEnumerator<T> enumerator;
        private bool hasMoreItems = true;

        public PaginatedAsyncSequence(IAsyncEnumerable<T> sequence)
        {
            enumerator = sequence.GetAsyncEnumerator();
        }

        public async Task<Page<T>> GetPage(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            if (pageNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            var startIndex = pageNumber * pageSize;
            var endIndex = startIndex + pageSize;

            // Fill the buffer as needed (try to load one more items to see if we are at the end of the stream)
            while (bufferedItems.Count <= endIndex && hasMoreItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (await enumerator.MoveNextAsync())
                {
                    bufferedItems.Add(enumerator.Current);
                }
                else
                {
                    hasMoreItems = false;
                }
            }

            var items = new List<T>();
            for (int i = startIndex; i < endIndex && i < bufferedItems.Count; ++i)
            {
                items.Add(bufferedItems[i]);
            }

            return new Page<T>(
                items,
                isFirstPage: pageNumber == 0,
                isLastPage: endIndex >= bufferedItems.Count
            );
        }

        public ValueTask DisposeAsync()
        {
            return enumerator.DisposeAsync();
        }
    }
}