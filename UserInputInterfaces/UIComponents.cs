using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInputInterfaces
{
    public struct UIComponents
    {
        public ICanBrowseFiles CanBrowseFiles;
        public ICanChooseFromList CanChooseFromList;
        public ICanVerify CanVerify;
        public ICanGetText CanGetText;
        public ICanShowMessage CanMessage;
        public UIComponents(ICanBrowseFiles canBrowseFiles, ICanChooseFromList canChooseFromList, ICanVerify canVerify, ICanGetText canGetText, ICanShowMessage canMessage)
        {
            CanBrowseFiles = canBrowseFiles;
            CanChooseFromList = canChooseFromList;
            CanVerify = canVerify;
            CanGetText = canGetText;
            CanMessage = canMessage;
        }
    }


}
