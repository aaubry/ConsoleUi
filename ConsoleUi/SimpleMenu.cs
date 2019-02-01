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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUi
{
    public abstract class SimpleMenu : DynamicMenu
    {
        protected SimpleMenu() : base(string.Empty)
        {
            Title = Regex.Replace(DescriptionHelper.Get(GetType()), " menu$", "");
        }

        protected override Task<IEnumerable<IMenuItem>> LoadItems(CancellationToken cancellationToken)
        {
            var items = new List<IMenuItem>();

            var myType = GetType();
            foreach (var menuItemMethod in myType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (menuItemMethod.DeclaringType == typeof(MenuSkeleton))
                    continue;

                if (menuItemMethod.DeclaringType == typeof(SimpleMenu))
                    continue;

                if (menuItemMethod.DeclaringType == typeof(object))
                    continue;

                var isVoid = menuItemMethod.ReturnType == typeof(void);
                var isTask = typeof(Task).IsAssignableFrom(menuItemMethod.ReturnType);
                var isSubMenu = typeof(IMenu).IsAssignableFrom(menuItemMethod.ReturnType);

                if (!isVoid && !isTask && !isSubMenu)
                    continue;

                var parameters = menuItemMethod.GetParameters();
                if (isVoid)
                {
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IMenuContext))
                    {
                        var invokeMethod = (Action<IMenuContext>)Delegate.CreateDelegate(typeof(Action<IMenuContext>), this, menuItemMethod);
                        items.Add(new ActionMenuItem(DescriptionHelper.Get(menuItemMethod), invokeMethod));
                    }
                    else if (parameters.Length == 0)
                    {
                        var invokeMethod = (Action)Delegate.CreateDelegate(typeof(Action), this, menuItemMethod);
                        items.Add(new ActionMenuItem(DescriptionHelper.Get(menuItemMethod), ctx => invokeMethod()));
                    }
                }
                else if (isSubMenu)
                {
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IMenuContext))
                    {
                        throw new NotSupportedException("Passing IMenuContext to a menu factory method is no longer supported");
                    }
                    else if (parameters.Length == 0)
                    {
                        var invokeMethod = (Func<IMenu>)Delegate.CreateDelegate(typeof(Func<IMenu>), this, menuItemMethod);
                        items.Add(invokeMethod());
                    }
                }
                else
                {
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IMenuContext))
                    {
                        var invokeMethod = (Func<IMenuContext, Task>)Delegate.CreateDelegate(typeof(Func<IMenuContext, Task>), this, menuItemMethod);
                        items.Add(new ActionMenuItem(DescriptionHelper.Get(menuItemMethod), invokeMethod));
                    }
                    else if (parameters.Length == 0)
                    {
                        var invokeMethod = (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), this, menuItemMethod);
                        items.Add(new ActionMenuItem(DescriptionHelper.Get(menuItemMethod), r => invokeMethod()));
                    }
                }
            }

            return Task.FromResult(items.AsEnumerable());
        }
    }
}