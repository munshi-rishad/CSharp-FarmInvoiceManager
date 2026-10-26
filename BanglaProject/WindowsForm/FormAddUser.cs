using System;
using System.Windows.Forms;
using BanglaProject.Data;
using BanglaProject.Helpers;
using BanglaProject.Validation;

namespace BanglaProject
{
    // Admin adds a new admin or employee
    public partial class FormAddUser : BaseForm
    {
        private readonly FormAdminView adminView;

        public FormAddUser()
        {
            InitializeComponent();
        }

        public FormAddUser(FormAdminView adminView) : this()
        {
            this.adminView = adminView;
        }

        private Label[] ErrorLabels()
        {
            return new[] { lblFullNameError, lblEmailError, lblUsernameError, lblMobileError,
                           lblNidError, lblAddressError, lblRoleError, lblPasswordError };
        }

        private void FormAddUser_Load(object sender, EventArgs e)
        {
            ValidationLabel.Reset(ErrorLabels());
        }

        // live validation while typing
        private void txtFullName_TextChanged(object sender, EventArgs e) => UserValidator.ValidateFullName(txtFullName, lblFullNameError);
        private void txtEmail_TextChanged(object sender, EventArgs e) => UserValidator.ValidateEmail(txtEmail, lblEmailError);
        private void txtUsername_TextChanged(object sender, EventArgs e) => UserValidator.ValidateUsername(txtUsername, lblUsernameError);
        private void txtMobile_TextChanged(object sender, EventArgs e) => UserValidator.ValidateMobile(txtMobile, lblMobileError);
        private void txtPassword_TextChanged(object sender, EventArgs e) => UserValidator.ValidatePassword(txtPassword, lblPasswordError);
        private void txtNid_TextChanged(object sender, EventArgs e) => UserValidator.ValidateNID(txtNid, lblNidError);
        private void txtAddress_TextChanged(object sender, EventArgs e) => UserValidator.ValidateAddress(txtAddress, lblAddressError);
        private void cmbRole_SelectedIndexChanged(object sender, EventArgs e) => UserValidator.ValidateRole(cmbRole, lblRoleError);

        // checks every field once more and shows all errors
        private bool AllFieldsValid()
        {
            bool ok = true;
            ok &= UserValidator.ValidateFullName(txtFullName, lblFullNameError);
            ok &= UserValidator.ValidateEmail(txtEmail, lblEmailError);
            ok &= UserValidator.ValidateUsername(txtUsername, lblUsernameError);
            ok &= UserValidator.ValidateMobile(txtMobile, lblMobileError);
            ok &= UserValidator.ValidatePassword(txtPassword, lblPasswordError);
            ok &= UserValidator.ValidateNID(txtNid, lblNidError);
            ok &= UserValidator.ValidateAddress(txtAddress, lblAddressError);
            ok &= UserValidator.ValidateRole(cmbRole, lblRoleError);
            return ok;
        }

        // Password = username with a capital first letter + "@" + last 4 digits of the mobile number
        private void btnGeneratePassword_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string mobile = txtMobile.Text.Trim();

            if (username == "" || mobile.Length < 4)
            {
                Msg.Warning("Please enter username and mobile number first.", "Cannot Generate Password");
                return;
            }

            txtPassword.Text = char.ToUpper(username[0]) + username.Substring(1) + "@" + mobile.Substring(mobile.Length - 4);
            Msg.Info("Password generated successfully!", "Password Generated");
        }

        private void btnUserSave_Click(object sender, EventArgs e)
        {
            if (!AllFieldsValid())
            {
                Msg.Warning("Please fill all fields correctly before saving.", "Validation Error");
                return;
            }

            try
            {
                string username = txtUsername.Text.Trim();
                if (Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM UserInfo WHERE USERNAME = @U", Db.P("@U", username))) > 0)
                {
                    Msg.Warning("This username is already taken. Please choose another one.", "Validation Error");
                    return;
                }

                // roles are saved in small letters ("admin" / "employee") because the login checks them that way.
                // Admins get A-xxx and employees E-xxx, and the number continues for both (A-001, E-002, E-003 ...)
                bool isAdmin = cmbRole.SelectedItem.ToString().ToLower() == "admin";
                string role = isAdmin ? "admin" : "employee";
                string prefix = isAdmin ? "A-" : "E-";
                string newUserId = Db.NextId("UserInfo", "USER_ID", prefix, "[AE]-%", 3);

                Db.Execute(
                    "INSERT INTO UserInfo (USER_ID, ROLE, FULL_NAME, USERNAME, EMAIL, MOBILE, PASSWORD, NID, ADDRESS, CREATED_DATE, STATUS) " +
                    "VALUES (@ID, @Role, @FullName, @Username, @Email, @Mobile, @Password, @NID, @Address, CAST(GETDATE() AS DATE), @Status)",
                    Db.P("@ID", newUserId),
                    Db.P("@Role", role),
                    Db.P("@FullName", txtFullName.Text.Trim()),
                    Db.P("@Username", username),
                    Db.P("@Email", txtEmail.Text.Trim()),
                    Db.P("@Mobile", txtMobile.Text.Trim()),
                    Db.P("@Password", txtPassword.Text.Trim()),
                    Db.P("@NID", txtNid.Text.Trim()),
                    Db.P("@Address", txtAddress.Text.Trim()),
                    Db.P("@Status", chkIsActive.Checked));

                Msg.Info("User saved successfully! User ID: " + newUserId, "Success");
                ClearForm();
                adminView.LoadUsers();
                GoTo(adminView);
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }

        private void ClearForm()
        {
            txtFullName.Clear();
            txtEmail.Clear();
            txtUsername.Clear();
            txtMobile.Clear();
            txtPassword.Clear();
            txtNid.Clear();
            txtAddress.Clear();
            cmbRole.SelectedIndex = -1;
            chkIsActive.Checked = false;
            ValidationLabel.Reset(ErrorLabels());   // after clearing, so no red messages stay
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            GoTo(adminView);
        }
    }
}
