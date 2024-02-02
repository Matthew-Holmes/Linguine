using Infrastructure;
using System.Text;
using System.Threading.Tasks.Sources;
using UserInputInterfaces;

namespace ExternalMedia
{
    public class TextualMediaLoader
    {
        private ICanVerify _verifiesWithUser;
        private ICanChooseFromList _userChoosesFromList;

        public TextualMediaLoader(ICanVerify verifiesWithUser, ICanChooseFromList userChoosesFromList)
        {
            _verifiesWithUser = verifiesWithUser;
            _userChoosesFromList = userChoosesFromList;
        }

        public TextualMedia LoadFromFile(String path, LanguageCode lc)
        {
            return LoadFromString(ReadStringFromFile(path), lc);
        }

        private TextualMedia LoadFromString(String rawText, LanguageCode lc)
        {
            TextualMedia textualMedia = new TextualMedia(rawText, lc);
            return textualMedia;
        }


        private String ReadStringFromFile(String path)
        {
            // Check if the path is valid
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path is invalid.");
            }

            // Check if the file exists
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found at the specified path.", path);
            }

            Encoding encoding = DetermineEncoding(path);

            // Reading the file
            try
            {
                return File.ReadAllText(path, encoding);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading from file.", ex);
            }
        }

        private Encoding DetermineEncoding(String path)
        {
            // Determines encoding with user validation
            Encoding predictedEncoding = PredictEncoding(path);
            Encoding encoding;

            String preview = PreviewWithEncoding(path, predictedEncoding, 300);

            if (_verifiesWithUser.AskYesNo($"Is this legible? \"{preview}\""))
            {
                encoding = predictedEncoding;
            }
            else
            {
                List<(Encoding, String)> previews = GenerateDecodedPreviews(path, 300);

                int correct = _userChoosesFromList.SelectFromListZeroIndexed(
                    "Choose correct preview: ",
                     previews.Select(preview => preview.Item2).ToList());

                if (correct < 0 || correct >= previews.Count())
                {
                    // out of list range
                    throw new ArgumentException("Invalid selection");
                }

                encoding = previews[correct].Item1;
            }

            return encoding;
        }

        private Encoding PredictEncoding(String path)
        {
            // Use BOM (Byte Order Mark) to detect encoding
            // Default to UTF-8 if BOM is not present
            using (var reader = new StreamReader(path, Encoding.Default, true))
            {
                if (reader.Peek() >= 0) // you need this!
                    reader.Read();

                return reader.CurrentEncoding;
            }
        }

        private List<(Encoding, String)> GenerateDecodedPreviews(String path, int previewLength)
        {
            List<Encoding> possibleEncodings = new List<Encoding>
            {
                Encoding.UTF8,
                Encoding.ASCII,
                Encoding.Unicode,
                Encoding.BigEndianUnicode,
                Encoding.UTF32,
            };

            List<(Encoding, String)> previews = new List<(Encoding, String)>();
            foreach (var encoding in possibleEncodings)
            {
                try
                {
                    string preview = PreviewWithEncoding(path, encoding, previewLength);
                    previews.Add((encoding, preview));
                }
                catch
                {
                    // Handle or log exceptions if necessary
                }
            }

            return previews;
        }

        private String PreviewWithEncoding(String path, Encoding encoding, int previewLength)
        {
            using (var reader = new StreamReader(path, encoding, true))
            {
                char[] buffer = new char[previewLength];
                int totalRead = 0;
                int read;

                // Read up to previewLength characters or until the end of the file
                while (totalRead < previewLength && (read = reader.Read(buffer, totalRead, previewLength - totalRead)) > 0)
                {
                    totalRead += read;
                }

                return new String(buffer, 0, totalRead);
            }
        }
    }
}
