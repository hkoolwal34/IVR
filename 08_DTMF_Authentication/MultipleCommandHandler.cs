using Ozeki.VoIP;
using System;
using System.Collections.Generic;

namespace _08_DTMF_Authentication
{
    class MultipleCommandHandler : ICommand
    {
        Queue<ICommand> _commandQueue;
        List<ICommand> _commandList;
        ICall _call;
        ICommand _currentCommand;

        public MultipleCommandHandler()
        {
            _commandList = new List<ICommand>();
        }

        public void Start(ICall call)
        {
            _call = call;
            _commandQueue = new Queue<ICommand>(_commandList);
            StartNextCommand();            
        }

        public void Cancel()
        {
            if (_currentCommand != null)
            {
                _currentCommand.Completed -= command_Completed;
                _currentCommand.Cancel();
            }
        }

        public event EventHandler Completed;

        public void AddCommand(ICommand command)
        {
            _commandList.Add(command);
        }

        void StartNextCommand()
        {
            if (_commandQueue.Count > 0)
            {
                _currentCommand = _commandQueue.Dequeue();
                _currentCommand.Completed += command_Completed;
                _currentCommand.Cancel();
                _currentCommand.Start(_call);
            }
            else
                OnCompleted();
        }

        void command_Completed(object sender, EventArgs e)
        {
            StartNextCommand();
        }

        void OnCompleted()
        {
            var handler = Completed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
