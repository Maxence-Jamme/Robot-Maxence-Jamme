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
using System.Threading;
using MouseKeyboardActivityMonitor.WinApi;
using MouseKeyboardActivityMonitor;
using System.Windows.Forms;
using Utilities;
using WpfAsservissementDisplay;

namespace Robot_Interface_JAMME_JUILLE
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        ReliableSerialPort serialPort1;
        //AsyncCallback SerialPort1_DataRecived;
        DispatcherTimer timerAffichage;
        // /!\ Ne pas le déclarer, déjà fait dans le xaml
       // AsservissementRobot2RouesDisplayControl Asserv = new AsservissementRobot2RouesDisplayControl();

        private readonly KeyboardHookListener m_KeyboardHookManager;

        int i;
        int couleur_2 = 0;
        bool autoControlActivated = true;
        Robot robot = new Robot();
        bool keyloger = false;
        bool LinAng = true;

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

        public enum FunctionId
        {
            led = 0x0020,
            telem = 0x0030,
            vitesse = 0x0040,
            etape = 0x0050,
            state_robot = 0x0051,
            position_data = 0x0061,
            asservissement = 0x0070,
            text = 0x0080
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

            m_KeyboardHookManager = new KeyboardHookListener( new GlobalHooker() ) ;
            m_KeyboardHookManager.Enabled = true;
            m_KeyboardHookManager.KeyDown += M_KeyboardHookManager_KeyDown;// += HookManager_KeyDown;



           
        }

        private void M_KeyboardHookManager_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {     
            if(autoControlActivated == false && keyloger)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        UartEncodeAndSendMessage(0x0052, 1, new byte[] {( byte ) StateRobot.STATE_TOURNE_SUR_PLACE_GAUCHE});
                        TextBoxetatrobot.Text = "Robot State : Tourne à gauche";
                        break;

                    case Keys.Right:
                        UartEncodeAndSendMessage(0x0052, 1, new byte[] {( byte ) StateRobot.STATE_TOURNE_SUR_PLACE_DROITE });
                        TextBoxetatrobot.Text = "Robot State : Tourne à droite";
                        break;

                    case Keys.Up:
                        UartEncodeAndSendMessage(0x0052, 1, new byte[]{ ( byte ) StateRobot.STATE_AVANCE });
                        TextBoxetatrobot.Text = "Robot State : Avance";
                        break;

                    case Keys.PageDown:
                        UartEncodeAndSendMessage(0x0052, 1, new byte[]{ ( byte ) StateRobot.STATE_ARRET });
                        TextBoxetatrobot.Text = "Robot State : Arret";
                        break;

                    case Keys.Down:
                        UartEncodeAndSendMessage(0x0052, 1, new byte[]{ ( byte ) StateRobot.STATE_RECULE });
                        TextBoxetatrobot.Text = "Robot State : Recule";
                        break;
                }
            }
        }


        private void TimerAffichage_Tick(object sender, EventArgs e)        // peut etre faut à voir
        {
            //while (robot.byteListReceived.Count != 0)
            //{
            //    byte byteReceived = robot.byteListReceived.Dequeue();
            //    DecodeMessage(byteReceived);
            //    //string blabla;
            //    //blabla = byteReceived.ToString("X");
            //    //blabla += ;
            //    //TextTest.Text += byteReceived.ToString("X");
            //    //TextTest.Text += Convert.ToChar(byteReceived) + " "; // "0x"+blabla+"";
            //    //TextBoxReception.Text += byteReceived+"\n";
            //    //TextBoxReception.Text += Convert.ToChar(byteReceived);
            //}

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
                DecodeMessage(e.Data[i]);
                //robot.byteListReceived.Enqueue(e.Data[i]);
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

        private void TextBoxEmission_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)

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
            if (autoControlActivated == false)
            {
                TextBoxetatrobot.Text = "Robot State : Manuel";
            }
            else
            {
                TextBoxetatrobot.Text = "Robot State : Automatique";
            }
                
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
                        Dispatcher.Invoke(delegate { ProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload); });                        
                    }
                    else
                    {                        
                        nb_snif++;
                        Dispatcher.Invoke(delegate { TextTest.Text = nb_snif.ToString(); });
                        //TextBoxReception.Text += msgDecodedFunction.ToString() + " " + msgDecodedPayloadLength.ToString() +" "+  msgDecodedPayload + "\n";
                    }
                    rcvState = StateReception.Waiting;
                    break;
                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }

        void ProcessDecodedMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {  
            switch (msgFunction)
            {
                case ((int)FunctionId.text):
                    for (i = 0; i < msgPayloadLength; i++)
                    {
                        TextBoxReception.Text += Convert.ToChar(msgPayload[i]);
                    }
                    //TextBoxReception.Text += "test\n";
                    break;
                case ((int)FunctionId.telem):
                    TBoxD.Text = msgPayload[0] + " cm";
                    TBoxC.Text = msgPayload[1] + " cm";
                    TBoxG.Text = msgPayload[2] + " cm";
                    TBoxExG.Text = msgPayload[3] + " cm";
                    TBoxExD.Text = msgPayload[4] + " cm";
                    break;
                case ((int)FunctionId.vitesse):
                    TBoxMotG.Text = msgPayload[0] + " %";
                    TBoxMotD.Text = msgPayload[1] + " %";
                    break;
                case ((int)FunctionId.etape):
                    int instant = (((int)msgPayload[1]) << 24) + (((int)msgPayload[2]) << 16) + (((int)msgPayload[3]) << 8) + ((int)msgPayload[4]);
                    TextBoxetatrobot.Text = "Robot State : " + ((StateRobot)(msgPayload[0])).ToString() + " − " + instant.ToString() + " ms";
                    break;
                case ((int)FunctionId.led):
                    if (msgPayload[0] == 0x49 && msgPayload[1] == 0x31)
                    {
                        checkBoxLED1.IsChecked = true;
                    }
                    if (msgPayload[0] == 0x4F && msgPayload[1] == 0x31)
                    {
                        checkBoxLED1.IsChecked = false;
                    }
                    if (msgPayload[0] == 0x49 && msgPayload[1] == 0x32)
                    {
                        checkBoxLED2.IsChecked = true;
                    }
                    if (msgPayload[0] == 0x4F && msgPayload[1] == 0x32)
                    {
                        checkBoxLED2.IsChecked = false;
                    }
                    if (msgPayload[0] == 0x49 && msgPayload[1] == 0x33)
                    {
                        checkBoxLED3.IsChecked = true;
                    }
                    if (msgPayload[0] == 0x4F && msgPayload[1] == 0x33)
                    {
                        checkBoxLED3.IsChecked = false;
                    }
                    break;
                case ((int)FunctionId.position_data):
                    byte[] tab = msgPayload.GetRange(4, 4);
                    robot.positionXOdo = tab.GetFloat();
                    tab = msgPayload.GetRange(8, 4);
                    robot.positionYOdo = tab.GetFloat();
                    tab = msgPayload.GetRange(12, 4);
                    robot.AngleRadOdo = tab.GetFloat();
                    tab = msgPayload.GetRange(16, 4);
                    robot.vLinéaireOdo = tab.GetFloat();
                    tab = msgPayload.GetRange(20, 4);
                    robot.vAngulaireOdo = tab.GetFloat();
                    TBoxPosX.Text = (robot.positionXOdo).ToString("0.00") + " m";
                    TBoxPosY.Text = (robot.positionYOdo).ToString("0.00") + " m";
                    TBoxAngle.Text = (robot.AngleRadOdo * (180 / Math.PI)).ToString("0.00") + "°";
                    TBoxVitLin.Text = robot.vLinéaireOdo.ToString("0.00") + " m/s";
                    TBoxVitAng.Text = robot.vAngulaireOdo.ToString("0.00") + " m/s";
                    break;

                case ((int)FunctionId.asservissement):
                    //asservSpeedDisplay.SetTitle("TEST");
                    int nb_octet = 0;
                    //-------------------
                    double consigneX;
                    double consigneTheta;                    
                    double valueX;
                    double valueTheta;
                    double errorX;
                    double errorTheta;
                    double commandX;
                    double commandTheta;
                    //-------------------
                    double corrPX;
                    double corrPTheta;
                    double corrIX;
                    double corrITheta;
                    double corrDX;
                    double corrDTheta;
                    //-------------------
                    double KpX;
                    double KpTheta; 
                    double KiX; 
                    double KiTheta; 
                    double KdX; 
                    double KdTheta;
                    //-------------------
                    double corrLimitPX;
                    double corrLimitPTheta;
                    double corrLimitIX;
                    double corrLimitITheta;
                    double corrLimitDX;
                    double corrLimitDTheta;                    
                    //-------------------
                    byte[] tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    consigneX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    consigneTheta = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;                    
                    valueX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    valueTheta = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    errorX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    errorTheta = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    commandX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    commandTheta = tabl.GetFloat();
                    //------------------- corrPX, corrPTheta, corrIX, corrITheta, corrDX, corrDTheta
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrPX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrPTheta = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrIX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrITheta = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrDX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrDTheta = tabl.GetFloat();
                    //------------------- KpX, KpTheta, KiX, KiTheta, KdX, KdTheta
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    KpX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    KpTheta = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    KiX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    KiTheta = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    KdX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    KdTheta = tabl.GetFloat();
                    //------------------- corrLimitPX, corrLimitPTheta, corrLimitIX, corrLimitITheta, corrLimitDX, corrLimitDTheta
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrLimitPX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrLimitPTheta = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrLimitIX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrLimitITheta = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrLimitDX = tabl.GetFloat();
                    tabl = msgPayload.GetRange(nb_octet, 4); nb_octet = nb_octet + 4;
                    corrLimitDTheta = tabl.GetFloat();
                    //-------------------
                    asservSpeedDisplay.UpdatePolarSpeedConsigneValues(consigneX, consigneTheta);
                    asservSpeedDisplay.UpdatePolarSpeedCommandValues(commandX, commandTheta);
                    asservSpeedDisplay.UpdatePolarOdometrySpeed(valueX, valueTheta);
                    asservSpeedDisplay.UpdatePolarSpeedErrorValues(errorX, errorTheta);
                    //-------------------
                    asservSpeedDisplay.UpdatePolarSpeedCorrectionValues(corrPX, corrPTheta, corrIX, corrITheta, corrDX, corrDTheta);
                    asservSpeedDisplay.UpdatePolarSpeedCorrectionGains(KpX, KpTheta, KiX, KiTheta, KdX, KdTheta);
                    asservSpeedDisplay.UpdatePolarSpeedCorrectionLimits(corrLimitPX, corrLimitPTheta, corrLimitIX, corrLimitITheta, corrLimitDX, corrLimitDTheta);

                    //asservSpeedDisplay.UpdatePolarSpeedConsigneValues(consigneX, consigneTheta);
                    break;
            } 
        }

        private void checkBox1_Click(object sender, RoutedEventArgs e)      // ORANGE
        {
            byte[] msgPayload;
            if (checkBoxLED1.IsChecked == true){
                TextTest.Text += "I1";
                msgPayload = Encoding.ASCII.GetBytes("I1");
            }else{
                TextTest.Text += "O1";
                msgPayload = Encoding.ASCII.GetBytes("O1");
            }
            UartEncodeAndSendMessage((int)FunctionId.led, msgPayload.Length, msgPayload);
        }

        private void checkBox2_Click(object sender, RoutedEventArgs e)      // BLEU
        {
            byte[] msgPayload;
            if (checkBoxLED2.IsChecked == true){
                TextTest.Text += "I2";
                msgPayload = Encoding.ASCII.GetBytes("I2");
            }else{
                TextTest.Text += "O2";
                msgPayload = Encoding.ASCII.GetBytes("O2");
            }
            UartEncodeAndSendMessage((int)FunctionId.led, msgPayload.Length, msgPayload);
        }

        private void checkBox3_Click(object sender, RoutedEventArgs e)      // BLANC
        {
            byte[] msgPayload;
            if (checkBoxLED3.IsChecked == true){
                TextTest.Text += "I3";
                msgPayload = Encoding.ASCII.GetBytes("I3");
            }else{
                TextTest.Text += "O3";
                msgPayload = Encoding.ASCII.GetBytes("O3");
            }
            UartEncodeAndSendMessage((int)FunctionId.led, msgPayload.Length, msgPayload);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (keyloger)
            {
                BT4.Content = "Etat keyloger : OFF";
                keyloger = !keyloger;
            }else
            {
                BT4.Content = "Etat keyloger : ON";
                keyloger = !keyloger;
            }

        }

        double consigneX = 152;
        double consigneTheta = 599.2;

        private void TextTest_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextTest.Text = "";            
        }
        
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Switch_color();
        }

        private void CH_Click(object sender, RoutedEventArgs e)
        {
            if (autoControlActivated == false)
            {
                UartEncodeAndSendMessage(0x0052, 1, new byte[] { (byte)StateRobot.STATE_AVANCE });
                TextBoxetatrobot.Text = "Robot State : Avance";
            }
        }

        private void CG_Click(object sender, RoutedEventArgs e)
        {
            if (autoControlActivated == false)
            {
                UartEncodeAndSendMessage(0x0052, 1, new byte[] { (byte)StateRobot.STATE_TOURNE_SUR_PLACE_GAUCHE });
                TextBoxetatrobot.Text = "Robot State : Tourne à gauche";
            }
        }

        private void CB_Click(object sender, RoutedEventArgs e)
        {
            if (autoControlActivated == false)
            {
                UartEncodeAndSendMessage(0x0052, 1, new byte[] { (byte)StateRobot.STATE_RECULE });
                TextBoxetatrobot.Text = "Robot State : Recule";
            }
        }

        private void CD_Click(object sender, RoutedEventArgs e)
        {
            if (autoControlActivated == false)
            {
                UartEncodeAndSendMessage(0x0052, 1, new byte[] { (byte)StateRobot.STATE_TOURNE_SUR_PLACE_DROITE });
                TextBoxetatrobot.Text = "Robot State : Tourne à droite";
            }
        }

        private void CS_Click(object sender, RoutedEventArgs e)
        {
            if (autoControlActivated == false)
            {
                UartEncodeAndSendMessage(0x0052, 1, new byte[] { (byte)StateRobot.STATE_ARRET });
                TextBoxetatrobot.Text = "Robot State : Arret";
            }
        }

        private void ButAsservEnv_Click(object sender, RoutedEventArgs e)
        {
            int LinAng_valeur = 0;
            if (LinAng)
            {
                LinAng_valeur = 1;
            }
            byte[] msgPayload = new byte[28];
            //byte[] msgPayload = Encoding.ASCII.GetBytes(LinAng_valeur + TBoxKp.Text + TBoxKi.Text + TBoxKd.Text + TBoxKpmax.Text + TBoxKimax.Text + TBoxKdmax.Text);
            //byte[] msgPayload = Encoding.ASCII.GetBytes(TBoxKp.Text);
            msgPayload.SetValueRange(((float)(LinAng_valeur)).GetBytes(), 0);
            msgPayload.SetValueRange(((float)(Convert.ToByte(TBoxKp.Text))).GetBytes(), 4);
            msgPayload.SetValueRange(((float)(Convert.ToByte(TBoxKi.Text))).GetBytes(), 8);
            msgPayload.SetValueRange(((float)(Convert.ToByte(TBoxKd.Text))).GetBytes(), 12);
            msgPayload.SetValueRange(((float)(Convert.ToByte(TBoxKpmax.Text))).GetBytes(), 16);
            msgPayload.SetValueRange(((float)(Convert.ToByte(TBoxKimax.Text))).GetBytes(), 20);
            msgPayload.SetValueRange(((float)(Convert.ToByte(TBoxKdmax.Text))).GetBytes(), 24);            
            int msgFunction = (int)FunctionId.asservissement;
            int msgPayloadLength = msgPayload.Length;
            UartEncodeAndSendMessage(msgFunction, msgPayloadLength, msgPayload);
            TBoxKp.Text = "0";
            TBoxKi.Text = "0";
            TBoxKd.Text = "0";
            TBoxKpmax.Text = "0";
            TBoxKimax.Text = "0";
            TBoxKdmax.Text = "0";
        }

        private void ButAngLin_Click(object sender, RoutedEventArgs e)
        {
            if (LinAng)
            {
                ButAngLin.Content = "Angulaire";
                TBoxKp.Text = "0";
                TBoxKi.Text = "0";
                TBoxKd.Text = "0";
                TBoxKpmax.Text = "0";
                TBoxKimax.Text = "0";
                TBoxKdmax.Text = "0";
            }
            else
            {
                ButAngLin.Content = "Linéaire";
                TBoxKp.Text = "0";
                TBoxKi.Text = "0";
                TBoxKd.Text = "0";
                TBoxKpmax.Text = "0";
                TBoxKimax.Text = "0";
                TBoxKdmax.Text = "0";
            }
            LinAng = !LinAng;
        } 

        private void Switch_color()
        {
            if (couleur_2 == 0)
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#3E3E42");
                //#F0F0F0
                TBckKp.Foreground = TBckKpmax.Foreground = TBckKi.Foreground = TBckKimax.Foreground = TBckKd.Foreground = TBckKdmax.Foreground = ParaAsser.Foreground = TextBoxetatrobot.Foreground = TextBoxEmission.Foreground = TextBoxReception.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F0F0F0");
                //#75757E
                GB1.Background = TextBoxEmission.Background = GB2.Background = TextBoxReception.Background = TextBoxetatrobot.Background = GB4.Background = TextTest.Background = GB3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#75757E");
                //#F1F1F1
                ButAsservEnv.Foreground = ButAngLin.Foreground = BT5.Foreground = BT4.Foreground = BT3.Foreground = BT2.Foreground = BT1.Foreground = groupBox2.Foreground = groupBox1.Foreground = groupBox.Foreground = GB4.Foreground = GB3.Foreground = GBC.Foreground = GB1.Foreground = GB2.Foreground = Odometrie.Foreground = TBckVitLin.Foreground = TBckVitAng.Foreground = TBckPosX.Foreground = TBckPosY.Foreground = TBckAngle.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F1F1F1");
                //#C8C8C8
                TBoxKp.Background = TBoxKi.Background = TBoxKd.Background = TBoxKpmax.Background = TBoxKimax.Background = TBoxKdmax.Background = TBoxMotD.Background = TBoxD.Background = TBoxC.Background = TBoxG.Background = TBoxMotG.Background = TBoxExD.Background = TBoxExG.Background = TBoxVitLin.Background = TBoxVitAng.Background = TBoxPosX.Background = TBoxPosY.Background = TBoxAngle.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#C8C8C8");
                //#858585
                ButAsservEnv.Background = ButAngLin.Background = BT1.Background = BT2.Background = BT3.Background = BT4.Background = BT5.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#858585");
                //#FFFFFF
                checkBoxLED3.Background = checkBoxLED1.Background = checkBoxLED2.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                //LED
                checkBoxLED3.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");//BLANC
                checkBoxLED1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF9B00");//ORANGE
                checkBoxLED2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#367ff5");//BLEU
                couleur_2 = 1;
            }
            else if (couleur_2 == 1)
            {
                //#000000
                ButAsservEnv.Foreground = ButAngLin.Foreground = TBckKp.Foreground = TBckKpmax.Foreground = TBckKi.Foreground = TBckKimax.Foreground = TBckKd.Foreground = TBckKdmax.Foreground = ParaAsser.Foreground = checkBoxLED2.Foreground = checkBoxLED1.Foreground = checkBoxLED3.Foreground = TextBoxetatrobot.Foreground = TextBoxEmission.Foreground = TextBoxReception.Foreground = BT5.Foreground = BT4.Foreground = BT3.Foreground = BT2.Foreground = BT1.Foreground = groupBox2.Foreground = groupBox1.Foreground = groupBox.Foreground = GB4.Foreground = GB3.Foreground = GBC.Foreground = GB1.Foreground = GB2.Foreground = Odometrie.Foreground = TBckVitLin.Foreground = TBckVitAng.Foreground = TBckPosX.Foreground = TBckPosY.Foreground = TBckAngle.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                //#FFFFFF
                TBoxKp.Background = TBoxKi.Background = TBoxKd.Background = TBoxKpmax.Background = TBoxKimax.Background = TBoxKdmax.Background = Background = checkBoxLED3.Background = checkBoxLED1.Background = checkBoxLED2.Background = TBoxMotD.Background = TBoxD.Background = TBoxC.Background = TBoxG.Background = TBoxMotG.Background = TBoxExD.Background = TBoxExG.Background = TBoxVitLin.Background = TBoxVitAng.Background = TBoxPosX.Background = TBoxPosY.Background = TBoxAngle.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                //#c9c9c9
                GB1.Background = TextBoxEmission.Background = GB2.Background = TextBoxReception.Background = TextBoxetatrobot.Background = GB4.Background = TextTest.Background = GB3.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#c9c9c9");
                //#DDDDDD
                ButAsservEnv.Background = ButAngLin.Background = BT1.Background = BT2.Background = BT3.Background = BT4.Background = BT5.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDDDDD");                
                couleur_2 = 0;
            }
        }
    }
}