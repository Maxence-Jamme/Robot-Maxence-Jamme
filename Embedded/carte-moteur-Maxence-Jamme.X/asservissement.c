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
    int nb_octect = 0;
    unsigned char asservissementPayload [104] ;
    getBytesFromInt32(asservissementPayload, 0, (float)(consigneX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, 4, (float)(consigneTheta)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, 8, (float)(valueX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, 12, (float)(valueTheta)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, 16, (float)(errorX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, 20, (float)(errorTheta)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, 24, (float)(commandX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, 28, (float)(commandTheta)); nb_octect = nb_octect + 4;
    //-------------------
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrPX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrPTheta)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrIX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrITheta)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrDX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrDTheta)); nb_octect = nb_octect + 4;
    //-------------------
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(KpX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(KpTheta)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(KiX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(KiTheta)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(KdX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(KdTheta)); nb_octect = nb_octect + 4;
    //-------------------
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrLimitPX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrLimitPTheta)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrLimitIX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrLimitITheta)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrLimitDX)); nb_octect = nb_octect + 4;
    getBytesFromInt32(asservissementPayload, nb_octect, (float)(corrLimitDTheta)); nb_octect = nb_octect + 4;
    
    UartEncodeAndSendMessage(0x0070, 104, asservissementPayload);
}