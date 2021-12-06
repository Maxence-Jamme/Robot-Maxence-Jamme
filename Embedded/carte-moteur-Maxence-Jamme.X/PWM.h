#ifndef PWM_H
#define	PWM_H

#define MOTEUR_DROIT 0
#define MOTEUR_GAUCHE 1
#define COEFF_VITESSE_LINEAIRE_PERCENT 90
#define COEFF_VITESSE_ANGULAIRE_PERCENT 77

//void PWMSetSpeed(float vitesseEnPourcents, float moteur);
void InitPWM(void);
void PWMUpdateSpeed(void);
void PWMSetSpeedConsigne(float vitesseEnPourcents, char moteur);
void PWMSetSpeedConsignePolaire();
#endif	/* PWM_H */

