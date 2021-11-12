#include <xc.h>
#include <stdio.h>
#include <stdlib.h>
#include "main.h"
#include "asservissement.h"
#include "UART_Protocol.h"
#include "Toolbox.h"

void SetupPidAsservissement (volatile PidCorrector* PidCorr, double Kp, double Ki, double Kd, double proportionelleMax, double integralMax, double deriveeMax)
{   
    PidCorr->Kp = Kp;
    PidCorr->erreurProportionelleMax = proportionelleMax; //On limite la correction due au Kp
    PidCorr->Ki = Ki;
    PidCorr->erreurIntegraleMax = integralMax; //On limite la correction due au Ki
    PidCorr->Kd = Kd;
    PidCorr->erreurDeriveeMax = deriveeMax;
}

unsigned char asservissementPayload [104] ;
void AsservissementValeur(){
    double consigneX = 0.01;
    double consigneTheta = 0.02;                    
    double valueX = 0.03;
    double valueTheta = 0.04;
    double errorX = 0.05;
    double errorTheta = 0.06;
    double commandX = 0.07;
    double commandTheta = 0.08;
    //-------------------
    double corrPX = 0.09;
    double corrPTheta = 0.10;
    double corrIX = 0.11;
    double corrITheta = 0.12;
    double corrDX = 0.13;
    double corrDTheta = 0.14;
    //-------------------
    double KpX = 0.15;
    double KpTheta = 0.16; 
    double KiX = 0.17; 
    double KiTheta = 0.18; 
    double KdX = 0.19; 
    double KdTheta = 0.20;
    //-------------------
    double corrLimitPX = 0.21;
    double corrLimitPTheta = 0.22;
    double corrLimitIX = 0.23;
    double corrLimitITheta = 0.24;
    double corrLimitDX = 0.25;
    double corrLimitDTheta = 0.26;
    //-------------------
    int nb_octet = 0;
    getBytesFromFloat(asservissementPayload, nb_octet, (float)(consigneX)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(consigneTheta));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(valueX)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(valueTheta)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(errorX));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(errorTheta)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(commandX));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(commandTheta)); 
    //-------------------
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrPX)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrPTheta));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrIX));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrITheta)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrDX)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrDTheta)); 
    //-------------------
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(KpX));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(KpTheta));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(KiX));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(KiTheta));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(KdX)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(KdTheta));
    //-------------------
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrLimitPX)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrLimitPTheta)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrLimitIX)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrLimitITheta));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrLimitDX)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(corrLimitDTheta));
    
    UartEncodeAndSendMessage(0x0070, nb_octet +=4, asservissementPayload);
}