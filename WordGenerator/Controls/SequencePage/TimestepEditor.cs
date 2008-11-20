using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DataStructures;

namespace WordGenerator.Controls
{
    public partial class TimestepEditor : UserControl
    {

        private bool marked;

        public bool Marked
        {
            get { return marked; }
            set { 
                marked = value;
                updateBackColor(false);
            }
        }

        /// <summary>
        /// Call this function on each timestep editor after a change in the sequence mode, to have the editor update its enabled/disabled and show/hide
        /// </summary>
        public void refreshButtonsAfterSequenceModeChange()
        {
            bool nowEnabled = (this.enabledButton.Text == "Enabled");

            if (nowEnabled != stepData.StepEnabled)
            {
                layoutEnableButton();
            }


            layoutShowhideButton();
        }

        public void updateBackColor(bool currentlyOutput)
        {
            if (Marked)
            {
                this.BackColor = Color.Salmon;
            }
            else
            {
                this.BackColor = Color.Transparent;
            }

            if (currentlyOutput)
            {
                this.BackColor = Color.DarkGray;
            }

            if (currentlyOutput && Marked)
            {
                this.BackColor = Color.DarkRed;
            }

        }

        public const int TimestepEditorWidth = 86;
        public const int TimestepEditorHeight = 215;

        private TimeStep stepData;

        public TimeStep StepData
        {
            get { return stepData; }
        }

        public event EventHandler updateGUI;

        public EventHandler messageLog;

        /// <summary>
        /// This is the number DISPLAYED above the timestep. Note that the actualy timestep index is this number MINUS ONE.
        ///
        /// </summary>
        private int stepNumber;

        public int StepNumber
        {
            get { return stepNumber; }
            set
            {
                stepNumber = value;
                redrawStepNumberLabel(stepData, stepNumber);
                
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
           base.OnPaint(e);
        }

        


        public TimestepEditor()
        {
            InitializeComponent();
            stepData = new TimeStep();
            analogSelector.Items.Add("Continue");
            analogSelector.SelectedItem = "Continue";

            gpibSelector.Items.Add("Continue");
            gpibSelector.SelectedItem = "Continue";

            rs232Selector.Items.Add("Continue");
            rs232Selector.SelectedItem = "Continue";

            durationEditor.setMinimumAllowableManualValue(0);

            this.Width = TimestepEditorWidth;
            this.Height = TimestepEditorHeight;

        }

        public TimestepEditor(TimeStep stepData, int timeStepNumber) : this()
        {
            if (stepData!=null)
                this.stepData = stepData;

            updateDescriptionTooltip(stepData);
            descriptionTextBox.Text = stepData.Description;

            this.stepNumber = timeStepNumber;

            timestepName.Text = stepData.StepName;

            this.durationEditor.setParameterData(stepData.StepDuration);
            redrawStepNumberLabel(stepData, timeStepNumber);

            analogSelector.Items.AddRange(Storage.sequenceData.AnalogGroups.ToArray());
            gpibSelector.Items.AddRange(Storage.sequenceData.GpibGroups.ToArray());
            rs232Selector.Items.AddRange(Storage.sequenceData.RS232Groups.ToArray());

            if (stepData.AnalogGroup != null)
            {
                this.analogSelector.SelectedItem = stepData.AnalogGroup;
                this.analogSelector.BackColor = Color.White;
            }
            else
            {
                this.analogSelector.SelectedItem = "Continue";
                this.analogSelector.BackColor = Color.Green;
            }


            if (stepData.GpibGroup != null)
            {
                this.gpibSelector.SelectedItem = stepData.GpibGroup;
                this.gpibSelector.BackColor = Color.White;
            }
            else
            {
                this.gpibSelector.SelectedItem = "Continue";
                this.gpibSelector.BackColor = Color.Green;
            }

            if (stepData.rs232Group != null)
            {
                rs232Selector.SelectedItem = stepData.rs232Group;
                rs232Selector.BackColor = Color.White;
            }
            else
            {
                rs232Selector.SelectedItem = "Continue";
                rs232Selector.BackColor = Color.Green;
            }


            layoutEnableButton();
            layoutShowhideButton();
            updatePulsesIndicator();
        }

        private void redrawStepNumberLabel(TimeStep stepData, int timeStepNumber)
        {
            this.timeStepNumber.Text = timeStepNumber.ToString();

            if (stepData.HotKeyCharacter != 0)
                this.timeStepNumber.Text += " {" + char.ToUpper(stepData.HotKeyCharacter) + "}";
        }

        private void updateDescriptionTooltip(TimeStep stepData)
        {
            toolTip1.SetToolTip(this.timeStepNumber, stepData.Description);
            toolTip1.SetToolTip(this.timestepName, stepData.Description);
        }


        private void timestepName_TextChanged(object sender, EventArgs e)
        {
            stepData.StepName = timestepName.Text;
        }

        private void enabledButton_Click(object sender, EventArgs e)
        {
            stepData.StepEnabled = !stepData.StepEnabled;

            layoutEnableButton();
            if (updateGUI != null)
                updateGUI(sender, e);
        }

        private void layoutEnableButton()
        {
            if (stepData.StepEnabled)
            {
                enabledButton.Text = "Enabled";
                enabledButton.BackColor = Color.Green;
            }
            else
            {
                enabledButton.Text = "Disabled";
                enabledButton.BackColor = Color.Red;
            }
            enabledButton.Invalidate();
        }

        private void showHideButton_Click(object sender, EventArgs e)
        {
            stepData.StepHidden = !stepData.StepHidden;

            layoutShowhideButton();

            if (Storage.sequenceData.stepHidingEnabled)
            {
                WordGenerator.mainClientForm.instance.sequencePage1.showOrHideHiddenTimestepEditors();
                WordGenerator.mainClientForm.instance.sequencePage1.layoutTheRest();
            }
        }

        private void layoutShowhideButton()
        {
            if (stepData.StepHidden)
            {
                showHideButton.Text = "Hidden";
                showHideButton.BackColor = Color.DarkKhaki;
            }
            else
            {
                showHideButton.Text = "Visible";
                showHideButton.BackColor = Color.Transparent;
            }
        }

        private void analogSelector_DropDown(object sender, EventArgs e)
        {
            analogSelector.Items.Clear();
            analogSelector.BackColor = Color.White;
            analogSelector.Items.Add("Continue");
            analogSelector.Items.AddRange(Storage.sequenceData.AnalogGroups.ToArray());
        }

        private object analogSelectorBackupItem;

        private void analogSelector_SelectedValueChanged(object sender, EventArgs e)
        {
            if (analogSelector.SelectedItem.ToString() == "Continue")
            {
                analogSelector.BackColor = Color.Green;
                toolTip1.SetToolTip(analogSelector, "Continue previous analog group.");
            }
            else
                analogSelector.BackColor = Color.White;

            if (stepData != null)
            {
                AnalogGroup ag = analogSelector.SelectedItem as AnalogGroup;
                stepData.AnalogGroup = ag;
                if (updateGUI != null)
                {
                    updateGUI(sender, e);
                }
                if (ag != null)
                    toolTip1.SetToolTip(analogSelector, ag.GroupDescription);

            }
            analogSelectorBackupItem = analogSelector.SelectedItem;
        }

        private object gpibSelectorBackupItem;

        private void gpibSelector_SelectedValueChanged(object sender, EventArgs e)
        {
            if (gpibSelector.SelectedItem.ToString() == "Continue")
            {
                gpibSelector.BackColor = Color.Green;
                toolTip1.SetToolTip(gpibSelector, "Continue previous GPIB group.");
            }
            else
                gpibSelector.BackColor = Color.White;

            if (stepData != null)
            {
                GPIBGroup gg = gpibSelector.SelectedItem as GPIBGroup;
                stepData.GpibGroup = gg;
                if (updateGUI != null)
                    updateGUI(sender, e);
                if (gg != null)
                    toolTip1.SetToolTip(gpibSelector, gg.GroupDescription);
            }

            gpibSelectorBackupItem = gpibSelector.SelectedItem;
        }

        private void gpibSelector_DropDown(object sender, EventArgs e)
        {
            gpibSelector.Items.Clear();
            gpibSelector.BackColor = Color.White;
            gpibSelector.Items.Add("Continue");
            gpibSelector.Items.AddRange(Storage.sequenceData.GpibGroups.ToArray());
        }

        private void outputNowToolStripMenuItem_Click(object sender, EventArgs e)
        {

            outputTimestepNow(false, true);

        }

        public void unRegsiterHotkey()
        {
            if (stepData.HotKeyCharacter != 0)
            {
                WordGenerator.mainClientForm.instance.unregisterHotkey(stepData.HotKeyCharacter, this);
            }
        }

        public void registerHotkey()
        {
            if (stepData.HotKeyCharacter != 0)
            {
                WordGenerator.mainClientForm.instance.registerTimestepHotkey(stepData.HotKeyCharacter, this);
            }
        }

        public void updatePulsesIndicator()
        {
            if (stepData == null)
            {
                pulseIndicator.Visible = false;
                return;
            }

            if (stepData.usesPulses())
            {
                pulseIndicator.Visible = true;
            }
            else
            {
                pulseIndicator.Visible = false;
            }

        }

        public bool outputTimestepNow()
        {
            return outputTimestepNow(false, false);
        }

        /// <summary>
        /// outputs the editor's timestep. set silent to true if no message logs should be generated.
        /// </summary>
        /// <param name="silent"></param>
        /// <returns></returns>
        public bool outputTimestepNow(bool silent, bool showErrorDialog)
        {
            List<string> unconnectedServers = Storage.settingsData.unconnectedRequiredServers();

            if (!Storage.sequenceData.Lists.ListLocked)
            {
                if (!silent)
                    messageLog(this, new MessageEvent("Unable to output timestep, lists not locked."));
                if (showErrorDialog)
                {
                    MessageBox.Show("Unable to output timestep, lists not locked.");
                }
                return false;
            }

            if (unconnectedServers.Count == 0)
            {

                WordGenerator.mainClientForm.instance.cursorWait();



                ServerManager.ServerActionStatus actionStatus = Storage.settingsData.serverManager.outputSingleTimestepOnConnectedServers(
                    Storage.settingsData,
                    Storage.sequenceData.getSingleOutputFrameAtEndOfTimestep(this.stepNumber - 1, Storage.settingsData, Storage.settingsData.OutputAnalogDwellValuesOnOutputNow),
                    messageLog);

                WordGenerator.mainClientForm.instance.cursorWaitRelease();

                if (actionStatus == ServerManager.ServerActionStatus.Success)
                {
                    if (!silent)
                        messageLog(this, new MessageEvent("Successfully output timestep " + stepData.ToString()));
                    WordGenerator.mainClientForm.instance.CurrentlyOutputtingTimestep = this.stepData;
                    return true;
                }
                else
                {
                   
                    if (!silent)
                        messageLog(this, new MessageEvent("Communication or server error attempting to output this timestep: " + actionStatus.ToString()));
                    if (showErrorDialog)
                    {
                        MessageBox.Show("Communication or server error attempting to output this timestep: " + actionStatus.ToString());
                    }
                }
            }
            else
            {
                string missingServerList = ServerManager.convertListOfServersToOneString(unconnectedServers);
                if (!silent)
                    messageLog(this, new MessageEvent("Unable to output this timestep. The following required servers are not connected: " + missingServerList));
            
                if (showErrorDialog) {
                    MessageBox.Show("Unable to output this timestep. The following required servers are not connected: " + missingServerList);
                }
            }
            return false;
        }

        private void insertTimestepBeforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TimeStep newStep = new TimeStep("New timestep.");
            Storage.sequenceData.TimeSteps.Insert(stepNumber-1, newStep );
            Storage.sequenceData.populateWithChannels(Storage.settingsData);

            TimestepEditor te = new TimestepEditor(newStep, stepNumber);

            WordGenerator.mainClientForm.instance.sequencePage1.insertTimestepEditor(te, stepNumber - 1);


        //    WordGenerator.mainClientForm.instance.RefreshSequenceDataToUI(Storage.sequenceData);
        //    WordGenerator.mainClientForm.instance.sequencePage1.scrollToTimestep(newStep);
        }

        private void insertTimestepAfterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TimeStep newStep = new TimeStep("New timestep.");
            Storage.sequenceData.TimeSteps.Insert(stepNumber, newStep);
            Storage.sequenceData.populateWithChannels(Storage.settingsData);

            WordGenerator.mainClientForm.instance.sequencePage1.insertTimestepEditor(
                new TimestepEditor(newStep, stepNumber + 1), stepNumber);
 

        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TimeStep newStep = new TimeStep(this.stepData);
            Storage.sequenceData.TimeSteps.Insert(stepNumber, newStep);

            WordGenerator.mainClientForm.instance.sequencePage1.insertTimestepEditor(
                new TimestepEditor(newStep, stepNumber + 1), stepNumber);

        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Storage.sequenceData.TimeSteps.RemoveAt(stepNumber-1);

            WordGenerator.mainClientForm.instance.sequencePage1.removeTimestepEditor(this);
        }

        private void durationEditor_updateGUI(object sender, EventArgs e)
        {
            if (updateGUI != null)
                updateGUI(sender, e);
        }

        private void analogSelector_DropDownClosed(object sender, EventArgs e)
        {
            if (analogSelector.SelectedItem == null)
                analogSelector.SelectedItem = analogSelectorBackupItem;

        }

        private void gpibSelector_DropDownClosed(object sender, EventArgs e)
        {
            if (gpibSelector.SelectedItem == null)
                gpibSelector.SelectedItem = gpibSelectorBackupItem;
        }

 

 

        private void removeTimestepHotkeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (stepData.HotKeyCharacter != 0)
            {
                unRegsiterHotkey();
                stepData.HotKeyCharacter = (char) 0;
                WordGenerator.mainClientForm.instance.RefreshSequenceDataToUI(Storage.sequenceData);
            }
        }

        private void rs232Selector_DropDown(object sender, EventArgs e)
        {

            rs232Selector.Items.Clear();
            rs232Selector.BackColor = Color.White;
            rs232Selector.Items.Add("Continue");
            rs232Selector.Items.AddRange(Storage.sequenceData.RS232Groups.ToArray());

        }

        private void rs232Selector_DropDownClosed(object sender, EventArgs e)
        {
            if (rs232Selector.SelectedItem == null)
                rs232Selector.SelectedItem = rs232SelectorBackupItem;
        }

        Object rs232SelectorBackupItem;

        private void rs232Selector_SelectedValueChanged(object sender, EventArgs e)
        {
            if (rs232Selector.SelectedItem.ToString() == "Continue")
            {
                rs232Selector.BackColor = Color.Green;
                toolTip1.SetToolTip(rs232Selector, "Continue previous RS232 group.");
            }
            else
                rs232Selector.BackColor = Color.White;

            if (stepData != null)
            {
                RS232Group gg = rs232Selector.SelectedItem as RS232Group;
                stepData.rs232Group = gg;
                if (updateGUI != null)
                    updateGUI(sender, e);
                if (gg != null)
                    toolTip1.SetToolTip(rs232Selector, gg.GroupDescription);
            }

            rs232SelectorBackupItem = rs232Selector.SelectedItem;
        }

        private void hotkeyEntryTextBox_TextChanged(object sender, EventArgs e)
        {
            if (hotkeyEntryTextBox.Text.Length > 0)
            {
                char hotkeyChar = hotkeyEntryTextBox.Text[0];
                if (char.IsLetter(hotkeyChar))
                {
                    setHotkeyChar(hotkeyChar);
                    contextMenuStrip1.Close();
                }
                else
                {
                    hotkeyEntryTextBox.Text = "";
                }


            }
        }

        private void setHotkeyChar(char hChar)
        {
            if (hChar != 0)
            {
                foreach (TimeStep step in Storage.sequenceData.TimeSteps)
                {
                    if ((step != stepData) && (step.HotKeyCharacter == hChar))
                    {
                        MessageBox.Show("That hotkey is already in use.");
                        return;
                    }
                }

                if (stepData.HotKeyCharacter != 0)
                    unRegsiterHotkey();

                stepData.HotKeyCharacter = hChar;
                WordGenerator.mainClientForm.instance.RefreshSequenceDataToUI(Storage.sequenceData);
            }
        }

        private void moveToTimestepCombobox_DropDown(object sender, EventArgs e)
        {
            moveToTimestepCombobox.Items.Clear();
            int lastStep = Storage.sequenceData.NTimeSteps;
            for (int i = 0; i < lastStep; i++)
            {
                moveToTimestepCombobox.Items.Add(i + 1);
            }
            moveToTimestepCombobox.SelectedItem = stepNumber;
        }

        private void moveToTimestepCombobox_DropDownClosed(object sender, EventArgs e)
        {
            if (moveToTimestepCombobox.SelectedItem is int)
            {
                moveTimestep((int)moveToTimestepCombobox.SelectedItem);
                contextMenuStrip1.Close();
            }
        }

        private void moveTimestep(int destination)
        {
            if (destination > 0)
            {
                if (destination <= Storage.sequenceData.TimeSteps.Count)
                {
                    int destinationIndex = destination - 1;
                    int currentIndex = this.stepNumber - 1;

                    Storage.sequenceData.TimeSteps.RemoveAt(currentIndex);
                    Storage.sequenceData.TimeSteps.Insert(destinationIndex, this.stepData);

                    WordGenerator.mainClientForm.instance.sequencePage1.moveTimestepEditor(currentIndex, destinationIndex);

                }
            }
        }

        private void agLabel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (stepData.AnalogGroup != null)
            {
                WordGenerator.mainClientForm.instance.activateAnalogGroupEditor(stepData.AnalogGroup);
            }
        }

        private void ggLabel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (stepData.GpibGroup != null)
            {
                WordGenerator.mainClientForm.instance.activateGPIBGroupEditor(stepData.GpibGroup);
            }
        }

        private void rgLabel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (stepData.rs232Group != null)
            {
                WordGenerator.mainClientForm.instance.activateRS232GroupEditor(stepData.rs232Group);
            }
        }

        private void descriptionTextChanged(object sender, EventArgs e)
        {
            stepData.Description = descriptionTextBox.Text;
            updateDescriptionTooltip(stepData);
        }

        private void viewDescMenuItem_Click(object sender, EventArgs e)
        {
            if (stepData.Description != "")
            {
                MessageBox.Show(stepData.Description, "Timestep description.");
            }
            else
            {
                MessageBox.Show("This timestep has no description.");
            }
        }

        private void TimestepEditor_Enter(object sender, EventArgs e)
        {
            // Together, these solve a former problem that when a partially concealed timestep was selected, it would be
            // scrolled to, but a scroll event would not be raised by stupid stupid windows, causing the 
            // horizontal scroll bars on the sequence page to become out of sync.

            WordGenerator.mainClientForm.instance.sequencePage1.timeStepsPanel.ScrollControlIntoView(this);
            WordGenerator.mainClientForm.instance.sequencePage1.forceUpdateAllScrollbars();
        }

        private void mark_Click(object sender, EventArgs e)
        {
            this.Marked = true;
        }

        private void unmark_Click(object sender, EventArgs e)
        {
            this.Marked = false;
        }

        private void markall_Click(object sender, EventArgs e)
        {
            WordGenerator.mainClientForm.instance.sequencePage1.markAllTimesteps();
        }

        private void unmarkall_Click(object sender, EventArgs e)
        {
            WordGenerator.mainClientForm.instance.sequencePage1.unmarkAllTimesteps();
        }



    }
}