using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Basler.Pylon;

namespace PylonLiveViewControl
{
    // Displays a slider bar, the name of the node, minimum, maximum, and current value.
    public partial class IntSliderUserControl : UserControl
    {

        // Sets up the initial state.
        public IntSliderUserControl()
        {
            InitializeComponent();
            Reset();
        }

        private IIntegerParameter parameter = null; // The interface of the integer parameter.
        private string defaultName = "N/A";


        // Sets the parameter displayed by the user control.
        public IIntegerParameter Parameter
        {
            set
            {
                // Remove the old parameter.
                if (parameter != null)
                {
                    parameter.ParameterChanged -= ParameterChanged;
                }

                // Set the new parameter.
                parameter = value;
                if (parameter != null)
                {
                    parameter.ParameterChanged += ParameterChanged;
                    labelName.Text = parameter.Advanced.GetPropertyOrDefault(AdvancedParameterAccessKey.DisplayName, parameter.Name);
                    UpdateValues();
                }
                else
                {
                    labelName.Text = defaultName;
                    Reset();
                }
            }
        }


        // Sets the default name of the control.
        public string DefaultName
        {
            set
            {
                defaultName = value;
                if (parameter == null)
                {
                    labelName.Text = defaultName;
                }
            }
            get
            {
                return defaultName;
            }
        }


        // The parameter state changed. Update the control.
        private void ParameterChanged(Object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                BeginInvoke(new EventHandler<EventArgs>(ParameterChanged),sender, e);
                return;
            }
            try
            {
                UpdateValues();
            }
            catch
            {
                // If errors occurred disable the control.
                Reset();
            }
        }


        // Deactivate the control.
        private void Reset()
        {
            slider.Enabled = false;
            labelMin.Enabled = false;
            labelMax.Enabled = false;
            labelName.Enabled = false;
            labelCurrentValue.Enabled = false;
        }


        // Get the current values from the parameter and display them.
        private void UpdateValues()
        {
            try
            {
                if (parameter != null)
                {
                    if (parameter.IsReadable)  // Check if parameter is accessible.
                    {
                        // Get values.
                        int min = checked((int)parameter.GetMinimum());
                        int max = checked((int)parameter.GetMaximum());
                        int val = checked((int)parameter.GetValue());
                        int inc = checked((int)parameter.GetIncrement());

                        // Update the slider.
                        slider.Minimum = min;
                        slider.Maximum = max;
                        slider.Value = val;
                        slider.SmallChange = inc;
                        slider.TickFrequency = (max - min + 5) / 10;

                        // Update the displayed values.
                        labelMin.Text = "" + min;
                        labelMax.Text = "" + max;
                        labelCurrentValue.Text = "" + val;

                        // Update accessibility.
                        slider.Enabled = parameter.IsWritable;
                        labelMin.Enabled = true;
                        labelMax.Enabled = true;
                        labelName.Enabled = true;
                        labelCurrentValue.Enabled = true;

                        return;
                    }
                }
            }
            catch
            {
                // If errors occurred disable the control.
            }
            Reset();
        }


        // Handle slider position changes.
        private void slider_Scroll(object sender, EventArgs e)
        {
            if (parameter != null)
            {
                try
                {
                    // Set the value if writable.
                    parameter.TrySetValue(slider.Value, IntegerValueCorrection.Nearest);
                }
                catch
                {
                    // Ignore any errors here.
                }
            }
        }
    }
}
