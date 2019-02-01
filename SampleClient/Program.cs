using ConsoleUi;
using ConsoleUi.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var runner = new ConsoleMenuRunner();
            await runner.Run(new MainMenu());
        }
    }

    public class MainMenu : SimpleMenu
    {
        public void DoStuff(IMenuContext context)
        {
            context.UserInterface.Info("Doing stuff...");
        }

        public async Task DoMoreStuff(IMenuContext context)
        {
            await context.RunUntilCancelled(async ct =>
            {
                using (var progress = context.UserInterface.StartProgress("Doing stuff..."))
                {
                    for (int i = 0; i <= 100 && !ct.IsCancellationRequested; i++)
                    {
                        await Task.Delay(100, ct);
                        progress.SetProgress(i);
                    }
                }
            });
        }

        public async Task DoEvenMoreStuff(IMenuContext context)
        {
            await context.RunUntilCancelled(async ct =>
            {
                using (var progress = context.UserInterface.StartProgress("Doing stuff..."))
                {
                    progress.SetIndeterminate();
                    
                    await Task.Delay(10000, ct);
                }
            });
        }

        public IMenu Choose()
        {
            return new ChoiceMenu();
        }

        public IMenu DynamicChoose()
        {
            return new DynamicChoiceMenu();
        }

        public IMenu ManyOptions()
        {
            return new Menu("Many options", Enumerable.Range(0, 100).Select(i => new ActionMenuItem(i.ToString(), ctx => { })));
        }

        protected override Task<bool> CanExit(IMenuContext context)
        {
            return context.UserInterface.Confirm(true, "Exit ?");
        }
    }

    public class ChoiceMenu : SimpleMenu
    {
        public ChoiceMenu()
        {
            ShouldExit = true;
        }

        public void ChooseA(IMenuContext context)
        {
            context.SuppressPause();
        }

        public void ChooseB()
        {

        }
    }

    public class DynamicChoiceMenu : Menu
    {
        public DynamicChoiceMenu() : base("Dynamic choice", GetItems())
        {
            ShouldExit = true;
        }

        private static IAsyncEnumerable<IMenuItem> GetItems()
        {
            return AsyncEnumerable.CreateEnumerable(() =>
            {
                var delay = Task.Delay(500);
                var itemReturned = false;
                return AsyncEnumerable.CreateEnumerator<IMenuItem>(
                    async ct =>
                    {
                        if (!itemReturned)
                        {
                            await delay;
                            itemReturned = true;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    },
                    () => new ActionMenuItem(ctx => ctx.SuppressPause()),
                    () => { }
                );
            });
        }
    }
}
