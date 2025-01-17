﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace WbotMgr
{
    public partial class Blockedfrm : Form
    {
        // Property to store a reference to the MainForm form
        public new MainForm ParentForm { get; set; }

        // Add a property to store the botConfig object
        public BotConfiguration BotConfig { get; set; }

        // Declare a list to store current items
        public List<string> currentItems = new List<string>();

        // Declare a new list for odd items
        private List<string> oddItems = new List<string>();

        public Blockedfrm()
        {
            InitializeComponent();
        }

        // Method to load blocked items into the ListBox
        private void Blockedfrm_Load(object sender, EventArgs e)
        {
            // Associate the context menu with the ListBox
            BlockedList.ContextMenuStrip = contextMenuStrip1;

            // Call the method to load blocked items into the ListBox
            LoadBlockedItems();

            // If it doesn't have a name above, add it if necessary
            AddUniqueNameIfNeeded();

            // Save the current ListBox items in the currentItems list
            foreach (string item in BlockedList.Items)
            {
                currentItems.Add(item);
            }

            // Apply changes in the main form (Mainfrm)
            BotConfig.blocked = currentItems.ToList();
            ParentForm.ApplyChanges();

            // Filter odd items and update ListBox
            FilterOddItems();
            BlockedList.Items.Clear();
            BlockedList.Items.AddRange(oddItems.ToArray());
        }

        private void LoadBlockedItems()
        {
            // Check if a BotConfig object is provided
            if (BotConfig != null)
            {
                // Load all items into the ListBox
                List<string> blockedItems = BotConfig.blocked.Cast<string>().ToList();

                // Clear the ListBox before loading new items
                BlockedList.Items.Clear();

                // Add each item to the ListBox
                foreach (string item in blockedItems)
                {
                    BlockedList.Items.Add(item);
                }
            }
        }

        private void AddUniqueNameIfNeeded()
        {
            // Iterate through the items in the ListBox
            for (int i = 1; i < BlockedList.Items.Count; i += 2)
            {
                // Check if the previous item is null or is a number
                if (BlockedList.Items[i - 1] == null || IsValidPhoneNumber(BlockedList.Items[i - 1].ToString()))
                {
                    // Add a unique name above the number with a unique name
                    BlockedList.Items.Insert(i - 1, GetUniqueName("John Doe"));
                }
            }

            // If the last item is a number, add a unique name above it
            if (BlockedList.Items.Count % 2 == 1 && IsValidPhoneNumber(BlockedList.Items[BlockedList.Items.Count - 1].ToString()))
            {
                // Use Insert to add a unique name above the last item with a unique name
                BlockedList.Items.Insert(BlockedList.Items.Count - 1, GetUniqueName("John Doe"));
            }
        }

        private void FilterOddItems()
        {
            oddItems.Clear();

            // Add odd items to the new list
            for (int i = 0; i < BlockedList.Items.Count; i += 2)
            {
                if (BlockedList.Items[i] != null)
                {
                    oddItems.Add(BlockedList.Items[i].ToString());
                }
            }
        }

        private bool IsValidName(string name)
        {
            //13_01_2024
            // Check if the name is only numeric or starts with a "+"
            return !name.All(char.IsDigit) && !(name.StartsWith("+") && name.Substring(1).All(char.IsDigit));
        }

        private string CleanNumber(string number)
        {
            //13_01_2024
            // Remove leading "+" and any occurrence of "-" or " " from the number
            return number.TrimStart('+').Replace("-", "").Replace(" ", "");
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            //13_01_2024
            // Check if the phoneNumber is a valid number (contains only digits, may start with '+' and can contain '-')
            return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ');
        }

        private string GetUniqueName(string baseName)
        {
            // Counter to generate unique names
            int counter = 1;

            // Generate a new unique name until it does not exist in the ListBox
            while (BlockedList.Items.Contains($"{baseName}{counter}"))
            {
                counter++;
            }

            return $"{baseName}{counter}";
        }

        private void BlockedList_DoubleClick(object sender, EventArgs e)
        {
            if (BlockedList.SelectedIndex != -1)
            {
                // Get the contact name selected
                string selectedName = BlockedList.SelectedItem.ToString();
                int selectedIndex = currentItems.IndexOf(selectedName);

                // Get the number immediately below
                string phoneNumber = currentItems[selectedIndex + 1];

                //12_01_2024
                // Create and show an instance of DbleInputForm for the user to enter contact values
                DbleInputfrm contactInputForm = new DbleInputfrm();
                contactInputForm.Text = "Contact Data";
                contactInputForm.TextBoxInput1.Text = selectedName;
                contactInputForm.TextBoxInput2.Text = phoneNumber;
                contactInputForm.ShowDialog();

                //13_01_2024
                if (!string.IsNullOrEmpty(contactInputForm.UserInput1) & !string.IsNullOrEmpty(contactInputForm.UserInput2))
                {
                    // Check if the name is valid
                    if (IsValidName(contactInputForm.UserInput1))
                    {
                        // Check if the number is valid
                        if (IsValidPhoneNumber(contactInputForm.UserInput2))
                        {
                            // Modify the name in currentItems
                            currentItems[selectedIndex] = contactInputForm.UserInput1;

                            // Modify the number in currentItems
                            currentItems[selectedIndex + 1] = contactInputForm.UserInput2;

                            // You can update the ListBox with the new data if necessary
                            UpdateListBox();
                        }
                        else
                        {
                            // Handle the case where the entered number is not valid
                            MessageBox.Show("The entered number is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        // Handle the case where the entered name is not valid
                        MessageBox.Show("The entered name is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("An item must be selected!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateListBox()
        {
            // Update the ListBox with the items from currentItems
            BlockedList.Items.Clear();
            foreach (string item in currentItems)
            {
                BlockedList.Items.Add(item);
            }

            // Filter odd items and update ListBox
            FilterOddItems();
            BlockedList.Items.Clear();
            BlockedList.Items.AddRange(oddItems.ToArray());
        }

        private void BlockRemove_Click(object sender, EventArgs e)
        {
            if (BlockedList.SelectedIndex != -1)
            {
                string selectedName = BlockedList.SelectedItem.ToString();
                int selectedIndex = currentItems.IndexOf(selectedName);

                // Get the number immediately below
                string phoneNumber = currentItems[selectedIndex + 1];

                // Remove selected name and the associated number
                currentItems.RemoveAt(selectedIndex);
                currentItems.Remove(phoneNumber);

                // Update ListBox
                UpdateListBox();
            }
            else
            {
                MessageBox.Show("An item must be selected!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BlockAdd_Click(object sender, EventArgs e)
        {
            //13_01_2024
            // Create and show an instance of DbleInputForm for the user to enter contact values
            DbleInputfrm contactInputForm = new DbleInputfrm();
            contactInputForm.Text = "Contact Data";
            contactInputForm.TextBoxInput1.Text = "Name";
            contactInputForm.TextBoxInput2.Text = "Number";
            contactInputForm.delatfrstclic = true;
            contactInputForm.ShowDialog();

            if (!string.IsNullOrEmpty(contactInputForm.UserInput1) && !string.IsNullOrEmpty(contactInputForm.UserInput2))
            {
                // Get the name entered by the user
                string blockedName = contactInputForm.UserInput1;

                // Get the number entered by the user
                string blockedNumber = contactInputForm.UserInput2;

                // Check if the name is valid
                if (IsValidName(blockedName))
                {
                    // Check if the number is valid
                    if (IsValidPhoneNumber(blockedNumber))
                    {
                        // Clean the number by removing invalid characters
                        blockedNumber = CleanNumber(blockedNumber);

                        // Add the name and number to the blocked list in botConfig
                        currentItems.Add(blockedName);
                        currentItems.Add(blockedNumber);

                        // Update ListBox with the new items
                        UpdateListBox();
                    }
                    else
                    {
                        // Handle the case where the entered number is not valid
                        MessageBox.Show("The entered number is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Handle the case where the entered name is not valid
                    MessageBox.Show("The entered name is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            // Update botConfig with the current items and apply changes in the main form
            BotConfig.blocked = currentItems.ToList();
            ParentForm.ApplyChanges();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            // Trigger the DoubleClick event to edit the selected item
            BlockedList_DoubleClick(this, EventArgs.Empty);
        }

        private void BlockedList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Show the context menu
                contextMenuStrip1.Show(BlockedList, e.Location);
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Trigger the DoubleClick event to edit the selected item
            BlockedList_DoubleClick(this, EventArgs.Empty);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Trigger the Remove Click event to delete the selected item
            BlockRemove_Click(this, EventArgs.Empty);
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Trigger the Add Click event to add a new item
            BlockAdd_Click(this, EventArgs.Empty);
        }

        private void peekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BlockedList.SelectedIndex != -1)
            {
                // Peek at the selected item's number
                string selectedName = BlockedList.SelectedItem.ToString();
                int selectedIndex = currentItems.IndexOf(selectedName);

                // Get the number immediately below
                string phoneNumber = currentItems[selectedIndex + 1];

                // Show another form to get the new number
                Inputfrm numberInputForm = new Inputfrm();
                numberInputForm.Text = "Contact Number";
                numberInputForm.TextBoxInput.Text = phoneNumber;
                numberInputForm.ShowDialog();

                if (!string.IsNullOrEmpty(numberInputForm.UserInput))
                {
                    // Modify the number in currentItems
                    currentItems[selectedIndex + 1] = numberInputForm.UserInput;

                    // You can update the ListBox with the new data if necessary
                    UpdateListBox();
                }
            }
            else
            {
                MessageBox.Show("An item must be selected!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}