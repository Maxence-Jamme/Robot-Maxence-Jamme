﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot_Interface_JAMME_JUILLE
{
    public class Robot
    {
        public Queue<byte> byteListReceived = new Queue<byte>();
        public string receivedText = "" ;
        public float distanceTelemetreDroit;
        public float distanceTelemetreCentre; 
        public float distanceTelemetreGauche;
        public float positionXOdo;
        public float positionYOdo;
        public float AngleRadOdo;
        public float vLinéaireOdo;
        public float vAngulaireOdo;
        public Robot ( )
        {
            
        }
    }

}
