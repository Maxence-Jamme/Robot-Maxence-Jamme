#include <xc.h>
#include <stdio.h>
#include <stdlib.h>
#include "main.h"
#include "asservissement.h"
#include "UART_Protocol.h"
#include "Toolbox.h"
#include "Robot.h"

void SetupPidAsservissement (volatile PidCorrector* PidCorr, double Kp, double Ki, double Kd, double proportionelleMax, double integralMax, double deriveeMax)
{   
    PidCorr->Kp = Kp;
    PidCorr->erreurProportionelleMax = proportionelleMax; //On limite la correction due au Kp
    PidCorr->Ki = Ki;
    PidCorr->erreurIntegraleMax = integralMax; //On limite la correction due au Ki
    PidCorr->Kd = Kd;
    PidCorr->erreurDeriveeMax = deriveeMax;
}

unsigned char asservissementPayload [104];
    /*double consigneX = 0.01;
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
    double corrLimitDTheta = 0.26;*/
    
void AsservissementValeur(){
    //-------------------
    int nb_octet = 0;
    getBytesFromFloat(asservissementPayload, nb_octet, (float)(robotState.PidX.consigne)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.consigne));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.value)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.value)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.error));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.error)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.command));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.command)); 
    //-------------------   
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.corrP));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.corrP));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.corrI));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.corrP));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.corrD));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.corrD));
    //-------------------
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.Kp)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.Kp));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.Ki));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.Ki));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.Kd)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.Kd));
    //-------------------
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.erreurProportionelleMax)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.erreurProportionelleMax)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.erreurIntegraleMax)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.erreurIntegraleMax));
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidX.erreurDeriveeMax)); 
    getBytesFromFloat(asservissementPayload, nb_octet += 4, (float)(robotState.PidTheta.erreurDeriveeMax));

    
    UartEncodeAndSendMessage(0x0070, nb_octet +=4, asservissementPayload);
}