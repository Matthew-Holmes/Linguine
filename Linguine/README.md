# Linguine

This directory is for the application files: xaml views, view models and the main model. I've used a MVVM architecture, with the model layer the penultimate overall layer - only above the Infrastructure layer.

Most of the coupling between view model and view is very loose, the one exception being the `Tabs/WPF/Controls/RichTextControl.xaml`

This control is responsible for displaying the processed text, and is visible in the top level README.md. It fills pages of text that fit the area provided, this means the logic is highly dependant on the UI, so couldn't be abstracted to a ViewModel. I implemented this interaction using a series of EventHandlers and RelayCommands, so that multiple pages can be turned at once, with only the full processed data loaded as the final page is reached. This tighter coupling does undermine the overall MVVM architecture, but I opted for it to preserve performance, and excessive database access, as only the processed data required for the text on screen is loaded.
