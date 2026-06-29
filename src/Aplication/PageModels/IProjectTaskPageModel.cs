using Aplication.Models;
using CommunityToolkit.Mvvm.Input;

namespace Aplication.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}