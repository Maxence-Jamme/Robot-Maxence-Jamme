#ifndef ASSERVISSEMENT_H
#define	ASSERVISSEMENT_H

typedef struct _PidCorrector
{
    double Kp;
    double Ki;
    double Kd;
    double erreurProportionelleMax;
    double erreurIntegraleMax;
    double erreurDeriveeMax;
    double erreurIntegrale;
    double epsilon_1;
    double erreur;
    
    //For Debug onl y
    double corrP;
    double corrI;
    double corrD ;
} PidCorrector;

void SetupPidAsservissement(volatile PidCorrector* PidCorr, double Kp, double Ki , double Kd, double proportionelleMax, double integralMax, double deriveeMax);

#endif

