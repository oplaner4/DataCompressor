using System;

namespace DataCompressor.Parts
{
    public class ModesPart
    {

        private static readonly Tuple<bool, bool>[] IntegerModesCombinations =
            new Tuple<bool, bool>[]
            {
                new(false, false),
                new(true, false),
                new(false, true),
                new(true, true),
            };

        /// <summary>
        /// Byte representing set and unset modes.
        /// </summary>
        public byte ModesByte { get; private set; }

        /// <summary>
        /// Creates modes part and sets default modes.
        /// </summary>
        public ModesPart()
        {
            ModesByte = 0;

            foreach (Mode mode in Settings.DefaultModesOn)
            {
                SetOn(mode);
            }
        }

        /// <summary>
        /// Reconstructs modes part.
        /// </summary>
        /// <param name="modesByte">Byte to reconstruct from.</param>
        public ModesPart(byte modesByte)
        {
            ModesByte = modesByte;
        }

        public bool IsOff(Mode mode)
        {
            int value = ModesByte & (1 << Settings.ModeAndBit[mode]);
            return value == 0;
        }

        public bool IsOn(Mode mode)
        {
            return !IsOff(mode);
        }

        /// <summary>
        /// Sets specific mode on
        /// </summary>
        /// <param name="mode">Mode to set</param>
        /// <returns>Current instance to allow chaining</returns>
        public ModesPart SetOn(Mode mode)
        {
            int value = 1 << Settings.ModeAndBit[mode];
            ModesByte |= (byte)value;
            return this;
        }

        /// <summary>
        /// Sets specific mode off
        /// </summary>
        /// <param name="mode">Mode to set</param>
        /// <returns>Current instance to allow chaining</returns>
        public ModesPart SetOff(Mode mode)
        {
            int value = 1 << Settings.ModeAndBit[mode];
            ModesByte &= (byte)~value;
            return this;
        }

        /// <summary>
        /// Interprets bytes count based on short and int mode.
        /// </summary>
        /// <param name="shortModeOn">Short mode is on</param>
        /// <param name="intModeOn">Int mode is on</param>
        /// <returns>Bytes count</returns>
        public static int GetBytesCount(
            bool shortModeOn, bool intModeOn)
        {
            if (shortModeOn && intModeOn)
            {
                return 8;
            }

            if (!shortModeOn && !intModeOn)
            {
                return 1;
            }

            if (shortModeOn)
            {
                return 2;
            }

            return 4;
        }

        /// <summary>
        /// Tries to find minimal necessary count of bytes 
        /// so that using this count creation succeeds. It 
        /// determines how int and short mode should be set.
        /// It modifies Modes property.
        /// </summary>
        /// <param name="shortMode">Specific short mode
        /// </param>
        /// <param name="intMode">Specific int mode</param>
        /// <param name="createAction">Action to be 
        /// done with actual combination of short and 
        /// int mode. It returns true if successfully 
        /// created.</param>
        /// <returns>Successfully chosen</returns>
        public bool ChooseIntegerModes(
            Mode shortMode, Mode intMode,
            Func<bool> createAction)
        {
            foreach (Tuple<bool, bool> combination in
                IntegerModesCombinations)
            {
                if (combination.Item1)
                {
                    SetOn(shortMode);
                }
                else
                {
                    SetOff(shortMode);
                }

                if (combination.Item2)
                {
                    SetOn(intMode);
                }
                else
                {
                    SetOff(intMode);
                }

                if (createAction())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
