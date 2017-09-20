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
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleUi
{
    public abstract class SimpleMenu : IMenu
    {
        protected SimpleMenu()
        {
            var myType = GetType();
            Title = Regex.Replace(DescriptionHelper.Get(myType), " menu$", "");

            Items = new List<IMenuItem>();
            foreach (var menuItemMethod in myType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (menuItemMethod.DeclaringType == typeof(SimpleMenu))
                    continue;

                if (menuItemMethod.DeclaringType == typeof(object))
                    continue;

                var isVoid = menuItemMethod.ReturnType == typeof(void);
                if (!isVoid && !typeof(Task).IsAssignableFrom(menuItemMethod.ReturnType))
                    continue;

                var parameters = menuItemMethod.GetParameters();
                if (isVoid)
                {
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IMenuContext))
                    {
                        var invokeMethod = (Action<IMenuContext>)Delegate.CreateDelegate(typeof(Action<IMenuContext>), this, menuItemMethod);
                        Items.Add(new ActionMenuItem(DescriptionHelper.Get(menuItemMethod), invokeMethod));
                    }
                    else if (parameters.Length == 0)
                    {
                        var invokeMethod = (Action)Delegate.CreateDelegate(typeof(Action), this, menuItemMethod);
                        Items.Add(new ActionMenuItem(DescriptionHelper.Get(menuItemMethod), r => invokeMethod()));
                    }
                }
                else
                {
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IMenuContext))
                    {
                        var invokeMethod = (Func<IMenuContext, Task>)Delegate.CreateDelegate(typeof(Func<IMenuContext, Task>), this, menuItemMethod);
                        Items.Add(new ActionMenuItem(DescriptionHelper.Get(menuItemMethod), invokeMethod));
                    }
                    else if (parameters.Length == 0)
                    {
                        var invokeMethod = (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), this, menuItemMethod);
                        Items.Add(new ActionMenuItem(DescriptionHelper.Get(menuItemMethod), r => invokeMethod()));
                    }
                }
            }
        }

        public IList<IMenuItem> Items { get; private set; }
        public string Title { get; protected set; }
        public string Description { get; set; }
        public bool IsHighlighted { get; set; }
        public bool ShouldExit { get; set; }

        void IMenu.Enter()
        {
            Enter();
        }

        protected virtual void Enter() { }

        void IMenuItem.Execute(IMenuContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.Run(this);
        }
    }
}