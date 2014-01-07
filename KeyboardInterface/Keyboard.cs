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

namespace KeyboardInterface
{
    public class Keyboard
    {
        private Device keyboardDevice;
        private KeyboardState state;
        private IntPtr hWnd;

        public bool keyboardUpdated = false;

        //Keyboard constructor
        public Keyboard(IntPtr window_handle)
        {
            hWnd = window_handle;
        }

        private void Poll()
        {
            try
            {
                // poll the keyboard
                keyboardDevice.Poll();

                // update the keyboard state field
                state = keyboardDevice.GetCurrentKeyboardState();
            }
            catch (Exception err)
            {
                throw new ArgumentException(err.ToString());
            }
        }


        public void InitKeyboard()
        {
            try
            {
                keyboardDevice = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
                keyboardDevice.SetCooperativeLevel(hWnd, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                keyboardDevice.Acquire();
            }
            catch (Exception err)
            {
                throw new ArgumentException(err.ToString());
            }
        }

        // Release keyboard
        public void ReleaseKeyboard()
        {
            keyboardDevice.Unacquire();
        }

        // Get current keyboard state
        public void UpdateStatus()
        {
            Poll();

            if (state[Key.Delete])
            {
                //angle += 0.03f;
            }
            if (state[Key.RightArrow])
            {
                //angle -= 0.03f;
            }
        }
    }
}
