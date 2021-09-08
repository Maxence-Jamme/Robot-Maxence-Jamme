using ExtendedSerialPort;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;

namespace Robot_Interface_JAMME_JUILLE
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        ReliableSerialPort serialPort1;
        AsyncCallback SerialPort1_DataRecived;
        DispatcherTimer timerAffichage;

        int i;
        int couleur;
        int couleur_2 = 1;
        bool autoControlActivated = true;
        Robot robot = new Robot();


        public MainWindow()
        {
            InitializeComponent();
            serialPort1 = new ReliableSerialPort("COM4", 115200, Parity.None, 8, StopBits.One);
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();

            timerAffichage = new DispatcherTimer();
            timerAffichage.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerAffichage.Tick += TimerAffichage_Tick;
            timerAffichage.Start();
            Switch_color();
        }

        private void TimerAffichage_Tick(object sender, EventArgs e)        // peut etre faut à voir
        {
            /*if (robot.receivedText != "")
            {
                TextBoxReception.Text = TextBoxReception.Text + "Reçu=" + robot.receivedText;
                robot.receivedText = "";
            }*/
            while (robot.byteListReceived.Count != 0)
            {
                byte byteReceived = robot.byteListReceived.Dequeue();
                DecodeMessage(byteReceived);
                //string blabla;
                //blabla = byteReceived.ToString("X");
                //blabla += ;
                //TextTest.Text += byteReceived.ToString("X");
                //TextTest.Text += Convert.ToChar(byteReceived) + " "; // "0x"+blabla+"";
                //TextBoxReception.Text += byteReceived+"\n";
                //TextBoxReception.Text += Convert.ToChar(byteReceived);
            }

        }

        byte CalculateChecksum(int msgFunction, int msgPayLoadLength, byte[] msgPayLoad)
        {
            byte Checksum = 0;
            Checksum ^= (byte)(0xFE);
            Checksum ^= (byte)(msgFunction >> 8);
            Checksum ^= (byte)(msgFunction >> 0);
            Checksum ^= (byte)(msgPayLoadLength >> 8);
            Checksum ^= (byte)(msgPayLoadLength >> 0);
            for (int i = 0; i < msgPayLoadLength; i++)
            {
                Checksum ^= (byte)msgPayLoad[i];
            }
            return Checksum;
        }

        void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte Checksum = 0;

            byte[] trame = new byte[msgPayloadLength + 6];
            int pos = 0;
            trame[pos++] = (byte)(0xFE);
            trame[pos++] = (byte)(msgFunction >> 8);
            trame[pos++] = (byte)(msgFunction >> 0);
            trame[pos++] = (byte)(msgPayloadLength >> 8);
            trame[pos++] = (byte)(msgPayloadLength >> 0);
            for (int i = 0; i < msgPayloadLength; i++)
            {
                trame[pos++] = (byte)msgPayload[i];
            }
            Checksum = CalculateChecksum(msgFunction, msgPayloadLength, msgPayload);

            trame[pos++] = (byte)(Checksum);
            serialPort1.Write(trame, 0, trame.Length);
        }

        private void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {
            //robot.receivedText += Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);
            for (i = 0; i < e.Data.Length; i++)
            {
                robot.byteListReceived.Enqueue(e.Data[i]);
            }

        }

        void SendMessage()
        {
            /*serialPort1.WriteLine(TextBoxEmission.Text);
            TextBoxEmission.Text = "";*/
            byte[] msgPayload = Encoding.ASCII.GetBytes(TextBoxEmission.Text);
            int msgFunction = 0x0080;
            int msgPayloadLength = msgPayload.Length;
            UartEncodeAndSendMessage(msgFunction, msgPayloadLength, msgPayload);
            TextBoxEmission.Text = "";
        }
        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)

        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextBoxReception.Text = "";
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (couleur_2 == 1)
            {
                switch (couleur)
                {
                    case 0:
                        TextBoxReception.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF0000");
                        couleur++;
                        break;
                    case 1:
                        TextBoxReception.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#00FF00");
                        couleur++;
                        break;
                    case 2:
                        TextBoxReception.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#0000FF");
                        couleur = 0;
                        break;

                }
            }

            //TextBoxReception.Background = Brushes.RoyalBlue;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (couleur_2 == 1)
            {
                TextBoxReception.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#150057");
            }
            else if (couleur_2 == 0)
            {
                TextBoxReception.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            byte[] msgPayload;
            int msgFunction = (int)FunctionId.state_robot;
            if (autoControlActivated)
            {
                msgPayload = Encoding.ASCII.GetBytes("0");
                BT3.Content = "Etat : Manuel";
            }
            else
            {
                msgPayload = Encoding.ASCII.GetBytes("1");
                BT3.Content = "Etat : Automatique";
            }
            autoControlActivated = !autoControlActivated;
            int msgPayloadLength = msgPayload.Length;
            UartEncodeAndSendMessage(msgFunction, msgPayloadLength, msgPayload);
        }
        public enum StateReception
        {
            Waiting,
            FunctionMSB,
            FunctionLSB,
            PayloadLengthMSB,
            PayloadLengthLSB,
            Payload,
            CheckSum
        }

        StateReception rcvState = StateReception.Waiting;
        int msgDecodedFunction = 0;
        int msgDecodedPayloadLength = 0;
        byte[] msgDecodedPayload;
        int msgDecodedPayloadIndex = 0;
        byte receivedChecksum = 0;
        byte calculatedChecksum = 0;
        int nb_snif = 0;
        private void DecodeMessage(byte c)
        {
            switch (rcvState)
            {
                case StateReception.Waiting:
                    if (c == 0xFE)
                    {
                        rcvState = StateReception.FunctionMSB;
                        msgDecodedPayloadLength = 0;
                        msgDecodedFunction = 0;
                        msgDecodedPayloadIndex = 0;
                    }
                    break;
                case StateReception.FunctionMSB:
                    msgDecodedFunction = (c << 8);
                    rcvState = StateReception.FunctionLSB;
                    break;
                case StateReception.FunctionLSB:
                    msgDecodedFunction += c;
                    rcvState = StateReception.PayloadLengthMSB;
                    break;
                case StateReception.PayloadLengthMSB:
                    msgDecodedPayloadLength = (c << 8);
                    rcvState = StateReception.PayloadLengthLSB;
                    break;
                case StateReception.PayloadLengthLSB:
                    msgDecodedPayloadLength += c;
                    if (msgDecodedPayloadLength > 1500)
                    {
                        rcvState = StateReception.Waiting;
                    }
                    rcvState = StateReception.Payload;
                    msgDecodedPayload = new byte[msgDecodedPayloadLength];
                    break;
                case StateReception.Payload:
                    msgDecodedPayload[msgDecodedPayloadIndex] = c;
                    msgDecodedPayloadIndex++;
                    if (msgDecodedPayloadIndex == msgDecodedPayloadLength)
                    {
                        rcvState = StateReception.CheckSum;
                    }
                    break;
                case StateReception.CheckSum:
                    receivedChecksum = c;
                    calculatedChecksum = CalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
                    if (calculatedChecksum == receivedChecksum)
                    {
                       // TextBoxReception.Text += "youpi\n";
                        ProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
                    }
                    else
                    {
                        TextBoxReception.Text += "snif\n";
                        nb_snif++;
                        TextTest.Text = nb_snif.ToString();
                        TextBoxReception.Text += msgDecodedFunction.ToString() + " " + msgDecodedPayloadLength.ToString() +" "+  msgDecodedPayload + "\n";
                    }
                    rcvState = StateReception.Waiting;
                    break;
                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }

        public enum FunctionId
        {
            text = 0x0080,
            led = 0x0020,
            telem = 0x0030,
            vitesse = 0x0040,
            etape = 0x0050,
            state_robot = 0x0051,
        }
        public enum StateRobot
        {
            STATE_ATTENTE = 0,
            STATE_ATTENTE_EN_COURS = 1,
            STATE_AVANCE = 2,
            STATE_AVANCE_EN_COURS = 3,
            STATE_TOURNE_GAUCHE = 4,
            STATE_TOURNE_GAUCHE_EN_COURS = 5,
            STATE_TOURNE_DROITE = 6,
            STATE_TOURNE_DROITE_EN_COURS = 7,
            STATE_TOURNE_SUR_PLACE_GAUCHE = 8,
            STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS = 9,
            STATE_TOURNE_SUR_PLACE_DROITE = 10,
            STATE_TOURNE_SUR_PLACE_DROITE_EN_COURS = 11,
            STATE_ARRET = 12,
            STATE_ARRET_EN_COURS = 13,
            STATE_RECULE = 14,
            STATE_RECULE_EN_COURS = 15,
            STATE_TOURNE_PETIT_GAUCHE = 16,
            STATE_TOURNE_PETIT_GAUCHE_EN_COURS = 17,
            STATE_TOURNE_PETIT_DROITE = 18,
            STATE_TOURNE_PETIT_DROITE_EN_COURS = 19,
            STATE_DEMI_TOUR_DROITE = 20,
            STATE_DEMI_TOUR_DROITE_EN_COURS = 21,
            STATE_DEMI_TOUR_GAUCHE_EN_COURS = 22,
            STATE_DEMI_TOUR_GAUCHE = 23
        }


        void ProcessDecodedMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            if (msgFunction == (int)FunctionId.text)
            {
                TextBoxReception.Text += "0x" + msgFunction.ToString("X4") + "\n";
                TextBoxReception.Text += msgPayloadLength + "\n";
                for (i = 0; i < msgPayloadLength; i++)
                {
                    TextBoxReception.Text += Convert.ToChar(msgPayload[i]);
                }
                TextBoxReception.Text += "\n";
            }
            if (msgFunction == (int)FunctionId.telem)
            {
                //TextBoxReception.Text += "0x" + msgFunction.ToString("X4") + "\n";
                //TextBoxReception.Text += msgPayloadLength + "\n";
                for (i = 0; i < msgPayloadLength; i++)
                {
                    switch (i)
                    {
                        case 0:
                            textBox1.Text ="";
                            textBox1.Text += msgPayload[0] + " cm";
                            break;
                        case 1:
                            textBox2.Text = "";
                            textBox2.Text += msgPayload[1] + " cm";
                            break;
                        case 2:
                            textBox3.Text = "";
                            textBox3.Text += msgPayload[2] + " cm";
                            break;
                    }
                }
                //TextBoxReception.Text += Convert.ToChar(msgPayload[0]);
                //TextBoxReception.Text += Convert.ToChar(msgPayload[1]);
                //TextBoxReception.Text += Convert.ToChar(msgPayload[2]);
                //TextBoxReception.Text += "\n";
            }
            if (msgFunction == (int)FunctionId.etape)
            {                
                int instant = (((int)msgPayload[1]) << 24) + (((int)msgPayload[2]) << 16) + (((int)msgPayload[3]) << 8) + ((int)msgPayload[4]);
                TextBoxetatrobot.Text = "Robot State: " + ((StateRobot)(msgPayload[0])).ToString() +" − " + instant.ToString() + " ms ";
                
            }
            if (msgFunction == (int)FunctionId.led)
            {
                //TextBoxReception.Text += Convert.ToChar(msgFunction) + " " + Convert.ToChar(msgPayloadLength) + " ";
                TextBoxReception.Text += "isoké ";
                for (i = 0; i < msgPayloadLength; i++)
                {
                    switch (i)
                    {
                        case 0:

                            TextBoxReception.Text += Convert.ToChar(msgPayload[0]);
                            break;
                        case 1:

                            TextBoxReception.Text += Convert.ToChar(msgPayload[1]);
                            break;
                        case 2:

                            TextBoxReception.Text += Convert.ToChar(msgPayload[2]);
                            break;                        
                    }
                    if (msgPayload[1] == 0x31)
                    {
                        checkBox2.IsChecked = true;
                    }
                    else
                    {
                        checkBox1.IsChecked = true;
                    }
                        
                }
                /*switch (msgPayload[0])
                {
                    case 1:
                        TextBoxReception.Text += Convert.ToChar(msgPayload[0]);
                        checkBox.IsChecked = !checkBox.IsChecked;
                        break;                    
                }*/
            }
        }

        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            byte[] msgPayload;
            if (checkBox.IsChecked == true)
            {
                //send on
                TextTest.Text += "I1";
                msgPayload = Encoding.ASCII.GetBytes("I1");
                
            }
            else
            {
                //send off
                TextTest.Text += "O1";
                msgPayload = Encoding.ASCII.GetBytes("O1");
                
            }
            int msgFunction = (int)FunctionId.led;
            int msgPayloadLength = msgPayload.Length;
            UartEncodeAndSendMessage(msgFunction, msgPayloadLength, msgPayload);

        }

        private void checkBox1_Click(object sender, RoutedEventArgs e)
        {
            byte[] msgPayload;
            if (checkBox1.IsChecked == true)
            {
                //send on
                TextTest.Text += "I3";
                msgPayload = Encoding.ASCII.GetBytes("I2");

            }
            else
            {
                //send off
                TextTest.Text += "O3";
                msgPayload = Encoding.ASCII.GetBytes("O2");

            }
            int msgFunction = (int)FunctionId.led;
            int msgPayloadLength = msgPayload.Length;
            UartEncodeAndSendMessage(msgFunction, msgPayloadLength, msgPayload);
        }

        private void checkBox2_Click(object sender, RoutedEventArgs e)
        {
            byte[] msgPayload;
            if (checkBox2.IsChecked == true)
            {
                //send on
                TextTest.Text += "I2";
                msgPayload = Encoding.ASCII.GetBytes("I3");

            }
            else
            {
                //send off
                TextTest.Text += "O2";
                msgPayload = Encoding.ASCII.GetBytes("O3");

            }
            int msgFunction = (int)FunctionId.led;
            int msgPayloadLength = msgPayload.Length;
            UartEncodeAndSendMessage(msgFunction, msgPayloadLength, msgPayload);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //textBox3.Text = "100";
            //checkBox.IsChecked = !checkBox.IsChecked;
        }

        private void TextTest_MouseEnter(object sender, MouseEventArgs e)
        {
            TextTest.Text = "";
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Switch_color();
        }
        private void Switch_color()
        {
            if (couleur_2 == 0)
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#4CFF37");

                GB1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FC37FF");
                GB1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#B200FF");
                TextBoxEmission.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#0B4DB2");

                GB2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#A20007");
                GB2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF00AE");
                TextBoxReception.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#150057");

                GB3.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#B0835D");
                GB3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#00E8FF");
                TextTest.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#0B2747");

                BT1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#9C88D9");
                BT2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#0D8A35");
                BT3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#EA6B7D");
                BT4.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FCB400");
                BT5.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#BFFC00");
                BT1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#B2A50B");
                BT2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#2807BD");
                BT3.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#079FBD");
                BT4.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000C3B");
                BT5.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#CE0D0D");

                checkBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#43A08B");
                checkBox1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFC322");
                checkBox2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#120754");
                checkBox.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#C914A3");
                checkBox1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#50CCF4");
                checkBox2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#344730");

                textBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#2F4285");
                textBox1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#901C02");
                textBox2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#2BA612");
                textBox3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#6A4D00");
                textBox4.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#6712A6");

                couleur_2 = 1;
            }
            else if (couleur_2 == 1)
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#3E3E42");

                GB1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                GB1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#75757E");
                TextBoxEmission.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#75757E");
                TextBoxEmission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F0F0F0");

                GB2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                GB2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#75757E");
                TextBoxReception.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#75757E");
                TextBoxReception.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F0F0F0");

                GB4.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                GB4.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#75757E");
                TextBoxetatrobot.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#75757E");
                TextBoxetatrobot.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F0F0F0");

                GB3.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                GB3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#75757E");
                TextTest.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#75757E");

                BT1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#858585");
                BT2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#858585");
                BT3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#858585");
                BT4.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#858585");
                BT5.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#858585");
                BT1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                BT2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                BT3.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                BT4.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                BT5.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");

                groupBox.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                groupBox1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                groupBox2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");

                checkBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                checkBox1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                checkBox2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                checkBox.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF9B00");
                checkBox1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                checkBox2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#476AD1");

                textBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#C8C8C8");
                textBox1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#C8C8C8");
                textBox2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#C8C8C8");
                textBox3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#C8C8C8");
                textBox4.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#C8C8C8");

                couleur_2 = 2;
            }
            else if (couleur_2 == 2)
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");

                GB1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                GB1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");
                TextBoxEmission.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");
                TextBoxEmission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");

                GB2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                GB2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");
                TextBoxReception.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");
                TextBoxReception.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");

                GB4.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                GB4.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");
                TextBoxetatrobot.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");
                TextBoxetatrobot.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");

                GB3.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                GB3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");
                TextTest.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");

                BT1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDDDDD");
                BT2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDDDDD");
                BT3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDDDDD");
                BT4.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDDDDD");
                BT5.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDDDDD");
                BT1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                BT2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                BT3.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                BT4.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                BT5.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");

                groupBox.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                groupBox1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                groupBox2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");

                checkBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                checkBox1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                checkBox2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                checkBox.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                checkBox1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                checkBox2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");

                textBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                textBox1.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                textBox2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                textBox3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                textBox4.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");



                couleur_2 = 0;
            }
        }

    }
}
