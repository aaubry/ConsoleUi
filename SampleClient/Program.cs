using ConsoleUi;
using ConsoleUi.Console;
using System;

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
}
