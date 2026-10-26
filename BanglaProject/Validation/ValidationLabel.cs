using System.Drawing;
using System.Windows.Forms;

namespace BanglaProject.Validation
{
    // Shows the result of a validation under a field.
    // error == null  -> green "✓ Looks good"
    // error != null  -> the error message in red
    public static class ValidationLabel
    {
        public static bool Show(Label label, string error, string okText = "✓ Looks good")
        {
            bool ok = error == null;

            label.Text = ok ? okText : error;
            label.ForeColor = ok ? Color.Green : Color.Red;
            label.Visible = true;
            return ok;
        }

        // True if any of the labels is currently showing a red error
        public static bool HasError(params Label[] labels)
        {
            foreach (Label label in labels)
            {
                if (label.Visible && label.ForeColor == Color.Red)
                    return true;
            }
            return false;
        }

        // Empties and hides the labels (used when a form opens or is cleared)
        public static void Reset(params Label[] labels)
        {
            foreach (Label label in labels)
            {
                label.Text = "";
                label.Visible = false;
            }
        }
    }
}
