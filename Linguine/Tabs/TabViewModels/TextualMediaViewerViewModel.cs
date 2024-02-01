using ExternalMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UserInputInterfaces;

namespace Linguine.Tabs
{
    internal class TextualMediaViewerViewModel : TabViewModelBase
    {
        public ICommand LoadCommand { get; private set; }

        public String RawText
        {
            get => rawText;
            private set
            {
                rawText = value;
                OnPropertyChanged(nameof(RawText));
            }
        }

        private InternalTextualMedia? _textualMedia;
        private TextualMediaLoader _loader;
        private string rawText;

        public TextualMediaViewerViewModel(UIComponents uiComponents, MainModel parent) : base(uiComponents, parent)
        {
            Title = "Text Viewer";

            _loader = new TextualMediaLoader(uiComponents.CanVerify, uiComponents.CanChooseFromList);

            LoadCommand = new RelayCommand(() => Load());
        }

        private void Load()
        {
            if (_textualMedia is not null)
            {
                if (!_uiComponents.CanVerify.AskYesNo("Media already loaded, load a new one?"))
                {
                    return;
                }
            }

            String filename = _uiComponents.CanBrowseFiles.Browse();

            try
            {
                _textualMedia = _loader.LoadFromFile(filename);
                RawText = _textualMedia.Text;
            }
            catch (Exception e)
            {
                _uiComponents.CanMessage.Show("media loading aborted");
            }
        }
    }
}
