using Hideez.ARM;
using HideezClient.Utilities;
using MvvmExtensions.Commands;
using ReactiveUI;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Hideez.SDK.Communication.Utils;

namespace HideezClient.ViewModels
{
    class AppViewModel : ReactiveObject
    {
        public AppViewModel(string title, bool isUrl = false)
        {
            this.IsUrl = isUrl;
            this.Title = title;

            this.WhenAnyValue(x => x.Title).Subscribe(x =>
            {
                this.RaisePropertyChanged(nameof(CanCancel));
                this.RaisePropertyChanged(nameof(CanApply));
                this.RaisePropertyChanged(nameof(CanEdit));
                this.RaisePropertyChanged(nameof(CanDelete));
            });

            this.WhenAnyValue(x => x.EditableTitle).Subscribe(x =>
            {
                this.RaisePropertyChanged(nameof(CanCancel));
                this.RaisePropertyChanged(nameof(CanApply));
                this.RaisePropertyChanged(nameof(CanEdit));
                this.RaisePropertyChanged(nameof(CanDelete));
            });
        }

        #region Property

        [Reactive] public bool IsInEditState { get; set; }
        [Reactive] public string Title { get; set; }
        [Reactive] public string EditableTitle { get; set; }
        [Reactive] public bool IsUrl { get; set; }

        public bool CanCancel { get; } = true;
        public bool CanApply { get { return !string.IsNullOrWhiteSpace(EditableTitle) && Title != EditableTitle; } }
        public bool CanEdit { get; } = true;
        public bool CanDelete { get; } = true;

        #endregion

        public void ApplyChanges()
        {
            if (IsUrl)
                Title = Hideez.ARM.UrlUtils.GetDomainFromUrl(EditableTitle) ?? EditableTitle;
            else
                Title = EditableTitle;

            IsInEditState = false;
        }

        public void Edit()
        {
            IsInEditState = true;
            EditableTitle = Title;
        }

        public void CancelEdit()
        {
            IsInEditState = false;
            EditableTitle = Title;
        }
    }
}
