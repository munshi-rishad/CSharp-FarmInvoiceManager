using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using BanglaProject.Data;

namespace BanglaProject.Helpers
{
    // Every window of the project inherits from this class.
    // Code that was copied into each form before now lives here only once:
    // window settings, the "close the application?" question, switching forms
    // and small helpers for grids, labels and the menu buttons.
    public class BaseForm : Form
    {
        public BaseForm()
        {
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
        }

        // Clicking the X button of any window asks once and closes the whole application
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel) return;

            if (Msg.Confirm("Are you sure you want to close the application?", "Confirm Exit"))
                Environment.Exit(0);
            else
                e.Cancel = true;
        }

        // Hides this window and shows the next one
        protected void GoTo(Form next)
        {
            Hide();
            next.Show();
        }

        // Fills a grid from a SELECT query and leaves no row selected
        protected static DataTable LoadGrid(DataGridView grid, string sql, params SqlParameter[] parameters)
        {
            try
            {
                DataTable table = Db.GetTable(sql, parameters);
                grid.AutoGenerateColumns = false;
                grid.DataSource = table;
                grid.ClearSelection();
                grid.CurrentCell = null;
                return table;
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
                return null;
            }
        }

        // "Selected: ID - Name" in green
        protected static void ShowSelected(Label label, string id, string name)
        {
            label.Text = "Selected: " + id + " - " + name;
            label.ForeColor = Color.Green;
            label.Visible = true;
        }

        // "Found" in green or "Not Found" in red after a search
        protected static void ShowFound(Label label, bool found)
        {
            label.Text = found ? "Found" : "Not Found";
            label.ForeColor = found ? Color.Green : Color.Red;
            label.Visible = true;
        }

        protected static void HideLabel(Label label)
        {
            label.Text = "";
            label.Visible = false;
        }

        // Colors the clicked menu button teal and the other menu buttons back to normal
        protected static void Highlight(Button active, params Button[] others)
        {
            active.BackColor = Color.Teal;
            active.ForeColor = Color.White;

            foreach (Button button in others)
            {
                button.BackColor = Color.LemonChiffon;
                button.ForeColor = Color.Black;
            }
        }

        // Customer list used by both the admin and the employee dashboard.
        // An empty search shows all customers, otherwise customers whose mobile number matches.
        protected static void LoadCustomers(DataGridView grid, Label resultLabel, string mobileSearch = "")
        {
            if (mobileSearch == "")
            {
                LoadGrid(grid, "SELECT * FROM CustomerInfo ORDER BY CUSTOMER_ID");
                HideLabel(resultLabel);
                return;
            }

            DataTable table = LoadGrid(grid,
                "SELECT * FROM CustomerInfo WHERE CUSTOMER_MOBILE LIKE @Search ORDER BY CUSTOMER_ID",
                Db.P("@Search", "%" + mobileSearch + "%"));

            if (table != null)
                ShowFound(resultLabel, table.Rows.Count > 0);
        }

        // Personal information page used by both the admin and the employee dashboard.
        // The text boxes have the same names on both forms.
        protected void ShowPersonalInfo(string userId)
        {
            string[,] fields =
            {
                { "txtUserId", "USER_ID" }, { "txtFullName", "FULL_NAME" }, { "txtEmail", "EMAIL" },
                { "txtUsername", "USERNAME" }, { "txtPassword", "PASSWORD" }, { "txtMobile", "MOBILE" },
                { "txtNID", "NID" }, { "txtAddress", "ADDRESS" }, { "txtRole", "ROLE" }, { "txtStatus", "STATUS" }
            };

            try
            {
                DataTable table = Db.GetTable("SELECT * FROM UserInfo WHERE USER_ID = @ID", Db.P("@ID", userId));
                if (table.Rows.Count == 0) return;

                DataRow user = table.Rows[0];
                for (int i = 0; i < fields.GetLength(0); i++)
                {
                    Control[] found = Controls.Find(fields[i, 0], true);
                    if (found.Length > 0)
                        found[0].Text = user[fields[i, 1]].ToString();
                }
            }
            catch (Exception ex)
            {
                Msg.DatabaseError(ex);
            }
        }
    }
}
