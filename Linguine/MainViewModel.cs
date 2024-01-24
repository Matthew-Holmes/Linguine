using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;
using ExternalMedia;

namespace Linguine
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ICanVerify _canVerify;
        private ICanChooseFromList _canChooseFromList;
        private ICanBrowseFiles _canBrowseFiles;

        private TextualMediaLoader _textualMediaLoader;
        private InternalTextualMedia _textualMedia;


        public RelayCommand LoadTextualMediaCommand { get; }

        public MainViewModel(ICanVerify canVerify, ICanChooseFromList canChooseFromList, ICanBrowseFiles canBrowseFiles)
        {
            _canVerify = canVerify;
            _canChooseFromList = canChooseFromList;
            _canBrowseFiles = canBrowseFiles;

            _textualMediaLoader = new TextualMediaLoader(canVerify, canChooseFromList);

            LoadTextualMediaCommand = new RelayCommand(LoadTextualMedia, () => true);
        }

        private void LoadTextualMedia()
        {
            String path = _canBrowseFiles.Browse();
            _textualMedia = _textualMediaLoader.LoadFromFile(path);
        }
    }
}
