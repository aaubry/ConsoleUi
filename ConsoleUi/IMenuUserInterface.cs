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
	public interface IMenuUserInterface
	{
		Task<bool> Confirm(string message, params object[] args);
        Task<bool> Confirm(bool force, string message, params object[] args);

		void Debug(string message, params object[] args);
		void Info(string message, params object[] args);
		void Warning(string message, params object[] args);
		void Error(string message, params object[] args);

        string Prompt(string message);

        Task<T> Select<T>(string message, Func<SelectionResult, (bool isValid, T selection)> choiceValidator, CancellationToken cancellationToken = default);

        IProgressBar StartProgress(string status);
    }

    public interface IProgressBar : IDisposable
    {
        void Clear();
        void Clear(string status);

        void SetProgress(int progressPercentage);
        void SetProgress(int progressPercentage, string status);

        void SetIndeterminate();
        void SetIndeterminate(string status);
    }

    public struct SelectionResult
    {
        public char SelectedCharacter { get; }
        public PromptType Type { get; }

        public SelectionResult(PromptType type)
        {
            Type = type;
            SelectedCharacter = '\0';
        }

        public SelectionResult(char selectedCharacter)
        {
            Type = PromptType.Character;
            SelectedCharacter = selectedCharacter;
        }
    }

    public enum PromptType
    {
        None,
        Character,
        Cancel,
        Accept,
    }
}