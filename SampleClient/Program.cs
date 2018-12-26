using ConsoleUi;
using ConsoleUi.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new ConsoleMenuRunner();
            runner.Run(new MainMenu());
        }
    }

    public class MainMenu : SimpleMenu
    {
        public void DoStuff(IMenuContext context)
        {
            context.UserInterface.Info("Doing stuff...");
        }

        public IMenu Choose(IMenuContext context)
        {
            return new ChoiceMenu();
        }

        public IMenu DynamicChoose()
        {
            return new DynamicChoiceMenu();
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

    public class DynamicChoiceMenu : DynamicMenu
    {
        public DynamicChoiceMenu() : base("Dynamic choice")
        {
            ShouldExit = true;
        }

        protected override async Task<IEnumerable<IMenuItem>> GetItems(IMenuContext context)
        {
            await Task.Delay(500);
            return new[] { new ActionMenuItem(ctx => ctx.SuppressPause()) };
        }
    }
}
