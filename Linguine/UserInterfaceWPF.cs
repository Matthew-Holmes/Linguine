using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UserInputInterfaces;

namespace Linguine
{
    public class UserInteractionServiceWPF : ICanVerify
    {
        public bool AskYesNo(string question)
        {
            // Implementation for asking the user a question and returning the response
            // For example, using MessageBox in WPF:
            var result = MessageBox.Show(question, "Confirm", MessageBoxButton.YesNo);
            return result == MessageBoxResult.Yes;
        }
    }


    public class UserSelectionServiceWPF : ICanChooseFromList
    {
        public int SelectFromListZeroIndexed(string question, List<string> options)
        {
            // Create a new window for the selection dialog
            var selectionWindow = new Window
            {
                Title = question,
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            // Create a ListBox to display the options
            var listBox = new System.Windows.Controls.ListBox
            {
                ItemsSource = options
            };
            listBox.SelectionChanged += (s, e) =>
            {
                // Close the window when a selection is made
                selectionWindow.DialogResult = true;
            };

            // Set the ListBox as the content of the window
            selectionWindow.Content = listBox;

            // Show the window as a dialog
            var result = selectionWindow.ShowDialog();

            // Return the index of the selected item or -1 if no selection was made
            return result == true ? listBox.SelectedIndex : -1;
        }
    }

    public class FileBrowserService : ICanBrowseFiles
    {
        public string Browse()
        {
            // Create an OpenFileDialog to let the user select a file
            var openFileDialog = new OpenFileDialog
            {
                // You can set properties like Filter, InitialDirectory, Title, etc., as needed
                Filter = "All files (*.*)|*.*",
                Title = "Select a file"
            };

            // Show the dialog and get the result
            bool? result = openFileDialog.ShowDialog();

            // Return the selected file path or null/empty string if the user cancels
            return result == true ? openFileDialog.FileName : string.Empty;
        }

        public string BrowseSaveFile(
            String defaultFileName,
            String defaultExtension,
            String filter)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName, 
                DefaultExt = defaultExtension, 
                Filter = filter, 
                Title = "Select Save Location"
            };

            bool? result = saveFileDialog.ShowDialog();

            return result == true ? saveFileDialog.FileName : String.Empty;
        }
    }


    public class UserResponseService : ICanGetText
    {
        public string GetResponse(string question)
        {
            // Create a new window for the input dialog
            var inputWindow = new Window
            {
                Title = question,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            // Create a StackPanel to layout the controls
            var stackPanel = new System.Windows.Controls.StackPanel();

            // Create a TextBox for user input
            var textBox = new System.Windows.Controls.TextBox();
            stackPanel.Children.Add(textBox);

            // Create a Button for submission
            var submitButton = new System.Windows.Controls.Button
            {
                Content = "Submit",
                Margin = new Thickness(5)
            };
            submitButton.Click += (s, e) =>
            {
                // Close the window when the button is clicked
                inputWindow.DialogResult = true;
            };
            stackPanel.Children.Add(submitButton);

            // Set the StackPanel as the content of the window
            inputWindow.Content = stackPanel;

            // Show the window as a dialog
            var result = inputWindow.ShowDialog();

            // Return the text entered by the user or an empty string if no input was given
            return result == true ? textBox.Text : string.Empty;
        }
    }

    public class MessageUserService : ICanShowMessage
    {
        public void Show(string message)
        {
            MessageBox.Show(message);
        }
    }


}
