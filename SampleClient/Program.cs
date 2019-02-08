using ConsoleUi;
using ConsoleUi.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SampleClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
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

        public IMenu Counter() => new CountingMenu();

        public IMenu DelayedException()
        {
            return new DelayedExceptionMenu();
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

        public async Task ChooseB(IMenuContext context)
        {
            if (await context.UserInterface.Confirm("Perform B ?"))
            {
                context.UserInterface.Info("Performing B");
            }
        }
    }

    public class CountingMenu : DynamicMenu
    {
        private int counter;

        public CountingMenu()
            : base("Counter")
        {
        }

        protected override Task<IEnumerable<IMenuItem>> LoadItems(CancellationToken cancellationToken)
        {
            return Task.FromResult(new IMenuItem[]
            {
                new ActionMenuItem($"Count: {counter}", ctx =>
                {
                    ++counter;
                    ctx.SuppressPause();
                    ctx.InvalidateMenu();
                })
            }.AsEnumerable());
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

    public class DelayedExceptionMenu : Menu
    {
        public DelayedExceptionMenu() : base("Delayed exception", GetItems())
        {
        }

        private static IAsyncEnumerable<IMenuItem> GetItems()
        {
            return AsyncEnumerable.CreateEnumerable(() =>
            {
                var delay = Task.Delay(500);
                return AsyncEnumerable.CreateEnumerator<IMenuItem>(
                    async ct =>
                    {
                        await Task.Delay(500);
                        throw new Exception("Failure!");
                    },
                    () => null,
                    () => { }
                );
            });
        }
    }
}
