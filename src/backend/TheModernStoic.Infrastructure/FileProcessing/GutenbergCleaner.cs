namespace TheModernStoic.Infrastructure.FileProcessing;

public class GutenbergCleaner
{
    //simple heuristic: Gutenberg texts usually have start/end markers
    private const string StartMarker = "*** START OF THE PROJECT GUTENBERG EBOOK";
    private const string EndMarker = "*** END OF THE PROJECT GUTENBERG EBOOK";

    public string Clean(string rawText)
    {
        //1. Ensure line endings are consistent
        var text = rawText.Replace("\r\n", "\n");

        //2. Find Start marker (ignore case)
        var startIndex = text.IndexOf(StartMarker, StringComparison.OrdinalIgnoreCase);

        //if start index is found
        if (startIndex != -1)
        {
            //goes to the new line
            startIndex = text.IndexOf('\n', startIndex) + 1;
        }
        else
        {
            startIndex = 0; //start from beginning
        }

        //3. Find End marker
        var endIndex = text.IndexOf(EndMarker, StringComparison.OrdinalIgnoreCase);
        //if the book doesn't have a marker
        if (endIndex == -1)
        {
            endIndex = text.Length; //goes to the end
        }

        // 4. Extract content
        return text.Substring(startIndex, endIndex - startIndex).Trim();
    }
}