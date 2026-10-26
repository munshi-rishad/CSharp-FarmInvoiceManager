using System;
using System.Linq;
using System.Windows.Forms;

namespace BanglaProject.Validation
{
    // Validation rules for the Add Product and Update Product forms.
    public static class ProductValidator
    {
        public static bool ValidateName(TextBox txt, Label lbl)
        {
            string name = txt.Text.Trim();
            string error = null;

            if (name == "") error = "Product name is required";
            else if (name.Length > 50) error = "Product name must be at most 50 characters";

            return ValidationLabel.Show(lbl, error);
        }

        public static bool ValidateQuantity(TextBox txt, Label lbl)
        {
            string error = null;

            if (string.IsNullOrWhiteSpace(txt.Text)) error = "Quantity is required";
            else if (!int.TryParse(txt.Text.Trim(), out int qty)) error = "Quantity must be a number";
            else if (qty < 0 || qty > 999) error = "Quantity must be between 0 and 999";

            return ValidationLabel.Show(lbl, error);
        }

        public static bool ValidateCategory(TextBox txt, Label lbl)
        {
            string category = txt.Text.Trim();
            string error = null;

            if (category == "") error = "Category is required";
            else if (!category.All(char.IsLetter)) error = "Category must contain only letters";
            else if (!char.IsUpper(category[0])) error = "Category must start with a capital letter";

            return ValidationLabel.Show(lbl, error);
        }

        public static bool ValidatePrice(TextBox txt, Label lbl)
        {
            string price = txt.Text.Trim();
            string error = null;

            if (price == "") error = "Unit price is required";
            else if (!price.All(char.IsDigit)) error = "Price must contain only digits";
            else if (!int.TryParse(price, out int value) || value <= 0) error = "Price must be greater than 0";

            return ValidationLabel.Show(lbl, error);
        }

        public static bool ValidateDate(DateTimePicker dtp, Label lbl)
        {
            return ValidationLabel.Show(lbl, dtp.Value.Date > DateTime.Today ? "Future date is not allowed" : null);
        }

        public static bool ValidateStatus(CheckBox chk, Label lbl)
        {
            return ValidationLabel.Show(lbl, chk.Checked ? null : "Product must be Active");
        }
    }
}
