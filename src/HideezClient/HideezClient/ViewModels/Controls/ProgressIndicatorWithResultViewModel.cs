using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.ViewModels.Controls
{
    public enum ProcessResult
    {
        /// <summary>
        /// Used when result of operation is unavailable
        /// </summary>
        Undefined,
        Successful,
        Failed
    }

    public class ProgressIndicatorWithResultViewModel : ReactiveObject
    {
        [Reactive] public bool InProgress { get; set; }
        /// <summary>
        /// Property to display the sucessful result
        /// </summary>
        [Reactive] public bool IsSuccessful { get; set; }
        /// <summary>
        /// Property to display the failed result
        /// </summary>
        [Reactive] public bool IsFailed { get; set; }
        /// <summary>
        /// Property to display the result of the operation for 5 seconds
        /// </summary>
        [Reactive] public ProcessResult Result { get; set; }

        public ProgressIndicatorWithResultViewModel()
        {
            this.ObservableForProperty(vm => vm.Result).Subscribe(vm => { OnResultChanged(); });
        }

        private async void OnResultChanged()
        {
            if (Result != ProcessResult.Undefined)
            {
                if (Result == ProcessResult.Successful)
                {
                    IsSuccessful = true;
                    await Task.Delay(5000);
                    IsSuccessful = false;
                }
                else if (Result == ProcessResult.Failed)
                {
                    IsFailed = true;
                    await Task.Delay(5000);
                    IsFailed = false;
                }
            }
            else
            {
                IsFailed = false;
                IsSuccessful = false;
            }
        }
    }
}
