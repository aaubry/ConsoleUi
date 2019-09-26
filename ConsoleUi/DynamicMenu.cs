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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUi
{
    public abstract class DynamicMenu : MenuSkeleton
    {
        public DynamicMenu(string title) : base(title)
        {
        }

#if NETSTANDARD2_0
        protected override IAsyncEnumerable<IMenuItem> Items => AsyncEnumerable.CreateEnumerable(() =>
        {
            IEnumerator<IMenuItem> enumerator = null;
            return AsyncEnumerable.CreateEnumerator(
                async ct =>
                {
                    if (enumerator == null)
                    {
                        var items = await LoadItems(ct);
                        enumerator = items.GetEnumerator();
                    }
                    return enumerator.MoveNext();
                },
                () => enumerator?.Current,
                () => enumerator?.Dispose()
            );
        });
#else
        protected override IAsyncEnumerable<IMenuItem> Items
        {
            get
            {
                async IAsyncEnumerable<IMenuItem> GetItems()
                {
                    var items = await LoadItems(CancellationToken.None);
                    foreach (var item in items)
                    {
                        yield return item;
                    }
                }

                return GetItems();
            }
        }
#endif

        protected abstract Task<IEnumerable<IMenuItem>> LoadItems(CancellationToken cancellationToken);
    }
}