using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace KarKBD
{
    internal class Program
    {
        private static string _pathToFile;
        private static List<KBDButton> _buttons;
        private static List<Tuple<int, int>> _positions;
        private static byte[] _bytes;
        private static List<KBDType> _types;

        // position, overflow/track value
        private static List<int> bytePositions;

        [STAThread]
        public static void Main(string[] args)
        {
            OpenFile();
            bytePositions = new List<int>();
            _buttons = GetNoteButtons();
            _positions = GetNotePositions();
            _types = GetNoteTypes();

            Console.WriteLine("Note count: {0}", _buttons.Count);
            for (var i = 0; i < _buttons.Count; i++)
                switch (_buttons[i])
                {
                    case KBDButton.A:
                        Console.WriteLine("X/A POSITION: {0} | OVERFLOW: {1} | LOCATION: {2} | TYPE: {3}" , _positions[i].Item1,
                            _positions[i].Item2, ToHex(bytePositions[i]), _types[i]);
                        break;
                    case KBDButton.B:
                        Console.WriteLine("O/B: {0} | OVERFLOW: {1} | LOCATION: {2} | TYPE: {3}" , _positions[i].Item1,
                            _positions[i].Item2, ToHex(bytePositions[i]), _types[i]);
                        break;
                    case KBDButton.X:
                        Console.WriteLine("SQUARE/X: {0} | OVERFLOW: {1} | LOCATION: {2} | TYPE: {3}" , _positions[i].Item1,
                            _positions[i].Item2, ToHex(bytePositions[i]), _types[i]);
                        break;
                    case KBDButton.Y:
                        Console.WriteLine("TRIANGLE/Y: {0} | OVERFLOW: {1} | LOCATION: {2} | TYPE: {3}" , _positions[i].Item1,
                            _positions[i].Item2, ToHex(bytePositions[i]), _types[i]);
                        break;
                }

            GetNoteButtons();
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }

        private static void OpenFile()
        {
            var dialogBox = new OpenFileDialog();
            dialogBox.Title = "Open karaoke bin file";
            dialogBox.Filter = "Dragon Engine Karaoke .kbd|*.kbd";
            dialogBox.InitialDirectory = @"C:\";
            if (dialogBox.ShowDialog() == DialogResult.OK)
            {
                _pathToFile = dialogBox.FileName;
                Console.WriteLine(dialogBox.FileName);
            }
            else
            {
                Console.Error.WriteLine("ERROR 00: Could not find file");
            }

            if (File.Exists(_pathToFile))
            {
                var reader = new BinaryReader(new FileStream(_pathToFile, FileMode.Open, FileAccess.ReadWrite));
                reader.BaseStream.Position = 0x00;
                _bytes = reader.ReadBytes((int) reader.BaseStream.Length);
                reader.Close();
            }
        }


        private static List<KBDButton> GetNoteButtons()
        {
            var buttonList = new List<KBDButton>();
            var startingPos = 0x2C;
            var jumpLength = 0x20;
            for (var i = startingPos; i < _bytes.Length; i += jumpLength) buttonList.Add(FindButtonType(_bytes[i]));
            return buttonList;
        }

        private static List<Tuple<int, int>> GetNotePositions()
        {
            var positionList = new List<Tuple<int, int>>();
            var startingPos = 0x1D;
            var jumpLength = 0x20;
            for (var i = startingPos; i < _bytes.Length; i += jumpLength)
            {
                bytePositions.Add(i);
                positionList.Add(new Tuple<int, int>(_bytes[i], _bytes[i + 1]));
            }

            return positionList;
        }
        
        // TODO: Make this into a tuple for getting note type and length if special note in the future
        private static List<KBDType> GetNoteTypes()
        {
            var typeList = new List<KBDType>();
            var startingPos = 0x30;
            var jumpLength = 0x20;
            for (var i = startingPos; i < _bytes.Length; i+=jumpLength)
            {
                switch (_bytes[i])
                {
                    
                    case 0x00:
                        typeList.Add(KBDType.REGULAR);
                        break;
                    case 0x01:
                        typeList.Add(KBDType.HOLD);
                        break;
                    case 0x02:
                        typeList.Add(KBDType.RAPID);
                        break;
                    default:
                        typeList.Add(KBDType.ERROR);
                        break;
                }
            }

            return typeList;
        }

        private static KBDButton FindButtonType(byte b)
        {
            switch (b)
            {
                case 0x00:
                    return KBDButton.B;
                case 0x01:
                    return KBDButton.A;
                case 0x02:
                    return KBDButton.X;
                case 0x03:
                    return KBDButton.Y;
                default:
                    Console.Error.WriteLine("ERROR 01: COULD NOT FIND BUTTON TYPE. TYPE RETURNED = {0}", b);
                    return KBDButton.NULL;
            }
        }

        public static string ToHex(int value)
        {
            return string.Format("0x{0:X}", value);
        }

        private enum KBDButton : byte
        {
            B = 0x00,
            A = 0x01,
            X = 0x02,
            Y = 0x03,
            NULL = 0x04
        }

        private enum KBDType : byte
        {
            REGULAR = 0x00,
            HOLD = 0x01,
            RAPID = 0x02,
            ERROR = 0xFF
        }
    }
}