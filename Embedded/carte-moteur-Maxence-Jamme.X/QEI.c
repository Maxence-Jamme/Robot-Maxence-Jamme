#include "main.h"
#define DISTROUES 281.2

void InitQEI1 (){
QEI1IOCbits .SWPAB = 1 ; //QEAx and QEBx a r e swapped
QEI1GECL = 0xFFFF;
QEI1GECH = 0xFFFF;
QEI1CONbits .QEIEN = 1 ; // Enable QEI Module
}

void InitQEI2 (){
QEI2IOCbits .SWPAB = 1 ; //QEAx and QEBx a r e not swapped
QEI2GECL = 0xFFFF;
QEI2GECH = 0xFFFF;
QEI2CONbits .QEIEN = 1 ; // Enable QEI Module
}

void QEIUpdateData (){
    //On sauvegarde les anciennes valeurs
    QeiDroitPosition_T_1 = QeiDroitPosition ;
    QeiGauchePosition_T_1 = QeiGauchePosition ;
    //On réactualise les valeurs des positions
    long QEI1RawValue = POS1CNTL;
    QEI1RawValue += ((long)POS1HLD<<16);
    long QEI2RawValue = POS2CNTL;
    QEI2RawValue += ((long)POS2HLD<<16);
    // Conversion en mm (r\?egl\? e pour la taille des roues codeuses)
    QeiDroitPosition = 0.01620 ? QEI1RawValue;
    QeiGauchePosition = ?0.01620 ? QEI2RawValue;
    // Calcul des deltas de position
    delta_d = QeiDroitPosition ? QeiDroitPosition_T_1 ;
    delta_g = QeiGauchePosition ? QeiGauchePosition_T_1 ;
    // delta_theta = atan((delta_d ? delta_g) / DISTROUES) ;
    delta_theta = (delta_d ? delta_g ) / DISTROUES;
    dx = (delta_d + delta_g ) / 2 ;
    // Calcul des vitesses
    // attention à remultiplier par la fréquence d'échantillonnage
    robotState.vitesseDroitFromOdometry = delta_d?FREQ_ECH_QEI;
    robotState.vitesseGaucheFromOdometry = delta_g?FREQ_ECH_QEI;
    robotState.vitesseLineaireFromOdometry = ( robotState.vitesseDroitFromOdometry + robotState.vitesseGaucheFromOdometry ) / 2 ;
    robotState.vitesseAngulaireFromOdometry = delta_theta?FREQ_ECH_QEI;
    //Mise à jour du positionnement terrain à t?1
    robotState.xPosFromOdometry_1 = robotState.xPosFromOdometry ;
    robotState.yPosFromOdometry_1 = robotState.yPosFromOdometry ;
    robotState.angleRadianFromOdometry_1 = robotState.angleRadianFromOdometry ;
    // C al c ul de s p o s i t i o n s dans l e r e f e r e n t i e l du t e r r a i n
    robotState.xPosFromOdometry = . . .
    robotState.yPosFromOdometry = . . .
    robotState.angleRadianFromOdometry = . . .
    if (robotState.angleRadianFromOdometry > PI )
        robotState.angleRadianFromOdometry ?= 2?PI ;
    if (robotState.angleRadianFromOdometry < ?PI )
        robotState.angleRadianFromOdometry += 2?PI ;
}