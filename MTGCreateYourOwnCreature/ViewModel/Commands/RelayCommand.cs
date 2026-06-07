
using System.Windows.Input;

namespace MTGCreateYourOwnCreature.ViewModel.Commands
{
    /// <summary>
    /// A generic command implementation that delegates execution and execution status logic via delegates.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// The backing field for the execution logic delegate.
        /// </summary>
        protected readonly Action<object?> m_Execute;

        /// <summary>
        /// The backing field for the execution status logic delegate.
        /// </summary>
        protected readonly Predicate<object?>? m_CanExecute;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// The WPF command manager generally listens to this event to enable or disable UI controls.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic to run when the command is invoked.</param>
        /// <param name="canExecute">The optional execution status logic that determines whether the command can run.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            m_Execute = execute;
            m_CanExecute = canExecute;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            return m_CanExecute == null || m_CanExecute(parameter);
        }

        /// <summary>
        /// Invokes the delegated execution logic.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object? parameter)
        {
            m_Execute(parameter);
        }

        /// <summary>
        /// Manually raises the <see cref="CanExecuteChanged"/> event.
        /// Used to notify the UI to re-evaluate the command's execution state and update bound controls accordingly.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
