using System.Windows.Input;

namespace MTGCreateYourOwnCreature.ViewModel.Commands
{
    public class RelayCommand : ICommand
    {
        protected readonly Action<object?> m_Execute;
        protected readonly Predicate<object?> m_CanExecute;

        public event EventHandler? CanExecuteChanged;


        public RelayCommand(Action<object?> execute, Predicate<object?> canExecute = null)
        {
            m_Execute = execute;
            m_CanExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return m_CanExecute == null || m_CanExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            m_Execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
