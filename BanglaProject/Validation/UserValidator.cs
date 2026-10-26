using System.Linq;
using System.Windows.Forms;

namespace BanglaProject.Validation
{
    // Validation rules for user and customer fields.
    // Every rule returns an error message, or null when the value is correct.
    // The Validate... methods show the result in the label and return true/false.
    public static class UserValidator
    {
        public static bool ValidateFullName(TextBox txt, Label lbl) => ValidationLabel.Show(lbl, FullNameError(txt.Text.Trim()));
        public static bool ValidateUsername(TextBox txt, Label lbl) => ValidationLabel.Show(lbl, UsernameError(txt.Text.Trim()));
        public static bool ValidateEmail(TextBox txt, Label lbl) => ValidationLabel.Show(lbl, EmailError(txt.Text.Trim()));
        public static bool ValidateMobile(TextBox txt, Label lbl) => ValidationLabel.Show(lbl, MobileError(txt.Text.Trim(), false));
        public static bool ValidatePassword(TextBox txt, Label lbl) => ValidationLabel.Show(lbl, PasswordError(txt.Text));
        public static bool ValidateNID(TextBox txt, Label lbl) => ValidationLabel.Show(lbl, NidError(txt.Text.Trim()));
        public static bool ValidateAddress(TextBox txt, Label lbl) => ValidationLabel.Show(lbl, AddressError(txt.Text.Trim()));

        // The mobile combo box on the sales form has a small label, so it uses short messages
        public static bool ValidateMobileComboBox(ComboBox cmb, Label lbl) => ValidationLabel.Show(lbl, MobileError(cmb.Text.Trim(), true), "✓ Valid");

        public static bool ValidateRole(ComboBox cmb, Label lbl)
        {
            return ValidationLabel.Show(lbl, cmb.SelectedIndex == -1 ? "Please select a role" : null);
        }

        private static string FullNameError(string name)
        {
            if (name == "") return "Full Name is required";
            if (!name.All(c => char.IsLetter(c) || c == ' ')) return "Full Name must contain only letters";
            if (name.Length < 2 || name.Length > 20) return "Full Name must be between 2 and 20 characters";

            // every word must start with a capital letter
            foreach (string word in name.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (!char.IsUpper(word[0])) return "Each word must start with a capital letter";
            }
            return null;
        }

        private static string UsernameError(string username)
        {
            if (username == "") return "Username is required";
            if (username.Contains(" ")) return "Username cannot contain spaces";
            if (username.Length < 5 || username.Length > 20) return "Username must be between 5 and 20 characters";
            if (!username.All(char.IsLower)) return "Username must contain only lowercase letters";
            return null;
        }

        private static string EmailError(string email)
        {
            if (email == "") return "Email is required";
            if (email.Any(char.IsUpper)) return "Email must be lowercase only";
            if (email.Contains(" ")) return "Email cannot contain spaces";
            if (!email.EndsWith("@gmail.com")) return "Only Gmail address is allowed (example@gmail.com)";

            string userPart = email.Substring(0, email.Length - "@gmail.com".Length);
            if (userPart == "" || !userPart.All(c => char.IsLower(c) || char.IsDigit(c) || c == '.'))
                return "Invalid Gmail address";
            return null;
        }

        private static string MobileError(string mobile, bool shortText)
        {
            if (mobile == "") return shortText ? "Mobile required" : "Mobile number is required";
            if (!mobile.All(char.IsDigit)) return shortText ? "Digits only" : "Mobile number must contain only digits";
            if (!mobile.StartsWith("01")) return shortText ? "Must start with 01" : "Mobile number must start with 01";
            if (mobile.Length != 11) return shortText ? "11 digits required" : "Mobile number must be 11 digits";
            return null;
        }

        private static string PasswordError(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return "Password is required";
            if (password.Contains(" ")) return "Password cannot contain spaces";
            if (password.Length < 8) return "Password must be at least 8 characters";
            if (!password.Contains("@")) return "Password must contain @ symbol";
            if (!password.Any(char.IsLower)) return "Password must contain at least one lowercase letter";
            if (!password.Any(char.IsUpper)) return "Password must contain at least one uppercase letter";
            if (!password.Any(char.IsDigit)) return "Password must contain at least one number";
            return null;
        }

        private static string NidError(string nid)
        {
            if (nid == "") return "NID is required";
            if (!nid.All(char.IsDigit)) return "NID must contain only digits";
            if (nid.Length != 10 && nid.Length != 17) return "NID must be 10 or 17 digits";
            return null;
        }

        private static string AddressError(string address)
        {
            // the ADDRESS column in the database holds at most 50 characters
            if (address == "") return "Address is required";
            if (address.Length < 4 || address.Length > 50) return "Address must be between 4 and 50 characters";
            if (!address.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == ',' || c == '.'))
                return "Address contains invalid characters";
            return null;
        }
    }
}
