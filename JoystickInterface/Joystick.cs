/******************************************************************************
 * C# Joystick Library - Copyright (c) 2006 Mark Harris - MarkH@rris.com.au
 ******************************************************************************
 * You may use this library in your application, however please do give credit
 * to me for writing it and supplying it. If you modify this library you must
 * leave this notice at the top of this file. I'd love to see any changes you
 * do make, so please email them to me :)
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectInput;

namespace JoystickInterface
{
    public class Joystick
    {
        private Device joystickDevice;
        private JoystickState state;
        private IntPtr hWnd;

        private int JOYSTICK_THRESHOLD;
        public bool joystickUpdated = false;
        public bool enabled = false;

        public int axisCount = 0;
        public int axisX1 = 0;
        public int axisY1 = 0;
        public int axisX2 = 0;
        public int axisY2 = 0;
        public int axisZ = 0;
        public int axisR = 0;

        public int povCount = 0;
        public int povPos = 0;

        public int buttonCount = 0;
        public bool[] buttons = null;

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="window_handle">Handle of the window which the joystick will be "attached" to.</param>
        /// <param name="joystickThreshold">When to update things with new joystick parameters</param>
        public Joystick(IntPtr window_handle, int joystickThreshold)
        {
            hWnd = window_handle;
            JOYSTICK_THRESHOLD = joystickThreshold;
        }

        private void Poll()
        {
            try
            {
                // poll the joystick
                joystickDevice.Poll();

                // update the joystick state field
                state = joystickDevice.CurrentJoystickState;
            }
            catch (Exception err)
            {
                throw new ArgumentException(err.ToString());
            }
        }

        // See that there is at least one joystick
        public int FindJoysticks()
        {
            int numberOfJoysticks = 0;
            try
            {
                // Find all the GameControl devices that are attached.
                DeviceList gameControllerList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);

                // check that we have at least one device.
                numberOfJoysticks = gameControllerList.Count;
            }
            catch (Exception err)
            {
                throw new ArgumentException(err.ToString());
            }

            //deactivate joystick object if no joysticks
            if (numberOfJoysticks > 0)
                enabled = true;

            return numberOfJoysticks;
        }

        // Aquire the joystick
        public bool AcquireJoystick()
        {
            try
            {
                DeviceList gameControllerList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
                gameControllerList.MoveNext();
                DeviceInstance deviceInstance = (DeviceInstance)gameControllerList.Current;

                // create a device from this controller so we can retrieve info.
                joystickDevice = new Device(deviceInstance.InstanceGuid);
                joystickDevice.SetCooperativeLevel(hWnd, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            
                // Tell DirectX that this is a Joystick.
                joystickDevice.SetDataFormat(DeviceDataFormat.Joystick);

                // Finally, acquire the device.
                joystickDevice.Acquire();

                // Find the capabilities of the joystick
                DeviceCaps cps = joystickDevice.Caps;
                axisCount = cps.NumberAxes;
                povCount = cps.NumberPointOfViews;
                buttonCount = cps.NumberButtons;

                // Update Status
                UpdateStatus();
            }
            catch (Exception err)
            {
                 throw new ArgumentException(err.ToString());
            }

            return true;
        }

        // Release joystick
        public void ReleaseJoystick()
        {
            joystickDevice.Unacquire();
        }

        // Get current joystick state
        public void UpdateStatus()
        {
            //Poll Joystick
            Poll();

            //see if things have changed more than the threshhold
            /*
            if (JOYSTICK_THRESHOLD == 0)
                joystickUpdated = true;
            else
            {
                joystickUpdated = false;
                if ((axisA < (state.Rz - JOYSTICK_THRESHOLD)) || (axisA > (state.Rz - JOYSTICK_THRESHOLD)))
                    joystickUpdated = true;
                if ((axisC < (state.X - JOYSTICK_THRESHOLD)) || (axisC > (state.X - JOYSTICK_THRESHOLD)))
                    joystickUpdated = true;
                if ((axisD < (state.Y - JOYSTICK_THRESHOLD)) || (axisD > (state.Y - JOYSTICK_THRESHOLD)))
                    joystickUpdated = true;
                if ((axisB < (state.GetSlider()[0] - JOYSTICK_THRESHOLD)) || (axisB < (state.GetSlider()[0] - JOYSTICK_THRESHOLD)))
                    joystickUpdated = true;
                if ((povPos < (state.GetPointOfView()[0] - JOYSTICK_THRESHOLD)) || (povPos > (state.GetPointOfView()[0] - JOYSTICK_THRESHOLD)))
                    joystickUpdated = true;
            }
             * */

            //if threshold has not been passed, just return
            //if (!joystickUpdated)
            //    return;

            //If game pad, use different mapping
            if(joystickDevice.DeviceInformation.InstanceName == "Controller (TSZ360 Pad)")  //xbox
            {
                axisR = state.Rx;
                axisZ = state.Z;
                axisX1 = state.X;
                axisY1 = state.Y;
            }
            else  //joystick
            {
                axisR = state.Rz;
                axisZ = state.GetSlider()[0]; //slider is funky
                axisX1 = state.X;
                axisY1 = state.Y;
            }

             //get POV
            int[] povPosArray = state.GetPointOfView();
            povPos = povPosArray[0];

            // not using buttons, so don't take the tiny amount of time it takes to get/parse
            byte[] jsButtons = state.GetButtons();
            buttons = new bool[jsButtons.Length];

            int i = 0;
            foreach (byte button in jsButtons)
            {
                buttons[i] = button >= 128;
                i++;
            }
        }
    }
}
