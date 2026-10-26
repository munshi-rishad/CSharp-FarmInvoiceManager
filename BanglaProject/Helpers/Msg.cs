using System.Windows.Forms;

namespace BanglaProject.Helpers
{
    // Short helpers for the message boxes used all over the project
    public static class Msg
    {
        public static void Info(string text, string title = "Information")
        {
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Warning(string text, string title = "Warning")
        {
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(string text, string title = "Error")
        {
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Yes / No question, returns true when the user clicks Yes
        public static bool Confirm(string text, string title = "Confirm")
        {
            return MessageBox.Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        // Shown when a database call throws an exception
        public static void DatabaseError(System.Exception ex)
        {
            Error("Something went wrong with the database. Please try again.\n\nDetails: " + ex.Message, "Database Error");
        }
    }
}
